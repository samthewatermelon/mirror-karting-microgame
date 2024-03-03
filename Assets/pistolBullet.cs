using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class pistolBullet : NetworkBehaviour
{
    public float force;
    public float lifetime;
    [SyncVar] public uint bulletOwner;
    private void Start()
    {
        if (isServer)
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * force);
            StartCoroutine(expireBullet());
        }

    }

    public IEnumerator expireBullet()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

}
