using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pauseMenu : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "Intro")
            pauseGame();
    }
    public void pauseGame()
    {
        foreach (Transform t in transform.GetComponentInChildren<Transform>())
            t.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void unPauseGame()
    {
        foreach (Transform t in transform.GetComponentInChildren<Transform>())
            t.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void changeScene()
    {
        string newScene = transform.Find("LevelSelect").GetComponent<TMPro.TMP_InputField>().text; 
        foreach (Transform t in transform.GetComponentInChildren<Transform>())
            t.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Mirror.NetworkManager.singleton.ServerChangeScene("level" + newScene);
    }
    public void quitGame()
    {
        Application.Quit();
    }
}
