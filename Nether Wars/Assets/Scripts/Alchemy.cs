using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alchemy : MonoBehaviour
{
    public const int elementsCount = 5;

    public static Receipt receipt;
    public static Alchemy local;

    public GameResources resourceFile;

    public List<AudioClip> cauldronSounds;
    public Animator cauldronAnimator;

    public Animator animator;

    void Awake()
    {
        local = this;
        receipt = new Receipt();
    }

    void Update()
    {

    }

    public static void ConfirmReceipt()
    {
        int sum = receipt.GetSum();
        if (sum == 0)
            return;

        ErrCodesAlchemy check = CheckReceiptFinal();
        if (check == ErrCodesAlchemy.success)
        {
            GetPerksFromReciept(out Perk perk1, out Perk perk2);
            Unit unitType = GetUnitFromReceipt();

            if (perk2 == null)
                UIManager.ShowCreatureName(unitType.unitName, perk1);
            else
                UIManager.ShowCreatureName(unitType.unitName, perk1, perk2);

            Player.SpawnUnit(unitType, perk1, perk2);

            AnimateCauldron();

            ClearReceipt();
            UIReceiptTable.UpdateTable();

        }
        else
            UIManager.ShowErrorMessage(check);
        
    }

    public static Unit GetUnitFromReceipt()
    {
        Unit unit = GetUnitFromReceipt(receipt);
        return unit;
    }
    public static Unit GetUnitFromReceipt(Receipt receipt)
    {
        Unit unit = null;
        int lvl = receipt.GetSum() - 2;
        if (lvl <= 0)
        {
            Debug.Log("Что-то пошло не так...");
            return unit;
        }
        //Debug.Log("Уровень существа: " + lvl);
        switch (receipt.basic.element)
        {
            case Elements.blood:
                //Debug.Log("Основа существа: Кровь");
                if (lvl < 6)
                    unit = local.resourceFile.BloodBound.tiers[lvl - 1].generic;
                else
                    unit = local.resourceFile.BloodBound.general;
                break;
            case Elements.bone:
                //Debug.Log("Основа существа: Кости");
                if (lvl < 6)
                    unit = local.resourceFile.BoneGnashers.tiers[lvl - 1].generic;
                else
                    unit = local.resourceFile.BoneGnashers.general;
                break;
            case Elements.cinder:
                //Debug.Log("Основа существа: Пепел");
                if (lvl < 6)
                    unit = local.resourceFile.AshRisen.tiers[lvl - 1].generic;
                else
                    unit = local.resourceFile.AshRisen.general;
                break;
            case Elements.slime:
                //Debug.Log("Основа существа: Слизь");
                if (lvl < 6)
                    unit = local.resourceFile.Vilespawn.tiers[lvl - 1].generic;
                else
                    unit = local.resourceFile.Vilespawn.general;
                break;
            case Elements.shadow:
                //Debug.Log("Основа существа: Мрак");
                if (lvl < 6)
                    unit = local.resourceFile.Revenants.tiers[lvl - 1].generic;
                else
                    unit = local.resourceFile.Revenants.general;
                break;
        }
        return unit;
    }

    public static void GetPerksFromReciept(out Perk perk1, out Perk perk2)
    {
        GetPerksFromReciept(receipt, out perk1, out perk2);
    }
    public static void GetPerksFromReciept(Receipt receipt, out Perk perk1, out Perk perk2)
    {
        perk1 = null;
        perk2 = null;

        List<Perk> perk1pull = new List<Perk>();
        List<Perk> perk2pull = new List<Perk>();
        foreach (var perkInfo in local.resourceFile.alchemy.perksCatalogue)
        {
            if (receipt.second != null)
                if (perkInfo.element == receipt.second.element && perkInfo.level == receipt.second.portion)
                    perk1pull.Add(perkInfo);
            if (receipt.third != null)
                if (perkInfo.element == receipt.third.element && perkInfo.level == receipt.third.portion)
                    perk2pull.Add(perkInfo);
        }

        int c1 = perk1pull.Count;
        if (c1 > 0)
            perk1 = perk1pull[Random.Range(0, perk1pull.Count)];
        
        int c2 = perk2pull.Count;
        if (c2 > 0)
            perk2 = perk2pull[Random.Range(0, perk1pull.Count)];
    }

    public static void AddElement(Elements element)
    {
        UIManager.ClearCreatureName();
        ErrCodesAlchemy check = receipt.AddElement(element);
        if (check != ErrCodesAlchemy.success)
        {
            //UIManager.LogErrorMessage(check);
            UIManager.ShowErrorMessage(check);
            return;
        }
        UIReceiptTable.UpdateTable();
    }

    public static void RemoveElement(Elements element)
    {
        receipt.RemoveElement(element);
        UIReceiptTable.UpdateTable();
    }

    public static ErrCodesAlchemy CheckReceiptFinal()
    {
        return CheckReceiptFinal(receipt);
    }
    public static ErrCodesAlchemy CheckReceiptFinal(Receipt receipt)
    {
        #region Проверка лимита Игрока
        int lvl = receipt.GetSum() - 2;
        if (Player.limitOccupied + lvl > Player.limitMax)
            return ErrCodesAlchemy.notEnoughLimit;
        #endregion

        #region 1 правило Алхимии: Неутолимость
        // одного элемента не может быть достаточно
        int count = receipt.ingredients.Count;
        if (count < 2)
            return ErrCodesAlchemy.singleElement;
        #endregion

        #region 2 правило Алхимии: Дисбаланс
        // два элемента не могут находить в равных пропорциях
        for (int i = 0; i < receipt.ingredients.Count; i++)        
            for (int j = 0; j < receipt.ingredients.Count; j++)            
                if (i != j && receipt.ingredients[i].portion == receipt.ingredients[j].portion)
                    return ErrCodesAlchemy.wrongProportion;
        #endregion

        return ErrCodesAlchemy.success;
    }

    public static void ClearReceipt()
    {
        receipt.ingredients.Clear();
        receipt.Clear();
        UIReceiptTable.ClearTable();
    }

    static void AnimateCauldron()
    {
        local.cauldronAnimator.SetTrigger("animate");
        int rand = Random.Range(0, local.cauldronSounds.Count);
        UIManager.PlaySound(local.cauldronSounds[rand]);
    }

}

