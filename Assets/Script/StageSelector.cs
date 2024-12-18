using TMPro;
using UnityEngine;
using UnityEngine.UI; // For handling UI elements like Image

public class StageSelector : MonoBehaviour
{
	public GameObject[] lockedObjects;
	public GameObject[] unlockedObjects;
	public GameObject[] clearedObjects;
	public Image[] starImages;

	public int totalStarsEarned = 0;
	public TextMeshProUGUI starsText;

	public Sprite starSprite0;
	public Sprite starSprite1;
	public Sprite starSprite2;
	public Sprite starSprite3;

	private void Start()
	{
		InitializePlayerPrefs();
		UpdateStageStates();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetProgress();
		}
	}

	private void UpdateStageStates()
	{
		for (int i = 0; i < lockedObjects.Length; i++)
		{
			int levelStatus = PlayerPrefs.GetInt($"Level{i + 1}Status", 0);
			int starsEarned = PlayerPrefs.GetInt($"Level{i + 1}Stars", 0);

			// Status 0: Locked, 1: Unlocked, 2: Cleared
			lockedObjects[i].SetActive(levelStatus == 0);
			unlockedObjects[i].SetActive(levelStatus == 1);
			clearedObjects[i].SetActive(levelStatus == 2);

			UpdateStarSprite(i, starsEarned);
			totalStarsEarned += starsEarned;
		}
		starsText.text = totalStarsEarned.ToString() + "/12";
	}

	private void InitializePlayerPrefs()
	{
		if (!PlayerPrefs.HasKey("IsFirstLaunch"))
		{
			// Set default PlayerPrefs values
			for (int i = 1; i <= lockedObjects.Length; i++)
			{
				PlayerPrefs.SetInt($"Level{i}Status", 0); // Lock all levels
				PlayerPrefs.SetInt($"Level{i}Stars", 0);
			}

			PlayerPrefs.SetInt("Level1Status", 1);
			PlayerPrefs.SetInt("IsFirstLaunch", 1);
			PlayerPrefs.Save();
		}
	}

	private void UpdateStarSprite(int levelIndex, int starsEarned)
	{
		if (starImages.Length > levelIndex)
		{
			switch (starsEarned)
			{
				case 0:
					starImages[levelIndex].sprite = starSprite0;
					break;
				case 1:
					starImages[levelIndex].sprite = starSprite1;
					break;
				case 2:
					starImages[levelIndex].sprite = starSprite2;
					break;
				case 3:
					starImages[levelIndex].sprite = starSprite3;
					break;
			}
		}
	}

	public void ResetProgress()
	{
		for (int i = 1; i <= lockedObjects.Length; i++)
		{
			PlayerPrefs.SetInt($"Level{i}Status", 0);
			PlayerPrefs.SetInt($"Level{i}Stars", 0);
		}

		// Unlock the first level by default
		PlayerPrefs.SetInt("Level1Status", 1);

		PlayerPrefs.DeleteKey("IsFirstLaunch");

		PlayerPrefs.Save();
		totalStarsEarned = 0;
		UpdateStageStates(); // Refresh UI after resetting progress
	}
}
