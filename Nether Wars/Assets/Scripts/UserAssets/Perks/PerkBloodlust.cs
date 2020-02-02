using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Кровожадный", menuName = "Custom/Perks/Bloodthirsting")]
public class PerkBloodlust : Perk
{
    [Range(0f, 2f)]
    public float attackSpeedBonus = 0.1f;

    public override void EffectOnAdd(Unit unit)
    {
        unit.attackCoolDown /= (1 + attackSpeedBonus);
    }

    public override void EffectOnRemove(Unit unit)
    {
        unit.attackCoolDown = unit.unitType.attackCoolDown;
    }
}
