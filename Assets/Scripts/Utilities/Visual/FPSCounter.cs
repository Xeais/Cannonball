using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{
  private int fpsAccumulator = 0;
  private const float fpsMeasurePeriod = 0.5f;
  private float fpsNextPeriod = 0f;
  private int currentFps;
  private Text text;
  private const string display = "{0} FPS";

  private void Start()
  {
    text = GetComponent<Text>();

    fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod; 
  }

  private void Update()
  {
    fpsAccumulator++;
    if(Time.realtimeSinceStartup > fpsNextPeriod)
    {
      currentFps = (int)(fpsAccumulator / fpsMeasurePeriod);
      fpsAccumulator = 0;
      fpsNextPeriod += fpsMeasurePeriod;
      text.text = string.Format(display, currentFps);
    }
  }
}