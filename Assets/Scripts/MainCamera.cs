using TMPro;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeReference]
    private EventObserver _eventCameraShake;

    private Animator _animator;
    private int _hashShake = Animator.StringToHash("Shake");

    private void Awake()
    {
        _animator= GetComponent<Animator>();

        _eventCameraShake.eventHandle += CameraShake; 
    }

    private void OnDestroy()
    {
        _eventCameraShake.eventHandle -= CameraShake;
    }

    private void CameraShake(ISubject subject)
    {
        _animator.Play(_hashShake);
    }
}
