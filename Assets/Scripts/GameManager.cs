using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Components")]
    [SerializeField]
    private EventObserver _balloonsDestroy;

    [Header("Components")]
    [SerializeField]
    private KunaiController _kunai;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _ballonPrefab;

    private int _balloonsDestroyCount;

    private List<Vector3> _listPositionsBallon = new();
    private List<Ballon> _listBalloons = new();

    private void Awake()
    {
        Instance = this;

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
        if(_balloonsDestroyCount == _listBalloons.Count)
        {
            FinishLevel();
            return true;
        }
        return false;
    }

}
