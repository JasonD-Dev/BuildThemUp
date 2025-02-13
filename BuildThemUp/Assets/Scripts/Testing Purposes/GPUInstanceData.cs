using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] //For testing
public class GPUInstanceData
{
    private Mesh mMesh;
    private Material mMaterial;
    private List<List<Matrix4x4>> mMatricesList;

    const int mMaxArrayElements = 1000;

    //Vacant list coords
    private int mVacantListIndex;
    private int mVacantListCount;

    private string mName;

    public GPUInstanceData(Mesh aMesh, Material aMaterial)
    {
        mMesh = aMesh;
        mMaterial = aMaterial;

        mMatricesList = new List<List<Matrix4x4>>
        {
            new List<Matrix4x4>()
        };

        mVacantListIndex = 0;
        mVacantListCount = 0;

        mName = GenerateName(aMesh, aMaterial);
    }

    //Returns index value as a way to more efficiently removing the add matrix later.
    public int AddTransform(Matrix4x4 aMatrix)
    {
        mMatricesList[mVacantListIndex].Add(aMatrix);
        mVacantListCount++;
        int tListIndex = mVacantListIndex;

        //Adjusting vacant index if list is now full.
        if (mVacantListCount >= mMaxArrayElements)
        {
            bool tFoundVacantListIndex = false;

            int tMatricesListsCount = mMatricesList.Count;
            for (int i = mVacantListIndex + 1; i < tMatricesListsCount; i++)
            {
                int tMatricesCount = mMatricesList[i].Count;
                if (tMatricesCount <= mMaxArrayElements)
                {
                    mVacantListIndex = i;
                    mVacantListCount = tMatricesCount;

                    tFoundVacantListIndex = true;
                    break;
                }
            }

            if (tFoundVacantListIndex == false)
            {
                mVacantListCount = 0;
                mVacantListIndex = tMatricesListsCount; //Do this before a new element is added to avoid -1 math.

                mMatricesList.Add(new List<Matrix4x4>());
            }
        }

        return tListIndex;
    }

    //Not ideal to use an index (since you'll have to know / store it) but it should be alot quicker since this fuction may need to be called alot.
    public bool RemoveTransform(int aListIndex, Matrix4x4 aMatrix)
    {
        if (aListIndex > mVacantListIndex)
        {
            Log.Error(mName, $"Cannot remove transform matrix because the list index is out of bounds!");
            return false;
        }

        bool tOutcome = mMatricesList[aListIndex].Remove(aMatrix);

        if (tOutcome == true)
        {
            //Adjust vacant list index.
            if (aListIndex < mVacantListIndex)
            {
                mVacantListIndex = aListIndex;
                mVacantListCount = mMaxArrayElements - 1;
            }
            else
            {
                //If the list we are removing the element from is also already the vacant list
                mVacantListCount--;
            }
        }

        return tOutcome;
    }

    public void Render()
    {
        for (int i = 0; i < mMatricesList.Count; i++)
        {
            Graphics.DrawMeshInstanced(mMesh, 0, mMaterial, mMatricesList[i]);
        }
    }

    public bool MatchName(string aName)
    {
        return mName == aName;
    }


    public string GetName()
    {
        return mName;
    }

    public static string GenerateName(Mesh aMesh, Material aMaterial)
    {
        return aMesh.name + " + " + aMaterial.name;
    }

    public static Matrix4x4 TransformToMatrix(Transform aTransform)
    {
        return Matrix4x4.TRS(aTransform.position, aTransform.rotation, aTransform.localScale);
    }
}
