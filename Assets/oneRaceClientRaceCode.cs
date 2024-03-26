using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class oneRaceClientRaceCode : NetworkBehaviour
{
    private int totalLaps;
    public int currentLap = 0;
    public Transform lastCheckpoint;
    public AudioSource LapCountSound;
    public AudioSource youWin;
    public DapperDino.Scoreboards.OneRaceScoreboard sb;
    public TextMeshPro numberPlateName;
    [SyncVar(hook = nameof(setPlateName))]
    public string plateName;

    private oneRaceRaceManager rm;

    private void Start()
    {
        CmdSetNumberPlateName(playerPreferences.singleton.playerName);

        if (SceneManager.GetActiveScene().name == "KartSelect" || SceneManager.GetActiveScene().name == "TrackSelect")
            return;

        rm = oneRaceRaceManager.singleton;
        rm.CmdUpdatePlayerCount();
        totalLaps = rm.totalLaps;
        sb = DapperDino.Scoreboards.OneRaceScoreboard.singleton;
        sb.highscoresHolderTransform.gameObject.SetActive(false);

        sb.RemoveAllEntries();
    }
    void setPlateName(string oldName, string newName)
    {
        numberPlateName.text = newName;
    }

    [Command]
    public void CmdSetNumberPlateName(string name)
    {
        plateName = name;
    }

    private void OnTriggerEnter(Collider other)
    {

        TextMeshProUGUI lc = GameObject.Find("UI/LapCount").GetComponent<TextMeshProUGUI>();

        if (other.tag == "Checkpoint" && isOwned)
        {
            Debug.Log("you touched a checkpoint!");
            other.GetComponent<checkpoint>().activateNextLap();
            lastCheckpoint = other.transform;
        }
        if (other.tag == "StartFinish" && currentLap <= totalLaps && isOwned)
        {
            currentLap++;
            Debug.Log("you touched the start/finish line!");
            //LapCountSound.Play();
            other.GetComponent<checkpoint>().activateNextLap();
            for (int i = 0; i < currentLap; i++)
            {
                lc.text = "Lap: " + currentLap.ToString() + " / " + totalLaps;
                lc.color = Color.yellow;
            }
        }
        if (other.tag == "StartFinish" && currentLap > totalLaps && isOwned)
        {
            if (rm.raceOutcome.text == "")
            {
                rm = oneRaceRaceManager.singleton;
                lc.text = "";
                int pos = rm.handOutPosition();
                rm.raceOutcome.text = "POSITION " + pos;           //grabs new position from server and shows it on the local player
                rm.raceOutcome.color = Color.green;
                sb.AddOrReplaceEntry(pos);// m.numberOfPlayers - pos);
                rm.CmdIncrementPos();
                //youWin.Play();
            }            
        }
    }

    private void Update()
    {
       if (Input.GetKeyDown(KeyCode.R) && isOwned) /// respawn player
        {
            gameObject.transform.position = lastCheckpoint.position;
            gameObject.transform.rotation = lastCheckpoint.rotation;
        }
    }
}