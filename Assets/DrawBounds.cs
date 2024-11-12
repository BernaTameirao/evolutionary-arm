using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBounds : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // Get the Renderer component to access the bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Set the color of the Gizmos
            Gizmos.color = Color.green;

            // Draw a wireframe cube representing the bounds
            Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
        }
    }
}
