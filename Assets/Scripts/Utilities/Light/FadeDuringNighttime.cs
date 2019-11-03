using UnityEngine;

//Fades a light when it is rotated upside down so objects aren't still illuminated.
[RequireComponent(typeof(Light))]
public class FadeDuringNighttime : MonoBehaviour
{
  [Header("Twilight Angles")]
  [SerializeField] private float twilightStartAngle = 85f;
  [SerializeField] private float twilightStopAngle = 100f;

  private Light Light;
  private readonly Quaternion MiddayAngle = Quaternion.Euler(90f, 0f, 0f); //Straight down

  private void Start()
  {
    Light = GetComponent<Light>();
  }

  private void Update()
  {
    //Above "twilightStartAngle", set intensity to 1. Below "twilightStopAngle", set to 0. Lerp in between.
    float angle = Quaternion.Angle(MiddayAngle, transform.rotation);
    Light.intensity = 1 - Mathf.Clamp01((angle - twilightStartAngle) / (twilightStopAngle - twilightStartAngle));
  }
}