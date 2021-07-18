using System.Collections;
using System.Collections.Generic;
using FoW;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering;

public class FogController : MonoBehaviour
{
    public Volume volume;

    // Start is called before the first frame update
    void Start()
    {
        var playerid =  NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
        VolumeProfile profile = volume.sharedProfile;
        if (profile != null && profile.TryGet(out FogOfWarURP fow))
            fow.team.value = playerid;
    }
}
