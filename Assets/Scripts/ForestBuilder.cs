using String = System.String;
using UnityEngine;

public class ForestBuilder : MonoBehaviour
{
  [SerializeField] private float tileSize = 50f;
  public float TileSize {get {return tileSize;}}
  [SerializeField] private float groundVertSpacing = 2f;
  [SerializeField] private float trunkRadiusMult = 0.3f;
  [SerializeField] private float leafDimpleMult = 0.7f;
  [SerializeField] private float treeTaper = 0.5f;
  [SerializeField] private float rockClusterMaxRadius = 3f;
  [Space(2)]
  [Header("Materials")]
  [SerializeField] private Material groundMaterial;
  [SerializeField] private Material treeMaterial;
  [Header("Objects")]
  [SerializeField] private GameObject rockPrefab = null;
  [SerializeField] private GameObject cannonballPrefab = null;
  [SerializeField] private GameObject redBallPrefab = null;
  [SerializeField] private GameObject platformPrefab = null;
  [SerializeField] private GameObject housePrefab = null;

  private float groundXFreq, groundZFreq;
  private float groundXOffset, groundZOffset;
  private bool platformSet, houseAutoOpenSet;

  private void Awake()
  {
    //Randomize ground:
    groundXFreq = Random.Range(0.05f, 0.15f);
    groundZFreq = Random.Range(0.05f, 0.15f);
    groundXOffset = Random.value * 100f;
    groundZOffset = Random.value * 100f;

    platformSet = houseAutoOpenSet = false;
  }

  public GameObject Build(Vector3 position, bool start = false)
  {
    GameObject tile = new GameObject(String.Format("Tile [{0}/{1}]", position.x, position.z));
    tile.transform.position = position;

    //Generate ground:
    GameObject ground = MakeGround(position);
    ground.transform.parent = tile.transform;
    ground.transform.localPosition = Vector3.zero;

    if(!start)
    {
      //Place trees:
      int treeCount = Random.Range(8, 20);
      for(int i = 0; i < treeCount; i++)
      {
        GameObject tree = GrowTree();
        tree.transform.parent = tile.transform;

        PlaceObjectOnGround(tree.transform, position.x + Random.Range(0f, tileSize), position.z + Random.Range(0f, tileSize));
      }

      //Make rock clusters:
      int rockCount = Random.Range(5, 15);
      for(int i = 0; i < rockCount; i++)
      {
        GameObject rocks = MakeRocks();
        rocks.transform.parent = tile.transform;

        PlaceObjectOnGround(rocks.transform, position.x + Random.Range(0f, tileSize), position.z + Random.Range(0f, tileSize), 0.5f);
      }

      //Maybe also stick some animals in there:
      int animalCount = Random.Range(0, 3);
      for(int i = 0; i < animalCount; i++)
      {
        GameObject animal = Instantiate(cannonballPrefab);
        animal.transform.parent = tile.transform;

        PlaceObjectOnGround(animal.transform, position.x + Random.Range(0f, TileSize), position.z + Random.Range(0f, TileSize), 2f);
      }
    }
    else //Start region:
    {
      if(!platformSet)
      {
        GameObject platform = Instantiate(platformPrefab);
        platform.transform.parent = tile.transform;

        PlaceObjectOnGround(platform.transform, position.x + Random.Range(0f, TileSize), position.z + Random.Range(0f, TileSize), 1.25f);
        platformSet = true;
      }
      else if(!houseAutoOpenSet)
      {
        GameObject houseAutoOpen = Instantiate(housePrefab);
        houseAutoOpen.GetComponentInChildren<Door>().AutoOpen = true;
        houseAutoOpen.transform.parent = tile.transform;

        PlaceObjectOnGround(houseAutoOpen.transform, position.x + Random.Range(0f, TileSize), position.z + Random.Range(0f, TileSize), 2.8f);

        GameObject redBall = Instantiate(redBallPrefab, houseAutoOpen.transform.GetChild(3).position, Quaternion.identity);
        redBall.transform.parent = tile.transform;
        houseAutoOpenSet = true;
      }
    }
                                                
    return tile;
  }

