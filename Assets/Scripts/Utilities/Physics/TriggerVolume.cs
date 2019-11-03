using UnityEngine;

public class TriggerVolume : MonoBehaviour
{
  [SerializeField] private GameObject fireworksAll;

  private GameObject firework;
  private bool instantiated = false;

  private void OnTriggerEnter(Collider other)
  {
    if(other.CompareTag("Player"))
    {
      if(!instantiated)
      {
        firework = Instantiate(fireworksAll, transform.position, Quaternion.identity);
        instantiated = true;
      }

      firework.GetComponent<ParticleSystem>().Play();
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if(other.CompareTag("Player"))
      firework.GetComponent<ParticleSystem>().Stop();
  }
}