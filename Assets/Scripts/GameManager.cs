using System;
using System.Collections.Generic;
using UnityEngine;

namespace BubblePop
{
    public class GameManager : MonoBehaviour
    {
        public BubbleHandler _bubbleHandler;

        private readonly List<int> _bubbleValues = new List<int>();

        [SerializeField] 
        private GameSettings _gameSettings;

        private void Awake()
        {
            SetMaximumBubbleValue(_gameSettings.maxPowerValue);

            _bubbleHandler.SetValues(_bubbleValues, _gameSettings.maxGridPowerValue);
        }

        private void SetMaximumBubbleValue(int gridPowerValue)
        {
            var value = 2;

            for (var power = 1; power <= gridPowerValue; power++)
            {
                var powerValue = (int) Math.Pow(value, power);
                _bubbleValues.Add(powerValue);
            }
        }
    }
}