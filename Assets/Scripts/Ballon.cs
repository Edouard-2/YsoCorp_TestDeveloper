using UnityEngine;

public class Ballon : MonoBehaviour, ISubject
{
    [Header("Components")]
    [SerializeField]
    private ParticleSystem _vfxExplosion;
    [SerializeField]
    private Animator _meshAnimator;

    [Header("Events")]
    [SerializeField]
    private EventObserver _balloonsExplosed;
        
    internal int _hashExplosion = Animator.StringToHash("Explosion");
    internal int _hashRespawn = Animator.StringToHash("Respawn");

    internal bool _hasExplosed;

    private void Start()
    {
        GameManager.Instance?.AddBallon(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasExplosed) return;
        _hasExplosed = true;

        _vfxExplosion.Play();

        _meshAnimator.Play(_hashExplosion);

        _balloonsExplosed.Raise(this);
    }

    internal void Respawn()
    {
        _hasExplosed = false;
        _meshAnimator.Play(_hashRespawn);
    }
}
