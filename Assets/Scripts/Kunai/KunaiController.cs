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
    private GameObject _prefabMeshKunai;
    [SerializeField]
    private TrailRenderer _prefabTrailRenderer;

    [Header("Camera")]
    [SerializeField]
    private Camera _camera;

    [Header("Components")]
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
    private bool _hasPressed;
    private bool _canPlay;

    private int _hashStuck = Animator.StringToHash("Stuck");
    private int _hashRespawn = Animator.StringToHash("Spawn");
    private int _hashFinish = Animator.StringToHash("Finish");

    internal int _currentKunaiCount = 3;

    private List<GameObject> _listMeshesInLevel = new();

    private Animator _animator;
    private CharacterController _charaController;
    private Collider _previousCollider;

    private Vector2 _endPositionInput;
    private Vector2 _startPositionInput;
    private Vector3 _startPosition;
    private Vector3 _directionRotation;
    private Quaternion _startRotation;

    private void Awake()
    {
        _charaController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _trailRenderer.enabled = false;

        _startPositionInput = _camera.WorldToScreenPoint(transform.position);

        HideLineRenderer();
    }

    private void Update()
    {
        if (_canPlay)
        {
            if (Input.touchCount > 0)
            {
                _hasPressed = true;
                UpdateKunaiRotation(Input.mousePosition);
            }
            else if (Input.touchCount == 0 && _hasPressed)
            {
                _hasPressed = false;

                _canPlay = false;

                UpdateKunaiRotation(Input.mousePosition);

                _endPositionInput = _startPositionInput;

                Launch();
            }
        }
    }
    private void FixedUpdate()
    {
        if (_charaController == null || !_hasBeenLaunched || _isStuck) return;
        _charaController.Move(transform.up * _powerLaunch * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("Rebond") && _previousCollider != hit.collider)
        {
            _previousCollider = hit.collider;
            _previousCollider.isTrigger = true;
            transform.up = Vector3.Reflect(transform.up, hit.normal);
        }
        else if (hit.transform.CompareTag("Wood"))
        {
            Stuck();
        }
        else if (hit.transform.CompareTag("FinishLine"))
        {
            Finish();
        }
        else if (hit.transform.CompareTag("Border"))
        {
            Respawn();
        }

        NotifyObservers(_observersImpact);
    }

    private void UpdateEndPosition(Vector3 padPosition)
    {
        // Update User Position
        _endPositionInput = padPosition;
    }

    internal void UpdateKunaiRotation(Vector3? padPosition = null, bool feedback = true)
    {
        if (padPosition != null)
            UpdateEndPosition((Vector3)padPosition);

        // Calcul direction
        Vector2 dir = (_startPositionInput - _endPositionInput).normalized;
        _directionRotation.y = dir.x;
        _directionRotation.z = dir.y;

        // Update Kunai Direction
        UpdateRotation(dir);

        if (!feedback) return;

        // Update Kunai Direction Feedback
        UpdateDirectionFeedback(dir);
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

    private void SpawnMesh()
    {
        _listMeshesInLevel.Add(Instantiate(_prefabMeshKunai, transform.position, transform.rotation, GameManager.Instance._transform));
    }

    internal void InputStop()
    {
        _canPlay = false;
    }

    private void InputReady()
    {
        _canPlay = true;
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
        if(_trailRenderer!=null)
            _trailRenderer.enabled = false;

        _isStuck = true;

        _charaController.enabled = false;
    }

    internal void UpdateRotation(Vector3 direction)
    {
        transform.up = direction;
    }
    
    internal void Teleport(Vector3 position)
    {
        _trailRenderer.transform.SetParent(null);
        _trailRenderer.transform.position = transform.position;
        Destroy(_trailRenderer.gameObject, 1);

        _charaController.Move(position - transform.position);
    }
    
    internal void EditDirection(Vector3 direction)
    {
        transform.up = direction;
    }

    internal void FinishTeleport()
    {
        _trailRenderer = Instantiate(_prefabTrailRenderer, _transformTrailPosition.position, _transformTrailPosition.rotation, _transformTrailPosition);
    }
    
    internal void UpdateDirectionFeedback(Vector3 direction)
    {
        _lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, 10, _layerFeedback))
        {
            if ((_rebondLayer.value & 1 << hit.collider.gameObject.layer) > 0)
            {
                if (Physics.SphereCast(transform.position, _charaController.radius, transform.up, out hit, 10, _layerFeedback))
                {
                    _lineRenderer.SetPosition(1, hit.point);

                    if ((_rebondLayer.value & 1 << hit.collider.gameObject.layer) > 0)
                    {
                        RaycastHit hitRebond;
                        if (Physics.Raycast(hit.point, Vector3.Reflect(transform.up, hit.normal), out hitRebond, 10 - hit.distance, _layerFeedback))
                        {
                            _lineRenderer.SetPosition(2, hitRebond.point);
                        }
                        else
                        {
                            _lineRenderer.SetPosition(2, hit.point + Vector3.Reflect(transform.up, hit.normal) * (10 - hit.distance));
                        }
                    }
                    else
                    {
                        _lineRenderer.SetPosition(2, hit.point);
                    }
                }
            }
            else
            {
                _lineRenderer.SetPosition(1, hit.point);
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

        _charaController.enabled = true;

        if (_trailRenderer != null)
        {
            _trailRenderer.enabled = true;
            _trailRenderer.Clear();
        }

        _currentKunaiCount--;

        HideLineRenderer();

        NotifyObservers(_observersLaunchKunai);
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

        if (_previousCollider != null)
        {
            _previousCollider.isTrigger = false;
            _previousCollider = null;
        }
        
        _charaController.enabled = false;

        transform.position = _startPosition;
        transform.rotation = _startRotation;

        _hasBeenLaunched = false;

        ShowLineRenderer();

        _animator.Play(_hashRespawn);
    }

    internal void ClearMeshesInLevel()
    {
        foreach (GameObject go in _listMeshesInLevel)
        {
            Destroy(go);
        }
        _listMeshesInLevel.Clear();
    }

    internal void RestartLevel()
    {
        Stop(); 
        ClearMeshesInLevel();
        ResetCount();
        Respawn();
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


    private void NotifyObservers(List<EventObserver> listObservers)
    {
        foreach(EventObserver observer in listObservers)
        {
            observer.Raise(this);
        }
    }
}
