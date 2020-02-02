using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericMenu : MonoBehaviour
{
    public void DisableRaycasting()
    {
        // Пуск лучей отключен
        Player.mouseOverUI = true;
    }

    public void EnableRaycasting()
    {
        // Пуск лучей включен

        Player.mouseOverUI = false;
    }

    public void PauseMenu()
    {
        Player.mouseOverUI = true;
        UIManager.SwitchMenu(Menues.pause);
    }
    public void AlchemyMenu()
    {
        Player.mouseOverUI = true;
        UIManager.SwitchMenu(Menues.alchemy);
    }
}
