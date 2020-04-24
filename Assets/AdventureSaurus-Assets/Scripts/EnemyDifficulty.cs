using UnityEngine;

public class EnemyDifficulty : MonoBehaviour
{
    // The enemies difficulty
    [SerializeField] private int _difficulty = 1;
    // Getter
    public int GetDifficulty() { return _difficulty; }
}
