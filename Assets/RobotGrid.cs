//This code is responsible for creating the three-dimensional grid that the robot can see and for rendering it

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotGrid : MonoBehaviour
{
    private LayerMask unwalkableMask;
    private float nodeRadius;
    public Node[,,] grid;
    private int layers; //how many layers of distance to consider when looking at obstacles, it means that if the layers is set to 3, there will be a 3 node distance from obstacles until it is not anymore calculated

    public List<Node> path;
    private Vector3 gridWorldSize;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY, gridSizeZ;
    private float segmentLength;
    private int numberSegments;

    private Material gridMaterial;
    private bool enabled = false;

    private void Awake()    //takes the values ​​set in unity and passes the values ​​to private variables

    {
        gridWorldSize = GetComponent<CreateScene>().worldSize;
        segmentLength = GetComponent<CreateScene>().distJoints;
        numberSegments = GetComponent<CreateScene>().N;
        nodeRadius = GetComponent<Gridi>().nodeRadius;
        unwalkableMask = GetComponent<Gridi>().unwalkableMask;
        layers = GetComponent<Gridi>().layers;

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
    }

    private void Start()
    {
        gridMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    /*private bool getRobotLengthInNodes(Vector3 position){

        float robotLength = segmentLength*numberSegments;


        return null;
    }*/


    public void createGrid()   //creation of the array that stores each cube of the collision

    {
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
        Vector3 basePosition = GetComponent<Evolution>().robot.transform.GetChild(0).position;
        
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
                    bool walkable = true;
                    grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, 0);
                }
            }
        }
    }

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

    public void AddObstacleToGrid(GameObject obj){

        Vector3 influenceArea = GetInfluenceArea(obj, layers);
        Vector3 objectSize = obj.GetComponent<Renderer>().bounds.size;

        Vector3 currentPosition = obj.transform.position;
        Node currentPositionNode = NodeFromWorldPoint(currentPosition);

        int[] auxX = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridX - influenceArea.x/2),
            Mathf.RoundToInt(currentPositionNode.gridX + influenceArea.x/2)
        };

        int[] auxY = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridY - influenceArea.y/2),
            Mathf.RoundToInt(currentPositionNode.gridY + influenceArea.y/2)
        };

        int[] auxZ = new int[2]{
            Mathf.RoundToInt(currentPositionNode.gridZ - influenceArea.z/2),
            Mathf.RoundToInt(currentPositionNode.gridZ + influenceArea.z/2)
        };

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = auxX[0]; x < auxX[1]; x++)
        {
            for (int y = auxY[0]; y < auxY[1]; y++)
            {
                for (int z = auxZ[0]; z < auxZ[1]; z++) 
                {   
                    if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                        grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                        //Debug.DrawRay(worldPoint, Vector3.up * 0.1f, Color.red, 100f);
                    }
                }
            }
        }

        for(int i = layers; i > 0; i--) {
            for (int x = auxX[0]; x < auxX[1]; x++)
            {
                for (int y = auxY[0]; y < auxY[1]; y++)
                {
                    for (int z = auxZ[0]; z < auxZ[1]; z++) 
                    {
                        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                            if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                                //List <Node> nrs = GetNeighbours(grid[x, y, z], true);
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


    public void RecreateGrid(GameObject obj) {

        Vector3 influenceArea = GetInfluenceArea(obj, layers);
        Vector3 objectSize = obj.GetComponent<Renderer>().bounds.size;

        Vector3 lastPosition = obj.GetComponent<CheckMovement>().getLastPosition();
        Node lastPositionNode = NodeFromWorldPoint(lastPosition);

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

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.z / 2 - Vector3.up * gridWorldSize.y / 2;
        //Vector3 lastAreaBottomLeft = lastPosition - Vector3.right * objectSize.x / 2 - Vector3.forward * objectSize.z / 2 - Vector3.up * objectSize.y / 2;

        for (int x = auxX[0]; x < auxX[1]; x++)
        {
            for (int y = auxY[0]; y < auxY[1]; y++)
            {
                for (int z = auxZ[0]; z < auxZ[1]; z++) 
                {   
                    if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                        grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                        //Debug.DrawRay(worldPoint, Vector3.up * 0.1f, Color.green, 100f);
                    }
                }
            }
        }

        for(int i = layers; i > 0; i--) {
            for (int x = auxX[0]; x < auxX[1]; x++)
            {
                for (int y = auxY[0]; y < auxY[1]; y++)
                {
                    for (int z = auxZ[0]; z < auxZ[1]; z++) 
                    {
                        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                            if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                                //List <Node> nrs = GetNeighbours(grid[x, y, z], true);
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

        Vector3 currentPosition = obj.transform.position;
        Node currentPositionNode = NodeFromWorldPoint(currentPosition);

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

        //Vector3 currentAreaBottomLeft = currentPosition - Vector3.right * objectSize.x / 2 - Vector3.forward * objectSize.z / 2 - Vector3.up * objectSize.y / 2;

        for (int x = auxX[0]; x < auxX[1]; x++)
        {
            for (int y = auxY[0]; y < auxY[1]; y++)
            {
                for (int z = auxZ[0]; z < auxZ[1]; z++) 
                {   
                    if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                        grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, layers);
                        //Debug.DrawRay(worldPoint, Vector3.up * 0.1f, Color.red, 100f);
                    }
                }
            }
        }

        for(int i = layers; i > 0; i--) {
            for (int x = auxX[0]; x < auxX[1]; x++)
            {
                for (int y = auxY[0]; y < auxY[1]; y++)
                {
                    for (int z = auxZ[0]; z < auxZ[1]; z++) 
                    {
                        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ){
                            if((grid[x, y, z]).layer == i + 1) {//it means it is an obstacle or the previous layer and the neighbours need to be considered
                                //List <Node> nrs = GetNeighbours(grid[x, y, z], true);
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

    public List<Node> GetNeighbours(Node node, bool getObstacles)
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

                    //Checar se tá certo
                    if((checkX >= 0 && checkX < gridSizeX && checkY>=0 && checkY< gridSizeY && checkZ >= 0 && checkZ < gridSizeZ) && (getObstacles || grid[checkX, checkY, checkZ].walkable))
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

    public void setEnabled(bool newState){

        enabled = newState;
    }
}