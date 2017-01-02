using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using DG.Tweening;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;
#pragma warning disable 0436 // TODO: Conflicts-with-imported-type warning. Clean up later.

/*
 * When the RayOject script's ray cast collides with an object with a rigid body, the object is stored in the ray variable.
 * This script looks at the ray object and sees if it has a "Dialog" script. The dialog script stores all the dialog data.
 * If the script is found, we display the tree that we would like from the dialog script.   
 * 
 * Written By: Hector Medina-Fetterman
     */
public class DialogViewer : MonoBehaviour
{

    public List<trackedObject> savedTrackedObject = new List<trackedObject>();
    float savedTime;
    // List<DialogButton> MultipleSelectedButtons = new List<DialogButton>();
    public Text loading;
    public Sprite pressed_img;
    public Sprite unpressed_img;
    public Sprite disabled_img;

    //  float lastTap;
    public GameObject taskWindow;
    public Text taskText;

    public GameObject dialogCanvas;
    public AudioClip blip;
    public AudioSource blipblip;
    public Blur blurEffect;
    //objects for the Next window
    public GameObject nextWindow;
    public Text nextTitle;
    public Text nextText;
    public Text nextButton;

    //objects for the Yes No Window
    public GameObject yesNoWindow;
    public Text yesNoTitle;
    public Text yesNoText;
    public Text yesButton;
    public Text noButton;

    //objects for the multiple choice window, 3 options
    public GameObject multipleChoice3Window;
    public Text multipleChoice3Title;
    public Text multipleChoice3Text;
    public Text a3Button;
    public Text b3Button;
    public Text c3Button;

    //objects for the multiple choice window, 4 options
    public GameObject multipleChoice4Window;
    public Text multipleChoice4Title;
    public Text multipleChoice4Text;
    public Text a4Button;
    public Text b4Button;
    public Text c4Button;
    public Text d4Button;
    public List<Image> MC4Image = new List<Image>();

    //objects for the multiple choice window,
    public GameObject selectMultipleWindow;
    public Text selectMultipleWindowTitle;
    public List<Text> selectMultipleWindowText;
    public Text a5Button;
    public Text b5Button;
    public Text c5Button;
    public Text d5Button;
    public Text e5Button;

    //objects for the Image Window
    public GameObject imageWindow;
    public Text imageTitle;
    public Text aiButton;
    public Text biButton;
    public Text ciButton;
    public Text diButton;
    public RawImage image;

    //objects for the Movie Window
    public GameObject movieWindow;
    public Text movieTitle;
    public Text amButton;
    public Text bmButton;
    public Text cmButton;
    public Text dmButton;
    public RawImage movie;

    //object for teleport
    public GameObject teleportMenu;
    //object for escMe
    public GameObject escMenu;
    public Text escMenuTlt;
    public Text escb0;
    public Text escb1;
    public Text escb2;
    public Text escb3;
    public Text escb4;

    //object for toolManu
    public GameObject toolWindow;
    public GameObject fader;
    public Text B4Text;

    //object for testWindow
    public GameObject testWindow;
    public Text testTitle;
    public Text atButton;
    public Text btButton;
    public Text ctButton;
    public Text dtButton;
    public RawImage testImg;
    AudioSource sound;

    //sound slider
    public GameObject soundWindow;
    public Slider soundSliderControl;
    public Text soundReturnBtn;


    int treeId = -1, frameId = -1;
    RayObject ray;
    CharacterMotor motor;
    CameraController cc;
    public GameObject scorePanel;

    public GameObject finger;

    private Dialog dialog;
    public bool dialogSwitch;
    public bool isDialogActive = true;
    public GameObject helpWindow;

    float lastRayTime = 0;
    float rayCoolDown = 0.8f;

    string temporaryTime = "";

