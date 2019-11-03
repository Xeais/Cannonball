using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class FOVKick
{
  [SerializeField] private Camera cam; //If null the main camera will be used.
  [SerializeField] private float fovIncrease = 3f; //The amount the field of view increases when going into a run.
  [SerializeField] private AnimationCurve increaseCurve;
  [SerializeField] private float timeToIncrease = 1f;
  [SerializeField] private float timeToDecrease = 1f;

  private float originalFov;

  public void Setup(Camera camera)
  {
    CheckStatus(camera);

    cam = camera;
    originalFov = camera.fieldOfView;
  }

  private void CheckStatus(Camera camera)
  {
    if(camera == null)
      throw new Exception("\"FOVKick.cs\": \"Camera\" is null, please supply the camera to the inspector.");

    if(increaseCurve == null)
      throw new Exception("\"FOVKick.cs\": \"increaseCurve\" is null, please define the curve for the field of view kicks.");
  }

  public void ChangeCamera(Camera camera) {cam = camera;}

  public IEnumerator FOVKickUp()
  {
    float t = Mathf.Abs((cam.fieldOfView - originalFov) / fovIncrease);
    while(t < timeToIncrease)
    {
      cam.fieldOfView = originalFov + (increaseCurve.Evaluate(t / timeToIncrease) * fovIncrease);
      t += Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }
  }

  public IEnumerator FOVKickDown()
  {
    float t = Mathf.Abs((cam.fieldOfView - originalFov) / fovIncrease);
    while(t > 0f)
    {
      cam.fieldOfView = originalFov + (increaseCurve.Evaluate(t / timeToDecrease) * fovIncrease);
      t -= Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }

    cam.fieldOfView = originalFov; //Ensure that FOV returns to the original size!
  }
}