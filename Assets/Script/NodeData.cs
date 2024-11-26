using UnityEngine;

public enum NodeType
{
	Normal,
	BlueSpecial,  // C (Blue)
	OrangeSpecial // E (Orange)
}

[System.Serializable]
public class NodeData
{
	public string nodeName;
	public int nodeId;        // Unique ID for the node
	public NodeType nodeType = NodeType.Normal;
	public Vector2 position;  // Position of the node in 2D space
}
