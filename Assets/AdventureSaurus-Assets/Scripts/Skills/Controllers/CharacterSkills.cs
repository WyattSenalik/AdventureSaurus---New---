using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is meant to be a parent of a similar script that will be attached to allies and enemies
public class CharacterSkills : MonoBehaviour
{
    // The skills this character current has available to them to use
    protected List<Skill> _availableSkills;
    public List<Skill> GetAvailableSkills() { return new List<Skill>(_availableSkills); }
    // Reference to the MoveAttack script attached to this character
    protected MoveAttack _mARef;
    // Reference to the skill holder
    protected SkillHolder _skillHoldRef;


    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called 0th
    // Set references to itself
    private void Awake()
    {
        _mARef = this.GetComponent<MoveAttack>();
        if (_mARef == null)
            Debug.Log("WARNING - There was no MoveAttack script attached to " + this.name);
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("WARNING - No GameObject with the tag GameController was found");
        _skillHoldRef = gameContObj.GetComponent<SkillHolder>();
        if (_skillHoldRef == null)
            Debug.Log("WARNING - There was no SkillHolder script attached to " + gameContObj.name);

        // Initialize the list of skills
        _availableSkills = new List<Skill>();

        // Subscribe to the procedural gen event
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    /// <summary>
    /// Called when generation is finished.
    /// Initialize some things.
    /// </summary>
    protected virtual void Initialize()
    {
        // TODO in children
    }

    /// <summary>
    /// Sets the character's active skill in MoveAttack
    /// </summary>
    /// <param name="skillToSet">The skill that will be active now</param>
    protected void SetSkill(Skill skillToSet)
    {
        _mARef.SkillRef = skillToSet;
    }

    /// <summary>
    /// Calls the SpawnSkillAddition that spawns a prefab with another animation on it
    /// </summary>
    public void SpawnSkillAdditionForActive()
    {
        _mARef.SkillRef.SpawnSkillAddition();
    }
}
