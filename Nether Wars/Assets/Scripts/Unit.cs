using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class Unit : MonoBehaviour
{
    [Tooltip("Ссылка на оригинальный префаб (для настроек по умолчанию")]
    public Unit unitType;

    #region Общие параметры
    [Header("Общие параметры")]
    [Tooltip("Собственное имя этого существа")]
    public string unitName = "Bob";

    [Range(1,6)][Tooltip("Уровень существа")]
    public int level = 1;

    [Range(1,50)][Tooltip("Максимальный запас здоровья")]
    public int healthMax = 10;
    [HideInInspector]
    public float health;
    [HideInInspector]
    public bool isAlive = true;

    [Range(1, 50)]
    [Tooltip("Максимальный запас маны")]
    public int manaMax = 0;
    [HideInInspector]
    public float mana;

    [Tooltip("Команда, к которой принадлежит этот юнит")]
    public Team team = Team.player;
    #endregion

    #region Параметры движения
    [Header("Параметры движения")]
    [Tooltip("Скорость поворота (градусов в сек.")][Range(6, 360)]
    public int rotationSpeed = 90;

    [Tooltip("Скорость движения")][Range(0.5f, 5)]
    public float moveSpeed = 2;

    #endregion

    #region Параметры сражения
    [Space]
    [Header("Параметры сражения")]

    [Tooltip("Базовый урон от оружия")][Range(1, 10)]
    public int basicDamage = 2;

    [Tooltip("Базовый тип атаки")]
    public AttackTypes attackType;

    [Tooltip("Базовый тип урона")]
    public Elements damageType;

    [Range(0.1f, 5f)][Tooltip("Задержка между атаками (является интерфейсом для скилла 'Атака (ближний/дальний бой)'")]
    public float attackCoolDown = 2;
    [HideInInspector]
    public float attackTimer = 0;
    [HideInInspector]
    public Skill activeSkill = null;

    [Range(0.5f, 5f)][Tooltip("Дальность взаимодействия в ближнем бою")]
    public float interactionRange = 1;

    [Tooltip("Защита")][Range(1, 10)]
    public int armor = 2;

    [Tooltip("Тип защиты")]
    public Elements armorType;

    [Tooltip("Эффект нанесения урона")]
    public ParticleSystem hitEffect;

    [Tooltip("Набор звуков нанесения урона")]
    public List<AudioClip> hitSounds;

    [Tooltip("Звук, который юнит издает умирая")]
    public AudioClip deathSound;

    [Tooltip("Звук, который юнит издает при блокировании урона")]
    public AudioClip blockSound;

    #endregion

    #region Прочие параметры
    [HideInInspector]
    public List<Perk> perks;
    #endregion

    #region Ссылки на необходимые компоненты
    [Space]
    [Header("Ссылки на компоненты")]
    [Tooltip("Ссылка на компонент, управляющий анимацией модели")]
    public Animator animator;
    [Tooltip("Ссылка на компонент, управляющий событиями анимации")]
    public UnitAnimationHandler animationHandler;
    [Tooltip("Ссылка на компонент, управляющий движение юнита")]
    public NavMeshAgent navigationAgent;
    [Tooltip("Ссылка на компонент, позволяющий юниту воспроизводить звуки")]
    public AudioSource audioSource;
    public new Collider collider;
    public Rigidbody body;
    public new SkinnedMeshRenderer renderer;
    public UnitSkillManager skillManager;
    #endregion

    #region Технические переменные
    IEnumerator turnRoutine;
    [HideInInspector] public bool isOnMove = false;
    public Vector3 position { get => transform.position; }

    private Vector3 MovePosition;
    public Vector3 movePosition
    {
        get => MovePosition;
        set
        {
            navigationAgent.destination = value;
            MovePosition = navigationAgent.destination;
        }
    }

    #endregion

    void Start()
    {
        InitUnit();
    }

    void InitUnit()
    {
        if (!CheckConsistence())
            Debug.Break();

        unitsInGame.Add(this);

        name = unitName;
        navigationAgent.speed = moveSpeed;
        navigationAgent.angularSpeed = rotationSpeed * 2;
        health = healthMax;
        mana = manaMax;
    }

    void FixedUpdate()
    {
        UpdateUnit();
    }

    void UpdateUnit()
    {
        if (isAlive)
        {
            if (attackTimer > 0)
                attackTimer -= Time.fixedDeltaTime;

            animator.SetBool("isMoving", isOnMove);
        }
    }

    void OnDestroy()
    {
        unitsInGame.Remove(this);
    }

    #region Методы, реализующие нанесение и получение урона

    public void ActivateSkill(Skill skill)
    {
        if (activeSkill != null)
        {
            Debug.Log("Юнит занят применением другой способности");
            return;
        }
        activeSkill = skill;
        skill.ActivateCast();
    }
    public void ActivateSkill(Skill skill, Unit target)
    {
        if (target == null)
            Debug.Log("Пустая ссылка");
        if (activeSkill != null)
        {
            //Debug.Log("Юнит занят применением другой способности");
            return;
        }
        activeSkill = skill;
        skill.ActivateCast(target);
    }
    public void ActivateSkill(Skill skill, Structure target)
    {
        if (activeSkill != null)
        {
            Debug.Log("Юнит занят применением другой способности");
            return;
        }
        activeSkill = skill;
        skill.ActivateCast(target);
    }

    public void StartCast()
    { }
    public void Cast()
    {
        if (activeSkill != null && isAlive)
            activeSkill.Cast();
    }
    public void EndCast()
    {
        if (activeSkill)
            activeSkill = null;
    }

    public float MeleeAttack (Unit target)
    {
        float realDamage = Damage(this,target,basicDamage,damageType,attackType);
        if (realDamage > 0)
        {
            int rand = UnityEngine.Random.Range(0, hitSounds.Count);
            PlaySound(hitSounds[rand]);
            ParticleSystem effect = Instantiate(hitEffect, GetAttackPoint(), transform.rotation);
            Destroy(effect.gameObject, 1);
        }
        ApplyDamageEffects(target);
        return realDamage;
    }

    public float TakeDamage(float damage, Elements damageType, AttackTypes attackType)
    {
        // Урон масштабируется в зависимости от сочетания типа урона и типа защиты цели
        damage *= DamageElementFactor(damageType, armorType);
        // Из урона вычитается броня
        damage -= armor;
        if (damage <= 0)
            return 0;
        else
            health -= damage;

        if (health <= 0)
            Die();
        return damage;
    }

    public void Die()
    {
        isAlive = false;
        activeSkill = null;
        Stop();
        navigationAgent.enabled = false;

        PlaySound(deathSound);
        PlayAnimation("die");

        animationHandler.ReleaseBodyParts();

        Destroy(gameObject, 3f);
    }
    #endregion

    #region Поворот, анимация и проигрывание звуков

    public void PlayAnimation(string tag)
    {
        animator.SetTrigger(tag);
    }

    public void PlayAnimation(string tag, float speed)
    {
        animator.speed = speed;
        PlayAnimation(tag);
    }

    public void PlaySound(AudioClip sound)
    {
        if (sound)
            audioSource.PlayOneShot(sound);
    }

    public void Face(Vector3 direction, bool instant)
    {
        if (turnRoutine != null)
            StopCoroutine(turnRoutine);
        if (instant)
            RotateInstant(direction);
        else
        {
            turnRoutine = RotateRoutine(direction);
            StartCoroutine(turnRoutine);
        }
    }

    public float GetFacingAngle()
    {
        return transform.eulerAngles.y;
    }

    public void RotateInstant (Vector3 direction)
    {
        Vector3 euler = new Vector3(0, AngleToPoint(direction), 0);
        transform.eulerAngles = euler;
    }

    public void RotateFrame(Vector3 facingPoint)
    {
        float alpha = AngleToPoint(facingPoint);
        int direction = NearestRotation(facingPoint);
        Vector3 newEuler = new Vector3(0, alpha, 0);

        transform.eulerAngles += direction * new Vector3(0, rotationSpeed * Time.deltaTime, 0);
    }

    IEnumerator RotateRoutine(Vector3 facingPoint)
    {
        float alpha = AngleToPoint(facingPoint);
        int direction = NearestRotation(facingPoint);
        Vector3 newEuler = new Vector3(0, alpha, 0);

        while (Mathf.Abs(transform.eulerAngles.y - newEuler.y) > 3)
        {
            transform.eulerAngles += direction * new Vector3(0, moveSpeed * 360 * Time.deltaTime, 0);
            yield return null;
        }
    }

    public void Stop()
    {
        if (turnRoutine != null)
            StopCoroutine(turnRoutine);
        movePosition = transform.position;
        navigationAgent.velocity = Vector3.zero;
        isOnMove = false;
    }

    public void SetMovePoint (Vector3 toPoint)
    {
        navigationAgent.destination = toPoint;
        navigationAgent.stoppingDistance = 0.1f;
        isOnMove = true;
    }

    bool CheckConsistence()
    {
        if (animator == null)
        {
            Debug.Log("Не хватает аниматора у " + name);
            return false;
        }
        if (animationHandler == null)
        { 
            Debug.Log("Не хватает обработчика событий анимации у " + name);
            return false;
        }
        if (navigationAgent == null)
        { 
            Debug.Log("Не хватает агента перемещения у " + name);
            return false;
        }
        if (audioSource == null)
        { 
            Debug.Log("Не хватает источника звука у " + name);
            return false;
        }
        if (collider == null)
        { 
            Debug.Log("Не хватает коллайдера у " + name);
            return false;
        }
        if (collider == null)
        { 
            Debug.Log("Не хватает Rigidbody у " + name);
            return false;
        }
        if (renderer == null)
        {
            Debug.Log("Не хватает MeshRenderer'а у " + name);
            return false;
        }
        return true;
    }

    #endregion

    #region Вспомогательный мат. аппарата

    public Vector3 GetAttackPoint()
    {
        float h = 0.5f;
        if (navigationAgent)
            h = navigationAgent.height / 2;
        return transform.position
            + transform.forward * interactionRange / 2
            + Vector3.up * h;
    }

    /// <summary>Указывает направление вращения для кратчайшего поворота юнита лицом
    /// к указанной точке; 1 - По часовой стрелке; -1 - против часовой стрелки</summary>
    public int NearestRotation(Vector3 toPoint)
    {
        float facing = GetFacingAngle();
        float alpha = AngleToPoint(toPoint);

        int direction = 0;
        float delta = alpha - facing;
        if (Mathf.Abs(delta) >= 180)
        {
            if (delta >= 0)
                direction = -1;
            else
                direction = 1;
        }
        else
        {
            if (delta >= 0)
                direction = 1;
            else
                direction = -1;
        }
        // if (direction > 0)
            // Debug.Log("По часовой стрелке");
        // else
           // Debug.Log("Против часовой стрелки");

        return direction;
    }

    /// <summary> Определяет необходимый мировой угол поворота юнита,
    /// чтобы он встал лицом к точке (от 0 до 360 градусов) </summary>
    public float AngleToPoint(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.Normalize();

        Vector3 defaultForward = Vector3.forward + transform.position - transform.position;
        float alpha = AngleBetweenVectors(defaultForward, direction) * 180 / Mathf.PI;
        if (point.x > transform.position.x)
            return alpha;
        else
            return 360-alpha;
    }
    #endregion

    #region Перки юнита

    public void AddPerk(Perk perk)
    {
        if (perks.Contains(perk))
            return;
        perks.Add(perk);
        perk.EffectOnAdd(this);
    }

    public void RemovePerk(Perk perk)
    {
        if (!perks.Contains(perk))
            return;
        perks.Remove(perk);
        perk.EffectOnRemove(this);
    }

    public void ApplyDamageEffects(Unit target)
    {
        foreach (var perk in perks)
            perk.EffectOnAttack(this, target);       
    }

    #endregion

    #region Модификация параметров

    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        navigationAgent.speed = moveSpeed;
    }

    public void SetMoveSpeedDefault()
    {
        SetMoveSpeed(unitType.moveSpeed);
    }

    #endregion

    #region Модификаторы урона

    /// <summary> Cоотношения типа урона и типа защиты </summary>
    public static float DamageElementFactor(Elements damageType, Elements armorType)
    {
        float incFactor = 1.5f;
        // Кости <> Мрак
        if (damageType == Elements.bone && armorType == Elements.shadow)
            return incFactor;
        if (damageType == Elements.shadow && armorType == Elements.bone)
            return incFactor;
        // Слизь <> Пепел
        if (damageType == Elements.slime && armorType == Elements.cinder)
            return incFactor;
        if (damageType == Elements.cinder && armorType == Elements.slime)
            return incFactor;
        // В остальных случаях без бонусов
        return 1;
    }

    /// <summary> Возвращает множитель урона от суммы перков, модифицирующих урон </summary>
    public float GetDamageFactor()
    {
        float factor = 1;
        foreach (var perk in perks)
        {
            float perkFactor = perk.GetDamageFactor(damageType, attackType);
            if (perk.sumsUp)
                factor += perkFactor - 1;
            else if (perkFactor > factor)
                factor = perkFactor;
        }
        return factor;
    }

    /// <summary> Возвращает слагаемое урона от суммы перков, модифицирующих урон </summary>
    public float GetDamageSummand()
    {
        float summand = 0;
        foreach (var perk in perks)
        {
            float perkSummand = perk.GetDamageSummand(damageType, attackType);
            if (perk.sumsUp)
                summand += perkSummand;
            else if (perkSummand > summand)
                summand = perkSummand;
        }
        return summand;
    }

    #endregion

    // Дополнительный интерфейс в режиме сцены
    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            Vector3 point = GetAttackPoint();

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.02f);

            Gizmos.DrawWireSphere(point, interactionRange / 2);
            Color transparent = Color.red;
            transparent.a = 0.3f;
            Gizmos.color = transparent;
            Gizmos.DrawSphere(point, interactionRange / 2);

            if (navigationAgent && isOnMove)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, navigationAgent.destination);
                Gizmos.DrawSphere(navigationAgent.destination, 0.1f);
                Gizmos.DrawWireSphere(navigationAgent.destination, 0.1f);
            }
        }
    }
}

public enum AttackTypes { melee, ranged, magic }

public enum Team { player, enemy }

