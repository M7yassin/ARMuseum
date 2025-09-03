using UnityEngine;

/// <summary>
/// Rotates the object to face the camera, but only on the Y-axis.
/// This prevents the object from tilting up or down.
/// </summary>
public class RotateWithCam : MonoBehaviour
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
            // Get the direction to the camera, ignoring the Y-axis difference.
            Vector3 lookDirection = mainCamera.position - transform.position;
            lookDirection.y = 0;

            // Only update rotation if the look direction is not zero.
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                // Create a rotation that looks in the horizontal direction of the camera.
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                // Apply the rotation directly.
                transform.rotation = targetRotation;
            }
        }
    }
}
