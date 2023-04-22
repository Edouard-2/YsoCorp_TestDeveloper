using UnityEngine;
using UnityEngine.EventSystems;

public class KunaiInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    // ------- Serialized ------ //
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private KunaiController _kunaiController;
    [Range(0,100),SerializeField]
    private float _distanceReductor;

    // ------- Private ------ //
    private bool _canPlay;

    private Vector3 _directionRotation;
    private Vector2 _startPosition;
    private Vector2 _endPosition;

    private void Start()
    {
        _startPosition = _camera.WorldToScreenPoint(_kunaiController.transform.position);
        _endPosition = _startPosition;
    }

    #region UI Inputs

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateEndPosition(eventData.position);
        if (!_canPlay) return;

        UpdateKunaiRotation(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_canPlay) return;

        UpdateKunaiRotation(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_canPlay)
        {
            _endPosition = _startPosition; 
            return;
        }
        
        _canPlay = false;

        UpdateKunaiRotation(eventData.position);

        _endPosition = _startPosition;

        _kunaiController.Launch();
    }

    #endregion

    internal void InputReady()
    {
        _canPlay = true;
    }

    private void UpdateEndPosition(Vector3 padPosition)
    {
        // Update User Position
        _endPosition = padPosition;
    }

    internal void UpdateKunaiRotation( Vector3? padPosition = null, bool feedback = true)
    {
        if(padPosition != null)
            UpdateEndPosition((Vector3)padPosition);

        // Calcul direction
        Vector2 dir = (_startPosition - _endPosition).normalized;
        _directionRotation.y = dir.x;
        _directionRotation.z = dir.y;

        // Update Kunai Direction
        _kunaiController.UpdateRotation(dir);

        if (!feedback) return;

        // Update Kunai Direction Feedback
        _kunaiController.UpdateDirectionFeedback(dir);
    }
}
