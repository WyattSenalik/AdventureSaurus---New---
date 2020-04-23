using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    [SerializeField] private Transform _roomsParent = null;
    [SerializeField] private GameObject _roomPrefab = null;
    [SerializeField] private Transform _bleedLightsParent = null;
    [SerializeField] private GameObject _bleedLightPrefab = null;

    public void TestLoadRooms()
    {
        LoadRoomsController.LoadRooms(_roomsParent, _roomPrefab);
        LoadRoomsController.LoadBleedLights(_bleedLightsParent, _bleedLightPrefab);
        LoadRoomsController.LoadReferences(_roomsParent, _bleedLightsParent);
    }

    public void TestSaveRooms()
    {
        SaveSystem.SaveGame();
    }
}
