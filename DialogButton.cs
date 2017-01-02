using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * The 
 *  class is used to store button information.
 */
[Serializable]
public class DialogButton {

    public enum Button {None, Fire1, Fire2, Fire3, Fire4}

	public bool maximized;

	public string text;

    //frame id of the frame to jump to when clicked. -1 closes dialog.
	public int gotoOnClick = -1;

    public string linkOnClick = "";

    public bool isCorrectAnswer;

    public bool interactable;

    public bool colorGreen;

    public Button button;

    public MonoBehaviour script;

    public string functionOnClick = "";
}
