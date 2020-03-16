using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstigateGeneration : MonoBehaviour
{
    // Called before the first frame
    // We use start to start the generation because we have to wait for references to be set in awake
    private void Start()
    {
        // Get the persistant controller
        GameObject persistContRef = GameObject.FindWithTag("PersistantController");
        if (persistContRef == null)
            Debug.Log("Could not find a gameobject with the tag PersistantController");
        // Get the persistant controller script
        PersistantController persistContScript = persistContRef.GetComponent<PersistantController>();
        if (persistContScript == null)
            Debug.Log(persistContScript.name + " did not have a PersistantController script attached");
        // Start the generation of this floor
        persistContScript.StartGeneration();
    }
}