  //Build a square mesh using the y-values generated in "GetGroundHeight()".
  private GameObject MakeGround(Vector3 position)
  {
    int rows = (int)(TileSize / groundVertSpacing);
    Vector3[] verts = new Vector3[(rows + 1) * (rows + 1)];
    int[] tris = new int[(rows + 1) * (rows + 1) * 2 * 3];

    int vertIndex, triIndex = vertIndex = 0;
    for(int x = 0; x <= rows; x++)
    {
      for(int z = 0; z <= rows; z++)
      {
        verts[vertIndex] = new Vector3(x * groundVertSpacing,
                                       GetGroundHeight(position.x + x * groundVertSpacing, position.z + z * groundVertSpacing),
                                       z * groundVertSpacing);

        if(x < rows && z < rows)
        {
          tris[triIndex] = vertIndex;
          tris[triIndex + 1] = vertIndex + (rows + 1) + 1;
          tris[triIndex + 2] = vertIndex + (rows + 1);
          tris[triIndex + 3] = vertIndex;
          tris[triIndex + 4] = vertIndex + 1;
          tris[triIndex + 5] = vertIndex + (rows + 1) + 1;
          triIndex += 6;
        }

        vertIndex++;
      }
    }

    Mesh mesh = new Mesh
    {
      vertices = verts,
      triangles = tris
    };
    mesh.RecalculateNormals();

    GameObject ground = new GameObject("Ground");
    MeshFilter filter = ground.AddComponent<MeshFilter>();
    filter.mesh = mesh;

    MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
    renderer.sharedMaterial = groundMaterial;

    ground.AddComponent<MeshCollider>();

    return ground;
  }

  //Build a randomized tree-mesh.
  private GameObject GrowTree()
  {
    //Randomize some tree parameters:
    int levels = Random.Range(2, 4);
    int leaves = Random.Range(3, 12);
    float radius = Random.Range(0.8f, 3f);
    float levelHeight = Random.Range(1.5f, 2.5f);

    //Preallocate the mesh arrays:
    Vector3[] verts = new Vector3[1 + (levels + 1) * leaves * 4 + levels * leaves * 4 + leaves * 2];
    int[] tris = new int[(levels * 2 + 1) * leaves * 6 + leaves * 3];
    Vector2[] uvs = new Vector2[verts.Length];
    int vertIndex, triIndex = vertIndex = 0;

    //Bottom vertex:
    verts[0] = Vector3.zero;
    uvs[0] = new Vector2(0.75f, 0.25f); //Trunk
    vertIndex = 1;

    //Build the leaves on each level. Treat the trunk as another level with its radius.
    for(int level = 0; level < levels + 1; level++)
    {
      for(int leaf = 0; leaf < leaves; leaf++)
      {
        float bottomRadius = (level == 0) ? radius * trunkRadiusMult //Trunk radius if at bottom
                                          : radius * (1f - treeTaper * (level - 1) / levels); //Taper toward the top
        float topRadius = (level == levels) ? 0f //Close to a point if at top
                                            : radius * trunkRadiusMult;

        //Add the quad vertices for this leaf:
        AddTreeVerts(verts, vertIndex, leaves, leaf, level * levelHeight, bottomRadius);
        AddTreeVerts(verts, vertIndex + 2, leaves, leaf, (level + 1) * levelHeight, topRadius);

        tris[triIndex] = vertIndex;
        tris[triIndex + 1] = vertIndex + 3;
        tris[triIndex + 2] = vertIndex + 1;
        tris[triIndex + 3] = vertIndex;
        tris[triIndex + 4] = vertIndex + 2;
        tris[triIndex + 5] = vertIndex + 3;
        triIndex += 6;

        //Set UVs to leaf or trunk color:
        uvs[vertIndex] =
        uvs[vertIndex + 1] =
        uvs[vertIndex + 2] =
        uvs[vertIndex + 3] = (level > 0) ? new Vector2(0.25f, 0.75f) : new Vector2(0.75f, 0.25f);
        vertIndex += 4;

        if(level > 0)
        {
          //Join with previous level:
          AddTreeVerts(verts, vertIndex, leaves, leaf, level * levelHeight, bottomRadius);
          AddTreeVerts(verts, vertIndex + 2, leaves, leaf, level * levelHeight, radius * trunkRadiusMult);

          tris[triIndex] = vertIndex + 2;
          tris[triIndex + 1] = vertIndex + 1;
          tris[triIndex + 2] = vertIndex + 3;
          tris[triIndex + 3] = vertIndex + 2;
          tris[triIndex + 4] = vertIndex;
          tris[triIndex + 5] = vertIndex + 1;
          triIndex += 6;

          uvs[vertIndex] =
          uvs[vertIndex + 1] =
          uvs[vertIndex + 2] =
          uvs[vertIndex + 3] = new Vector2(0.25f, 0.75f); //Leaf
          vertIndex += 4;
        }
        else
        {
          //Create tree bottom:
          AddTreeVerts(verts, vertIndex, leaves, leaf, level * levelHeight, bottomRadius);

          tris[triIndex] = vertIndex;
          tris[triIndex + 1] = vertIndex + 1;
          tris[triIndex + 2] = 0;
          triIndex += 3;

          uvs[vertIndex] =
          uvs[vertIndex + 1] = new Vector2(0.75f, 0.25f); //Trunk
          vertIndex += 2;
        }
      }
    }

    Mesh mesh = new Mesh
    {
      vertices = verts,
      triangles = tris,
      uv = uvs
    };
    mesh.RecalculateNormals();

    GameObject tree = new GameObject("Tree") {tag = "Tree"};

    tree.AddComponent<MeshFilter>().mesh = mesh;
    tree.AddComponent<MeshRenderer>().sharedMaterial = treeMaterial;

    var collider = tree.AddComponent<CapsuleCollider>();
    collider.center = new Vector3(0f, (levels + 1) * levelHeight * 0.5f, 0f);
    collider.height = (levels + 1) * levelHeight;
    collider.direction = 1; //Y-axis
    collider.radius = radius;

    return tree;
  }

