using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 100f;

    public Transform cameraTransform;

    void Update()
    {
        float moveX = 0f; 
        float moveY = 0f; 
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.D)) moveX = 1f; // moves to the right
        if (Input.GetKey(KeyCode.A)) moveX = -1f; // moves to the left

        if (Input.GetKey(KeyCode.W)) moveZ = 1f;  // moves frontward
        if (Input.GetKey(KeyCode.S)) moveZ = -1f; // moves backward
        
        if (Input.GetKey(KeyCode.Z)) moveY = 1f;   // Moves up
        if (Input.GetKey(KeyCode.X)) moveY = -1f; // Moves down

        Vector3 moveDirection;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        Vector3 cameraUp = cameraTransform.up;

        cameraForward.y = 0f;
        cameraForward.Normalize();
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Moves accordingly to the camera
        moveDirection = (cameraRight * moveX + cameraForward * moveZ + cameraUp * moveY).normalized;

        float rotate = 0f;
        if(Input.GetKey(KeyCode.Q)) rotate = 1f; // rotates to the left
        if(Input.GetKey(KeyCode.E)) rotate = -1f; // rotates to the right

        transform.Rotate(Vector3.up, rotate*rotationSpeed*Time.deltaTime, Space.World);

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public void activate(){

        this.enabled = true;
    }

    public void deactivate(){

        this.enabled = false;
    }
}