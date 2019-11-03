using System;
using UnityEngine;
using Random = UnityEngine.Random;

//This is based on the RigidbodyController from Unity's Standard Assets.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonRigidbodyController : MonoBehaviour
{
  [Serializable]
  public class MovementSettings
  {
    [Header("Speeds")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float reverseSpeed = 4f;
    [SerializeField] private float strafeSpeed = 4f; //Speed when walking sideways
    [Header("Running")]
    [SerializeField] private float runMultiplier = 2f; //Speed when sprinting
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 50f;
    public float JumpForce {get {return jumpForce;}}
    [Header("Slope Handling")]
    [SerializeField] private AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
    public AnimationCurve SlopeCurveModifier {get {return slopeCurveModifier;}}

    private float currentTargetSpeed = 8f;
    public float CurrentTargetSpeed {get {return currentTargetSpeed;}}
    private bool running = false;
    public bool Running
    {
      get {return running;}
      set {running = value;}
    }

    public void UpdateDesiredTargetSpeed(Vector2 input)
    {
      if(input == Vector2.zero)
        return;

      if(input.x > 0f || input.x < 0f)
        currentTargetSpeed = strafeSpeed;

      if(input.y < 0f)
        currentTargetSpeed = reverseSpeed;

      if(input.y > 0f)  //If the player is strafing and moving forward at the same time forward speed should take precedence.
        currentTargetSpeed = forwardSpeed;

      if(Input.GetKey(runKey))
      {
        currentTargetSpeed *= runMultiplier;
        running = true;
      }
      else
        running = false;
    }
  }

  [Serializable]
  public class AdvancedSettings
  {
    [SerializeField] private float groundCheckDistance = 0.01f; //Distance for checking if the controller is grounded (0.01f seems to work best for this).
    public float GroundCheckDistance {get {return groundCheckDistance;}}
    [SerializeField] private float stickToGroundHelperDistance = 0.6f; //Stops the character.
    public float StickToGroundHelperDistance {get {return stickToGroundHelperDistance;}}
    [SerializeField] private float slowDownRate = 20f; //Rate at which the controller comes to a stop when there is no input.
    public float SlowDownRate {get {return slowDownRate;}}
    [SerializeField] private bool airControl = true;
    public bool AirControl {get {return airControl;}}
    [Tooltip("Set this to 0.1 or more if you get stuck in wall.")]
    [SerializeField] private float shellOffset = 0f; //Reduce the radius by that ratio to avoid getting stuck in wall.
    public float ShellOffset {get {return shellOffset;}}
  }

  [Header("Camera")]
  [SerializeField] private Camera cam;
  [Space(15)]
  [SerializeField] private MovementSettings movementSettings = new MovementSettings();
  [Space(15)]
  [SerializeField] private AdvancedSettings advancedSettings = new AdvancedSettings();
  [Space(15)]
  [SerializeField] private MouseLook mouseLook = new MouseLook();
  [Space(15)]
  [SerializeField] private bool useFovKick;
  [SerializeField] private FOVKick fovKick = new FOVKick();
  [Space(5)]
  [Header("Sounds")]
  [SerializeField] private AudioClip[] footstepSounds;
  [Space(2)]
  [SerializeField] private AudioClip jumpSound;
  [SerializeField] private AudioClip landSound;
  [Header("Step Configuration")]
  [SerializeField][Range(0f, 1f)] private float runStepLength = 0.7f;
  [SerializeField] private float stepInterval = 4f;

  private float rigidBodyDrag; //Store the original drag from the rigidbody.

  public bool Running
  {
    get {return movementSettings.Running;}
    set {movementSettings.Running = value;}
  }
  public bool Jumping {get; set;}
  public bool Grounded {get; set;}
  public Vector3 Velocity {get {return rigidBody.velocity;}}

  private Rigidbody rigidBody;
  private CapsuleCollider capsuleCollider;
  private Vector2 input;
  private Vector3 desiredMove;
  private float yRotation;
  private Vector3 groundContactNormal;
  private bool jump, previouslyGrounded;
  private AudioSource audioSource;
  private float nextStep, stepCycle = 0f;

  private void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    rigidBodyDrag = rigidBody.drag;
    capsuleCollider = GetComponent<CapsuleCollider>();
    audioSource = GetComponent<AudioSource>();

    nextStep = stepCycle / 2f;

    mouseLook.Init(transform, cam.transform);
    fovKick.Setup(cam);
  }

  private void Update()
  {
    RotateView();

    if(Input.GetButtonDown("Jump") && !jump)
      jump = true;
  }

  private void FixedUpdate()
  {
    GroundCheck();
    input = GetInput();

    if((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.AirControl || Grounded))
    {
      desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
      desiredMove = Vector3.ProjectOnPlane(desiredMove, groundContactNormal).normalized;

      desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
      desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
      desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;

      if(rigidBody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
        rigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
    }

    if(Grounded)
    {
      rigidBody.drag = advancedSettings.SlowDownRate + rigidBodyDrag;

      if(jump)
      {
        rigidBody.drag = rigidBodyDrag;
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        rigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
        Jumping = true;

        PlayJumpSound();
      }

      if(!Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && rigidBody.velocity.magnitude < 1f)
        rigidBody.Sleep();
    }
    else
    {
      rigidBody.drag = rigidBodyDrag;

      if(previouslyGrounded && !Jumping)
        StickToGroundHelper();
    }

    jump = false;

    ProgressStepCycle(movementSettings.CurrentTargetSpeed);
  }

  private Vector2 GetInput()
  {
    Vector2 input = new Vector2
    {
      x = Input.GetAxis("Horizontal"),
      y = Input.GetAxis("Vertical")
    };
    movementSettings.UpdateDesiredTargetSpeed(input);

    bool wasWalking = !Running;

    Running = Input.GetKey(KeyCode.LeftShift);  //Keep track of whether or not the character is walking or running.

    if(!Running != wasWalking && useFovKick && rigidBody.velocity.sqrMagnitude > 0f)
    {
      StopAllCoroutines();
      StartCoroutine(Running ? fovKick.FOVKickUp() : fovKick.FOVKickDown());
    }

    return input;
  }

  private float SlopeMultiplier()
  {
    float angle = Vector3.Angle(groundContactNormal, Vector3.up);

    return movementSettings.SlopeCurveModifier.Evaluate(angle);
  }

  private void StickToGroundHelper()
  {
    RaycastHit hitInfo;
    if(Physics.SphereCast(transform.position, capsuleCollider.radius * (1f - advancedSettings.ShellOffset), Vector3.down, out hitInfo,
                          ((capsuleCollider.height / 2f) - capsuleCollider.radius) + advancedSettings.StickToGroundHelperDistance, Physics.AllLayers,
                          QueryTriggerInteraction.Ignore))
    {
      if(Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
        rigidBody.velocity = Vector3.ProjectOnPlane(rigidBody.velocity, hitInfo.normal);
    }
  }

  private void RotateView()
  {
    if(Mathf.Abs(Time.timeScale) < float.Epsilon) //Avoids the mouse-looking if the game is effectively paused.
      return;

    float oldYRotation = transform.eulerAngles.y;
    mouseLook.LookRotation(transform, cam.transform);

    if(Grounded || advancedSettings.AirControl)
    {
      //Rotate the rigidbody's velocity to match the new direction that the character is looking:
      Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
      rigidBody.velocity = velRotation * rigidBody.velocity;
    }
  }

  private void GroundCheck()
  {
    previouslyGrounded = Grounded;

    RaycastHit hitInfo;
    if(Physics.SphereCast(transform.position, capsuleCollider.radius * (1f - advancedSettings.ShellOffset), Vector3.down, out hitInfo,
                          ((capsuleCollider.height / 2f) - capsuleCollider.radius) + advancedSettings.GroundCheckDistance, Physics.AllLayers,
                          QueryTriggerInteraction.Ignore))
    {
      Grounded = true;
      groundContactNormal = hitInfo.normal;
    }
    else
    {
      Grounded = false;
      groundContactNormal = Vector3.up;
    }

    if(!previouslyGrounded && Grounded && Jumping)
    {
      Jumping = false;

      PlayLandingSound();
    }
  }

  private void PlayJumpSound()
  {
    audioSource.clip = jumpSound;
    audioSource.Play();
  }

  private void PlayLandingSound()
  {
    audioSource.clip = landSound;
    audioSource.Play();
    nextStep = stepCycle + 0.5f;
  }

  private void ProgressStepCycle(float speed)
  {
    if(rigidBody.velocity.sqrMagnitude > 0 && (input.x != 0f || input.y != 0f))
      stepCycle += (rigidBody.velocity.magnitude + (speed * (!Running ? 1f : runStepLength))) * Time.fixedDeltaTime;

    if(!(stepCycle > nextStep))
      return;

    nextStep = stepCycle + stepInterval;

    PlayFootStepAudio();
  }

  private void PlayFootStepAudio()
  {
    if(!Grounded)
      return;

    //Pick and play a random footstep-sound from the array, excluding the sound at zero-index:
    int n = Random.Range(1, footstepSounds.Length);
    audioSource.clip = footstepSounds[n];
    audioSource.PlayOneShot(audioSource.clip);

    //Move picked sound to zero-index so it's not played right again!
    footstepSounds[n] = footstepSounds[0];
    footstepSounds[0] = audioSource.clip;
  }
}