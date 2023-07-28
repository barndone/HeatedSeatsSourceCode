using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    //  reference to the main menu canvas object
    [SerializeField] GameObject mainMenuCanvas;
    //  reference to the join screen canvas object
    [SerializeField] GameObject joinScreenCanvas;
    //  reference to the parent object for the loading panel
    [SerializeField] GameObject loadingCanvas;

    [SerializeField] GameObject logo;

    //  reference to the parent object for the controls panel
    [SerializeField] GameObject controlPanel;
    //  reference to the parent object for the credits panel
    [SerializeField] GameObject creditsPanel;
    //  reference to the loading operation for tracking load progress
    AsyncOperation loadingOperation;

    [SerializeField] List<Sprite> characterPortraits = new List<Sprite>();

    //  list containing a reference to each game object associated to each button in the main menu
    [SerializeField] List<GameObject> mainMenuButtons;

    [SerializeField] GameObject startButton;
    [SerializeField] TextMeshProUGUI startText;

    [SerializeField] InputSystemUIInputModule uiInput;

    //  the number of connected gamepads 
    [SerializeField] int connectedControllers;
    [SerializeField] const int MAX_PLAYERS = 4;

    [SerializeField] GameObject joinPanel;
    [SerializeField] GameObject joinPanelGroup;

    [SerializeField] GameObject noReqDevicePrompt;

    [SerializeField] float startingRaceDelay = 3.0f;
    private float startingRaceTimer = 0.0f;

    private bool hiddenUI = false;

    private bool noValidDevice = true;

    public UnityEvent ReturnEvent;

    void Start()
    {
        if (ReturnEvent == null)
        {
            ReturnEvent = new UnityEvent();
        }
    }

    void Awake()
    {
        //  initialize state of each menu canvas/panel
        mainMenuCanvas.SetActive(true);
        joinScreenCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        controlPanel.SetActive(false);
        creditsPanel.SetActive(false);
        noReqDevicePrompt.SetActive(false);

        StartCoroutine(AudioManager.instance.PlayMusicIntro(AudioManager.instance.mainMenuIntro, AudioManager.instance.mainMenuLoop));
    }

    void OnEnable()
    {
        var devices = InputSystem.devices.ToArray();

        if (devices.Length != 0)
        {
            noValidDevice = false;
        }

        if (noValidDevice)
        {
            noReqDevicePrompt.SetActive(noValidDevice);
            ToggleActiveButtons();
        }
    }

    void Update()
    {

        if (noValidDevice)
        {
            if (InputSystem.devices.Count != 0)
            {
                noValidDevice = false;

                noReqDevicePrompt.SetActive(noValidDevice);
                ToggleActiveButtons();
            }
        }

        else
        {
            if (InputSystem.devices.Count == 0)
            {
                noValidDevice = true;

                noReqDevicePrompt.SetActive(noValidDevice);
                ToggleActiveButtons();
            }
        }


        if (!hiddenUI)
        {
            SplitScreenUIHandler.instance.HideUI();
            hiddenUI = true;
        }

        //  check if the cancel input is triggered
        if (uiInput.cancel.ToInputAction().triggered)
        {
            //  return to previous screen
            Return();
        }

        if (joinScreenCanvas.activeInHierarchy)
        {
            bool allReady = true;

            int joined = 0;

            foreach (JoinPanel panel in GameManager.instance.joinPanels)
            {
                if (panel.joined)
                {
                    joined++;
                }

                if (panel.ready == false && panel.joined)
                {
                    allReady = false;
                }
            }

            if (joined == 0)
            {
                allReady = false;
            }

            if (allReady)
            {
                startingRaceTimer += Time.deltaTime;

                startText.text = "Starting in: " + ((int)startingRaceDelay - (int)startingRaceTimer);

                if (startingRaceTimer >= startingRaceDelay)
                {
                    Go();
                }
            }
            else
            {
                startText.text = "Not All Players Ready";
                startingRaceTimer = 0.0f;

            }
        }
    }

    //  Method to be called when the play button is triggered in the main menu
    public void Play()
    {
        //  flip state of the main menu and join screen canvases
        mainMenuCanvas.SetActive(false);
        joinScreenCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startButton);
        PlayerInputManager.instance.EnableJoining();
        //  assigns the number of connected gamepads
        connectedControllers = Gamepad.all.Count;

        for (int i = 0; i < GameManager.instance.playerChairSelections.Length; i++)
        {
            GameManager.instance.playerChairSelections[i] = 0;
        }


        //PlayerInputManager.instance;.

        //  instantiate the panels:
        //  case 1: we have less controllers connected than max players:
        if (connectedControllers <= MAX_PLAYERS)
        {
            for (int i = 0; i < connectedControllers; i++)
            {
                var panel = Instantiate(joinPanel, joinPanelGroup.transform);
                JoinPanel playerPanel = panel.GetComponent<JoinPanel>();

                //if ()
                {

                }

                playerPanel.portrait.sprite = characterPortraits[i];
                playerPanel.playerIndex = i;

                playerPanel.inputDevice = Gamepad.all[i];

                GameManager.instance.joinPanels.Add(playerPanel);
            }
        }

        //  case 2: we have more controllers connected than max players
        else
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                var panel = Instantiate(joinPanel, joinPanelGroup.transform);
                JoinPanel playerPanel = panel.GetComponent<JoinPanel>();

                playerPanel.portrait.sprite = characterPortraits[i];
                playerPanel.playerIndex = i;


                playerPanel.inputDevice = Gamepad.all[i];


                GameManager.instance.joinPanels.Add(playerPanel);
            }
        }

        AudioManager.instance.SwapTrack(AudioManager.instance.selectLoop);

    }

    //  Method to be called when the credits button is triggered in the main menu
    public void Credits()
    {
        logo.SetActive(false);
        ToggleActiveButtons();

        AudioManager.instance.SwapTrack(AudioManager.instance.optionsLoop);

        creditsPanel.SetActive(true);
    }

    //  Method to be called when the controls button is triggered in the main menu
    public void Controls()
    {
        logo.SetActive(false);
        ToggleActiveButtons();

        AudioManager.instance.SwapTrack(AudioManager.instance.optionsLoop);

        controlPanel.SetActive(true);
    }
    
    //  Method to be called when the quit button is triggered in the main menu
    public void Quit()
    {
        Application.Quit();
    }

    //  Method to be called when the B button is pressed on the controller while in a panel/canvas
    public void Return()
    {
        //  if returning from the control panel
        if (controlPanel.activeInHierarchy)
        {
            controlPanel.SetActive(false);
            ToggleActiveButtons();
            AudioManager.instance.SwapTrack(AudioManager.instance.mainMenuLoop);
        }

        //  if returning from the credits panel
        else if (creditsPanel.activeInHierarchy)
        {
            creditsPanel.SetActive(false);
            ToggleActiveButtons();
            AudioManager.instance.SwapTrack(AudioManager.instance.mainMenuLoop);
        }

        //  if returning from the join screen
        else if (joinScreenCanvas.activeInHierarchy)
        {
            bool anyReady = false;

            foreach (JoinPanel panel in GameManager.instance.joinPanels)
            {
                if (panel.ready)
                {
                    anyReady = true;
                }
            }

            if (!anyReady)
            {
                joinScreenCanvas.SetActive(false);
                mainMenuCanvas.SetActive(true);
                PlayerInputManager.instance.DisableJoining();
                EventSystem.current.SetSelectedGameObject(mainMenuButtons[0]);

                foreach (Transform child in joinPanelGroup.transform)
                {
                    Destroy(child.gameObject);
                }

                GameManager.instance.joinPanels.Clear();

                foreach (PlayerInput input in GameManager.instance.playerInputs)
                {
                    Destroy(input.gameObject.transform.parent.transform.parent.gameObject);
                }

                for (int i = 0; i < GameManager.instance.playerChairSelections.Length; i++)
                {
                    GameManager.instance.playerChairSelections[i] = 0;
                }

                GameManager.instance.playerInputs.Clear();
                GameManager.instance.playerControlSchemes.Clear();


                AudioManager.instance.SwapTrack(AudioManager.instance.mainMenuLoop);
            }
        }
        ReturnEvent.Invoke();
        logo.SetActive(true);
    }

    //  method for flipping the active state of each button in the mainMenuButtons list
    public void ToggleActiveButtons()
    {
        foreach (GameObject button in mainMenuButtons)
        {
            button.SetActive(!button.activeInHierarchy);
        }
    }

    public void Go()
    {
        bool allReady = true;

        int joined = 0;

        foreach (JoinPanel panel in GameManager.instance.joinPanels)
        {
            if (panel.joined)
            {
                joined++;
            }

            if (panel.ready == false && panel.joined)
            {
                allReady = false;
            }
        }

        if (joined == 0)
        {
            allReady = false;
        }

        if (allReady)
        {
            GameManager.instance.LoadGameTest();
        }
    }

    public void Cycle()
    {
        AudioManager.instance.PlayMenuCycling();
    }

    public void Select()
    {
        if (EventSystem.current.currentSelectedGameObject == startButton)
        {
            bool allReady = true;

            foreach (JoinPanel panel in GameManager.instance.joinPanels)
            {
                if (!panel.ready && panel.joined)
                {
                    allReady = false;
                }
            }

            if (allReady)
            {
                AudioManager.instance.PlayMenuSelect();
            }

            else
            {
                AudioManager.instance.PlayNotReadySound();
            }
        }
        
        else
        {
            AudioManager.instance.PlayMenuSelect();
        }
    }
}
