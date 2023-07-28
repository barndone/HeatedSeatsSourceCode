using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Events;

public class JoinPanel : MonoBehaviour
{
    public bool joined = false;
    public bool ready = false;
    public int chairIndex = 0;

    public int playerIndex = 0;

    public Image portrait;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI readyText;

    public PlayerInput playerInput;
    public InputDevice inputDevice;

    public TextMeshProUGUI description;

    //  public List<GameObject> weightBars = new List<GameObject>();
    //  public List<GameObject> stabilityBars = new List<GameObject>();
    //  public List<GameObject> maneuverabilityBars = new List<GameObject>();
    //  
    //  public List<int> weightValues = new List<int>();
    //  public List<int> stabilityValues = new List<int>();
    //  public List<int> maneuverabilityValues = new List<int>();

    public List<string> descriptionStrings = new List<string>();

    private bool updatePanel = false;

    [SerializeField] float cycleDelay = 0.5f;
    private float cycleTimer = 0.0f;

    public bool forceMove = false;

    private float previousNavValue = 0.0f;


    private InputSystemUIInputModule uiInput;
    
    private void Awake()
    {
        uiInput = GetComponent<InputSystemUIInputModule>();
        cycleTimer = cycleDelay;
    }

    void Update()
    {
        if (joined)
        {
            if (!portrait.gameObject.activeSelf)
            {
                portrait.gameObject.SetActive(true);
            }

            if (updatePanel)
            {
                switch (chairIndex)
                {
                    case 0:
                        promptText.text = "Stool";
                        description.text = descriptionStrings[chairIndex];
                        //  UpdateStatVis(chairIndex);

                        break;
                    case 1:

                        promptText.text = "Office Chair";
                        description.text = descriptionStrings[chairIndex];

                        //  UpdateStatVis(chairIndex);

                        break;
                    case 2:

                        promptText.text = "Gaming Chair";
                        description.text = descriptionStrings[chairIndex];

                        //  UpdateStatVis(chairIndex);

                        break;
                    case 3:

                        promptText.text = "Lounge Chair";
                        description.text = descriptionStrings[chairIndex];

                        //  UpdateStatVis(chairIndex);

                        break;
                }

                switch (ready)
                {
                    case true:
                        readyText.text = "Ready!";
                        break;
                    case false:
                        readyText.text = "Press 'A' to Ready Up!";
                        break;
                }

                updatePanel = false;
            }
        }

        else
        {
            promptText.text = "Press 'A' to Join";

            if (description.gameObject.activeSelf)
            {
                description.gameObject.SetActive(false);
            }

            if (portrait.gameObject.activeSelf)
            {
                portrait.gameObject.SetActive(false);
            }
        }

        if (playerInput != null)
        {
            if (playerInput.uiInputModule == null)
            {
                playerInput.uiInputModule = uiInput;
                playerInput.uiInputModule.move.ToInputAction().canceled += ReleaseNavigation;
            }

            if (cycleTimer >= cycleDelay || forceMove && !ready)
            {
                cycleTimer = 0.0f;
                if (playerInput.uiInputModule.move.ToInputAction().triggered || playerInput.uiInputModule.move.ToInputAction().inProgress && !ready)
                {
                    AudioManager.instance.PlayMenuCycling();

                    if (!forceMove)
                    {
                        if (playerInput.uiInputModule.move.ToInputAction().ReadValue<Vector2>().x > 0.0f)
                        {
                            chairIndex++;

                            if (chairIndex >= 4)
                            {
                                chairIndex = 0;
                            }
                        }

                        else if (playerInput.uiInputModule.move.ToInputAction().ReadValue<Vector2>().x < 0.0f)
                        {
                            chairIndex--;

                            if (chairIndex < 0)
                            {
                                chairIndex = 3;

                            }
                        }
                    }

                }

                else if (forceMove && !ready)
                {
                    if (previousNavValue > 0.0f)
                    {
                        chairIndex++;

                        if (chairIndex >= 4)
                        {
                            chairIndex = 0;
                        }
                    }

                    else if (previousNavValue < 0.0f)
                    {
                        chairIndex--;

                        if (chairIndex < 0)
                        {
                            chairIndex = 3;

                        }
                    }

                    forceMove = false;
                }

                GameManager.instance.playerChairSelections[playerIndex] = chairIndex;

                updatePanel = true;
            }

            else
            {
                cycleTimer += Time.deltaTime;
            }

            if (playerInput.uiInputModule.cancel.ToInputAction().triggered && ready)
            {
                ready = false;
                updatePanel = true;

            }

            if (playerInput.actions["Select"].triggered && !ready)
            {
                ready = true;
                updatePanel = true;

            }

            if (playerInput.uiInputModule.move.ToInputAction().triggered || playerInput.uiInputModule.move.ToInputAction().inProgress)
            {
                previousNavValue = playerInput.uiInputModule.move.ToInputAction().ReadValue<Vector2>().x;
            }
        }
    }

    public void PairPlayerToPanel(PlayerInput player)
    {
        playerInput = player;
        inputDevice = player.devices[0];
        joined = true;
        description.gameObject.SetActive(true);
        updatePanel = true;
        //  UpdateStatVis(chairIndex);
    }

    public void ReleaseNavigation(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            forceMove = true;
        }
    }

    //  public void UpdateStatVis(int index)
    //  {
    //      for (int i = 0; i < weightBars.Count; i++)
    //      {
    //          if (i < weightValues[index])
    //          {
    //              weightBars[i].SetActive(true);
    //          }
    //          else
    //          {
    //              weightBars[i].SetActive(false);
    //          }
    //      }
    //  
    //      for (int i = 0; i < stabilityBars.Count; i++)
    //      {
    //          if (i < stabilityValues[index])
    //          {
    //              stabilityBars[i].SetActive(true);
    //          }
    //          else
    //          {
    //              stabilityBars[i].SetActive(false);
    //          }
    //      }
    //  
    //      for (int i = 0; i < maneuverabilityBars.Count; i++)
    //      {
    //          if (i < maneuverabilityValues[index])
    //          {
    //              maneuverabilityBars[i].SetActive(true);
    //          }
    //          else
    //          {
    //              maneuverabilityBars[i].SetActive(false);
    //          }
    //      }
    //  }

}
