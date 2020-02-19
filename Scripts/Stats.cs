using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [SerializeField] private int strength = 1;
    public int Strength
    {
        get { return strength; }
    }
    [SerializeField] private int magic = 0;
    public int Magic
    {
        get { return magic; }
    }
    [SerializeField] private int speed = 2;
    public int Speed
    {
        get { return speed; }
    }
    [SerializeField] private int defense = 2;
    public int Defense
    {
        get { return defense; }
    }
    [SerializeField] private int vitality = 2;
    public int Vitality
    {
        get { return vitality; }
    }
    private MoveAttack mARef;   // Reference to the MoveAttack script attached to this character
    private Health hpRef;   // Reference to the Health script attached to this character

    private void Awake()
    {
        mARef = this.GetComponent<MoveAttack>();
        if (mARef == null)
            Debug.Log("Could not find MoveAttack attached to " + this.name);
        else
        {
            mARef.MoveRange = speed;
            mARef.DmgToDeal = strength;
        }

        hpRef = this.GetComponent<Health>();
        if (hpRef == null)
            Debug.Log("Could not find Health attached to " + this.name);
        else
        {
            hpRef.MaxHP = vitality;
        }
    }

    public void Initialize()
    {
        mARef.MoveRange = speed;
        mARef.DmgToDeal = strength;
        hpRef.MaxHP = vitality;
    }
}
