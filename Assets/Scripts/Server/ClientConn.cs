using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Kernys.Bson;
using Unity.Collections.LowLevel.Unsafe;

public class ClientConn : MonoBehaviour
{
    public static ClientConn conn;
    private Host _client;
    private Peer _peer;

    private bool _isConnected;
    public bool IsConnected() => _isConnected;

    private const ushort Port = 6005;
    private const string ServerIP = "127.0.0.1";
    private const int MaxChannels = 1;

    string machID;

    public int isGuest = 0;
    public string myGuestName;

    public BSONObject worldsData = new BSONObject();
    void Start()
    {
        Application.runInBackground = true;
        Library.Initialize();
        ConnectToServer();
    }

    private void Awake()
    {
        machID = SystemInfo.deviceUniqueIdentifier;
        if (conn != null && conn != this)
        {
            Destroy(gameObject);
            return;
        }

        conn = this;
        DontDestroyOnLoad(gameObject);
    }


    private void OnApplicationQuit()
    {
        DisconnectFromServer();
        Library.Deinitialize();
    }

    private void ConnectToServer()
    {
        _client = new Host();
        Address address = new Address
        {
            Port = Port
        };
        address.SetHost(ServerIP);

        _client.Create();

        _peer = _client.Connect(address, MaxChannels);

        Debug.Log("Connecting to server...");
    }
    private void DisconnectFromServer()
    {
        if (_isConnected)
        {
            _peer.Disconnect(0);
            _client.Flush();
            Debug.Log("Disconnected from server.");
        }

        _client.Dispose();
    }
    private void Update()
    {
        if (_client == null) return;

        ENet.Event netEvent;
        while (_client.Service(0, out netEvent) > 0)
        {
            switch (netEvent.Type)
            {
                case ENet.EventType.Connect:
                    _isConnected = true;
                    Debug.Log("Connected to server.");
                    GetGuestAccount(machID);
                    StartCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Disconnect:
                    _isConnected = false;
                    Debug.Log("Disconnected from server.");
                    StopCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Timeout:
                    _isConnected = false;
                    Debug.Log("Connection timeout.");
                    StopCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Receive:
                    HandlePacket(netEvent);
                    netEvent.Packet.Dispose();
                    break;
            }
        }
    }
    private IEnumerator SendHeartbeat()
    {
        while (_isConnected)
        {
            yield return new WaitForSeconds(5);
            byte[] heartbeatPacket = new byte[1];
            heartbeatPacket[0] = (byte)PacketType.Heartbeat;
            SendPacket(heartbeatPacket);
        }
    }
    public void SendPacket(byte[] data)
    {
        if (_isConnected)
        {
            Packet packet = default;
            packet.Create(data);
            _peer.Send(0, ref packet);
        }
    }
    public void GetGuestAccount(string machID)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketType.OnConnected);
            writer.Write(machID);

            SendPacket(stream.ToArray());
        }
    }
    public void SendEnterWorld(string worldName)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketType.EnterWorld);
            writer.Write(worldName);

            SendPacket(stream.ToArray());
        }
    }
    private void HandlePacket(ENet.Event netEvent)
    {
        byte[] data = new byte[netEvent.Packet.Length];
        netEvent.Packet.CopyTo(data);

        using (MemoryStream stream = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            PacketType packetId = (PacketType)reader.ReadByte();
            if (packetId == PacketType.OnConnected)
            {
                int guest = reader.ReadInt32();
                string guestName = reader.ReadString();
                if(guest > 0)
                {
                    isGuest = guest;
                    myGuestName = guestName;
                }
            }
            else if (packetId == PacketType.EnterWorld)
            {
                byte[] datas = reader.ReadBytes(reader.ReadInt32());
                BSONObject worldData = SimpleBSON.Load(datas);
                worldsData = worldData;
                SceneManager.LoadScene("World");
            }
        }
    }
    public enum PacketType
    {
        Heartbeat = 1,
        OnConnected = 2,
        EnterWorld = 3
    }
}
