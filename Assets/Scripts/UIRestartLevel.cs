using UnityEngine;
using UnityEngine.EventSystems;

public class UIRestartLevel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField]
    private KunaiController _kunaiInput;

    public void OnPointerDown(PointerEventData eventData)
    {
        _kunaiInput.InputStop();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameManager.Instance != null) 
            GameManager.Instance.RestartLevel();
    }
}
