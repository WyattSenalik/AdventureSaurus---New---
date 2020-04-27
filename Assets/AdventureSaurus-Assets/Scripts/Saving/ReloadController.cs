using UnityEngine;
using UnityEngine.Tilemaps;

public class ReloadController : MonoBehaviour
{
    // Prefabs
    [SerializeField] private GameObject _roomPrefab = null;
    [SerializeField] private GameObject _bleedLightPrefab = null;
    [SerializeField] private GameObject _wallPrefab = null;
    [SerializeField] private GameObject _stairsPrefab = null;
    [SerializeField] private GameObject _gridPrefab = null;

    // Parents
    private Transform _roomParent;
    private Transform _bleedLightParent;
    private Transform _wallParent;
    private Transform _interactableParent;
    private Transform _enemyParent;
    private Transform _allyParent;
    // Other necessities
    private Transform _stairsTrans;
    private Tilemap _tilemap;

    // Called before the first frame
    private void Start()
    {
        // Load everything
        CreateParents();
        LoadFloor();
        // Get the procedural generation controller from the generation controller
        GameObject genContObj = GameObject.FindWithTag("GenerationController");
        if (genContObj != null)
        {
            ProceduralGenerationController genContRef = genContObj.GetComponent<ProceduralGenerationController>();
            if (genContRef != null)
            {
                genContRef.GiveEssentials(_roomParent, _bleedLightParent, _wallParent, _stairsTrans, _tilemap,
                    _enemyParent, _allyParent, _interactableParent);
                genContRef.GenerateFloor(false, 0, _allyParent);
            }
            else
                Debug.LogError("No ProceduralGenerationController was attached to " + genContObj.name);
        }
        else
            Debug.LogError("No GameObject with the tag GenerationController was found");
    }

    /// <summary>
    /// Creates the objects whose transforms will be parents of the objects to spawn
    /// </summary>
    private void CreateParents()
    {
        _roomParent = new GameObject().transform;
        _bleedLightParent = new GameObject().transform;
        _wallParent = new GameObject().transform;
        _interactableParent = new GameObject().transform;
        _enemyParent = new GameObject().transform;
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;
    }

    /// <summary>
    /// Loads the stored data for the floor
    /// </summary>
    private void LoadFloor()
    {
        // Rooms
        LoadRoomsController.LoadRooms(_roomParent, _roomPrefab);
        LoadRoomsController.LoadBleedLights(_bleedLightParent, _bleedLightPrefab);
        LoadRoomsController.LoadReferences(_roomParent, _bleedLightParent);
        // Walls
        LoadWallsController.LoadWalls(_wallParent, _wallPrefab);
        // Stairs
        _stairsTrans = LoadStairsController.LoadStairs(_stairsPrefab);
        // Tilemap
        _tilemap = LoadTilemapController.LoadTilemap(_gridPrefab);
        // Interactables
        LoadInteractablesController.LoadInteractables(_interactableParent);
        // Enemies
        LoadEnemiesController.LoadEnemies(_enemyParent);
        // Allies
        LoadAlliesController.LoadAllies(_allyParent);
    }

    /// <summary>
    /// Finds the Persistant controller and calls PrepareNextFloor
    /// </summary>
    public void PrepareNextFloor()
    {
        try
        {
            GameObject persistObj = GameObject.FindWithTag("PersistantController");
            PersistantController persistRef = persistObj.GetComponent<PersistantController>();
            persistRef.PrepareNextFloor();
        }
        catch
        {
            Debug.LogError("Failed to Prepare Next Floor");
        }
    }
}
