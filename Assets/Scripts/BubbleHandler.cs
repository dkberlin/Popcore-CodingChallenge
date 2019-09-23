using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BubblePop
{
    public class BubbleHandler : MonoBehaviour
    {
        public PointsPopup _pointsPopupPrefab;
        
        private readonly List<Color> _bubbleColors = new List<Color>
        {
            Color.blue,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.red,
            Color.yellow,
            new Color32(255, 100, 100, 255),
            new Color32(100, 255, 100, 255),
            new Color32(100, 100, 255, 255),
            new Color32(200, 200, 100, 255),
            new Color32(100, 200, 200, 255),
            new Color32(200, 100, 200, 255),
            new Color32(100, 200, 255, 255)
        };

        private List<int> _bubbleValues;
        private int _maxGridValue;
        public delegate void BubblePopped(int points);
        public event BubblePopped OnBubblePopped;
        public event BubblePopped MaxBubblePopped;

        public void SetValues(List<int> values, int maxGridPowerValue)
        {
            _bubbleValues = values;
            _maxGridValue = maxGridPowerValue;
        }

        public void SetBulletValueAndColor(Bubble bubble)
        {
            var rndValue = Random.Range(0, _maxGridValue);

            bubble.SetValue(_bubbleValues[rndValue]);
            bubble.SetColor(_bubbleColors[rndValue]);
        }
        
        public void CreatePointsPopup(Vector2 position, int amount, Color color)
        {
            var popupTransform = Instantiate(_pointsPopupPrefab, position, Quaternion.identity);
            var popup = popupTransform.GetComponent<PointsPopup>();
            
            popup.Setup(amount, color);
        }
        
        public PoolBubble GetRandomPoolBubble()
        {
            var b = new PoolBubble();
            var rndValue = Random.Range(0, _maxGridValue);
            
            b.BubbleValue = _bubbleValues[rndValue];
            b.BubbleColor = _bubbleColors[rndValue];
            
            return b;
        }
        
        public int GetMaxBubbleValue()
        {
            var maxValueIndex = _bubbleValues.Count -1;
            
            return _bubbleValues[maxValueIndex];
        }

        public void SetNewValueForBubble(Bubble bubble, int sortedNeighborsCount)
        {
            var bubbleValueIndex = _bubbleValues.FindIndex(a => a.Equals(bubble.BubbleValue));
            
            if (_bubbleValues[bubbleValueIndex] == GetMaxBubbleValue())
            {
                bubble.TriggerMaxValueEffect();
                
                if (MaxBubblePopped != null)
                {
                    MaxBubblePopped.Invoke(bubble.BubbleValue);
                }
                
                return;
            }

            try
            {
                bubble.SetValue(_bubbleValues[bubbleValueIndex + sortedNeighborsCount -1]);
                bubble.SetColor(_bubbleColors[bubbleValueIndex + sortedNeighborsCount -1]);
            }
            catch (Exception e)
            {
                if (MaxBubblePopped != null)
                {
                    MaxBubblePopped.Invoke(bubble.BubbleValue);
                }
                
                Debug.LogFormat("{0} for bubble {1}",e,bubble.BubbleValue);
                throw;
            }
            
            if (bubble.BubbleValue == GetMaxBubbleValue())
            {
                bubble.TriggerMaxValueEffect();
                
                if (MaxBubblePopped != null)
                {
                    MaxBubblePopped.Invoke(bubble.BubbleValue);
                }
                
                return;
            }
            
            CreatePointsPopup(bubble.transform.position, bubble.BubbleValue, bubble.BubbleColor);
            bubble.TriggerParticleEffect(bubble.BubbleColor);
            
            if (OnBubblePopped != null)
            {
                OnBubblePopped.Invoke(bubble.BubbleValue);
            }
        }
    }
}