using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    [Tooltip("Юнит, которым управляет данный ИИ")]
    public Unit unit;

    [Range(2f,10f)][Tooltip("Радиус поиска цели")]
    public float targetingRange = 3;

    [Tooltip("Атака по умолчанию, которую будет пытаться применить ИИ данного юнита, " +
        "обнаружив противника, если другие способности недоступны")]
    public Skill basicAttack;

    [Tooltip("Поведение юнита при виде противника")]
    public CombatModes combatMode = CombatModes.regular;

    [Tooltip("Поведение движения юнита (стоит на месте, патрулирует или бродит вокруг)")]
    public MovementModes movementMode = MovementModes.none;

    //[Tooltip("Бродит ли существо вокруг?")]
    //public bool wandering = true;
    //[Tooltip("Период брождения вокруг")][Range(15f,75f)]
    //public float wanderingPeriod = 25;
    //[HideInInspector] public float wanderingTimer = 10;

    public States state = States.idling;
    [HideInInspector] public Unit mainTarget = null;
    [HideInInspector] public List<Unit> enemiesSpotted = new List<Unit>();
    [HideInInspector] public bool ignoringEnemies = false;


    void Update()
    {
        if (unit.isAlive)
            AI();
    }

    public virtual void AI()
    {
        //if (wanderingTimer > 0)
            //wanderingTimer -= Time.deltaTime;

        // Если юнит занят применением скилла, то ИИ не анализирует окружающую обстановку
        if (unit.activeSkill)
            return;


        // Осматриваемся вокруг в поисках противников
        DetectEnemies();

        if (mainTarget)
        {
            if (!mainTarget.isAlive)
                mainTarget = null;
            else
            {
                Skill skill = PickSkill(mainTarget, out CastError feedback);
                if (skill == null)
                    return;

                if (feedback == CastError.tooFar)
                    IssueMoveOrder(mainTarget.position);
                else if (feedback == CastError.success)
                    IssueSkillOrder(skill, mainTarget);
            }            
        }
        else
        {
            mainTarget = SearchTarget();
            if (mainTarget == null)
            {
                //if (wandering && wanderingTimer <= 0 && !unit.isOnMove)
                //{
                    //Vector3 point = TerrainSystem.GetRandomFreePoint(true);
                    //IssueMoveOrder(point);
                   // wanderingTimer = wanderingPeriod + Random.Range(-wanderingPeriod*0.1f, wanderingPeriod * 0.1f);
                //}
                //else if (!wandering && !unit.isOnMove)
                    //unit.Stop();
            }
        }
    }
                    
    public void OnIdle()
    {

    }

    public Unit SearchTarget()
    {
        int count = enemiesSpotted.Count;
        if (count == 0)
            return null;

        Unit wanted = enemiesSpotted[0];
        float minSqrDistance = Unit.GetSqrFlatDistance(unit, wanted);
        for (int i = 1; i < count; i++)
        {
            float sqrDistance = Unit.GetSqrFlatDistance(unit, enemiesSpotted[i]);
            if (sqrDistance < minSqrDistance)
            {
                wanted = enemiesSpotted[i];
                minSqrDistance = sqrDistance;
            }
        }
        return wanted;
    }

    public void DetectEnemies()
    {
        int count = enemiesSpotted.Count;

        // Очистка пустых ссылок
        if (count > 0)
        {
            List<Unit> checkList = new List<Unit>(enemiesSpotted);
            foreach (var enemy in checkList)
                if (enemy == null)
                    enemiesSpotted.Remove(enemy);
                else if (!enemy.isAlive)
                    enemiesSpotted.Remove(enemy);
        }

        // Лимит количества противников, которое можно обнаружить
        count = enemiesSpotted.Count;
        if (count < 7)
        {
            List<Unit> unitsInRange = Unit.GetUnitsInRange(unit.position, targetingRange);
            foreach (var target in unitsInRange)
            {
                if (!enemiesSpotted.Contains(target))
                    if (target != unit && target.isAlive && target.team != unit.team)
                        enemiesSpotted.Add(target);
                
                if (enemiesSpotted.Count >= 7)
                    return;
            }
        }
    }

    public Skill PickSkill(Unit target, out CastError feedback)
    {
        foreach (var skill in unit.skillManager.skills)
        {
            if (skill.coolDownTimer <= 0 && skill.targeting == InteractionTargeting.unit)
            {
                feedback = skill.CheckCast(target);
                if (feedback == CastError.success || feedback == CastError.tooFar || feedback == CastError.tooNear)
                    return skill;
            }
        }
        feedback = CastError.wrongTarget;
        return null;
    }

    public void IssueSkillOrder(Skill skill, Unit target)
    {
        if (unit.isOnMove)
            unit.Stop();

        float angleDelta = Mathf.Abs(unit.GetFacingAngle() - unit.AngleToPoint(target.position));
        if (angleDelta < 10)                            
            unit.ActivateSkill(skill, target);        // Если герой стоит лицом к цели - кастуем
        else
            unit.RotateFrame(target.position);    // Иначе - поворачиваемся
    }

    public void IssueMoveOrder(Vector3 toPoint)
    {
        // Проверяем расстояние до точки
        if (Vector3.Distance(unit.transform.position, toPoint) > 1)
        {
            // Если оно БОЛЬШЕ 1 метра, то отправляем юнита туда маршировать
            unit.SetMovePoint(toPoint);
        }
        else
        {
            // Если оно МЕНЬШЕ 1 метра, то просто разворачиваемся
            unit.Stop();
            unit.Face(toPoint, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, targetingRange);

            foreach (var enemy in enemiesSpotted)
            {
                if (enemy == mainTarget)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.yellow;
                Gizmos.DrawLine(unit.position, enemy.position);
            }
        }
    }

    public enum MovementModes { none, wandering, patroling }

    /* Описание режимов:
     * panicing - при виде Противника Существо обращается в бегство;
     * peaceful - Существо игнорирует Противника, то тех пор пока Противник не причинит Существу ущерб;
     * regular - замечая Противника, Существо нападает на него и преследует в течение некоторого времени
     * defensive - замечая Противника, Существо не двигается с места, и атакует только в случае,
     *             если Противник вышел на дистанцию атаки ближнего/дальнего боя;
     * agressive - замечая Противника, Существо нападает на него и преследует пока противник
     *             не скроется из виду или не умрет.
     */
    public enum CombatModes { panicing, peaceful, regular, defensive, agressive }

    public enum States { idling, movingIdle, movingStrict, casting, }
}
