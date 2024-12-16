using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    public float moveSpeed = 3f; // Speed at which the enemy moves
    public float stopDistance = 0.1f; // Distance at which the enemy stops moving
    private bool isMoving = false; // Track if the enemy is moving
    private Vector3 targetPosition; // Position to move the enemy to
    private Vector2Int previousPlayerGridPosition; // To store previous player's grid position

    public Pathfinding pathfinding; // Reference to the Pathfinding component
    public ObstacleData obstacleData; // Reference to the ObstacleData component

    private PlayerController playerController; // Reference to the player

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

        playerController = FindObjectOfType<PlayerController>(); // Getting the player controller reference
        targetPosition = transform.position; // Initial target position is the enemy's starting position
        previousPlayerGridPosition = playerController.GetGridPosition(); // Store player's starting grid position
    }

    void Update() {
        if (playerController != null) {
            FollowPlayerMovement();
        }
    }

    // Move the enemy to the same tiles the player moves
    private void FollowPlayerMovement() {
        Vector2Int currentPlayerGridPosition = playerController.GetGridPosition();

        // Only move if the player has moved to a new grid position
        if (currentPlayerGridPosition != previousPlayerGridPosition) {
            // Set target position to the player's new position
            targetPosition = new Vector3(
                currentPlayerGridPosition.x * pathfinding.gridManager.tileSize,
                transform.position.y, // Keep the Y position constant
                currentPlayerGridPosition.y * pathfinding.gridManager.tileSize
            );

            // Start moving the enemy towards the new target position
            StartCoroutine(MoveEnemyToTarget(targetPosition));

            // Update the previous player grid position
            previousPlayerGridPosition = currentPlayerGridPosition;
        }
    }

    private IEnumerator MoveEnemyToTarget(Vector3 targetPosition) {
        while (Vector3.Distance(transform.position, targetPosition) > stopDistance) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;  // Wait until the next frame
        }

        // Ensure the enemy reaches the target position
        transform.position = targetPosition;
        isMoving = false; // Stop moving when the target is reached
    }
}
