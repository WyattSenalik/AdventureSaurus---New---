using System.Collections.Generic;
using UnityEngine;

public class PlaceAllies : MonoBehaviour
{
    /// <summary>
    /// Puts allies in the start room
    /// </summary>
    /// <param name="roomParent">Parent of the rooms</param>
    /// <param name="allyParent">Parent of the allies</param>
    public void PutAlliesInStartRoom(Transform roomParent, Transform allyParent)
    {
        // Get the first room
        Transform firstRoom = roomParent.GetChild(0);
        // Get the bounds of the room
        Vector2Int botLeftFloorSpace = new Vector2Int(Mathf.RoundToInt(firstRoom.position.x - (firstRoom.localScale.x - 1) / 2f + 1),
            Mathf.RoundToInt(firstRoom.position.y - (firstRoom.localScale.y - 1) / 2f + 1));
        Vector2Int topRightFloorSpace = new Vector2Int(Mathf.RoundToInt(firstRoom.position.x + (firstRoom.localScale.x - 1) / 2f - 1),
            Mathf.RoundToInt(firstRoom.position.y + (firstRoom.localScale.y - 1) / 2f - 1));
        // Make a list of the already occupied tiles, so we don't spawn a player on top of another player
        List<Vector2Int> occupiedTiles = new List<Vector2Int>();
        // Choose a random location to place the ally at
        int randX = Random.Range(botLeftFloorSpace.x + 1, topRightFloorSpace.x);
        int randY = Random.Range(botLeftFloorSpace.y + 1, topRightFloorSpace.y);
        Vector2Int placePos = new Vector2Int(randX, randY);
        Vector2Int curPos = placePos;
        foreach (Transform allyTrans in allyParent)
        {
            do
            {
                // Choose a random location from the placePos to place the ally
                int randXDis = Random.Range(-1, 1 + 1);
                int randYDis = Random.Range(-1, 1 + 1);
                curPos = placePos + new Vector2Int(randXDis, randYDis);
                // Check to make sure there is not already a player there
            } while (occupiedTiles.Contains(curPos));
            // Add that tile to the occupied tiles and move the player there
            occupiedTiles.Add(curPos);
            allyTrans.position = new Vector3(curPos.x, curPos.y, 0);
        }
    }

    /// <summary>
    /// Puts allies at positions closest to x=0 where y=0, z=0
    /// </summary>
    /// <param name="allyParent"></param>
    public void PutAlliesAroundZero(Transform allyParent)
    {
        int placeX = 0;
        // Place initial ally at 0
        if (allyParent.childCount > 0)
            allyParent.GetChild(0).transform.position = new Vector3(placeX, 0);
        // Place enemies until we have placed them all
        int alliesPlaced = 1;
        while (alliesPlaced < allyParent.childCount)
        {
            // Increment x by 1 and place an ally
            placeX += 1;
            allyParent.GetChild(alliesPlaced).transform.position = new Vector3(placeX, 0);
            ++alliesPlaced;
            // Make sure that was not the last ally
            if (alliesPlaced < allyParent.childCount)
            {
                // Flip the x sign and place the next ally
                allyParent.GetChild(alliesPlaced).transform.position = new Vector3(-placeX, 0);
                ++alliesPlaced;
            }
        }

    }
}
