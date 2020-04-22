using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType 
{
    Weapon,
    Armor,
    Boots,
    Accessory
}


[CreateAssetMenu]
public class Item : ScriptableObject
{
    private string ItemName;
    public Sprite Icon;


    //Stat values of the Item
    public int Vit;
    public int Str;
    public int Mgk;
    public int Spd;
    [Space]
    public ItemType ItemType;

    /// <summary>
    /// used to modify the stats of player chacter c
    /// </summary>
    /// <param name="c"></param>
    public void Equip(AllyStats c)
    {
        if (Vit != 0)
        {
            c.ChangeVitality(Vit);
        }

        if (Str != 0)
        {
            c.ChangeStrength(Str);
        }

        if (Mgk != 0)
        {
            c.ChangeMagic(Mgk);
        }

        if (Spd != 0)
        {
            c.ChangeSpeed(Spd);
        }
    }


    /// <summary>
    /// used to remove the modifications from equipment
    /// </summary>
    /// <param name = "c" ></ param >
    public void Unequip(AllyStats c)
    {
        c.ChangeVitality(-Vit);
        c.ChangeStrength(-Str);
        c.ChangeMagic(-Mgk);
        c.ChangeSpeed(-Spd);
    }
}
