using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    Floor floor;
    Wall wall;
    void Start() {
        //lays out floor tiles
        floor = gameObject.GetComponent<Floor> ();
        floor.CreateFloor ();

        //lays out wall tiles
        wall = gameObject.GetComponent<Wall>();
        wall.CreateWall();
    }
}

