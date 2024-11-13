using UnityEngine;

public class NodeComponent : MonoBehaviour
{
	public int nodeId;
	public string nodeName;

	private PlayerManager playerManager;

	public void Initialize(int id, string name)
	{
		nodeId = id;
		nodeName = name;

		// Get reference to the PlayerManager
		playerManager = FindObjectOfType<PlayerManager>();
	}

	void OnMouseDown()
	{
		if (playerManager != null)
		{
			Debug.Log("Node clicked, attempting to move to node");
			playerManager.MoveToNode(this);
		}
	}

}
