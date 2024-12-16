using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f; // Movement speed
    private List<GridManager.Node> currentPath;  // The path the player will follow
    private bool isMoving = false; // Track if the player is moving
    private Vector3 targetPosition; // Position to move the player to

    public Pathfinding pathfinding; // Reference to the Pathfinding component
    public ObstacleData obstacleData; // Reference to the ObstacleData component

    public Text warningText; // Reference to the warning UI text
    private float warningTime = 2f; // Time to show the warning message

    public static event System.Action OnPlayerReachDestination; // Event to notify when player reaches destination
    public bool canMove = true; // Flag to control player movement

    void Start() {
        if (pathfinding == null) {
            pathfinding = FindObjectOfType<Pathfinding>();
            if (pathfinding == null) {
                Debug.LogError("Pathfinding component is missing! Please assign it in the Inspector.");
                enabled = false;
                return;
            }
        }

        if (obstacleData == null) {
            obstacleData = FindObjectOfType<ObstacleData>();
            if (obstacleData == null) {
                Debug.LogError("ObstacleData component is missing! Please assign it in the Inspector.");
                enabled = false;
                return;
            }
        }

        targetPosition = transform.position; // Initial target position is the player's starting position

        if (warningText != null) {
            warningText.gameObject.SetActive(false); // Hide warning by default
        } else {
            Debug.LogWarning("Warning Text UI not assigned in the Inspector!");
        }
    }

    void Update() {
        if (canMove) {
            HandleInput();
            MovePlayer();
        }
    }

    // Handle the mouse input for selecting destination
    private void HandleInput() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
                if (tileInfo != null) {
                    Vector3 clickedPosition = hit.collider.transform.position;

                    // Get the target node based on the clicked position
                    GridManager.Node targetNode = pathfinding.gridManager.GetNodeFromWorldPosition(clickedPosition);

                    // Check if the node is walkable before setting the path
                    if (targetNode != null && targetNode.isWalkable && !obstacleData.IsTileBlocked(new Vector2Int(targetNode.gridX, targetNode.gridZ))) {
                        SetPath(clickedPosition);
                    } else {
                        ShowWarning("Target position is not walkable or blocked by obstacle.");
                    }
                }
            }
        }
    }

    // Set the path for the player, avoiding obstacles
    private void SetPath(Vector3 targetPosition) {
        currentPath = pathfinding.FindPath(transform.position, targetPosition);

        if (currentPath != null && currentPath.Count > 0) {
            // Set the first target position in the path
            this.targetPosition = new Vector3(
                currentPath[0].gridX * pathfinding.gridManager.tileSize,
                1f, // Keep Y = 1
                currentPath[0].gridZ * pathfinding.gridManager.tileSize
            );

            isMoving = true;
        } else {
            Debug.LogWarning("No valid path found!");
            ShowWarning("No valid path to target position.");
        }
    }

    // Move the player along the path
    private void MovePlayer() {
        if (isMoving && currentPath != null && currentPath.Count > 0) {
            // Move the player towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if the player has reached the current target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
                currentPath.RemoveAt(0); // Remove the current node from the path

                // If there are more nodes, set the new target position
                if (currentPath.Count > 0) {
                    targetPosition = new Vector3(
                        currentPath[0].gridX * pathfinding.gridManager.tileSize,
                        1f, // Keep Y = 1
                        currentPath[0].gridZ * pathfinding.gridManager.tileSize
                    );
                } else {
                    isMoving = false; // Stop moving when the path is complete
                    Debug.Log("Player reached the destination.");

                    // Notify that the player has reached the destination
                    OnPlayerReachDestination?.Invoke();
                }
            }
        }
    }

    // Display a warning message when an invalid action is attempted
    private void ShowWarning(string message) {
        if (warningText != null) {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            Invoke(nameof(HideWarning), warningTime);
        }
    }

    // Hide the warning message after a set time
    private void HideWarning() {
        if (warningText != null) {
            warningText.gameObject.SetActive(false);
        }
    }

    // Method to get the player's current grid position
    public Vector2Int GetGridPosition() {
        int gridX = Mathf.FloorToInt(transform.position.x / pathfinding.gridManager.tileSize);
        int gridZ = Mathf.FloorToInt(transform.position.z / pathfinding.gridManager.tileSize);
        return new Vector2Int(gridX, gridZ);
    }
}
