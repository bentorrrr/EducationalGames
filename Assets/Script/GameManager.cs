using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager instance;
	private AudioSource audioSource;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void PlaySound(AudioClip clip)
	{
		if (audioSource != null && clip != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}
}
