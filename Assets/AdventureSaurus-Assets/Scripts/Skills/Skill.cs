using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    
    private TurnSystem turnSysRef;//need turn system to call isPlayerDone after using Skill
    private MoveAttackGUIController maGuiControllerRef;
    private MoveAttack maRef;// reference to the selected character's MoveAttack script
    [SerializeField] private List<Vector2Int> rangeTiles = null;
    [SerializeField] private int cooldown = 1;
    [SerializeField] private int skillNum = -1;
    [SerializeField] private int damage = 0;
    [SerializeField] private Animator anime = null;//damage to deal, amount to heal ,and amount to buff

    private void Awake()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if(gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else{
            turnSysRef = gameControllerObj.GetComponent<TurnSystem>();
            if (turnSysRef == null)
            {
                Debug.Log("Could not find TurnSystem attached to " + gameControllerObj.name);
            }
            maGuiControllerRef = gameControllerObj.GetComponent<MoveAttackGUIController>();
            if (maGuiControllerRef== null)
            {
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameControllerObj.name);
            }
        }
    }

    virtual public void DoSkill()
    {
        anime.SetInteger("SkillNum", skillNum);
    }
   
}
