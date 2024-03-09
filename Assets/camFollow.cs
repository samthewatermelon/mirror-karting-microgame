using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class camFollow : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "KartSelect" || SceneManager.GetActiveScene().name == "TrackSelect")
            return;

        if (isOwned)
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = gameObject.transform;
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>().LookAt = gameObject.transform;
        }
    }
}
