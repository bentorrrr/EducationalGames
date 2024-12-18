using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class NodeComponent : MonoBehaviour
{
	public int nodeId;
	public NodeType nodeType;
	public string nodeName;

	private PlayerManager playerManager;
	public AudioSource audioSource;

	public void Initialize(int id, string name, NodeType type)
	{
		nodeId = id;
		nodeName = name;
		nodeType = type;

		playerManager = FindObjectOfType<PlayerManager>();
		audioSource = GetComponent<AudioSource>();
	}

	void OnMouseDown()
	{
		Debug.Log("Mouse Clicked");
		if (playerManager != null)
		{
			Debug.Log("Node clicked, attempting to move to node");
			playerManager.MoveToNode(this);
		}
	}

	public void PlaySound()
	{
		if (audioSource != null && audioSource.clip != null)
		{
			GameObject lingeringSoundObject = new GameObject("LingeringSound");
			lingeringSoundObject.transform.position = transform.position;

			AudioSource lingeringAudioSource = lingeringSoundObject.AddComponent<AudioSource>();
			lingeringAudioSource.clip = audioSource.clip;
			lingeringAudioSource.volume = audioSource.volume;
			lingeringAudioSource.pitch = audioSource.pitch;
			lingeringAudioSource.loop = false;
			lingeringAudioSource.Play();

			Destroy(lingeringSoundObject, lingeringAudioSource.clip.length);
		}
		else
		{
			Debug.LogWarning("AudioSource or AudioClip is missing. Unable to play lingering sound.");
		}
	}
}
