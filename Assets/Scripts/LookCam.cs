using UnityEngine;

/// <summary>
/// Makes the object this script is attached to always face the camera.
/// </summary>
public class LookCam : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        // Find the main camera in the scene.
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // Face the camera's position.
            transform.LookAt(mainCamera);
        }
    }
}

