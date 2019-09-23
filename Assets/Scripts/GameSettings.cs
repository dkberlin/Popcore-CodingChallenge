using System;
using UnityEngine;

namespace BubblePop
{
    [Serializable]
    public class GameSettings
    {
        [Range(2, 11)] public int maxPowerValue;
        [Range(2, 6)] public int maxGridPowerValue; 
    }
}