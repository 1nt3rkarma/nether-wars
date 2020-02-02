using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameResources))]
public class GameResourcesEditor : Editor
{
    GUIStyle fontStyleBold;
    GUIStyle fontStyleItalic;

    bool showPerksCatalogue = false;
    bool showPerksList = false;
    bool[] showElementSection = new bool[Alchemy.elementsCount];
    Object addPerk = null;

    bool showRaceCatalog = false;
    bool[] showRace = new bool[5];

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameResources resourceFile = (GameResources)target;

        fontStyleBold = new GUIStyle();
        fontStyleBold.fontStyle = FontStyle.Bold;

        fontStyleItalic = new GUIStyle();
        fontStyleItalic.fontStyle = FontStyle.Italic;

        int bd = (int)Elements.blood;
        int bn = (int)Elements.bone;
        int cd = (int)Elements.cinder;
        int sl = (int)Elements.slime;
        int sh = (int)Elements.shadow;

        #region Каталог рас
        showRaceCatalog = EditorGUILayout.Foldout(showRaceCatalog, "Race catalogue");
        if (showRaceCatalog)
        {
            List<Race> race = new List<Race>()
        {
            resourceFile.BloodBound,
            resourceFile.BoneGnashers,
            resourceFile.AshRisen,
            resourceFile.Vilespawn,
            resourceFile.Revenants
        };
            string[] raceLabel = new string[Alchemy.elementsCount];
            raceLabel[bd] = "Bloodbounds";
            raceLabel[bn] = "Bonegnashers";
            raceLabel[cd] = "Ashrisen";
            raceLabel[sl] = "Vilespawn";
            raceLabel[sh] = "Revenants";

            EditorGUILayout.BeginVertical();

            resourceFile.defaultUnit = (Unit)EditorGUILayout.ObjectField("Default creature",
                   resourceFile.defaultUnit, typeof(Unit), false, GUILayout.MinWidth(128), GUILayout.MaxWidth(300));

            for (int r = 0; r < race.Count; r++)
            {
                showRace[r] = EditorGUILayout.Foldout(showRace[r], raceLabel[r]);
                if (showRace[r])
                {
                    if (race[r].tiers == null)
                    {
                        race[r].tiers = new List<UnitTier>();
                        for (int i = 0; i < 5; i++)
                            race[r].tiers.Add(new UnitTier());
                    }
                    //Debug.Log("Тиры точно существуют");
                    if (race[r].tiers.Count != 5)
                    {
                        race[r].tiers = new List<UnitTier>();
                        for (int i = 0; i < 5; i++)
                            race[r].tiers.Add(new UnitTier());
                    }
                    //Debug.Log("Тиров ровно 5");
                    EditorGUILayout.BeginVertical();
                    for (int i = 0; i < 5; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Ур. " + (i + 1), GUILayout.Width(32));
                        // Обычный
                        race[r].tiers[i].generic = (Unit)EditorGUILayout.ObjectField(
                            race[r].tiers[i].generic, typeof(Unit), false, GUILayout.MinWidth(64), GUILayout.MaxWidth(100));
                        // Улучшенный
                        race[r].tiers[i].updgrade = (Unit)EditorGUILayout.ObjectField(
                            race[r].tiers[i].updgrade, typeof(Unit), false, GUILayout.MinWidth(64), GUILayout.MaxWidth(100));
                        // Редкий
                        race[r].tiers[i].rare = (Unit)EditorGUILayout.ObjectField(
                            race[r].tiers[i].rare, typeof(Unit), false, GUILayout.MinWidth(64), GUILayout.MaxWidth(100));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Архидемон", GUILayout.MinWidth(98), GUILayout.MaxWidth(136));
                    race[r].general = (Unit)EditorGUILayout.ObjectField(
                        race[r].general, typeof(Unit), false, GUILayout.MinWidth(128), GUILayout.MaxWidth(204));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Каталог перков
        showPerksCatalogue = EditorGUILayout.Foldout(showPerksCatalogue, "Perks catalogue");
        if (showPerksCatalogue)
        {
            EditorGUILayout.BeginVertical();

            #region Окно добавление перков

            if (GUILayout.Button("Clear"))
                resourceFile.alchemy.perksCatalogue.Clear();

            EditorGUILayout.BeginHorizontal();

                addPerk = (Object)EditorGUILayout.ObjectField("Add perk:",
                    addPerk, typeof(Object), false);
                if (addPerk is Perk)
                    if (!resourceFile.alchemy.perksCatalogue.Contains((Perk)addPerk))
                        resourceFile.alchemy.perksCatalogue.Add((Perk)addPerk);
                
                addPerk = null;

            EditorGUILayout.EndHorizontal();
            #endregion

            #region Отсортированный список перков

            showPerksList = EditorGUILayout.Foldout(showPerksList, "Perk list");
            if (showPerksList)
            {
                List<Perk> perksToRemove = new List<Perk>();

                #region Сортировка и рисование

                string[] elementLabel = new string[Alchemy.elementsCount];
                elementLabel[bd] = "Blood";
                elementLabel[bn] = "Bone";
                elementLabel[cd] = "Cinder";
                elementLabel[sl] = "Slime";
                elementLabel[sh] = "Shadow";

                List<List<Perk>> perksBlood = new List<List<Perk>>()
                {
                     new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>()
                };
                List<List<Perk>> perksBone = new List<List<Perk>>()
                {
                    new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>()
                };
                List<List<Perk>> perksCinder = new List<List<Perk>>()
                {
                    new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>()
                };
                List<List<Perk>> perksSlime = new List<List<Perk>>()
                {
                    new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>()
                };
                List<List<Perk>> perksShadow = new List<List<Perk>>()
                {
                    new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>(), new List<Perk>()
                };

                List<List<List<Perk>>> perksElements = new List<List<List<Perk>>>()
                { perksBlood, perksBone, perksCinder, perksSlime, perksShadow};

                foreach (var perk in resourceFile.alchemy.perksCatalogue)
                    if (perk != null)
                        perksElements[(int)perk.element][perk.level].Add(perk);
                    else
                        perksToRemove.Add(perk);

                for (int e = 0; e < perksElements.Count; e++)
                {
                    showElementSection[e] = EditorGUILayout.BeginFoldoutHeaderGroup(showElementSection[e], elementLabel[e]);
                    if (showElementSection[e])
                    {
                        for (int l = 0; l < perksElements[e].Count; l++)
                            if (perksElements[e][l].Count > 0)
                            {
                                EditorGUILayout.LabelField("Level " + l + ":", fontStyleBold);
                                foreach (var perk in perksElements[e][l])
                                {
                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.ObjectField(perk, typeof(Perk), false, GUILayout.MinWidth(144), GUILayout.MaxWidth(300));
                                    if (GUILayout.Button("Remove", GUILayout.MinWidth(96), GUILayout.MaxWidth(128)))
                                        perksToRemove.Add(perk);

                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.LabelField(perk.hint, fontStyleItalic);
                                    EditorGUILayout.EndVertical();
                                }
                            }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                #endregion

                foreach (var perk in perksToRemove)
                    resourceFile.alchemy.perksCatalogue.Remove(perk);
                perksToRemove.Clear();
            }
            #endregion

            EditorGUILayout.EndVertical();
        }
        #endregion

        EditorUtility.SetDirty(target);
    }
}
