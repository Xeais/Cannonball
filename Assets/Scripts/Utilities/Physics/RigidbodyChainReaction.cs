using System.Linq;
using String = System.String;
using UnityEngine;

//Add a rigidbody to myself and to any objects which I hit matching certain tags.
class RigidbodyChainReaction : MonoBehaviour
{
  [SerializeField] private string[] tags = null;
  public string[] Tags
  {
    get {return tags;}
    set {tags = value;}
  }

  private void Start()
  {
    if(gameObject.GetComponent<Rigidbody>() == null)
      gameObject.AddComponent<Rigidbody>();
  }

  private void OnCollisionEnter(Collision collision)
  {
    if(tags.Contains(collision.gameObject.tag) && collision.gameObject.GetComponent<RigidbodyChainReaction>() == null)
    {
      var chain = collision.gameObject.AddComponent<RigidbodyChainReaction>();
      chain.tags = tags;

      Debug.Log(String.Format("Tags: {0}" + GameManager.LogSeparator, tags));
    }
  }
}