using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateScene : MonoBehaviour
{
    private Evolution EvScript; //so that it can call the evolution script after everyithing is set up
    private PathFinding PathFind; //so that it can call the pathfinding algorithm after everything is set up
    private Gridi Gridi;
    private RobotGrid RobotGridScript;
    
    public Vector3 worldSize;
    public int N;
    public float distJoints;
    public int numObstacles = 20;

    public GameObject robotBase;
    public GameObject obstaclePrefab; 
    public GameObject segmentPrefab;   // Prefab to spawn
    public GameObject jointPrefab;
    public GameObject endPrefab;
    private GameObject lastSpawnedObject;  // Keep track of the last spawned object
    public GameObject[,] spawnedObjects;

    private List<GameObject> obstaclePool = new List<GameObject>();
    public bool isCreatingTunnels = false;
 
    private void Awake()
    {   
        //Declaring of arrays
        distJoints = segmentPrefab.GetComponent<Collider>().bounds.size.y;

        // Start spawning objects
        spawnObject();
    }

    void Start(){
        //gets the scripts
        EvScript = GetComponent<Evolution>();
        PathFind = GetComponent<PathFinding>();
        Gridi = GetComponent<Gridi>();
        RobotGridScript = GetComponent<RobotGrid>();

        createRandomObstacles(numObstacles, !isCreatingTunnels);//creates random obstacles
        PathFind.activate();//activates the pathfind that will eventually activate the evolution script
    }

    /// <summary>
    /// Destroy the objects that compose the robotic arm.
    public void destroyRobotArm(){
        
        // Loops through the array that stores the game objects and destroys them.
        for(int counter=0; counter<2; counter++){
            for(int counter2=0; counter2<N; counter2++){

                if (spawnedObjects[counter, counter2] != null)
                {
                    Destroy(spawnedObjects[counter, counter2]);
                }
            }
        }

        Destroy(spawnedObjects[2, 0]);
    }

    /// <summary>
    /// Gets an object to be used as an obstacle.
    /// <returns>
    /// obstacle (GameObject)
    public GameObject GetObstacleFromPool()
    {
        // Loops through the obstaclePool list, and if there is an obstacle avaliable, set it to active.
        foreach (GameObject obstacle in obstaclePool)
        {
            if (!obstacle.activeInHierarchy)
            {
                obstacle.SetActive(true);
                return obstacle;
            }
        }

        // If there is no obstacle avaliable to be used, creates a new one.
        GameObject newObstacle = Instantiate(obstaclePrefab);

        // Adds it to the list.
        obstaclePool.Add(newObstacle);
        return newObstacle;
    }

    /// <summary>
    /// Creates randomly placed obstacles.
    /// <parameters>
    /// n (int): Number of obstacles to be created.
    /// mode (bool): Bool that defines if the obstacles will be random, or in the shape of a tunnel.
    public void createRandomObstacles(int n, bool mode)
    {
        // Gets the grid bounds
        float sizeX = worldSize.x / 2;
        float sizeY = worldSize.y / 2;
        float sizeZ = worldSize.z / 2;
        
        // If mode is True, randomly placed cubes will be created.
        if(mode){
            for (int i = 0; i < n; i++)
            {
                
                // Gets a random position for the obstacle.
                float randomX = Random.Range(-sizeX, sizeX);
                float randomY = Random.Range(0f, sizeY);
                float randomZ = Random.Range(-sizeZ, sizeZ);

                // Constraint that prevents the obstacle from spawning inside the robotic arm position.
                if(randomX > -0.3f && randomX < 0.3f){
                    
                    float aux = Random.value;
                    randomZ = aux < 0.5f ? Random.Range(-sizeZ, -0.3f) : Random.Range(0.3f, sizeZ);
                }

                // Gets the obstacle from the obstacle pool list, and assigns the position to it.
                GameObject obstacle = GetObstacleFromPool();
                obstacle.transform.position = new Vector3(randomX, randomY, randomZ);
                obstacle.transform.rotation = Quaternion.identity;
                EvScript.addObstacle(obstacle);
            }

        // If mode is False, a randomly placed tunnel will be created.
        } else {

            float aux1 = Random.value;
            float aux2 = Random.value;

            // Gets a random position for the tunnel.
            float randomX = aux1 < 0.5f ? Random.Range(-sizeX, -0.5f) : Random.Range(0.5f, sizeX);
            float randomY = Random.Range(1.5f, sizeY);
            float randomZ = aux2 < 0.5f ? Random.Range(-sizeZ, -0.5f) : Random.Range(0.5f, sizeZ);

            // Creates the tunnel.
            GetComponent<HollowTube>().BuildTubeSystem(new Vector3(randomX, randomY, randomZ), 0.5f, 2);
        }

        Physics.SyncTransforms();

        // Creates both grids.
        Gridi.createGrid();
        RobotGridScript.createGrid();
    }

    /// <summary>
    /// Spawns the GameObjects that will compose the robotic arm.
    public void spawnObject()
    {
        spawnedObjects = new GameObject[3, N];

        // Get the prefab of the objects
        GameObject newJoint = jointPrefab;
        GameObject newSegment = segmentPrefab;
        
        // Uses the base position as the first spawn position
        Vector3 spawnPosition = robotBase.transform.GetChild(0).position; // Base of the robot

        for(int counter=0; counter<N; counter++){
            
            // Get the position that the object should be
            spawnPosition += Vector3.up * distJoints;

            // Instantiate the new objects
            newJoint = Instantiate(jointPrefab, spawnPosition, Quaternion.identity);
            newSegment  = Instantiate(segmentPrefab, newJoint.transform.position, Quaternion.identity);
            newJoint.transform.position = GetEdgeCenter(newSegment, true);
            newJoint.transform.parent = newSegment.transform;

            //Save the references of the objects spawned
            spawnedObjects[0, counter] = newSegment;
            spawnedObjects[1, counter] = newJoint;
        }

        //Get the vertices of the last segment and spawns the sphere in their position.
        GameObject endSphere =  Instantiate(endPrefab, spawnPosition, Quaternion.identity);

        //Save the reference of the sphere.
        spawnedObjects[2, 0] = endSphere;
    }

    /// <summary>
    /// Gets the bounds' top or bottom face center point position.
    /// <parameters>
    /// obj (GameObject): The object the function will obtain its center point.
    /// isTop (bool): bool that defines if the center point will be from the top face or bottom face.
    /// <returns>
    /// The position of the center point (Vector3)
    public Vector3 GetEdgeCenter(GameObject obj, bool isTop)
    {
        var bounds = obj.GetComponent<MeshRenderer>().bounds;
        return isTop ? bounds.center + obj.transform.up * bounds.extents.y
                    : bounds.center - obj.transform.up * bounds.extents.y;
    }

    /// <summary>
    /// Makes a copy of the values of an array.
    /// <parameters>
    /// originalArray (List<Vector3>): Array that will be copied
    /// <returns>
    /// arrayCopy (List<Vector3>): Copy of the original array
    public List<Vector3> DeepCopy(List<Vector3> originalArray)
    {

        List<Vector3> arrayCopy = new List<Vector3>();
        // Deep copy: copy the content (each Vector3) instead of the reference
        for (int i = 0; i < originalArray.Count; i++)
        {
            arrayCopy.Add(originalArray[i]);  // Copy each Vector3 by value
        }

        return arrayCopy;
    }
}