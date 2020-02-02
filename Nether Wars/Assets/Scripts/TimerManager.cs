using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public Timer timerPrefab;
    public static List<Timer> timers;

    public static TimerManager local;

    void Awake()
    {
        timers = new List<Timer>();
        local = this;
    }

    public static void DestroyTimer(Timer timer)
    {
        timers.Remove(timer);
        Destroy(timer.gameObject);
    }

    public static Timer CreateTimer(float period)
    {
        return CreateTimer(period,1);
    }

    public static Timer CreateTimer(float period, int tickLimit)
    {
        Timer newTimer = Instantiate(local.timerPrefab, local.transform);
        newTimer.Init(period, tickLimit);
        timers.Add(newTimer);
        return newTimer;
    }

    public static Timer CreateTimer(float period, bool expires)
    {
        if (expires)
            return CreateTimer(period, 1);
        else
            return CreateTimer(period, 0);
    }

    public static void PauseTimer(Timer timer)
    {
        timer.paused = true;
    }

    public static void ResumeTimer(Timer timer)
    {
        timer.paused = false;
    }
}
