using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleButton : MonoBehaviour
{
    public GameObject button;
    public void toggle()
    {
        if (button.activeInHierarchy)
            button.SetActive(false);
        else
            button.SetActive(true);

        gameObject.SetActive(false);
    }
}
