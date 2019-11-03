using UnityEngine;

interface IColliderHitListener
{
  void OnControllerColliderHit(ControllerColliderHit hitInfo);
}