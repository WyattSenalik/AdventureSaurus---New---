using UnityEngine;

public static class LoadEnemiesController
{
    /// <summary>
    /// Creates enemies based on the stored enemy data
    /// </summary>
    /// <param name="enemyParent">Transform that will serve as the parent for the loaded enemies</param>
    public static void LoadEnemies(Transform enemyParent)
    {
        // Get the amount of enemies
        ChildAmountData enemyAmData = SaveSystem.LoadEnemyAmount();
        int amountEnemies = enemyAmData.GetChildAmount();

        // Load each enemy
        for (int i = 0; i < amountEnemies; ++i)
        {
            // Get the data for the enemy
            EnemyData enemyData = SaveSystem.LoadEnemy(i);
            // Get the key of which prefab to spawn
            int key = enemyData.GetPrefabKey();
            // Use the key to get the prefab to spawn
            GameObject enemyPrefab = SaveEnemiesController.GetEnemyPrefab(key);
            // Spawn the prefab as a child of the enemy Parent
            GameObject enemyObj = Object.Instantiate(enemyPrefab, enemyParent);

            // Set its transform components
            enemyObj.transform.position = enemyData.GetPosition();

            // Get its EnemyDifficulty script
            EnemyDifficulty enemyDiffRef = enemyObj.GetComponent<EnemyDifficulty>();
            if (enemyDiffRef != null)
                enemyDiffRef.SetDifficulty(enemyData.GetDifficulty());
            else
                Debug.LogError("There is no EnemyDifficulty script attached to " + enemyObj.name);

            // Get its EnemyStats script
            EnemyStats enemyStatsRef = enemyObj.GetComponent<EnemyStats>();
            if (enemyStatsRef != null)
            {
                enemyStatsRef.SetStrength(enemyData.GetStrength());
                enemyStatsRef.SetMagic(enemyData.GetMagic());
                enemyStatsRef.SetSpeed(enemyData.GetSpeed());
                enemyStatsRef.SetVitality(enemyData.GetVitality());
                enemyStatsRef.SetBaseXpToGive(enemyData.GetBaseXpToGive());
            }
            else
                Debug.LogError("There is no EnemyStats script attached to " + enemyObj.name);

            // Get its EnemyHealth script
            EnemyHealth enemyHealthRef = enemyObj.GetComponent<EnemyHealth>();
            if (enemyHealthRef != null)
            {
                enemyHealthRef.MaxHP = enemyData.GetMaxHealth();
                enemyHealthRef.CurHP = enemyData.GetCurrentHealth();
            }
            else
                Debug.LogError("There is no EnemyHealth script attached to " + enemyObj.name);

            // If it is rainbow attach a Rainbow script to it
            if (enemyData.GetIsRainbow())
                enemyObj.AddComponent<Rainbow>();
        }
    }
}
