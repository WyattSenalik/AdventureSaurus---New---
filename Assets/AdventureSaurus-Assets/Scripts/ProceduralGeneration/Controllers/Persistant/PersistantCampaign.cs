using UnityEngine;

public class PersistantCampaign : PersistantController
{
    // At what floor do we swap to the next tile set?
    // Should have length tileSets.Length
    [SerializeField] private int[] _floorsUnilChange = null;

    /// <summary>
    /// Checks if we should swap the tileset to a different tileset.
    /// If we are on a floor to change, change it
    /// </summary>
    protected override void CheckSwapTileset()
    {
        // Check if we have reached any floor to change to
        for (int i = 0; i < _floorsUnilChange.Length; ++i)
        {
            // If we have reached that floor to change
            if (_floorsUnilChange[i] == GetNextFloorNum())
            {
                _activeTileSet = _tileSets[i];
            }
        }
    }
}
