using UnityEngine;

[System.Serializable]
public class RoomAmountData
{
    // Amount of rooms there are
    private int _roomAmount;
    public int GetRoomAmount() { return _roomAmount; }

    public RoomAmountData(Transform roomParent)
    {
        _roomAmount = roomParent.childCount; 
    }
}
