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
        RoomAmountData roomAmData = SaveSystem.LoadRoomAmount();
        int amountRooms = roomAmData.GetRoomAmount();

        // Load each room
        for (int i = 0; i < amountRooms; ++i)
        {
            // Get the data for the room
            RoomData roomData = SaveSystem.LoadRoom(i);
            // Create the new room as a child of the rooms parent
            GameObject roomObj = Object.Instantiate(roomPrefab, roomsParent);

            // Set its transform components
            Vector3 pos = new Vector3();
            pos.x = roomData.position[0];
            pos.y = roomData.position[1];
            pos.z = roomData.position[2];
            roomObj.transform.position = pos;
            Vector3 scale = new Vector3();
            scale.x = roomData.scale[0];
            scale.y = roomData.scale[1];
            scale.z = roomData.scale[2];
            roomObj.transform.localScale = scale;

            // Get its Room script
            Room roomScrRef = roomObj.GetComponent<Room>();
            if (roomScrRef == null)
            {
                Debug.LogError("No Room script attached to " + roomObj.name);
                return;
            }
            // Set all the room script variables
            roomScrRef.SetIsRoomActive(roomData.isRoomActive);
            roomScrRef.SetCurrentLightIntensity(roomData.currentLightIntensity);
            roomScrRef.SetClear(roomData.clear);
            roomScrRef.RoomWeight = roomData.roomWeight;
            roomScrRef.MyRoomType = (RoomType) roomData.myRoomType;
            roomScrRef.RoomDifficulty = roomData.roomDifficulty;

            // Get its SpriteRenderer
            SpriteRenderer sprRendRef = roomObj.GetComponent<SpriteRenderer>();
            if (sprRendRef == null)
            {
                Debug.LogError("No SpriteRenderer attached to " + roomObj.name);
                return;
            }
            // Set the color of the sprite renderer
            Color col = new Color();
            col.r = roomData.sprRendColor[0];
            col.g = roomData.sprRendColor[1];
            col.b = roomData.sprRendColor[2];
            col.a = roomData.sprRendColor[3];
            sprRendRef.color = col;
        }
    }
}
