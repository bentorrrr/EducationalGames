using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public GraphData graphData;
	public GameObject nodePrefab;
	public PlayerManager playerManager;

	// Edge sprites for different weights
	public Sprite weight1Sprite;
	public Sprite weight2Sprite;
	public Sprite weight3Sprite;

	public Dictionary<int, GameObject> nodeInstances;

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
			GameObject nodeObject = Instantiate(nodePrefab, node.position, Quaternion.identity);
			nodeObject.name = $"Node_{node.nodeName}";

			NodeComponent nodeComponent = nodeObject.GetComponent<NodeComponent>();
			if (nodeComponent != null)
			{
				nodeComponent.Initialize(node.nodeId, node.nodeName);
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
		edgeObject.transform.right = direction;  // Rotate to face the direction
		spriteRenderer.size = new Vector2(direction.magnitude, spriteRenderer.size.y);  // Stretch to match the distance
	}

	Sprite GetSpriteByWeight(int weight)
	{
		if (weight == 1)
			return weight1Sprite;
		else if (weight == 2)
			return weight2Sprite;
		else if (weight == 3)
			return weight3Sprite;
		else
			return weight1Sprite;  // Default to weight 1 if unexpected weight
	}

	void SpawnPlayerAtFirstNode()
	{
		if (graphData.nodes.Count > 0)
		{
			Debug.Log("Ya");
			Vector3 startPosition = graphData.nodes[0].position;  // Get the first node's position
			playerManager.SpawnPlayerAt(startPosition);           // Spawn the player at the first node
		}
		else
		{
			Debug.LogWarning("No nodes in graphData to spawn the player at.");
		}
	}
}
