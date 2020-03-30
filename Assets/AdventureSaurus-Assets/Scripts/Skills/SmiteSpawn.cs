using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmiteSpawn : MonoBehaviour
{
    private Health _dealDamageTo;
    public Health DealDamageTo
    {
        set { _dealDamageTo = value; }
    }

    private Stats _giveXPTo;
    public Stats GiveXPTo
    {
        set { _giveXPTo = value; }
    }

    private int _damage;
    public int Damage
    {
        set { _damage = value; }
    }

    public void SmiteThee()
    {
        
        _dealDamageTo.TakeDamage(_damage, _giveXPTo);
        Destroy(this.gameObject);
    }
    
}
