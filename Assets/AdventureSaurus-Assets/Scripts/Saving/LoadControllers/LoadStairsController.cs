using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoadStairsController
{
    /// <summary>
    /// Creates a stairs object based on the stored data
    /// </summary>
    /// <param name="stairsPrefab"></param>
    /// <returns>Transform of the new stairs</returns>
    public static Transform LoadStairs(GameObject stairsPrefab)
    {
        // Get the data for the stairs
        StairsData stairsData = SaveSystem.LoadStairs();
        // Create the new stairs
        GameObject stairsObj = Object.Instantiate(stairsPrefab);

        // Set its transform components
        stairsObj.transform.position = stairsData.GetPosition();

        // Give the stairs script the transform as a reference
        GameObject gameController = GameObject.FindWithTag("GameController");
        if (gameController != null)
        {
            Stairs stairsRef = gameController.GetComponent<Stairs>();
            if (stairsRef != null)
                stairsRef.SetStairsTrans(stairsObj.transform);
            else
                Debug.LogError("No Stairs script attached to " + gameController.name);
        }
        else
            Debug.LogError("No GameObject with the tag 'GameController' was found");

        return stairsObj.transform;
    }
}
