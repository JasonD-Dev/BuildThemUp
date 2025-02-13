using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main_Menu" || SceneManager.GetActiveScene().name == "Game_Win" || SceneManager.GetActiveScene().name == "Game_Over")
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void LoadScene(string aSceneName)
    {
        Log.Info(this, $"Chaning scene to {aSceneName}");
        SceneManager.LoadScene(aSceneName);
        Log.Info(this, "Scene changed to: \'" + aSceneName + "\'.");
    }
    public void QuitGame()
    {
        Application.Quit();
        Log.Info(this, "Quit game.");
    }
}
