using System.Text;
using UnityEngine;

namespace ExtensionMethods
{
  [AddComponentMenu("Utilities/Extension Methods")]
  public static class ExtensionMethods
  {
    public static GameObject GetGameObjectInChildrenWithTag(this GameObject gameObject, string tag)
    {
      foreach(Transform child in gameObject.transform)
      {
        if(child.CompareTag(tag))
          return child.gameObject;
      }

      return null;
    }

    public static StringBuilder PopulateArray<T>(this T[] arr, T value)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int arrLength = arr.Length;
      for(int i = 0; i < arrLength; i++)
        stringBuilder.Append(value);

      return stringBuilder;
    }
  }
}