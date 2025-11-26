using UnityEngine;

public class ObjectSelector : MonoBehaviour
{

    private GameObject clickedObject;

    private Color originalColor;
    private GameObject previousObject;

    void Update()
    {
        
        // Check for left mouse button click
        if (Input.GetMouseButtonDown(0)) // 0 = Left Mouse Button
        {
            // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the raycast hit something
                clickedObject = hit.collider.gameObject;

                if(previousObject != null){

                    ResetObjectColor();
                }
                HighlightObject(clickedObject);

                previousObject = clickedObject;
            } else {
                
                clickedObject = null;
                if(previousObject != null){

                    ResetObjectColor();
                }
            }
        } else if(clickedObject != null){
            
            Renderer renderer = clickedObject.GetComponent<Renderer>();
            if(renderer.material.color*2 != originalColor){

                originalColor = renderer.material.color;
                renderer.material.color *= 0.5f;
            }
        }
    }

    /// <summary>
    /// Highlights the object, changing its color
    /// <parameters>
    /// obj (GameObject): The object that will be highlighted
    private void HighlightObject(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Change color to highlight
            originalColor = renderer.material.color;
            renderer.material.color *= 0.5f;
        }
    }

    /// <summary>
    /// Reset the object color to the original
    private void ResetObjectColor()
    {
        Renderer renderer = previousObject.GetComponent<Renderer>();
        if (renderer != null)
        {
    
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// Returns the clicked object.
    public GameObject getClickedObject(){

        return clickedObject;
    }
}

