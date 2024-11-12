using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GraphData", menuName = "TSP/GraphData")]
public class GraphData : ScriptableObject
{
	public List<NodeData> nodes;  // List of nodes in the graph
	public List<EdgeData> edges;  // List of edges connecting nodes
}
