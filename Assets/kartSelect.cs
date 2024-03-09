using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class kartSelect : NetworkBehaviour
{
    public GameObject selectedCar;
    public GameObject uiElements;

    private void Start()
    {
        if (!isLocalPlayer || SceneManager.GetActiveScene().name != "KartSelect")
            uiElements.SetActive(false);
        if (!isServer)
            uiElements.transform.Find("OK").gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    private void CmdSpawn(string carName)
    {
        GameObject carPrefab = NetworkManager.singleton.spawnPrefabs.Find(car => car.name == carName);
        if (carPrefab == null)
            return;

        NetworkServer.Destroy(selectedCar);
        GameObject carObj = Instantiate(carPrefab, transform.position, Quaternion.identity);
        selectedCar = carObj;
        //NetworkServer.Spawn(carObj, connectionToClient);
        NetworkServer.Spawn(carObj, gameObject);
    }

    public override void OnStartLocalPlayer() // spawn car when player joins
    {
        base.OnStartLocalPlayer();
        var savedCarPref = playerPreferences.singleton.carPreference;
        if (savedCarPref != "")
        {
            CmdSpawn(savedCarPref);
        }
    }
    public void pickCar(string carSelect)
    {
        playerPreferences.singleton.carPreference = carSelect;
        CmdSpawn(carSelect);
    }
    public void loadScene(string sceneName)
    {
        if (isServer)
            NetworkManager.singleton.ServerChangeScene(sceneName);
    }
}
