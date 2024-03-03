using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    public GameObject nextCheckpoint;

    private void Start()
    {
        if (gameObject.tag == "Checkpoint")
            gameObject.SetActive(false);
    }

    public void activateNextLap()
    {
        if (nextCheckpoint.tag == "Checkpoint")
        {
            nextCheckpoint.SetActive(true);
            gameObject.SetActive(false);
        }
        if (nextCheckpoint.tag == "StartFinish")
        {
            nextCheckpoint.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
