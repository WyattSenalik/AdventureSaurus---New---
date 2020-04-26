using UnityEngine;

[System.Serializable]
public class AllyData
{
    /// Key to what prefab this is
    private int _prefKey;
    /// Simple Primitives
    private int _experience;
    private int _oneLvlExperience;
    private int _level;
    private int _nextLvlThreshold;
    private int _oneLvlThreshold;
    private int _amountStatIncreases;
    private int _strength, _magic, _speed, _vitality;
    private int _strengthBubblesFilled, _magicBubblesFilled, _speedBubblesFilled, _vitalityBubblesFilled;
    private int _maxHP;
    private int _curHP;
    /// Essential parts of non-primitives
    private float[] _position;

    /// Getters
    public int GetPrefabKey() { return _prefKey; }
    public int GetExperience() { return _experience; }
    public int GetOneLevelExperience() { return _oneLvlExperience; }
    public int GetLevel() { return _level; }
    public int GetNextLevelThreshold() { return _nextLvlThreshold; }
    public int GetOneLevelNextLevelThreshold() { return _oneLvlThreshold; }
    public int GetAmountStatIncreases() { return _amountStatIncreases; }
    public int GetStrength() { return _strength; }
    public int GetMagic() { return _magic; }
    public int GetSpeed() { return _speed; }
    public int GetVitality() { return _vitality; }
    public int GetStrengthBubblesFilled() { return _strengthBubblesFilled; }
    public int GetMagicBubblesFilled() { return _magicBubblesFilled; }
    public int GetSpeedBubblesFilled() { return _speedBubblesFilled; }
    public int GetVitalityBubblesFilled() { return _vitalityBubblesFilled; }
    public int GetMaxHP() { return _maxHP; }
    public int GetCurHP() { return _curHP; }
    public Vector3 GetPosition() { return new Vector3(_position[0], _position[1], _position[2]); }

    public AllyData(GameObject ally)
    {
        // Try to pull a PrefabKey script off the ally (it should have one)
        PrefabKey prefabKeyScriptRef = ally.GetComponent<PrefabKey>();
        if (prefabKeyScriptRef != null)
            _prefKey = prefabKeyScriptRef.GetPrefabKey();
        else
            Debug.LogError("No PrefabKey script was attached to " + ally.name);

        // Try to pull an AllyStats script off the ally (it should have one)
        AllyStats allyStatsScriptRef = ally.GetComponent<AllyStats>();
        if (allyStatsScriptRef != null)
        {
            _experience = allyStatsScriptRef.GetExperience();
            _oneLvlExperience = allyStatsScriptRef.GetOneLevelExperience();
            _level = allyStatsScriptRef.GetLevel();
            _nextLvlThreshold = allyStatsScriptRef.GetNextLevelThreshold();
            _oneLvlThreshold = allyStatsScriptRef.GetOneLevelNextLevelThreshold();
            _amountStatIncreases = allyStatsScriptRef.AmountStatIncreases;
            _strength = allyStatsScriptRef.GetStrength();
            _magic = allyStatsScriptRef.GetMagic();
            _speed = allyStatsScriptRef.GetSpeed();
            _vitality = allyStatsScriptRef.GetVitality();
            _strengthBubblesFilled = allyStatsScriptRef.StrBubblesFilled;
            _magicBubblesFilled = allyStatsScriptRef.MagicBubblesFilled;
            _speedBubblesFilled = allyStatsScriptRef.SpeedBubblesFilled;
            _vitalityBubblesFilled = allyStatsScriptRef.VitalityBubblesFilled;
        }
        else
            Debug.LogError("There was no AllyStats script attached to " + ally.name);

        // Try to pull an AllyHealth script off the ally (it should have one)
        AllyHealth allyHealthScriptRef = ally.GetComponent<AllyHealth>();
        if (allyHealthScriptRef != null)
        {
            _maxHP = allyHealthScriptRef.MaxHP;
            _curHP = allyHealthScriptRef.CurHP;
        }
        else
            Debug.LogError("There was no AllyHealth script attached to " + ally.name);

        // Transform components
        _position = new float[3];
        _position[0] = ally.transform.position.x;
        _position[1] = ally.transform.position.y;
        _position[2] = ally.transform.position.z;
    }
}
