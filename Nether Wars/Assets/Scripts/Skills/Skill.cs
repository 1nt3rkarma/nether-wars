using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [Tooltip("Название, отображаемое для Игрока")]
    public string label = "Способность";

    [Tooltip("Юнит-обладатель данного скилла")]
    public Unit caster;

    #region Основные функциональные свойства
    [Tooltip("Тип цели")]
    public InteractionTargeting targeting;

    [Tooltip("Тип взаимодействия")]
    public InteractionRange range;

    [Range(0f, 20f)][Tooltip("Макс. дальность взаимодействия в дальнем бою")]
    public float distanceMax = 2;

    [Range(0f, 20f)][Tooltip("Мин. дальность взаимодействия в дальнем бою")]
    public float distanceMin = 0;

    [Range(0.1f, 5f)][Tooltip("Задержка между атаками")]
    public float coolDown = 2;
    [HideInInspector]
    public float coolDownTimer = 0;

    [Range(0f, 10f)][Tooltip("Время подготовки скилла к касту")]
    public float delay = 0;
    [HideInInspector]
    public float delayTimer = 0;

    [Range(0f, 100f)][Tooltip("Требует маны")]
    public int manaCost = 0;

    [Tooltip("Разрешенные цели")]
    public TargetPermission targetPermissions;

    [Range(0,100)][Tooltip("Насколько данный скилл приоритетен для применения ИИ по сравнению с другими способностями в списке?")]
    public int AIPriority = 1;
    #endregion

    #region Графические настройки
    [Tooltip("Тэг анимации, которая соответствует этому касту (обычно 'attack или 'cast')")]
    public string animationTag;

    [Tooltip("Спецэффект воспроизводимый для цели")]
    public GameObject targetSFX;

    [Tooltip("Спецэффект воспроизводимый для применяющего")]
    public GameObject casterSFX;

    [Tooltip("Набор звуков применения")]
    public List<AudioClip> castSounds;
    #endregion

    [HideInInspector]
    public Structure targetStructure = null;
    [HideInInspector]
    public Unit targetUnit = null;
    [HideInInspector]
    public bool isActivated = false;

    public void ActivateCast()
    {
        //Debug.Log("Каст скилла инициализирован");
        isActivated = true;
        if (delay == 0)
            StartCast();
        else
            StartChannelling();
    }

    public void ActivateCast(Structure target)
    {
        targetStructure = target;
        ActivateCast();
    }

    public void ActivateCast(Unit target)
    {
        targetUnit = target;
        ActivateCast();
    }

    public void StartCast()
    {
        caster.PlayAnimation(animationTag);
        coolDownTimer = coolDown;
    }

    public void StartChannelling()
    {
        caster.PlayAnimation("channelling");
        StartCoroutine(ChannellingRoutine());
    }

    public IEnumerator ChannellingRoutine()
    {
        yield return new WaitForSeconds(delay);
        StartCast();
    }

    public virtual void Cast() { }

    public CastError CheckCast(Unit target)
    {
        if (coolDownTimer > 0)
            return CastError.coolDown;

        #region Проверки подходящих целей
        if (target == caster && !targetPermissions.allowSelf)
            return CastError.permitionSelf;

        if (target.team == caster.team && !targetPermissions.allowAllies)
            return CastError.perimitionAllies;

        if (target.team != caster.team && !targetPermissions.allowEnemies)
            return CastError.perimitionEnemies;

        if (!target.isAlive && !targetPermissions.allowDead)
            return CastError.permitionDead;
        #endregion

        #region Проверки расстояния
        float sqrDistance = Unit.GetSqrFlatDistance(target, caster);
        switch (range)
        {
            case InteractionRange.ranged:
                if (sqrDistance > distanceMax * distanceMax)
                    return CastError.tooFar;
                if (sqrDistance < distanceMin * distanceMin)
                    return CastError.tooNear;
                break;
            case InteractionRange.melee:
                if (sqrDistance > caster.interactionRange * caster.interactionRange)
                    return CastError.tooFar;
                break;
            default:
                break;
        }
        #endregion

        #region Проверки ресурсов заклинателя
        if (caster.mana < manaCost)
            return CastError.notEnoughMana;
        #endregion

        return CastError.success;
    }
}

public enum InteractionTargeting { none, unit, point}

public enum InteractionRange { melee, ranged, unlimited}

public enum CastError { success, coolDown, tooFar, tooNear,
    notEnoughMana, wrongTarget, permitionSelf,
    perimitionEnemies, perimitionAllies,permitionBuilding,
    perimitionTerrain, permitionDead }

[System.Serializable]
public class TargetPermission
{
    [Tooltip("Сам юнит")]
    public bool allowSelf;
    [Tooltip("Вражеские юниты")]
    public bool allowEnemies;
    [Tooltip("Союзные юниты")]
    public bool allowAllies;
    [Tooltip("Здания")]
    public bool allowBuildings;
    [Tooltip("Блоки террейна")]
    public bool allowTerrain;
    [Tooltip("Мертвые")]
    public bool allowDead;
}