using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemExpire : MonoBehaviour
{
    public float lifetime;

    private void OnEnable()
    {
        StartCoroutine(expire());
    }

    public IEnumerator expire()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }
}
