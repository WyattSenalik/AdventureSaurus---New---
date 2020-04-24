using UnityEngine;

public class PrefabKey : MonoBehaviour
{
    // Key that relates to a prefab of whatever kind of thing this is attached to
    [SerializeField] private int _prefabKey = -1;
    // Getter
    public int GetPrefabKey() { return _prefabKey; }
}
