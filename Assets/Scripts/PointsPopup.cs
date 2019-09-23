using BubblePop;
using TMPro;
using UnityEngine;

public class PointsPopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _tmpText;
    [SerializeField]
    private float _timeToDisappear;
    
    public BubbleHandler bubbleHandler;

    private Color _textColor;

    public void Setup(int pointValue, Color color)
    {
        _tmpText.text = pointValue.ToString();
        _tmpText.color = color;
    }

    private void Update()
    {
        float speed = 1f;   
        transform.position += new Vector3(0,speed) * Time.deltaTime;
        
        _timeToDisappear -= Time.deltaTime;

        if (!(_timeToDisappear < 0)) return;
        
        var disappearSpeed = 3f;
            
        _textColor.a -= disappearSpeed * Time.deltaTime;
        _tmpText.color = _textColor;
        
        if (_textColor.a < 0)
        {
            Destroy(gameObject);
        }
    }
}
