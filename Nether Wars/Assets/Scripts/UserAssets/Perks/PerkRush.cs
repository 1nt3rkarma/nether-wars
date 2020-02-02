using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Перк - Стремительный", menuName = "Custom/Perks/Rushing")]
public class PerkRush : Perk
{
    [Range(0f,2f)]
    public float moveSpeedBonus = 0.1f;

    public override void EffectOnAdd(Unit unit)
    {
        unit.SetMoveSpeed(unit.moveSpeed * (1 + moveSpeedBonus));
    }

    public override void EffectOnRemove(Unit unit)
    {
        unit.SetMoveSpeed(unit.unitType.moveSpeed);
    }
}
