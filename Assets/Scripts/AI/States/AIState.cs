using UnityEngine;

/* The base AI-state. This is an abstract class that defines the interface that all of the AI-states should follow. 
 * It also holds some common data and functions that should be usefull for all of its child classes. */
public abstract class AIState
{
  private string target = "FirstPersonRigidbodyController";
  public string Target
  {
    get { return target; }
    set { target = value; }
  }
  public Character Character {get; private set;}
  public CharacterAIController AIController {get; private set;}

  /* A constant defining how far the AI can see. Since this class isn't a "MonoBehavior" the value wont be available in the editor.
   * One way to fix that would be to define it on the character class. */
  private const float maxSightDist = 20f;
  private int visibilityCheckRayMask;

  public AIState(Character owningCharacter, CharacterAIController aiController)
  {
    Character = owningCharacter;
    AIController = aiController;

    /* Settting up the mask so that it hits everything BUT things in the enemy layer. This is needed so that the raycast 
     * won't hit the character casting it. Note, for better or worse, this will have the side effect of making the raycast 
     * avoid all enemies, not just this one. */
    visibilityCheckRayMask = ~LayerMask.GetMask("Enemy");
  }

  public abstract void Activate();

  public abstract void Deactivate();

  public abstract void Update();

  public abstract void OnControllerColliderHit(ControllerColliderHit hitInfo);

  public abstract string GetName();

  /* Gets the target. This is just fixed to be the player for now. Accquiring targets can get a lot more complicated though,
   * so it's nice to have it encapsulated in a function. */
  protected GameObject GetTarget() {return GameObject.Find(target);}

  /* A check to see if this character can see the player. This is currently using a single raycast.
   * This won't be a perfect visibility check, but the approximation works good enough in most cases.
   * If more precision is needed this can be extended to use multiple raycasts. */
  protected bool CanSeePlayer()
  {
    //Get the target:
    GameObject targetObj = GetTarget();
    if(targetObj == null)
      return false;

    //Calculate information about the ray:
    Vector3 rayStart = Character.transform.position;
    Vector3 rayEnd = targetObj.transform.position;

    Vector3 rayDir = rayEnd - rayStart;
    float rayDist = rayDir.magnitude;
    if(rayDist <= 0f)
      return true; //Assume it can see the player, if they are right on top of each other.
    else if(rayDist > maxSightDist)
      return false; //The target is too far away to see in this case.

    rayDir /= rayDist;

    RaycastHit hitInfo;
    if(!Physics.Raycast(rayStart, rayDir, out hitInfo, rayDist, visibilityCheckRayMask))
      return true; //If the raycast did not hit any obstacles assume the target is visible.

    GameObject collideObject = hitInfo.collider.gameObject;

    //If the raycast hit the target then it is visible.
    return collideObject == targetObj;
  }
}