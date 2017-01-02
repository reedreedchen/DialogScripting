using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DialogInput
{
    public DialogFrame frame;
    public int buttonClicked;
    public List<int> correctButtons;

    public DialogInput(DialogFrame frame, int buttonClicked, List<int> correctButtons)
    {
        this.frame = frame;
        this.buttonClicked = buttonClicked;
        this.correctButtons = correctButtons;
    }

    public bool IsCorrect()
    {
        return correctButtons.Contains(buttonClicked);
    }
}

