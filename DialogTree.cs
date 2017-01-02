using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DialogTree {

	public bool maximized;

	public int uid = 0;

	public int id = -1;

	public int startingFrameId = 0;

    public bool showOnce = true;

    public bool hasBeenSeen = false;

    public List<DialogFrame> frames = new List<DialogFrame>();

	public void addFrame()
	{
		DialogFrame temp = new DialogFrame ();
		temp.id = uid;
		uid++;
        frames.Add(temp);
	}

    public void normalizeFrameIds()
    {
        //frame IDs are used by buttons, and we would like to preserve this link after normalization
        //go through all buttons, if they are linked to a frame, add it to the list and shift its gotoId by the number of frames
        List<DialogButton> buttons = new List<DialogButton>();
        for (int i = 0; i < frames.Count; i++)
        {
            for (int b = 0; b < frames[i].buttons.Count; b++)
            {
                if (frames[i].buttons[b].gotoOnClick >= 0 && frames[i].buttons[b].gotoOnClick < frames.Count)
                {
                    frames[i].buttons[b].gotoOnClick += frames.Count;
                    buttons.Add(frames[i].buttons[b]);
                }
            }
        }

        for (int i = 0; i < frames.Count; i++)
        {
            int oldId = frames[i].id;
            uid = i;
            frames[i].id = uid;

            //iterate over the buttons, if any of them are linked to the old frame, link to the new frame and remove the button
            for(int j = buttons.Count - 1; j >= 0; j--)
            {
                if (buttons[j].gotoOnClick == frames.Count + oldId)
                {
                    buttons[j].gotoOnClick = uid;
                    buttons.RemoveAt(j);
                }
            }
        }

        uid++;
    }
}
