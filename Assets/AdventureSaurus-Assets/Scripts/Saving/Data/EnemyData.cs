using UnityEngine;

[System.Serializable]
public class EnemyData
{
    /// Key to what prefab this is
    private int _prefKey;
    /// Simple Primitives
    private int _difficulty;
    private int _strength, _magic, _speed, _vitality;
    private int _baseXpToGive;
    private int _curHP;
    private int _maxHP;
    private bool _isRainbow;
    /// Essential parts of non primitives
    private float[] _position;

    /// Getters
    public int GetPrefabKey() { return _prefKey; }
    public int GetDifficulty() { return _difficulty; }
    public int GetStrength() { return _strength; }
    public int GetMagic() { return _magic; }
    public int GetSpeed() { return _speed; }
    public int GetVitality() { return _vitality; }
    public int GetBaseXpToGive() { return _baseXpToGive; }
    public int GetCurrentHealth() { return _curHP; }
    public int GetMaxHealth() { return _maxHP; }
    public bool GetIsRainbow() { return _isRainbow; }
    public Vector3 GetPosition() { return new Vector3(_position[0], _position[1], _position[2]); }

    public EnemyData(GameObject enemy)
    {
        // Try to pull a PrefabKey script off the enemy (it should have one)
        PrefabKey prefabKeyScriptRef = enemy.GetComponent<PrefabKey>();
        if (prefabKeyScriptRef != null)
            _prefKey = prefabKeyScriptRef.GetPrefabKey();
        else
            Debug.LogError("No PrefabKey script was attached to " + enemy.name);

        // Try to pull an EnemyDifficulty script off the enemy (it should have one)
        EnemyDifficulty enemyDiffScriptRef = enemy.GetComponent<EnemyDifficulty>();
        if (enemyDiffScriptRef != null)
            _difficulty = enemyDiffScriptRef.GetDifficulty();
        else
            Debug.LogError("No EnemyDifficulty script was attached to " + enemy.name);

        // Try to pull an EnemyStats script off the enemy (it should have one)
        EnemyStats enemyStatsScriptRef = enemy.GetComponent<EnemyStats>();
        if (enemyStatsScriptRef != null)
        {
            _strength = enemyStatsScriptRef.GetStrength();
            _magic = enemyStatsScriptRef.GetMagic();
            _speed = enemyStatsScriptRef.GetSpeed();
            _vitality = enemyStatsScriptRef.GetVitality();
            _baseXpToGive = enemyStatsScriptRef.GetBaseXpToGive();
        }
        else
            Debug.LogError("No EnemyStats script was attached to " + enemy.name);

        // Try to pull an EnemyHealth script off the enemy (it should have one)
        EnemyHealth enemyHealthScriptRef = enemy.GetComponent<EnemyHealth>();
        if (enemyHealthScriptRef != null)
        {
            _curHP = enemyHealthScriptRef.CurHP;
            _maxHP = enemyHealthScriptRef.MaxHP;
        }
        else
            Debug.LogError("No EnemyHealth script was attached to " + enemy.name);

        // Try to pull a Rainbow script off the enemy (does not necessarily have one)
        Rainbow rainbowScriptRef = enemy.GetComponent<Rainbow>();
        if (rainbowScriptRef != null)
            _isRainbow = true;
        else
            _isRainbow = false;

        // Position off transform
        _position = new float[3];
        _position[0] = enemy.transform.position.x;
        _position[1] = enemy.transform.position.y;
        _position[2] = enemy.transform.position.z;
    }
}
