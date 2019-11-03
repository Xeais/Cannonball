using UnityEngine;

public class MathUtils
{
  public static float compareEpsilon = 0.00001f;

  public static float ExponentialEase(float easeSpeed, float start, float end, float dt)
  {
    float diff = end - start;
    diff *= Mathf.Clamp(dt * easeSpeed, 0f, 1f);

    return diff + start;
  }

  public static Vector3 ExponentialEase(float easeSpeed, Vector3 start, Vector3 end, float dt)
  {
    Vector3 diff = end - start;
    diff *= Mathf.Clamp(dt * easeSpeed, 0f, 1f);

    return diff + start;
  }

  public static float CalcRotationDegs(float x, float y) {return Mathf.Atan2(y, x) * Mathf.Rad2Deg;}

  public static bool AlmostEquals(float v1, float v2, float epsilon) {return Mathf.Abs(v2 - v1) <= epsilon;}

  public static bool AlmostEquals(float v1, float v2) {return AlmostEquals(v1, v2, compareEpsilon);}

  public static Vector2 RandomUnitVector2()
  {
    float angleRadians = Random.Range(0f, 2f * Mathf.PI);

    Vector2 unitVector = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));

    return unitVector;
  }

  public static Vector3 ReflectIfAgainstNormal(Vector3 vec, Vector3 normal)
  {
    //If the move direction is going back into a wall, reflect the movement away from it.
    float amountAlongNormal = Vector3.Dot(vec, normal);

    //If this value is negative it means it's going in the opposite direction of the normal. This means it has to be reflected.
    if(amountAlongNormal < 0f)
    {
      //Calculate the projection onto the normal:
      Vector3 directionAlongNormal = normal * amountAlongNormal / normal.sqrMagnitude;

      /* Subtract the projection once to remove the movement into the wall, and another time to make it move
       * away from the wall the same amount. (This adds up to subtracting twice the projection.) */
      vec -= directionAlongNormal * 2f;
    }

    return vec;
  }
}