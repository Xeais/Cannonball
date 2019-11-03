using UnityEngine;

class AttackAIState : AIState
{
  public AttackAIState(Character owningCharacter, CharacterAIController aiController) : base(owningCharacter, aiController) {}

  public override void Activate() {}

  public override void Deactivate() {Character.MoveAmount = Vector3.zero;}

  public override void Update()
  {
    //If you can't see the player, switch to the find target state to try to find the player again.
    if(!CanSeePlayer())
      AIController.SetState(new FindTargetAIState(Character, AIController));

    MoveToTarget();
    UpdateLookDirection();

    //TODO: Attack the player, if you are close enough.
  }

  public override void OnControllerColliderHit(ControllerColliderHit hitInfo) {}

  public override string GetName() {return "Attack State";}

  private void MoveToTarget()
  {
    //Get the target:
    GameObject targetObj = GetTarget();
    if(targetObj == null)
    {
      //If you don't have a target switch back to the wander state.
      AIController.SetState(new WanderAIState(Character, AIController));

      return;
    }

    //Move toward the target at full speed:
    Vector3 moveDir = targetObj.transform.position - Character.transform.position;
    moveDir.y = 0f;
    moveDir.Normalize();

    Character.MoveAmount = moveDir;
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