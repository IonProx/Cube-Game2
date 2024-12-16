using UnityEngine;

public class GridManager : MonoBehaviour {
    public GameObject tilePrefab; // Prefab for the tile
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float tileSize = 1f;

    public Node[,] gridNodes;
    public bool isGridReady = false; // Flag to indicate if the grid is ready

    private void Awake() {
        GenerateGrid(); // Generate the grid in Awake to ensure it's ready for other scripts
    }

    void GenerateGrid() {
        gridNodes = new Node[gridWidth, gridHeight]; // Initialize the grid node array

        // Loop through the grid and create the tiles
        for (int x = 0; x < gridWidth; x++) {
            for (int z = 0; z < gridHeight; z++) {
                // Calculate the world position of the current tile
                Vector3 worldPosition = new Vector3(x * tileSize, 0, z * tileSize);

                // Instantiate the tile at the correct position
                GameObject tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                tile.transform.parent = transform; // Make the tile a child of this GameObject

                // Create a node for each tile
                Node node = new Node(x, z, true, worldPosition);
                gridNodes[x, z] = node;

                // Set up additional logic for tile properties (like walkability)
                TileInfo tileInfo = tile.GetComponent<TileInfo>();
                if (tileInfo != null) {
                    tileInfo.SetPosition(x, z); // Set the tile's position
                    tileInfo.SetNode(node);    // Link the tile with the node
                    tileInfo.SetWalkable(true); // Set the tile as walkable
                } else {
                    Debug.LogWarning("TileInfo component missing on tile prefab.");
                }
            }
        }

        // Set the grid as ready
        isGridReady = true;

        // Optionally call PlaceObstacles if required by your workflow
        // FindObjectOfType<ObstacleManager>()?.PlaceObstacles(); // If you need it to run here
    }

    // Method to mark a tile as unwalkable (for obstacle placement)
    public void SetNodeWalkability(int x, int z, bool isWalkable) {
        if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight) {
            gridNodes[x, z].isWalkable = isWalkable;
        }
    }

    // Method to get a node from a world position
    public Node GetNodeFromWorldPosition(Vector3 worldPosition) {
        int x = Mathf.FloorToInt(worldPosition.x / tileSize);
        int z = Mathf.FloorToInt(worldPosition.z / tileSize);

        if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight) {
            return gridNodes[x, z];
        }

        Debug.LogWarning($"World position {worldPosition} is out of bounds.");
        return null;
    }

    public Vector3 GetTileWorldCenter(int gridX, int gridZ) {
        return new Vector3(gridX * tileSize + tileSize / 2f, 1f, gridZ * tileSize + tileSize / 2f);
    }

    public class Node {
        public int gridX; // X coordinate of the node in the grid
        public int gridZ; // Z coordinate of the node in the grid
        public bool isWalkable; // Indicates if the node is walkable or blocked
        public Vector3 worldPosition; // World position of the node

        // Pathfinding-specific properties
        public int gCost; // Cost from the start node to this node
        public int hCost; // Heuristic cost (distance to the target node)
        public int fCost { get { return gCost + hCost; } } // Total cost (gCost + hCost)
        public Node parent; // Reference to the parent node (used for path retracing)

        // Constructor for Node
        public Node(int gridX, int gridZ, bool isWalkable, Vector3 worldPosition) {
            this.gridX = gridX;
            this.gridZ = gridZ;
            this.isWalkable = isWalkable;
            this.worldPosition = worldPosition;
        }
    }
}
