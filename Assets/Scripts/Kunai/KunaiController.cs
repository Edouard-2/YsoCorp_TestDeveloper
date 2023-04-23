using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class KunaiController : MonoBehaviour, ISubject
{
    // ------- Serialized ------ //
    [Header("Stats")]
    [Range(0,500), SerializeField]
    private float _powerLaunch;
    
    [Header("Prefabs")]
    [SerializeField]
    private TrailRenderer _prefabTrailRenderer;

    [Header("Components")]
    [SerializeField]
    private KunaiInput _kunaiInput;
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private TrailRenderer _trailRenderer;
    [SerializeField]
    private Transform _transformTrailPosition;

    [Header("Layers")]
    [SerializeField]
    private LayerMask _woodLayer;
    [SerializeField]
    private LayerMask _rebondLayer;
    [SerializeField]
    private LayerMask _layerFeedback;

    [Header("IObeservers")]
    [SerializeReference]
    private List<EventObserver> _observersFinishLevel;
    [SerializeReference]
    private List<EventObserver> _observersLaunchKunai;
    [SerializeReference]
    private List<EventObserver> _observersResetKunai;
    [SerializeReference]
    private List<EventObserver> _observersImpact;

    // ------- Private ------ //
    private bool _hasBeenLaunched;
    internal bool _isStuck;

    private int _hashStuck = Animator.StringToHash("Stuck");
    private int _hashRespawn = Animator.StringToHash("Spawn");
    private int _hashFinish = Animator.StringToHash("Finish");

    internal int _currentKunaiCount = 3;

    private Animator _animator;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private PhysicMaterial _colliderMaterial;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        _colliderMaterial = _capsuleCollider.material;

        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _trailRenderer.enabled = false;

        HideLineRenderer();
    }

    private void Update()
    {
        if (_rb == null || !_hasBeenLaunched || _isStuck) return;
        UpdateRotation(_rb.velocity.normalized);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("FinishLine"))
        {
            _capsuleCollider.material = null;
            Finish();
        }
        else if (collision.transform.CompareTag("Border"))
        {
            Respawn();
        }
        else if (collision.transform.CompareTag("Wood"))
        {
            _capsuleCollider.material = null;
            Stuck();
        }
        else
        {
            //transform.position = collision.contacts[0].point;
        }

        NotifyObservers(_observersImpact);

        CheckIfWoodIsInFront(collision.contacts[0]);
    }

    internal void StartLevel()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
        _animator.Play(_hashRespawn);

        ShowLineRenderer();
    }

    private void Stuck()
    {
        _animator.Play(_hashStuck);
        Stop();
    }

    private async void Finish()
    {
        Stop();

        _animator.Play(_hashFinish);

        await Task.Delay(500);

        GameManager.Instance?.FinishLevel();
    }

    internal void Stop()
    {
        _trailRenderer.enabled = false;

        _isStuck = true;

        _rb.Sleep();
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    internal void EditVelocityDirection(Vector3 direction)
    {
        _rb.velocity = direction * _rb.velocity.magnitude;
    }

    internal void UpdateRotation(Vector3 direction)
    {
        transform.up = direction;
    }
    
    internal void Teleport(Vector3 position)
    {
        _trailRenderer.transform.SetParent(null);
        Destroy(_trailRenderer.gameObject, 1);

        transform.position = position;
    }

    internal void FinishTeleport()
    {
        _trailRenderer = Instantiate(_prefabTrailRenderer, _transformTrailPosition.position, _transformTrailPosition.rotation, _transformTrailPosition);
    }
    
    internal void UpdateDirectionFeedback(Vector3 direction)
    {
        _lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, transform.up), out hit, 10, _layerFeedback)){

            _lineRenderer.SetPosition(1, hit.point); 
            
            if ((_rebondLayer.value & 1 << hit.collider.gameObject.layer) > 0)
            {
                RaycastHit hitRebond;
                if (Physics.Raycast(new Ray(hit.point, Vector3.Reflect(transform.up, hit.normal)), out hitRebond, (10 - hit.distance)))
                {
                    _lineRenderer.SetPosition(2, hitRebond.point);
                }
                else
                {
                    _lineRenderer.SetPosition(2, hit.point + Vector3.Reflect(transform.up, hit.normal) * (10 - hit.distance));
                }
            }
            else if ((_woodLayer.value & 1 << hit.collider.gameObject.layer) > 0)
            {
                _lineRenderer.SetPosition(2, hit.point);
            }
            else
            {
                _lineRenderer.SetPosition(2, hit.point);
            }
        }
        else
        {
            _lineRenderer.SetPosition(1, transform.position + transform.up * 10);
            _lineRenderer.SetPosition(2, transform.position + transform.up * 10);
        }
    }

    internal void Launch()
    {
        _isStuck = false;
        _hasBeenLaunched = true;
        _rb.velocity = transform.up * _powerLaunch;

        _capsuleCollider.enabled = true;

        _trailRenderer.enabled = true;
        _trailRenderer.Clear();

        _currentKunaiCount--;

        HideLineRenderer();

        NotifyObservers(_observersLaunchKunai);

        CheckIfWoodIsInFront(transform.up);
    }

    private void CheckIfWoodIsInFront(ContactPoint contact)
    {
        Vector3 dir = Vector3.Reflect((contact.point - transform.position).normalized, contact.normal);

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, dir), out hit, 100))
        {
            if ((_woodLayer.value & 1 << hit.collider.gameObject.layer) > 0)
            {
                _capsuleCollider.material = null;
            }
        }
    }

    private void CheckIfWoodIsInFront(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, direction), out hit, 100))
        {
            if ((_woodLayer.value & 1 << hit.collider.gameObject.layer) > 0)
            {
                _capsuleCollider.material = null;
            }
        }
    }

    internal void Respawn()
    {
        if (GameManager.Instance != null && GameManager.Instance.CheckAllBallonAreDestroy())
            return;

        if (_currentKunaiCount == 0)
        {
            GameManager.Instance.RestartLevel();
            return;
        }

        transform.position = _startPosition;
        transform.rotation = _startRotation;

        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _hasBeenLaunched = false;

        _capsuleCollider.material = _colliderMaterial;

        ShowLineRenderer();

        _animator.Play(_hashRespawn);
    }

    internal void ResetCount()
    {
        _currentKunaiCount = 3;
        NotifyObservers(_observersResetKunai);
    }

    private void HideLineRenderer()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
        _lineRenderer.SetPosition(2, Vector3.zero);
    }
    
    private void ShowLineRenderer()
    {
        if(_lineRenderer.enabled) return;
        _lineRenderer.enabled = true;
    }

    private void InputReady()
    {
        _kunaiInput.InputReady();
        _kunaiInput.UpdateKunaiRotation(null, false);
    }

    private void NotifyObservers(List<EventObserver> listObservers)
    {
        foreach(EventObserver observer in listObservers)
        {
            observer.Raise(this);
        }
    }
}
