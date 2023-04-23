using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private Portal _otherPortal;

    private bool _canTeleport = true;
    private bool _canFinishTeleport = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_canTeleport)
        {
            if (_canFinishTeleport)
            {
                _canFinishTeleport = false;
                other.GetComponent<KunaiController>().FinishTeleport();
            }
            return;
        }
        _canTeleport = false;
        _otherPortal.TeleportKunai(other.transform);
    }
    
    private void OnTriggerExit(Collider other)
    {
        _canTeleport = true;
    }

    internal void TeleportKunai(Transform kunaiTransform)
    {
        _canTeleport = false;
        _canFinishTeleport = true;
        KunaiController kunai = kunaiTransform.GetComponent<KunaiController>();
        kunai.Teleport(transform.position);
        kunai.EditDirection(transform.up);
    }
}
