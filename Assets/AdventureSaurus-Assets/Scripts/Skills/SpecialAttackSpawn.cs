using UnityEngine;

public class SpecialAttackSpawn : MonoBehaviour
{
    // Reference to the MoveAttack script on the character that spawned this object
    private MoveAttack _spawner;
    public void SetSpawner(MoveAttack newSpawner) { _spawner = newSpawner; }

    /// <summary>
    /// Calls the end attack function for the character that spawned this SmiteSpawn
    /// </summary>
    public void EndAttackAnimationDestroyParent()
    {
        if (_spawner != null)
        {
            Debug.Log("EndAttackAnimationDestroyParent");
            _spawner.EndAttack();
            // Destroy this object's parent
            Destroy(this.transform.parent.gameObject);
        }
    }

    /// <summary>
    /// Calls the end attack function for the character that spawned this SmiteSpawn
    /// </summary>
    public void EndAttackAnimationDestroyThis()
    {
        if (_spawner != null)
        {
            _spawner.EndAttack();
            // Destroy this object's parent
            Destroy(this.gameObject);
        }
    }
}
