using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class customNetworkHud : NetworkBehaviour
{
    NetworkManager manager;

    void Start()
    {
        manager = NetworkManager.singleton;
    }

    public void HostButton()
    {
        if (!NetworkClient.active)
        {
            manager.StartHost();
        }
    }

    public void StopHost()
    {
        manager.StopHost();
    }
}
