using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private int fps;

    void Update()
    {
        // Calculate the frame time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void LateUpdate()
    {
        // Convert the frame time to FPS
        fps = (int)Mathf.Ceil(1.0f / deltaTime);
    }

    /// <summary>
    /// Returns the fps
    public int getFPS(){

        return fps;
    }
}
