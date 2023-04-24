using UnityEngine;
using UnityEngine.EventSystems;

public class UIEndLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private EventObserver _eventObserverEndLevel;

    [SerializeField]
    private Animator _animator;

    private int _hashEndLevel = Animator.StringToHash("EndLevel");

    private void Awake()
    {
        _eventObserverEndLevel.eventHandle += Display;
    }

    private void OnDestroy()
    {
        _eventObserverEndLevel.eventHandle -= Display;
    }

    private void Display(ISubject subject)
    {
        _animator.Play(_hashEndLevel);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameManager.Instance != null) 
            GameManager.Instance.SwitchSceneForEndLevel();
    }
}
