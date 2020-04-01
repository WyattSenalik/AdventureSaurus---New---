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

    public bool addItem(Item equip)
    {
        if (equip.ItemType == this.ItemType)
        {
            Item = equip;
            return true;
        }

        return false;
    }

    //public bool removeItem(Item unEquip)
    //{

    //}

}
