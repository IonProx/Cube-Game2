using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour {
    [Header("References")]
    public GridManager gridManager;  // Reference to the grid manager
    private ObstacleManager obstacleManager; // Reference to obstacle manager

    // This is the only Start method now
    void Start() {
        obstacleManager = FindObjectOfType<ObstacleManager>(); // Getting reference to ObstacleManager
        if (gridManager == null || obstacleManager == null) {
            Debug.LogError("Missing required references (GridManager or ObstacleManager).");
            enabled = false; // Disable the script if dependencies are missing
        }
    }

    // Finds the shortest path from the enemy to the player
    public List<GridManager.Node> FindPath(Vector3 startPos, Vector3 targetPos, Vector2Int playerGridPos) {
        var startNode = gridManager.GetNodeFromWorldPosition(startPos);
        var targetNode = gridManager.GetNodeFromWorldPosition(targetPos);

        if (startNode == null || targetNode == null || !targetNode.isWalkable) {
            Debug.LogError("Invalid path or target node is blocked.");
            return null;
        }

        var openSet = new List<GridManager.Node> { startNode };
        var closedSet = new HashSet<GridManager.Node>();

        while (openSet.Count > 0) {
            var currentNode = GetLowestFCostNode(openSet);
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                Debug.Log("Path found.");
                return RetracePath(startNode, currentNode); // Retrace path when target is reached
            }

            foreach (var neighbor in GetNeighbors(currentNode, playerGridPos)) {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor)) {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No path found.");
        return null; // No path found if openSet is exhausted
    }

    // Helper function to get the node with the lowest fCost
    private GridManager.Node GetLowestFCostNode(List<GridManager.Node> nodes) {
        GridManager.Node lowestFCostNode = nodes[0];
        foreach (var node in nodes) {
            if (node.fCost < lowestFCostNode.fCost ||
                (node.fCost == lowestFCostNode.fCost && node.hCost < lowestFCostNode.hCost)) {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    // Retrace the path from the end node to the start node
    private List<GridManager.Node> RetracePath(GridManager.Node startNode, GridManager.Node endNode) {
        var path = new List<GridManager.Node>();
        var currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse(); // Reverse path to start from the beginning
        return path;
    }

    // Get all valid neighbors of the current node, avoiding the player's position
    private List<GridManager.Node> GetNeighbors(GridManager.Node node, Vector2Int playerGridPos) {
        var neighbors = new List<GridManager.Node>();

        // Check all 8 surrounding tiles for neighbors
        for (int x = -1; x <= 1; x++) {
            AddNeighborIfValid(node.gridX + x, node.gridZ, neighbors, playerGridPos);
        }
        for (int z = -1; z <= 1; z++) {
            AddNeighborIfValid(node.gridX, node.gridZ + z, neighbors, playerGridPos);
        }

        return neighbors;
    }

    // Adds a valid neighbor to the list if the position is walkable and not the player's position
    private void AddNeighborIfValid(int x, int z, List<GridManager.Node> neighbors, Vector2Int playerGridPos) {
        if (x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight) {
            // Exclude the player's position and check for obstacles
            if ((x != playerGridPos.x || z != playerGridPos.y) &&
                !obstacleManager.IsTileBlocked(new Vector2Int(x, z))) {
                neighbors.Add(gridManager.gridNodes[x, z]);
            }
        }
    }

    // Calculate Manhattan distance between two nodes
    private int GetDistance(GridManager.Node a, GridManager.Node b) {
        return Mathf.Abs(a.gridX - b.gridX) + Mathf.Abs(a.gridZ - b.gridZ); // Manhattan distance
    }
}
