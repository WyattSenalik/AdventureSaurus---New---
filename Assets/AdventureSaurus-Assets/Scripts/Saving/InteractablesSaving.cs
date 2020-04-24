using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablesSaving : MonoBehaviour
{
    // All the Interactable Prefabs
    [SerializeField] private GameObject[] _allInteractablePrefabs = null;


    // Called before Start
    private void Awake()
    {
        // Build the hashtbale for the Interactables' keys
        if (_allInteractablePrefabs != null)
            SaveInteractablesController.BuildHashtables(_allInteractablePrefabs);
    }
}
