using UnityEngine;

[System.Serializable]
public class StairsData
{
    /// Essential parts of non primitives
    private float[] position;
    public Vector3 GetPosition() { return new Vector3(position[0], position[1], position[2]); }

    public StairsData(Transform stairs)
    {
        position = new float[3];
        position[0] = stairs.position.x;
        position[1] = stairs.position.y;
        position[2] = stairs.position.z;
    }
}
