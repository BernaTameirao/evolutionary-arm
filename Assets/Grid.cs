//This code is responsible for creating the three-dimensional grid and for rendering it

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;    //layer designated for obstacles
    public Vector3 gridWorldSize;   
    public float nodeRadius;
    Node[,,] grid;

    public List<Node> path;

    float nodeDiameter;
    int gridSizeX, gridSizeY, gridSizeZ;

    private void Start()    //takes the values ​​set in unity and passes the values ​​to private variables

    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

        createGrid();
    }

    void createGrid()   //creation of the array that stores each cube of the collision

    {
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];

        // worldBottomLeft is in one of the corners of the grid
        // and will be the reference to get the position of objects within the grid
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++) 
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    grid[x, y, z] = new Node(walkable, worldPoint, x, y, z);
                }
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x<=1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    if(checkX >= 0 && checkX < gridSizeX && checkY>=0 && checkY< gridSizeY && checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        neighbours.Add(grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }

        return neighbours;
    }

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
    private void OnDrawGizmos() //grid drawing, representing collisions in red and free path in white
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));
        if (grid != null)
        {
            
            foreach (Node node in grid)
            {
                // using 0f for transparency on free paths can ensure better visualization
                Color color = (node.walkable) ? new Color(0f, 0f, 0f, 0f) : new Color(0f, 0f, 0f, 0.9f); 
                if(path != null)
                {
                    if (path.Contains(node)) color = Color.blue;
                }
                Gizmos.color = color;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * nodeDiameter * 0.9f);
            }
        }
    }
}