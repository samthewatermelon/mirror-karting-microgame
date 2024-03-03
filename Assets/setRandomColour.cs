using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class setRandomColour : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetPlayerColour))]
    Color playerColour = Color.black;

    [SyncVar(hook = nameof(SetCarColour))]
    Color carColour = Color.black;

    public SkinnedMeshRenderer playerBody;
    public MeshRenderer carBody;

    void Start()
    {
        if (isServer)
        {
            playerColour = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            carColour = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
    }

    void SetPlayerColour(Color oldColour, Color newColour)
    {
        playerBody.material.color = newColour;
    }
    void SetCarColour(Color oldColour, Color newColour)
    {
        carBody.material.color = newColour;
    }
}