  private void AddTreeVerts(Vector3[] verts, int vertIndex, int leaves, int leaf, float y, float radius)
  {
    bool useDimples = leaves % 2 == 0 && leaves > 6;
    float angle = Mathf.PI * 2f / leaves;

    verts[vertIndex] = new Vector3(Mathf.Cos(angle * leaf) * radius * ((useDimples && leaf % 2 == 0) ? leafDimpleMult : 1f),
                                   y,
                                   Mathf.Sin(angle * leaf) * radius * ((useDimples && leaf % 2 == 0) ? leafDimpleMult : 1f));
    verts[vertIndex + 1] = new Vector3(Mathf.Cos(angle * (leaf + 1)) * radius * ((useDimples && leaf % 2 == 1) ? leafDimpleMult : 1f),
                                       y,
                                       Mathf.Sin(angle * (leaf + 1)) * radius * ((useDimples && leaf % 2 == 1) ? leafDimpleMult : 1f));
  }

  //Place 1-3 randomly scaled rocks close together.
  private GameObject MakeRocks()
  {
    GameObject rocks = new GameObject("Rocks");

    int rockCount = Random.Range(1, 3);
    for(int i = 0; i < rockCount; i++)
    {
      GameObject rock = Instantiate(rockPrefab);
      rock.transform.parent = rocks.transform;

      rock.transform.position = new Vector3(Random.Range(-rockClusterMaxRadius, rockClusterMaxRadius),
                                            0f,
                                            Random.Range(-rockClusterMaxRadius, rockClusterMaxRadius));

      rock.transform.rotation = Random.rotation;

      float scale = Random.Range(1f, 3f);
      rock.transform.localScale = new Vector3(scale, scale, scale);
    }

    return rocks;
  }

  //Move the transform-y to the ground height at the specific position.
  private void PlaceObjectOnGround(Transform transform, float x, float z, float above = 0f)
  {
    Vector3 position = new Vector3(x, 0f, z) {y = GetGroundHeight(x, z) + above};
    transform.position = position;

    transform.rotation = Quaternion.FromToRotation(Vector3.up, GetGroundNormal(x, z));
  }

  //Use a noise function to generate terrain with bumps and hills.
  public float GetGroundHeight(float x, float z)
  {
    return Mathf.PerlinNoise(x * groundXFreq + groundXOffset, z * groundZFreq + groundZOffset) * 2f + //Uneven ground
           Mathf.PerlinNoise(x * 0.01f + groundXOffset, z * 0.01f + groundZOffset) * 15f; //Big hills
  }

  //Use the ground slope at a point to get its normal.
  public Vector3 GetGroundNormal(float x, float z)
  {
    float xDiff = GetGroundHeight(x + 0.1f, z) - GetGroundHeight(x, z);
    float zDiff = GetGroundHeight(x, z + 0.1f) - GetGroundHeight(x, z);

    return Vector3.Cross(new Vector3(0f, zDiff, 0.1f), new Vector3(0.1f, xDiff, 0f));
  }
}