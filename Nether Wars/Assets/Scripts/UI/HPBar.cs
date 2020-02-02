using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    public Unit unit;

    public GameObject back;
    public GameObject fill;

    bool isRendered = true;

    void Update()
    {
        if (unit.isAlive && Player.renderHealthBars)
        {
            if (isRendered)
            {
                Vector3 scale = fill.transform.localScale;
                scale.x = unit.health / (float)unit.healthMax;
                fill.transform.localScale = scale;
                Vector3 euler = UIManager.activeCamera.transform.eulerAngles;
                //euler.y = -euler.y;
                transform.eulerAngles = euler;
            }
            else
            {
                isRendered = true;
                back.SetActive(true);
                fill.SetActive(true);
            }
        }
        else
        {
            if (isRendered)
            {
                isRendered = false;
                back.SetActive(false);
                fill.SetActive(false);
            }
        }
    }
}
