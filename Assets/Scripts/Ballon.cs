using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Ballon : MonoBehaviour, ISubject
{
    [Header("Stats")]
    [SerializeField]
    private float _delayLaunch;
    [SerializeField]
    private float _timeTranslation;
    [SerializeField]
    private AnimationCurve _curveTranslationBehaviour;

    [Header("Transform")]
    [SerializeField]
    private Transform _targetPositiontransform;

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

    private bool _restartLaunch = true;

    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Transform _transform;
    private Coroutine _coroutineTranslation;
    private WaitForSeconds _waitDelay;

    internal bool _hasExplosed;

    private void Awake()
    {
        _waitDelay = new WaitForSeconds(_delayLaunch);

        _transform = transform;
        _startPosition = _transform.position;
    }

    private void Start()
    {
        GameManager.Instance?.AddBallon(this);

        if (_targetPositiontransform == null) return;
        _endPosition = _targetPositiontransform.position;
        _coroutineTranslation = StartCoroutine(TranslationBallon_Coroutine(_startPosition, _endPosition));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasExplosed) return;
        _hasExplosed = true;

        if(_coroutineTranslation!=null)
            StopCoroutine(_coroutineTranslation);

        _meshAnimator.Play(_hashExplosion);

        _vfxExplosion.Play();
        _observersBalloonsExplosed.ForEach(e => e.Raise(this));
    }

    internal void Respawn()
    {
        _hasExplosed = false;
        _meshAnimator.Play(_hashRespawn);
        
        _restartLaunch = true;

        if (_targetPositiontransform == null) return;
        StopCoroutine(_coroutineTranslation);
        _coroutineTranslation = StartCoroutine(TranslationBallon_Coroutine(_startPosition, _endPosition));
    }

    IEnumerator TranslationBallon_Coroutine(Vector3 startPosition, Vector3 endPosition)
    {
        if (_restartLaunch)
        {
            _restartLaunch = false;
            yield return _waitDelay;
        }

        float time = 0;
        while (time < _timeTranslation)
        {
            _transform.position = Vector3.Lerp(startPosition,endPosition,_curveTranslationBehaviour.Evaluate(time / _timeTranslation));
            time += Time.deltaTime;
            yield return null;
        }

        _coroutineTranslation = StartCoroutine(TranslationBallon_Coroutine(endPosition, startPosition));
    }
}
