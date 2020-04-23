﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    // Room Parent and Prefab
    [SerializeField] private Transform _roomsParent = null;
    [SerializeField] private GameObject _roomPrefab = null;
    // Bleed Light Parent and Prefab
    [SerializeField] private Transform _bleedLightsParent = null;
    [SerializeField] private GameObject _bleedLightPrefab = null;

    // Wall Parent and Prefab
    [SerializeField] private Transform _wallsParent = null;
    [SerializeField] private GameObject _wallPrefab = null;

    public void TestLoad()
    {
        // Rooms
        LoadRoomsController.LoadRooms(_roomsParent, _roomPrefab);
        LoadRoomsController.LoadBleedLights(_bleedLightsParent, _bleedLightPrefab);
        LoadRoomsController.LoadReferences(_roomsParent, _bleedLightsParent);
        // Walls
        LoadWallsController.LoadWalls(_wallsParent, _wallPrefab);
    }

    public void TestSave()
    {
        SaveSystem.SaveGame();
    }
}
