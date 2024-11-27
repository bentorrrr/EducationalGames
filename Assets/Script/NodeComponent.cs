using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class NodeComponent : MonoBehaviour
{
	public int nodeId;
	public NodeType nodeType;
	public string nodeName;

	private PlayerManager playerManager;

	public void Initialize(int id, string name, NodeType type)
	{
		nodeId = id;
		nodeName = name;
		nodeType = type;

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
