using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class itemManager : NetworkBehaviour
{
    public UnityEngine.UI.Image[] itemImages;
    public GameObject[] itemPrefabs, itemAudio;
    public GameObject[] statePrefabs, stateAudio;
    public Transform bulletSpawnPosition;
    public GameObject onScreenUi;

    private int currentImageInt = 0;
    private bool randIsRunning = false;
    private bool cycleBegin = false;

    private void Start()
    {
        if (isOwned && SceneManager.GetActiveScene().name != "KartSelect" && SceneManager.GetActiveScene().name != "TrackSelect")
            onScreenUi.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!randIsRunning && cycleBegin)
            StartCoroutine(randomInterval(0.1f));
    }

    private void OnTriggerEnter(Collider other)  /// Picks up item boxes on the ground
    {
        if (other.gameObject.layer == 3 && !itemImages[currentImageInt].enabled) /// will only pick it up if there isn't item equipped
            StartCoroutine(itemPickedUp(3f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        /// deals with bullet collision
        if (collision.gameObject.layer == 6 &&                                                                            /// if the collision is in the bullet layer
            !itemPrefabs[1].activeInHierarchy &&                                                                          /// and the shield is not equipped
            isServer &&                                                                                                   /// and this is running on the server
            collision.transform.GetComponent<pistolBullet>().bulletOwner != GetComponent<NetworkIdentity>().netId)        /// and the bullet does not belong to this kart
        {
            RpcClientDisableCar(3f);
        }
    }

    [ClientRpc]
    void RpcClientDisableCar(float interval)
    {
        StartCoroutine(stopCar(interval)); // need to call a non-mirror function because its a coroutine!
    }

    private IEnumerator stopCar(float interval)
    {
        Debug.Log("stopping car!");
        GetComponent<Rigidbody>().velocity = new Vector3();              /// stops the car completely
        GetComponent<KartGame.KartSystems.ArcadeKart>().enabled = false; /// disable kart controls
        statePrefabs[0].SetActive(true);                                 /// turn on red bubble
        yield return new WaitForSeconds(interval);                       /// wait a few seconds...
        GetComponent<KartGame.KartSystems.ArcadeKart>().enabled = true;  /// enable kart controls
        statePrefabs[0].SetActive(false);                                /// turn red bubble off again
    }

    private IEnumerator itemPickedUp(float interval)
    {
        cycleBegin = true;
        yield return new WaitForSeconds(interval);
        cycleBegin = false;
    }

    private IEnumerator randomInterval(float interval)
    {
        randIsRunning = true;
        itemImages[currentImageInt].enabled = false;

        var prevImageInt = currentImageInt;
        var newImageInt = Random.Range(0, itemImages.Length);
        //Debug.Log(newImageInt);
        currentImageInt++;
        if (currentImageInt == itemImages.Length)
            currentImageInt = Random.Range(0, itemImages.Length - 1);
        itemImages[currentImageInt].enabled = true;
        yield return new WaitForSeconds(interval);
        randIsRunning = false;
    }

    [Command] /// Runs server command
    void CmdShootBubbles(Vector3 pos, Quaternion rot, uint kartNetId)
    {
        Debug.Log("sender: " + kartNetId);
        GameObject bullet = Instantiate(itemPrefabs[2], pos, rot);
        NetworkServer.Spawn(bullet);
        bullet.GetComponent<pistolBullet>().bulletOwner = kartNetId;  /// setting the owner of the bullet
        RpcShootBubbles();
    }

    [ClientRpc]
    void RpcShootBubbles()
    {
        itemAudio[2].GetComponent<AudioSource>().Play();
    }

    [Command]
    void CmdActivateShield()
    {
        RpcActivateShield();
    }
    
    [ClientRpc]
    void RpcActivateShield()
    {
        itemPrefabs[1].SetActive(true);
        itemAudio[1].GetComponent<AudioSource>().Play();
    }

    [Command]
    void CmdActivateNitro()
    {
        RpcActivateNitro();
    }

    [ClientRpc]
    void RpcActivateNitro()
    {
        itemPrefabs[0].SetActive(true);
        itemAudio[0].GetComponent<AudioSource>().Play();
    }

    public IEnumerator speedBoost(float speedTime)
    {
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.TopSpeed = 25f;
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.Acceleration = 12f;
        yield return new WaitForSeconds(speedTime);
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.TopSpeed = 15f;
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.Acceleration = 7f;
    }

    private void Update()
    {
        if (isOwned && Input.GetKeyDown(KeyCode.Space))
            useItem();
    }

    public void useItem()
    {
        if ((itemImages[currentImageInt].enabled) && isOwned && !cycleBegin)
        {
            itemImages[currentImageInt].enabled = false;
            switch (itemImages[currentImageInt].name)
            {
                case "Nitro":
                    Debug.Log("nitro");                    
                    CmdActivateNitro();
                    StartCoroutine(speedBoost(3f));
                    break;
                case "Shield":
                    Debug.Log("Shield");
                    CmdActivateShield();
                    break;
                case "Pistol":
                    Debug.Log("Pistol");
                    CmdShootBubbles(bulletSpawnPosition.position, bulletSpawnPosition.rotation, GetComponent<NetworkIdentity>().netId);
                    break;
            }
        }
            Debug.Log("using item: " + itemImages[currentImageInt].name);
    }
}
