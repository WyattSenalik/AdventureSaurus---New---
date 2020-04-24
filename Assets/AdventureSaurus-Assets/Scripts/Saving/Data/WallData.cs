using UnityEngine;

[System.Serializable]
public class WallData
{
    /// Essential parts of non primitives
    private float[] position;
    public Vector3 GetPosition() { return new Vector3(position[0], position[1], position[2]); }

    public WallData(Transform wall)
    {
        position = new float[3];
        position[0] = wall.position.x;
        position[1] = wall.position.y;
        position[2] = wall.position.z;
    }
}
