using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour {
    public GameObject obstaclePrefab; // Prefab for the obstacle
    public List<Vector2Int> obstaclePositions = new List<Vector2Int>(); // List of custom positions for obstacles

    private HashSet<Vector2Int> blockedPositions = new HashSet<Vector2Int>(); // Store blocked positions
    private GridManager gridManager; // Reference to GridManager

    void Start() {
        // Ensure the GridManager is available
        gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null) {
            Debug.LogError("GridManager is missing! Cannot place obstacles.");
            return;
        }

        // If the grid is not ready, start a coroutine to wait until it is
        if (!gridManager.isGridReady) {
            StartCoroutine(WaitForGridReady());
        } else {
            PlaceObstacles();
        }
    }

    // Coroutine to wait for GridManager to be ready
    private IEnumerator WaitForGridReady() {
        // Wait until the grid is ready
        while (!gridManager.isGridReady) {
            yield return null; // Wait for the next frame
        }

        // Once the grid is ready, call PlaceObstacles
        PlaceObstacles();
    }

    // This method places obstacles at custom positions
    public void PlaceObstacles() {
        blockedPositions.Clear(); // Clear any previous blocked positions

        // Ensure the gridManager is available before proceeding
        if (gridManager == null) {
            Debug.LogError("GridManager is missing! Cannot place obstacles.");
            return;
        }

        // Loop through the list of custom obstacle positions and place obstacles
        foreach (Vector2Int position in obstaclePositions) {
            // Convert grid position to world position
            Vector3 worldPosition = new Vector3(position.x * gridManager.tileSize, 1, position.y * gridManager.tileSize);

            // Ensure the position is valid and walkable
            GridManager.Node node = gridManager.GetNodeFromWorldPosition(worldPosition);

            if (node != null && node.isWalkable) {
                blockedPositions.Add(position); // Add the position to blocked positions

                // Instantiate the obstacle at the correct position
                Instantiate(obstaclePrefab, worldPosition, Quaternion.identity); // Place the obstacle prefab

                // Mark the node as unwalkable in the GridManager
                node.isWalkable = false; // Mark this node as unwalkable
            } else {
                Debug.LogWarning($"Obstacle cannot be placed at {position} because it's not walkable.");
            }
        }
    }

    // This method checks if a specific tile is blocked
    public bool IsTileBlocked(Vector2Int position) {
        return blockedPositions.Contains(position); // Return true if the tile is blocked
    }

    // This method clears all obstacles and restores the grid's walkability
    public void ClearObstacles() {
        foreach (Vector2Int position in blockedPositions) {
            // Convert grid position to world position
            Vector3 worldPosition = new Vector3(position.x * gridManager.tileSize, 1, position.y * gridManager.tileSize);

            // Restore the walkability of the nodes that were previously blocked
            GridManager.Node node = gridManager.GetNodeFromWorldPosition(worldPosition);
            if (node != null) {
                node.isWalkable = true; // Make the node walkable again
            }
        }

        // Clear the obstacle list and reset positions
        blockedPositions.Clear(); // Clear all blocked positions

        // Optionally, destroy any instantiated obstacles
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var obstacle in obstacles) {
            Destroy(obstacle); // Destroys all obstacles tagged with "Obstacle"
        }
    }

    // Optional: Debug method to visualize blocked positions in the scene view
    private void OnDrawGizmos() {
        if (gridManager != null) {
            Gizmos.color = Color.red;
            foreach (Vector2Int position in blockedPositions) {
                Vector3 worldPosition = new Vector3(position.x * gridManager.tileSize, 1, position.y * gridManager.tileSize);
                Gizmos.DrawSphere(worldPosition, 0.5f); // Draw a red sphere at the blocked position
            }
        }
    }
}
