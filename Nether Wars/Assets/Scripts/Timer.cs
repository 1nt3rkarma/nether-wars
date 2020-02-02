using System.Collections;
using UnityEngine;

public class Timer: MonoBehaviour
{
    public float period = 1;
    public int tickLimit = 0;
    public int ticks = 0;
    public bool paused = true;
    public bool tickEvent = false;
    public bool expired = false;

    public void Init (float period, bool expires)
    {
        if (expires)
            Init(period, 1);
        else
            Init(period, 0);
    }

    public void Init(float period)
    {
        Init(period, 1);
    }

    public void Init(float period, int tickLimit)
    {
        this.period = period;
        this.tickLimit = tickLimit;
        paused = false;
    }

    void Update()
    {
        if (!paused)
            if (!expired)
                if (tickEvent)
                    if (ticks < tickLimit || tickLimit == 0)
                        StartCoroutine(WaitTick());
                    else
                        expired = true;
                else
                    return;
            else
                tickEvent = false;

        if (expired) Debug.Log("Тамейр отсчитал: " + ticks + " тиков");
    }

    IEnumerator WaitTick()
    {
        yield return null;
        tickEvent = false;
        yield return new WaitForSeconds(period);
        tickEvent = true;
        ticks++;
        Debug.Log("Таймер тикнул");
    }
}
