using System;
using UnityEngine;
using static Assets.Scripts.Enums;

[CreateAssetMenu(fileName = "SO_GameSettings")]
public class SO_GameSettings : ScriptableObject
{
    [Header("HOUSE COUNTERS")]
    [Space(8)]
    [SerializeField]
    public int CagarTotalCounter = 15;
    [SerializeField]
    public int ComerTotalCounter = 15;
    [SerializeField]
    public int TrabajarTotalCounter = 10;
    [SerializeField]
    public int DormirTotalCounter = 20;

    [Header("CLUB COUNTERS")]
    [Space(8)]
    [SerializeField]
    public int VomitarTotalCounter = 20;
    [SerializeField]
    public int EscabiarTotalCounter = 20;
    [SerializeField]
    public int BesarTotalCounter = 20;
    [SerializeField]
    public int DrogarseTotalCounter = 20;

    [Header("UI")]
    [Space(8)]
    public float alphaFadeInOutIncrementalStep = 0.1f; //Hay alguna manera de en el script de GameManagerEditor ocultar algunos fields?
    public float timeFadeInOutIncrementalStep = 0.1f;

    [Header("Music Clips")]
    [Space(8)]


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

    public int Selector(Counter counter)
    {
        switch (counter)
        {
            case Counter.Defecating:
                return CagarTotalCounter;
            case Counter.Eating:
                return ComerTotalCounter;
            case Counter.Working:
                return TrabajarTotalCounter;
            case Counter.Sleeping:
                return DormirTotalCounter;
            case Counter.Vomiting:
                return VomitarTotalCounter;
            case Counter.Drinking:
                return EscabiarTotalCounter;
            case Counter.Kissing:
                return BesarTotalCounter;
            case Counter.Smoking:
                return DrogarseTotalCounter;
            default:
                throw new Exception("Bad Counters Enum value");
        }
    }
}
