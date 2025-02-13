using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static PromptManager instance { get; private set; }

    [SerializeField] public Prompt prompt;

    private bool mActive;
    
    private Dictionary<byte, PromptData> mStoredPrompts = new Dictionary<byte, PromptData>();
    private Stack<byte> mPromptStack = new Stack<byte>();
    private byte mCurIndex;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Log.Error(this, $"There were multiple instances of {typeof(PromptManager)} found. Destroying this one!");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void Update()
    {
        if (mActive)
        {
            var tActivePrompt = mStoredPrompts[mPromptStack.Peek()];
            if (Input.GetKeyDown(tActivePrompt.AcceptKey))
                tActivePrompt.AcceptCallback.Invoke();
        }
    }

    public byte ShowPrompt(string aText, Action aCallback, KeyCode aKey)
    {
        var tPromptData = new PromptData(aText, aCallback, aKey);
        
        // rolling index
        var tIndex = mCurIndex++;
        
        // add prompt to storage, push to stack
        mStoredPrompts.Add(tIndex, tPromptData);
        mPromptStack.Push(tIndex);
        
        // set active prompt text
        SetPromptUI(aText);
        mActive = true;
        
        // return index, as they will need this to remove it
        return tIndex;
    }

    private void SetPromptUI(string aText)
    {
        prompt.Deactivate();
        prompt.SetText(aText);
        prompt.Activate();
    }

    public void RemovePrompt(byte id)
    {
        // we have no prompts to remove...
        if (mPromptStack.Count <= 0)
            return;

        // remove the prompt from storage
        mStoredPrompts.Remove(id);
        
        // active prompt will only change if this was the active prompt
        if (id != mPromptStack.Peek())
            return;
        
        // get to the next one
        mPromptStack.Pop();
            
        // check: do we have another prompt to go to?
        if (mPromptStack.TryPeek(out var tNext))
        {
            // make sure it's actually still in the dictionary
            while (!mStoredPrompts.ContainsKey(tNext))
            {
                // no? let's go to the next one
                mPromptStack.Pop();
                
                // continue = check if this one is in the dict
                if (mPromptStack.TryPeek(out tNext))
                    continue;
                
                // no more new prompts, so deactivate
                prompt.Deactivate();
                mActive = false;
                return;
            }
            
            // set to the next prompt
            SetPromptUI(mStoredPrompts[tNext].PromptText);
        }
        else
        {
            // no new prompt, so deactivate
            prompt.Deactivate();
            mActive = false;
        }

    }

    private struct PromptData
    {
        public string PromptText;
        public Action AcceptCallback;
        public KeyCode AcceptKey;

        public PromptData(string aText, Action aCallback, KeyCode aKey)
        {
            PromptText = aText;
            AcceptCallback = aCallback;
            AcceptKey = aKey;
        }
    }

}