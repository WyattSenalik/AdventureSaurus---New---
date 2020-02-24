using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{

    protected TurnSystem turnSysRef;//need turn system to call isPlayerDone after using Skill
    protected MoveAttackGUIController mAGUIContRef;
    protected MoveAttack maRef;// reference to the selected character's MoveAttack script
    protected MoveAttackController mAContRef;
    protected EnemyMoveAttackAI enMAAIRef;
    protected List<Vector2Int> rangeTiles = null;
    protected int skillNum = -1;
    protected List<Health> enemiesHP;
    [SerializeField] private int cooldown = 1;
    [SerializeField] protected int damage = 0;
    [SerializeField] protected Animator anime = null;//damage to deal, amount to heal ,and amount to buff
    [SerializeField] protected SpriteRenderer sprRendRef = null;

    private void Awake()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else
        {
            turnSysRef = gameControllerObj.GetComponent<TurnSystem>();
            if (turnSysRef == null)
            {
                Debug.Log("Could not find TurnSystem attached to " + gameControllerObj.name);
            }
            mAGUIContRef = gameControllerObj.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
            {
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameControllerObj.name);
            }
            mAContRef = gameControllerObj.GetComponent<MoveAttackController>();
            if (mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameControllerObj.name);
            }
            enMAAIRef = gameControllerObj.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
            {
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameControllerObj.name);
            }
        }
    }

    virtual public void DoSkill()
    {
        anime.SetInteger("SkillNum", skillNum);
    }

    /// <summary>
    /// Calls StartSkillAnimation and gets reference to the enemies damaged by this skill
    /// </summary>
    virtual public void StartSkill(List<Vector2Int> attackNodesPos)
    {
        Debug.Log("StartAttack not implemented");
    }

    /// <summary>
    /// Stops the skill animation and deals damage to any enemy that was hit and if no enemies hit gives control back to the appropriate authority.
    /// </summary>
    virtual public void EndSkill()
    {
        Debug.Log("EndAttack not implemented");

    }

    /// <summary>
    /// This starts the animation of the skill.
    /// </summary>
    /// <param name="attackNodePos">This represents the direction the charcter will strike.</param>
    protected void StartSkillAnimation(Vector2Int attackNodePos)
    {
        int attackDirection = -1;
        // If I am below the node I am striking, I should attack up
        if (attackNodePos.y - this.transform.position.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 1;
        }
        // If I am right the node I am striking, I should attack left
        if (this.transform.position.x - attackNodePos.x > 0)
        {
            sprRendRef.flipX = true;
            attackDirection = 2;
        }
        // If I am left the node I am striking, I should attack right
        if (attackNodePos.x - this.transform.position.x > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 3;
        }
        // If I am above the node I am striking, I should attack down
        if (this.transform.position.y - attackNodePos.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 4;
        }

        anime.SetInteger("SkillNum", skillNum);
        //Debug.Log("Start Attack");
        anime.SetInteger("AttackDirection", attackDirection);
    }

    /// <summary>
    /// This ends the animation of the skill.
    /// </summary>
    protected void EndSkillAnimation()
    {
        anime.SetInteger("AttackDirection", -anime.GetInteger("AttackDirection"));
        anime.SetInteger("SkillNum", -1);
    }
}
