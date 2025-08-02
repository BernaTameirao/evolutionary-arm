using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateScene : MonoBehaviour
{
    public GameObject robotBase;
    private Evolution EvScript; //so that it can call the evolution script after everyithing is set up
    private PathFinding PathFind; //so that it can call the pathfinding algorithm after everything is set up
    //private DStarPathFinding dstarScript;
    private Gridi Gridi;
    private RobotGrid RobotGridScript;
    public Vector3 worldSize;
    public GameObject obstaclePrefab; 
    public int N;
    public float distJoints;
    public int numObstacles = 20;

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

        EvScript = GetComponent<Evolution>();//gets the scripts
        PathFind = GetComponent<PathFinding>();
        Gridi = GetComponent<Gridi>();

        //Mudança Fog of War
        RobotGridScript = GetComponent<RobotGrid>();
        //dstarScript = GetComponent<DStarPathFinding>();

        createRandomObstacles(numObstacles);//creates random obstacles, this will eventually be substituted by reading gameobjects from a save file like the 2d version
        PathFind.activate();//activates the pathfind that will eventually activate the evolution script
    }

    public void destroyRobotArm(){

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

    /*public void createRandomObstacles(int n) {
        //this will create random obstacles inside the grid sizes and add them to the evolution osbtacle list
        //for this i need to get a random x, y, z inside the real grid
        float sizeX = worldSize.x/2;
        float sizeY = worldSize.y/2;
        float sizeZ = worldSize.z/2;
    
        for(int i = 0; i < n; i++) {

            float aux1 = Random.value;
            float aux2 = Random.value;

            GameObject newObstacle = Instantiate(obstaclePrefab, new Vector3(aux1 < 0.5f ? Random.Range(-sizeX, -0.3f) : Random.Range(0.3f, sizeX), 
                                                                            Random.Range(0f, sizeY), 
                                                                            aux2 < 0.5f ? Random.Range(-sizeZ, -0.3f) : Random.Range(0.3f, sizeZ)), Quaternion.identity);

            EvScript.addObstacle(newObstacle);
        }

        //GetComponent<HollowTube>().InitializeTube(new Vector3(0, 1.5f, 1.5f), 0.3f, 3);

        Gridi.createGrid();
        //Mudança Fog of War
        RobotGridScript.createGrid();
        //dstarScript.Initialize();

    }*/

    public GameObject GetObstacleFromPool()
    {
        foreach (GameObject obstacle in obstaclePool)
        {
            if (!obstacle.activeInHierarchy)
            {
                obstacle.SetActive(true);
                return obstacle;
            }
        }

        // Se não tiver nenhum disponível, cria um novo
        GameObject newObstacle = Instantiate(obstaclePrefab);
        //newObstacle.layer = 3;
        //newObstacle.AddComponent<CheckMovement>();
        obstaclePool.Add(newObstacle);
        return newObstacle;
    }

    public void createRandomObstacles(int n)
    {
        float sizeX = worldSize.x / 2;
        float sizeY = worldSize.y / 2;
        float sizeZ = worldSize.z / 2;
        
        if(!isCreatingTunnels){
            for (int i = 0; i < n; i++)
            {

                float randomX = Random.Range(-sizeX, sizeX);
                float randomY = Random.Range(0f, sizeY);
                float randomZ = Random.Range(-sizeZ, sizeZ);

                if(randomX > -0.3f && randomX < 0.3f){
                    
                    float aux = Random.value;
                    randomZ = aux < 0.5f ? Random.Range(-sizeZ, -0.3f) : Random.Range(0.3f, sizeZ);
                }

                GameObject obstacle = GetObstacleFromPool();
                obstacle.transform.position = new Vector3(randomX, randomY, randomZ);
                obstacle.transform.rotation = Quaternion.identity;
                EvScript.addObstacle(obstacle);
            }
        } else {

            float aux1 = Random.value;
            float aux2 = Random.value;

            float randomX = aux1 < 0.5f ? Random.Range(-sizeX, -0.5f) : Random.Range(0.5f, sizeX);
            float randomY = Random.Range(1.5f, sizeY);
            float randomZ = aux2 < 0.5f ? Random.Range(-sizeZ, -0.5f) : Random.Range(0.5f, sizeZ);

            GetComponent<HollowTube>().InitializeTube(new Vector3(randomX, randomY, randomZ), 0.3f, 2);
            EvScript.goal.transform.position = new Vector3(randomX, randomY, randomZ+0.9f);
        }

        Physics.SyncTransforms();

        Gridi.createGrid();
        //Mudança Fog of War
        RobotGridScript.createGrid();
    }

    public void spawnObject()
    {
        spawnedObjects = new GameObject[3, N];

        GameObject newJoint = jointPrefab;
        GameObject newSegment = segmentPrefab;
        Vector3 spawnPosition = robotBase.transform.GetChild(0).position; // Base of the robot

        for(int counter=0; counter<N; counter++){
            
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

    public Vector3 GetEdgeCenter(GameObject obj, bool isTop)
    {
        var bounds = obj.GetComponent<MeshRenderer>().bounds;
        return isTop ? bounds.center + obj.transform.up * bounds.extents.y
                    : bounds.center - obj.transform.up * bounds.extents.y;
    }

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