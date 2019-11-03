using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(AudioSource))]
public class PadJoint : MonoBehaviour
{
  [Tooltip("The sound to play when someone/something compresses the joint of the pressure pad.")]
  [SerializeField] private AudioClip padSound;

  private ConfigurableJoint padJoint;
  private Rigidbody controllerRigidBody;
  private AudioSource audioSource;

  private void Start()
  {
    padJoint = GetComponent<ConfigurableJoint>();
    controllerRigidBody = GameManager.Instance.Controller.GetComponent<Rigidbody>();
    audioSource = GetComponent<AudioSource>();
  }

  private void Update()
  {
    if(padJoint.currentForce.magnitude >= controllerRigidBody.mass)
    {
      audioSource.clip = padSound;
      audioSource.Play();
    }

    Debug.Log(string.Format("Force -> Pad: {0}" + GameManager.LogSeparator, padJoint.currentForce.magnitude));
  }
}