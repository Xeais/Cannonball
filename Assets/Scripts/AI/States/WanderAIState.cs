using UnityEngine;

class WanderAIState : AIState
{
  //These values are used to calculate a random time between changing directions.
  private float minTimeToChangeDirection = 1f;
  private float maxTimeToChangeDirection = 5f;

  private float timeLeftTillChangeDirection;

  public WanderAIState(Character owningCharacter, CharacterAIController aiController) : base(owningCharacter, aiController) {}

  public override void Activate() {ChooseNewDirection();}

  public override void Deactivate() {Character.MoveAmount = Vector3.zero;}

  public override void Update()
  {
    //If you can see the player switch to attack state.
    if(CanSeePlayer())
    {
      AIController.SetState(new AttackAIState(Character, AIController));

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

    //Get random direction:
    ChooseNewDirection();

    //Ensure the new direction isn't going into a wall.
    Character.MoveAmount = MathUtils.ReflectIfAgainstNormal(Character.MoveAmount, hitInfo.normal);
  }

  public override string GetName() {return "Wander State";}

  private void ChooseNewDirection()
  {
    //Choose a random direction:
    Vector2 direction2D = MathUtils.RandomUnitVector2();

    Character.MoveAmount = new Vector3(direction2D.x, 0f, direction2D.y);

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