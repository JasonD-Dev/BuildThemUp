
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class DecalManager : MonoBehaviour
{
    public static DecalManager Instance;
    
    [SerializeField] private Decal mDecalPrefab;
    [SerializeField] private Material[] mDecalMaterials;
    private Dictionary<string, Material> mNameToDecalMaterial;
    
    private Queue<Decal> mDecalPool;
    
    public int PoolCapacity = 100;

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;

        mDecalPool = new Queue<Decal>(PoolCapacity);
        
        mNameToDecalMaterial = new Dictionary<string, Material>();
        foreach (var material in mDecalMaterials)
        {
            if (!mNameToDecalMaterial.TryAdd(material.name.ToLowerInvariant(), material))
                Log.Warning(this, "Duplicate decal material name: " + material.name);
        }
    }

    public Decal GetDecal(Vector3 aPosition, Vector3 aRotation, Vector3 aSize, string aDecalName, float lifetime = -1)
    {
        Decal tDecal;
        
        if (mDecalPool.Count < PoolCapacity)
            tDecal = Instantiate(mDecalPrefab, transform);
        else
            tDecal = mDecalPool.Dequeue();

        // move pooled decal to the back of the queue
        mDecalPool.Enqueue(tDecal);

        var tMaterial = GetDecalMaterial(aDecalName);
        
        tDecal.Initialise(aPosition, aRotation, aSize, tMaterial, lifetime);
        
        return tDecal;
    }
    
    public Material GetDecalMaterial(string aName)
    {
        var tName = aName.ToLowerInvariant();
        
        if (!mNameToDecalMaterial.TryGetValue(tName, out var tMaterial))
        {
            Log.Error(this,"Tried to get decal material with name " + aName + " that doesn't exist!");
            return null;
        }

        return tMaterial;
    }
}