using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void JoinWorld()
    {
        ClientConn.conn.SendEnterWorld("DAILYBONUS");
    }
}