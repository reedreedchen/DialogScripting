using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * The DialogFrame class is used to store all frame information. 
 * This includes size, position, text, title, and buttons.
 */
[Serializable]
public class DialogFrame {
    //stored editor data to determine if this frame is maximized in the editor
	public bool maximized;
    public bool isDirty = true;

    public bool saveUserInput = false;

    public enum Type
    {
        Next = 0,
        YesNo = 1,
        MultipleChoice3 = 2,
        MultipleChoice4 = 3,
        Movie,
		Image,
        Test,
        SelectMultiple
    }

    //unique id associated with the frame
	public int id = -1;

    public Type type;

	public string title = "";

    public Texture texture;

	public string text = "";

	public List<DialogButton> buttons = new List<DialogButton>();

    public void setType(Type type)
    {
        if (this.type != type)
        {
            isDirty = true;
            this.type = type;
        }
    }
}
