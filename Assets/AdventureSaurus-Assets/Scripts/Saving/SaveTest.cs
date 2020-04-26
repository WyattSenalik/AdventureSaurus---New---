using UnityEngine;

public class SaveTest : MonoBehaviour
{
    // Room Parent and Prefab
    [SerializeField] private Transform _roomsParent = null;
    [SerializeField] private GameObject _roomPrefab = null;
    // Bleed Light Parent and Prefab
    [SerializeField] private Transform _bleedLightsParent = null;
    [SerializeField] private GameObject _bleedLightPrefab = null;

    // Wall Parent and Prefab
    [SerializeField] private Transform _wallsParent = null;
    [SerializeField] private GameObject _wallPrefab = null;

    // Stairs Prefab
    [SerializeField] private GameObject _stairsPrefab = null;

    // Grid Prefab
    [SerializeField] private GameObject _gridPrefab = null;

    // Interactable Parent
    [SerializeField] private Transform _interactableParent = null;

    // Enemy Parent
    [SerializeField] private Transform _enemyParent = null;

    // Ally Parent
    [SerializeField] private Transform _allyParent = null;

    public void TestLoad()
    {
        // Rooms
        LoadRoomsController.LoadRooms(_roomsParent, _roomPrefab);
        LoadRoomsController.LoadBleedLights(_bleedLightsParent, _bleedLightPrefab);
        LoadRoomsController.LoadReferences(_roomsParent, _bleedLightsParent);
        // Walls
        LoadWallsController.LoadWalls(_wallsParent, _wallPrefab);
        // Stairs
        LoadStairsController.LoadStairs(_stairsPrefab);
        // Tilemap
        LoadTilemapController.LoadTilemap(_gridPrefab);
        // Interactables
        LoadInteractablesController.LoadInteractables(_interactableParent);
        // Enemies
        LoadEnemiesController.LoadEnemies(_enemyParent);
        // Allies
        LoadAlliesController.LoadAllies(_allyParent);
    }

    public void TestSave()
    {
        SaveSystem.SaveGame();
    }
}
