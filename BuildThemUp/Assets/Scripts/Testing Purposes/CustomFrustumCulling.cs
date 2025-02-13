using UnityEngine;

public class CustomFrustumCulling : MonoBehaviour
{
    [SerializeField] GameObject[] mObjectsToCull;

    Camera mCamera;

    private void Start()
    {
        mCamera = Camera.main;
    }

    void Update()
    {
        if (mCamera == null) return;

        // Get the frustum planes of the camera
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mCamera);

        foreach (GameObject obj in mObjectsToCull)
        {
            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer != null)
            {
                // Check if the object's bounding box is within the frustum
                bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);

                // Activate or deactivate the object based on visibility
                obj.SetActive(isVisible);
            }
        }
    }
}
