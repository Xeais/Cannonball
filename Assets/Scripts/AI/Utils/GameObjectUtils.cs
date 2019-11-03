using UnityEngine;

public class GameObjectUtils
{
  /* Recursively sets the alpha on all objects in the heirarchy, starting at the first object you 
   * pass in through "currentObj".
   *
   * Note: Make sure your material supports transparency, or this probably won't have an effect. */
  public static void SetAlpha(float alpha, GameObject currentObj)
  {
    //Set color on object:
    MeshRenderer meshRenderer = currentObj.GetComponent<Renderer>() as MeshRenderer;

    if(meshRenderer != null && meshRenderer.material != null)
    {
      foreach(Material material in meshRenderer.materials)
      {
        Vector4 color = material.color;
        color.w = alpha;

        material.color = color;
      }
    }

    //Set alpha on children:
    for(int i = 0; i < currentObj.transform.childCount; i++)
      SetAlpha(alpha, currentObj.transform.GetChild(i).gameObject);
  }
}