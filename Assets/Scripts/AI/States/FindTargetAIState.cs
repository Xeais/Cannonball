using UnityEngine;

class FindTargetAIState : AIState
{
  //These values are used to calculate a random time between changing directions.
  private float minTimeToChangeDirection = 1f;
  private float maxTimeToChangeDirection = 5f;
  private float maxTimeToGiveUpOnTarget = 5f;

  private float timeToReturnToWander;
  private float timeLeftTillChangeDirection;

  public FindTargetAIState(Character owningCharacter, CharacterAIController aiController) : base(owningCharacter, aiController) {}

  public override void Activate()
  {
    ChooseNewDirection();

    timeToReturnToWander = maxTimeToGiveUpOnTarget;
  }

  public override void Deactivate()
  {
    //Clear the movement info:
    Character.MoveAmount = Vector3.zero;
  }

  public override void Update()
  {
    //If you can see the player switch to attack state.
    if(CanSeePlayer())
    {
      AIController.SetState(new AttackAIState(Character, AIController));

      return;
    }

    //If you don't see your target after a certain amount of time return to your wander state.
    if(timeToReturnToWander > 0f)
      timeToReturnToWander -= Time.deltaTime;
    else
    {
      AIController.SetState(new WanderAIState(Character, AIController));

      return;
    }

    //Continue in the same direction for a bit of time then choose a new direction.
    if(timeLeftTillChangeDirection > 0f)
      timeLeftTillChangeDirection -= Time.deltaTime;
    else
      ChooseNewDirection();

    UpdateLookDirection();
  }

  public override void OnControllerColliderHit(ControllerColliderHit hitInfo)
  {
    //Check if the collision normal is almost horizontal:
    float hitAngleFromUp = Vector3.Angle(hitInfo.normal, Vector3.up);

    if(!MathUtils.AlmostEquals(hitAngleFromUp, 90f, 1f))
      return;

    //Choose a random direction:
    ChooseNewDirection();

    //Ensure the new direction isn't going into a wall.
    Character.MoveAmount = MathUtils.ReflectIfAgainstNormal(Character.MoveAmount, hitInfo.normal);
  }

  public override string GetName() {return "Find Target State";}

  private void ChooseNewDirection()
  {
    //If there isn't a target anymore, return to the wander state.
    GameObject targetObj = GetTarget();
    if(targetObj == null)
    {
      AIController.SetState(new WanderAIState(Character, AIController));

      return;
    }

    //Calculate the direction towards the target:
    Vector3 targetDir = targetObj.transform.position - Character.transform.position;
    targetDir.y = 0f;
    targetDir.Normalize();

    //Calculate a random direction:
    Vector2 randomDir2D = MathUtils.RandomUnitVector2();

    //Combine the directions:
    targetDir.x += randomDir2D.x;
    targetDir.z += randomDir2D.y;
    targetDir.Normalize();

    Character.MoveAmount = targetDir;

    timeLeftTillChangeDirection = Random.Range(minTimeToChangeDirection, maxTimeToChangeDirection);
  }

  private void UpdateLookDirection()
  {
    //If you aren't moving don't bother changing the direction.
    if(Character.MoveAmount.sqrMagnitude <= 0f)
      return;

    //Look in movement direction:
    Character.SetLookDirection(Character.MoveAmount);
  }
}