using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ChairController : MonoBehaviour
{    

    [Header("Movement Balancing")]
    [Tooltip("The maximum amount of time that this chair can charge a kick for, will be multiplied against the kick force to determine how powerful the kick is.")]
    [SerializeField] [Range(0.0f, 5.0f)] float chargeTimerMax = 2.0f;
    [Tooltip("The power used to determine how strong a kick is. Will be multiplied against the chargeTimer")]
    [SerializeField] float kickPower = 10.0f;
    [Tooltip("This modifier impacts how much stronger a wallkick will be than a normal kick")]
    [SerializeField] [Range(1.0f, 2.0f)] float wallKickModifier = 1.25f;
    [Tooltip("This multiplier impacts how fast the chair will slowdown while you are charging a kick")]
    [SerializeField] float chargingKickFrictionMultiplier = 2.0f;
    [Tooltip("This field is how long the timer will be after kicking before deceleration starts to kick in. (in seconds)")]
    [SerializeField] float decelerationDelay = 1.0f;
    [Tooltip("This field represents how long the delay is between wall kicks registering. (in seconds)")]
    [SerializeField] float wallKickedDelay = 1.0f;
    [Tooltip("This field represents how long the chair will remain on its side after the balance meter reaches 100. (in seconds)")]
    [SerializeField] float fallDelay = 2.0f;
    [Tooltip("The weight of the chair.\nImpacts the speed of the chair as well as how it affects other chairs in collisions.")]
    [Range(0.0f, 100.0f)] public float chairWeight;
    [Tooltip("The Stability of the chair.\nImpacts the magnitude of the balance changes to the chair.")]
    [Range(0.0f, 100.0f)] public float chairStability;
    [Tooltip("The Maneuverability of the chair.\nImpacts the power of the force applied to the chair when there is input detected from the balance stick.")]
    [Range(0.0f, 100.0f)] public float chairManeuverability;
    [Tooltip("This field dampens the change in balance so that all of the chairs do not immediately fall over.")]
    [SerializeField] float stabilityCoefficient = 1000.0f;
    [Tooltip("This field will impact how quickly stability degrades")]
    [SerializeField] float stabilityDegredationFactor = 1.0f;
    [Tooltip("This field represents the speed at which the player can aim")]
    public float rotationSpeed = 25f;
    [Tooltip("This field represents the effect of friction on the wheels")]
    [SerializeField][Range(0, 1)] float frictionCoefficient = 0.5f;
    [Tooltip("LowerThreshold for when the speed lines will start appearing!")]
    [SerializeField] float speedlineFloor = 20.0f;
    [Tooltip("This field represents the point at which the audio clip will change to the fast clip.\nAlso represents the velcotiy at which full opacity is reached for speedlines (1.0f)")]
    [SerializeField] float speedThreshold = 20.0f;
    [Tooltip("This parameter affects how much balance is restored after a wall kick")]
    [SerializeField] float balanceRecovery = 10.0f;
    [Tooltip("The grace period where you are able to wallkick after leaving a walls range")]
    [SerializeField] float coyoteTime = 0.2f;
    [Tooltip("The magnitude at which strafing will impact your instability")]
    [SerializeField] float strafeMultiplier = 1.0f;
    

    [Header("Catch-up mechanics")]
    [Tooltip("Instability Multiplier for if this player is in first")]
    [SerializeField] float firstPlaceMultiplier = 1.25f;
    [Tooltip("Instability Multiplier for if this player is in second")]
    [SerializeField] float secondPlaceMultiplier = 1.0f;
    [Tooltip("Instability Multiplier for if this player is in third")]
    [SerializeField] float thirdPlaceMultiplier = 0.75f;
    [Tooltip("Instability Multiplier for if this player is in fourth")]
    [SerializeField] float fourthPlaceMultiplier = 0.5f;

    [Space(10)]
    [Header("Player States")]
    [Tooltip("Is this chair in wall kick range?")]
    public bool wallKick = false;
    [Tooltip("Is this chair moving?")]
    public bool isMoving = false;
    [Tooltip("Is this chair able to kick?")]
    public bool canKick = true;
    [Tooltip("Has this chair fallen over?")]
    public bool fallen = false;
    [Tooltip("Has this chair performed a wall kick?")]
    public bool wallKicked = false;
    [Tooltip("Has this chair completed all the laps of the race?")]
    public bool finishedRacing => RaceHandler.instance != null ? lapCounter >= RaceHandler.instance.laps : false;    
    //  flag for starting deceleration
    private bool startDeceleration;
    //  internal flag for if we are charging a kick
    public bool chargingKick;
    //  internal flag to keep check if we are grounded
    private bool isGrounded = true;
    public bool canCharge = true;
    public bool decelerateWhileCharging = true;
    public bool facingWall = false;
    private bool speedLinesPlaying = false;
    private bool coyoteKickWindow;
    private bool fadeIn = true;
    [Tooltip("Reference to which type of chair this is (stool, gaming, office, or lounge.")]
    public int chairIndex;

    [Space(10)]
    [Header("References")]
    [Tooltip("Reference to the rigidbody motor of the chair.")]
    public Rigidbody rb;
    [Tooltip("Reference to the object that handles the aiming (rotation) of the chair/player.")]
    public GameObject rotator;
    //  [Tooltip("Reference to the gameobject representing the UI element showing the player can wall kick.")]
    //  public GameObject kickIndicator;
    [Tooltip("Reference to the UI element showing the current race position of the player.")]
    public TextMeshProUGUI placementText;
    [Tooltip("Reference to the sprite that is used to represent this player on the minimap.")]
    public SpriteRenderer miniMapIndicator;
    [Tooltip("Reference to the image used for the kick charging meter.")]
    public Image chargeMeterImg;
    [Tooltip("Reference to the image used for the chairs current instability.")]
    public Image balanceMeterImg;
    [Tooltip("Reference to the UI element showing the current lap the player is on")]
    public TextMeshProUGUI lapText;
    [Tooltip("Reference to the PlayerInput component for this player")]
    [SerializeField] private PlayerInput playerInput;
    //  reference to the action for kicking
    private InputAction kickAction;
    //  reference to the action for aiming
    public InputAction aimAction;
    //  reference to the action for balancing
    private InputAction balanceAction;
    //  reference to the action for pausing
    private InputAction pauseAction;
    //  reference to the action for wall kicking
    private InputAction wallKickAction;
    //  reference to the action for an alternative way of exiting the menu
    private InputAction returnAction;
    //  reference to the audiosource attached to this object
    public AudioSource rollingSource;
    public AudioSource sfx;
    [Tooltip("The layer used for checking if the player is grounded.")]
    [SerializeField] LayerMask groundMask;
    [Tooltip("Reference to the camera that is following this player.")]
    [SerializeField] GameObject chaseCam;
    public GameObject mesh;
    public GameObject meshCounterRotation;
    public GameObject meshPointer;
    public GameObject characterMesh;
    public Animator characterAnimator;
    public Animator placementAnimator;
    public TextMeshProUGUI endOfRaceIndicator;
    public GameObject cameraTarget;
    public GameObject wrongWayIndicator;
    public VisualEffect speedLineFX;
    public GameObject crownVis;
    public Image wallKickStamp;
    [SerializeField] ParticleSystem sparkSystem;


    [Space(10)]
    [Header("Fields that must be public for the game loop (DO NOT EDIT)")]
    [Tooltip("Represents the forward direction of the chair.")]
    public Vector3 forward;
    [Tooltip("Represents the value of this chairs balance")]
    public float chairBalance = 0.0f;
    [Tooltip("The number of laps this player has completed")]
    public int lapCounter = 0;
    [Tooltip("The current waypoint the player must cross to progress through the race")]
    public int activeWaypointIndex = 0;
    [Tooltip("Previous waypoint the clayer crossed- used to get rid of the wrong way indicator if there are multiple checkpoints to pass through.")]
    public int previousWaypoint;
    [Tooltip("The distance to the next waypoint")]
    public float distanceToNextWP = 0.0f;
    [Tooltip("The time it took for this chair to complete the race - default to a large number for the sake of sorting placement")]
    public float completionTime = 100000000.0f;
    [Tooltip("The index of the player (P1, P2, P3, P4)")]
    public int playerIndex;
    [Tooltip("The velocity of the chair as the game is paused.")]
    public Vector3 prePauseVelocity;
    [Tooltip("The normal of the surface the chair will be kicking off of")]
    public Vector3 approachNormal;
    public float wallDist;
    public int curPlacement;
    public bool updatePlacementTextWish = false;
    public Vector3 lookAtPos;
    public Vector3 localPos;
    public List<GameObject> wallsInRange = new List<GameObject>();
    



    public int stepCount = 8;
    public float angleStep = 360.0f / 8;


    //
    //  Internal fields used for tracking timers, or positions for respawning the player.
    //

    //  internal timer for the charge time
    public float chargeTimer = 0.0f;
    //  internal timer used for applying constant deceleration
    private float decelerationTimer = 0.0f;
    //  timers for the cooldown for wall kicks (if you remain in the collision bounds of the wall)
    private float wallKickedTimer = 0.0f;
    //  timer used to track how long the player has been fallen
    private float fallTimer = 0.0f;
    //  location that the player was last grounded at
    private Vector3 lastGroundedPos;
    //  location that the player fell at
    private Vector3 fallPos;
    private RagdollSkeletonUtil ragdollHelper;
    private Transform preFallMeshTransform;
    private int lastFramePlacement;
    private bool wallKickForceWish = false;
    private float speedLineTimer = 0.0f;
    private float speedLineLength = 2.5f;
    private float coyoteTimer = 0.0f;

    private float placementMultiplier = 1.0f;
    private float targetAlpha;
    private float currentAlpha;
    [SerializeField] float transitionTime = 1.0f;


    void Start()
    {
        //  cache the forward of this chair on spawn, ignoring the y axis.
        forward = Vector3.Normalize(new Vector3(rotator.transform.forward.x, 0.0f, rotator.transform.forward.z));
        mesh = rotator.transform.GetChild(0).gameObject;
        //mesh.transform.LookAt(forward);

        meshCounterRotation = mesh.transform.GetChild(0).gameObject;
        meshPointer = mesh.transform.GetChild(1).gameObject;
        ragdollHelper = characterMesh.GetComponent<RagdollSkeletonUtil>();

        localPos = characterMesh.transform.localPosition;

        speedLineFX = chaseCam.GetComponentInChildren<VisualEffect>();
        speedLineFX.Stop();

        //  wake up the rigidbody attached to this chair
        rb.WakeUp();
    }

    private void Awake()
    {
        //  enable the playterInput (helps with issues if we instantiate a prefab of the player from a previous scene)
        playerInput.enabled = true;

        //  cache references to each action available to the player!
        aimAction = playerInput.actions["Aim"];
        kickAction = playerInput.actions["Kick"];
        balanceAction = playerInput.actions["Balance"];
        pauseAction = playerInput.actions["Pause"];
        wallKickAction = playerInput.actions["WallKick"];
        returnAction = playerInput.actions["Back"];
        //  assign the rigidbody mass to the chair weight
        rb.mass = chairWeight;

        speedLineFX = chaseCam.GetComponentInChildren<VisualEffect>();
        speedLineFX.Stop();

        //  previousWaypoint = 0;
    }

    void Update()
    {
        //  if the race is currently ongoing, the chair has not fallen, this player hasn't finished racing, and the game is not paused
        if (GameManager.instance.RaceOngoing && !fallen && !finishedRacing && !GameManager.instance.paused)
        {

            //  on the isGrounded state changing:
            switch (isGrounded)
            {
                //  if it is true, we do nothing
                case true:
                    canKick = true;
                    if (isMoving && !rollingSource.isPlaying)
                    {
                        rollingSource.Play();
                    }
                    break;
                //  if it is false, cache the last location
                case false:
                    canKick = false;
                    lastGroundedPos = transform.position;
                    //  reset the charge timer
                    //chargeTimer = 0.0f;
                    //  mark that we are no longer charging a kick
                  // chargingKick = false;
                    rollingSource.Stop();
                    break;
            }

            if (curPlacement != lastFramePlacement)
            {
                lastFramePlacement = curPlacement;
                placementAnimator.SetBool("shouldFlip", true);
            }

            switch (curPlacement)
            {
                case 0:
                    placementText.text = "1st";
                    placementMultiplier = firstPlaceMultiplier;
                    crownVis.SetActive(true);
                    break;
                case 1:
                    placementText.text = "2nd";
                    placementMultiplier = secondPlaceMultiplier;
                    crownVis.SetActive(false);
                    break;
                case 2:
                    placementText.text = "3rd";
                    placementMultiplier = thirdPlaceMultiplier;
                    crownVis.SetActive(false);
                    break;
                case 3:
                    placementText.text = "4th";
                    placementMultiplier = fourthPlaceMultiplier;
                    crownVis.SetActive(false);
                    break;
            }

            //  listen for if the aimAction is in progress
            if (aimAction.inProgress)
            {
                //  if so, we rotate!
                rotator.transform.Rotate(Vector3.up, aimAction.ReadValue<Vector2>().x * rotationSpeed * Time.deltaTime);
                //  update the forward direction
                forward = Vector3.Normalize(new Vector3(rotator.transform.forward.x, 0.0f, rotator.transform.forward.z));
            }

            //  if we are able to kick
            if (canKick && canCharge)
            {
               // //  if the kickAction is started while the player can kick and can charge
               // //  mark as valid press
               // kickAction.started += context => validChargePress = true;

                //  listen for the kick action being triggered
                if (kickAction.triggered)
                {
                    if (chargeTimer < chargeTimerMax)
                    {
                        //  mark that we are charging a kick!
                        chargingKick = true;
                        characterAnimator.SetBool("ChargingKick", chargingKick);
                    }
                    sfx.PlayOneShot(AudioManager.instance.chargeStarted);
                }

                //  if we are charging a kick:
                if (chargingKick)
                {
                    //  check that the timer is less than the max
                    if (chargeTimer < chargeTimerMax)
                    {
                        //  if so, increment the timer
                        chargeTimer += Time.deltaTime;
                    }
                    else
                    {
                        sfx.PlayOneShot(AudioManager.instance.chargeFull);
                        chargingKick = false;
                    }
                    //  update the fill amount of the charge meter
                    chargeMeterImg.fillAmount = chargeTimer / chargeTimerMax;
                }

                //  checks if the kick action was released this frame:
                if (kickAction.WasReleasedThisFrame())
                {
                    chargingKick = false;
                    //  applies the kick
                    Kick(chargeTimer);
                    //  resets the timer
                    //chargeTimer = 0.0f;
                    //  update the fill amount of the charge meter
                    chargeMeterImg.fillAmount = chargeTimer / chargeTimerMax;
                }
            }

            if (kickAction.WasReleasedThisFrame())
            {
                sfx.Stop();
            }

            //  check if we are moving:
            if (isMoving)
            {
                if (!rollingSource.isPlaying)
                {
                    rollingSource.Play(); 
                }
                
                //if (rb.velocity.magnitude > speedlineFloor)
                //{
                    var currentSpeedPercentage = rb.velocity.magnitude - speedlineFloor / speedThreshold - speedlineFloor;

                    if (currentSpeedPercentage > 1.0f)
                    {
                        currentSpeedPercentage = 1.0f;
                    }

                    speedLineFX.SetFloat("speedMultiplier", currentSpeedPercentage);
               // }

                //  else
                //  {
                //      speedLineFX.Stop();
                //  }



                if (rb.velocity.magnitude >= speedThreshold)
                {
                    if (rollingSource.clip != AudioManager.instance.fastChairLoop)
                    {
                        rollingSource.clip = AudioManager.instance.fastChairLoop;
                        //rollingSource.Stop();
                        //audioSource.Play();
                    }
                    //  otherwise do nothing
                }

                else
                {
                    if (rollingSource.clip != AudioManager.instance.slowChairLoop)
                    {
                        rollingSource.clip = AudioManager.instance.slowChairLoop;
                        //rollingSource.Stop();
                        //audioSource.Play();
                    }
                }


                //  check if the deceleration timer is less than the delay and we have NOT started deceleration
                if (decelerationTimer < decelerationDelay && !startDeceleration)
                {
                    //  increment the timer
                    decelerationTimer += Time.deltaTime;
                }

                //  otherwise:
                else
                {
                    //  reset the timer
                    decelerationTimer = 0.0f;
                    //  mark that we have started deceleration
                    startDeceleration = true;

                    //  if the square magnitude of the velocity is LESS than an arbitrary number:
                    if (rb.velocity.sqrMagnitude <= 0.005f)
                    {
                        //  we're not moving, we're not decelerating, transform the rotation to the forward
                        isMoving = false;
                        startDeceleration = false;
                        transform.rotation = Quaternion.LookRotation(forward);
                        rb.velocity = Vector3.zero;
                        rollingSource.Stop();
                        speedLineFX.Stop();
                        speedLinesPlaying = false;
                    }
                }
            }

            //  if wall kick is true->
            if (wallKick)
            {
                //kickIndicator.SetActive(true);

                //  check to see if we have kicked off of the wall
                //  if we haven't
                if (!wallKicked)
                {
                    //kickIndicator.SetActive(true);
                    //  listen for input
                    if (wallKickAction.triggered)
                    {
                        //  mark that we have kicked from a wall
                        wallKicked = true;
                        //  kick with the max force
                        Kick(chargeTimerMax);
                        //kickIndicator.SetActive(false);
                    }
                }
            }

            //  otherwise we are out of range, so hide the indicator
            else
            {
                //kickIndicator.SetActive(false);
            }

            //  if we have kicked from the wall
            if (wallKicked)
            {
                //kickIndicator.SetActive(false);
                //  start a timer
                wallKickedTimer += Time.deltaTime;

                //  if we exceed that timer
                if (wallKickedTimer >= wallKickedDelay)
                {
                    //  reset the timer to zero
                    wallKickedTimer = 0.0f;
                    //  mark wallKicked as false so we can kick again
                    wallKicked = false;
                }
            }

            //  convert the chairs balance into a percentage
            float balanceFill = chairBalance / 100.0f;
            //  check if it is over 100%
            if (balanceFill > 1.0f)
            {
                //  if so, cap at 100%
                balanceFill = 1.0f;
            }
            //  assign the fill amount to the balance meter ui element
            balanceMeterImg.fillAmount = balanceFill;

            //  if we exceed 100 balance:
            if (chairBalance >= 100.0f)
            {
                //  fall
                Fall();
            }
        }

        //  otherwise, if the race is ongoing, we haven't finished racing, the game isn't paused, but we HAVE Fallen
        else if (GameManager.instance.RaceOngoing && fallen && !finishedRacing && !GameManager.instance.paused)
        {
            //  increment the fall timer
            fallTimer += Time.deltaTime;

            //  when we surpass the fall delay
            if (fallTimer >= fallDelay)
            {
                //  respawn the chair
                Respawn();
                //  reset the fall timer
                fallTimer = 0.0f;

                //  reset the balance to zero
                chairBalance = 0.0f;
            }
        }

        //  while we haven't finished racing
        if (!finishedRacing)
        {
            //  applies the transform of the motor to the rotator object (which contains the mesh of the chair)
            rotator.transform.position = rb.transform.position;

            if (facingWall)
            {
                var direction = lookAtPos - mesh.transform.position;
                direction.y = 0.0f;

                var directionRotation = Quaternion.LookRotation(direction);
                mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, directionRotation, 2.5f * Time.deltaTime);
            }

            else
            {
                if (isMoving)
                {
                    var direction = rb.velocity.normalized;
                    direction.y = 0.0f;

                    var directionRotation = Quaternion.LookRotation(direction);
                    mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, directionRotation, 2.5f * Time.deltaTime);
                }
            }
        }

        //  otherwise the race is over
        else
        {
            //  applies the transform of the motor to the rotator object (which contains the mesh of the chair)
            rotator.transform.position = rb.transform.position;

            //  if the completiontimes dictionary doens't contain a key corresponding to this chair controller
            if (!GameManager.instance.completionTimes.ContainsKey(this))
            {
                //  add the completion time to the game manager
                GameManager.instance.completionTimes.Add(this, GameManager.instance.raceTimer);
                //  assign the completion time to the internal field (used for sorting placement)
                completionTime = GameManager.instance.raceTimer;
            }
        }

        //  if the pause action has been triggered:
        if (pauseAction.triggered)
        {
            //  if the game is not currently paused AND the race is ongoing
            if (!GameManager.instance.paused && GameManager.instance.RaceOngoing && !GameManager.instance.pauseWish)
            {
                //  mark for a wish to pause
                GameManager.instance.pauseWish = true;
                GameManager.instance.inputListener.actionsAsset = playerInput.actions;
                //GameManager.instance.pauseScrnEventSystem.playerRoot = gameObject;
                //playerInput.SwitchCurrentActionMap("Menu");//GetActionMap("Player").id);
            }
        }

        if (GameManager.instance.paused)
        {
            if (returnAction.triggered)
            {
                GameManager.instance.Resume();
                GameManager.instance.inputListener.actionsAsset = playerInput.actions;
            }
        }

        if (GameManager.instance.RaceOngoing)
        {
            //  update the lap text
            lapText.text = "Lap: " + (lapCounter + 1).ToString() + " of " + RaceHandler.instance.laps;

            if (finishedRacing && endOfRaceIndicator != null)
            {
                lapText.text = "";

                endOfRaceIndicator.text = "FINISHED " + placementText.text;

                endOfRaceIndicator.gameObject.SetActive(true);
            }

            if (coyoteKickWindow)
            {
                coyoteTimer += Time.deltaTime;

                if (coyoteTimer >= coyoteTime)
                {
                    coyoteKickWindow = false;
                    wallKicked = false;
                    wallKick = false;

                    Debug.Log("Coyote Time Over (can no longer wall kick)");

                    currentAlpha = Mathf.MoveTowards(currentAlpha, 0.0f, transitionTime * Time.deltaTime);
                    wallKickStamp.color = new Color(wallKickStamp.color.r, wallKickStamp.color.g, wallKickStamp.color.b, currentAlpha);
                }
            }

            if (wallKick)
            {
                
                if (fadeIn)
                {
                    if (targetAlpha != 1.0f)
                    {
                        targetAlpha = 1.0f;
                    }
                    
                    if (currentAlpha >= targetAlpha)
                    {
                        targetAlpha = 0.0f;
                        fadeIn = false;
                    }
                }
                else
                {
                    if (currentAlpha <= targetAlpha)
                    {
                        targetAlpha = 1.0f;
                        fadeIn = true;
                    }
                }

                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, transitionTime * Time.deltaTime);
                wallKickStamp.color = new Color(wallKickStamp.color.r, wallKickStamp.color.g, wallKickStamp.color.b, currentAlpha);
            }
            else
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 0.0f, transitionTime * Time.deltaTime);
                wallKickStamp.color = new Color(wallKickStamp.color.r, wallKickStamp.color.g, wallKickStamp.color.b, currentAlpha);
            }
            //Debug.DrawRay(rb.position, approachNormal * 8f, Color.yellow);
        }

        if (GameManager.instance.RaceEnding && GameManager.instance.RaceOngoing && endOfRaceIndicator != null)
        {
            endOfRaceIndicator.gameObject.SetActive(true);

            if (!finishedRacing)
            {
                endOfRaceIndicator.text = "Race Ending in: " + GameManager.instance.FormatTimer(GameManager.instance.raceOverTimer);
            }
        }
    }


    //  Method used for handling kicking on the chair.
    //  applies a force to the chair scaled off of how long a kick has been charged
    //  FORCE applied with animation events to time it with the actual kick motion
    void Kick(float scalar)
    {
        //  mark that this chair is moving
        isMoving = true;
        //  if we kicked from a wall:
        if (wallKicked)
        {
            characterAnimator.SetBool("WallKick", true);
            wallKickForceWish = true;
        }
        //  otherwise, we charged the kick
        else
        {
            characterAnimator.SetBool("Kicked", true);

            //  mark that we are no longer charging a kick
            chargingKick = false;
        }
        canCharge = false;
        ApplyKickForce();
    }

    void OnTriggerEnter(Collider other)
    {
        //  Is the trigger a wall kickable surface?
        if (other.CompareTag("Wall") || (other.gameObject.GetComponentInChildren<WallKickTrigger>() != null))
        {
            wallsInRange.Add(other.gameObject);

            wallKick = true;
            coyoteKickWindow = false;
        }

        //  is the trigger a checkpoint?
        if (other.CompareTag("Waypoint"))
        {
            //RaceHandler.instance.HandleCheckpoint(this, other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //  if we have collided with another player
        if (collision.gameObject.CompareTag("Player"))
        {
            //  adjust this chairs balance
            chairBalance += ChangeInBalanceAfterCollision(collision.gameObject.GetComponent<ChairController>());
        }

        //  if we have collided with a wall
        else if (collision.gameObject.layer != 3)
        {
            PlaySparks();
            //  update the balance of the chair
            chairBalance += (chairStability * rb.velocity.magnitude); //* BalanceUtils.DetermineSideOfCollision(this, collision.GetContact(0).point));
        }
    }


    void OnTriggerExit(Collider other)
    {
        //  if (other.CompareTag("Wall"))
        //  {
        //      wallKicked = false;
        //      wallKick = false;
        //      Debug.Log("Out of wall kick range");
        //  }

        //  Is the trigger a wall kickable surface?
        if (other.CompareTag("Wall") || (other.gameObject.GetComponentInChildren<WallKickTrigger>() != null))
        {
            wallsInRange.Remove(other.gameObject);

            if (wallsInRange.Count == 0)
            {
                //  wallKicked = false;
                //  wallKick = false;
                Debug.Log("Out of wall kick range");

                coyoteKickWindow = true;
                coyoteTimer = 0.0f;
            }
        }
    }

    //  apply physics forces over time
    void FixedUpdate()
    {
        //  so long as the game isn't paused:
        if (!GameManager.instance.paused && !finishedRacing)
        {
            //  if the chair is moving and we have started the deceleration process
            if (isMoving && startDeceleration)
            {
                //  world space velocity of the chair
                Vector3 worldVelocity = rb.GetPointVelocity(transform.position);

                //  deceleration velocity (dot product of forward direction and the current velocity)
                float decelerationVelocity = Vector3.Dot(rb.velocity.normalized, worldVelocity);

                //  desired delta velocity => - velocity * the friction coeffecient (strength)
                float desiredVelocityChange = -decelerationVelocity * frictionCoefficient;

                //  showing deceleration force for debug forces
                Debug.DrawLine(transform.position, transform.position + rb.velocity * desiredVelocityChange, Color.red);

                // if we are not charging a kick: apply default friction
                if (!chargingKick)
                {
                    //  apply force
                    rb.AddForce(rb.velocity.normalized * desiredVelocityChange, ForceMode.Force);
                }
                //  otherwise: apply friction scaled by x
                else
                {
                    if (decelerateWhileCharging)
                    {
                        //  apply force
                        rb.AddForce(rb.velocity.normalized * (desiredVelocityChange * chargingKickFrictionMultiplier), ForceMode.Force);
                    }
                }

            }

            //  so long as we are moving and a balance action is in progress:
            if (isMoving && (balanceAction.inProgress && isGrounded) && !fallen)
            {
                //  adjust the balance accordingly
                chairBalance += ChangeInBalance();
                //  apply a slight force to the player in the desired direction
                rb.AddForceAtPosition(Vector3.Cross(forward, rotator.transform.up) * chairManeuverability * -balanceAction.ReadValue<Vector2>().x * Time.deltaTime, rb.transform.position, ForceMode.Impulse);
            }

            //  if the balance is NOT zero, and we are not currently balancing the chair
            if (chairBalance != 0.0f && !balanceAction.inProgress)
            {
                //  cache the difference between zero and our current balance
                float curDifference = 0.0f - chairBalance;
                //  apply that difference to the balance over time, scaled by the stability degradation factor
                chairBalance += curDifference * stabilityDegredationFactor * Time.deltaTime / chairStability;
            }

            //  raycast check for if this chair is grounded
            RaycastHit hit;

            // if this cast returns false:
            if (!Physics.Raycast(rb.transform.position, -Vector3.up, out hit, 1.1f, groundMask))
            {
                //  if we were grounded
                if (isGrounded)
                {
                    //  we are no longer grounded
                    isGrounded = false;
                }
            }
            //  otherwise, there was ground so we are now grounded
            else
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                }

                canCharge = true;
                rb.velocity.Set(rb.velocity.x, 0.0f, rb.velocity.z);

                var slopeRotation = Quaternion.FromToRotation(mesh.transform.up, hit.normal);
                mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, slopeRotation * mesh.transform.rotation, 10 * Time.deltaTime);
            }

            //  so long as the race handler instance is NOT null
            if (RaceHandler.instance != null)
            {
                //  calculate the distance to the next waypoint
                distanceToNextWP = Vector3.Distance(this.transform.position, RaceHandler.instance.waypoints[activeWaypointIndex].transform.position);
            }
        }
    }

    //  this method will calculate the base balance of the chair according to the following formula:
    //  base stability * chairs current speed * player input (-1 or 1) = result
    public float ChangeInBalance()
    {
        PlaySparks();
        return (chairStability * rb.velocity.magnitude * strafeMultiplier * placementMultiplier) / stabilityCoefficient;
    }

    //  this method will resolve balance adjustments to both chairs that are part of a collision by the following formula:
    //  (other chairs weight / this chairs weight) * other chairs relaitve velocity * side of collision (left-> 1, right-> -1) = result to player
    public float ChangeInBalanceAfterCollision(ChairController other)
    {
        PlaySparks();
        return (other.chairWeight / this.chairWeight) * placementMultiplier
            * BalanceUtils.CalcRelativeVelocity(this.rb, other.rb).magnitude;
    }

    //  Method to be called when the player reachers -100 or 100 balance
    public void Fall()
    {
        //  mark that we have fallen
        fallen = true;
        ToggleRagdoll(fallen);
        //  cache the fall position
        fallPos = transform.position;

        //  we are no longer charging a kick
        chargingKick = false;
        //  reset the charge timer
        chargeTimer = 0.0f;

        rollingSource.Stop();

        speedLineFX.Stop();
        speedLinesPlaying = false;

        sfx.PlayOneShot(AudioManager.instance.PlayChairFall());
    }

    //  this method will be called at any point when the player respawns
    //  --either after getting up from falling, or from getting out of the map
    public void Respawn()
    {
        transform.position = fallPos;
        rb.velocity = Vector3.zero;
        fallen = false;
        ToggleRagdoll(fallen);
    }

    //  on the chairWeight changing in the inspector, update the rigidbodies mass accordingly
    void OnValidate()
    {
        rb.mass = chairWeight;
    }

    public void ToggleRagdoll(bool fallenState)
    {
        foreach(Rigidbody joint in ragdollHelper.joints)
        {
            joint.isKinematic = !fallenState;
        }

        if (!fallenState)
        {
            characterMesh.transform.parent = meshCounterRotation.transform;
            characterMesh.transform.localPosition = localPos;
            characterMesh.transform.localEulerAngles = Vector3.zero;
            characterAnimator.enabled = true;
        }

        if (fallenState)
        {
            preFallMeshTransform = characterMesh.transform;
            characterMesh.transform.parent = null;
            characterAnimator.enabled = false;
        }
    }

    public void ApplyKickForce()
    {
        if (wallKickForceWish)
        {
            sfx.PlayOneShot(AudioManager.instance.wallKickBoost);
            //  apply the wallkick modifier
            Vector3 force = chargeTimerMax * forward * wallKickModifier * kickPower;

            rb.AddForce(force, ForceMode.Impulse);

            chairBalance -= balanceRecovery;

            if (chairBalance < 0.0f)
            {
                chairBalance = 0.0f;
            }

            wallKickForceWish = false;
        }

        else
        {
            //  do not apply the wallkick modifier
            rb.AddForce(chargeTimer * forward * kickPower, ForceMode.Impulse);

            //  resets the timer
            chargeTimer = 0.0f;
            //  update the fill amount of the charge meter
            chargeMeterImg.fillAmount = chargeTimer / chargeTimerMax;
        }
        //  play a random kick sound
        AudioManager.instance.PlayRandomKick();

        canCharge = true;

        if (!speedLinesPlaying)
        {
            speedLineFX.Play();
            speedLinesPlaying = true;

        }
    }

    void PlaySparks()
    {
        if (sparkSystem != null)
        {
            sparkSystem.Play();
        }
    }
}

