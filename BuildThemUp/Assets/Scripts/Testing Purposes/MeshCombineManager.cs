using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombineManager : MonoBehaviour
{
    public static MeshCombineManager instance;

    [SerializeField] GridMeshCombineData[,] mAllGridsMeshCombineData; //Assigned in start();
    [SerializeField] Transform mGridStart;
    [SerializeField] Transform mGridEnd;
    [SerializeField, Range(1, 8)] int mNumberOfSegments = 4;

    bool mIsAllCombined = false;
    Vector3 mMinPoint; //Assigned in Start();
    Vector3 mMaxPoint; //Assigned in Start();
    Vector2 mGridCellSize; //Assigned in Start();, The length of width of each segment inside the grid.

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

        if (mGridEnd == null ||  mGridStart == null)
        {
            Log.Error(this, "Both, Grid start and grid end need to be provided!");
            Destroy(this);
            return;
        }

        mMinPoint.x = Mathf.Min(mGridStart.position.x, mGridEnd.position.x);
        mMinPoint.y = Mathf.Min(mGridStart.position.z, mGridEnd.position.z);

        mMaxPoint.x = Mathf.Max(mGridStart.position.x, mGridEnd.position.x);
        mMaxPoint.y = Mathf.Max(mGridStart.position.z, mGridEnd.position.z);

        mGridCellSize.x = (mMaxPoint.x - mMinPoint.x) / mNumberOfSegments;
        mGridCellSize.y = (mMaxPoint.y - mMinPoint.y) / mNumberOfSegments;

        mAllGridsMeshCombineData = new GridMeshCombineData[mNumberOfSegments, mNumberOfSegments];
        for (int i = 0; i < mNumberOfSegments; i++)
        {
            for (int j = 0; j < mNumberOfSegments; j++)
            {
                mAllGridsMeshCombineData[i, j] = new GridMeshCombineData();
            }
        }
    }

    void Update()
    {
        if (mIsAllCombined == false)
        {
            mIsAllCombined = true;
            CombineAll();
            Log.Info(this, "All Meshes Combines. Destroying...");
            Destroy(this);
        }
    }

    public void Add(Material aMaterial, MeshFilter aMeshFilter)
    {
        Vector3 tPosition = aMeshFilter.transform.position;

        if (IsPositionOnGrid(tPosition) == false) 
        {
            Log.Warning(this, $"{aMeshFilter.name} is out of bounds of the Mesh Combiner Grind. Aborting...");
            return;
        }

        int tXIndex = (int)((tPosition.x - mMinPoint.x) / mGridCellSize.x);
        int tYIndex = (int)((tPosition.z - mMinPoint.y) / mGridCellSize.y);

        //Log.Info(this, $"{aMeshFilter.name} is in grid cell: [{tXIndex}, {tYIndex}]");

        mAllGridsMeshCombineData[tXIndex, tYIndex].AddToGridCombineInstance(aMaterial, aMeshFilter);
    }

    void CombineAll()
    {
        for (int i = 0; i < mNumberOfSegments; i++)
        {
            for (int j = 0; j < mNumberOfSegments; j++)
            {
                GameObject tParentGameobject = new GameObject($"Grid Coords {i}, {j}");
                mAllGridsMeshCombineData[i, j].CombineAll(tParentGameobject.transform);

                if (tParentGameobject.transform.childCount == 0)
                {
                    GameObject.Destroy(tParentGameobject);
                }
            }
        }
    }

    bool IsPositionOnGrid(Vector3 aPositon)
    {
        if (aPositon.x < mMinPoint.x || aPositon.x > mMaxPoint.x)
        {
            return false;
        }

        if (aPositon.y < mMinPoint.y || aPositon.y > mMaxPoint.y)
        {
            return false;
        }

        return true;
    }
}



[Serializable]
public class GridMeshCombineData
{
    [field: SerializeField] public List<MeshCombineData> gridMeshCombineData = new List<MeshCombineData>();

