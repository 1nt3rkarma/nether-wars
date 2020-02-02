using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Файл Ресурсов",menuName ="Custom/ResourceFile")]
public class GameResources : ScriptableObject
{
    //public SoundResources sounds;

    public AlchemyResources alchemy;

    [HideInInspector] public Unit defaultUnit;
    [HideInInspector] public Race BloodBound;
    [HideInInspector] public Race BoneGnashers;
    [HideInInspector] public Race AshRisen;
    [HideInInspector] public Race Vilespawn;
    [HideInInspector] public Race Revenants;

    public Color GetElementColor(Elements element)
    {
        switch (element)
        {
            case Elements.blood:
                return alchemy.colorBlood;
            case Elements.bone:
                return alchemy.colorBone;
            case Elements.cinder:
                return alchemy.colorCinder;
            case Elements.slime:
                return alchemy.colorSlime;
            case Elements.shadow:
                return alchemy.colorShadow;
            default:
                return Color.white;
        }
    }
}

[System.Serializable]
public class SoundResources: object
{

}

[System.Serializable]
public class AlchemyResources: object
{
    public Sprite icoBlood;
    public Color colorBlood;

    public Sprite icoBone;
    public Color colorBone;

    public Sprite icoCinder;
    public Color colorCinder;

    public Sprite icoSlime;
    public Color colorSlime;

    public Sprite icoShadow;
    public Color colorShadow;

    [HideInInspector] public List<Perk> perksCatalogue;
}

[System.Serializable]
public class Race
{
    public List<UnitTier> tiers = new List<UnitTier>(5);
    public Unit general;
}

[System.Serializable]
public class UnitTier
{
    public Unit generic;
    public Unit updgrade;
    public Unit rare;
}