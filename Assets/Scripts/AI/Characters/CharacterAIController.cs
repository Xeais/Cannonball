using UnityEngine;

public class CharacterAIController : MonoBehaviour, IColliderHitListener
{
  private AIState currentAIState;

  private void Start()
  {
    Character character = GetComponent<Character>();

    //Start the AI in the wander state:
    SetState(new WanderAIState(character, this));
  }

  private void Update()
  {
    //Update the state, if you have one:
    if(currentAIState != null)
      currentAIState.Update();

    //Update debug info:
    UpdateDebugDisplay();
  }

  public void OnControllerColliderHit(ControllerColliderHit hitInfo)
  {
    //Pass hit events onto your state for processing:
    if(currentAIState != null)
      currentAIState.OnControllerColliderHit(hitInfo);
  }

  public void SetState(AIState state)
  {
    //Deactivate your old state:
    if(currentAIState != null)
      currentAIState.Deactivate();

    //Switch to the new state:
    currentAIState = state;

    //Activate the new state:
    if(currentAIState != null)
      currentAIState.Activate();
  }

  //Update the displayed info about the AI.  
  private void UpdateDebugDisplay()
  {
    AIDebugGUI debugGUI = Camera.main.GetComponent<AIDebugGUI>();
    if(debugGUI == null)
      return;

    /*Ignore this, if the object isn't selected. Note that this won't work properly, if there is more than one AI-entity selected.
    if(!UnityEditor.Selection.Contains(gameObject))
      return;*/

    debugGUI.AIDebugDisplayMsg = "Current AI-State: ";

    if(currentAIState != null)
      debugGUI.AIDebugDisplayMsg += currentAIState.GetName();
    else
      debugGUI.AIDebugDisplayMsg += "null";
  }
}