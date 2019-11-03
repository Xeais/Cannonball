using System.Collections.Generic;
using String = System.String;
using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("Cannonball/Game Manager")]
public class GameManager : Singleton<GameManager>
{
  [SerializeField] private Transform controller = null;
  public Transform Controller {get {return controller;}}

  public ForestBuilder ForestBuilder {get; private set;}

  private const float adjacentDistance = 150f;
  private HashSet<Vector3> tiles = new HashSet<Vector3>();

  protected GameManager() {}

  private void Start()
  {
    ForestBuilder = GetComponent<ForestBuilder>();
    BuildAdjacentTiles(true);

    //Reposition controller above ground:
    Vector3 startPos = controller.transform.position;
    startPos.y = ForestBuilder.GetGroundHeight(0f, 0f) + 2f;
    controller.transform.position = startPos;
  }

  private void Update()
  {
    BuildAdjacentTiles();

    if(Input.GetKey(KeyCode.Escape))
      Application.Quit();
  }

  //Ensure that all the tiles within "adjacentDistance" from the player exist.
  private void BuildAdjacentTiles(bool start = false)
  {
    for(float x = -adjacentDistance; x < adjacentDistance; x += ForestBuilder.TileSize)
    {
      for(float z = -adjacentDistance; z < adjacentDistance; z += ForestBuilder.TileSize)
      {
        Vector3 tilePos = new Vector3((int)((controller.position.x + x) / ForestBuilder.TileSize) * ForestBuilder.TileSize, //Round down (cast) to closest tile-origin.
                                      0f,
                                      (int)((controller.position.z + z) / ForestBuilder.TileSize) * ForestBuilder.TileSize);

        if(!tiles.Contains(tilePos))
        {
          ForestBuilder.Build(tilePos, start);
          tiles.Add(tilePos);

          Debug.Log(String.Format("Built tile [{0}/{1}] (start: {2})" + LogSeparator, tilePos.x, tilePos.z, start));
        }
      }
    }
  }
}