using UnityEngine;

public class Rotate : MonoBehaviour
{
  [Header("Axes and Rotation")] //Header-attributes are not valid on enumerations! They are only valid on "field" declarations. 
  [SerializeField] private float rotSpeed = 1f;
  [SerializeField] private enum Axis {X, Y, Z}
  [SerializeField] private Axis rotateAxis;

  private void Update()
  {
    Vector3 rot = Vector3.zero;
    switch(rotateAxis)
    {
      case Axis.X:
        rot = Vector3.right;
        break;
      case Axis.Y:
        rot = Vector3.up;
        break;
      case Axis.Z:
        rot = Vector3.forward;
        break;
    }
    rot *= rotSpeed * Time.deltaTime;

    transform.rotation *= Quaternion.Euler(rot);
  }
}