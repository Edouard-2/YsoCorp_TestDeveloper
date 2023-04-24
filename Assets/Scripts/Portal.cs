using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Component")]
    [SerializeField]
    internal Portal _otherPortal;

    [Header("VFX")]
    [SerializeField]
    internal GameObject _vfxExitFrom;
    [SerializeField]
    internal GameObject _vfxGoThrough;

    private bool _canTeleport = true;
    private bool _canFinishTeleport = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_canTeleport)
        {
            if (_canFinishTeleport)
            {
                _canFinishTeleport = false;
                SpawnVFXTeleport(_vfxExitFrom);
                other.GetComponent<KunaiController>().FinishTeleport();
            }
            return;
        }
        SpawnVFXTeleport(_vfxGoThrough);
        _canTeleport = false;
        _otherPortal.TeleportKunai(other.transform, transform);
    }
    
    private void OnTriggerExit(Collider other)
    {
        _canTeleport = true;
    }

    internal void TeleportKunai(Transform kunaiTransform, Transform otherPortalTransform)
    {
        _canTeleport = false;
        _canFinishTeleport = true;
        KunaiController kunai = kunaiTransform.GetComponent<KunaiController>();

        Vector3 positionOtherPortal = kunai.CalculPosiotionForNextPortal(kunaiTransform.position, otherPortalTransform, this);

        kunai.Teleport(positionOtherPortal);

        kunai.EditDirection(transform.up);
    }

    internal void SpawnVFXTeleport(GameObject prefab)
    {
        Destroy(Instantiate(prefab, transform.position, transform.rotation),1);
    }
}
