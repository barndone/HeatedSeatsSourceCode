using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplitScreenUIHandler : MonoBehaviour
{
    //
    //  References to the gameObjects holding each separate UI Canvas
    //

    public List<GameObject> fourPlayerSplitUI = new List<GameObject>();
    public List<GameObject> twoPlayerSplitUI = new List<GameObject>();

    public List<GameObject> hideUIHelper = new List<GameObject>();

    public GameObject soloUI;
    public GameObject controllsReminder;
    public GameObject endScrn;
    public GameObject pauseScrn;

    public GameObject soloTimer;
    public GameObject sharedTimer;

    public GameObject sharedUI;

    public GameObject threePlayerFix;
    public List<GameObject> activeElements = new List<GameObject>();

    [SerializeField]
    private float transitionTime = 0.25f;

    private float currentAlpha;
    private float targetAlpha;

    private bool transitioning = false;
    private bool activeTarget;

    [SerializeField]
    private GameObject fadePanel;
    private Image fadePanelImage;

    static public SplitScreenUIHandler instance;

    //  uhhh does this work?
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        else
        {
            Debug.LogWarning("ERROR: Second UI Handler Found in Scene");
            Destroy(gameObject);
        }

        //DontDestroyOnLoad(this);
    }

    public void InitializeUI()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(true);
        }

        //  are we in the race scene
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            controllsReminder.SetActive(false);
            endScrn.SetActive(false);
            pauseScrn.SetActive(false);

            //  if so, do we have less than or equal to 2 players
            if (GameManager.instance.players.Count <= 2)
            {
                

                //  iterate through and disable the four player setup
                foreach (GameObject go in fourPlayerSplitUI)
                {
                    go.SetActive(false);
                }

                if (GameManager.instance.players.Count == 1)
                {
                    sharedUI.SetActive(false);

                    sharedTimer.SetActive(true);
                    soloTimer.SetActive(false);
                    //  iterate through and disable the two player setup
                    foreach (GameObject go in twoPlayerSplitUI)
                    {
                        go.SetActive(false);
                    }

                    //  initialize a list of gameobjects
                    List<GameObject> children = new List<GameObject>();

                    //  for each child attached to each game object in the two player split ui
                    foreach (Transform child in soloUI.transform)
                    {
                        //  add it to the list of children
                        children.Add(child.gameObject);
                    }

                    GameObject meterParent = children[^2];
                    GameObject placementParent = children[0];

                    //  assign the fields needed to the player
                    GameManager.instance.players[0].wallKickStamp   =   children[^1].transform.GetChild(0).GetComponent<Image>();
                    GameManager.instance.players[0].placementText   =   placementParent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    GameManager.instance.players[0].placementAnimator = placementParent.transform.GetChild(0).GetComponent<Animator>();
                    GameManager.instance.players[0].crownVis          = placementParent.transform.GetChild(2).gameObject;
                    GameManager.instance.players[0].lapText         =   children[1].GetComponent<TextMeshProUGUI>();
                    GameManager.instance.players[0].chargeMeterImg  =   meterParent.transform.GetChild(1).GetComponent<Image>();
                    GameManager.instance.players[0].balanceMeterImg =   meterParent.transform.GetChild(3).GetComponent<Image>();
                    GameManager.instance.players[0].wrongWayIndicator = children[3];

                    children[0].SetActive(true);
                    children[2].SetActive(true);
                }

                else
                {
                    soloUI.SetActive(false);

                    //  iterate through each player in the game:
                    for (int i = 0; i < GameManager.instance.players.Count; i++)
                    {
                        //  initialize a list of gameobjects
                        List<GameObject> children = new List<GameObject>();

                        //  for each child attached to each game object in the two player split ui
                        foreach (Transform child in twoPlayerSplitUI[i].transform)
                        {
                            //  add it to the list of children
                            children.Add(child.gameObject);
                        }

                        // //  assign the fields needed to the player
                        // GameManager.instance.players[i].balanceMeterImg = children[0].GetComponent<Image>();
                        // GameManager.instance.players[i].kickIndicator = children[1];
                        // GameManager.instance.players[i].placementText = children[2].GetComponent<TextMeshProUGUI>();
                        // GameManager.instance.players[i].lapText = children[4].GetComponent<TextMeshProUGUI>();
                        // GameManager.instance.players[i].chargeMeterImg = children[5].GetComponentInChildren<Image>();
                        // children[0].SetActive(true);
                        // children[2].SetActive(true);

                        GameObject meterParent = children[^2];
                        GameObject placementParent = children[0];

                        //  assign the fields needed to the player
                        GameManager.instance.players[i].wallKickStamp = children[^1].transform.GetChild(0).GetComponent<Image>();
                        GameManager.instance.players[i].placementText = placementParent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                        GameManager.instance.players[i].placementAnimator = placementParent.transform.GetChild(0).GetComponent<Animator>();
                        GameManager.instance.players[i].crownVis = placementParent.transform.GetChild(2).gameObject;
                        GameManager.instance.players[i].lapText = children[1].GetComponent<TextMeshProUGUI>();
                        GameManager.instance.players[i].chargeMeterImg = meterParent.transform.GetChild(1).GetComponent<Image>();
                        GameManager.instance.players[i].balanceMeterImg = meterParent.transform.GetChild(3).GetComponent<Image>();
                        GameManager.instance.players[i].chargeMeterImg.gameObject.SetActive(true);
                        GameManager.instance.players[i].placementText.gameObject.SetActive(true);
                        GameManager.instance.players[i].endOfRaceIndicator = children[2].GetComponent<TextMeshProUGUI>();
                        GameManager.instance.players[i].wrongWayIndicator = children[3];

                        GameManager.instance.players[i].endOfRaceIndicator.gameObject.SetActive(false);
                    }
                }
            }

            //  otherwise, we have more than 2
            else
            {
                soloUI.SetActive(false);

                //  iterate through and disable the two player setup
                foreach (GameObject go in twoPlayerSplitUI)
                {
                    go.SetActive(false);
                }

                //  iterate through each player in the game:
                for (int i = 0; i < GameManager.instance.players.Count; i++)
                {
                    //  initialize a list of gameobjects
                    List<GameObject> children = new List<GameObject>();

                    //  for each child attached to each game object in the two player split ui
                    foreach (Transform child in fourPlayerSplitUI[i].transform)
                    {
                        //  add it to the list of children
                        children.Add(child.gameObject);
                    }

                    GameObject meterParent = children[^2];
                    GameObject placementParent = children[0];

                    //  assign the fields needed to the player
                    GameManager.instance.players[i].wallKickStamp = children[^1].transform.GetChild(0).GetComponent<Image>();
                    GameManager.instance.players[i].placementText = placementParent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    GameManager.instance.players[i].placementAnimator = placementParent.transform.GetChild(0).GetComponent<Animator>();
                    GameManager.instance.players[i].crownVis = placementParent.transform.GetChild(2).gameObject;
                    GameManager.instance.players[i].lapText = children[1].GetComponent<TextMeshProUGUI>();
                    GameManager.instance.players[i].chargeMeterImg = meterParent.transform.GetChild(1).GetComponent<Image>();
                    GameManager.instance.players[i].balanceMeterImg = meterParent.transform.GetChild(3).GetComponent<Image>();
                    GameManager.instance.players[i].chargeMeterImg.gameObject.SetActive(true);
                    GameManager.instance.players[i].placementText.gameObject.SetActive(true);
                    GameManager.instance.players[i].endOfRaceIndicator = children[2].GetComponent<TextMeshProUGUI>();
                    GameManager.instance.players[i].wrongWayIndicator = children[3];


                    GameManager.instance.players[i].endOfRaceIndicator.gameObject.SetActive(false);
                }

                if (GameManager.instance.players.Count == 3)
                {
                    fourPlayerSplitUI[3].SetActive(false);
                }
            }
        }
    }

    public void HideUI()
    {
        foreach (GameObject go in hideUIHelper)
        {
            go.SetActive(false);
        }
    }

    public void ThreePlayerFix()
    {
        threePlayerFix.SetActive(true);
    }

    public void ToggleInGameUI(bool state)
    {
        activeTarget = state;

        if (state == false)
        {
            activeElements.Clear();

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.activeInHierarchy && !child.gameObject.CompareTag("FadePanel"))
                {
                    activeElements.Add(child.gameObject);
                }
            }
        }
        BeginTransition();
    }

    public void BeginTransition()
    {
        transitioning = true;
        targetAlpha = 1.0f;
        fadePanelImage = fadePanel.GetComponent<Image>();
    }

    void Update()
    {
        if (transitioning)
        {
            if (targetAlpha == 1.0f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, transitionTime * Time.deltaTime);

                fadePanelImage.color = new Color(fadePanelImage.color.r, fadePanelImage.color.g, fadePanelImage.color.b, currentAlpha);

                if (currentAlpha >= targetAlpha)
                {
                    targetAlpha = 0.0f;

                    foreach (GameObject go in activeElements)
                    {
                        go.SetActive(activeTarget);
                    }

                    if (GameManager.instance.endScrn != null)
                    {
                        GameManager.instance.endScrn.gameObject.SetActive(!activeTarget);
                        pauseScrn.SetActive(!activeTarget);
                    }
                }
            }

            else if (targetAlpha == 0.0f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, transitionTime * Time.deltaTime);

                fadePanelImage.color = new Color(fadePanelImage.color.r, fadePanelImage.color.g, fadePanelImage.color.b, currentAlpha);

                if (currentAlpha <= targetAlpha)
                {
                    transitioning = false;
                }
            }
        }
    }
}