    void Awake() { blurEffect = GameManager.MainCamera.GetComponent<Blur>(); }
    void Start()
    {
        if (!SceneManager.GetActiveScene().name.Equals("menu") && !SceneManager.GetActiveScene().name.Equals("overview"))
        {
            GameObject.Find("waterParticle").GetComponent<AudioSource>().Stop();
            GameObject.Find("waterParticle_snd").GetComponent<AudioSource>().Stop();
        }

        ray = GameManager.MainCamera.GetComponent<RayObject>();
        cc = GameManager.MainCamera.GetComponent<CameraController>(); // EDIT: Revise
                                                                      //      speech =    GameManager.MainCamera.GetComponent<SpeechInput>();
                                                                      // cursor =    GameManager.Player.GetComponent<LockCursor>();
        motor = GameManager.Player.GetComponent<CharacterMotor>();

        AudioListener.pause = false;

        if (!Tutorial.menuToTutorial && !Tutorial.menuToWalkThrough && ObjectTracker.trackedObjects.Count > 0)
            foreach (trackedObject i in ObjectTracker.trackedObjects)
            {
                if (SceneManager.GetActiveScene().name.Equals(i.module))
                {
                    //add selected objects back to game from serilization
                    if (!ray.selected.Contains(GameObject.Find(i._name).gameObject))
                        ray.selected.Add(GameObject.Find(i._name).gameObject);
                    //add score back to game from serilization
                    if ((!i.module.Equals("accessment") && i.answer == 1) || (i.module.Equals("accessment") && i.frameId.Equals(0) && i.answer_multiple[0] == 1))
                    {
                        if (i.room == LayerMask.NameToLayer("kitchen")) cc.kitchenRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("outdoors")) cc.yardRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("livingRoom")) cc.livingRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("hallway") || i.room == LayerMask.NameToLayer("hallwayCloset")) cc.hallwayRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("secondfloor")|| i.room == LayerMask.NameToLayer("secondfloorBathroom")|| i.room == LayerMask.NameToLayer("secondfloorStorage")) cc.secondfloorRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("bathroom")) cc.bathroomRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("bedroom")) cc.bedroomRoomCount++;
                        else if (i.room == LayerMask.NameToLayer("basement")) cc.basementRoomCount++;
                    }
                }
            }

        if (!ObjectTracker.crashTrack.Equals(0))
        {
            GameManager.Player.transform.position = ObjectTracker.playerPosition;
            GameManager.Player.transform.rotation = ObjectTracker.playerDirection;
        }

        soundSliderControl.value = playSound.soundVol;
        AudioListener.volume = soundSliderControl.value;

    }

    void OpenDialog()
    {
        Dialog objDialog = ray.rayInfo.rigidbody.GetComponent<Dialog>();
        if (objDialog != null)
        {
            if (Input.GetButtonDown("Fire1") && Time.timeSinceLevelLoad - lastRayTime > rayCoolDown)
            {
                DateTime dt = System.DateTime.Now;
                temporaryTime = System.String.Format("{0:00}/{1:00}/{2:0000},{3:00}:{4:00}:{5:00},", dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, dt.Second);//save absolute time when fire1.
                savedTime = Time.timeSinceLevelLoad;
                if (GameManager.UsingOVR) cc.canUpdateDialog = true;
                dialog = objDialog;
                isDialogActive = true;
                treeId = objDialog.treeToOpen;
                frameId = 0;
                dialogSwitch = true;
                objDialog.trees[objDialog.treeToOpen].hasBeenSeen = true;
            }
        }
    }
    void Update()
    {


        //sound
        if (Input.GetKey(KeyCode.Period))
        {
            soundSliderControl.value += 0.01f;
            AudioListener.volume = soundSliderControl.value;
            playSound.soundVol = soundSliderControl.value;
        }
        else if (Input.GetKey(KeyCode.Comma))
        {
            soundSliderControl.value -= 0.01f;
            AudioListener.volume = soundSliderControl.value;
            playSound.soundVol = soundSliderControl.value;
        }

        if (!isDialogActive && !Tutorial.menuToTutorial)
        {
            if (dialogSwitch)
            {
                blurEffect.enabled = false;

                if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Locked;
                else Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //limit dialog to one time in assessment module only
                if (ray.rayInfo.rigidbody != null && SceneManager.GetActiveScene().name.Equals("accessment") && !ray.selected.Contains(ray.oldObject))
                {
                    OpenDialog();
                }
                else if (ray.rayInfo.rigidbody != null && !SceneManager.GetActiveScene().name.Equals("accessment"))
                {//unlimitedOpen
                    OpenDialog();
                }else
                    dialogSwitch = false;
            }
        }else if (isDialogActive && !Tutorial.menuToTutorial)
        {

            //We switched dialog frames or opened a new dialog tree, lets do this stuff once not every update
            if (dialogSwitch)
            {
                blurEffect.enabled = true;

                if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Locked;
                else Cursor.lockState = CursorLockMode.None;
                if (!Cursor.visible) Cursor.visible = true;

                if (yesNoWindow.activeSelf) yesNoWindow.SetActive(false);
               else if(nextWindow.activeSelf) nextWindow.SetActive(false);
               else if(multipleChoice3Window.activeSelf) multipleChoice3Window.SetActive(false);
               else if(multipleChoice4Window.activeSelf) multipleChoice4Window.SetActive(false);
               else if(movieWindow.activeSelf) movieWindow.SetActive(false);
               else if(imageWindow.activeSelf) imageWindow.SetActive(false);
               else if(testWindow.activeSelf) testWindow.SetActive(false);
               else if(selectMultipleWindow.activeSelf) selectMultipleWindow.SetActive(false);
               cc.LockCamera(true);


                DialogFrame.Type type = dialog.trees[treeId].frames[frameId].type;
                string title = dialog.trees[treeId].frames[frameId].title;
                string text = dialog.trees[treeId].frames[frameId].text;

                // List<DialogButton> dButtons = dialog.trees[treeId].frames[frameId].buttons;
                if (type == DialogFrame.Type.Next)
                {
                    DialogButton b = dialog.trees[treeId].frames[frameId].buttons[0];
                    nextTitle.text = title;
                    nextText.text = text;
                    nextButton.text = b.text;
                    nextButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b.interactable;

                    nextWindow.SetActive(true);
                }
                else if (type == DialogFrame.Type.YesNo)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    yesNoTitle.text = title;
                    yesNoText.text = text;

                    yesButton.text = b0.text;
                    noButton.text = b1.text;
                    yesButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    noButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;

                    yesNoWindow.SetActive(true);
                }
                else if (type == DialogFrame.Type.MultipleChoice3)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    multipleChoice3Title.text = title;
                    multipleChoice3Text.text = text;
                    a3Button.text = b0.text;
                    b3Button.text = b1.text;
                    c3Button.text = b2.text;
                    a3Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    b3Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    c3Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;

                    multipleChoice3Window.SetActive(true);
                }
                else if (type == DialogFrame.Type.MultipleChoice4)
                {

                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    DialogButton b3 = dialog.trees[treeId].frames[frameId].buttons[3];

                    multipleChoice4Title.text = title;
                    multipleChoice4Text.text = text;

                    a4Button.text = b0.text;
                    if (b0.colorGreen)
                        a4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        a4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    b4Button.text = b1.text;
                    if (b1.colorGreen)
                        b4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        b4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    c4Button.text = b2.text;
                    if (b2.colorGreen)
                        c4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        c4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    d4Button.text = b3.text;
                    if (b3.colorGreen)
                        d4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        d4Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    a4Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    b4Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    c4Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;
                    d4Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b3.interactable;

                    multipleChoice4Window.SetActive(true);
                }
                else if (type == DialogFrame.Type.SelectMultiple)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    DialogButton b3 = dialog.trees[treeId].frames[frameId].buttons[3];
                    DialogButton b4 = dialog.trees[treeId].frames[frameId].buttons[4];//continue

                    selectMultipleWindowTitle.text = title;
                    selectMultipleWindowText[0].text = b0.text;
                    selectMultipleWindowText[1].text = b1.text;
                    selectMultipleWindowText[2].text = b2.text;
                    selectMultipleWindowText[3].text = b3.text;

                   if (b0.colorGreen)
                        a5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        a5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;

                    if (b1.colorGreen)
                        b5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        b5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;


                    if (b2.colorGreen)
                        c5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        c5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;

                    if (b3.colorGreen)
                        d5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        d5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;

                    if (b4.colorGreen)
                        e5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        e5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;

                    a5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    b5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    c5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;
                    d5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b3.interactable;
                   

                    if (!b0.interactable) a5Button.gameObject.transform.parent.GetComponent<Image>().sprite = disabled_img;
                    if (!b1.interactable) b5Button.gameObject.transform.parent.GetComponent<Image>().sprite = disabled_img;
                    if (!b2.interactable) c5Button.gameObject.transform.parent.GetComponent<Image>().sprite = disabled_img;
                    if (!b3.interactable) d5Button.gameObject.transform.parent.GetComponent<Image>().sprite = disabled_img;
                  
                    selectMultipleWindow.SetActive(true);
                }
                else if (type == DialogFrame.Type.Image)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    DialogButton b3 = dialog.trees[treeId].frames[frameId].buttons[3];
                    if (SceneManager.GetActiveScene().name.Equals("electricalFireHazards2") && title.Equals("Is this a hazard?"))
                    { title = "Is this a fire/electrical/burn hazard?"; }
                    else if(SceneManager.GetActiveScene().name.Equals("environmental") && title.Equals("Is this a hazard?"))
                    { title = "Is this an environmental hazard?"; }
                    else if (SceneManager.GetActiveScene().name.Equals("slipTripLift") && title.Equals("Is this a hazard?"))
                    { title = "Is this a slip, trip, or lift hazard?"; }
                    imageTitle.text = title;
                    image.texture = dialog.trees[treeId].frames[frameId].texture;
                    aiButton.text = b0.text;
                    if (b0.colorGreen)
                        aiButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        aiButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    biButton.text = b1.text;
                    if (b1.colorGreen)
                        biButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        biButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    ciButton.text = b2.text;
                    if (b2.colorGreen)
                        ciButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        ciButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    diButton.text = b3.text;
                    if (b3.colorGreen)
                        diButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        diButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    aiButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    biButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    ciButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;
                    diButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b3.interactable;

                    imageWindow.SetActive(true);
                }
                else if (type == DialogFrame.Type.Movie)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    DialogButton b3 = dialog.trees[treeId].frames[frameId].buttons[3];

                    movieTitle.text = title;
                    movie.texture = dialog.trees[treeId].frames[frameId].texture;
                    amButton.text = b0.text;
                    bmButton.text = b1.text;
                    cmButton.text = b2.text;
                    dmButton.text = b3.text;
                    amButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    bmButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    cmButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;
                    dmButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b3.interactable;
                    Debug.Log("enabled movie");
                    movieWindow.SetActive(true);
                }
                else if (type == DialogFrame.Type.Test)
                {
                    DialogButton b0 = dialog.trees[treeId].frames[frameId].buttons[0];
                    DialogButton b1 = dialog.trees[treeId].frames[frameId].buttons[1];
                    DialogButton b2 = dialog.trees[treeId].frames[frameId].buttons[2];
                    DialogButton b3 = dialog.trees[treeId].frames[frameId].buttons[3];

                    testImg.texture = dialog.trees[treeId].frames[frameId].texture;
                    testTitle.text = title;

                    List<AudioSource> AudioList = new List<AudioSource>();
                    ray.hovered.gameObject.GetComponents<AudioSource>(AudioList);
                    if (AudioList.Count > 0)
                    {
                        sound = ray.hovered.gameObject.GetComponent<AudioSource>();
                    }
                    else { sound = null; } //dialog.trees[treeId].frames[frameId].sound;
                    atButton.text = b0.text;
                    btButton.text = b1.text;
                    ctButton.text = b2.text;
                    dtButton.text = b3.text;


                    atButton.text = b0.text;
                    if (b0.colorGreen)
                        atButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        atButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    btButton.text = b1.text;
                    if (b1.colorGreen)
                        btButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        btButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    ctButton.text = b2.text;
                    if (b2.colorGreen)
                        ctButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        ctButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;
                    dtButton.text = b3.text;
                    if (b3.colorGreen)
                        dtButton.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                    else
                        dtButton.gameObject.transform.parent.GetComponent<Image>().color = Color.white;


                    atButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b0.interactable;
                    btButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b1.interactable;
                    ctButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b2.interactable;
                    dtButton.gameObject.transform.parent.parent.GetComponent<Button>().interactable = b3.interactable;
                    //Debug.Log("enabled test");
                    testWindow.SetActive(true);
                }
                if (testWindow.activeSelf) resetTestedDialog();
                dialogSwitch = false;

            }

            
            if (selectMultipleWindow.activeSelf && dialog.trees[treeId].frames[frameId].type == DialogFrame.Type.SelectMultiple && (frameId == 2))
            {
                float bufferTime = Time.timeSinceLevelLoad - savedTime;
                if (bufferTime <= 1)
                {
                    e5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = false;
                    e5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.white;

                }
                else
                {
                    e5Button.gameObject.transform.parent.parent.GetComponent<Button>().interactable = true;
                    e5Button.gameObject.transform.parent.GetComponent<Image>().color = Color.green;
                }
                
            }
        }
    }


    public void turnOnMC4Image()
    {
        MC4Image[0].enabled = true;
    }

    public void turnOffMC4Image()
    {
        MC4Image[0].enabled = false;
    }

    public void turnOnMC4Image1()
    {
        MC4Image[1].enabled = true;
    }

    public void turnOffMC4Image1()
    {
        MC4Image[1].enabled = false;
    }


    public void teleport()
    {
        escMenu.SetActive(false);
        teleportMenu.SetActive(true);
        GameObject.Find("teleMenuTitleLabel").GetComponent<Text>().text = "Teleport";
        GameObject b0 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option0").gameObject;
        GameObject b1 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option1").gameObject;
        GameObject b2 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option2").gameObject;
        GameObject b3 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option3").gameObject;
        GameObject b4 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option4").gameObject;
        GameObject b5 = teleportMenu.transform.FindChild("Buttons").transform.FindChild("Option5").gameObject;
        if (Tutorial.menuToTutorial && GameObject.Find("Tutorial").GetComponent<Tutorial>().counter <= 12)
        {
            b0.GetComponent<Button>().interactable = false;
            b0.transform.FindChild("Background").GetComponent<Image>().color = Color.white;
            b1.GetComponent<Button>().interactable = false;
            b1.transform.FindChild("Background").GetComponent<Image>().color = Color.white;
            b2.GetComponent<Button>().interactable = false;
            b2.transform.FindChild("Background").GetComponent<Image>().color = Color.white;
            b3.GetComponent<Button>().interactable = false;
            b3.transform.FindChild("Background").GetComponent<Image>().color = Color.white;
            b4.GetComponent<Button>().interactable = false;
            b4.transform.FindChild("Background").GetComponent<Image>().color = Color.white;
            b5.GetComponent<Button>().interactable = false;
            b5.transform.FindChild("Background").GetComponent<Image>().color = Color.white;

        }
        else if (Tutorial.menuToTutorial && GameObject.Find("Tutorial").GetComponent<Tutorial>().counter > 12)
        {

            b0.GetComponent<Button>().interactable = true;
            b0.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
            b1.GetComponent<Button>().interactable = true;
            b1.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
            b2.GetComponent<Button>().interactable = true;
            b2.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
            b3.GetComponent<Button>().interactable = true;
            b3.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
            b4.GetComponent<Button>().interactable = true;
            b4.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
            b5.GetComponent<Button>().interactable = true;
            b5.transform.FindChild("Background").GetComponent<Image>().color = Color.green;
        }

    }
    GameObject teleportTarget;
    public void teleportThroughMenu(int room)
    {
        switch (room)
        {
            case 0:
                //to livingroom
                teleportTarget = GameObject.Find("locationCube_livingRoom");
                GameObject.Find("Living Room Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 1:
                //to kitchen
                teleportTarget = GameObject.Find("locationCube_kitchen");
                GameObject.Find("Kitchen Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 2:
                //to bedroom
                teleportTarget = GameObject.Find("locationCube_bedroom");
                GameObject.Find("Bedroom Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 3:
                //to Bathroom
                teleportTarget = GameObject.Find("locationCube_bathroom");
                GameObject.Find("First Floor Bathroom Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 4:
                //to SndFloor
                teleportTarget = GameObject.Find("locationCube_sndFloor");
                GameObject.Find("Second Floor Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 5:
                //to basement
                teleportTarget = GameObject.Find("locationCube_basement3");
                GameObject.Find("Basement Trigger").GetComponent<RoomTrigger>().roomChange();
                break;
            case 6:
                //cancel
                break;
            default:
              //  Debug.Log("wrong case number");
                break;
        }
        closeTeleportMenu();

        if (!room.Equals(6)) askToTeleport();
    }
    void closeTeleportMenu() {
        teleportMenu.SetActive(false);
        isDialogActive = false;
        cc.LockCamera(false);
        if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Confined;
        else Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void askToTeleport()
    {
        fader.GetComponent<CanvasGroup>().DOFade(1, 0.5f).OnComplete(fadeOut);
        GameObject.Find("Loading").GetComponent<Text>().text = "Teleporting...";

    }
    void fadeOut()
    {
        cc.detectRoomTrigger();
//        Debug.Log(teleportTarget.transform.gameObject.name);
        GameManager.Player.transform.position = new Vector3(teleportTarget.transform.position.x, teleportTarget.GetComponent<locationCubeCollision>().iniY + 0.094f, teleportTarget.transform.position.z);
        Invoke("closeError", 2f);
        Invoke("fadeIn", 0.5f);
    }
    void fadeIn()
    {
        fader.SetActive(true);
        fader.GetComponent<CanvasGroup>().DOFade(0, 0.7f).OnComplete(clearContent);
    }
    void clearContent() { 
        GameObject.Find("Loading").GetComponent<Text>().text = "";
    }
    public void PauseGame()
    {
       // Debug.Log(isDialogActive);
        if (!Tutorial.menuToTutorial || !Tutorial.menuToWalkThrough)
        {
            if (!isDialogActive && Time.timeScale == 1)
            {
                GameManager.Player.GetComponent<CharacterMotor>().enabled = false;

                cc.LockCamera(true);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                isDialogActive = true;
                showMessage.showError("Game Pause");
                GameManager.MainCamera.GetComponent<Blur>().enabled = true;
                showMessage.msg.GetComponent<Text>().color = new Color(showMessage.msg.GetComponent<Text>().color.r, showMessage.msg.GetComponent<Text>().color.g, showMessage.msg.GetComponent<Text>().color.b, 1f);
                Time.timeScale = 0;
            }
            else if(Time.timeScale == 0)
            {
                Time.timeScale = 1;
                GameManager.MainCamera.GetComponent<Blur>().enabled = false;
                GameManager.Player.GetComponent<CharacterMotor>().enabled = true;
                isDialogActive = false;
                cc.LockCamera(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                showMessage.msg.GetComponent<Text>().color = new Color(showMessage.msg.GetComponent<Text>().color.r, showMessage.msg.GetComponent<Text>().color.g, showMessage.msg.GetComponent<Text>().color.b, 0f);
            }
        }
    }

    public void OpenEscapeMenu()
    {
        cc.LockCamera(true);
        isDialogActive = true;
        escMenu.SetActive(true);
        dialogSwitch = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
                
        if (escb0.text != "Help")
        {
            escMenuTlt.text = "Menu";
            escb0.text = "Help";
            escb1.text = "Toolbox";
            escb2.text = "Return";
            escb2.gameObject.transform.parent.gameObject.SetActive(true);
            escb3.gameObject.transform.parent.gameObject.SetActive(true);
            escb4.gameObject.transform.parent.gameObject.SetActive(true);
        }
    }



    public void soundSlider()
    {
        AudioListener.volume = soundSliderControl.value;
        playSound.soundVol = soundSliderControl.value;
    }

    public void forSoundReturn()
    {
        isDialogActive = false;
        cc.LockCamera(false);
        soundWindow.SetActive(false);
        if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //temp
    }


    //testWindow start~renee

    public void testBtn()
    {
        if (sound!=null && !sound.isPlaying)
        {
            sound.enabled = true;
            sound.Play();
            showMessage.showError("Beep, beep, beep...");
            Invoke("closeError", 2f);
            dialog.trees[treeId].frames[frameId].buttons[2].text = "Stop Test";

        }else if (sound != null && sound.isPlaying)
        {
            sound.Stop();
            sound.enabled = false;

            string decidedTitle="";
            if (SceneManager.GetActiveScene().name == "electricalFireHazards2") decidedTitle = "Is this a fire/electrical/burn hazard?";
            else if (SceneManager.GetActiveScene().name == "environmental") decidedTitle = "Is this an environmental hazard?";
            else if (SceneManager.GetActiveScene().name == "slipTripLift") decidedTitle = "Is this a slip, trip, or lift hazard?";
            else if (SceneManager.GetActiveScene().name == "accessment") decidedTitle = "Is this a hazard?";

            dialog.trees[treeId].frames[frameId].title = decidedTitle;
            dialog.trees[treeId].frames[frameId].buttons[2].text = "Test";
            dialog.trees[treeId].frames[frameId].buttons[2].colorGreen = false;
            dialog.trees[treeId].frames[frameId].buttons[2].interactable = false;
            dialog.trees[treeId].frames[frameId].buttons[0].colorGreen = true;
            dialog.trees[treeId].frames[frameId].buttons[0].interactable = true;
            dialog.trees[treeId].frames[frameId].buttons[1].colorGreen = true;
            dialog.trees[treeId].frames[frameId].buttons[1].interactable = true;
        }
        else
        {
            showMessage.showError("This item stays quiet.");
            Invoke("closeError", 2f);
            string decidedTitle = "";
            if (SceneManager.GetActiveScene().name == "electricalFireHazards2") decidedTitle = "Is this a fire/electrical/burn hazard?";
            else if (SceneManager.GetActiveScene().name == "environmental") decidedTitle = "Is this an environmental hazard?";
            else if (SceneManager.GetActiveScene().name == "slipTripLift") decidedTitle = "Is this a slip, trip, or lift hazard?";
            else if (SceneManager.GetActiveScene().name == "accessment") decidedTitle = "Is this a hazard?";
            dialog.trees[treeId].frames[frameId].title = decidedTitle;
            dialog.trees[treeId].frames[frameId].buttons[2].colorGreen = false;
            dialog.trees[treeId].frames[frameId].buttons[2].interactable = false;
            dialog.trees[treeId].frames[frameId].buttons[0].colorGreen = true;
            dialog.trees[treeId].frames[frameId].buttons[0].interactable = true;
            dialog.trees[treeId].frames[frameId].buttons[1].colorGreen = true;
            dialog.trees[treeId].frames[frameId].buttons[1].interactable = true;

        }
    }

    public void closeError()
    {
        showMessage.closeError();
    }

    public void resetTestedDialog()
    {
        dialog.trees[treeId].frames[frameId].title = "Test";
        dialog.trees[treeId].frames[frameId].buttons[2].colorGreen = true;
        dialog.trees[treeId].frames[frameId].buttons[2].interactable = true;
        dialog.trees[treeId].frames[frameId].buttons[0].colorGreen = false;
        dialog.trees[treeId].frames[frameId].buttons[0].interactable = false;
        dialog.trees[treeId].frames[frameId].buttons[1].colorGreen = false;
        dialog.trees[treeId].frames[frameId].buttons[1].interactable = false;
    }
    //testWindow end~renee

    
    //tool window start~renee

    public void toolbtnsBlip(){  AudioSource.PlayClipAtPoint(blip, motor.gameObject.transform.position); }


    public void forEscMenuB1()
    {// help
        if (escb0.text == "Help")
        {
            helpWindow.SetActive(true);
            escMenu.SetActive(false);

         }else if (escb0.text == "Yes")
         {
            Debug.Log("Quit Game;");
			fader.SetActive(true);
			fader.GetComponent<CanvasGroup>().DOFade(1, 1f).OnComplete(backtomenu);
         }
    }

    public void closeHelp()
    {
        helpWindow.SetActive(false);
        isDialogActive = false;
        cc.LockCamera(false);
        escMenu.SetActive(false);
        if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Confined;
        else Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false; //temp
    }

    public void forEscMenuB2()
    {
        if (escb1.text.Equals("Toolbox") & !toolWindow.activeSelf)
        {
            escMenu.SetActive(false);
            toolWindow.SetActive(true);
        }
        else if (escb1.text == "No")
        {
            escMenuTlt.text = "Menu";
            escb0.text = "Help";
            escb1.text = "Toolbox";
            escb2.text = "Return";
            escb2.gameObject.transform.parent.gameObject.SetActive(true);
            escb3.gameObject.transform.parent.gameObject.SetActive(true);
            escb4.gameObject.transform.parent.gameObject.SetActive(true);
        }
    }

    public void backtomenu()
    {
        ObjectTracker.trackModuleTime(SceneManager.GetActiveScene().name, Time.timeSinceLevelLoad);
        sendDataForTracking();
        SceneManager.LoadScene("menu");
    }

    void OnApplicationQuit()
    {
        sendDataForTracking();
        ObjectTracker.trackModuleTime(SceneManager.GetActiveScene().name, Time.timeSinceLevelLoad);
        ObjectTracker.crashTrack = 0;
        ObjectTracker.printScore();
        gameLoader.saveData();
    }

    public void sendDataForTracking()
    {
        if (SceneManager.GetActiveScene().name != "menu" && !Tutorial.menuToTutorial && !Tutorial.menuToWalkThrough)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "electricalFireHazards2":
                    ObjectTracker.moduleHazardTotal_ef = cc.totalRoomsTotal;
                    ObjectTracker.moduleHazardFound_ef = cc.totalRoomsCount;
                    break;
                case "slipTripLift":
                    ObjectTracker.moduleHazardTotal_stl = cc.totalRoomsTotal;
                    ObjectTracker.moduleHazardFound_stl = cc.totalRoomsCount;
                    break;
                case "environmental":
                    ObjectTracker.moduleHazardTotal_env = cc.totalRoomsTotal;
                    ObjectTracker.moduleHazardFound_env = cc.totalRoomsCount;
                    break;
                case "accessment":
                    ObjectTracker.moduleHazardTotal_ass = cc.totalRoomsTotal;
                    ObjectTracker.moduleHazardFound_ass = cc.totalRoomsCount;
                    break;
                default:
                    break;
            }
            Debug.Log("dataSentForTracking");
        }
    }

    public void forEscMenuB3()
    {
        if (escb2.text == "Return")
        {
            isDialogActive = false;

            cc.LockCamera(false);
            escMenu.SetActive(false);
            if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Confined;
            else Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false; //temp
        }
    }
       

    public void forEscMenuB4()
    {
        if (escb3.text == "Exit Module")
        {
            escMenuTlt.text = "Are you sure?";
            escb0.text = "Yes";
            escb1.text = "No";
            escb2.gameObject.transform.parent.gameObject.SetActive(false);
            escb3.gameObject.transform.parent.gameObject.SetActive(false);
            escb4.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }


    public void forToolB1() // flashlight
    {
        Debug.Log("A: " + GameManager.MainCamera.GetComponent<flashlight>());
        GameManager.MainCamera.GetComponent<flashlight>().isOn = 
            !GameManager.MainCamera.GetComponent<flashlight>().isOn;
        GameManager.MainCamera.GetComponent<flashlight>().detectFlashlight();
    }

    public void forToolB2() // zoom
    {
        GameManager.MainCamera.GetComponent<zoom>().isZoomed = 
            !GameManager.MainCamera.GetComponent<zoom>().isZoomed;
        GameManager.MainCamera.GetComponent<zoom>().detectZoom();
    }
    
    public void forToolB3(){ // test
      //  Debug.Log("Sound Selection");
        soundWindow.SetActive(true);
        toolWindow.SetActive(false);

    }
    
    public void forToolB5() // return 
    { 
        toolWindow.SetActive(false);
      //  GameManager.DialogViewer.escMenu.SetActive(true);
        isDialogActive = false;
        cc.LockCamera(false);
        escMenu.SetActive(false);
        if (GameManager.UsingOVR) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    //tool windows end~renee

   
    public void ChangeButtonPressed(int id)
    {
        AudioSource.PlayClipAtPoint(blip, motor.gameObject.transform.position);
        if (!Tutorial.menuToTutorial && !cc.tutorial.gameObject.activeSelf)
        {
            if (dialog.trees[treeId].frames[frameId].type.Equals(DialogFrame.Type.Test) && id == 2)
             {
                savedTime = Time.timeSinceLevelLoad;
                Debug.Log("Testing"); }
            else if (!dialog.trees[treeId].frames[frameId].type.Equals(DialogFrame.Type.SelectMultiple) && treeId.Equals(0) && !nextWindow.activeSelf)
            {
                if (frameId.Equals(0))
                {
                    if (!SceneManager.GetActiveScene().name.Equals("accessment"))
                    {
                        if (!ray.selected.Contains(ray.hovered))
                        {
                            //save 1st page in the first 3 modules
                            int answer_user = 999;
                            if (id == 0) answer_user = 1; //yes button
                            else if (id == 1) answer_user = 0;

                            Debug.Log("SAVE1");
                            int correctAnswer = 999;
                            if (dialog.trees[treeId].frames[frameId].saveUserInput) correctAnswer = 1; else correctAnswer = 0;
                            ObjectTracker.trackObject(SceneManager.GetActiveScene().name, ray.hovered.layer, ray.hovered.name, correctAnswer, answer_user);
                            ObjectTracker.trackObjectTimePages(dialog.trees[treeId].frames[frameId].title, Time.timeSinceLevelLoad - savedTime, ObjectTracker.trackedObjects.Count - 1);
                            ObjectTracker.trackObjectAbsoluteTime(ObjectTracker.trackedObjects.Count - 1, temporaryTime);//need to save this on temporaryTime first because it shoul come later than tranctobject function
                            ObjectTracker.trackObjectSession(ObjectTracker.trackedObjects.Count - 1);
                        }
                    }
                    else
                    {
                        if (!ray.selected.Contains(ray.hovered))
                        {
                            //save 1st page in assessment
                            List<int> answer_multiple = new List<int>();
                            List<int> answer_multiple_user = new List<int>();
                            if (id == 0) answer_multiple_user.Add(1); //yes Button
                            else if (id == 1) answer_multiple_user.Add(0);
                            if (dialog.trees[treeId].frames[frameId].saveUserInput)
                                answer_multiple.Add(1);
                            else answer_multiple.Add(0);

                            Debug.Log("SAVE1_assessment");
                            ObjectTracker.trackObject_multiple(SceneManager.GetActiveScene().name, ray.hovered.layer, ray.hovered.name, answer_multiple, answer_multiple_user, Time.timeSinceLevelLoad - savedTime, frameId);
                            ObjectTracker.trackObjectSession(ObjectTracker.trackedObjects.Count - 1);
                            ObjectTracker.trackObjectAbsoluteTime(ObjectTracker.trackedObjects.Count - 1, temporaryTime);
                        }
                    }
                }
                else // save page time  in the first 3 modules, or for non-hazards in assessment
                {
                    if (!ray.selected.Contains(ray.hovered))
                    {
                        ObjectTracker.trackObjectTimePages(dialog.trees[treeId].frames[frameId].title, Time.timeSinceLevelLoad - savedTime, ObjectTracker.trackedObjects.Count - 1);
                        Debug.Log("SAVE3");
                    }
                }
            }
            //save multiple answers
            else if (id == 4 && dialog.trees[treeId].frames[frameId].type.Equals(DialogFrame.Type.SelectMultiple) && treeId.Equals(0) && !nextWindow.activeSelf)
            {

                if (!ray.selected.Contains(ray.hovered) && frameId != 3)
                {
                    Debug.Log("SAVE2");
                    Sprite a5Sprite = a5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                    Sprite b5Sprite = b5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                    Sprite c5Sprite = c5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                    Sprite d5Sprite = d5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                    DialogFrame thisDialogFrame = dialog.trees[treeId].frames[frameId];


                    List<int> answers = new List<int>();

                    //save system answers

                    if (thisDialogFrame.buttons[0].isCorrectAnswer) answers.Add(1); else if (thisDialogFrame.buttons[0].interactable) answers.Add(0);
                    if (thisDialogFrame.buttons[1].isCorrectAnswer) answers.Add(1); else if (thisDialogFrame.buttons[1].interactable) answers.Add(0);
                    if (thisDialogFrame.buttons[2].isCorrectAnswer) answers.Add(1); else if (thisDialogFrame.buttons[2].interactable) answers.Add(0);
                    if (thisDialogFrame.buttons[3].isCorrectAnswer) answers.Add(1); else if (thisDialogFrame.buttons[3].interactable) answers.Add(0);
               
                    List<int> answer_user = new List<int>();

                    //save user answers

                    if (a5Sprite.Equals(pressed_img)) answer_user.Add(1); else if (thisDialogFrame.buttons[0].interactable) answer_user.Add(0);
                    if (b5Sprite.Equals(pressed_img)) answer_user.Add(1); else if (thisDialogFrame.buttons[1].interactable) answer_user.Add(0);
                    if (c5Sprite.Equals(pressed_img)) answer_user.Add(1); else if (thisDialogFrame.buttons[2].interactable) answer_user.Add(0);
                    if (d5Sprite.Equals(pressed_img)) answer_user.Add(1); else if (thisDialogFrame.buttons[3].interactable) answer_user.Add(0);
                    //count only for score screen when living assessment;
                    for (int i = 1; i < answers.Count; i++)
                    {
                        if (answers[i] != 999)
                        {
                            if (answers[i] == answer_user[i])
                            {
                                if (frameId == 1) ObjectTracker.WhyCorrect += 1;
                                if (frameId == 2) ObjectTracker.WTDCorrect += 1;
                            }
                            if (frameId == 1) ObjectTracker.WhyTotal += 1;
                            if (frameId == 2) ObjectTracker.WTDTotal += 1;
                        }
                    }

                    ObjectTracker.trackObject_multiple(SceneManager.GetActiveScene().name, ray.hovered.layer, ray.hovered.name, answers, answer_user, Time.timeSinceLevelLoad - savedTime, frameId);
                }
            }
            if (dialog.trees[treeId].frames[frameId].type.Equals(DialogFrame.Type.SelectMultiple))
            {
                if (id == 4) savedTime = Time.timeSinceLevelLoad;
            }
            else savedTime = Time.timeSinceLevelLoad;


            bool idPass = false;

            if (SceneManager.GetActiveScene().name == "accessment")
            {//addscore() on Yes
                if (id == 0)
                    idPass = true;
            }
            else
            {//addscore() on Yes and No
                if (id == 0 || id == 1)
                    idPass = true;
            }

            if (dialog.trees[treeId].frames[frameId].saveUserInput && idPass)
            {
                if (!ray.selected.Contains(ray.hovered) && ray.hovered.tag == "SelectForDanger" && frameId == 0)
                {
                    Debug.Log("addScore");
                    addScore();
                    cc.detectRoomTrigger();
                }
            }
            

            /*run every last frame of the dialogs*/
            if(SceneManager.GetActiveScene().name=="accessment" && id == 4 && frameId == dialog.trees[treeId].frames.Count - 2)
            {     
                ray.selected.Add(ray.oldObject);
                sendDataForTracking(); gameLoader.saveData();
                ObjectTracker.moduleTime_temp = Time.timeSinceLevelLoad;
            }else if (frameId == dialog.trees[treeId].frames.Count - 1)
            {
                ray.selected.Add(ray.oldObject);
                sendDataForTracking(); gameLoader.saveData();
                ObjectTracker.moduleTime_temp = Time.timeSinceLevelLoad;
            }
            
            
                MonoBehaviour script = null;
                string function = "";

                if (dialog.trees[treeId].frames[frameId].buttons[id].script != null)
                {
                    script = dialog.trees[treeId].frames[frameId].buttons[id].script;
                    function = dialog.trees[treeId].frames[frameId].buttons[id].functionOnClick;
                }
                int storedID = frameId;
                frameId = dialog.trees[treeId].frames[frameId].buttons[id].gotoOnClick;
                dialogSwitch = true;
                if (frameId == -1 /*&& !dialog.trees[treeId].frames[storedID].type.Equals(DialogFrame.Type.SelectMultiple)*/)
                {
                  
                
                if (GameManager.UsingOVR) cc.canUpdateDialog = true;

                CloseOpenDialog();
                }//do not set the last continue to -1 when selectedmultiple. set it as the frame id
                else
                {
                    if (dialog.trees[treeId].frames[storedID].type == DialogFrame.Type.SelectMultiple && id != 4)
                    {
                        dialogSwitch = false;

                        Sprite a5Sprite = a5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                        Sprite b5Sprite = b5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                        Sprite c5Sprite = c5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                        Sprite d5Sprite = d5Button.gameObject.transform.parent.GetComponent<Image>().sprite;
                      
                        if (id.Equals(0) && a5Sprite.Equals(unpressed_img)) a5Sprite = pressed_img;
                        else if (id.Equals(0) && a5Sprite.Equals(pressed_img)) a5Sprite = unpressed_img;
                        if (id.Equals(1) && b5Sprite.Equals(unpressed_img)) b5Sprite = pressed_img;
                        else if (id.Equals(1) && b5Sprite.Equals(pressed_img)) b5Sprite = unpressed_img;
                        if (id.Equals(2) && c5Sprite.Equals(unpressed_img)) c5Sprite = pressed_img;
                        else if (id.Equals(2) && c5Sprite.Equals(pressed_img)) c5Sprite = unpressed_img;
                        if (id.Equals(3) && d5Sprite.Equals(unpressed_img)) d5Sprite = pressed_img;
                        else if (id.Equals(3) && d5Sprite.Equals(pressed_img)) d5Sprite = unpressed_img;
                        a5Button.gameObject.transform.parent.GetComponent<Image>().sprite = a5Sprite;
                        b5Button.gameObject.transform.parent.GetComponent<Image>().sprite = b5Sprite;
                        c5Button.gameObject.transform.parent.GetComponent<Image>().sprite = c5Sprite;
                        d5Button.gameObject.transform.parent.GetComponent<Image>().sprite = d5Sprite;
                    }
                    else if (dialog.trees[treeId].frames[storedID].type == DialogFrame.Type.SelectMultiple && id == 4)
                    {
                        if (storedID == 2)
                        {
                            if (GameManager.UsingOVR) cc.canUpdateDialog = true;
                            if (ray.oldObject.name.Contains("CandleSet")) ray.setFire();
                            CloseOpenDialog();

                        }

                        a5Button.gameObject.transform.parent.GetComponent<Image>().sprite = unpressed_img;
                        b5Button.gameObject.transform.parent.GetComponent<Image>().sprite = unpressed_img;
                        c5Button.gameObject.transform.parent.GetComponent<Image>().sprite = unpressed_img;
                        d5Button.gameObject.transform.parent.GetComponent<Image>().sprite = unpressed_img;
                        dialogSwitch = true;
                    }
                }

                if (script != null)
                {
                    script.StartCoroutine(function);
                }
            ObjectTracker.crashTrack = SceneManager.GetActiveScene().buildIndex;
            ObjectTracker.playerPosition = GameManager.Player.transform.position;
            ObjectTracker.playerDirection = GameManager.Player.transform.rotation;
        }
    }


    void addScore()
    {
        if (ray.hovered.layer == LayerMask.NameToLayer("kitchen"))
        {
            cc.kitchenRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("outdoors"))
        {
            cc.yardRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("livingRoom"))
        {
            cc.livingRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("hallway") || ray.hovered.layer == LayerMask.NameToLayer("hallwayCloset"))
        {
            cc.hallwayRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("secondfloor")||
            ray.hovered.layer == LayerMask.NameToLayer("secondfloorBathroom") ||
            ray.hovered.layer == LayerMask.NameToLayer("secondfloorStorage")
            )
        {
            cc.secondfloorRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("bathroom"))
        {
            cc.bathroomRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("bedroom") || ray.hovered.layer == LayerMask.NameToLayer("bedroomcloset"))
        {
            cc.bedroomRoomCount++;
        }
        else if (ray.hovered.layer == LayerMask.NameToLayer("basement"))
        {
            cc.basementRoomCount++;
        }
    }

//    void addtoSelected()
 //   {
  //     if(!cc.selected.Contains(dialog.gameObject))
  //          cc.selected.Add(dialog.gameObject);
  //  }
    private void CloseOpenDialog()
    {
        lastRayTime = Time.timeSinceLevelLoad;   
        yesNoWindow.SetActive(false);
        nextWindow.SetActive(false);
        multipleChoice3Window.SetActive(false);
        multipleChoice4Window.SetActive(false);
        imageWindow.SetActive(false);
        movieWindow.SetActive(false);
        testWindow.SetActive(false);
        selectMultipleWindow.SetActive(false);
        isDialogActive = false;
        cc.LockCamera(false);
        dialog = null;
    }
}