    public GridMeshCombineData() { }

    public void AddToGridCombineInstance(Material aMaterial, MeshFilter aMeshFilter)
    {

        //Combine Instance
        CombineInstance tCombineInstance = new CombineInstance();
        tCombineInstance.mesh = aMeshFilter.sharedMesh;
        tCombineInstance.transform = aMeshFilter.transform.localToWorldMatrix;

        for (int i = 0; i < gridMeshCombineData.Count; i++)
        {
            if (aMaterial.name == gridMeshCombineData[i].material.name)
            {
                //if material is the same then add it to the existing MeshCombineData with the matching material.
                //Log.Info(typeof(GridMeshCombineData), $"A MeshCombineData was already found with this Material. ({aMaterial.name})");
                gridMeshCombineData[i].AddCombineInstance(tCombineInstance);
                return;
            }
        }

        //If here, then no previous MeshCombineData has the param Material, thus we need to create a new one.
        //Log.Info(typeof(GridMeshCombineData), $"No MeshCombineData was found with this Material ({aMaterial.name}), creating a new one.");
        MeshCombineData tNewMeshCombineData = new MeshCombineData(aMaterial);
        tNewMeshCombineData.AddCombineInstance(tCombineInstance);
        gridMeshCombineData.Add(tNewMeshCombineData);
    }

    public void CombineAll(Transform aParent = null)
    {
        for (int i = 0; i < gridMeshCombineData.Count; i++)
        {
            //Log.Info(typeof(GridMeshCombineData), $"Combining meshes for {gridMeshCombineData[i].material} material.");
            gridMeshCombineData[i].Combine(aParent);
        }
    }
}



[Serializable]
public class MeshCombineData
{
    [field: SerializeField] public Material material { get; private set; }
    public List<List<CombineInstance>> combineInstancesList { get; private set; }

    const int mMaxArrayElements = 1000;

    private int mVacantListIndex;
    private int mVacantListCount;

    public MeshCombineData(Material aMaterial)
    {
        material = aMaterial;
        combineInstancesList = new List<List<CombineInstance>>
        {
            new List<CombineInstance>()
        };

        mVacantListIndex = 0;
        mVacantListCount = 0;
    }

    public void AddCombineInstance(CombineInstance aCombineInstance)
    {
        combineInstancesList[mVacantListIndex].Add(aCombineInstance);
        mVacantListCount++;

        //Adjusting vacant index if list is now full.
        if (mVacantListCount >= mMaxArrayElements)
        {
            mVacantListCount = 0;
            mVacantListIndex = combineInstancesList.Count;

            combineInstancesList.Add(new List<CombineInstance>());
        }
    }

    public void Combine(Transform aParent)
    {
        for (int i = 0; i < combineInstancesList.Count; i++)
        {
            Mesh tCombinedMesh = new Mesh();
            tCombinedMesh.indexFormat = IndexFormat.UInt32;

            for (int j = 0;  j < combineInstancesList[i].Count; j++)
            {
                Mesh tMesh = combineInstancesList[i][j].mesh;
                tMesh.UploadMeshData(false);

                if (tMesh.isReadable == false)
                {
                    Log.Warning($"MeshCombiner[{material.name}]", "The Mesh provided is not readable. Not Combining.");
                    continue;
                }
            }

            tCombinedMesh.CombineMeshes(combineInstancesList[i].ToArray());

            GameObject tCommbinedGameObject = new GameObject(material.name);
            MeshRenderer tMeshRenderer = tCommbinedGameObject.AddComponent<MeshRenderer>();
            MeshFilter tMeshFiler = tCommbinedGameObject.AddComponent<MeshFilter>();

            tMeshFiler.mesh = tCombinedMesh;
            tMeshRenderer.material = material;

            //Log.Info($"MeshCombiner[{material.name}]", $"Mesh combined with a total of {tCombinedMesh.vertexCount} vertices.");

            tCommbinedGameObject.transform.SetParent(aParent, false);
        }
    }
}
