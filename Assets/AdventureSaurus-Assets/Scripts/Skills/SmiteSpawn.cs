using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmiteSpawn : MonoBehaviour
{
    private Health dealDamageTo;
    public Health DealDamageTo
    {
        set { dealDamageTo = value; }
    }

    private Stats giveXPTo;
    public Stats GiveXPTo
    {
        set { giveXPTo = value; }
    }

    private int damage;
    public int Damage
    {
        set { damage = value; }
    }

    public void SmiteThee()
    {
        
        dealDamageTo.TakeDamage(damage, giveXPTo);
        Destroy(this.gameObject);
    }
    
}
