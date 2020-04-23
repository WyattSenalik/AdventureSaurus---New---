using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    // Events
    // For saving the game
    public delegate void Save();
    public static event Save OnSave;

    /// <summary>
    /// Calls the OnSave event
    /// </summary>
    public static void SaveGame()
    {
        if (OnSave != null)
            OnSave();
    }

    //// Rooms
    /// <summary>
    /// Saves the amount of rooms in a binary file at /roomAmount.num
    /// </summary>
    /// <param name="roomParent"></param>
    public static void SaveRoomAmount(Transform roomParent)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/roomAmount.num";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        ChildAmountData data = new ChildAmountData(roomParent);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the amount of rooms from a binary file at /roomAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadRoomAmount()
    {
        // The attempted path
        string path = Application.persistentDataPath + "/roomAmount.num";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            ChildAmountData data = formatter.Deserialize(stream) as ChildAmountData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    /// <summary>
    /// Saves the data for a room in a binary file at /roomX.rm where X is the sibling index of the room
    /// </summary>
    /// <param name="room">Room whose data we want to save</param>
    public static void SaveRoom(Room room)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/room" + room.transform.GetSiblingIndex().ToString() + ".rm";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        RoomData data = new RoomData(room);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads saved data for a room from a binary file at /roomX.rm where X is the sibling index of the room
    /// </summary>
    /// <param name="roomSiblingIndex">Index of the room we want to load</param>
    /// <returns>RoomData. The data loaded from the file</returns>
    public static RoomData LoadRoom(int roomSiblingIndex)
    {
        // The attempted path
        string path = Application.persistentDataPath + "/room" + roomSiblingIndex.ToString() + ".rm";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            RoomData data = formatter.Deserialize(stream) as RoomData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }


    //// BleedLights
    /// <summary>
    /// Saves the amount of bleed lights in a binary file at /bleedLightAmount.num
    /// </summary>
    /// <param name="bleedLightParent"></param>
    public static void SaveBleedLightAmount(Transform bleedLightParent)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/bleedLightAmount.num";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        ChildAmountData data = new ChildAmountData(bleedLightParent);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the amount of bleed lights from a binary file at /bleedLightAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadBleedLightAmount()
    {
        // The attempted path
        string path = Application.persistentDataPath + "/bleedLightAmount.num";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            ChildAmountData data = formatter.Deserialize(stream) as ChildAmountData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    /// <summary>
    /// Saves the data for a bleed light in a binary file at /bleedLightX.bl where X is the sibling index of the bleedLight
    /// </summary>
    /// <param name="bleedLight">Bleed light whose data we want to save</param>
    public static void SaveBleedLight(BleedLight bleedLight)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/bleedLight" + bleedLight.transform.GetSiblingIndex().ToString() + ".bl";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        BleedLightData data = new BleedLightData(bleedLight);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads saved data for a bleed light from a binary file at /bleedLightX.bl where X is the sibling index of the room
    /// </summary>
    /// <param name="bleedLightSiblingIndex">Index of the bleed light we want to load</param>
    /// <returns>BleedLightData. The data loaded from the file</returns>
    public static BleedLightData LoadBleedLight(int bleedLightSiblingIndex)
    {
        // The attempted path
        string path = Application.persistentDataPath + "/bleedLight" + bleedLightSiblingIndex.ToString() + ".bl";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            BleedLightData data = formatter.Deserialize(stream) as BleedLightData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }


    //// Walls
    /// <summary>
    /// Saves the amount of walls in a binary file at /wallAmount.num
    /// </summary>
    /// <param name="wallParent">Parent of all the walls we will be saving</param>
    public static void SaveWallAmount(Transform wallParent)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/wallAmount.num";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        ChildAmountData data = new ChildAmountData(wallParent);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the amount of walls from a binary file at /wallAmount.num
    /// </summary>
    /// <returns>ChildAmountData. The data loaded from the file</returns>
    public static ChildAmountData LoadWallAmount()
    {
        // The attempted path
        string path = Application.persistentDataPath + "/wallAmount.num";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            ChildAmountData data = formatter.Deserialize(stream) as ChildAmountData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    /// <summary>
    /// Saves the data for a wall in a binary file at /wallX.wl where X is the sibling index of the room
    /// </summary>
    /// <param name="wall">Transform of wall whose data we want to save</param>
    public static void SaveWall(Transform wall)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/wall" + wall.GetSiblingIndex().ToString() + ".wl";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        WallData data = new WallData(wall);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads saved data for a wall from a binary file at /wallX.wl where X is the sibling index of the room
    /// </summary>
    /// <param name="wallSiblingIndex">Index of the wall we want to load</param>
    /// <returns>WallData. The data loaded from the file</returns>
    public static WallData LoadWall(int wallSiblingIndex)
    {
        // The attempted path
        string path = Application.persistentDataPath + "/wall" + wallSiblingIndex.ToString() + ".wl";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            WallData data = formatter.Deserialize(stream) as WallData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }


    //// Stairs
    /// <summary>
    /// Saves the data for the stairs in a binary file at /stairs.st
    /// </summary>
    /// <param name="stairs">Transform of stairs whose data we want to save</param>
    public static void SaveStairs(Transform stairs)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/stairs.st";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        StairsData data = new StairsData(stairs);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads saved data for the stairs from a binary file at /stairs.st
    /// </summary>
    /// <returns>StairsData. The data loaded from the file</returns>
    public static StairsData LoadStairs()
    {
        // The attempted path
        string path = Application.persistentDataPath + "/stairs.st";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            StairsData data = formatter.Deserialize(stream) as StairsData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }


    //// Tilemap
    /// <summary>
    /// Saves the amount of tiles in a binary file at /tilemapBounds.twopos
    /// </summary>
    /// <param name="botLeft">The bottomleft-most position of the tilemap</param>
    /// <param name="topRight">The topRight-most position of the tilemap</param>
    public static void SaveTilemapBounds(Vector2Int botLeft, Vector2Int topRight)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/tilemapBounds.twopos";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        TilemapBoundsData data = new TilemapBoundsData(botLeft, topRight);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the bounds of the tilemap from a binary file at /tilemapBounds.twopos
    /// </summary>
    /// <returns>TilemapBoundsData. The data loaded from the file</returns>
    public static TilemapBoundsData LoadTilemapBounds()
    {
        // The attempted path
        string path = Application.persistentDataPath + "/tilemapBounds.twopos";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            TilemapBoundsData data = formatter.Deserialize(stream) as TilemapBoundsData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    /// <summary>
    /// Saves the data for a tile from the tilemap in a binary file at /tile_X_Y_Z.tl where XYZ is the position of the tile
    /// </summary>
    /// <param name="tilePos">Position of the tile we will save</param>
    /// <param name="tileKey">Key of the tile we will save</param>
    public static void SaveTile(Vector3Int tilePos, int tileKey)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // Location of the file
        string path = Application.persistentDataPath + "/tile_" + tilePos.x.ToString() + "_" + tilePos.y.ToString() +
           "_" + tilePos.z.ToString() + ".tl";
        // Open a connection to the file
        FileStream stream = new FileStream(path, FileMode.Create);

        // Data to put in the file
        TileSaveData data = new TileSaveData(tilePos, tileKey);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads saved data for a tile from a binary file at /tileXYZ.tl where XYZ is the position of the tile
    /// </summary>
    /// <param name="tilePos">Position of the tile we will are loading</param>
    /// <returns>TileSaveData. The data loaded from the file</returns>
    public static TileSaveData LoadTile(Vector3Int tilePos)
    {
        // The attempted path
        string path = Application.persistentDataPath + "/tile_" + tilePos.x.ToString() + "_" + tilePos.y.ToString() +
           "_" + tilePos.z.ToString() + ".tl";
        // If there is a file there
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Open a connection to the file
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create data from the file
            TileSaveData data = formatter.Deserialize(stream) as TileSaveData;
            // Close the connection to the file
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
