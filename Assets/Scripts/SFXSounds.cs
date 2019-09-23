using BubblePop;
using UnityEngine;

public class SFXSounds : MonoBehaviour
{
    [SerializeField]
    private AudioSource _bubblePopSource;
    [SerializeField]
    private AudioSource _fireWorkSource;
    [SerializeField]
    private AudioSource _levelUpSource;
    [SerializeField]
    private BubbleHandler _bubbleHandler;
    [SerializeField]
    private ScoreHandler _scoreHandler; 
    

    private void Start()
    {
        _bubbleHandler.OnBubblePopped += PlayBubblePopSound;
        _bubbleHandler.MaxBubblePopped += PlayFireworkSound;
        _scoreHandler.OnLevelUp += PlayLevelUpSound;
    }

    private void PlayLevelUpSound()
    {
        _levelUpSource.Play();
    }

    private void PlayFireworkSound(int points)
    {
        _fireWorkSource.Play();
    }

    private void PlayBubblePopSound(int points)
    {
        _bubblePopSource.Play();
    }
}
