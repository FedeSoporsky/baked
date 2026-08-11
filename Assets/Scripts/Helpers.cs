using System;
using static Assets.Scripts.Enums;

namespace Assets.Scripts
{
    public static class Helpers
    {
        public static string GetCounterName(Counter counter)
        {
            switch (counter)
            {
                case Counter.Defecating:
                    return "CagarCounter";
                case Counter.Eating:
                    return "ComerCounter";
                case Counter.Working:
                    return "TrabajarCounter";
                case Counter.Sleeping:
                    return "DormirCounter";
                default:
                    throw new Exception("Bad Counters Enum value");
            }
        }
    }
}
