using System.Linq;
using String = System.String;
using UnityEngine;

/* A cannonball which moves around randomly. When the player approaches it, it stands still.
 * When the player gets too close, it spooks and starts running away (yes, cannonballs are very timid creatures), 
 * knocking over things in its path. */
[AddComponentMenu("Cannonball/Cannonball")]
[RequireComponent(typeof(Rigidbody))]
public class Cannonball : MonoBehaviour
{
  [Header("Speeds")]
  [SerializeField] private float speed = 10f;
  //[SerializeField] private float rotationSpeed = 1f; -> (*)
  [Header("Noticing")]
  [SerializeField] private float noticeDistance = 15f;
  [Header("Spooking")]
  [SerializeField] private float spookDistance = 7f;
  [SerializeField] private float spookLength = 8f;
  [SerializeField] private float spookSpeed = 12f;
  [SerializeField] private string[] spookCollisionTags = null; //The tags of objects that will be knocked over.

  private ForestBuilder forestBuilder;
  private Transform controller;
  private Rigidbody rigidBody;
  private bool spooked = false;
  private Vector3 direction;
  private float directionChangeTime = -1f;

  private void Start()
  {
    forestBuilder = GameManager.Instance.ForestBuilder;
    controller = GameManager.Instance.Controller;
    rigidBody = GetComponent<Rigidbody>();
  }

  private void Update()
  {
    if(Time.time >= directionChangeTime)
    {
      if(spooked)
      {
        direction = Vector3.zero; //Stop if spooked.
        spooked = false;
      }
      else
      {
        direction = Random.insideUnitSphere;  //Choose a random direction to move in.
        direction.y = 0f;
      }

      directionChangeTime = Time.time + Random.Range(1f, 4f);
    }

    float sqrDistance = (transform.position - controller.position).sqrMagnitude;
    if(sqrDistance < spookDistance * spookDistance)
    {
      //In spook range, start running in the opposite direction.
      spooked = true;
      direction = (transform.position - controller.position).normalized;
      directionChangeTime = Time.time + spookLength; //Run for "spookLength" seconds.

      Move();
    }
    else if(!spooked && sqrDistance < noticeDistance * noticeDistance) {/* In notice range, don't move. */}
    else
      Move();

    //Rotate to ground:
    transform.rotation = Quaternion.FromToRotation(Vector3.up, forestBuilder.GetGroundNormal(transform.position.x, transform.position.z));
  }

  private void Move()
  {
    if(direction.sqrMagnitude > 0f) //Don't stop abruptly!
    {
      //(*) Does not matter with a ball. transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, transform.up), rotationSpeed * Time.deltaTime); 
      rigidBody.AddForce(direction * (spooked ? spookSpeed : speed) * Time.deltaTime, ForceMode.VelocityChange); 
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    //Knock down trees and rocks when spooked:
    if(spooked && spookCollisionTags.Contains(collision.gameObject.tag))
    {
      var chain = collision.gameObject.AddComponent<RigidbodyChainReaction>();
      chain.Tags = spookCollisionTags;

      Debug.Log(String.Format("Spook Collision Tags: {0}" + GameManager.LogSeparator, spookCollisionTags));
    }
  }
}