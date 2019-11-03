using UnityEngine;

public class Character : MonoBehaviour
{
  [SerializeField] private float maxMoveSpeed = 5f;
  [SerializeField] private float maxTurnSpeed = 1000f;

  //These properties and functions can be used to control the movement of the character:
  public Vector3 MoveAmount {get; set;}
  public float RotateAmount {get; set;}

  private Rigidbody rbController;
  private Vector3 moveVelocity;

  private void Start()
  {
    MoveAmount = Vector3.zero;
    rbController = GetComponent<Rigidbody>();
  }

  private void Update()
  {
    //Update position:
    moveVelocity = MoveAmount * maxMoveSpeed;

    //Update rotation:
    float rotationThisFrame = RotateAmount * maxTurnSpeed * Time.deltaTime;
    Quaternion rotation = Quaternion.AngleAxis(rotationThisFrame, new Vector3(0f, 1f, 0f));
    transform.rotation *= rotation;
  }

  private void FixedUpdate()
  {
    rbController.MovePosition(rbController.position + moveVelocity * Time.fixedDeltaTime);
  }

  private void OnControllerColliderHit(ControllerColliderHit hitInfo)
  {
    //If the character hits anything, notify all of the components that implement the "IColliderHitListener"-interface.
    Component[] hitListeners = GetComponents(typeof(IColliderHitListener));
    foreach(IColliderHitListener hitListener in hitListeners)
      hitListener.OnControllerColliderHit(hitInfo);
  }

  public void SetLookDirection(Vector3 direction) {transform.rotation = Quaternion.LookRotation(direction);}
}