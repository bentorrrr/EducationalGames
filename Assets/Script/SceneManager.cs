using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
	public static string targetScene;
	public string loadingScreenSceneName = "LoadingScene"; // Set the name of the loading screen scene

	public void LoadScene(string sceneName)
	{
		Debug.Log("Loading Scene: " + sceneName);
		targetScene = sceneName;
		UnityEngine.SceneManagement.SceneManager.LoadScene(loadingScreenSceneName);
	}

	public void ReloadScene()
    {
		string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		LoadScene(currentScene);
	}

    public void QuitGame()
    {
        Application.Quit();
    }
}
