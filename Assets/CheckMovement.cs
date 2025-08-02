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
        if(transform.position != lastPosition) {

            if(!isMoving){
                lastInstance = lastPosition;
            }

            isMoving = true;
        } else {
            isMoving = false;
        }
        lastPosition = transform.position;
    }

    public Vector3 getLastPosition(){

        return lastInstance;
    }
}
