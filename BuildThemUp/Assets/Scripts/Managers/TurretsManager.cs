using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretsManager : MonoBehaviour
{
    public static TurretsManager instance { get; private set; }
    
    [SerializeField] private Turret[] mTurretPrefabs;
    private Dictionary<string, Turret> mNameToTurret;
    
    public List<Turret> allTurrets;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
        }
        
        mNameToTurret = new Dictionary<string, Turret>();
        foreach (var i in mTurretPrefabs)
        {
            if (!mNameToTurret.TryAdd(i.name.ToLowerInvariant(), i))
            {
                Log.Warning(this, "Duplicate turret name: " + i.name);
            }
        }
    }

    void Start()
    {
        GetAllTurrets();
    }

    void GetAllTurrets() //Testing only. Incase there are already turrets placed.
    {
        allTurrets = FindObjectsByType<Turret>(FindObjectsSortMode.None).ToList();
    }

    public Turret GetTurretByName(string aName)
    {
        if (mNameToTurret.TryGetValue(aName.ToLowerInvariant(), out var turret))
            return turret;

        Log.Error(this, "Tried to get turret that doesn't exist: " + aName);
        return null;
    }

    public Turret SpawnTurret(string aName, Vector3 aPosition, Quaternion aRotation)
    {
        var tName = aName.ToLowerInvariant();
        
        if (!mNameToTurret.TryGetValue(tName, out var tTurretPrefab))
        {
            Log.Error(this,"Tried to get turret with name " + aName + " that doesn't exist!");
            return null;
        }

        var tTurret = Instantiate(tTurretPrefab, aPosition, aRotation);
        tTurret.transform.parent = transform;
        
        allTurrets.Add(tTurret);

        return tTurret;
    }
    
    public Turret SpawnTurret(Turret aTurretPrefab, Vector3 aPosition, Quaternion aRotation)
    {
        var tTurret = Instantiate(aTurretPrefab, aPosition, aRotation);
        tTurret.transform.parent = transform;
        
        allTurrets.Add(tTurret);

        return tTurret;
    }

    public bool DeleteTurret(Turret aTurret)
    {
        Log.Info(this, $"Attempting to delete the turret {aTurret.name}");
        bool tOutcome = allTurrets.Remove(aTurret);
        if (tOutcome == false)
        {
            Log.Warning(this, $"{aTurret.name} does not exist in the manager's list of all turrets!");
            return false;
        }

        Destroy(aTurret.gameObject);
        Log.Info(this, $"Destroying the turret {aTurret.name}");
        return true;
    }
}
