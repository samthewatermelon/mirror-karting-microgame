using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class serverFunctions : MonoBehaviour
{
    public GameObject ipAddressField;
    public GameObject steamUi;
    public GameObject[] regularUi;

    private void Start()
    {
        Debug.Log(GetComponent<SteamRoomManager>().transport.GetType().Name);

        if (GetComponent<SteamRoomManager>().transport.GetType().Name != "FizzySteamworks")
            steamUi.SetActive(false);
        else
            turnOffUi();
    }

    public void setServerConnectAddress()
    {
        SteamRoomManager.singleton.networkAddress = ipAddressField.GetComponent<TMPro.TMP_InputField>().text;
        SteamRoomManager.singleton.StartClient();
    }

    public void turnOffUi()
    {
        foreach (GameObject go in regularUi)
            go.SetActive(false);
    }
}
