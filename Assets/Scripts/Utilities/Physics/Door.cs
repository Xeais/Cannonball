using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
  [SerializeField] private float speed = 2f; //Increasing this value will make the door open faster.
  [SerializeField] private float doorOpenAngle = 85f; //A positive value opens the door outwards - a negative on the contrary inwards.
  [SerializeField] private bool autoOpen = false;
  public bool AutoOpen
  {
    get {return autoOpen;}
    set {autoOpen = value;}
  }

  private bool open, enteredTrigger, aOscillate;
  private float defaultRotationAngle;
  private const float similar = 0.01f;

  private void Start()
  {
    defaultRotationAngle = transform.localEulerAngles.y;
  }

  private void Update()
  {
    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 
                                             Mathf.LerpAngle(transform.localEulerAngles.y, (open ? doorOpenAngle : defaultRotationAngle), speed * Time.deltaTime), 
                                             transform.localEulerAngles.z);

    if(aOscillate && (Mathf.Abs(transform.localEulerAngles.y - doorOpenAngle) <= similar ||
       Mathf.Abs(transform.localEulerAngles.y - defaultRotationAngle) <= similar))
    {
      autoOpen = !autoOpen;
      aOscillate = !aOscillate;

      StartCoroutine(AutoClose());
    }

    if((enteredTrigger && Input.GetKeyDown(KeyCode.F)) || (enteredTrigger && autoOpen))
    {
      open = !open;

      if(autoOpen)
      {
        autoOpen = !autoOpen;
        aOscillate = true;
      }
    }
  }

  private IEnumerator AutoClose()
  {
    Debug.Log("<color=red>\"AutoClose()\" was called!</color>" + GameManager.LogSeparator);
    yield return new WaitForSeconds(5.5f);
    open = false;
  }

  private void OnGUI()
  {
    if(enteredTrigger && !autoOpen && !aOscillate)
      GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 200, 20), "Press \"F\" to open/close the door.");
  }

  private void OnTriggerEnter(Collider other)
  {
    if(other.CompareTag("Player"))
      enteredTrigger = true;
  }

  private void OnTriggerExit(Collider other)
  {
    if(other.CompareTag("Player"))
      enteredTrigger = false;
  }
}