using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class shootProjectile : NetworkBehaviour
{

    public GameObject projectilePrefab;
    public Transform projectileSpawnPosition;
    //public bool shootHeld = false;

    void Update()
    {
        if (Input.GetKeyDown("space") && isOwned)
        {
            //Debug.Log("you pressed space!");
            shootNetworked();
        }
    }

    [Command]
    void shootNetworked()
    {
        var activeProjectile = Instantiate(projectilePrefab, projectileSpawnPosition.position, projectileSpawnPosition.rotation);
        activeProjectile.GetComponent<Rigidbody>().AddRelativeForce(0f, 0f, 60000f);
        NetworkServer.Spawn(activeProjectile);
    }

}
