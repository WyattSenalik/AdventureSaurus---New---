using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnStairsOnDeath : MonoBehaviour
{
    // Reference to the tilemap
    [SerializeField] private Tilemap _grid = null;
    // Tile of stairs that will be spawned
    [SerializeField] private TileBase _stairstile = null;
    // Reference to the transform of the stairs
    [SerializeField] private Transform _stairsTrans = null;

    // Called when this component is destroyed
    private void OnDestroy()
    {
        SpawnStairsAtCurrentNode();
    }

    /// <summary>
    /// Moves the stairs transform to where the character is and then changes the tile map to reflect this
    /// </summary>
    private void SpawnStairsAtCurrentNode()
    {
        if (_grid != null && _stairsTrans != null)
        {
            // Get position of this character as Vector3Int
            Vector3Int charPos = new Vector3Int(Mathf.RoundToInt(this.transform.position.x),
                                                Mathf.RoundToInt(this.transform.position.y), 0);
            // Place the stairs tile there
            _grid.SetTile(charPos, _stairstile);
            // Moe the stairs transform there
            _stairsTrans.position = charPos;
        }
    }
}
