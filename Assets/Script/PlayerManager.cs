using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    private GameObject playerInstance;

    public bool isMoving = false;
    public float moveSpeed = 10f;
    public int totalWeightUsed = 0;
    public int currentOrder = 0;

	public TextMeshProUGUI levelTextWin;
	public TextMeshProUGUI levelTextLose;
	public TextMeshProUGUI scoreText;
    public GameObject levelCompleteWindow;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI pathText;
    public GameObject levelFailedWindow;
    public TextMeshProUGUI weightUsedText;
    public TextMeshProUGUI weightLimitText;
	public GameObject oneStarPic;
	public GameObject twoStarPic;
	public GameObject threeStarPic;

	private GraphData graphData;
    private List<int> visitedNodes = new List<int>();
    private List<int> pathTaken = new List<int>();

    private bool isGameCompleted = false;

	[Header("Energy Bar Manager")]
	public EnergyBarManager energyBarManager;

	void Awake()
    {
		graphData = FindObjectOfType<LevelManager>().graphData;
    }

    void Start()
    {
		Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		string sceneName = scene.name;
		levelTextWin.text = sceneName;
		levelTextLose.text = sceneName;

		UpdateScoreText();

		if (energyBarManager != null)
		{
			energyBarManager.InitializeEnergyBar(graphData.weightLimit);
		}

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
        float xDiff = startPosition.x - targetPosition.x;

		PlayerAnimManager playerAnimManager = FindObjectOfType<PlayerAnimManager>();
		if (playerAnimManager != null)
		{
            playerAnimManager.audioSource.Play();
			playerAnimManager.animator.SetBool("isMoving", true);
            if (xDiff > 0)
            {
                playerAnimManager.animator.SetBool("moveR", false);
            }
            else
            {
                playerAnimManager.animator.SetBool("moveR", true);
            }
		}

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
        bool specialNodeActivated = AdjustWeightForSpecialNodes(targetNode.nodeId, currentOrder);
        if (specialNodeActivated)
        {
            targetNode.audioSource.clip = targetNode.specialSound;
        }
        UpdateScoreText();
        Debug.Log("Player score: " + totalWeightUsed);

		if (energyBarManager != null)
		{
			energyBarManager.UpdateEnergyBar(totalWeightUsed);
		}

		if (!visitedNodes.Contains(targetNode.nodeId))
        {
            visitedNodes.Add(targetNode.nodeId);
        }

        pathTaken.Add(targetNode.nodeId);

        if (!isGameCompleted)
        {
            if (totalWeightUsed > graphData.weightLimit)
            {
				AudioSource audioSource = FindObjectOfType<BGM>().GetComponent<AudioSource>();
				audioSource.Stop();
				DisplayLevelFailed();
            }
            else
            {
                CheckWinCondition(targetNode.nodeId);
            }
        }

        isMoving = false;
        if (playerAnimManager != null)
		{
			playerAnimManager.audioSource.Stop();
			playerAnimManager.animator.SetBool("isMoving", false);
        }

		LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
		{
			targetNode.PlaySound();
			levelManager.CheckAndRevertSpecialNodes(currentOrder);
            levelManager.MarkNodeAsVisited(targetNode.nodeId);
		}
    }

    private void CheckWinCondition(int currentNodeId)
    {
        if (visitedNodes.Count == graphData.nodes.Count && currentNodeId == visitedNodes[0])
        {
			AudioSource bgmSource = FindObjectOfType<BGM>().GetComponent<AudioSource>();
			bgmSource.Stop();
			DisplayLevelComplete();
        }
    }

    private bool AdjustWeightForSpecialNodes(int targetNodeId, int currentOrder)
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
                        return true;
                    }
                    break;

                case NodeType.GreenSpecial: // Green Node
                    if (currentOrder == 4)
                    {
                        Debug.Log($"Green Special Node reached at order {currentOrder}. Reducing total weight by 4.");
                        totalWeightUsed = Mathf.Max(totalWeightUsed - 4, 0);
						return true;
					}
                    break;

                case NodeType.OrangeSpecial: // Orange Node
                    if (currentOrder == 5)
                    {
                        Debug.Log($"Orange Special Node reached at order {currentOrder}. Reducing total weight by 4.");
                        totalWeightUsed = Mathf.Max(totalWeightUsed - 4, 0);
						return true;
					}
                    break;

                case NodeType.PurpleSpecial: // Purple Node
                    if (currentOrder == 6)
                    {
                        Debug.Log($"Purple Special Node reached at order {currentOrder}. Reducing total weight by 2.");
                        totalWeightUsed = Mathf.Max(totalWeightUsed - 2, 0);
						return true;
					}
                    break;
            }
            return false;
        }
        return false;
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
            scoreText.text = totalWeightUsed.ToString();
        }
    }

    private void DisplayLevelComplete()
    {
        if (!isGameCompleted && levelCompleteWindow != null)
        {
			if (totalWeightUsed >= graphData.weightLimit)
			{
				DisplayLevelFailed();
				return;
			}
			isGameCompleted = true;
			levelCompleteWindow.SetActive(true);
            finalScoreText.text = totalWeightUsed.ToString();
            pathText.text = "Path Taken: " + string.Join(" -> ", pathTaken);
            int stars = CalculateStars();
			switch (stars)
            {
                case 1:
                    oneStarPic.SetActive(true);
					break;
				case 2:
					twoStarPic.SetActive(true);
					break;
				case 3:
					threeStarPic.SetActive(true);
					break;
			}
			Debug.Log("Stars Earned: " + stars);
        }
    }

    private void DisplayLevelFailed()
    {
        if (!isGameCompleted && levelFailedWindow != null)
        {
            isGameCompleted = true;
            levelFailedWindow.SetActive(true);
            weightUsedText.text = totalWeightUsed.ToString();
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
