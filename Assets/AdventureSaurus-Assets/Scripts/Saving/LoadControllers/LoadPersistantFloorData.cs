
public static class LoadPersistantFloorData
{
    /// <summary>
    /// Loads the data for the persistant controller and applies the save data to runtime
    /// </summary>
    /// <param name="persistCont">PersistantController that will recieve the data </param>
    public static void LoadPersistantController(PersistantController persistCont)
    {
        // Get the save data
        PersistantFloorData persistFloorData = SaveSystem.LoadPersistantFloorData();
        // Set the values of the PersistantController
        persistCont.SetNextFloorDifficulty(persistFloorData.GetNextFloorDifficulty());
        persistCont.SetNextFloorNum(persistFloorData.GetNextFloor());
        PersistantController.SetPotCharges(persistFloorData.GetPotCharges());
    }
}
