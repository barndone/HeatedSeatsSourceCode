using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using Cinemachine;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [Header("Game States")]
    [Tooltip("Is the race still going?")]
    public bool RaceOngoing = false;
    [Tooltip("Is the race ending?")]
    public bool RaceEnding = false;
    [Tooltip("Is there a wish to pause the game?")]
    public bool pauseWish = false;
    [Tooltip("Is the game currently paused?")]
    public bool paused;
    //  is the game currently loading (used for load screens)
    private bool currentlyLoading = false;
    //  is the GO portion of the timer over?
    private bool goOver = false;
    //  is the control reminder prompt active at the start of the race?
    private bool controllsPromptActive;
    //  did the race just load?
    private bool raceJustLoaded = true;
    //  have the results been loaded?
    private bool resultsLoaded = false;
    private bool goStart = false;

    [Space(10)]
    [Header("Timers")]
    [Tooltip("Length of the countdown")]
    public float preRaceTimer;
    [Tooltip("Time spent in the race")]
    public float raceTimer;
    [Tooltip("Length of the delay for a race to end when all but one player have crossed the finish line.")]
    [SerializeField] float raceOverDelay = 20f;
    //  internal timers
    public float raceOverTimer = 0;
    private float goDelay = 1.0f;
    private float timer;

    [SerializeField] List<Color> minimapColors = new List<Color> ();
    public List<PlayerInput> playerInputs = new List<PlayerInput> ();
    public List<InputDevice> inputDevices = new List<InputDevice> ();
    public List<string> playerControlSchemes = new List<string>();
    public List<Rect> playerCameraRects = new List<Rect> ();
    public List<JoinPanel> joinPanels = new List<JoinPanel> ();

    //  hard cap array size to the number of players
    public int[] playerChairSelections = new int [4];

    static public GameManager instance;
    [SerializeField] List<GameObject> playerOnePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> playerTwoPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> playerThreePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> playerFourPrefabs = new List<GameObject>();

    public Camera endScrn;


    //  list containing each player racing
    public List<ChairController> players = new List<ChairController> ();
    public Dictionary<ChairController, float> completionTimes = new Dictionary<ChairController,float> ();
    private int numberOfFinishedPlayers = 0;

    [SerializeField] float updateRateSeconds = 4.0f;
    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;

    public TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI FPSCounter;
    [SerializeField] GameObject controllsPanel;
    public InputSystemUIInputModule inputListener;
    [SerializeField] List<TextMeshProUGUI> placementStrings = new List<TextMeshProUGUI> ();
    [SerializeField] List<TextMeshProUGUI> pausePlacementStrings = new List<TextMeshProUGUI>();
    [SerializeField] GameObject resultsScreen;


    //  max number of cans allowed to be spawned on a level, after 100-> recycle prefabs
    public int MAX_SPAWNED_CANS = 100;
    public Queue<GameObject> spawnedCans = new Queue<GameObject>();
    public bool stopSpawning = false;

    [Tooltip("Update this to increase or decrease the amount of real world time (seconds) between list iterations for placement updating.")]
    [SerializeField] float placementIterationDelay = 0.5f;
    private float placementIterationTimer = 0.0f;

    private bool raceMusicStarted = false;


    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        else
        {
            Debug.LogWarning("ERROR: Second Game Manager Found in Scene");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnRaceLoaded;
    }

    private void Start()
    {

    }

    private void Update()
    {
        frameCount++;
        dt += Time.unscaledDeltaTime;
        if (dt > 1.0 / updateRateSeconds)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRateSeconds;
        }

        FPSCounter.text = "FPS: " + Mathf.Round(fps).ToString();

        //  if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene") && raceJustLoaded)
        //  {
        //      SplitScreenUIHandler.instance.HideUI();
        //  
        //      var cameras = Camera.allCameras;
        //  
        //      foreach (var cam in cameras)
        //      {
        //          if (cam.gameObject.CompareTag("EndCamera"))
        //          {
        //              endScrn = cam;
        //          }
        //      }
        //      
        //      if (endScrn != null)
        //      {
        //          endScrn.gameObject.SetActive(false);
        //      }
        //  
        //     //    controllsPromptActive = true;
        //     //    controllsPanel.SetActive(true);
        //      if (players.Count == 3)
        //      {
        //          SplitScreenUIHandler.instance.ThreePlayerFix();
        //      }
        //      raceJustLoaded = false;
        //      goStart = false;
        //      inputListener = controllsPanel.GetComponent<InputSystemUIInputModule>();
        //  
        //      raceTimer = 0.0f;
        //  
        //      foreach (ChairController player in players)
        //      {
        //          player.ToggleRagdoll(player.fallen);
        //          player.previousWaypoint = 0;
        //          player.activeWaypointIndex = 0;
        //      }
        //  
        //  }

        // if (controllsPromptActive)
        // {
        //     if (inputListener.submit.ToInputAction().triggered)
        //     {
        //         controllsPanel.SetActive(false);
        //         controllsPromptActive = false;
        //         StartCoroutine(AudioManager.instance.RaceStart());
        //     }
        // }

        //  if the race hasn't started
        if (!RaceOngoing && SceneManager.GetActiveScene() != SceneManager.GetSceneByName("MainMenu") &&
            !RaceEnding && !controllsPromptActive && !raceJustLoaded && !paused)
        {

            timerText = SplitScreenUIHandler.instance.sharedTimer.GetComponent<TextMeshProUGUI>();
            timerText.gameObject.SetActive(true);

            //  if the timer is greater than 0
            if (timer > 0.0f)
            {
                //  decrement the timer
                timer -= Time.deltaTime;

                if (timer < 1.0f)
                {
                    if (!goStart)
                    {
                        timerText.text = "Go!";
                        //  the race has started  
                        RaceOngoing = true;
                        SplitScreenUIHandler.instance.InitializeUI();

                        if (players.Count != 3)
                        {
                            SplitScreenUIHandler.instance.threePlayerFix.SetActive(false);
                        }

                        goStart = true;

                        foreach (var player in players)
                        {
                            player.canCharge = true;
                        }
                    }
                }

                else
                {
                    timerText.text = ((int)timer).ToString();
                }
            }

            if (!raceMusicStarted)
            {
                StartCoroutine(AudioManager.instance.RaceStart());
                raceMusicStarted = true;
            }
        }

        else if (RaceOngoing && SceneManager.GetActiveScene() != SceneManager.GetSceneByName("MainMenu") && 
            !RaceEnding && !controllsPromptActive && !raceJustLoaded && !paused)
        {
            if (goDelay > 0.0f)
            {
                goDelay -= Time.deltaTime;
            }

            else
            {
                if (!goOver)
                {
                    goOver = true;
                    if (players.Count < 2)

                    {
                        SplitScreenUIHandler.instance.sharedTimer.SetActive(false);
                        timerText = SplitScreenUIHandler.instance.soloTimer.GetComponent<TextMeshProUGUI>();
                        timerText.gameObject.SetActive(true);
                    }
                }
                timerText.text = FormatTimer(raceTimer);
            }

            //  update the time spent in the race
            raceTimer += Time.deltaTime;

            placementIterationTimer += Time.deltaTime;

            if (placementIterationTimer >= placementIterationDelay)
            {
                //  if the race is ongoing: order the list of players by current lap (descending), active waypoint (descending), distance to current waypoint 
                var sortedPlayers = players.
                    OrderByDescending(x => x.lapCounter).
                    ThenByDescending(x => x.activeWaypointIndex).
                    ThenBy(x => x.distanceToNextWP);

                players = sortedPlayers.ToList<ChairController>();




                //  assign number of finished players to 0
                numberOfFinishedPlayers = 0;

                //  iterate through the list of players
                foreach (ChairController player in players)
                {
                    //  update placement text for each player based off of their position within the list
                    switch (players.IndexOf(player))
                    {
                        case 0:
                            //player.placementText.text = "1st";
                            player.curPlacement = players.IndexOf(player);
                            break;
                        case 1:
                            //player.placementText.text = "2nd";
                            player.curPlacement = players.IndexOf(player);
                            break;
                        case 2:
                            //player.placementText.text = "3rd";
                            player.curPlacement = players.IndexOf(player);
                            break;
                        case 3:
                            //player.placementText.text = "4th";
                            player.curPlacement = players.IndexOf(player);
                            break;
                    }


                    //  if that player is finished racing
                    if (player.finishedRacing)
                    {
                        //  increment the count of finished players
                        numberOfFinishedPlayers++;
                    }

                }

                //  if the number of finished players is less than or equal to the count -1
                if (numberOfFinishedPlayers >= players.Count - 1)
                {
                    //  check that the number of finished players is not zero
                    if (numberOfFinishedPlayers != 0)
                    {
                        //  mark that the race is ending
                        RaceEnding = true;
                        //  assign the race over timer to the delay assigned in inspector
                        raceOverTimer = raceOverDelay;
                    }
                }
                //  reset the timer
                placementIterationTimer = 0.0f;
            }
        }

        else if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("MainMenu") && 
            RaceEnding && !controllsPromptActive && !raceJustLoaded && !paused)
        {

            if (numberOfFinishedPlayers <= players.Count - 1)
            {
                //  if the timer is greater than 0
                if (raceOverTimer > 0.0f)
                {
                    //  update the time spent in the race (since we aren't over yet)
                    raceTimer += Time.deltaTime;
                    timerText.text = FormatTimer(raceTimer);

                    //  decrement the timer
                    raceOverTimer -= Time.deltaTime;

                    //  //  if the race is ongoing: order the list of players by current lap (descending), active waypoint (descending), distance to current waypoint 
                    //  var sortedPlayers = players.
                    //      OrderByDescending(x => x.lapCounter).
                    //      ThenByDescending(x => x.activeWaypointIndex).
                    //      ThenBy(x => x.completionTime).
                    //      ThenBy(x => x.distanceToNextWP);
                    //  
                    //  players = sortedPlayers.ToList<ChairController>();

                    numberOfFinishedPlayers = 0;

                    //  iterate through the list of players
                    foreach (ChairController player in players)
                    {
                        //  does not need to be done when there is only ONE player left racing, placements are decided
                        //  //  update placement text for each player based off of their position within the list
                        //  switch (players.IndexOf(player))
                        //  {
                        //      case 0:
                        //          //player.placementText.text = "1st";
                        //          player.curPlacement = players.IndexOf(player);
                        //          break;
                        //      case 1:
                        //          //player.placementText.text = "2nd";
                        //          player.curPlacement = players.IndexOf(player);
                        //          break;
                        //      case 2:
                        //          //player.placementText.text = "3rd";
                        //          player.curPlacement = players.IndexOf(player);
                        //          break;
                        //      case 3:
                        //          //player.placementText.text = "4th";
                        //          player.curPlacement = players.IndexOf(player);
                        //          break;
                        //  }


                        //  if that player is finished racing
                        if (player.finishedRacing)
                        {
                            //  increment the count of finished players
                            numberOfFinishedPlayers++;
                        }

                    }

                }

                //  otherwise
                else
                {
                    //  the race is no longer ongoing  
                    RaceOngoing = false;
                    SplitScreenUIHandler.instance.HideUI();

                }
            }

            else
            {
                RaceOngoing = false;
                SplitScreenUIHandler.instance.HideUI();
            }
        }

        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("MainMenu") && RaceEnding && !RaceOngoing)
        {
            if (!resultsLoaded)
            {
                //  if the race is ongoing: order the list of players by completion time (ascending) current lap (descending), active waypoint (descending), distance to current waypoint 
                var sortedPlayers = players.
                    OrderByDescending(x => x.lapCounter).
                    ThenByDescending(x => x.activeWaypointIndex).
                    ThenBy(x => x.completionTime);
                    //ThenBy(x => x.distanceToNextWP);

                players = sortedPlayers.ToList<ChairController>();

                UpdatePlacementResults();
                resultsLoaded = true;

                if (endScrn != null)
                {
                    endScrn.gameObject.SetActive(true);
                }

                resultsScreen.SetActive(true);
            }
        }

        if (paused)
        {
            //  idk some shit
        }
    }

    private void LateUpdate()
    {
        if (pauseWish)
        {

            paused = true;
            SplitScreenUIHandler.instance.ToggleInGameUI(false);
            //  SplitScreenUIHandler.instance.pauseScrn.SetActive(true);
            pauseWish = false;
            UpdatePlacementResults();

            //  if (endScrn != null)
            //  {
            //      endScrn.gameObject.SetActive(true);
            //  }

            foreach (ChairController chair in players)
            {
                chair.prePauseVelocity = chair.rb.velocity;
                chair.rb.isKinematic = true;
                chair.rollingSource.mute = true;
            }

            AudioManager.instance.sfxSource.mute = false;

            AudioManager.instance.PlayPauseSound();
            AudioManager.instance.SwapTrack(AudioManager.instance.pausedLoop);
        }
    }

    public void Resume()
    {
        paused = false;
        SplitScreenUIHandler.instance.ToggleInGameUI(true);
        //  SplitScreenUIHandler.instance.pauseScrn.SetActive(false);

        foreach (ChairController chair in players)
        {
            chair.rb.velocity = chair.prePauseVelocity;
            chair.rb.isKinematic = false;
            chair.rollingSource.mute = false;
        }

        //  if (endScrn != null)
        //  {
        //      endScrn.gameObject.SetActive(false);
        //  }

        AudioManager.instance.sfxSource.mute = true;
        AudioManager.instance.SwapTrack(AudioManager.instance.raceLoop);
    }

    public void AddPlayer(PlayerInput player)
    {
        if (!currentlyLoading)
        {
            playerInputs.Add(player);
            inputDevices.Add(player.devices[0]);
            playerControlSchemes.Add(player.currentControlScheme);
            joinPanels[player.playerIndex].PairPlayerToPanel(player);
        }
    }

    public void RemovePlayer(PlayerInput player)
    {
        if (!currentlyLoading)
        {
            playerInputs.Remove(player);
        }
    }

    public void InstantiateJoinedPlayers(List<Transform> spawns)
    {
        players.Clear();

        //  iterate through the list of joined players
        for (int i = 0; i < playerInputs.Count; i++)
        {
            PlayerInput input = null;
            if (i == 0)
            {
                input = PlayerInput.Instantiate(playerOnePrefabs[playerChairSelections[i]], i, playerControlSchemes[i], i, inputDevices[i]);

            }
            else if (i == 1)
            {
                input = PlayerInput.Instantiate(playerTwoPrefabs[playerChairSelections[i]], i, playerControlSchemes[i], i, inputDevices[i]);

            }
            else if (i == 2)
            {
                input = PlayerInput.Instantiate(playerThreePrefabs[playerChairSelections[i]], i, playerControlSchemes[i], i, inputDevices[i]);

            }
            else
            {
                input = PlayerInput.Instantiate(playerFourPrefabs[playerChairSelections[i]], i, playerControlSchemes[i], i, inputDevices[i]);
            }

            playerInputs[i] = input;
            inputDevices[i] = input.devices[0];
            playerControlSchemes[i] = input.currentControlScheme;

            input.gameObject.name = "Player " + (i + 1);
            players.Add(input.gameObject.GetComponent<ChairController>());
            players[i].playerIndex = i;
            players[i].chairIndex = playerChairSelections[i];

            players[i].miniMapIndicator.color = minimapColors[i];
            players[i].rb.Sleep();
            players[i].transform.position = spawns[i].transform.position;
            players[i].rb.WakeUp();
        }

        // set up the player cameras for split screen
        for (int i = 0; i < playerCameraRects.Count; i++)
        {
            Camera.allCameras[i].rect = playerCameraRects[i];

            //  given the player index-
            switch (i)
            {
                //  player 1
                case 0:
                    //  assign the corresponding layer to this camera
                    Camera.allCameras[i].gameObject.layer = 6;

                    //  update the layer the camera will look at, update the follow target, look at target
                    Camera.allCameras[i].cullingMask |=   1 << LayerMask.NameToLayer("P1");
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P2"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P3"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P4"));
                    break;
                //  player 2
                case 1:
                    //  assign the corresponding layer to this camera
                    Camera.allCameras[i].gameObject.layer = 7;

                    //  update the layer the camera will look at, update the follow target, look at target
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P1"));
                    Camera.allCameras[i].cullingMask |=   1 << LayerMask.NameToLayer("P2");
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P3"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P4"));
                    break;
                //  player 3
                case 2:
                    //  assign the corresponding layer to this camera
                    Camera.allCameras[i].gameObject.layer = 8;

                    //  update the layer the camera will look at, update the follow target, look at target
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P1"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P2"));
                    Camera.allCameras[i].cullingMask |=   1 << LayerMask.NameToLayer("P3");
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P4"));
                    break;
                //  player 4
                case 3:
                    //  assign the corresponding layer to this camera
                    Camera.allCameras[i].gameObject.layer = 9;

                    //  update the layer the camera will look at, update the follow target, look at target
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P1"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P2"));
                    Camera.allCameras[i].cullingMask &= ~(1 << LayerMask.NameToLayer("P3"));
                    Camera.allCameras[i].cullingMask |=   1 << LayerMask.NameToLayer("P4");
                    break;
            }

            CinemachineVirtualCamera vCam = Camera.allCameras[i].gameObject.GetComponent<CinemachineVirtualCamera>();

            vCam.Follow = players[i].rotator.transform;
            vCam.LookAt = players[i].cameraTarget.transform;
        }
    }

    public void LoadGameTest()
    {
        PlayerInputManager.instance.DisableJoining();

        //  if our camerarect list does not have the same amount of entries as the players list
        if (playerCameraRects.Count != playerInputs.Count)
        {
            //  reset the list
            playerCameraRects.Clear();

            //  add the rects to the list
            foreach (PlayerInput player in playerInputs)
            {
                playerCameraRects.Add(player.camera.rect);
            }
        }

        SceneManager.LoadScene("SampleScene");
        currentlyLoading = true;
        raceJustLoaded = true;
        RaceEnding = false;
        RaceOngoing = false;
        resultsLoaded = false;
        timer = preRaceTimer;
        resultsScreen.SetActive(false);
        endScrn.gameObject.SetActive(false);
        SplitScreenUIHandler.instance.pauseScrn.SetActive(false);
        paused = false;
        completionTimes.Clear();
        goOver = false;
        stopSpawning = false;
        spawnedCans.Clear();
        raceMusicStarted = false;
    }

    public string FormatTimer(float time)
    {
        var ms = (time % 1).ToString(".00");
        var ss = ((int)(time % 60)).ToString("00");
        var mm = (Mathf.FloorToInt(time / 60) % 60).ToString("00");

        return mm + ":" + ss + ms;
    }

    public void UpdatePlacementResults()
    {
        //  iterate through the list of players
        for (int i = 0; i < players.Count; i++)
        {
            switch (i)
            {
                case 0:
                    pausePlacementStrings[i].text = "1st - ";
                    placementStrings[i].text = "1st - ";
                    break;
                case 1:
                    pausePlacementStrings[i].text = "2nd - ";
                    placementStrings[i].text = "2nd - ";
                    break;
                case 2:
                    pausePlacementStrings[i].text = "3rd - ";
                    placementStrings[i].text = "3rd - ";
                    break;
                case 3:
                    pausePlacementStrings[i].text = "4th - ";
                    placementStrings[i].text = "4th - ";
                    break;
            }


            if (!RaceOngoing)
            {
                placementStrings[i].text += GeneratePlacementString(players[i]);

            }
            else
            {
                pausePlacementStrings[i].text += GeneratePlacementString(players[i]);
            }
        }
    }

    public string GeneratePlacementString(ChairController player)
    {
        float time;
        if (!RaceOngoing)
        {
            if (completionTimes.TryGetValue(player, out time))
            {
                return "Player " + (player.playerIndex + 1) + " in " + FormatTimer(time);

            }
            else
            {
                return "Player " + (player.playerIndex + 1) + " DNF..";
            }
        }

        else
        {
            return "Player " + (player.playerIndex + 1);
        }
    }

    //
    //  Methods for the results/pause screen
    //

    public void ReturnToMenu()
    {

        resultsScreen.SetActive(false);
        SplitScreenUIHandler.instance.pauseScrn.SetActive(false);

        playerInputs.Clear();
        inputDevices.Clear();
        playerControlSchemes.Clear();
        playerCameraRects.Clear();
        joinPanels.Clear();
        stopSpawning = false;
        spawnedCans.Clear();
        currentlyLoading = false;
        paused = false;
        RaceOngoing = false;
        raceMusicStarted = false;
        resultsLoaded = false;
        RaceEnding = false;
        completionTimes.Clear();

        SceneManager.LoadScene("MainMenu");
    }

    public void OnRaceLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetSceneByName("SampleScene"))
        {
            SplitScreenUIHandler.instance.HideUI();

            var cameras = Camera.allCameras;

            foreach (var cam in cameras)
            {
                if (cam.gameObject.CompareTag("EndCamera"))
                {
                    endScrn = cam;
                }
            }

            if (endScrn != null)
            {
                endScrn.gameObject.SetActive(false);
            }

            //    controllsPromptActive = true;
            //    controllsPanel.SetActive(true);
            if (players.Count == 3)
            {
                SplitScreenUIHandler.instance.ThreePlayerFix();
            }
            raceJustLoaded = false;
            goStart = false;
            inputListener = controllsPanel.GetComponent<InputSystemUIInputModule>();

            raceTimer = 0.0f;

            foreach (ChairController player in players)
            {
                player.ToggleRagdoll(player.fallen);
                player.previousWaypoint = 0;
                player.activeWaypointIndex = 0;
            }
        }
    }
}
