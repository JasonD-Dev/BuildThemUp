using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GPUInstancer : MonoBehaviour
{
    private Mesh mMesh;
    private Material mMaterial;
    private Matrix4x4 mMatrix;

    private Transform mTransform;

    private GPUInstanceData mGPUInstanceData;
    private int mGPUInstanceDataListIndex = -1;

    [SerializeField] bool isEnabled = true;

    private void Awake()
    {
        if (isEnabled == false)
        {
            Log.Info(this, "Instancer was set to be disabled. Destorying instancer.");
            Destroy(this);
            return;
        }

        //Sanity Checks
        MeshFilter tMeshFilter = GetComponent<MeshFilter>();
        if (tMeshFilter.mesh == null)
        {
            Log.Error(this, "The object does not have a mesh assigned to the mesh filter! Aborting.");
            return;
        }
        mMesh = tMeshFilter.mesh;

        Renderer tRenderer = GetComponent<Renderer>();
        if (tRenderer == null)
        {
            Log.Error(this, "No Renderer was found! Aborting");
            return;
        }
        tRenderer.enabled = false;

        mMaterial = tRenderer.material;
        if (mMaterial == null)
        {
            Log.Error(this, "No Material was found! Aborting");
            return;
        }

        mTransform = transform;
        mMatrix = GPUInstanceData.TransformToMatrix(mTransform);
    }

    private void Start()
    {
        if (GPUInstancingManager.instance == null)
        {
            Log.Error(this, "GPU Instancing manager does not exist! Destorying.");
            return;
        }

        mGPUInstanceData = GPUInstancingManager.instance.CreateGPUInstancData(mMesh, mMaterial);
        AddToGPUInstance();
    }

    //TODO dynamically add and remove instances needs to be implemented.

    private void AddToGPUInstance()
    {
        //Log.Info(this, "Adding to GPU instancing.");
        mGPUInstanceDataListIndex = mGPUInstanceData.AddTransform(mMatrix);
    }

    private void RemoveFromGPUInstance()
    {
        //Log.Info(this, "Removing from GPU instancing.");
        bool tOutcome = mGPUInstanceData.RemoveTransform(mGPUInstanceDataListIndex, mMatrix);

        if (tOutcome == true)
        {
            mGPUInstanceDataListIndex = -1;
        }
    }
}
