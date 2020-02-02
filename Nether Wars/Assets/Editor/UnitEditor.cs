using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Unit))]
public class UnitEditor : Editor
{
    Object addSkill = null;
    List<Skill> skillToRemove = new List<Skill>();
    bool showSkillMenu = false;
    bool showSkillList = false;
    bool sorted;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Unit unit = (Unit) target;

        if (unit.skillManager)
        {
            showSkillMenu = EditorGUILayout.BeginFoldoutHeaderGroup(showSkillMenu, "Skills");
            if (showSkillMenu)
            {
                #region Окно добавления способностей

                EditorGUILayout.BeginHorizontal();

                addSkill = (Object)EditorGUILayout.ObjectField("Add skill:",
                    addSkill, typeof(Object), false);
                if (addSkill is Skill)
                    unit.skillManager.AddSkill((Skill)addSkill);
                else if (addSkill is GameObject)
                    unit.skillManager.AddSkill((GameObject)addSkill);
                addSkill = null;

                EditorGUILayout.EndHorizontal();
                #endregion

                #region Отрисовка списка способностей
                //showSkillList = EditorGUILayout.Foldout(showSkillList, "Список");
                int count = unit.skillManager.skills.Count;
                if (count > 0)
                {
                    if (GUILayout.Button("Clear list"))
                        unit.skillManager.RemoveAllSkillsEditor();

                    sorted = EditorGUILayout.Toggle("Sort", sorted);
                    List<Skill> skills = unit.skillManager.GetSkillList(sorted);
                    foreach (var skill in skills)
                    {
                        if (skill == null)
                            skillToRemove.Add(skill);
                        else
                        {
                            EditorGUILayout.BeginVertical();

                            EditorGUILayout.LabelField(skill.label);

                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.ObjectField(skill, typeof(Skill), false, GUILayout.MinWidth(144), GUILayout.MaxWidth(300));
                            if (GUILayout.Button("Remove", GUILayout.MinWidth(96), GUILayout.MaxWidth(128)))
                                skillToRemove.Add(skill);

                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.EndVertical();
                        }
                    }

                    foreach (var skill in skillToRemove)
                        unit.skillManager.RemoveSkillEditor(skill);

                    skillToRemove.Clear();
                }
                else
                    EditorGUILayout.LabelField("List is empty");
                #endregion
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
