using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DialogInputAggregator
{
    private static DialogInputAggregator instance = new DialogInputAggregator();
    public static DialogInputAggregator Instance
    {
        get 
        {
            return instance; 
        }
    }

    private Dictionary<GameObject, List<DialogInput>> inputMap;

    private DialogInputAggregator()
    {
        inputMap = new Dictionary<GameObject, List<DialogInput>>();
    }

    public void AddDialogInput(GameObject obj, DialogInput input)
    {
        inputMap[obj].Add(input);
    }

    public void RegisterObject(GameObject obj)
    {
        if (!inputMap.ContainsKey(obj))
            inputMap.Add(obj, new List<DialogInput>());
    }

    public void DeregisterObject(GameObject obj)
    {
        if (inputMap.ContainsKey(obj))
            inputMap.Remove(obj);
    }

    public List<GameObject> GetObjectsWithNoInput()
    {
        List<GameObject> objList = new List<GameObject>();
        foreach (GameObject obj in inputMap.Keys)
        {
            if (inputMap[obj].Count == 0)
            {
                objList.Add(obj);
            }
        }
        return objList;
    }

    public int Count
    {
        get
        {
            return inputMap.Count;
        }
    }
}
