using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perk : ScriptableObject
{
    [Tooltip("Название перка, которое увидит Игрок")]
    public string label;

    [Tooltip("Стихия, к которой относится этой перк")]
    public Elements element;

    [Range(1,5)][Tooltip("Уровень перка")]
    public int level = 1;

    [Tooltip("Что именно модифицирует данный перк?")]
    public PerkType type;

    [Tooltip("Суммирируется ли эффект с другими аналогичными перками?")]
    public bool sumsUp = false;

    [TextArea][Tooltip("Короткая подсказка")]
    public string hint;

    public virtual void EffectOnAdd(Unit unit){ }

    public virtual void EffectOnRemove(Unit unit) { }

    public virtual void EffectOnAttack(Unit caster, Unit target) { }

    public virtual void EffectOnUpdate(Unit unit) { }

    public virtual float GetDamageSummand(Elements damageType, AttackTypes attackType) { return 0; }

    public virtual float GetDamageFactor(Elements damageType, AttackTypes attackType) { return 1; }
}

// Выставляя тип 'unique' вы гарантируете, что эффект перка
// уникален и не может даже в теории пересекаться с эффектами других перков
public enum PerkType { none, attackSpeed, moveSpeed, unique}