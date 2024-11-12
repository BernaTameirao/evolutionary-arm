using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateScene : MonoBehaviour
{
    public GameObject robotBase;
    private Evolution EvScript; //so that it can call the evolution script after everyithing is set up
    private PathFinding PathFind; //so that it can call the pathfinding algorithm after everything is set up
    public GameObject obstaclePrefab; 
    public GameObject armPart;
    public GameObject armEnd;
    public GameObject joint;

    public int N;

    // List to store references to each arm segment
    public List<GameObject> armParts = new List<GameObject>();
    public GameObject armEndgo;

    public float distJoints = 0.4f;

    public int numObstacles = 5;

    void Awake(){
    }

    public void activate() {
        EvScript = GetComponent<Evolution>();//gets the scripts
        PathFind = GetComponent<PathFinding>();
        createRobotArm();
    }

    void createRobotArm(){
        GameObject lastArm = robotBase;

        for (int i = 0; i < N; i++)
        {
            // Instantiate the arm part
            GameObject nextArm = Instantiate(armPart, Vector3.zero, Quaternion.identity);
            GameObject currentJoint = Instantiate(joint, Vector3.zero, Quaternion.identity);
            
            // Parent it to the last arm
            nextArm.transform.parent = lastArm.transform;
            currentJoint.transform.parent = lastArm.transform;

            // Set local position to extend along Y-axis
            if(i != 0) {
                nextArm.transform.localPosition = new Vector3(0, distJoints, 0);
                nextArm.transform.localRotation = Quaternion.identity;
                currentJoint.transform.localPosition = new Vector3(0, distJoints, 0);
                currentJoint.transform.localRotation = Quaternion.Euler(i%3!=2?0:90, i%3!=1?0:90, i%3!=0?0:90);
            } else {
                nextArm.transform.localPosition = new Vector3(0, 0, 0);
                nextArm.transform.localRotation = Quaternion.identity;
                currentJoint.transform.localPosition = new Vector3(0, 0, 0);
                currentJoint.transform.localRotation = Quaternion.Euler(i%3!=2?0:90, i%3!=1?0:90, i%3!=0?0:90);
            }
            // Add to the list of arm parts
            armParts.Add(nextArm);

            // Update lastArm to the current arm part
            lastArm = nextArm;
        }

        // Instantiate the arm end and parent it to the last arm segment
        armEndgo = Instantiate(armEnd, Vector3.zero, Quaternion.identity);
        armEndgo.transform.parent = lastArm.transform;
        armEndgo.transform.localPosition = new Vector3(0, distJoints, 0);
        armEndgo.transform.localRotation = Quaternion.identity;
    }
}