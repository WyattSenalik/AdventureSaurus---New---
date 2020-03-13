using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeProceduralGeneration : MonoBehaviour
{
    private ProceduralGenerationController proGenContRef;

    // Set references
    private void Awake()
    {
        proGenContRef = this.GetComponent<ProceduralGenerationController>();
        if (proGenContRef == null)
            Debug.Log("Could not find ProceduralGenerationController attached to " + this.name);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
