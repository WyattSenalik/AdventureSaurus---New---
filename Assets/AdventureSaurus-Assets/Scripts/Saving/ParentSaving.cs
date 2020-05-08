using UnityEngine;

public class ParentSaving : MonoBehaviour
{
    // Parent of rooms
    private Transform _roomParent;
    // Parent of bleed lights
    private Transform _bleedLightParent;
    // Parent of the walls
    private Transform _wallParent;
    // Transform of the stairs
    private Transform _stairsTrans;
    // Parent of the interactables
    private Transform _interactableParent;
    // Parent of the enemies
    private Transform _enemyParent;
    // Parent of the allies
    private Transform _allyParent;

    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When we finish generating initialize
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
        // When the game saves, save the amount of rooms we currently have
        SaveSystem.OnSave += SaveRoomAmount;
        SaveSystem.OnSave += SaveBleedLightAmount;
        // Also save the walls and the amount of walls
        SaveSystem.OnSave += SaveWallsAndWallAmount;
        // Also save the stairs
        SaveSystem.OnSave += SaveStairs;
        // Also save the amount of interactables
        SaveSystem.OnSave += SaveInteractableAmount;
        // Also save the enemies and amount of enemies
        SaveSystem.OnSave += SaveEnemiesAndEnemyAmount;
        // Also save the allies and amount of allies
        SaveSystem.OnSave += SaveAlliesAndAllyAmount;
    }
    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        SaveSystem.OnSave -= SaveRoomAmount;
        SaveSystem.OnSave -= SaveBleedLightAmount;
        SaveSystem.OnSave -= SaveWallsAndWallAmount;
        SaveSystem.OnSave -= SaveStairs;
        SaveSystem.OnSave -= SaveInteractableAmount;
        SaveSystem.OnSave -= SaveEnemiesAndEnemyAmount;
        SaveSystem.OnSave -= SaveAlliesAndAllyAmount;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        SaveSystem.OnSave -= SaveRoomAmount;
        SaveSystem.OnSave -= SaveBleedLightAmount;
        SaveSystem.OnSave -= SaveWallsAndWallAmount;
        SaveSystem.OnSave -= SaveStairs;
        SaveSystem.OnSave -= SaveInteractableAmount;
        SaveSystem.OnSave -= SaveEnemiesAndEnemyAmount;
        SaveSystem.OnSave -= SaveAlliesAndAllyAmount;
    }

    /// <summary>
    /// Initializes the references to things that did not exist prior to generation
    /// </summary>
    private void Initialize()
    {
        // Set the room parent
        _roomParent = GameObject.Find(ProceduralGenerationController.ROOM_PARENT_NAME).transform;
        // Set the bleed light parent
        _bleedLightParent = GameObject.Find(ProceduralGenerationController.BLEEDLIGHT_PARENT_NAME).transform;
        // Set the wall parent
        _wallParent = GameObject.Find(ProceduralGenerationController.WALL_PARENT_NAME).transform;
        // Set the stairs transform
        _stairsTrans = GameObject.Find(ProceduralGenerationController.STAIRS_NAME).transform;
        // Set the interactable parent
        _interactableParent = GameObject.Find(ProceduralGenerationController.INTERACT_PARENT_NAME).transform;
        // Set the enemy parent
        _enemyParent = GameObject.Find(ProceduralGenerationController.ENEMY_PARENT_NAME).transform;
        // Set the ally parent
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;
    }

    /// <summary>
    /// Saves the amount of rooms we have to a binary file
    /// </summary>
    private void SaveRoomAmount()
    {
        SaveSystem.SaveRoomAmount(_roomParent);
    }

    /// <summary>
    /// Saves the amount of bleed lights we have to a binary file
    /// </summary>
    private void SaveBleedLightAmount()
    {
        SaveSystem.SaveBleedLightAmount(_bleedLightParent);
    }

    /// <summary>
    /// Saves the amount of walls we have to a binary file.
    /// Also saves each wall to a binary file.
    /// </summary>
    private void SaveWallsAndWallAmount()
    {
        SaveSystem.SaveWallAmount(_wallParent);
        // Save each wall
        foreach (Transform wall in _wallParent)
        {
            SaveSystem.SaveWall(wall);
        }
    }

    /// <summary>
    /// Saves the stairs to a binary file
    /// </summary>
    private void SaveStairs()
    {
        SaveSystem.SaveStairs(_stairsTrans);
    }

    /// <summary>
    /// Saves the amount of interactables we have to a binary file
    /// </summary>
    private void SaveInteractableAmount()
    {
        SaveSystem.SaveInteractableAmount(_interactableParent);
    }

    /// <summary>
    /// Saves the amount of enemies we have to a binary file.
    /// Also saves each enemy to a binary file depending on their sibling index
    /// </summary>
    private void SaveEnemiesAndEnemyAmount()
    {
        // Save the amount of enemies
        SaveSystem.SaveEnemyAmount(_enemyParent);

        int counter = 0;
        // Iterate over each enemy
        foreach (Transform enemyTrans in _enemyParent)
        {
            // Save the current enemy
            SaveSystem.SaveEnemy(enemyTrans.gameObject);
        }
    }

    /// <summary>
    /// Saves the amount of allies we have to a binary file.
    /// Also saves each ally to a binary file depending on their sibling index
    /// </summary>
    private void SaveAlliesAndAllyAmount()
    {
        // Save the amount of allies
        SaveSystem.SaveAllyAmount(_allyParent);

        // Iterate over each ally
        foreach (Transform allyTrans in _allyParent)
        {
            // Save the current ally
            SaveSystem.SaveAlly(allyTrans.gameObject);
        }
    }
}
