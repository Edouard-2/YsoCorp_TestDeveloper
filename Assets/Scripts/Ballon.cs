using UnityEngine;

public class Ballon : MonoBehaviour
{
    [SerializeField]
    private GameObject _mesh;
    [SerializeField]
    private ParticleSystem _vfxExplosion;
    
    private bool _hasExplosed;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasExplosed) return;
        _hasExplosed = true;

        _vfxExplosion.Play();
        _mesh.SetActive(false);

        Destroy(gameObject, 5);
    }
}
