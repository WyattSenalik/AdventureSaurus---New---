
[System.Serializable]
public class PersistantFloorData
{
    /// Simple Primitives
    private int _nextFloor;
    private int _nextFloorDiff;
    private int _potCharges;

    /// Getters
    public int GetNextFloor() { return _nextFloor; }
    public int GetNextFloorDifficulty() { return _nextFloorDiff; }
    public int GetPotCharges() { return _potCharges; }

    public PersistantFloorData(PersistantController persistCont)
    {
        _nextFloor = persistCont.GetNextFloorNum();
        _nextFloorDiff = persistCont.GetNextFloorDifficulty();
        _potCharges = PersistantController.GetPotCharges();
    }
}
