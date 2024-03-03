using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class loadTrack : NetworkBehaviour
{
    [Scene]
    public string trackScene;
    
    public void loadTrackScene()
    {
        if (isServer)
        {
            Debug.Log("you loaded track " + trackScene);
            NetworkManager.singleton.ServerChangeScene(trackScene);
        }
    }
}
