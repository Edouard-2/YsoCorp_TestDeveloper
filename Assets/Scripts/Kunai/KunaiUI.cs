using System.Collections.Generic;
using UnityEngine;

public class KunaiUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField]
    private EventObserver _observerUpdateKunaiUI;

    [Header("Animators")]
    [SerializeField]
    private List<Animator> _listKunaisAnimator;

    private bool[] _isUsed = { false, false, false };

    private int _hashRemove = Animator.StringToHash("Remove");
    private int _hashAdd = Animator.StringToHash("Add");

    private void Awake()
    {
        _observerUpdateKunaiUI.eventHandle += UpdateKunaiUI;
    }

    private void OnDestroy()
    {
        _observerUpdateKunaiUI.eventHandle -= UpdateKunaiUI;
    }

    private void UpdateKunaiUI(ISubject subject)
    {
        Debug.Log("Update Kunai UI");

        if (subject is not KunaiController kunaiController) return;

        for (int i = 0; i < 3; i++)
        {
            if(kunaiController._currentKunaiCount > i && _isUsed[i])
            {
                _listKunaisAnimator[i].Play(_hashAdd);
                _isUsed[i] = false;
            }
            else if (kunaiController._currentKunaiCount == i && !_isUsed[i])
            {
                _isUsed[i] = true;
                _listKunaisAnimator[i].Play(_hashRemove);
            }
        }
    }
}