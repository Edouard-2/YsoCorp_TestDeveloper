using UnityEngine;

public class Ballon : MonoBehaviour, ISubject
{
    [Header("Components")]
    [SerializeField]
    private GameObject _mesh;
    [SerializeField]
    private ParticleSystem _vfxExplosion;

    [Header("Events")]
    [SerializeField]
    private EventObserver _balloonsExplosed;
    
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
        _mesh.SetActive(false);

        _balloonsExplosed.Raise(this);

        Destroy(gameObject, 5);
    }
}
