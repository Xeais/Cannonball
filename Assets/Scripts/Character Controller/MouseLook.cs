using System;
using UnityEngine;

[Serializable]
public class MouseLook
{
  [Header("Sensitivities")]
  [SerializeField] private float xSensitivity = 2f;
  [SerializeField] private float ySensitivity = 2f;
  [Header("Clamping")]
  [SerializeField] private bool clampVerticalRotation = true;
  [SerializeField] private float minimumX = -90f;
  [SerializeField] private float maximumX = 90f;
  [Header("Smoothing")]
  [SerializeField] private bool smooth = false;
  [SerializeField] private float smoothTime = 5f;
  [Header("Cursor")]
  [SerializeField] private bool lockCursor = true;

  private Quaternion characterTargetRotation;
  private Quaternion cameraTargetRotation;
  private bool cursorIsLocked = true;

  public void Init(Transform character, Transform camera)
  {
    characterTargetRotation = character.localRotation;
    cameraTargetRotation = camera.localRotation;
  }

  public void LookRotation(Transform character, Transform camera)
  {
    float yRotation = Input.GetAxisRaw("Mouse X") * xSensitivity;
    float xRotation = Input.GetAxisRaw("Mouse Y") * ySensitivity;

    characterTargetRotation *= Quaternion.Euler(0f, yRotation, 0f);
    cameraTargetRotation *= Quaternion.Euler(-xRotation, 0f, 0f);

    if(clampVerticalRotation)
      cameraTargetRotation = ClampRotationAroundXAxis(cameraTargetRotation);

    if(smooth)
    {
      character.localRotation = Quaternion.Slerp(character.localRotation, characterTargetRotation, smoothTime * Time.deltaTime);
      camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRotation, smoothTime * Time.deltaTime);
    }
    else
    {
      character.localRotation = characterTargetRotation;
      camera.localRotation = cameraTargetRotation;
    }

    UpdateCursorLock();
  }

  public void SetCursorLock(bool value)
  {
    lockCursor = value;
    //Force unlock the cursor if the user disables the cursor-locking-helper.
    if(!lockCursor)
    {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  public void UpdateCursorLock()
  {
    //If the user sets "lockCursor" check and properly lock the cursor.
    if(lockCursor)
      InternalLockUpdate();
  }

  private void InternalLockUpdate()
  {
    if(Input.GetKeyUp(KeyCode.Escape))
      cursorIsLocked = false;
    else if(Input.GetMouseButtonUp(0))
      cursorIsLocked = true;

    if(cursorIsLocked)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }
    else if(!cursorIsLocked)
    {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  Quaternion ClampRotationAroundXAxis(Quaternion q)
  {
    q.x /= q.w;
    q.y /= q.w;
    q.z /= q.w;
    q.w = 1f;

    float angleX = 2f * Mathf.Rad2Deg * Mathf.Atan(q.x);
    angleX = Mathf.Clamp(angleX, minimumX, maximumX);
    q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

    return q;
  }
}