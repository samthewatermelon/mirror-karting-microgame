using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerPreferences : MonoBehaviour
{
    public static playerPreferences singleton;
    public string carPreference = "testCube";
    public string playerName;

    private void Start()
    {
        singleton = this;
        playerName = PlayerPrefs.GetString("savedName");
        transform.Find("UI").Find("PlayerName").GetComponent<TMPro.TMP_InputField>().text = playerName;
    }

    public void updatePlayerName()
    {
        playerName = transform.Find("UI").Find("PlayerName").GetComponent<TMPro.TMP_InputField>().text;
        PlayerPrefs.SetString("savedName", playerName);
        //GetComponent<Mirror.NetworkRoomManager>().enabled = true;
        //GetComponent<Mirror.NetworkManagerHUD>().enabled = true;
        transform.Find("UI").Find("PlayerName").gameObject.SetActive(false);
        transform.Find("UI").Find("Go").gameObject.SetActive(false);
    }

    public void clientConnect()
    {
        var ip = transform.Find("UI").Find("IP").GetComponent<TMPro.TMP_InputField>().text;
        Mirror.NetworkRoomManager.singleton.networkAddress = ip;
        Mirror.NetworkRoomManager.singleton.StartClient();
    }
}
