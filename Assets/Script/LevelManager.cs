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
		edgeObject.transform.SetParent(transform); // Optional: Parent it for better hierarchy management

		// Add LineRenderer component
		LineRenderer lineRenderer = edgeObject.AddComponent<LineRenderer>();

		// Configure the LineRenderer
		lineRenderer.positionCount = 2; // A line has two points
		lineRenderer.SetPosition(0, start); // Start point
		lineRenderer.SetPosition(1, end);   // End point

		lineRenderer.startWidth = GetLineWidthByWeight(weight); // Width at the start of the line
		lineRenderer.endWidth = GetLineWidthByWeight(weight);   // Width at the end of the line

		lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Assign a default material
		lineRenderer.startColor = GetLineColorByWeight(weight); // Color at the start
		lineRenderer.endColor = GetLineColorByWeight(weight);   // Color at the end
	}

	float GetLineWidthByWeight(int weight)
	{
		switch (weight)
		{
			case 1: return 0.2f; // Thin line
			case 2: return 0.3f;  // Medium line
			case 3: return 0.4f; // Thick line
			default: return 0.2f; // Default width
		}
	}

	Color GetLineColorByWeight(int weight)
	{
		string hexColor;

		switch (weight)
		{
			case 1:
				hexColor = "#0f6742";
				break;
			case 2:
				hexColor = "#177085";
				break;
			case 3:
				hexColor = "#763f3c";
				break;
			default:
				hexColor = "#3c3e57";
				break;
		}

		if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
		{
			return color;
		}
		else
		{
			Debug.LogWarning($"Invalid hex color: {hexColor}. Defaulting to white.");
			return Color.white;
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
