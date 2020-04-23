using UnityEngine;

[System.Serializable]
public class ChildAmountData
{
    // Amount of children there are
    private int _childAmount;
    public int GetChildAmount() { return _childAmount; }

    public ChildAmountData(Transform parent)
    {
        _childAmount = parent.childCount;
    }
}
