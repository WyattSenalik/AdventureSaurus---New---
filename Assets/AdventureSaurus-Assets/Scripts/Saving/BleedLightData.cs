using UnityEngine;

[System.Serializable]
public class BleedLightData
{
    /// Sibling Indices of references to other objects
    /// All of these are children of some parent
    // RoomParent children
    private int _broadcastRoomSiblingIndex;
    private int _receiveRoomSiblingIndex;
    /// Essential parts of non primitives
    private float[] _sprRendColor;
    private float[] _position;
    private float[] _rotation;

    /// Getters
    public int GetBroadcastRoomSiblingIndex() { return _broadcastRoomSiblingIndex; }
    public int GetReceiveRoomSiblingIndex() { return _receiveRoomSiblingIndex; }

    public Color GetSpriteRendererColor() {
        return new Color(_sprRendColor[0], _sprRendColor[1], _sprRendColor[2], _sprRendColor[3]);
    }
    public Vector3 GetPosition() {
        return new Vector3(_position[0], _position[1], _position[2]);
    }
    public Quaternion GetRotation() {
        return new Quaternion(_rotation[0], _rotation[1], _rotation[2], _rotation[3]);
    }

    public BleedLightData (BleedLight bleedLight)
    {
        // Broadcast Room
        _broadcastRoomSiblingIndex = bleedLight.GetBroadcastRoom().transform.GetSiblingIndex();
        // Receive Room
        _receiveRoomSiblingIndex = bleedLight.GetReceiveRoom().transform.GetSiblingIndex();

        // Get the color off the SpriteRenderer
        SpriteRenderer sprRendRef = bleedLight.GetComponent<SpriteRenderer>();
        if (sprRendRef == null)
        {
            Debug.LogError("No Sprite Renderer on Room " + bleedLight.name);
        }
        else
        {
            _sprRendColor = new float[4];
            _sprRendColor[0] = sprRendRef.color.r;
            _sprRendColor[1] = sprRendRef.color.g;
            _sprRendColor[2] = sprRendRef.color.b;
            _sprRendColor[3] = sprRendRef.color.a;
        }
        // Get the position
        _position = new float[3];
        _position[0] = bleedLight.transform.position.x;
        _position[1] = bleedLight.transform.position.y;
        _position[2] = bleedLight.transform.position.z;
        // Get the rotation
        _rotation = new float[4];
        _rotation[0] = bleedLight.transform.rotation.x;
        _rotation[1] = bleedLight.transform.rotation.y;
        _rotation[2] = bleedLight.transform.rotation.z;
        _rotation[3] = bleedLight.transform.rotation.w;
    }
}
