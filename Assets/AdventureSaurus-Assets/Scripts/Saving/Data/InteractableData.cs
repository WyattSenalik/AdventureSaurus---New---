using UnityEngine;

[System.Serializable]
public class InteractableData
{
    /// Key to what prefab this is
    private int _prefKey;
    /// Simple Primitives
    private bool _canInteractWith;
    /// Essential parts of non primitives
    private float[] _position;

    /// Getters
    public int GetPrefabKey() { return _prefKey; }
    public bool GetCanInteractWith() { return _canInteractWith; }
    public Vector3 GetPosition() { return new Vector3(_position[0], _position[1], _position[2]); }

    public InteractableData(Interactable interact)
    {
        // Try to pull prefab key off
        PrefabKey prefabKeyScriptRef = interact.GetComponent<PrefabKey>();
        if (prefabKeyScriptRef != null)
            _prefKey = prefabKeyScriptRef.GetPrefabKey();
        else
            Debug.LogError("No PrefabKey attached to " + interact.name);

        _canInteractWith = interact.GetCanInteractWith();

        _position = new float[3];
        _position[0] = interact.transform.position.x;
        _position[1] = interact.transform.position.y;
        _position[2] = interact.transform.position.z;
    }
}
