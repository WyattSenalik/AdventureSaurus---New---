using UnityEngine;

public static class LoadAlliesController
{
    /// <summary>
    /// Creates allies based on the stored ally data
    /// </summary>
    /// <param name="allyParent">Transform that will serve as the parent for the loaded allies</param>
    public static void LoadAllies(Transform allyParent)
    {
        // Get the amount of allies
        ChildAmountData allyAmData = SaveSystem.LoadAllyAmount();
        int allyAmount = allyAmData.GetChildAmount();

        // Load each ally
        for (int i = 0; i < allyAmount; ++i)
        {
            // Get the data for the ally
            AllyData allyData = SaveSystem.LoadAlly(i);
            // Get the key of which prefab to spawn
            int key = allyData.GetPrefabKey();
            // Use the key to get the prefab to spawn
            GameObject allyPrefab = SaveAlliesController.GetAllyPrefab(key);
            // Spawn the prefab as a child of the ally Parent
            GameObject allyObj = Object.Instantiate(allyPrefab, allyParent);

            // Set its transform components
            allyObj.transform.position = allyData.GetPosition();

            // Get its AllyStats script
            AllyStats allyStatsScriptRef = allyObj.GetComponent<AllyStats>();
            if (allyStatsScriptRef != null)
            {
                allyStatsScriptRef.SetExperience(allyData.GetExperience());
                allyStatsScriptRef.SetOneLevelExperience(allyData.GetOneLevelExperience());
                allyStatsScriptRef.SetLevel(allyData.GetLevel());
                allyStatsScriptRef.SetNextLevelThreshold(allyData.GetNextLevelThreshold());
                allyStatsScriptRef.SetOneLevelNextLevelThreshold(allyData.GetOneLevelNextLevelThreshold());
                allyStatsScriptRef.AmountStatIncreases = allyData.GetAmountStatIncreases();
                allyStatsScriptRef.SetStrength(allyData.GetStrength());
                allyStatsScriptRef.SetMagic(allyData.GetMagic());
                allyStatsScriptRef.SetSpeed(allyData.GetSpeed());
                allyStatsScriptRef.SetVitality(allyData.GetVitality());
                allyStatsScriptRef.StrBubblesFilled = allyData.GetStrengthBubblesFilled();
                allyStatsScriptRef.MagicBubblesFilled = allyData.GetMagicBubblesFilled();
                allyStatsScriptRef.SpeedBubblesFilled = allyData.GetSpeedBubblesFilled();
                allyStatsScriptRef.VitalityBubblesFilled = allyData.GetVitalityBubblesFilled();
            }
            else
                Debug.LogError("There was no AllyStats script attached to " + allyObj.name);

            // Get its AllyHealth script
            AllyHealth allyHealthScriptRef = allyObj.GetComponent<AllyHealth>();
            if (allyHealthScriptRef != null)
            {
                allyHealthScriptRef.MaxHP = allyData.GetMaxHP();
                allyHealthScriptRef.CurHP = allyData.GetCurHP();
            }
            else
                Debug.LogError("There was no AllyHealth script attached to " + allyObj.name);
        }
    }
}
