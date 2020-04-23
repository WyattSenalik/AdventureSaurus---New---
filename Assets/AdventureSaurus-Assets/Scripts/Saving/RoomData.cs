using UnityEngine;

[System.Serializable]
public class RoomData
{
    /// Sibling Indices of references to other objects
    /// All of these are children of some parent
    // BleedLightParent children
    private int[] _broadcastLightSiblingIndices;
    private int[] _receiveLightSiblingIndices;
    // RoomParent children
    private int[] _adjRoomsSiblingIndices;
    // CharacterParent children
    private int[] _alliesInRoomSiblingIndicies;
    private int[] _enemiesInRoomSiblingIndicies;
    /// Simple primitives
    private bool _isRoomActive;
    private float _currentLightIntensity;
    private bool _clear;
    private int _roomWeight;
    private int _myRoomType;
    private int _roomDifficulty;
    /// Essential parts of non primitives
    private float[] _sprRendColor;
    private float[] _position;
    private float[] _scale;

    /// Getters
    public int[] GetBroadcastLightSiblingIndices() { return _broadcastLightSiblingIndices.Clone() as int[]; }
    public int[] GetReceiveLightSiblingIndices() { return _receiveLightSiblingIndices.Clone() as int[]; }
    public int[] GetAdjRoomsSiblingIndices() { return _adjRoomsSiblingIndices.Clone() as int[]; }
    public int[] GetAlliesInRoomSiblingIndices() { return _alliesInRoomSiblingIndicies.Clone() as int[]; }
    public int[] GetEnemiesInRoomSiblingIndices() { return _enemiesInRoomSiblingIndicies.Clone() as int[]; }

    public bool GetIsRoomActive() { return _isRoomActive; }
    public float GetCurrentLightIntensity() { return _currentLightIntensity; }
    public bool GetClear() { return _clear; }
    public int GetRoomWeight() { return _roomWeight; }
    public RoomType GetMyRoomType() { return (RoomType)_myRoomType; }
    public int GetRoomDifficulty() { return _roomDifficulty; }

    public Color GetSpriteRendererColor() { return new Color(_sprRendColor[0], _sprRendColor[1], _sprRendColor[2], _sprRendColor[3]); }
    public Vector3 GetPosition() { return new Vector3(_position[0], _position[1], _position[2]); }
    public Vector3 GetScale() { return new Vector3(_scale[0], _scale[1], _scale[2]); }

    public RoomData (Room room)
    {
        // BroadcastLights
        _broadcastLightSiblingIndices = new int[room.BroadcastLights.Count];
        for (int i = 0; i < room.BroadcastLights.Count; ++i)
            _broadcastLightSiblingIndices[i] = room.BroadcastLights[i].transform.GetSiblingIndex();
        // ReceiveLights
        _receiveLightSiblingIndices = new int[room.ReceiveLights.Count];
        for (int i = 0; i < room.ReceiveLights.Count; ++i)
            _receiveLightSiblingIndices[i] = room.ReceiveLights[i].transform.GetSiblingIndex();
        // AdjacentRooms
        _adjRoomsSiblingIndices = new int[room.AdjacentRooms.Count];
        for (int i = 0; i < room.AdjacentRooms.Count; ++i)
            _adjRoomsSiblingIndices[i] = room.ReceiveLights[i].transform.GetSiblingIndex();
        // AlliesInRoom
        _alliesInRoomSiblingIndicies = new int[room.GetAlliesInRoom().Count];
        for (int i = 0; i < room.GetAlliesInRoom().Count; ++i)
            _alliesInRoomSiblingIndicies[i] = room.GetAlliesInRoom()[i].transform.GetSiblingIndex();
        // EnemiesInRoom
        _enemiesInRoomSiblingIndicies = new int[room.GetEnemiesInRoom().Count];
        for (int i = 0; i < room.GetEnemiesInRoom().Count; ++i)
            _enemiesInRoomSiblingIndicies[i] = room.GetEnemiesInRoom()[i].transform.GetSiblingIndex();

        // If the room is active
        _isRoomActive = room.GetIsRoomActive();
        // The current light intensity
        _currentLightIntensity = room.GetCurrentLightIntensity();
        // If the room has been cleared
        _clear = room.GetClear();
        // The weight of the room
        _roomWeight = room.RoomWeight;
        // The type of room it is
        _myRoomType = (int) room.MyRoomType;
        // The difficulty of the room
        _roomDifficulty = room.RoomDifficulty;

        // Get the color from the spriteRenderer
        SpriteRenderer sprRendRef = room.GetComponent<SpriteRenderer>();
        if (sprRendRef == null)
        {
            Debug.LogError("No Sprite Renderer on Room " + room.name);
        }
        else {
            _sprRendColor = new float[4];
            _sprRendColor[0] = sprRendRef.color.r;
            _sprRendColor[1] = sprRendRef.color.g;
            _sprRendColor[2] = sprRendRef.color.b;
            _sprRendColor[3] = sprRendRef.color.a;
        }
        // Get the position from the transform
        _position = new float[3];
        _position[0] = room.transform.position.x;
        _position[1] = room.transform.position.y;
        _position[2] = room.transform.position.z;
        // Get the scale from the transform
        _scale = new float[3];
        _scale[0] = room.transform.localScale.x;
        _scale[1] = room.transform.localScale.y;
        _scale[2] = room.transform.localScale.z;
    }
}
