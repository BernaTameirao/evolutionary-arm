using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMovement : MonoBehaviour
{
    private Vector3 lastPosition;
    public Vector3 lastInstance;
    public bool isMoving;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
        lastInstance = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        // If the current position isn't the position stored in "lastPosition", it means the object is moving. 
        if(transform.position != lastPosition) {
            
            // Saves the initial position before moving as "lastInstance".
            if(!isMoving){
                lastInstance = lastPosition;
            }

            isMoving = true;
        } else {
            isMoving = false;
        }

        // Updates "lastPosition"
        lastPosition = transform.position;
    }

    /// <summary>
    /// Returns the object's last position before moving.
    public Vector3 getLastPosition(){

        return lastInstance;
    }
}
