using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRestartLevel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private KunaiController _kunaiInput;

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.25f;
        _kunaiInput.InputStop();
    }

    public async void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;

        await Task.Delay(50);

        _kunaiInput.InputReady();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameManager.Instance != null) 
            GameManager.Instance.RestartLevel();
    }
}
