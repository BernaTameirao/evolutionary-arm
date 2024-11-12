using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCorpo : MonoBehaviour
{
    public GameObject segmentPrefab;   // Prefab to spawn
    public GameObject jointPrefab;
    public GameObject endPrefab;

    public int numSegments;
    public Vector3[] spawnRotation;

    private GameObject lastSpawnedObject;  // Keep track of the last spawned object
    private GameObject[,] spawnedObjects;
    private Vector3[] lastSpawnRotation;

    private void Awake()
    {   
        //Declaring of arrays
        lastSpawnRotation = new Vector3[numSegments];
        spawnedObjects = new GameObject[3, numSegments];

        // Start spawning objects
        SpawnObject();
    }

    private void Update()
    {
        //updateObject();
    }

    public void updateObject(){

        for(int counter=0; counter<numSegments; counter++){

            //If there is no change made, skip this iteraction
            if(lastSpawnRotation[counter] == spawnRotation[counter])
            {

                continue;
            }

            //Apply the change in the rotation
            spawnedObjects[0, counter].transform.rotation =  Quaternion.Euler(spawnRotation[counter]);

            //Alter the other segments according to the change made
            Vector3 vertexPosition;
            for(int counter2=counter+1; counter2<numSegments; counter2++){
                
                //Get the position of the vertices of the top of the current segment, and move along the other objects
                vertexPosition = GetEdgeVertices(spawnedObjects[1, counter2-1], true);
                spawnedObjects[0, counter2].transform.position = vertexPosition;
            }

            //Move the sphere to the top of the last segment
            vertexPosition = GetEdgeVertices(spawnedObjects[1, numSegments-1], true);
            spawnedObjects[2, 0].transform.position = vertexPosition;
        }

        //Turn this spawn rotation in its last version
        lastSpawnRotation = DeepCopy(spawnRotation);
    }

    void SpawnObject()
    {
        GameObject newJoint = jointPrefab;
        GameObject newSegment = segmentPrefab;

        for(int counter=0; counter<numSegments; counter++){
            // Determine spawn position based on the last spawned object
            Vector3 spawnPosition;
            if (lastSpawnedObject == null)
            {
                spawnPosition = transform.position + new Vector3(0, Mathf.Abs(transform.localScale.y)*1, 0);  // Spawn at spawner's position if it's the first object
            }
            else
            {   
                //Get the top vertices of the last object and spawn the new object on their position
                Vector3 verticesPosition = GetEdgeVertices(lastSpawnedObject, true);

                spawnPosition = verticesPosition;
            }

            // Instantiate the new objects
            newJoint = Instantiate(jointPrefab, spawnPosition, Quaternion.identity);
            newSegment  = Instantiate(segmentPrefab, newJoint.transform.position, Quaternion.identity);

            //Turn the segment in a child of the joint
            newSegment.transform.parent = newJoint.transform;

            //Save the position of the objects
            Vector3 trueJointPosition = newJoint.transform.position;
            Vector3 trueSegmentPosition = newSegment.transform.position;
            Vector3 temporaryJointPosition = GetEdgeVertices(newSegment, false);

            //Move the joint, that is in the center of the segment, to its edge. But it carries the segment with it.
            newJoint.transform.position = temporaryJointPosition;

            //Move the segment back to its original position, and now the joint is in its edge.
            newSegment.transform.position = trueSegmentPosition;

            //Move the joint back to its original position, and it carries the segment.
            newJoint.transform.position = trueJointPosition;

            //Save the references of the objects spawned
            lastSpawnedObject = newSegment;
            spawnedObjects[0, counter] = newJoint;
            spawnedObjects[1, counter] = newSegment;
        }

        //Get the vertices of the last segment and spawns the sphere in their position.
        Vector3 endPosition = GetEdgeVertices(newSegment, true);
        GameObject endSphere =  Instantiate(endPrefab, endPosition, Quaternion.identity);

        endSphere.name = "SphereRobot";

        //Save the reference of the sphere.
        spawnedObjects[2, 0] = endSphere;
    }

    Vector3 GetEdgeVertices(GameObject cube, bool isTop)
    {   
        Vector3[] localVertices = cube.GetComponent<MeshFilter>().mesh.vertices;

        Vector3 localVertex1;
        Vector3 localVertex2;

        switch(isTop){

            case true:
                localVertex1 = localVertices[3];
                localVertex2 = localVertices[4];
                break;
            
            case false:
                localVertex1 = localVertices[1];
                localVertex2 = localVertices[6];
                break;
        }

        // Convert local vertices to world space
        Vector3 worldVertex1 = cube.transform.TransformPoint(localVertex1);
        Vector3 worldVertex2 = cube.transform.TransformPoint(localVertex2);

        worldVertex1 = (worldVertex1 + worldVertex2)/2;

        // Return the edge vertices in world space
        return worldVertex1;
    }

    Vector3[] DeepCopy(Vector3[] originalArray)
    {

        Vector3[] arrayCopy = new Vector3[originalArray.Length];

        // Deep copy: copy the content (each Vector3) instead of the reference
        for (int i = 0; i < originalArray.Length; i++)
        {
            arrayCopy[i] = originalArray[i];  // Copy each Vector3 by value
        }

        return arrayCopy;
    }
}