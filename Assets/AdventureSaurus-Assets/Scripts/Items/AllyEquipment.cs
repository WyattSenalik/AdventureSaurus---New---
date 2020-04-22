using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyEquipment : MonoBehaviour
{
    [SerializeField] AllyStats AllyStatsREF;

    private Item weapon;
    private Item armor;
    private Item boots;
    private Item accessory1;
    private Item accessory2;

    //public Item Equip(Item equipping)
    //{

    //    switch (equiping.ItemType)
    //    {

    //        //case ItemType.Weapon:
    //        //    if (weapon != null)





    //    }

    //}

    public Item UnEquip(Item unequipping)
    {
        Item temp;
        if(unequipping == weapon)
        {
            removeItemStats(weapon);
            temp = weapon;
            weapon = null;
            return temp;
        } else if (unequipping == armor)
        {
            removeItemStats(armor);
            temp = armor;
            armor = null;
            return temp;
        } else if (unequipping == boots)
        {
            removeItemStats(boots);
            temp = boots;
            boots = null;
            return temp;
        } else if (unequipping == accessory1)
        {
            removeItemStats(accessory1);
            temp = accessory1;
            accessory1 = null;
            return temp;
        } else if (unequipping == accessory2)
        {
            removeItemStats(accessory2);
            temp = accessory2;
            accessory2 = null;
            return temp;
        }

        return null;




    }

    public void removeItemStats(Item item)
    {
        AllyStatsREF.ChangeVitality(-item.Vit);
        AllyStatsREF.ChangeMagic(-item.Mgk);
        AllyStatsREF.ChangeStrength(-item.Str);
        AllyStatsREF.ChangeSpeed(-item.Spd);
    }
}
