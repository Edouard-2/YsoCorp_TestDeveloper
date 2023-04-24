using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour, ISubject
{
    public static GameManager Instance;

    [Header("Events")]
    [SerializeField]
    private EventObserver _balloonsDestroy;
    [SerializeField]
    private EventObserver _eventObserverEndLevel;

    [Header("Components")]
    [SerializeField]
    private KunaiController _kunai;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _ballonPrefab;


    private int _balloonsDestroyCount;

    internal Transform _transform;

    private List<Vector3> _listPositionsBallon = new();
    private List<Ballon> _listBalloons = new();

    private void Awake()
    {
        Instance = this;

        _transform = transform;

        if (_balloonsDestroy != null)
            _balloonsDestroy.eventHandle += AddBalloonExplosed;
    }

    private void OnDestroy()
    {
        if(_balloonsDestroy != null)
            _balloonsDestroy.eventHandle -= AddBalloonExplosed;
    }

    internal void StartLevel()
    {
        _kunai?.StartLevel();
    }

    internal void FinishLevel()
    {
        _eventObserverEndLevel.Raise(this);
    }

    internal void SwitchSceneForEndLevel()
    {
        SystemManager.Instance.FinishLevel();
    }
    
    internal void RestartLevel()
    {
        _balloonsDestroyCount = 0;

        ResetBalloons();

        _kunai.RestartLevel();
    }
    
    private void ResetBalloons()
    {
        for (int i = 0; i < _listPositionsBallon.Count; i++)
        {
            if (_listBalloons[i]._hasExplosed)
                _listBalloons[i].Respawn();
            
        }
    }

    internal void AddBallon(Ballon ballon)
    {
        _listPositionsBallon.Add(ballon.transform.position);
        _listBalloons.Add(ballon);
    }
    
    internal void AddBalloonExplosed(ISubject subject)
    {
        if (subject is not Ballon balloon) return;
        _balloonsDestroyCount++;
    }

    internal bool CheckAllBallonAreDestroy()
    {
        Debug.Log(_balloonsDestroyCount);
        Debug.Log(_listBalloons.Count);
        if(_balloonsDestroyCount == _listBalloons.Count)
        {
            FinishLevel();
            return true;
        }
        return false;
    }

}
