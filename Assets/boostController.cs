using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class boostController : NetworkBehaviour
{
    public Slider boostSlider;
    public float startingBoostValue = 100f;
    public float boostDecayRate = 0.5f;
    public float boostRegenRate = 0.2f;

    public float defaultTopSpeed = 15f;
    public float defaultAcceleration = 7f;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "KartSelect" && SceneManager.GetActiveScene().name != "TrackSelect")
        {
            boostSlider = NetworkRoomManager.singleton.transform.Find("UI").Find("BoostBar").GetComponent<Slider>();
            boostSlider.transform.Find("Fill").GetComponent<Image>().enabled = true;
            boostSlider.value = startingBoostValue;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space) && boostSlider.value > 1f) 
            setSpeed(defaultTopSpeed * 1.7f, defaultAcceleration * 2f);
        else
            setSpeed(defaultTopSpeed, defaultAcceleration);      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && isOwned) /// update the layer to the same layer of your BoostBox layer!!!
            boostSlider.value += 30f;
    }


    void setSpeed(float top, float acc)
    {
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.TopSpeed = top;    
        GetComponent<KartGame.KartSystems.ArcadeKart>().baseStats.Acceleration = acc;
        if (top > defaultTopSpeed) // if boosting then subtract from boost bar
            boostSlider.value -= boostDecayRate;
        else
            boostSlider.value += boostRegenRate;
    }

}
