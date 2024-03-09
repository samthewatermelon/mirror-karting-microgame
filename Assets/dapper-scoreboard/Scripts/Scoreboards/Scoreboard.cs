using System.IO;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Leguar.TotalJSON;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DapperDino.Scoreboards
{
    public class Scoreboard : NetworkBehaviour
    {
        [SerializeField] private int maxScoreboardEntries = 5;
        public Transform highscoresHolderTransform = null;      // had to make this public to be accessed by raceManager
        [SerializeField] private GameObject scoreboardEntryObject = null;

        [SyncVar(hook = nameof(updateScoresForClient))]
        public string json; //made the json file in-memory to simplify sync across network

        [SyncVar]
        public int currentRaceHolder = 1;

        private playerPreferences pp;

        public static Scoreboard singleton;

        public string testName;
        public int testScore;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;                        
            ScoreboardSaveData savedScores = GetSavedScores();            
            UpdateUI(savedScores);
            CmdSaveScores(JsonUtility.ToJson(savedScores, true));
            pp = playerPreferences.singleton;
        }

        //[ContextMenu("run test function")] /// really useful feature for debugging!
        //void testFunction()
        //{
        //    Debug.Log("test function!");
        //}

        public string returnWinner()
        {
            JSON jsonObject = JSON.ParseString(json);
            JSON[] jarray = jsonObject.GetJArray("highscores").AsJSONArray();

            return jarray[0].GetString("entryName");
        }

        ScoreboardSaveData SortEntries()
        {           
            JSON[] jarray = JSON.ParseString(json).GetJArray("highscores").AsJSONArray();
            List<JSON> list = jarray.ToList().OrderByDescending(o => o.GetInt("entryScore")).ToList<JSON>();

            foreach (JSON item in list)
            {
                RemoveEntry(item.GetString("entryName"));
            }            

            ScoreboardSaveData savedScores = GetSavedScores();

            foreach (JSON item in list)
            {
                savedScores.highscores.Add(
                    new ScoreboardEntryData()
                    {
                        entryName = item.GetString("entryName"),
                        entryScore = item.GetInt("entryScore")
                    });
            }
            return savedScores;
        }
    

        private void Update()
        {
            if (SceneManager.GetActiveScene().name == "KartSelect" || SceneManager.GetActiveScene().name == "TrackSelect")
                return;

            if (Input.GetKeyDown(KeyCode.Tab))                         // added this to toggle scoreboard with tab
                highscoresHolderTransform.gameObject.SetActive(true);  // added this to toggle scoreboard with tab
                                                                       // added this to toggle scoreboard with tab
            if (Input.GetKeyUp(KeyCode.Tab))                           // added this to toggle scoreboard with tab
                highscoresHolderTransform.gameObject.SetActive(false); // added this to toggle scoreboard with tab
        }

        public void AddOrReplaceEntry(int playerAward)
        {
            int oldScore = RemoveEntry(pp.playerName);

            ScoreboardEntryData scoreboardEntryData = new ScoreboardEntryData()
            {
                entryName = pp.playerName,
                entryScore = playerAward + oldScore
            };

            ScoreboardSaveData savedScores = GetSavedScores();
            bool scoreAdded = false;            

            //Check if the score is high enough to be added.
            for (int i = 0; i < savedScores.highscores.Count; i++)
            {
                if (playerAward > savedScores.highscores[i].entryScore)
                {
                    savedScores.highscores.Insert(i, scoreboardEntryData);
                    scoreAdded = true;
                    break;
                }
            }

            //Check if the score can be added to the end of the list.
            if (!scoreAdded && savedScores.highscores.Count < maxScoreboardEntries)
            {
                savedScores.highscores.Add(scoreboardEntryData);
            }

            //Remove any scores past the limit.
            if (savedScores.highscores.Count > maxScoreboardEntries)
            {
                savedScores.highscores.RemoveRange(maxScoreboardEntries, savedScores.highscores.Count - maxScoreboardEntries);
            }

            UpdateUI(savedScores);
            json = JsonUtility.ToJson(savedScores, true);            /// save this locally for the meantime because this is required for sorting
            CmdSaveScores(JsonUtility.ToJson(SortEntries(), true));  /// sorts and saves the entries to all clients
        }

        public void RemoveAllEntries()
        {
            if (string.IsNullOrEmpty(json))
                return;

            JSON jsonObject = JSON.ParseString(json);
            JSON[] jarray = jsonObject.GetJArray("highscores").AsJSONArray();

            for (int j = 0; j < jarray.Length; j++)
            {
                Debug.Log("removing one at a time til their all gone");
                jsonObject.GetJArray("highscores").RemoveAt(0);
            }

            json = jsonObject.CreateString();
        }

        public int RemoveEntry(string recordToRemove)
        {
            if (string.IsNullOrEmpty(json))
                return 0;
            else
            {
                JSON jsonObject = JSON.ParseString(json);
                JSON[] jarray = jsonObject.GetJArray("highscores").AsJSONArray();

                int oldScore = 0;
                for (int j = 0; j < jarray.Length; j++)
                {
                    if (jarray[j].GetString("entryName") == recordToRemove)
                    {
                        oldScore = jarray[j].GetInt("entryScore");
                        jsonObject.GetJArray("highscores").RemoveAt(j);
                        json = jsonObject.CreateString();
                        return oldScore;
                    }
                }
                return oldScore;
            }
        }

        private void updateScoresForClient(string oldJson, string newJson)
        {
            UpdateUI(JsonUtility.FromJson<ScoreboardSaveData>(newJson));
        }

        private void UpdateUI(ScoreboardSaveData savedScores)
        {
            foreach (Transform child in highscoresHolderTransform)
                Destroy(child.gameObject);

            foreach (ScoreboardEntryData highscore in savedScores.highscores)
                Instantiate(scoreboardEntryObject, highscoresHolderTransform).GetComponent<ScoreboardEntryUI>().Initialise(highscore);
        }

        private ScoreboardSaveData GetSavedScores() /// updated code to fetch from variable instead of from disk
        {
            if (string.IsNullOrEmpty(json))
                    return new ScoreboardSaveData();

            return JsonUtility.FromJson<ScoreboardSaveData>(json);
        }

        [Command(requiresAuthority = false)]
        private void CmdSaveScores(string scoreboardSaveData)   //saves the score as json string on server which replicates on clients via syncvar hook
        {
            json = scoreboardSaveData;
        }
    }
}
