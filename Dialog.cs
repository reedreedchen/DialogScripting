using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * The Dialog component stores per-object data to be used by the DialogViewer component.  
 * The hierarchy of data is as follows:
 * Dialog
 *     Dialog Tree
 *         Dialog Frames
 *              Dialog Buttons
 */

public class Dialog : MonoBehaviour {
	private int uid = 0;

    public int treeToOpen = 0;

    public bool keepScore = false;
    public bool isHazard = false;

    //all the dialog trees
    public List<DialogTree> trees = new List<DialogTree>();

    public void OnCreate()
    {
        setTreeToOpen(treeToOpen);
    }
     
	public void addDialogTree()
	{
		DialogTree temp = new DialogTree ();
		temp.id = uid;
		uid++;
		trees.Add(temp);
	}

    public void normalizeTreeIds()
    {
        for (int i = 0; i < trees.Count; i++)
        {
            uid = i;
            trees[i].id = uid;
        }
        uid++;
    }

    public void setTreeToOpen(int id)
    {
        treeToOpen = id;
        if (trees[treeToOpen].frames.Exists(item => item.saveUserInput))
            DialogInputAggregator.Instance.RegisterObject(gameObject);
        else
            DialogInputAggregator.Instance.DeregisterObject(gameObject);
    }
}
