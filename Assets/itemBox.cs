using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Mirror;

public class itemBox : NetworkBehaviour
{
    public GameObject itemPickedUpParticleEffect;
   private IEnumerator itemBoxCollected()
    {
        //Debug.Log("itemBox collected on client");
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        itemPickedUpParticleEffect.SetActive(true);
        yield return new WaitForSeconds(3f);
        itemPickedUpParticleEffect.SetActive(false);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<BoxCollider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(itemBoxCollected()); // run this locally first to avoid double ups
        //if (other.gameObject.layer == 13 && isServer) // 13 is Kart layer
        if (isServer) // 13 is Kart layer
        {            
            RpcItemBoxCollected();
            var root = other.transform.parent;
            NetworkIdentity playerId = root.GetComponent<NetworkIdentity>();
        }
    }

    [ClientRpc]
    void RpcItemBoxCollected()
    {
        StartCoroutine(itemBoxCollected());
    }
}
