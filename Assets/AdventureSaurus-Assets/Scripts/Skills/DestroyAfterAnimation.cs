using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    public void DestroyThisObject()
    {
        Destroy(this.gameObject);
    }
}
