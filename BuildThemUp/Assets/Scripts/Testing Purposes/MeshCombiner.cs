using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    [SerializeField] bool mIsEnabled = true;
    [SerializeField] bool mDestoryUponCompletion = false;

    private MeshFilter mMeshFilter;
    private Material mMaterial;

    void Start()
    {
        if (mIsEnabled == false)
        {
            Log.Info(this, "Combiner was set to be disabled. Destorying combiner.");
            Destroy(this);
            return;
        }

        mMeshFilter = GetComponent<MeshFilter>();
        if (mMeshFilter == null)
        {
            Log.Info(this, "MeshFilter was null. Destorying combiner.");
            Destroy(this);
            return;
        }

        MeshRenderer tMeshRenderer = GetComponent<MeshRenderer>();
        mMaterial = tMeshRenderer.material;
        if (mMaterial == null)
        {
            Log.Info(this, "Material was null. Destorying combiner.");
            Destroy(this);
            return;
        }

        MeshCombineManager.instance.Add(mMaterial, mMeshFilter);

        if (mDestoryUponCompletion == true)
        {
            Destroy(gameObject);
        }
        else
        {
            tMeshRenderer.enabled = false;
        }
    }
}