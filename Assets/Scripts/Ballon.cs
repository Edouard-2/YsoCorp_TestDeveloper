using System.Collections.Generic;
using System.Threading.Tasks;
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
    private List<EventObserver> _observersBalloonsExplosed = new();
        
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

        _meshAnimator.Play(_hashExplosion);

        _vfxExplosion.Play();
        _observersBalloonsExplosed.ForEach(e => e.Raise(this));
    }

    internal void Respawn()
    {
        _hasExplosed = false;
        _meshAnimator.Play(_hashRespawn);
    }
}
