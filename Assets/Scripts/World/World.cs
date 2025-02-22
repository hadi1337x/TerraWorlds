using BasicTypes;
using Kernys.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static World;

public class World : MonoBehaviour
{
    public enum BlockClass
    {
        Ground = 1,
        Platform = 2,
        Background = 3,
        isWrenchable = 4,
        isHarvest = 5,
        END_OF_THE_ENUM
    }
    public enum BlockType
    {
        SoilBlock = 1,
        CaveBack = 2,
        Bedrock = 3,
        LavaBlock = 4,
        RockBlock = 5,
        BlackBrick = 6,
        RedBrick = 7,
        WhiteBrick = 8,
        GreenBrick = 9,
        BrownBrick = 10,
        TreeTrunk = 11,
        HedgeBlock = 12,
        Bush = 13,
        FireEscape = 14,
        FlowerGrass = 15,
        Grass = 16,
        MainEnterance = 17,
        END_OF_THE_ENUM
    }
    public enum GemType
    {
        Normal = 1,
        Farmable = 2,
        NoGem = 3
    }
    public enum WorldLayoutType
    {
        Empty = 1,
        Winter = 2
    }

    public GameObject BlockLayer;
    public GameObject BlockBackgroundLayer;
    public GameObject spritePrefab;
    private GameObject blockLayer;
    private GameObject[,] blockGameObjects;
    private GameObject[,] blockBackgroundGameObjects;
    private Transform[,] blockBackgroundSpriteTransforms;

    private SpriteRenderer[,] blockBackgroundSpriteRenderers;
    private SpriteRenderer[,] blockSpriteRenderers;

    private Vector2[] edgeColliderVerticiesTop;
    private Vector2[] edgeColliderVerticiesBottom;

    public Sprite[] blockSprites;

    private Transform[,] blockSpriteTransforms;

    private void Start()
    {
        BlockBackgroundLayer = GameObject.FindGameObjectWithTag("BlockBackgroundLayer");
        blockLayer = GameObject.FindGameObjectWithTag("BlockLayer");
        InitializeWorld("Test", 100, 60, ClientConn.conn.worldsData);
    }

    public void InitializeWorld(string worldName, int width, int height, BSONObject worldData)
    {
        blockGameObjects = new GameObject[width, height];
        blockSpriteRenderers = new SpriteRenderer[width, height];
        blockSpriteTransforms = new Transform[width, height];

        blockBackgroundGameObjects = new GameObject[width, height];
        blockBackgroundSpriteRenderers = new SpriteRenderer[width, height];
        blockBackgroundSpriteTransforms = new Transform[width, height];

        if (worldData.TryGetValue("Tiles", out BSONValue tilesValue) && tilesValue is BSONArray tileArray)
        {
            foreach (BSONObject tileObj in tileArray)
            {
                int x = tileObj["x"].int32Value;
                int y = tileObj["y"].int32Value;
                int fgID = tileObj["fg"].int32Value;
                int bgID = tileObj["bg"].int32Value;

                SetBlockBackground((BlockType)bgID, x, y);
                SetBlock((BlockType)fgID, x, y);
                Debug.Log("X "+ x + " Y "+  y + " FGID "+  fgID + " BGID " + bgID);
            }

            Debug.Log($"World '{worldName}' with {width}x{height} tiles loaded.");
        }
        else
        {
            Debug.LogError("Tiles array not found in world data.");
        }
    }


    public void SetBlock(World.BlockType type, int x, int y)
    {
        if (type != 0)
        {
            InstantiateBlockSprite(type, x, y);
            blockSpriteTransforms[x, y].gameObject.SetActive(true);
            AddBlockCollider(type, blockGameObjects[x, y], new Vector2i(x, y));
        }
    }
    public void SetBlockBackground(World.BlockType type, int x, int y)
    {
        if (type != 0)
        {
            InstantiateBlockBackgroundSprite(type, x, y);
            blockBackgroundSpriteRenderers[x, y].gameObject.SetActive(true);
        }
    }
    private void AddBlockCollider(World.BlockType type,GameObject BlockGameObject, Vector2i vec)
    {
        if (type == BlockType.FireEscape)
        {
            EdgeCollider2D edgeCollider = BlockGameObject.AddComponent<EdgeCollider2D>();
            edgeColliderVerticiesTop = new Vector2[2];
            edgeColliderVerticiesTop[0] = new Vector2(-0.4f, 0.5f);
            edgeColliderVerticiesTop[1] = new Vector2(0.4f, 0.5f);
        }
        else if (type == BlockType.MainEnterance)
        {
            EdgeCollider2D edgeCollider = BlockGameObject.AddComponent<EdgeCollider2D>();
            edgeColliderVerticiesBottom = new Vector2[2];
            edgeColliderVerticiesBottom[0] = new Vector2(-0.4f, -0.5f);
            edgeColliderVerticiesBottom[1] = new Vector2(0.4f, -0.5f);
        }
        else
        {
            BoxCollider2D boxCollider = BlockGameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1.0f, 1.0f); 
            boxCollider.offset = new Vector2(0, 0);
        }
    }
    private void InstantiateBlockSprite(World.BlockType blockType, int x, int y)
    {
        DestroyBlockSpriteOrQuad(x, y);
        GameObject gameObject = UnityEngine.Object.Instantiate(spritePrefab, new Vector3(x, y, 0), spritePrefab.transform.rotation);
        gameObject.transform.name = "Sprite";
        gameObject.transform.SetParent(blockLayer.transform);
        blockGameObjects[x, y] = gameObject;
        blockSpriteTransforms[x, y] = gameObject.transform;
        blockSpriteRenderers[x, y] = blockSpriteTransforms[x, y].GetComponent<SpriteRenderer>();
        blockSpriteRenderers[x, y].sprite = blockSprites[(int)blockType - 1];
    }

    private void InstantiateBlockBackgroundSprite(World.BlockType blockType, int x, int y)
    {
        DestroyBlockSpriteOrQuad(x, y);
        GameObject gameObject = UnityEngine.Object.Instantiate(spritePrefab, new Vector3(x, y, 0), spritePrefab.transform.rotation);
        gameObject.transform.name = "Sprite";
        gameObject.transform.SetParent(BlockBackgroundLayer.transform);
        blockBackgroundGameObjects[x, y] = gameObject;
        blockBackgroundSpriteTransforms[x, y] = gameObject.transform;
        blockBackgroundSpriteRenderers[x, y] = blockBackgroundSpriteTransforms[x, y].GetComponent<SpriteRenderer>();
        blockBackgroundSpriteRenderers[x, y].sprite = blockSprites[(int)blockType - 1];
    }
    private void DestroyBlockSpriteOrQuad(int x, int y)
    {
        if (blockSpriteTransforms[x, y] != null)
        {
            UnityEngine.Object.DestroyImmediate(blockSpriteTransforms[x, y].gameObject);
        }
    }
}
