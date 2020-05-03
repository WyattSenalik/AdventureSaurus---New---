
[System.Serializable]
public class PersistantFloorData
{
    /// Simple Primitives
    private int _nextFloor;
    private int _nextFloorDiff;

    /// Getters
    public int GetNextFloor() { return _nextFloor; }
    public int GetNextFloorDifficulty() { return _nextFloorDiff; }

    public PersistantFloorData(PersistantController persistCont)
    {
        _nextFloor = persistCont.GetNextFloorNum();
        _nextFloorDiff = persistCont.GetNextFloorDifficulty();
    }
}
