using UnityEngine;
using UnityEngine.EventSystems;

public class UIRestartLevel : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameManager.Instance != null) 
            GameManager.Instance.RestartLevel();
    }
}
