using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class raceManager : NetworkBehaviour
{
    public string localPlayerName;
    public AudioSource youLose;
    public static raceManager singleton;
    public int totalLaps = 3;
    public int totalRaces = 2;

    public TextMeshProUGUI raceOutcome;

    [SyncVar]
    public int positionHolder = 0;

    [SyncVar]
    public int numberOfPlayers;

    [SyncVar(hook = nameof(removeAll))] public bool racesComplete = false;

    private DapperDino.Scoreboards.Scoreboard sb;

    private void Start()
    {
        raceOutcome = GetComponent<TextMeshProUGUI>();
        sb = DapperDino.Scoreboards.Scoreboard.singleton;
        singleton = this;
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdatePlayerCount()
    {
        numberOfPlayers = NetworkRoomManager.singleton.numPlayers;
    }

    public void removeAll(bool oldVal, bool newVal)
    {
        if (oldVal != newVal && isServer)
            sb.RemoveAllEntries();
    }

    public int handOutPosition()
    {
        return positionHolder;        
    }

    [Command(requiresAuthority = false)]
    public void CmdIncrementPos()
    {
        positionHolder++;
        if ((positionHolder - 1) == numberOfPlayers)
        {            
            if (sb.currentRaceHolder == totalRaces) /// if this is the last race in the series
            {
                Debug.Log("all races finished - showing scores then returning to kart selection");
                sb.currentRaceHolder = 1; // resetting in case players want to race again
                sb.highscoresHolderTransform.gameObject.SetActive(true);
                raceOutcome.text = sb.returnWinner() + " WINS";
                setClientWinner(sb.returnWinner());
                StartCoroutine(delayThenChangeScene(10f, "KartSelect", true)); 
            }
            else
            {
                Debug.Log("all players finished race - returning to track selection");
                sb.currentRaceHolder++; // not last race so incrementing race counter
                StartCoroutine(delayThenChangeScene(3f, "TrackSelect", false));
            }   
        }            
    }

    [ClientRpc]
    public void setClientWinner(string winner)
    {
        raceOutcome.text = winner + " WINS";
        sb.highscoresHolderTransform.gameObject.SetActive(true);
    }

    IEnumerator delayThenChangeScene(float delay, string scene, bool lastRace)
    {
        yield return new WaitForSeconds(delay);
        sb.highscoresHolderTransform.gameObject.SetActive(false);
        if (lastRace)
            racesComplete = true;
            //sb.RemoveAllEntries();
        NetworkRoomManager.singleton.ServerChangeScene(scene);
    }
}
