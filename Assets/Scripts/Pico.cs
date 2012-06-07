using System;
using UnityEngine;

public static class Pico
{
    const float LevelTransitionDuration = 1.75f;

    public static event Action LevelChanged;

    public static float HeartbeatDuration = 1;

    public static GameObject EmitterTemplate;
    public static GameObject ProjectorTemplate;
    public static GameObject AccumulatorTemplate;
    public static GameObject ReceiverTemplate;
    public static GameObject EnergyTemplate;

    public static float GridHeight;
    public static float TimeLeftToHeartbeat;
    public static float BeatsLeftToGeneration;

    public static bool Heartbeat, PostHeartbeat;
    public static bool Generation;

    public static Level Level { get; set; }
    public static Func<Level> CurrentLevel { get; set; }

    static Func<Level> NextLevel;

    static float? SinceLevelTransition;

    public static void RebootLevel()
    {
        if (Level != null)
            Level.Destroy();

        Level = CurrentLevel();

        TimeLeftToHeartbeat = HeartbeatDuration;
        BeatsLeftToGeneration = 1;

        if (LevelChanged != null)
            LevelChanged();
    }
    
    public static void CycleLevels(Func<Level> nextLevel)
    {
        NextLevel = nextLevel;

        if (Level != null)
        {
            SinceLevelTransition = -0.5f;
            Level.FallDown();
        }
        else
            ChangeLevel();
    }

    static void ChangeLevel()
    {
        if (Level != null)
            Level.Destroy();

        CurrentLevel = NextLevel;
        Level = NextLevel();

        BeatsLeftToGeneration = 2;

        if (LevelChanged != null)
            LevelChanged();
    }

    public static void Update()
    {
        if (IsChangingLevel)
        {
            SinceLevelTransition += Time.deltaTime;

            if (SinceLevelTransition >= LevelTransitionDuration)
            {
                ChangeLevel();
                SinceLevelTransition = null;
            }
        }

        Level.SinceAlive += Time.deltaTime * 1.5f;
        TimeLeftToHeartbeat -= Time.deltaTime;

        PostHeartbeat = false;

        if (Heartbeat)
            PostHeartbeat = true;

        if (TimeLeftToHeartbeat < 0)
        {
            //Debug.Log("Heartbeat!");

            if (!IsChangingLevel)
                Heartbeat = true;

            BeatsLeftToGeneration--;
            TimeLeftToHeartbeat += HeartbeatDuration;

            if (BeatsLeftToGeneration == 0)
            {
                BeatsLeftToGeneration = 4;
                if (!IsChangingLevel)
                    Generation = true;
            }
            else
                Generation = false;
        }
        else
        {
            Heartbeat = false;
            Generation = false;
        }
    }

    public static bool IsChangingLevel
    {
        get { return SinceLevelTransition.HasValue; }
    }
    public static float FallDistance
    {
        get { return Easing.EaseIn(Math.Max(0, SinceLevelTransition.Value), EasingType.Cubic); }
    }
}

