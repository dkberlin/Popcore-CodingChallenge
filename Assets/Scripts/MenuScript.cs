using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private Button _playButton;
    
    [SerializeField]
    private RaycastAiming _aimingHandler;
    
    public delegate void PlayClicked();
    public event PlayClicked OnPlayButtonClicked;

    void Start()
    {
        _aimingHandler.gameObject.SetActive(false);
        _playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        _aimingHandler.gameObject.SetActive(true);
        var cg = transform.GetComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
