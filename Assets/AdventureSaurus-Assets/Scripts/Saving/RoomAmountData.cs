using UnityEngine;

[System.Serializable]
public class RoomAmountData
{
    // Amount of rooms there are
    int roomAmount;

    public RoomAmountData(Transform roomParent)
    {
        roomAmount = roomParent.childCount; 
    }
}
