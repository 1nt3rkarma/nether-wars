using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        UIManager.SwitchMenu(Menues.generic);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void SaveTerrain()
    {
        SaveSystem.SaveTerrain(TerrainSystem.local);
    }
}
