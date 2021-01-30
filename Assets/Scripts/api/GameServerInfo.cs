using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameServer
{
    public int id;
    public string main;
}
[System.Serializable]
public class GameServerInfo
{
    public int id;
    public string name;
    public List<GameServer> GameServer;
}