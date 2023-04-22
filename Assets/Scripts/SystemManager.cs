using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance;

    [SerializeField]
    private Animator _animatorFadeUI;
    
    private int _hashFadeIn = Animator.StringToHash("FadeIn");
    private int _hashFadeOut = Animator.StringToHash("FadeOut");

    private int _maxSceneID;

    private int _currentSceneID = 1;
    private int _creditSceneID;

    private void Awake()
    {
        Instance = this;

        _maxSceneID = SceneManager.sceneCountInBuildSettings;
        _creditSceneID = _maxSceneID - 1;
    }

    private async void Start()
    {
        FadeOut();

        await Task.Delay(1000);
        
        LauchSceneAsync();
    }

    internal async void FinishLevel()
    {
        await StartSwitchScene(_currentSceneID);

        if (_currentSceneID < _creditSceneID) _currentSceneID++;
        
        LauchSceneAsync();
    }
    private async Task StartSwitchScene(int sceneID)
    {
        FadeOut();

        await Task.Delay(1000);

        SceneManager.UnloadSceneAsync(sceneID);
    }

    internal async void SwitchActiveScene(int sceneID)
    {
        await StartSwitchScene(_currentSceneID);

        _currentSceneID = Mathf.Clamp(sceneID, 1, _creditSceneID);

        LauchSceneAsync();
    }

    private void LauchSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(_currentSceneID, LoadSceneMode.Additive);
        operation.allowSceneActivation = true;
        operation.completed += FinishLoading;
    }

    private async void FinishLoading(AsyncOperation operation)
    {
        FadeIn();
        await Task.Delay(600);

        if (_currentSceneID == _creditSceneID) return;
        GameManager.Instance?.StartLevel();
    }

    private void FadeOut()
    {
        _animatorFadeUI.Play(_hashFadeOut);
    }

    private void FadeIn()
    {
        _animatorFadeUI.Play(_hashFadeIn);
    }

    internal void RestartGame()
    {
        SwitchActiveScene(1);
    }
}
