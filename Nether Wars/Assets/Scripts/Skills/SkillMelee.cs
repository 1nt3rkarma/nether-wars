using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMelee : Skill
{
    [Tooltip("Урон наносится 1 цели в зоне поражения, или любому количеству?")]
    public bool splashDamage = false;

    [Tooltip("Отбрасывать цель назад при нанесении урона?")]
    public bool knockBack = true;

    [Tooltip("Сила отбрасывания")]
    [Range(1f, 4f)]
    public float knockBackForce = 2.5f;

    void Start()
    {
        coolDown = caster.attackCoolDown;
    }

    void Update()
    {
        if (coolDownTimer > 0)
            coolDownTimer -= Time.deltaTime;
    }

    public override void Cast()
    {
        base.Cast();
        CastDamage();
    }

    public void CastDamage()
    {
        if (splashDamage)
        {
            Vector3 attackPoint = caster.GetAttackPoint();
            float hitArea = caster.interactionRange / 2;
            Collider[] colliders = Physics.OverlapSphere(attackPoint, hitArea);
            foreach (var collider in colliders)
            {
                Unit target = collider.GetComponent<Unit>();
                if (target)
                    if (target != caster && target.isAlive && target.team != caster.team)
                    {
                        float realDamage = caster.MeleeAttack(target);
                        if (realDamage > 0 && knockBack)
                            KnockBack(target, knockBackForce);
                    }
            }
        }
        else
            caster.MeleeAttack(targetUnit);
    }

    void KnockBack(Unit target, float force)
    {
        Vector3 direction = (target.position - caster.position);
        target.navigationAgent.velocity += direction * force;
    }
}
