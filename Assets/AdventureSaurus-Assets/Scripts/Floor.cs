using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    //sets floor prefab
    public GameObject FloorPrefab;

    //2d array for floor in room 1
    public GameObject[,] ftiles1 = new GameObject[12, 12];

    //2d array for floor in hallway
    public GameObject[,] fhtiles1 = new GameObject[6, 2];

    //2d array for floor in room 2
    public GameObject[,] ftiles2 = new GameObject[12, 12];

    //Labels to reference a specific piece
    //Room 1
    public static string[] R1Label = new string[] { "R1F_" };
    //Hallway
    public static string[] H1Label = new string[] { "H1F_" };
    //Room 2
    public static string[] R2Label = new string[] { "R2F_" };
    //Column in a room
    public static string[] CLabel = new string[] { "C1_", "C2_", "C3_", "C4_", "C5_", "C6_", "C7_", "C8_", "C9_", "C10_", "C11_", "C12_" };


    public void CreateFloor()
    {
        //Floor For Room 1
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                ftiles1[i, j] = Instantiate(FloorPrefab, new Vector3(i, j, 0), Quaternion.identity);
                //applies labels and makes each piece a child olf game manager
                ftiles1[i, j].transform.SetParent(gameObject.transform);
                ftiles1[i, j].name = R1Label[0] + CLabel[i] + (j + 1);
            }
        }

        //Hallway
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                fhtiles1[i, j] = Instantiate(FloorPrefab, new Vector3(i+12, j+5, 0), Quaternion.identity);
                //applies labels and makes each piece a child olf game manager
                fhtiles1[i, j].transform.SetParent(gameObject.transform);
                fhtiles1[i, j].name = H1Label[0] + CLabel[i] + (j + 1);
            }
        }
        //Floor for Room 2
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                ftiles1[i, j] = Instantiate(FloorPrefab, new Vector3(i+18, j, 0), Quaternion.identity);
                //applies labels and makes each piece a child olf game manager
                ftiles1[i, j].transform.SetParent(gameObject.transform);
                ftiles1[i, j].name = R2Label[0] + CLabel[i]+ (j + 1);
            }
        }

    }

}
