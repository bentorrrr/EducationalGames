using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
	public GameObject playerPrefab;
	private GameObject playerInstance;

	public bool isMoving = false;
	public float moveSpeed = 10f;
	public int totalWeightUsed = 0;
	public int currentOrder = 0;

	public TextMeshProUGUI scoreText;
	public GameObject levelCompleteWindow;
	public TextMeshProUGUI finalScoreText;
	public TextMeshProUGUI pathText;
	public GameObject levelFailedWindow;
	public TextMeshProUGUI weightUsedText;
	public TextMeshProUGUI weightLimitText;

	private GraphData graphData;
	private List<int> visitedNodes = new List<int>();
	private List<int> pathTaken = new List<int>();

	private bool isGameCompleted = false;

	void Awake()
	{
		graphData = FindObjectOfType<LevelManager>().graphData;
	}

	void Start()
	{
		UpdateScoreText();

		if (levelCompleteWindow != null) levelCompleteWindow.SetActive(false);
		if (levelFailedWindow != null) levelFailedWindow.SetActive(false);
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

		int startNodeId = GetClosestNodeId(position);
		if (startNodeId != -1 && !visitedNodes.Contains(startNodeId))
		{
			visitedNodes.Add(startNodeId);
			pathTaken.Add(startNodeId);
		}
	}

	public void MoveToNode(NodeComponent targetNode)
	{
		// Check if player is already moving
		if (isMoving || isGameCompleted)
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
		currentOrder = pathTaken.Count;

		totalWeightUsed += edgeWeight; // Update the player's score based on edge weight
		AdjustWeightForSpecialNodes(targetNode.nodeId, currentOrder);
		UpdateScoreText();
		Debug.Log("Player score: " + totalWeightUsed);

		if (!visitedNodes.Contains(targetNode.nodeId))
		{
			visitedNodes.Add(targetNode.nodeId);
		}

		pathTaken.Add(targetNode.nodeId);

		if (!isGameCompleted)
		{
			if (totalWeightUsed > graphData.weightLimit)
			{
				DisplayLevelFailed();
			}
			else
			{
				CheckWinCondition(targetNode.nodeId);
			}
		}

		isMoving = false;

		LevelManager levelManager = FindObjectOfType<LevelManager>();
		if (levelManager != null)
		{
			Debug.Log("levelmanager");
			levelManager.CheckAndRevertSpecialNodes(currentOrder);
			levelManager.MarkNodeAsVisited(targetNode.nodeId);
		}
	}

	private void CheckWinCondition(int currentNodeId)
	{
		// Check if the player has visited all nodes and returned to the start
		if (visitedNodes.Count == graphData.nodes.Count && currentNodeId == visitedNodes[0])
		{
			DisplayLevelComplete();
		}
	}

	private void AdjustWeightForSpecialNodes(int targetNodeId, int currentOrder)
	{
		NodeData targetNode = graphData.nodes.Find(n => n.nodeId == targetNodeId);

		if (targetNode != null)
		{
			switch (targetNode.nodeType)
			{
				case NodeType.BlueSpecial: // Blue Node
					if (currentOrder == 2)
					{
						Debug.Log($"Blue Special Node reached at order {currentOrder}. Reducing total weight by 2.");
						totalWeightUsed = Mathf.Max(totalWeightUsed - 2, 0); // Ensure total weight is non-negative
					}
					break;

				case NodeType.GreenSpecial: // Green Node
					if (currentOrder == 4)
					{
						Debug.Log($"Green Special Node reached at order {currentOrder}. Reducing total weight by 4.");
						totalWeightUsed = Mathf.Max(totalWeightUsed - 4, 0);
					}
					break;

				case NodeType.OrangeSpecial: // Orange Node
					if (currentOrder == 5)
					{
						Debug.Log($"Orange Special Node reached at order {currentOrder}. Reducing total weight by 4.");
						totalWeightUsed = Mathf.Max(totalWeightUsed - 4, 0);
					}
					break;

				case NodeType.PurpleSpecial: // Purple Node
					if (currentOrder == 6)
					{
						Debug.Log($"Purple Special Node reached at order {currentOrder}. Reducing total weight by 2.");
						totalWeightUsed = Mathf.Max(totalWeightUsed - 2, 0);
					}
					break;
			}
		}
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
			scoreText.text = "Total Distance" + "\n" + totalWeightUsed.ToString();  // Update the score text
		}
	}

	private void DisplayLevelComplete()
	{
		if (!isGameCompleted && levelCompleteWindow != null)
		{
			isGameCompleted = true;
			levelCompleteWindow.SetActive(true);
			finalScoreText.text = "Final Score: " + totalWeightUsed;
			pathText.text = "Path Taken: " + string.Join(" -> ", pathTaken);
			int stars = CalculateStars();
			Debug.Log("Stars Earned: " + stars);
		}
	}

	private void DisplayLevelFailed()
	{
		if (!isGameCompleted && levelFailedWindow != null)
		{
			isGameCompleted = true; // Mark the game as completed
			levelFailedWindow.SetActive(true);
			weightUsedText.text = "Weight Used: " + totalWeightUsed;
			weightLimitText.text = "Weight Limit: " + graphData.weightLimit;
		}
	}

	private int CalculateStars()
	{
		if (totalWeightUsed <= graphData.threeStarWeight)
			return 3;
		else if (totalWeightUsed <= graphData.twoStarWeight)
			return 2;
		else if (totalWeightUsed <= graphData.oneStarWeight)
			return 1;
		else
			return 0;
	}
}
