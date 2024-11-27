using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class NodeComponent : MonoBehaviour
{
	public int nodeId;
	public NodeType nodeType;
	public string nodeName;

	public GameObject normalNodeModel;
	public GameObject blueSpecialNodeModel;
	public GameObject orangeSpecialNodeModel;

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
		Debug.Log("Mouse Clicked");
		if (playerManager != null)
		{
			Debug.Log("Node clicked, attempting to move to node");
			playerManager.MoveToNode(this);
		}
	}
}
