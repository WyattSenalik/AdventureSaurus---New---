using UnityEngine;

[System.Serializable]
public class BleedLightAmountData
{
    // Amount of bleed lights there are
    private int _bleedLightAmount;
    public int GetBleedLightAmount() { return _bleedLightAmount; }

    public BleedLightAmountData(Transform bleedLightParent)
    {
        _bleedLightAmount = bleedLightParent.childCount;
    }
}
