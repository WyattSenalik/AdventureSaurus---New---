using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    [SerializeField] private Transform _roomsParent = null;
    [SerializeField] private GameObject _roomPrefab = null;

    public void TestLoadRooms()
    {
        LoadRoomsController.LoadRooms(_roomsParent, _roomPrefab);
    }

    public void TestSaveRooms()
    {
        SaveSystem.SaveGame();
    }
}
