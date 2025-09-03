using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneClassifier : MonoBehaviour
{
    public ARPlaneManager arPlaneManager;

    private void OnEnable()
    {
        if (arPlaneManager == null)
        {
            Debug.LogError("ARPlaneManager not assigned!");
            return;
        }
        arPlaneManager.planesChanged += OnPlanesChanged;
    }

    private void OnDisable()
    {
        if (arPlaneManager != null)
        {
            arPlaneManager.planesChanged -= OnPlanesChanged;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Deactivate planes that aren't suitable ground surfaces
        foreach (var plane in args.added)
        {
            ClassifyPlane(plane);
        }
        foreach (var plane in args.updated)
        {
            ClassifyPlane(plane);
        }
    }

    private void ClassifyPlane(ARPlane plane)
    {
        if (plane == null) return;

        // We only want horizontal planes that are facing up (floors, low tables).
        // Deactivate everything else.
        if (plane.alignment != PlaneAlignment.HorizontalUp || plane.center.y > 1.2f)
        {
            plane.gameObject.SetActive(false);
        }
    }
}