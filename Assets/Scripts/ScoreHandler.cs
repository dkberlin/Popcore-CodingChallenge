using System;
using BubblePop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _scoreText;
    [SerializeField]
    private BubbleHandler _bubbleHandler;
    [SerializeField]
    Slider _progressBar;
    [SerializeField]
    TMP_Text _currentLevelText;
    [SerializeField]
    TMP_Text _nextLevelText;
    
    private float _levelUpIncreaseValue = 1.5f;
    private float _score;
    private float _nextLevelScore;
    private int _currentLevel;
    
    public delegate void LeveledUp();
    public event LeveledUp OnLevelUp; 

    private void Start()
    {
        _bubbleHandler.OnBubblePopped += OnBubblePopped;
        _bubbleHandler.MaxBubblePopped += OnBubblePopped;
        _scoreText.text = _score.ToString();
        _nextLevelScore = 1000;
        OnLevelUp += LevelUp;
        _progressBar.value = 0;
        _currentLevel = 1;
        
        UpdateLevelBubbles(_currentLevel);
    }

    private void OnBubblePopped(int points)
    {
        _score += points;
        
        var scoreString = _score.ToString();
        
        _progressBar.value = _score / _nextLevelScore;
        
        if (_score > 10000)
        {
            var thousands = _score / 10000;
            var hundreds = _score - (thousands * 10000);
            var firstTwoHundreds = GetFirstTwoDigits(hundreds);
            scoreString = thousands + "k." + firstTwoHundreds;
        }
        
        _scoreText.text = scoreString;
        
        if (_score >= _nextLevelScore)
        {
            if (OnLevelUp != null)
            {
                OnLevelUp.Invoke();
            }
        }
    }

    private void LevelUp()
    {
        _nextLevelScore = _nextLevelScore + (int) (_nextLevelScore * _levelUpIncreaseValue);
        _progressBar.value = _score / _nextLevelScore;
        
        _currentLevel++;
        
        UpdateLevelBubbles(_currentLevel);
    }

    private void UpdateLevelBubbles(int currentLevel)
    {
        _currentLevelText.text = currentLevel.ToString();
        _nextLevelText.text = currentLevel + 1.ToString();
    }

    private float GetFirstTwoDigits(float number)
    {
        if(number == 0)
        {
            return number;
        }
        
        int numberOfDigits = (int)Math.Floor(Math.Log10(number) + 1);
        
        if (numberOfDigits >= 2)
        {
            return (int)Math.Truncate((number / Math.Pow(10, numberOfDigits - 2)));
        }
        else
        {
            return number; 
        }
    }
}
