﻿using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    // The folder we are to be saving in
    private static string _folderDir = "";

    // Events
    // For saving the game
    public delegate void Save();
    public static event Save OnSave;

    /// <summary>
    /// Calls the OnSave event
    /// </summary>
    public static void SaveGame()
    {
        RefreshMainSaveFolder();
        OnSave?.Invoke();
    }

    /// <summary>
    /// For setting the folder we will be saving in.
    /// If the folder exists, we destroy it.
    /// We create a new folder with the given name.
    /// All the files saved after this is called will be saved in that folder
    /// </summary>
    /// <param name="folderName">String name of the folder we will be saving in. Does not expect / before it</param>
    public static void RefreshMainSaveFolder(string folderName)
    {
        // Set the folder directory
        _folderDir = "/" + folderName;
        // Get the fullPath
        string fullPath = Application.persistentDataPath + _folderDir;
        // Delete any already existing directory
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            dir.Delete(true);
        }
        // Create a new folder with the path
        Directory.CreateDirectory(fullPath);
    }

    /// <summary>
    /// For resetting the folder we are saving in
    /// </summary>
    public static void RefreshMainSaveFolder()
    {
        if (_folderDir != "")
        {
            // Get the fullPath
            string fullPath = Application.persistentDataPath + _folderDir;
            // Delete any already existing directory
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo dir = new DirectoryInfo(fullPath);
                dir.Delete(true);
            }
            // Create a new folder with the path
            Directory.CreateDirectory(fullPath);
        }
    }

    /// <summary>
    /// Checks if there is a checkpoint to be loaded or not
    /// </summary>
    /// <returns>True if there is a saved checpoint</returns>
    public static bool CheckForCheckpoint()
    {
        // Get the fullPath to the starting room
        string fullPath = Application.persistentDataPath + _folderDir + "/room0.rm";
        // If the first room exists, there is a checkpoint
        if (File.Exists(fullPath))
        {
            return true;
        }
        // Otherwise, if room0 does not exist, there is no checkpoint
        return false;
    }

    //// Base Save and Load
    /// <summary>
    /// Saves the passed in data in a binary file at the specified path
    /// </summary>
    /// <param name="data">Data to save</param>
    /// <param name="additionalPath">What to save the file as. Must start with '/'</param>
    private static void SaveData(System.Object data, string additionalPath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + _folderDir + additionalPath;
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads data from a binary file with the specified path
    /// </summary>
    /// <param name="additionalPath">Name of the file to load. Must start with '/'</param>
    /// <returns>Data as System.Object. Cast to the desired data type with an 'as DataType'</returns>
    private static T LoadData<T>(string additionalPath)
    {
        // The attempted path
        string path = Application.persistentDataPath + _folderDir + additionalPath;
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            T data = (T) formatter.Deserialize(stream);
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return default;
        }
    }


    //// Child Amount
    /// <summary>
    /// Saves the amount of children of the passed in parent
    /// </summary>
    /// <param name="parent">Transform whose childCount we will save</param>
    /// <param name="additionalPath">What to save the file as. Must start with '/'</param>
    private static void SaveChildAmount(Transform parent, string additionalPath)
    {
        // Data to put in the file
        ChildAmountData data = new ChildAmountData(parent);
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads the amount of children a parent had from a binary file at the specified additional path
    /// </summary>
    /// <param name="additionalPath">Path to load the data from</param>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    private static ChildAmountData LoadChildAmount(string additionalPath)
    {
        // Load the data as ChildAmountData
        return LoadData<ChildAmountData>(additionalPath);
    }


    //// Rooms
    /// <summary>
    /// Saves the amount of rooms in a binary file at /roomAmount.num
    /// </summary>
    /// <param name="roomParent">Parent of the rooms to save</param>
    public static void SaveRoomAmount(Transform roomParent)
    {
        SaveChildAmount(roomParent, "/roomAmount.num");
    }
    /// <summary>
    /// Loads the amount of rooms from a binary file at /roomAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadRoomAmount()
    {
        return LoadChildAmount("/roomAmount.num");
    }

    /// <summary>
    /// Saves the data for a room in a binary file at /roomX.rm where X is the sibling index of the room
    /// </summary>
    /// <param name="room">Room whose data we want to save</param>
    public static void SaveRoom(Room room)
    {
        // Data to put in the file
        RoomData data = new RoomData(room);
        // Save the data
        string additionalPath = "/room" + room.transform.GetSiblingIndex().ToString() + ".rm";
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads saved data for a room from a binary file at /roomX.rm where X is the sibling index of the room
    /// </summary>
    /// <param name="roomSiblingIndex">Index of the room we want to load</param>
    /// <returns>RoomData. The data loaded from the file</returns>
    public static RoomData LoadRoom(int roomSiblingIndex)
    {
        // Return the data as RoomData
        string additionalPath = "/room" + roomSiblingIndex.ToString() + ".rm";
        return LoadData<RoomData>(additionalPath);
    }


    //// BleedLights
    /// <summary>
    /// Saves the amount of bleed lights in a binary file at /bleedLightAmount.num
    /// </summary>
    /// <param name="bleedLightParent"></param>
    public static void SaveBleedLightAmount(Transform bleedLightParent)
    {
        SaveChildAmount(bleedLightParent, "/bleedLightAmount.num");
    }
    /// <summary>
    /// Loads the amount of bleed lights from a binary file at /bleedLightAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadBleedLightAmount()
    {
        return LoadChildAmount("/bleedLightAmount.num");
    }

    /// <summary>
    /// Saves the data for a bleed light in a binary file at /bleedLightX.bl where X is the sibling index of the bleedLight
    /// </summary>
    /// <param name="bleedLight">Bleed light whose data we want to save</param>
    public static void SaveBleedLight(BleedLight bleedLight)
    {
        // Data to put in the file
        BleedLightData data = new BleedLightData(bleedLight);
        // File name
        string additionalPath = "/bleedLight" + bleedLight.transform.GetSiblingIndex().ToString() + ".bl";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads saved data for a bleed light from a binary file at /bleedLightX.bl where X is the sibling index of the room
    /// </summary>
    /// <param name="bleedLightSiblingIndex">Index of the bleed light we want to load</param>
    /// <returns>BleedLightData. The data loaded from the file</returns>
    public static BleedLightData LoadBleedLight(int bleedLightSiblingIndex)
    {
        // File name
        string additionalPath = "/bleedLight" + bleedLightSiblingIndex.ToString() + ".bl";
        // Return the data as BleedLightData
        return LoadData<BleedLightData>(additionalPath);
    }


    //// Walls
    /// <summary>
    /// Saves the amount of walls in a binary file at /wallAmount.num
    /// </summary>
    /// <param name="wallParent">Parent of all the walls we will be saving</param>
    public static void SaveWallAmount(Transform wallParent)
    {
        SaveChildAmount(wallParent, "/wallAmount.num");
    }
    /// <summary>
    /// Loads the amount of walls from a binary file at /wallAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadWallAmount()
    {
        return LoadChildAmount("/wallAmount.num");
    }

    /// <summary>
    /// Saves the data for a wall in a binary file at /wallX.wl where X is the sibling index of the room
    /// </summary>
    /// <param name="wall">Transform of wall whose data we want to save</param>
    public static void SaveWall(Transform wall)
    {
        // Data to put in the file
        WallData data = new WallData(wall);
        // File Name
        string additionalPath = "/wall" + wall.GetSiblingIndex().ToString() + ".wl";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads saved data for a wall from a binary file at /wallX.wl where X is the sibling index of the room
    /// </summary>
    /// <param name="wallSiblingIndex">Index of the wall we want to load</param>
    /// <returns>WallData. The data loaded from the file</returns>
    public static WallData LoadWall(int wallSiblingIndex)
    {
        // File Name
        string additionalPath = "/wall" + wallSiblingIndex.ToString() + ".wl";
        // Return the data as WallData
        return LoadData<WallData>(additionalPath);
    }


    //// Stairs
    /// <summary>
    /// Saves the data for the stairs in a binary file at /stairs.st
    /// </summary>
    /// <param name="stairs">Transform of stairs whose data we want to save</param>
    public static void SaveStairs(Transform stairs)
    {
        // File Name
        string additionalPath = "/stairs.st";
        // Data to put in the file
        StairsData data = new StairsData(stairs);
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads saved data for the stairs from a binary file at /stairs.st
    /// </summary>
    /// <returns>StairsData. The data loaded from the file</returns>
    public static StairsData LoadStairs()
    {
        // File Name
        string additionalPath = "/stairs.st";
        // Return the data as StairsData
        return LoadData<StairsData>(additionalPath);
    }


    //// Tilemap
    /// <summary>
    /// Saves the amount of tiles in a binary file at /tilemapBounds.twopos
    /// </summary>
    /// <param name="botLeft">The bottomleft-most position of the tilemap</param>
    /// <param name="topRight">The topRight-most position of the tilemap</param>
    public static void SaveTilemapBounds(Vector2Int botLeft, Vector2Int topRight)
    {
        // Data to put in the file
        TilemapBoundsData data = new TilemapBoundsData(botLeft, topRight);
        // File Name
        string additionalPath = "/tilemapBounds.twopos";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads the bounds of the tilemap from a binary file at /tilemapBounds.twopos
    /// </summary>
    /// <returns>TilemapBoundsData. The data loaded from the file</returns>
    public static TilemapBoundsData LoadTilemapBounds()
    {
        // File Name
        string additionalPath = "/tilemapBounds.twopos";
        // Return the data as TilemapBoundsData
        return LoadData<TilemapBoundsData>(additionalPath);
    }

    /// <summary>
    /// Saves the data for a tile from the tilemap in a binary file at /tile_X_Y_Z.tl where XYZ is the position of the tile
    /// </summary>
    /// <param name="tilePos">Position of the tile we will save</param>
    /// <param name="tileKey">Key of the tile we will save</param>
    public static void SaveTile(Vector3Int tilePos, int tileKey)
    {
        // Data to put in the file
        TileSaveData data = new TileSaveData(tilePos, tileKey);
        // File Name
        string additionalPath = "/tile_" + tilePos.x.ToString() + "_" + tilePos.y.ToString() +
           "_" + tilePos.z.ToString() + ".tl";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads saved data for a tile from a binary file at /tileXYZ.tl where XYZ is the position of the tile
    /// </summary>
    /// <param name="tilePos">Position of the tile we will are loading</param>
    /// <returns>TileSaveData. The data loaded from the file</returns>
    public static TileSaveData LoadTile(Vector3Int tilePos)
    {
        // File Name
        string additionalPath = "/tile_" + tilePos.x.ToString() + "_" + tilePos.y.ToString() +
           "_" + tilePos.z.ToString() + ".tl";
        // Return the data as TileSaveData
        return LoadData<TileSaveData>(additionalPath);
    }


    //// Interactables
    /// <summary>
    /// Saves the amount of interactables in a binary file at /interactableAmount.num
    /// </summary>
    /// <param name="interactablesParent">Parent of all the interactables we will be saving</param>
    public static void SaveInteractableAmount(Transform interactablesParent)
    {
        SaveChildAmount(interactablesParent, "/interactableAmount.num");
    }
    /// <summary>
    /// Loads the amount of interactables from a binary file at /interactableAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadInteractableAmount()
    {
        return LoadChildAmount("/interactableAmount.num");
    }

    /// <summary>
    /// Saves the data for an interactable in a binary file at /interactableX.inac where X is the interactable's sibling index
    /// </summary>
    /// <param name="interact">Interactable whose data we want to save</param>
    public static void SaveInteractable(Interactable interact)
    {
        // Interact data to put in the file
        InteractableData data = new InteractableData(interact);
        // File name
        string additionalPath = "/interactable" + interact.transform.GetSiblingIndex() + ".inac";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads save data for an Interactable from a binary file at /interactableX.inac where X is the interactable's sibling index
    /// </summary>
    /// <param name="interactableSiblingIndex">Sibling index of the Interactable we want to load data for</param>
    /// <returns>InteractableData. The data loaded from the file</returns>
    public static InteractableData LoadInteractable(int interactableSiblingIndex)
    {
        // File name
        string additionalPath = "/interactable" + interactableSiblingIndex.ToString() + ".inac";
        // Return the data as InteractableData
        return LoadData<InteractableData>(additionalPath);
    }


    //// Enemies
    /// <summary>
    /// Saves the amount of enemies in a binary file at /enemyAmount.num
    /// </summary>
    /// <param name="enemyParent">Parent of all the enemies</param>
    public static void SaveEnemyAmount(Transform enemyParent)
    {
        SaveChildAmount(enemyParent, "/enemyAmount.num");
    }
    /// <summary>
    /// Loads the amount of enemies from a binary file at /enemyAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadEnemyAmount()
    {
        return LoadChildAmount("/enemyAmount.num");
    }

    /// <summary>
    /// Saves the data for an enemy in a binary file at /enemyX.en where X is the enemy's sibling index
    /// </summary>
    /// <param name="enemy">Enemy whose data we want to save</param>
    public static void SaveEnemy(GameObject enemy)
    {
        // Enemy data to put in the file
        EnemyData data = new EnemyData(enemy);
        // File name
        string additionalPath = "/enemy" + enemy.transform.GetSiblingIndex() + ".en";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads save data for an Enemy from a binary file at /enemyX.en where X is the enemy's sibling index
    /// </summary>
    /// <param name="enemySiblingIndex">Sibling index of the Enemy we want to load data for</param>
    /// <returns>EnemyData. The data loaded from the file</returns>
    public static EnemyData LoadEnemy(int enemySiblingIndex)
    {
        // File name
        string additionalPath = "/enemy" + enemySiblingIndex.ToString() + ".en";
        // Return the data as InteractableData
        return LoadData<EnemyData>(additionalPath);
    }


    //// Allies
    /// <summary>
    /// Saves the amount of allies in a binary file at /allyAmount.num
    /// </summary>
    /// <param name="allyParent">Parent of all the allies</param>
    public static void SaveAllyAmount(Transform allyParent)
    {
        SaveChildAmount(allyParent, "/allyAmount.num");
    }
    /// <summary>
    /// Loads the amount of allies from a binary file at /allyAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadAllyAmount()
    {
        return LoadChildAmount("/allyAmount.num");
    }

    /// <summary>
    /// Saves the data for an ally in a binary file at /allyX.ay where X is the ally's sibling index
    /// </summary>
    /// <param name="ally">Ally whose data we want to save</param>
    public static void SaveAlly(GameObject ally)
    {
        // Ally data to put in the file
        AllyData data = new AllyData(ally);
        // File name
        string additionalPath = "/ally" + ally.transform.GetSiblingIndex() + ".ay";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads save data for an Ally from a binary file at /allyX.ay where X is the ally's sibling index
    /// </summary>
    /// <param name="allySiblingIndex">Sibling index of the Ally we want to load data for</param>
    /// <returns>AllyData. The data loaded from the file</returns>
    public static AllyData LoadAlly(int allySiblingIndex)
    {
        // File name
        string additionalPath = "/ally" + allySiblingIndex.ToString() + ".ay";
        // Return the data as InteractableData
        return LoadData<AllyData>(additionalPath);
    }


    /// <summary>
    /// Saves the data for the PersistantController in a binary file at /persist.cont
    /// </summary>
    /// <param name="persistCont">PersistantController whose data we want to save</param>
    public static void SavePersistantFloorData(PersistantController persistCont)
    {
        // PersistantController data to put in the file
        PersistantFloorData data = new PersistantFloorData(persistCont);
        // File name
        string additionalPath = "/persist.cont";
        // Save the data
        SaveData(data, additionalPath);
    }
    /// <summary>
    /// Loads save data for the PersistantController from a binary file at /persist.cont
    /// </summary>
    /// <returns>PersistantFloorData. The data loaded from the file</returns>
    public static PersistantFloorData LoadPersistantFloorData()
    {
        // File name
        string additionalPath = "/persist.cont";
        // Return the data as PersistantFloorData
        return LoadData<PersistantFloorData>(additionalPath);
    }
}
