using UnityEngine;
using UnityEngine.EventSystems;

public class UIEndLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private EventObserver _eventObserverEndLevel;

    private Animator _animator;

    private int _hashEndLevel = Animator.StringToHash("EndLevel");

    private void Awake()
    {
        _animator = transform.parent.GetComponent<Animator>();

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
