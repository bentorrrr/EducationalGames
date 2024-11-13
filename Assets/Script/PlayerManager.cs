using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
	public GameObject playerPrefab;
	private GameObject playerInstance;

	public bool isMoving = false;
	public float moveSpeed = 5f; // Speed of player movement
	public int playerScore = 0;

	public TextMeshProUGUI scoreText;

	private GraphData graphData;

	void Start()
	{
		graphData = FindObjectOfType<LevelManager>().graphData;
		UpdateScoreText();
	}

	public void SpawnPlayerAt(Vector3 position)
	{
		if (playerInstance == null)
		{
			playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
			playerInstance.name = "Player";
		}
		else
		{
			playerInstance.transform.position = position;
		}
	}

	public void MoveToNode(NodeComponent targetNode)
	{
		// Check if player is already moving
		if (isMoving)
			return;

		// Get the current node's ID based on player's current position
		int currentNodeId = GetClosestNodeId(playerInstance.transform.position);

		// Check if there is an edge connecting the current node to the target node
		EdgeData edge = GetEdgeBetweenNodes(currentNodeId, targetNode.nodeId);
		if (edge != null)
		{
			// Start the movement coroutine
			StartCoroutine(MovePlayer(targetNode, edge.weight));
		}
		else
		{
			Debug.LogWarning("No path between the current node and the selected node.");
		}
	}

	IEnumerator MovePlayer(NodeComponent targetNode, int edgeWeight)
	{
		isMoving = true;
		Debug.Log("Starting to move player");

		Vector3 startPosition = playerInstance.transform.position;
		Vector3 targetPosition = targetNode.transform.position;
		float journeyLength = Vector3.Distance(startPosition, targetPosition);
		float startTime = Time.time;

		while (Vector3.Distance(playerInstance.transform.position, targetPosition) > 0.01f)
		{
			float distCovered = (Time.time - startTime) * moveSpeed;
			float fractionOfJourney = distCovered / journeyLength;
			playerInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
			yield return null;
		}

		playerInstance.transform.position = targetPosition; // Ensure the player reaches the exact target position
		Debug.Log("Player reached the target node");

		playerScore += edgeWeight; // Update the player's score based on edge weight
		UpdateScoreText();
		Debug.Log("Player score: " + playerScore);

		isMoving = false;
	}


	private int GetClosestNodeId(Vector3 position)
	{
		float closestDistance = float.MaxValue;
		int closestNodeId = -1;

		foreach (var node in graphData.nodes)
		{
			float distance = Vector3.Distance(position, node.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestNodeId = node.nodeId;
			}
		}

		return closestNodeId;
	}

	private EdgeData GetEdgeBetweenNodes(int startNodeId, int endNodeId)
	{
		foreach (var edge in graphData.edges)
		{
			if ((edge.startNodeId == startNodeId && edge.endNodeId == endNodeId) ||
				(edge.startNodeId == endNodeId && edge.endNodeId == startNodeId))
			{
				return edge;
			}
		}
		return null;
	}

	private void UpdateScoreText()
	{
		if (scoreText != null)
		{
			scoreText.text = "Score: " + playerScore.ToString();  // Update the score text
		}
	}
}
