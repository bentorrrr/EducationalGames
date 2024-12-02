using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
	public Slider progressBar;  // Assign in the Inspector
	public TextMeshProUGUI loadingText;   // Optional, for displaying progress text
	public float fakeLoadingTime = 3f;

	private void Start()
	{
		StartCoroutine(UpdateLoadingProgress());
	}

	private IEnumerator UpdateLoadingProgress()
	{
		float elapsedTime = 0f;

		// Simulate loading progress
		while (elapsedTime < fakeLoadingTime)
		{
			elapsedTime += Time.deltaTime;
			float progress = Mathf.Clamp01(elapsedTime / fakeLoadingTime);

			// Update progress bar and text
			if (progressBar != null)
				progressBar.value = progress;

			if (loadingText != null)
				loadingText.text = $"Loading... {Mathf.FloorToInt(progress * 100)}%";

			yield return null;
		}

		LoadTargetScene();
	}

	private void LoadTargetScene()
	{
		string targetScene = SceneManager.targetScene; // Get the target scene from the static variable

		if (!string.IsNullOrEmpty(targetScene))
		{
			Debug.Log($"Loading Target Scene: {targetScene}");
			UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene); // Load the actual target scene
		}
		else
		{
			Debug.LogError("Target scene is not set in SceneManager!");
		}
	}
}
