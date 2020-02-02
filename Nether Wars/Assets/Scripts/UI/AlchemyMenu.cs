using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyMenu : MonoBehaviour
{
    public void Back()
    {
        Alchemy.ClearReceipt();
        UIManager.SwitchMenu(Menues.generic);
    }

    public void Confirm()
    {
        Alchemy.ConfirmReceipt();
    }
}
