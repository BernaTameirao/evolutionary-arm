//This code is responsible for creating the three-dimensional grid and for rendering it

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gridi : MonoBehaviour
{
    public LayerMask unwalkableMask;    //layer designated for obstacles
    public float nodeRadius;
    public int layers; //how many layers of distance to consider when looking at obstacles, it means that if the layers is set to 3, there will be a 3 node distance from obstacles until it is not anymore calculated

    public List<Node> path;
    private Vector3 gridWorldSize;
    public float nodeDiameter;
    public int gridSizeX, gridSizeY, gridSizeZ;

    Node[,,] grid;
    private Material gridMaterial;
    private bool enabled = true;

    private void Awake()    //takes the values ​​set in unity and passes the values ​​to private variables

    {
        gridWorldSize = GetComponent<CreateScene>().worldSize;
        nodeDiameter = nodeRadius * 2;

        // Calculates the grid size in nodes.
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
    }

    private void Start()
    {
        gridMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    /// <summary>
    /// Creates the main grid that will record the position of obstacles.
    public void createGrid()   
    {   
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
        
        // worldBottomLeft is in one of the corners of the grid
        // and will be the reference to get the position of objects within the grid
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2 - Vector3.up * gridWorldSize.y / 2;

       // Iterates through a loop that goes through each node position of the grid, and creates its node. 
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++) 
                {
                    // Finds the position of the node in the world.
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                    
                    // Checks if the world position has any obstacle.
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                    grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                }
            }
        }

        // with the grid created, the layers will be added
        for(int i = layers; i > 0; i--) {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    for (int z = 0; z < gridSizeZ; z++) 
                    {
                        if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                            List <Node> nrs = GetNeighbours(grid[x, y, z], true);
                            foreach(Node element in nrs) {
                                if(element.layer == 0) {
                                    element.layer = i;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the Influence Area, that is the area that an object can affect its nodes layers.
    /// <parameters>
    /// obj (GameObject): The main object that will have its influence area calculated.
    /// extra (int): The extra nodes that will compose the area. (usually is the number of layers)
    /// <returns>
    /// The size of the area (Vector3)
    private Vector3 GetInfluenceArea(GameObject obj, int extra){

        Vector3 objectSize = obj.GetComponent<Renderer>().bounds.size;

        int localSizeX = Mathf.RoundToInt((objectSize.x)/nodeDiameter) + extra;
        int localSizeY = Mathf.RoundToInt((objectSize.y)/nodeDiameter) + extra;
        int localSizeZ = Mathf.RoundToInt((objectSize.z)/nodeDiameter) + extra;

        return new Vector3(
            localSizeX,
            localSizeY,
            localSizeZ
        );
    }

    /// <summary>
    /// Remakes the influence area of an object in the grid. It prevents the software from slowing down, trying to remake the whole grid.
    /// <parameters>
    /// obj (GameObject): The object that will have its area remade.
    public void RecreateGrid(GameObject obj) {

        Vector3 influenceArea = GetInfluenceArea(obj, layers);

        // Gets the object position before moving.
        Vector3 lastPosition = obj.GetComponent<CheckMovement>().getLastPosition();
        Node lastPositionNode = NodeFromWorldPoint(lastPosition);

        // Gets the area boundaries for this position
        int[] auxX = new int[2]{
            Mathf.RoundToInt(lastPositionNode.gridX - influenceArea.x/2),
            Mathf.RoundToInt(lastPositionNode.gridX + influenceArea.x/2)
        };
        int[] auxY = new int[2]{
            Mathf.RoundToInt(lastPositionNode.gridY - influenceArea.y/2),
            Mathf.RoundToInt(lastPositionNode.gridY + influenceArea.y/2)
        };
        int[] auxZ = new int[2]{
            Mathf.RoundToInt(lastPositionNode.gridZ - influenceArea.z/2),
            Mathf.RoundToInt(lastPositionNode.gridZ + influenceArea.z/2)
        };

        // Gets the world corner, to serve as a reference.
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2 - Vector3.up * gridWorldSize.y / 2;

        // Iterates through the demarcated area, and remakes it.
        for (int x = auxX[0]; x < auxX[1]; x++)
        {
            for (int y = auxY[0]; y < auxY[1]; y++)
            {
                for (int z = auxZ[0]; z < auxZ[1]; z++) 
                {   
                    if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                        // Finds the position of the node in the world
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                        
                        // Checks if the world position has any obstacle.
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                        grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                    }
                }
            }
        }

        // with the grid created, the layers will be added
        for(int i = layers; i > 0; i--) {
            for (int x = auxX[0]; x < auxX[1]; x++)
            {
                for (int y = auxY[0]; y < auxY[1]; y++)
                {
                    for (int z = auxZ[0]; z < auxZ[1]; z++) 
                    {
                        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                            if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                                grid[x, y, z].neighbours = GetNeighbours(grid[x, y, z], true);
                                foreach(Node element in grid[x, y, z].neighbours) {
                                    if(element.layer == 0) {
                                        element.layer = i;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Gets the object current position
        Vector3 currentPosition = obj.transform.position;
        Node currentPositionNode = NodeFromWorldPoint(currentPosition);

        // Gets the area boundaries for this position
        auxX = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridX - influenceArea.x/2),
            Mathf.RoundToInt(currentPositionNode.gridX + influenceArea.x/2)
        };
        auxY = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridY - influenceArea.y/2),
            Mathf.RoundToInt(currentPositionNode.gridY + influenceArea.y/2)
        };
        auxZ = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridZ - influenceArea.z/2),
            Mathf.RoundToInt(currentPositionNode.gridZ + influenceArea.z/2)
        };

        // Iterates through the demarcated area, and remakes it.
        for (int x = auxX[0]; x < auxX[1]; x++)
        {
            for (int y = auxY[0]; y < auxY[1]; y++)
            {
                for (int z = auxZ[0]; z < auxZ[1]; z++) 
                {   
                    if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                        // Finds the position of the node in the world
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                        
                        // Checks if the world position has any obstacle.
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                        
                        grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                    }
                }
            }
        }

        // with the grid created, the layers will be added
        for(int i = layers; i > 0; i--) {
            for (int x = auxX[0]; x < auxX[1]; x++)
            {
                for (int y = auxY[0]; y < auxY[1]; y++)
                {
                    for (int z = auxZ[0]; z < auxZ[1]; z++) 
                    {
                        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                            if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                                grid[x, y, z].neighbours = GetNeighbours(grid[x, y, z], true);
                                foreach(Node element in grid[x, y, z].neighbours) {
                                    if(element.layer == 0) {
                                        element.layer = i;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get neighbours from a node.
    /// <parameters>
    /// node (Node): The node which will have its neighbours identified.
    /// getObstacles (bool): Bool that defines if the obstacles (unwalkable nodes) will be considered neighbours or not.
    /// <returns>
    /// neighbours (List<Node>): The list of neighbours from that node.
    public List<Node> GetNeighbours(Node node, bool getObstacles)
    {
        List<Node> neighbours = new List<Node>();

        // Iterates through a loop that goes through the neighbours of a specifc node.
        for(int x = -1; x<=1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // If it is the main node (not a neighbout), ignore.
                    if (x == 0 && y == 0 && z == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    // If the node is within the grid and pass the condition of getObstacles, it is added to the list.
                    if((checkX >= 0 && checkX < gridSizeX && checkY>=0 && checkY< gridSizeY && checkZ >= 0 && checkZ < gridSizeZ) && (grid[checkX, checkY, checkZ].layer < 20 || getObstacles))
                    {
                        neighbours.Add(grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Get the grid node present in a specific position in the world
    /// <parameters>
    /// worldPosition (Vector3): The function will get the node in this position.
    /// <returns>
    /// The node (Node).
    public Node NodeFromWorldPoint(Vector3 worldPosition)   //this method receives a position and locates the node in the grid
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y;
        float percentZ = (worldPosition.z - transform.position.z + gridWorldSize.z / 2) / gridWorldSize.z;

        percentX = Mathf.Clamp01(percentX); //Clamp 01 limits the value between 0 and 1, preventing code breakage.
        percentY = Mathf.Clamp01(percentY);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);
        
        return grid[x, y, z];
    }

    private void OnRenderObject()
    {
        if (path == null || !enabled)
            return;

        // Activate the material
        gridMaterial.SetPass(0);

        GL.Begin(GL.QUADS); // Begin drawing quads
        foreach (Node node in path)
        {

            // Set color based on the node state
            Color color = Color.blue;
            GL.Color(color);

            // Draw the quad for the node
            Vector3 position = node.worldPosition;
            float size = nodeDiameter * 0.9f;
            GL.Vertex(position + new Vector3(-size, 0, -size)); // Bottom-left
            GL.Vertex(position + new Vector3(size, 0, -size));  // Bottom-right
            GL.Vertex(position + new Vector3(size, 0, size));   // Top-right
            GL.Vertex(position + new Vector3(-size, 0, size));  // Top-left
        }
        GL.End(); // End drawing
    }

    /// <summary>
    /// Sets the path created by the path-finding visible or not.
    /// <parameters>
    /// newState (bool): bool that defines if the path will be visible or not.
    public void setEnabled(bool newState){

        enabled = newState;
    }
}