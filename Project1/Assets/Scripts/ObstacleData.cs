using System.Collections.Generic;
using UnityEngine;

public class ObstacleData : MonoBehaviour {
    public HashSet<Vector2Int> blockedTiles = new HashSet<Vector2Int>(); // Keeps track of blocked tiles
    public List<Vector2Int> obstaclePositions = new List<Vector2Int>(); // List of custom obstacle positions

    // Public property to provide controlled access to blockedTiles
    public HashSet<Vector2Int> BlockedTiles => blockedTiles;

    // Method to mark a tile as blocked or unblocked
    public void SetTileBlocked(Vector2Int position, bool isBlocked) {
        if (isBlocked) {
            blockedTiles.Add(position); // Add the position to the blocked set
        } else {
            blockedTiles.Remove(position); // Remove the position from the blocked set
        }
    }

    // Check if a specific tile is blocked
    public bool IsTileBlocked(Vector2Int position) {
        return blockedTiles.Contains(position);
    }

    // Method to place obstacles at custom positions
    public void PlaceObstacles() {
        foreach (Vector2Int position in obstaclePositions) {
            SetTileBlocked(position, true); // Mark the tile as blocked
        }
    }

    // Optional: Call this method to clear the obstacles
    public void ClearObstacles() {
        blockedTiles.Clear(); // Clear all blocked tiles
    }
}
