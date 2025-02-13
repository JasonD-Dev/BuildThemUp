
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private WorldParticle[] mWorldParticles;
    private Dictionary<string, WorldParticle> mNameToWorldParticle;
    
    public static ParticleManager Instance;
    
    private Dictionary<int, Queue<WorldParticle>> mParticlePool;

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
        
        mParticlePool = new Dictionary<int, Queue<WorldParticle>>();
        
        mNameToWorldParticle = new Dictionary<string, WorldParticle>();
        for (int i = 0; i < mWorldParticles.Length; i++)
        {
            var particle = mWorldParticles[i];
            particle.SetPoolingId(particle.GetInstanceID());
            if (!mNameToWorldParticle.TryAdd(particle.name.ToLowerInvariant(), particle))
                Log.Warning(this, "Duplicate decal material name: " + particle.name);
        }
    }
    
    public void AddToPool(WorldParticle aParticle)
    {
        aParticle.OnPool();
        
        Queue<WorldParticle> tQueue;
        if (!mParticlePool.TryGetValue(aParticle.PoolingId, out tQueue))
        {
            tQueue = new Queue<WorldParticle>();
            mParticlePool.Add(aParticle.PoolingId, tQueue);
        }
        tQueue.Enqueue(aParticle);
    }
    
    public WorldParticle GetFromPool(Vector3 aPosition, Vector3 aRotation, string aParticleName)
    {
        var tParticleName = aParticleName.ToLowerInvariant();
        var tParticlePrefab = GetWorldParticle(tParticleName);
        var tPoolingId = tParticlePrefab.GetInstanceID();
        
        WorldParticle tInstance;
        if (!mParticlePool.TryGetValue(tPoolingId, out var tQueue)
            || !tQueue.TryDequeue(out tInstance))
            tInstance = Instantiate(tParticlePrefab, transform);

        tInstance.SetPoolingId(tPoolingId);
        tInstance.Initialise(aPosition, aRotation);
        return tInstance;
    }
    
    public WorldParticle GetWorldParticle(string aName)
    {
        var tName = aName.ToLowerInvariant();
        
        if (!mNameToWorldParticle.TryGetValue(tName, out var tWorldParticle))
        {
            Log.Error(this,"Tried to get world particle with name " + aName + " that doesn't exist!");
            return null;
        }

        return tWorldParticle;
    }
}