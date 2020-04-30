using UnityEngine;
using UnityEngine.UI;

public class SkillHolder : MonoBehaviour
{
    // All of the skills obtainable in the game
    private static readonly string[] _skillNames = { "BasicAttack", "Heal", "Smite", "ThreeSixtySwing", "Push" };
    // The icons for each skill
    private static Sprite[] _skillIcons;
    [SerializeField] private Sprite[] _skillIconsEditor = null;
    // Descriptions for each skill
    private static readonly string[] _skillDescriptions = {"Simple attack. Its damage is based off strength.",
        "Restores health to a friendly character.", "Ranged magic attack.",
        "Hits all characters in a circle around the user.", "Moves a character away from the user."};

    public const byte BASIC_ATTACK = 0;
    public const byte HEAL = 1;
    public const byte SMITE = 2;
    public const byte THREE_SIXTY_SWING = 3;
    public const byte PUSH = 4;


    // Called before start
    private void Awake()
    {
        _skillIcons = _skillIconsEditor;
    }

    /// <summary>
    /// Bestoys a skill upon an ally
    /// </summary>
    /// <param name="chosenChara">The ally chosen to get the skill</param>
    /// <param name="skillIndex">The index of the skill</param>
    public Skill GiveSkill(AllySkillController chosenChara, byte skillIndex)
    {
        if (skillIndex >= _skillNames.Length)
        {
            Debug.Log("There is no skill at that index");
            return null;
        }

        // Attaches the desired skill if the ally does not already have it
        switch (_skillNames[skillIndex])
        {
            case ("BasicAttack"):
                if (!HasSkill<BasicAttack>(chosenChara))
                    return chosenChara.gameObject.AddComponent<BasicAttack>();
                break;
            case ("Heal"):
                if (!HasSkill<Heal>(chosenChara))
                    return chosenChara.gameObject.AddComponent<Heal>();
                break;
            case ("Smite"):
                if (!HasSkill<Smite>(chosenChara))
                    return chosenChara.gameObject.AddComponent<Smite>();
                break;
            case ("ThreeSixtySwing"):
                if (!HasSkill<ThreeSixtySwing>(chosenChara))
                    return chosenChara.gameObject.AddComponent<ThreeSixtySwing>();
                break;
            default:
                Debug.Log("WARNING - BUG DETECTED - Unrecognized Skill to acquire");
                return null;
        }
        Debug.Log(chosenChara.name + " already has " + _skillNames[skillIndex] + " attached");
        return null;
    }

    /// <summary>
    /// Checks if the character already has the specified skill
    /// </summary>
    /// <typeparam name="T">Skill to check if the character has</typeparam>
    /// <param name="chosenChara">Character to check if they have the skill</param>
    /// <returns>True if has skill. False if thye do not.</returns>
    private bool HasSkill<T>(AllySkillController chosenChara)
    {
        Skill hasSkill = chosenChara.gameObject.GetComponent<T>() as Skill;
        return hasSkill != null;
    }

    /// <summary>
    /// Removes a skill from an ally
    /// </summary>
    /// <param name="chosenChara">The ally chosen to lose the skill</param>
    /// <param name="skillIndex">The index of the skill</param>
    public void RemoveSkill(AllySkillController chosenChara, byte skillIndex)
    {
        if (skillIndex >= _skillNames.Length)
        {
            Debug.Log("There is no skill at that index");
            return;
        }

        // Removes the desired skill if the ally has it
        switch (_skillNames[skillIndex])
        {
            case ("BasicAttack"):
                if (HasSkill<BasicAttack>(chosenChara))
                    Destroy(chosenChara.gameObject.GetComponent<BasicAttack>());
                break;
            case ("Heal"):
                if (HasSkill<Heal>(chosenChara))
                    Destroy(chosenChara.gameObject.GetComponent<Heal>());
                break;
            case ("Smite"):
                if (HasSkill<Smite>(chosenChara))
                    Destroy(chosenChara.gameObject.GetComponent<Smite>());
                break;
            case ("ThreeSixtySwing"):
                if (HasSkill<ThreeSixtySwing>(chosenChara))
                    Destroy(chosenChara.gameObject.GetComponent<ThreeSixtySwing>());
                break;
            default:
                Debug.Log("WARNING - BUG DETECTED - Unrecognized Skill to acquire");
                return;
        }
        Debug.Log(chosenChara.name + " already has " + _skillNames[skillIndex] + " attached");
    }

    /// <summary>
    /// Returns the name of the skill with the given index
    /// </summary>
    /// <param name="index">Index of the skill</param>
    /// <returns>string name of skill</returns>
    public static string GetSkillName(int index)
    {
        return _skillNames[index];
    }

    /// <summary>
    /// Returns the sprite icon of the skill with the given index
    /// </summary>
    /// <param name="index">Index of the skill</param>
    /// <returns>Sprite icon of skill</returns>
    public static Sprite GetSkillImage(int index)
    {
        return _skillIcons[index];
    }

    /// <summary>
    /// Returns the description of the skill with the given index
    /// </summary>
    /// <param name="index">Index of the skill</param>
    /// <returns>string description of skill</returns>
    public static string GetSkillDescription(int index)
    {
        return _skillDescriptions[index];
    }
}
