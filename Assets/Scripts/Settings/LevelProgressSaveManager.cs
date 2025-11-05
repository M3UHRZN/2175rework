using System.Collections.Generic;
using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// Handles level progression and unlock status for the game.
    /// Maintains which levels are unlocked and persists them using PlayerPrefs.
    /// </summary>
    public class LevelProgressSaveManager : MonoBehaviour
    {
        private const string UnlockedLevelsKey = "LevelProgress.UnlockedLevels";
        private const string CompletedLevelsKey = "LevelProgress.CompletedLevels";

        public static LevelProgressSaveManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("İlk seviye otomatik olarak açık mı olsun?")]
        [SerializeField] private bool unlockFirstLevel = true;

        [Tooltip("İlk seviye adı (otomatik unlock için)")]
        [SerializeField] private string firstLevelName = "";

        private HashSet<string> unlockedLevels = new();
        private HashSet<string> completedLevels = new();

        /// <summary>
        /// Belirli bir seviyenin açık olup olmadığını kontrol eder.
        /// </summary>
        public bool IsLevelUnlocked(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            return unlockedLevels.Contains(sceneName);
        }

        /// <summary>
        /// Belirli bir seviyenin tamamlanıp tamamlanmadığını kontrol eder.
        /// </summary>
        public bool IsLevelCompleted(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            return completedLevels.Contains(sceneName);
        }

        /// <summary>
        /// Bir seviyeyi açık hale getirir.
        /// </summary>
        public void UnlockLevel(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (unlockedLevels.Add(sceneName))
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Bir seviyeyi tamamlandı olarak işaretler ve bir sonraki seviyeyi açar.
        /// </summary>
        public void CompleteLevel(string sceneName, string nextSceneName = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // Mevcut seviyeyi tamamlandı olarak işaretle
            if (completedLevels.Add(sceneName))
            {
                SaveProgress();
            }

            // Mevcut seviyeyi açık hale getir (eğer değilse)
            UnlockLevel(sceneName);

            // Bir sonraki seviyeyi aç
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                UnlockLevel(nextSceneName);
            }
        }

        /// <summary>
        /// Tüm seviyeleri açık hale getirir (debug/test için).
        /// </summary>
        public void UnlockAllLevels()
        {
            unlockedLevels.Clear();
            completedLevels.Clear();
            SaveProgress();
        }

        /// <summary>
        /// Tüm ilerlemeyi sıfırlar.
        /// </summary>
        public void ResetProgress()
        {
            unlockedLevels.Clear();
            completedLevels.Clear();
            PlayerPrefs.DeleteKey(UnlockedLevelsKey);
            PlayerPrefs.DeleteKey(CompletedLevelsKey);
            PlayerPrefs.Save();

            if (unlockFirstLevel && !string.IsNullOrEmpty(firstLevelName))
            {
                UnlockLevel(firstLevelName);
            }
        }

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

            // İlk seviyeyi otomatik aç
            if (unlockFirstLevel && !string.IsNullOrEmpty(firstLevelName))
            {
                if (!IsLevelUnlocked(firstLevelName))
                {
                    UnlockLevel(firstLevelName);
                }
            }
        }

        private void LoadProgress()
        {
            unlockedLevels.Clear();
            completedLevels.Clear();

            // Unlocked levels
            string unlockedData = PlayerPrefs.GetString(UnlockedLevelsKey, "");
            if (!string.IsNullOrEmpty(unlockedData))
            {
                string[] levels = unlockedData.Split(',');
                foreach (var level in levels)
                {
                    if (!string.IsNullOrEmpty(level))
                    {
                        unlockedLevels.Add(level.Trim());
                    }
                }
            }

            // Completed levels
            string completedData = PlayerPrefs.GetString(CompletedLevelsKey, "");
            if (!string.IsNullOrEmpty(completedData))
            {
                string[] levels = completedData.Split(',');
                foreach (var level in levels)
                {
                    if (!string.IsNullOrEmpty(level))
                    {
                        completedLevels.Add(level.Trim());
                    }
                }
            }
        }

        private void SaveProgress()
        {
            // Unlocked levels
            string unlockedData = string.Join(",", unlockedLevels);
            PlayerPrefs.SetString(UnlockedLevelsKey, unlockedData);

            // Completed levels
            string completedData = string.Join(",", completedLevels);
            PlayerPrefs.SetString(CompletedLevelsKey, completedData);

            PlayerPrefs.Save();
        }
    }
}
