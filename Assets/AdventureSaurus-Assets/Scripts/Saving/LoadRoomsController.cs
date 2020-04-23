using System.Collections.Generic;
using UnityEngine;

public static class LoadRoomsController
{
    /// <summary>
    /// Creates room objects based on the stored room data.
    /// Does not set references to any other game objects for the rooms
    /// </summary>
    /// <param name="roomsParent">Transform that will serve as the parent of the new rooms</param>
    /// <param name="roomPrefab">Prefab of a room</param>
    public static void LoadRooms(Transform roomsParent, GameObject roomPrefab)
    {
        // Get the amount of rooms
        ChildAmountData roomAmData = SaveSystem.LoadRoomAmount();
        int amountRooms = roomAmData.GetChildAmount();

        // Load each room
        for (int i = 0; i < amountRooms; ++i)
        {
            // Get the data for the room
            RoomData roomData = SaveSystem.LoadRoom(i);
            // Create the new room as a child of the rooms parent
            GameObject roomObj = Object.Instantiate(roomPrefab, roomsParent);

            // Set its transform components
            roomObj.transform.position = roomData.GetPosition();
            roomObj.transform.localScale = roomData.GetScale();

            // Get its Room script
            Room roomScrRef = roomObj.GetComponent<Room>();
            if (roomScrRef == null)
            {
                Debug.LogError("No Room script attached to " + roomObj.name);
                return;
            }
            // Set all the room script variables
            roomScrRef.SetIsRoomActive(roomData.GetIsRoomActive());
            roomScrRef.SetCurrentLightIntensity(roomData.GetCurrentLightIntensity());
            roomScrRef.SetClear(roomData.GetClear());
            roomScrRef.RoomWeight = roomData.GetRoomWeight();
            roomScrRef.MyRoomType = roomData.GetMyRoomType();
            roomScrRef.RoomDifficulty = roomData.GetRoomDifficulty();

            // Get its SpriteRenderer
            SpriteRenderer sprRendRef = roomObj.GetComponent<SpriteRenderer>();
            if (sprRendRef == null)
            {
                Debug.LogError("No SpriteRenderer attached to " + roomObj.name);
                return;
            }
            // Set the color of the sprite renderer
            sprRendRef.color = roomData.GetSpriteRendererColor();
        }
    }

    /// <summary>
    /// Creates bleed light objects based on the stored bleed light data.
    /// Does not set references to any other game objects for the bleed lights
    /// </summary>
    /// <param name="bleedLightParent">Transform that will serve as the parent of the new bleed lights</param>
    /// <param name="bleedLightPrefab">Prefab of a bleed light</param>
    public static void LoadBleedLights(Transform bleedLightParent, GameObject bleedLightPrefab)
    {
        // Get the amount of bleed lights
        ChildAmountData bleedLightAmData = SaveSystem.LoadBleedLightAmount();
        int amountBleedLights = bleedLightAmData.GetChildAmount();

        // Load each bleed light
        for (int i = 0; i < amountBleedLights; ++i)
        {
            // Get the data saved for the bleed light
            BleedLightData bleedLightData = SaveSystem.LoadBleedLight(i);
            // Create the new bleed light as a child of the bleed light parent
            GameObject bleedLightObj = Object.Instantiate(bleedLightPrefab, bleedLightParent);

            // Set its transform components
            bleedLightObj.transform.position = bleedLightData.GetPosition();
            bleedLightObj.transform.rotation = bleedLightData.GetRotation();

            // Get its SpriteRenderer
            SpriteRenderer sprRendRef = bleedLightObj.GetComponent<SpriteRenderer>();
            if (sprRendRef == null)
            {
                Debug.LogError("No SpriteRenderer attached to " + bleedLightObj.name);
                return;
            }
            // Set the color of the sprite renderer
            sprRendRef.color = bleedLightData.GetSpriteRendererColor();
        }
    }

