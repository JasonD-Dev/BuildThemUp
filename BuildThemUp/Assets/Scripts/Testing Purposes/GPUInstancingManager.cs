using System.Collections.Generic;
using UnityEngine;

public class GPUInstancingManager : MonoBehaviour
{
    public static GPUInstancingManager instance;

    public List<GPUInstanceData> mAllInstancingData = new List<GPUInstanceData>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {

    }

    void Update()
    {
        for (int i = 0; i < mAllInstancingData.Count; i++)
        {
            mAllInstancingData[i].Render();
        }
    }

    public GPUInstanceData CreateGPUInstancData(Mesh aMesh, Material aMaterial)
    {
        string tIdenifierName = GPUInstanceData.GenerateName(aMesh, aMaterial);
        for (int i = 0; i < mAllInstancingData.Count; ++i)
        {
            if (mAllInstancingData[i].MatchName(tIdenifierName) == true)
            {
                Log.Info(this, "A GPUInstanceData already exist for this material and mesh combo! Returning the found data.");
                return mAllInstancingData[i];
            }
        }

        GPUInstanceData tData = new GPUInstanceData(aMesh, aMaterial);
        mAllInstancingData.Add(tData);
        return tData;
    }
}
