using System.Collections.Generic;
using ExtensionMethods;
using UnityEngine;

[AddComponentMenu("Utilities/Singleton<T>")]
/// <summary>
/// Be aware this will not prevent a non singleton constructor such as "T myT = new T();".
/// To prevent that, add "protected T() {};" to your singleton class!
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
  protected static bool Quitting {get; private set;}

  private static readonly object Lock = new object();
  private static Dictionary<System.Type, Singleton<T>> instances;
  private static readonly short logSeparatorLength = 120; //Length of separator for debugging messages
  private static readonly string logSeparator = System.Environment.NewLine + new char[logSeparatorLength].PopulateArray('-');
  public static string LogSeparator {get {return logSeparator;}}

  public static T Instance
  {
    get
    {
      if(Quitting)
      {
        Debug.LogWarning("Instance of \"Singleton<" + typeof(T) + ">\" already destroyed on application quit. " +
                         "Won't create again - returning null." + logSeparator);

        return null;
      }

      lock(Lock)
      {
        if(instances == null)
          instances = new Dictionary<System.Type, Singleton<T>>();

        if(instances.ContainsKey(typeof(T)))
        {
          T test = instances[typeof(T)] as T;

          Debug.Log("Using instance already created: " + test.name + "!" + logSeparator);
          /* The ??-operator is called the null-coalescing operator. 
           * It returns the left-hand operand if the operand is not null; 
           * otherwise it returns the right hand operand. */
          return test ?? null;
        }
        else
          return null;
      }
    }
  }

  private void OnEnable()
  {
    if(!Quitting)
    {
      bool iAmSingleton = false;

      lock(Lock)
      {
        if(instances == null)
          instances = new Dictionary<System.Type, Singleton<T>>();

        if(instances.ContainsKey(GetType()))
        {
          Debug.Log("<color=red>There is already an instance of \"Singleton<" + typeof(T) + ">\". \"" + gameObject.name + "\" will be deleted!</color>" + logSeparator);
          Destroy(gameObject);
        }
        else
        {
          iAmSingleton = true;

          instances.Add(GetType(), this);

          DontDestroyOnLoad(gameObject);
          Debug.Log("<color=green>An instance of \"Singleton<" + typeof(T) + ">\" was created with \"DontDestroyOnLoad()\".</color>" + logSeparator);
        }
      }

      if(iAmSingleton)
        OnEnableCallback();
    }
  }

  private void OnApplicationQuit()
  {
    Quitting = true;
    OnApplicationQuitCallback();
  }

  protected virtual void OnEnableCallback() {Debug.Log("The virtual function \"OnEnableCallback()\" of \"Singleton<" + typeof(T) + ">\" has been called." + logSeparator);}

  protected virtual void OnApplicationQuitCallback() {Debug.Log("The virtual function \"OnApplicationQuitCallback()\" of \"Singleton<" + typeof(T) + ">\" has been called." + logSeparator);}
}