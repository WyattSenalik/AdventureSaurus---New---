using UnityEngine;

[System.Serializable]
public class RoomData
{
    /// Sibling Indices of references to other objects
    /// All of these are children of some parent
    // BleedLightParent children
    public int[] broadcastLightSiblingIndices;
    public int[] receiveLightSiblingIndices;
    // RoomParent children
    public int[] adjRoomsSiblingIndices;
    // CharacterParent children
    public int[] alliesInRoomSiblingIndicies;
    public int[] enemiesInRoomSiblingIndicies;
    /// Simple primitives
    public bool isRoomActive;
    public float currentLightIntensity;
    public bool clear;
    public int roomWeight;
    public int myRoomType;
    public int roomDifficulty;
    /// Essential parts of non primitives
    public float[] sprRendColor;
    public float[] position;
    public float[] scale;

    public RoomData (Room room)
    {
        // BroadcastLights
        broadcastLightSiblingIndices = new int[room.BroadcastLights.Count];
        for (int i = 0; i < room.BroadcastLights.Count; ++i)
            broadcastLightSiblingIndices[i] = room.BroadcastLights[i].transform.GetSiblingIndex();
        // ReceiveLights
        receiveLightSiblingIndices = new int[room.ReceiveLights.Count];
        for (int i = 0; i < room.ReceiveLights.Count; ++i)
            receiveLightSiblingIndices[i] = room.ReceiveLights[i].transform.GetSiblingIndex();
        // AdjacentRooms
        adjRoomsSiblingIndices = new int[room.AdjacentRooms.Count];
        for (int i = 0; i < room.AdjacentRooms.Count; ++i)
            adjRoomsSiblingIndices[i] = room.ReceiveLights[i].transform.GetSiblingIndex();
        // AlliesInRoom
        alliesInRoomSiblingIndicies = new int[room.GetAlliesInRoom().Count];
        for (int i = 0; i < room.GetAlliesInRoom().Count; ++i)
            alliesInRoomSiblingIndicies[i] = room.GetAlliesInRoom()[i].transform.GetSiblingIndex();
        // EnemiesInRoom
        enemiesInRoomSiblingIndicies = new int[room.GetEnemiesInRoom().Count];
        for (int i = 0; i < room.GetEnemiesInRoom().Count; ++i)
            enemiesInRoomSiblingIndicies[i] = room.GetEnemiesInRoom()[i].transform.GetSiblingIndex();

        // If the room is active
        isRoomActive = room.GetIsRoomActive();
        // The current light intensity
        currentLightIntensity = room.GetCurrentLightIntensity();
        // If the room has been cleared
        clear = room.GetClear();
        // The weight of the room
        roomWeight = room.RoomWeight;
        // The type of room it is
        myRoomType = (int) room.MyRoomType;
        // The difficulty of the room
        roomDifficulty = room.RoomDifficulty;

        // Get the color from the spriteRenderer
        SpriteRenderer sprRendRef = room.GetComponent<SpriteRenderer>();
        if (sprRendRef == null)
        {
            Debug.LogError("No Sprite Renderer on Room " + room.name);
        }
        else {
            sprRendColor = new float[4];
            sprRendColor[0] = sprRendRef.color.r;
            sprRendColor[1] = sprRendRef.color.g;
            sprRendColor[2] = sprRendRef.color.b;
            sprRendColor[3] = sprRendRef.color.a;
        }
        // Get the position from the transform
        position = new float[3];
        position[0] = room.transform.position.x;
        position[1] = room.transform.position.y;
        position[2] = room.transform.position.z;
        // Get the scale from the transform
        scale = new float[3];
        scale[0] = room.transform.localScale.x;
        scale[1] = room.transform.localScale.y;
        scale[2] = room.transform.localScale.z;
    }
}
