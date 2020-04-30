using UnityEngine;

public class SmiteSpawn : MonoBehaviour
{
    // Reference to the MoveAttack script on the character that spawned this object
    private MoveAttack _spawner;
    public void SetSpawner(MoveAttack newSpawner) { _spawner = newSpawner; }

    /// <summary>
    /// Calls the end attack function for the character that spawned this SmiteSpawn
    /// </summary>
    public void SmiteThee()
    {
        _spawner.EndAttack();
        // Destroy this object
        Destroy(this.transform.parent.gameObject);
    }
}
