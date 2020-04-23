﻿using UnityEngine;
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
        RoomAmountData data = new RoomAmountData(roomParent);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the amount of rooms from a binary file at /roomAmount.num
    /// </summary>
    /// <returns>RoomAmountData. The data loaded from the file</returns>
    public static RoomAmountData LoadRoomAmount()
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
            RoomAmountData data = formatter.Deserialize(stream) as RoomAmountData;
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
        BleedLightAmountData data = new BleedLightAmountData(bleedLightParent);

        // Write to the file
        formatter.Serialize(stream, data);
        // Close the connection to the file
        stream.Close();
    }
    /// <summary>
    /// Loads the amount of bleed lights from a binary file at /bleedLightAmount.num
    /// </summary>
    /// <returns>BleedLightAmountData. The data loaded from the file</returns>
    public static BleedLightAmountData LoadBleedLightAmount()
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
            BleedLightAmountData data = formatter.Deserialize(stream) as BleedLightAmountData;
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
}
