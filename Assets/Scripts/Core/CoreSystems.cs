using UnityEngine;
using UnityEngine.SceneManagement;
using TrinetraKavach.Data;
using TrinetraKavach.Database;
using TrinetraKavach.Localization;

namespace TrinetraKavach.Core
{
    public class AppBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeServices()
        {
            Debug.Log("[TRINETRA KAVACH] Bootstrapping Offline Core Services...");

            GameObject root = new GameObject("Trinetra_CoreServices");
            Object.DontDestroyOnLoad(root);

            root.AddComponent<JSONDataManager>();
            root.AddComponent<LocalizationManager>();
            root.AddComponent<DatabaseManager>();
            root.AddComponent<GameManager>();
            root.AddComponent<NavigationManager>();
            root.AddComponent<SettingsManager>();
        }
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public string ActiveModuleId { get; set; } = "MOD_FIRE";
        public string ActiveScenarioId { get; set; } = "electrical_fire";
        public bool IsDevModeEnabled { get; set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            Debug.Log($"[NavigationManager] Loading Scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void GoToLanguageSelection() => LoadScene("LanguageSelection");
        public void GoToLogin() => LoadScene("Login");
        public void GoToDashboard() => LoadScene("MainMenu");
        public void GoToModuleSelection() => LoadScene("ModuleSelection");
        public void GoToInstructions() => LoadScene("Instructions");
        public void GoToARTraining() => LoadScene("ARTraining");
        public void GoToAssessment() => LoadScene("Assessment");
        public void GoToResult() => LoadScene("Result");
        public void GoToCertificate() => LoadScene("Certificate");
        public void GoToCertificatesList() => LoadScene("CertificatesList");
        public void GoToSettings() => LoadScene("Settings");
    }

    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public float MasterVolume { get; set; } = 1.0f;
        public bool HighContrastUI { get; set; } = false;
        public bool ShowFPSCounter { get; set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
