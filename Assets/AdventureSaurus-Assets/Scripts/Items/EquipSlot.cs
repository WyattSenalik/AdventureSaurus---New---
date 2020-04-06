using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSlot : ItemSlot
{
    public ItemType ItemType;

    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.name = ItemType.ToString() + " Slot";
    }


}
