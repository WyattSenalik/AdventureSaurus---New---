using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GameObject WallPrefab;
    //Room 1
        //2d array for Walls
        public GameObject[,] R1WallTilesE = new GameObject[1, 13];
        public GameObject[,] R1WallTilesW = new GameObject[1, 13];
        public GameObject[,] R1WallTilesN = new GameObject[13, 1];
        public GameObject[,] R1WallTilesS = new GameObject[13, 1];
    //

    //Hallways
    //2d array for walls
    public GameObject[,] H1WallTilesN = new GameObject[4, 1];
    public GameObject[,] H1WallTilesS = new GameObject[4, 1];
    //

    //Room 2
    //2d array for Walls
    public GameObject[,] R2WallTilesE = new GameObject[1, 13];
        public GameObject[,] R2WallTilesW = new GameObject[1, 13];
        public GameObject[,] R2WallTilesN = new GameObject[13, 1];
        public GameObject[,] R2WallTilesS = new GameObject[13, 1];
    //
    //Labels to reference a specific piece
    //Room 1
    public static string[] R1Label = new string[] { "R1W_" };
    //Direction
    public static string[] DLabel = new string[] { "N_","E_","S_","W_" };
    //Hallway
    public static string[] H1Label = new string[] { "H1W_" };
    //Room 2
    public static string[] R2Label = new string[] { "R2W_" };

    public void CreateWall()
    {
    //Room 1 Walls
        //WestWall
            for (int j = 0; j < 13; j++)
            {
                R1WallTilesW[0, j] = Instantiate(WallPrefab, new Vector3(-1, j, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R1WallTilesW[0, j].transform.SetParent(gameObject.transform);
                R1WallTilesW[0, j].name = R1Label[0] + DLabel[3] + (j + 1);
            }
        //NorthWall
            for (int i = 0; i < 13; i++)
            {
                R1WallTilesN[i, 0] = Instantiate(WallPrefab, new Vector3(i, 12, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R1WallTilesN[i, 0].transform.SetParent(gameObject.transform);
                R1WallTilesN[i, 0].name = R1Label[0] + DLabel[0] + (i + 1);
            }
        //SouthWall
            for (int i = 0; i < 13; i++)
            {
                R1WallTilesS[i, 0] = Instantiate(WallPrefab, new Vector3(i-1, -1, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R1WallTilesS[i, 0].transform.SetParent(gameObject.transform);
                R1WallTilesS[i, 0].name = R1Label[0] + DLabel[2] + (i + 1);
            }
        //EastWall
            for (int j = 0; j < 13; j++)
            {
            if (j < 6)
            {
                R1WallTilesE[0, j] = Instantiate(WallPrefab, new Vector3(12, j-1, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R1WallTilesE[0, j].transform.SetParent(gameObject.transform);
                R1WallTilesE[0, j].name = R1Label[0] + DLabel[1] + (j + 1);
            }
            if (j>7)
            {
                R1WallTilesE[0, j] = Instantiate(WallPrefab, new Vector3(12, j-1, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R1WallTilesE[0, j].transform.SetParent(gameObject.transform);
                R1WallTilesE[0, j].name = R1Label[0] + DLabel[1] + (j + 1);
            }
            }
        //Hallway Walls

         //north wall
         for (int i=0; i<4;i++)
        {
            H1WallTilesN[i, 0] = Instantiate(WallPrefab, new Vector3(i+13, 7, 0), Quaternion.identity);
            //applies labels and makes each piece a child of game manager
            H1WallTilesN[i, 0].transform.SetParent(gameObject.transform);
            H1WallTilesN[i, 0].name = H1Label[0] + DLabel[0] + (i + 1);
        }
        //south wall
        for (int i = 0; i < 4; i++)
        {
            H1WallTilesS[i, 0] = Instantiate(WallPrefab, new Vector3(i+13, 4, 0), Quaternion.identity);
            //applies labels and makes each piece a child of game manager
            H1WallTilesS[i, 0].transform.SetParent(gameObject.transform);
            H1WallTilesS[i, 0].name = H1Label[0] + DLabel[2] + (i + 1);
        }
        //Room 2 Walls
        //
            //WestWall
            
        for (int j = 0; j < 13; j++)
        {
            if (j < 5)
            {
                R2WallTilesW[0, j] = Instantiate(WallPrefab, new Vector3(17, j, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R2WallTilesW[0, j].transform.SetParent(gameObject.transform);
                R2WallTilesW[0, j].name = R2Label[0] + DLabel[3] + (j + 1);
            }
            if (j > 6)
            {
                R2WallTilesW[0, j] = Instantiate(WallPrefab, new Vector3(17, j, 0), Quaternion.identity);
                //applies labels and makes each piece a child of game manager
                R2WallTilesW[0, j].transform.SetParent(gameObject.transform);
                R2WallTilesW[0, j].name = R2Label[0] + DLabel[3] + (j + 1);
            }
        }
             //NorthWall
      
        for (int i = 0; i < 13; i++)
        {
            R2WallTilesN[i, 0] = Instantiate(WallPrefab, new Vector3(i+18, 12, 0), Quaternion.identity);
            //applies labels and makes each piece a child of game manager
            R2WallTilesN[i, 0].transform.SetParent(gameObject.transform);
            R2WallTilesN[i, 0].name = R2Label[0] + DLabel[0] + (i + 1);
        }

            //SouthWall
         
        for (int i = 0; i < 13; i++)
        {
            R2WallTilesS[i, 0] = Instantiate(WallPrefab, new Vector3(i+17, -1, 0), Quaternion.identity);
            //applies labels and makes each piece a child of game manager
            R2WallTilesS[i, 0].transform.SetParent(gameObject.transform);
            R2WallTilesS[i, 0].name = R2Label[0] + DLabel[2] + (i + 1);
        }

             //EastWall
        for (int j = 0; j < 13; j++)
        {
            R2WallTilesE[0, j] = Instantiate(WallPrefab, new Vector3(30, j-1, 0), Quaternion.identity);
            //applies labels and makes each piece a child of game manager
            R2WallTilesE[0, j].transform.SetParent(gameObject.transform);
            R2WallTilesE[0, j].name = R2Label[0] + DLabel[1] + (j + 1);
        }
        //
        }
    }


    

