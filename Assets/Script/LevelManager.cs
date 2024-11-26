using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public GraphData graphData;
	public PlayerManager playerManager;

	// Prefabs for different node types
	public GameObject normalNodePrefab;
	public GameObject blueSpecialNodePrefab;
	public GameObject orangeSpecialNodePrefab;

	// Edge sprites for different weights
	public Sprite weight1Sprite;
	public Sprite weight2Sprite;
	public Sprite weight3Sprite;

	private Dictionary<int, GameObject> nodeInstances;

	void Start()
	{
		InitializeGraph();
		SpawnPlayerAtFirstNode();
	}

	void InitializeGraph()
	{
		nodeInstances = new Dictionary<int, GameObject>();

		// Instantiate nodes
		foreach (var node in graphData.nodes)
		{
			GameObject nodePrefab = GetPrefabForNodeType(node.nodeType);
			if (nodePrefab == null)
			{
				Debug.LogError($"Prefab not assigned for node type: {node.nodeType}");
				continue;
			}

			GameObject nodeObject = Instantiate(nodePrefab, node.position, Quaternion.identity);
			nodeObject.name = $"Node_{node.nodeName}";

			NodeComponent nodeComponent = nodeObject.GetComponent<NodeComponent>();
			if (nodeComponent != null)
			{
				nodeComponent.Initialize(node.nodeId, node.nodeName, node.nodeType);
			}

			nodeInstances[node.nodeId] = nodeObject;
		}

		// Instantiate edges
		foreach (var edge in graphData.edges)
		{
			if (nodeInstances.TryGetValue(edge.startNodeId, out GameObject startNode) &&
				nodeInstances.TryGetValue(edge.endNodeId, out GameObject endNode))
			{
				CreateEdge(startNode.transform.position, endNode.transform.position, edge.weight);
			}
		}
	}

	void CreateEdge(Vector3 start, Vector3 end, int weight)
	{
		// Create an empty GameObject for the edge
		GameObject edgeObject = new GameObject("Edge");

		// Set the position between start and end nodes
		edgeObject.transform.position = (start + end) / 2;

		// Add SpriteRenderer to the edge to display the sprite
		SpriteRenderer spriteRenderer = edgeObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = GetSpriteByWeight(weight);

		// Rotate and scale the edge to connect the start and end points
		Vector3 direction = end - start;
		edgeObject.transform.right = direction; // Rotate to face the direction

		// Set size based on distance between nodes
		spriteRenderer.size = new Vector2(direction.magnitude, spriteRenderer.size.y);
	}

	Sprite GetSpriteByWeight(int weight)
	{
		switch (weight)
		{
			case 1: return weight1Sprite;
			case 2: return weight2Sprite;
			case 3: return weight3Sprite;
			default:
				Debug.LogWarning($"Unexpected weight: {weight}. Defaulting to weight 1 sprite.");
				return weight1Sprite;
		}
	}

	private GameObject GetPrefabForNodeType(NodeType nodeType)
	{
		switch (nodeType)
		{
			case NodeType.Normal: return normalNodePrefab;
			case NodeType.BlueSpecial: return blueSpecialNodePrefab;
			case NodeType.OrangeSpecial: return orangeSpecialNodePrefab;
			default:
				Debug.LogError($"Unknown NodeType: {nodeType}");
				return null;
		}
	}

	void SpawnPlayerAtFirstNode()
	{
		if (graphData.nodes.Count > 0)
		{
			Vector3 startPosition = graphData.nodes[0].position; // Get the first node's position
			playerManager.SpawnPlayerAt(startPosition);         // Spawn the player at the first node
		}
		else
		{
			Debug.LogWarning("No nodes in graphData to spawn the player at.");
		}
	}

	public void ConvertNodeToPrefab(int nodeId, NodeType newType)
	{
		if (!nodeInstances.TryGetValue(nodeId, out GameObject currentNode))
		{
			Debug.LogError($"Node with ID {nodeId} not found for conversion.");
			return;
		}

		// Get the new prefab based on the desired type
		GameObject newPrefab = GetPrefabForNodeType(newType);
		if (newPrefab == null)
		{
			Debug.LogError($"Prefab for NodeType {newType} not assigned.");
			return;
		}

		// Save the current node's properties
		Vector3 position = currentNode.transform.position;
		string nodeName = currentNode.name;

		// Destroy the old node
		Destroy(currentNode);

		// Instantiate the new prefab
		GameObject newNode = Instantiate(newPrefab, position, Quaternion.identity);
		newNode.name = nodeName;

		// Initialize the new node
		NodeComponent nodeComponent = newNode.GetComponent<NodeComponent>();
		if (nodeComponent != null)
		{
			nodeComponent.Initialize(nodeId, nodeName, newType);
		}

		// Update the nodeInstances dictionary
		nodeInstances[nodeId] = newNode;

		Debug.Log($"Converted Node {nodeId} to {newType}.");
	}

	public void CheckAndRevertSpecialNodes(int currentOrder)
	{
		Debug.Log("Check and convert");
		foreach (var node in graphData.nodes)
		{
			if (node.nodeType == NodeType.BlueSpecial && currentOrder >= 2)
			{
				Debug.Log("Blue");
				ConvertNodeToPrefab(node.nodeId, NodeType.Normal);
			}
			else if (node.nodeType == NodeType.OrangeSpecial && currentOrder >= 5)
			{
				Debug.Log("Orange");
				ConvertNodeToPrefab(node.nodeId, NodeType.Normal);
			}
		}
	}
}
