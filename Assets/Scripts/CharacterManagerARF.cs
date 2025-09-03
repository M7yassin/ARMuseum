using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CharacterManagerARF : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterEntry
    {
        public string imageName;
        public GameObject prefab;
        public float scale;       // shrink/enlarge prefab
        public float rightOffset; // offset to the right of the image
    }

    [Header("AR Managers (assign XR Origin components)")]
    public ARTrackedImageManager trackedImageManager;
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    [Header("Placement")]
    public float heightOffset = 0.01f;
    public bool faceCamera = true;
    public Camera arCamera;

    [Header("Characters mapping")]
    public List<CharacterEntry> characterEntries = new();

    private readonly Dictionary<string, GameObject> spawned = new();

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs e)
    {
        foreach (var img in e.added) HandleImage(img);
        foreach (var img in e.updated) HandleImage(img);
        foreach (var img in e.removed) RemoveImage(img);
    }

    private void HandleImage(ARTrackedImage img)
    {
        var name = img.referenceImage.name;

        // Spawn prefab if not already
        if (!spawned.TryGetValue(name, out var go))
        {
            var entry = characterEntries.Find(c => c.imageName == name);
            if (entry.prefab == null) return;

            go = Instantiate(entry.prefab, transform);
            if (entry.scale > 0f)
                go.transform.localScale = entry.prefab.transform.localScale * entry.scale;
            spawned[name] = go;
        }

        // Hide if not tracking
        if (img.trackingState != TrackingState.Tracking)
        {
            go.SetActive(false);
            return;
        }

        // Try to place on closest valid ground plane
        var placed = PlaceOnClosestPlane(img, go);
        if (!placed)
        {
            // Fallback: hover above image if no plane is found
            var pos = img.transform.position + Vector3.up * heightOffset;
            var entry = characterEntries.Find(c => c.imageName == name);
            pos += img.transform.right * entry.rightOffset;
            go.transform.position = pos;
        }

        // Rotate to face camera
        if (faceCamera && arCamera != null)
        {
            var dir = arCamera.transform.position - go.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                go.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        go.SetActive(true);
    }

    private bool PlaceOnClosestPlane(ARTrackedImage img, GameObject go)
    {
        if (planeManager == null) return false;

        ARPlane closestPlane = null;
        float closestDist = float.MaxValue;
        Vector3 imgPos = img.transform.position;

        // The PlaneClassifier script has already disabled non-ground planes,
        // so we just need to find the closest one that is still active.
        foreach (var plane in planeManager.trackables)
        {
            // Skip any planes that have been deactivated by our classifier
            if (!plane.gameObject.activeSelf) continue;

            // Distance on XZ between image and plane center
            float dist = Vector2.Distance(
                new Vector2(imgPos.x, imgPos.z),
                new Vector2(plane.center.x, plane.center.z)
            );

            if (dist < closestDist)
            {
                closestDist = dist;
                closestPlane = plane;
            }
        }

        if (closestPlane != null)
        {
            var entry = characterEntries.Find(c => c.imageName == img.referenceImage.name);
            Vector3 pos = new Vector3(
                imgPos.x,
                closestPlane.transform.position.y + heightOffset,
                imgPos.z
            ) + img.transform.right * entry.rightOffset;

            go.transform.position = pos;
            return true;
        }

        return false;
    }

    private void RemoveImage(ARTrackedImage img)
    {
        var name = img.referenceImage.name;
        if (spawned.TryGetValue(name, out var go))
        {
            // Instead of just deactivating, you might want to destroy and remove from dictionary
            // to allow re-detection later. For now, deactivating is fine.
            go.SetActive(false);
        }
    }
}