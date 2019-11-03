using UnityEngine;

public class AIDebugGUI : MonoBehaviour
{
  private void Start() {}

  private void Update() {}

  public string AIDebugDisplayMsg {set; get;}

  private void OnGUI()
  {
    if(!string.IsNullOrEmpty(AIDebugDisplayMsg))
    {
      GUI.contentColor = Color.red;
      /* This is a quick bit of code to display information from the AI-system. Note that a more elaborate
       * system will propably be needed, if the info is going to come from multiple AI-entities. */
      GUI.Label(new Rect(30f, 20f, 300f, 20f), AIDebugDisplayMsg);
    }
  }
}