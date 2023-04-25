using System;
using System.Collections;
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
    private LineRenderer _lineRendererForPortal;
    [SerializeField]
    private TrailRenderer _trailRenderer;
    [SerializeField]
    private Transform _transformTrailPosition;
    [SerializeField]
    private CharacterController _charaController;
    [SerializeField]
    private Animator _animator;

    [Header("VFX")]
    [SerializeField]
    private GameObject _vfxStuckInWood;
    [SerializeField]
    private GameObject _vfxImpactRebond;

    [Header("Layers")]
    [SerializeField]
    private LayerMask _woodLayer;
    [SerializeField]
    private LayerMask _rebondLayer;
    [SerializeField]
    private LayerMask _portalLayer;
    [SerializeField]
    private LayerMask _layerFeedback;

    [Header("IObeservers")]
    [SerializeReference]
    private List<EventObserver> _observersFinishLevel;
    [SerializeReference]
    private List<EventObserver> _observersLaunchKunai;
    [SerializeReference]
    private List<EventObserver> _observersStuckKunai;
    [SerializeReference]
    private List<EventObserver> _observersTeleport;
    [SerializeReference]
    private List<EventObserver> _observersResetKunai;
    [SerializeReference]
    private List<EventObserver> _observersImpact;


    // ------- Private ------ //
    private bool _hasBeenLaunched;
    internal bool _isStuck;
    private bool _hasPressed;
    private bool _canPlay;
    private bool _InputStop = false;

    private int _hashStuck = Animator.StringToHash("Stuck");
    private int _hashRespawn = Animator.StringToHash("Spawn");
    private int _hashFinish = Animator.StringToHash("FinishLevel");

    internal int _currentKunaiCount = 3;

    private List<GameObject> _listMeshesInLevel = new();

    private Collider _previousCollider;

    private Vector2 _endPositionInput;
    private Vector2 _startPositionInput;
    private Vector3 _startPosition;
    private Vector3 _directionRotation;
    private Quaternion _startRotation;


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
        // If the player can play
        if (_canPlay && !_InputStop)
        {
            // If the player touch the screen
            if (Input.touchCount > 0)
            {
                _hasPressed = true;
                UpdateKunaiRotation(Input.mousePosition);
            }
            // If the player released the screen
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
        // Move the Kunai upward he's launch
        if (_charaController == null || !_hasBeenLaunched || _isStuck) return;
        _charaController.Move(transform.up * _powerLaunch * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("Rebond") && _previousCollider != hit.collider)
        {
            if (_previousCollider != null)
                _previousCollider.isTrigger = false;

            _previousCollider = hit.collider;
            _previousCollider.isTrigger = true;
            transform.up = Vector3.Reflect(transform.up, hit.normal);

            Destroy(Instantiate(_vfxImpactRebond, transform.position, Quaternion.identity),1);
        }
        else if (hit.transform.CompareTag("Wood"))
        {
            Stuck();
        }
        else if (hit.transform.CompareTag("FinishLine"))
        {
            FinishLevel();
        }
        else if (hit.transform.CompareTag("Border"))
        {
            Respawn();
        }

        NotifyObservers(_observersImpact);
    }

    // ----------------- Input Management ----------------- //
    #region Input Management

    internal void CanPlay()
    {
        _canPlay = true;
    }

    internal void InputStop()
    {
        _InputStop = true;
    }

    internal void InputReady()
    {
        _InputStop = false;
    }
    #endregion

    // ----------------- Level Behaviour ----------------- //
    #region Level Behaviour

    /// <summary>
    /// Remove the kunais stuck in the woods
    /// </summary>
    internal void ClearMeshesInLevel()
    {
        foreach (GameObject go in _listMeshesInLevel)
        {
            Destroy(go);
        }
        _listMeshesInLevel.Clear();
    }

    /// <summary>
    /// Reset the number of kunai that the player has
    /// </summary>
    internal void ResetCount()
    {
        _currentKunaiCount = 3;
        NotifyObservers(_observersResetKunai);
    }

    private async void FinishLevel()
    {
        Stop();

        _animator.Play(_hashFinish);

        await Task.Delay(500);

        GameManager.Instance?.FinishLevel();
    }

    internal void StartLevel()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        _animator.Play(_hashRespawn);

        ShowLineRenderer();
    }

    /// <summary>
    /// Reset all stats and the position of the player
    /// </summary>
    internal void RestartLevel()
    {
        Stop();
        ClearMeshesInLevel();
        ResetCount();
        Respawn();
    }

    #endregion

    // ----------------- Gameplay Behaviour ----------------- //
    #region Gameplay Behaviour

    /// <summary>
    /// Throw the kunai in the current direction
    /// </summary>
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

    internal void UpdateRotation(Vector3 direction)
    {
        transform.up = direction;
    }

    /// <summary>
    /// Respawn the player and restart the current level
    /// <summary>
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

    /// <summary>
    /// Spawn mesh when the kunai is stuck on a wood board
    /// </summary>
    private void SpawnMesh()
    {
        _listMeshesInLevel.Add(Instantiate(_prefabMeshKunai, transform.position, transform.rotation, GameManager.Instance._transform));
    }

    /// <summary>
    /// When the kunai collide with wood
    /// </summary>
    private void Stuck()
    {
        Destroy(Instantiate(_vfxStuckInWood, transform.position, transform.rotation), 1);

        _animator.Play(_hashStuck);

        NotifyObservers(_observersStuckKunai);

        Stop();
    }

    /// <summary>
    /// Stop input and the current kunai
    /// </summary>
    internal void Stop()
    {
        if (_trailRenderer != null)
            _trailRenderer.enabled = false;

        _isStuck = true;

        _charaController.enabled = false;
    }

    #endregion

    // ----------------- Teleportation ----------------- //
    #region Teleportation
    /// <summary>
    /// Teleport the kunai from his current position to the target position
    /// </summary>
    /// <param name="position"> Target Position </param>
    internal void Teleport(Vector3 position)
    {
        if (_trailRenderer != null)
        {
            _trailRenderer.transform.SetParent(null);
            _trailRenderer.transform.position = transform.position;
            Destroy(_trailRenderer.gameObject, 1);
        }

        NotifyObservers(_observersTeleport);

        _charaController.enabled = false;
        transform.position = position;
    }

    /// <summary>
    /// Calcul the position on the next portal with the offset
    /// </summary>
    /// <param name="contactHitPos"> Position hit by the kunai </param>
    /// <param name="portalHit"> Transform of the portal hit by the kunai </param>
    /// <param name="otherPortal"> The other portal </param>
    /// <returns></returns>
    internal Vector3 CalculPosiotionForNextPortal(Vector3 contactHitPos, Transform portalHit, Portal otherPortal)
    {
        Vector3 directionOffsetFromPortal = (portalHit.position - contactHitPos);
        float dotForOtherPortal = Vector3.Dot(portalHit.right, directionOffsetFromPortal);

        return otherPortal.transform.position + otherPortal.transform.right * dotForOtherPortal;
    }

    /// <summary>
    /// When the player has arrived to his new position after teleportation
    /// </summary>
    internal void FinishTeleport()
    {
        _charaController.enabled = true;
        _trailRenderer = Instantiate(_prefabTrailRenderer, _transformTrailPosition.position, _transformTrailPosition.rotation, _transformTrailPosition);
    }
    #endregion

    // ----------------- Feedback Kunai Direction ----------------- //
    #region Feedback Kunai Direction

    /// <summary>
    /// Update linerenderer feedback chen the player aim
    /// </summary>
    internal void UpdateDirectionFeedback()
    {
        _lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, 10, _layerFeedback))
        {
            // REBOND
            if (RebondLineFeedback(hit))
                return;

            // PORTAL
            if (PortalLineFeedback(hit))
                return;

            // WOOD
            _lineRenderer.SetPosition(1, hit.point);
            _lineRenderer.SetPosition(2, hit.point);

        }
        else
        {
            _lineRenderer.SetPosition(1, transform.position + transform.up * 10);
            _lineRenderer.SetPosition(2, transform.position + transform.up * 10);
        }
    }

    /// <summary>
    /// Update the rotation of the kunai when he's not launch
    /// </summary>
    /// <param name="padPosition"> Input Position </param>
    /// <param name="feedback"> True : Do feedback </param>
    internal void UpdateKunaiRotation(Vector3? padPosition = null, bool feedback = true)
    {
        if (padPosition != null)
            UpdateEndPositionInput((Vector3)padPosition);

        // Calcul direction
        Vector2 dir = (_startPositionInput - _endPositionInput).normalized;
        _directionRotation.y = dir.x;
        _directionRotation.z = dir.y;

        // Update Kunai Direction
        UpdateRotation(dir);

        if (!feedback) return;

        // Update Kunai Direction Feedback
        UpdateDirectionFeedback();
    }

    /// <summary>
    /// Update the position of the player on the screen
    /// </summary>
    /// <param name="padPosition"> position of the player on the screen </param>
    private void UpdateEndPositionInput(Vector3 padPosition)
    {
        // Update User Position
        _endPositionInput = padPosition;
    }

    /// <summary>
    /// Update LineRenderer for feedback when the obstacle is metalic
    /// </summary>
    /// <param name="hit"> RaycastHitInfo of the obstacle touch whith the raycast </param>
    /// <returns></returns>
    private bool RebondLineFeedback(RaycastHit hit)
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
            return true;
        }
        return false;
    }

    /// <summary>
    /// Update LineRenderer for feedback when the obstacle is a portal
    /// </summary>
    /// <param name="hit"> RaycastHitInfo of the obstacle touch whith the raycast </param>
    /// <returns></returns>
    private bool PortalLineFeedback(RaycastHit hit)
    {
        if ((_portalLayer.value & 1 << hit.collider.gameObject.layer) > 0)
        {
            if (!_lineRendererForPortal.enabled) ShowLineRendererForPortal();

            _lineRenderer.SetPosition(1, hit.point);
            _lineRenderer.SetPosition(2, hit.point);

            Portal otherPortal = hit.collider.GetComponent<Portal>()._otherPortal;
            Vector3 portalUp = otherPortal.transform.up;
            Vector3 positionOtherPortal = CalculPosiotionForNextPortal(hit.point, hit.transform, otherPortal);

            _lineRendererForPortal.SetPosition(0, positionOtherPortal);

            RaycastHit hitPortal;
            if (hit.collider.Raycast(new Ray(positionOtherPortal, portalUp), out hitPortal, 10 - hit.distance))
            {
                _lineRendererForPortal.SetPosition(1, hitPortal.point);
            }
            else
            {
                _lineRendererForPortal.SetPosition(1, positionOtherPortal + portalUp * (10 - hit.distance));
            }
            return true;
        }

        if (_lineRendererForPortal.enabled) HideLineRendererPortal();
        return false;
    }

    #endregion

    // ----------------- Line Renderer For Feedback ----------------- //
    #region Line Renderer For Feedback


    /// <summary>
    /// Disable / Hide all the line renderer used for the feedback before the throw
    /// </summary>
    private void HideLineRenderer()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
        _lineRenderer.SetPosition(2, Vector3.zero);

        HideLineRendererPortal();
    }


    /// <summary>
    /// Disable / Hide only the line renderer used for the portal
    /// </summary>
    private void HideLineRendererPortal()
    {
        _lineRendererForPortal.enabled = false;
        _lineRendererForPortal.SetPosition(0, Vector3.zero);
        _lineRendererForPortal.SetPosition(1, Vector3.zero);
    }
    
    /// <summary>
    /// Enable / Show all the line renderer used for the feedback before the throw
    /// </summary>
    private void ShowLineRenderer()
    {
        if(_lineRenderer.enabled) return;
        _lineRenderer.enabled = true;

        ShowLineRendererForPortal();
    }

    /// <summary>
    /// Enable / Show only the line renderer used for the portal
    /// </summary>
    private void ShowLineRendererForPortal()
    {
        if(_lineRendererForPortal.enabled) return;
        _lineRendererForPortal.enabled = true;
    }

    #endregion

    // ----------------- Observers ----------------- //
    #region Observers

    /// <summary>
    /// Notify all the observers in a IEnumerable
    /// </summary>
    /// <param name="observers"> The collection of observers to notify </param>
    private void NotifyObservers(IEnumerable<EventObserver> observers)
    {
        foreach(EventObserver observer in observers)
        {
            observer.Raise(this);
        }
    }
    #endregion
}