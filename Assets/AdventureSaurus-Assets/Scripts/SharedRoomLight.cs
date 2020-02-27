using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class SharedRoomLight : MonoBehaviour
{
    [SerializeField] private Light2D sharedLight = null;    // The light that is shared
    public Light2D SharedLight
    {
        get { return sharedLight; }
    }
    [SerializeField] private Room roomToShareWith = null;   // The room the light is shared with
    public Room RoomToShareWith
    {
        get { return roomToShareWith; }
    }
    [SerializeField] private Room roomToBroadcastFrom = null;   // The room the light is broadcast from
    public Room RoomToBroadcastFrom
    {
        get { return roomToBroadcastFrom; }
    }
}
