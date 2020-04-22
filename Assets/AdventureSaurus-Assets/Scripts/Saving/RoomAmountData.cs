using UnityEngine;

[System.Serializable]
public class RoomAmountData
{
    // Amount of rooms there are
    private int roomAmount;
    public int GetRoomAmount() { return roomAmount; }

    public RoomAmountData(Transform roomParent)
    {
        roomAmount = roomParent.childCount; 
    }
}
