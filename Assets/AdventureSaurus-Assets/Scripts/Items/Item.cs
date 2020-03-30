using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType 
{
    LightArmor, 
    HeavyArmor, 
    MagicArmor,
    LightBoots, 
    HeavyBoots,
    Sword, 
    Axe, 
    Staff,
    Rune, 
    Other,
}


[CreateAssetMenu]
public class Item : ScriptableObject
{
    private string ItemName;
    public Sprite Icon;

    public int Vit;
    public int Str;
    public int Mgk;
    public int Spd;
    [Space]
    public ItemType ItemType;
}
