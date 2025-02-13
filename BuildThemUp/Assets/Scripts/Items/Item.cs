using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public Sprite icon { get; private set; } = null;
    [field: SerializeField] public string Name { get; private set; } = "";
    [field: SerializeField] public string displayName { get; private set; } = "";
    [field: SerializeField] public string description { get; private set; } = "";   
    [field: SerializeField] public int stackLimit { get; private set; } = 1;

    [field: SerializeField] public LoadoutController LoadoutController { get; private set; }

    public void Initialise(LoadoutController aOwner)
    {
        LoadoutController = aOwner;
    }

    protected virtual void Awake()
    {
    }
    
    protected virtual void Start()
    {  
    }
    
    protected virtual void Update()
    {   
    }

    protected virtual void FixedUpdate()
    {
    }

    public virtual void Use(bool aRelease)
    { 
    }

    public virtual void OnEquip()
    { 
    }
    
    public virtual void OnUnequip()
    {
    }

    public bool IsEqual(Item aOtherItem) //Didn't override Equals because I didnt want to override GetHashCode
    {
        return Name == aOtherItem.Name;
    }
}
