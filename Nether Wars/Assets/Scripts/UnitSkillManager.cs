using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnitSkillManager : MonoBehaviour
{
    [Tooltip("Юнит-владелец этого списка способностей")]
    public Unit caster;
    [HideInInspector]
    public List<Skill> skills;

    public List<Skill> GetSkillList(bool sorted)
    {
        if (!sorted)
            return new List<Skill>(skills);

        List<Skill> skillsSorted = new List<Skill>();
        foreach (var skill in skills)
        {
            if (skillsSorted.Count > 0)
            {
                if (skill.AIPriority < skillsSorted[0].AIPriority)
                    skillsSorted.Insert(0, skill);
                else if (skill.AIPriority > skillsSorted[skillsSorted.Count - 1].AIPriority)
                    skillsSorted.Add(skill);
                else
                {
                    foreach (var skillSorted in skillsSorted)
                        if (skill.AIPriority >= skillSorted.AIPriority)
                        {
                            int index = skillsSorted.IndexOf(skillSorted);
                            skillsSorted.Insert(index, skill);
                        }                    
                }

            }
            else
            {
                skillsSorted.Add(skill);
            }
        }
        return skillsSorted;
    }

    public void AddSkill(Skill skillPrefab)
    {
        Skill newSkill = Instantiate(skillPrefab, transform);
        newSkill.name = newSkill.label;
        newSkill.caster = caster;
        skills.Add(newSkill);
    }
    public void AddSkillEditor(Skill skillPrefab)
    {
        Skill newSkill = Instantiate(skillPrefab, transform);
        newSkill.name = newSkill.label;
        newSkill.caster = caster;
        skills.Add(newSkill);
    }
    public void AddSkill(GameObject skillPrefab)
    {
        Component[] components = skillPrefab.GetComponents(typeof(Component));
        foreach (var component in components)
        {
            if (component is Skill)
            {
                AddSkill((Skill)component);
                return;
            }
        }
    }
    public void AddSkillEditor(GameObject skillPrefab)
    {
        Component[] components = skillPrefab.GetComponents(typeof(Component));
        foreach (var component in components)
        {
            if (component is Skill)
            {
                AddSkillEditor((Skill)component);
                return;
            }
        }
    }

    public void RemoveAllSkills()
    {
        List<Skill> skillsToRemove = new List<Skill>();
        foreach (var skill in skills)
            skillsToRemove.Add(skill);
        foreach (var skill in skillsToRemove)
            RemoveSkill(skill);
    }
    public void RemoveAllSkillsEditor()
    {
        List<Skill> skillsToRemove = new List<Skill>();
        foreach (var skill in skills)
            skillsToRemove.Add(skill);
        foreach (var skill in skillsToRemove)
            RemoveSkillEditor(skill);
    }

    public void RemoveSkill(Skill skill)
    {
        if (skills.Contains(skill))
            skills.Remove(skill);
        Destroy(skill.gameObject);
    }
    public void RemoveSkillEditor(Skill skill)
    {
        if (skills.Contains(skill))
            skills.Remove(skill);
        DestroyImmediate(skill.gameObject);
    }
}
