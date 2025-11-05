using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    private const string CurrentLevelIndexKey = "LevelProgress.CurrentLevelIndex";
    private const string HighestUnlockedLevelIndexKey = "LevelProgress.HighestUnlockedLevelIndex";
    private const string CurrentLevelSceneKey = "LevelProgress.CurrentLevelScene";

    public int CurrentLevelIndex { get; private set; }
    public int HighestUnlockedLevelIndex { get; private set; }
    public string CurrentLevelSceneName { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public void SetCurrentLevel(int levelIndex, string sceneName)
    {
        if (levelIndex < 0)
        {
            levelIndex = 0;
        }

        CurrentLevelIndex = levelIndex;
        CurrentLevelSceneName = sceneName;

        if (levelIndex > HighestUnlockedLevelIndex)
        {
            HighestUnlockedLevelIndex = levelIndex;
        }

        SaveProgress();
    }

    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex > HighestUnlockedLevelIndex)
        {
            HighestUnlockedLevelIndex = levelIndex;
            SaveProgress();
        }
    }

    public void SetCurrentLevelScene(string sceneName)
    {
        CurrentLevelSceneName = sceneName;
        SaveProgress();
    }

    public void ResetProgress()
    {
        CurrentLevelIndex = 0;
        HighestUnlockedLevelIndex = 0;
        CurrentLevelSceneName = string.Empty;
        SaveProgress();
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(CurrentLevelIndexKey, CurrentLevelIndex);
        PlayerPrefs.SetInt(HighestUnlockedLevelIndexKey, HighestUnlockedLevelIndex);
        PlayerPrefs.SetString(CurrentLevelSceneKey, CurrentLevelSceneName ?? string.Empty);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        CurrentLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexKey, 0);
        HighestUnlockedLevelIndex = PlayerPrefs.GetInt(HighestUnlockedLevelIndexKey, 0);
        CurrentLevelSceneName = PlayerPrefs.GetString(CurrentLevelSceneKey, string.Empty);
    }
}