    /// <summary>
    /// Sets the references of the rooms and bleed lights based on saved data
    /// </summary>
    /// <param name="roomParent">Parent of all the rooms</param>
    /// <param name="bleedLightsParent">Parent of all the bleed lights</param>
    public static void LoadReferences(Transform roomParent, Transform bleedLightsParent)
    {
        // Load the room references
        LoadRoomReferences(roomParent, bleedLightsParent);
        // Load the bleed light references
        LoadBleedLightReferences(roomParent, bleedLightsParent);
    }

    /// <summary>
    /// Sets room references based on their data
    /// </summary>
    /// <param name="roomParent">Parent of all the rooms</param>
    /// <param name="bleedLightsParent">Parent of all the bleed lights</param>
    private static void LoadRoomReferences(Transform roomParent, Transform bleedLightsParent)
    {
        // Get the amount of rooms
        int amountRooms = roomParent.childCount;

        // Load each room
        for (int i = 0; i < amountRooms; ++i)
        {
            // Get the data for the room
            RoomData roomData = SaveSystem.LoadRoom(i);
            // Get the actual room
            Room curRoom = roomParent.GetChild(i).GetComponent<Room>();

            // Set its references based on the data
            // Broadcast lights
            List<BleedLight> newBroadcastLights = new List<BleedLight>();
            foreach (int siblingIndex in roomData.GetBroadcastLightSiblingIndices())
            {
                // Get the broadcast light with this sibling index
                BleedLight broadcastLight = bleedLightsParent.GetChild(siblingIndex).GetComponent<BleedLight>();
                // Add it to the broadcast lights
                newBroadcastLights.Add(broadcastLight);
            }
            curRoom.SetBroadcastLights(newBroadcastLights);
            // Receive lights
            List<BleedLight> newReceiveLights = new List<BleedLight>();
            foreach (int siblingIndex in roomData.GetReceiveLightSiblingIndices())
            {
                // Get the receive light with this sibling index
                BleedLight receiveLight = bleedLightsParent.GetChild(siblingIndex).GetComponent<BleedLight>();
                // Add it to the receive lights
                newReceiveLights.Add(receiveLight);
            }
            curRoom.SetReceiveLights(newReceiveLights);
            // Adjacent rooms
            List<Room> newAdjRooms = new List<Room>();
            Debug.Log("ChildCount: " + roomParent.childCount + ", " + roomData.GetAdjRoomsSiblingIndices().Length);
            foreach (int siblingIndex in roomData.GetAdjRoomsSiblingIndices())
            {
                Debug.Log("SiblingIndex: " + siblingIndex);
                // Get the adjacent room with this sibling index
                Room adjRoom = roomParent.GetChild(siblingIndex).GetComponent<Room>();
                // Add it to the adjacent rooms
                newAdjRooms.Add(adjRoom);
            }
            curRoom.SetAdjRooms(newAdjRooms);
        }
    }

    /// <summary>
    /// Sets bleed light references based on their data
    /// </summary>
    /// <param name="roomParent">Parent of all the rooms</param>
    /// <param name="bleedLightsParent">Parent of all the bleed lights</param>
    private static void LoadBleedLightReferences(Transform roomParent, Transform bleedLightsParent)
    {
        // Get the amount of bleed lights
        int amountBleedLights = bleedLightsParent.childCount;

        // Load each bleed light
        for (int i = 0; i < amountBleedLights; ++i)
        {
            // Get the data saved for the bleed light
            BleedLightData bleedLightData = SaveSystem.LoadBleedLight(i);
            // Get the actual bleed light
            BleedLight curBleedLight = bleedLightsParent.GetChild(i).GetComponent<BleedLight>();

            // Set its references based on the data
            // Broadcast room
            int broadcastSiblingIndex = bleedLightData.GetBroadcastRoomSiblingIndex();
            Room newBroadcastRoom = roomParent.GetChild(broadcastSiblingIndex).GetComponent<Room>();
            curBleedLight.BroadcastRoom = newBroadcastRoom;
            // Receive room
            int receiveSiblingIndex = bleedLightData.GetReceiveRoomSiblingIndex();
            Room newReceiveRoom = roomParent.GetChild(receiveSiblingIndex).GetComponent<Room>();
            curBleedLight.ReceiveRoom = newReceiveRoom;
        }
    }

}