public class Ingredient
{
    public Elements element;
    public int portion;

    public Ingredient(Elements element)
    {
        this.element = element;
        this.portion = 1;
    }

    public Ingredient (Elements element, int proportion)
    {
        this.element = element;
        this.portion = proportion;
    }
}

public class Receipt
{
    public List<Ingredient> ingredients;
    public Ingredient basic;
    public Ingredient second;
    public Ingredient third;

    public Receipt()
    {
        ingredients = new List<Ingredient>();
        basic = null;
        second = null;
        third = null;
    }

    public ErrCodesAlchemy AddElement(Elements element)
    {
        #region 3 правило Алхимии: Антипатия
        // Желчь не смешивается с Пеплом, Кости не смешиваются с Мраком;
        // только Кровь может смешиваться с любым другим элементом.

        foreach (var ingredient in ingredients)
        {
            if ((ingredient.element == Elements.bone && element == Elements.shadow) ||
                (ingredient.element == Elements.shadow && element == Elements.bone))
                return ErrCodesAlchemy.BoneContrShadow;
            else if ((ingredient.element == Elements.cinder && element == Elements.slime) ||
                (ingredient.element == Elements.slime && element == Elements.cinder))
                return ErrCodesAlchemy.SlimeContrCinder;
        }

        #endregion

        #region Проверка уровня Игрока
        int sum = GetSum();
        if (sum >= Player.level + 2)
            return ErrCodesAlchemy.notEnoughLevel;
        #endregion

        Ingredient added = GetIngredient(element);
        if (added != null)
            added.portion++;
        else
            ingredients.Add(new Ingredient(element));
        
        UpdateIngredients();
        UIReceiptTable.UpdateTable();

        return ErrCodesAlchemy.success;
    }

    public void RemoveElement(Elements element)
    {
        Ingredient removed = GetIngredient(element);
        if (removed != null)
        {
            removed.portion--;
            if (removed.portion == 0)
                ingredients.Remove(removed);
        }
        UIReceiptTable.UpdateTable();
    }

    void UpdateIngredients()
    {
        basic = SearchBasic();
        second = SearchSecond();
        third = SearchThird();
    }

    Ingredient SearchBasic()
    {
        int maxProportion = 0;
        Ingredient basic = null;
        foreach (var ingredient in ingredients)
            if (ingredient.portion > maxProportion)
            {
                maxProportion = ingredient.portion;
                basic = ingredient;
            }
        return basic;
    }

    Ingredient SearchSecond()
    {
        int maxProportion = 0;
        Ingredient second = null;
        foreach (var ingredient in ingredients)
            if (ingredient != basic)
                if (ingredient.portion > maxProportion)
                {
                    maxProportion = ingredient.portion;
                    second = ingredient;
                }                      
        return second;
    }

    Ingredient SearchThird()
    {
        int minProportion = 10;
        Ingredient third = null;
        foreach (var ingredient in ingredients)
            if (ingredient != basic && ingredient != second)
                if (ingredient.portion < minProportion)
                {
                    minProportion = ingredient.portion;
                    third = ingredient;
                }
        return third;
    }

    public Ingredient GetIngredient (Elements byElement)
    {
        foreach (var ingredient in ingredients)
            if (ingredient.element == byElement)
                return ingredient;
        return null;
    }

    public int GetSum()
    {
        int sum = 0;
        foreach (var ingredient in ingredients)
            sum += ingredient.portion;
        return sum;
    }

    public void Clear()
    {
        ingredients.Clear();
        basic = null;
        second = null;
        third = null;
    }
}

public enum Elements { blood, bone, cinder, slime, shadow }

public enum ErrCodesAlchemy { success, notEnoughSupply, notEnoughLevel, notEnoughMana, notEnoughLimit, wrongProportion, SlimeContrCinder, BoneContrShadow, singleElement, empty}