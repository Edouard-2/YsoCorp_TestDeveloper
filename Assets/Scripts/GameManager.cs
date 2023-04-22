using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private KunaiController _kunai;

    private void Awake()
    {
        Instance = this;
    }

    internal void StartLevel()
    {
        _kunai?.StartLevel();
    }

    internal void FinishLevel()
    {
        SystemManager.Instance.FinishLevel();
    }

}
