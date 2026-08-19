using System;
using UnityEngine;
using static Assets.Scripts.Enums;

[CreateAssetMenu(fileName = "SO_GameSettings")]
public class SO_GameSettings : ScriptableObject
{
    [Header("HOUSE COUNTERS")]
    [Space(8)]
    [SerializeField]
    public int DefecatingTotalCounter = 15;
    [SerializeField]
    public int EatingTotalCounter = 15;
    [SerializeField]
    public int WorkingTotalCounter = 10;
    [SerializeField]
    public int SleepingTotalCounter = 20;

    [Header("CLUB COUNTERS")]
    [Space(8)]
    [SerializeField]
    public int VomitingTotalCounter = 20;
    [SerializeField]
    public int DrinkingTotalCounter = 20;
    [SerializeField]
    public int KissingTotalCounter = 20;
    [SerializeField]
    public int SmokingTotalCounter = 20;

    [Header("UI")]
    [Space(8)]
    public float alphaFadeInOutIncrementalStep = 0.1f;
    public float timeFadeInOutIncrementalStep = 0.1f;

    [Header("MISSCELLANEOUS")]
    [Space(8)]
    [SerializeField]
    public int totalTaskCounterRequired = 5;

    [SerializeField]
    public bool isTesting = false;

    [SerializeField]
    public int stageDurationInSeconds = 45;

    [SerializeField]
    public int hideEndingLimit = 21;

    [SerializeField]
    public int transitionBetweenStagesWaitingTimeInSeconds = 2;

    [SerializeField]
    public int ReadingTime = 3;

    [SerializeField]
    public int taskCooldownTime = 1;

    public int Selector(Counter counter)
    {
        switch (counter)
        {
            case Counter.Defecating:
                return DefecatingTotalCounter;
            case Counter.Eating:
                return EatingTotalCounter;
            case Counter.Working:
                return WorkingTotalCounter;
            case Counter.Sleeping:
                return SleepingTotalCounter;
            case Counter.Vomiting:
                return VomitingTotalCounter;
            case Counter.Drinking:
                return DrinkingTotalCounter;
            case Counter.Kissing:
                return KissingTotalCounter;
            case Counter.Smoking:
                return SmokingTotalCounter;
            default:
                throw new Exception("Bad Counters Enum value");
        }
    }
}
