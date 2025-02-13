using System;

public class Inventory<T> where T : Item
{
    public int Capacity => mCapacity;
    public int Count => mCount;

    private T[] mItems;
    private int mCapacity;
    private int mCount;

    public Inventory(int aCapacity)
    {
        mCapacity = aCapacity;
        mItems = new T[aCapacity];
    }

    public T GetItem(int idx)
    {
        if (idx >= mCount)
            return null;
        
        return mItems[idx];
    }

    public T GetItem(T aItem)
    {
        if (aItem == null)
        {
            Log.Info(typeof(Inventory<T>), "The item provided is null.");
            return null;
        }

        foreach(T aInventoryItem in mItems)
        {
            if (aInventoryItem == null)
            {
                continue;
            }

            if (aInventoryItem.IsEqual(aItem) == true)
            {
                return aInventoryItem;
            }
        }

        Log.Info(typeof(Inventory<T>), $"{aItem.displayName} does not exist in the current inventory.");
        return null;
    }
    
    /// <summary>
    ///     Swap two item indexes.
    /// </summary>
    /// <param name="aIdxA">Index A to swap.</param>
    /// <param name="aIdxB">Index B to swap.</param>
    public void SwapItems(int aIdxA, int aIdxB)
    {
        // fancy new C# feature to swap by deconstruction
        (mItems[aIdxA], mItems[aIdxB]) = (mItems[aIdxB], mItems[aIdxA]);
    }

    /// <summary>
    ///     Adds an item to the inventory at the specified index.
    /// </summary>
    /// <param name="aItem">Item to add to inventory.</param>
    /// <param name="aIdx">Desired index of item in inventory.</param>
    /// <returns>False if item could not be added to inventory.</returns>
    public bool AddItemAtIndex(T aItem, int aIdx)
    {
        // invalid index (out of range)
        if (aIdx < 0 || aIdx >= mCapacity)
            throw new ArgumentOutOfRangeException("Tried to add an item index that was out of range: " + aIdx);

        // already an item there
        if (mItems[aIdx] != null)
            throw new Exception("Item already exists at index " + aIdx);

        mItems[aIdx] = aItem;
        mCount++;
        return true;
    }
    
    /// <summary>
    ///     Adds an item to the inventory.
    /// </summary>
    /// <param name="aItem">Item to add to inventory</param>
    /// <returns>int - Returns the index of the item added. -1 if unable to add the item</returns>
    public int AddItem(T aItem)
    {
        var tFirstEmpty = GetFirstNullIndex();

        if (tFirstEmpty > -1)
            AddItemAtIndex(aItem, tFirstEmpty);
        
        return tFirstEmpty;
    }

    /// <summary>
    ///     Removes a specified item from the inventory.
    /// </summary>
    /// <param name="aIdx">Index of the item to remove</param>
    public void RemoveItem(int aIdx)
    {
        // invalid index (out of range)
        if (aIdx < 0 || aIdx >= mCapacity)
            throw new ArgumentOutOfRangeException("Tried to add an item index that was out of range: " + aIdx);
        
        // avoid messing up the count
        if (mItems[aIdx] == null)
            throw new Exception("Tried to remove an item at index that didn't exist: " + aIdx);
        
        mItems[aIdx] = null;
        mCount--;
    }

    /// <summary>
    ///     Gets the first null index in the inventory, i.e., the first available slot.
    /// </summary>
    /// <returns>int - First null index if found. -1 if inventory is full</returns>
    public int GetFirstNullIndex()
    {
        // skip checking as we know no slots are available
        if (mCount >= mCapacity)
            return -1;
        
        for (int i = 0; i < mCapacity; i++)
            if (mItems[i] == null)
                return i;
        
        // we actually shouldn't be able to reach here
        // if our internal counting works as intended...
        return -1;
    }

}