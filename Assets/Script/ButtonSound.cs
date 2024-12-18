using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
	public GameManager gameManager;
	public AudioClip clickSound;

	private void Awake()
	{
		gameManager = FindObjectOfType<GameManager>();
	}

	private void Start()
	{
		Button button = GetComponent<Button>();
		if (button != null)
		{
			Debug.Log("button.onClick.AddListener(PlayClickSound);");
			button.onClick.AddListener(PlayClickSound);
		}
		else
		{
			Debug.LogWarning("Button component not found on the GameObject.");
		}
	}

	private void PlayClickSound()
	{
		gameManager.PlaySound(clickSound);
	}
}
