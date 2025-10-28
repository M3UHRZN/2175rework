using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Game Started");
    }

    public void Load()
    {
        // Yeni Save Slot sekmesi açınca buraya eklicem.
        Debug.Log("Game Loaded");
    }

    public void Options()
    {
        //Options sekmesi açınca buraya eklicem.
        Debug.Log("Options Opened");
    }

    public void Exit()
    {
        Debug.Log("Exiting Game");
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}