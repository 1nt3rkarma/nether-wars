using System.Collections.Generic;
using UnityEngine;

public class UIReceiptTable : MonoBehaviour
{
    public ButtonAlchemy buttonBlood;
    public ButtonAlchemy buttonBone;
    public ButtonAlchemy buttonCinder;
    public ButtonAlchemy buttonSlime;
    public ButtonAlchemy buttonShadow;

    static UIReceiptTable local;

    public static List<ButtonAlchemy> elementsButtons;

    const int elemButtonSize = 100;

    void Awake()
    {
        local = this;
        elementsButtons = new List<ButtonAlchemy>();

    }

    public static void ClearTable()
    {
        foreach (var button in elementsButtons)
            Destroy(button.gameObject);
        elementsButtons.Clear();
    }

    public static void UpdateTable()
    {
        ClearTable();

        int sum = Alchemy.receipt.GetSum();
        if (sum == 0)
            return;

        int index = 0;
        int startX;
        if (sum % 2 == 0)
            startX = (sum / 2 - 1) * (-elemButtonSize) + (-elemButtonSize/2);
        else
            startX = sum / 2 * (-elemButtonSize);

        if (Alchemy.receipt.basic != null)
        {
            ButtonAlchemy buttonPrefab = GetButtonPrefab(Alchemy.receipt.basic.element);
            int basicProportion = Alchemy.receipt.basic.portion;
            for (int i = 0; i < basicProportion; i++)
            {
                ButtonAlchemy newButton = Instantiate(buttonPrefab, local.transform);
                newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + elemButtonSize * index, 0);
                elementsButtons.Add(newButton);
                index++;
            }
        }
        if (Alchemy.receipt.second != null)
        {
            ButtonAlchemy buttonPrefab = GetButtonPrefab(Alchemy.receipt.second.element);
            int secondProportion = Alchemy.receipt.second.portion;
            for (int i = 0; i < secondProportion; i++)
            {
                ButtonAlchemy newButton = Instantiate(buttonPrefab, local.transform);
                newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + elemButtonSize * index, 0);
                elementsButtons.Add(newButton);
                index++;
            }
        }
        if (Alchemy.receipt.third != null)
        {
            ButtonAlchemy buttonPrefab = GetButtonPrefab(Alchemy.receipt.third.element);
            int thirdProportion = Alchemy.receipt.third.portion;
            for (int i = 0; i < thirdProportion; i++)
            {
                ButtonAlchemy newButton = Instantiate(buttonPrefab, local.transform);
                newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + elemButtonSize * index, 0);
                elementsButtons.Add(newButton);
                index++;
            }
        }
    }

    public static ButtonAlchemy GetButtonPrefab (Elements forElement)
    {
        switch (forElement)
        {
            case Elements.blood:
                return local.buttonBlood;
            case Elements.bone:
                return local.buttonBone;
            case Elements.cinder:
                return local.buttonCinder;
            case Elements.slime:
                return local.buttonSlime;
            case Elements.shadow:
                return local.buttonShadow;
        }
        return null;
    }
}
