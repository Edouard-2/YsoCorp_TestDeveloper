using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEndLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private EventObserver _eventObserverEndLevel;
    [SerializeField]
    private EventObserver _eventObserverRestartLevel;

    [SerializeField]
    private Animator _animator;

    private int _hashEndLevel = Animator.StringToHash("EndLevel");
    private int _hashIdle = Animator.StringToHash("IdleInLevel");

    private void Awake()
    {
        _eventObserverEndLevel.eventHandle += Display;
        _eventObserverRestartLevel.eventHandle += Restart;
    }

    private void OnDestroy()
    {
        _eventObserverEndLevel.eventHandle -= Display;
        _eventObserverRestartLevel.eventHandle -= Restart;
    }

    private void Restart(ISubject subject)
    {
        _animator.Play(_hashIdle);
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
