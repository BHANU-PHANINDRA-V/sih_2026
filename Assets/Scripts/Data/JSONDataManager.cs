using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TrinetraKavach.Data
{
    public class JSONDataManager : MonoBehaviour
    {
        public static JSONDataManager Instance { get; private set; }

        public CertificationConfig CertificationConfig { get; private set; }
        public List<ModuleData> Modules { get; private set; } = new List<ModuleData>();
        public List<SafetyRuleData> SafetyRules { get; private set; } = new List<SafetyRuleData>();
        public List<QuestionData> Questions { get; private set; } = new List<QuestionData>();
        public List<PPEItemData> PPEItems { get; private set; } = new List<PPEItemData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllConfigurationData();
        }

        public void LoadAllConfigurationData()
        {
            // 1. Certification Config
            string certJson = LoadJsonString("Data/Certification/certification_config");
            if (!string.IsNullOrEmpty(certJson))
            {
                CertificationConfig = JsonUtility.FromJson<CertificationConfig>(certJson);
            }
            else
            {
                CertificationConfig = new CertificationConfig();
            }

            // 2. Modules
            string modJson = LoadJsonString("Data/Modules/modules");
            if (!string.IsNullOrEmpty(modJson))
            {
                Modules = JsonHelper.FromJsonList<ModuleData>(modJson);
            }

            // 3. Safety Rules
            string rulesJson = LoadJsonString("Data/Rules/safety_rules");
            if (!string.IsNullOrEmpty(rulesJson))
            {
                SafetyRules = JsonHelper.FromJsonList<SafetyRuleData>(rulesJson);
            }

            // 4. Questions
            string qJson = LoadJsonString("Data/Questions/questions");
            if (!string.IsNullOrEmpty(qJson))
            {
                Questions = JsonHelper.FromJsonList<QuestionData>(qJson);
            }

            // 5. PPE Items
            string ppeJson = LoadJsonString("Data/PPE/ppe_items");
            if (!string.IsNullOrEmpty(ppeJson))
            {
                PPEItems = JsonHelper.FromJsonList<PPEItemData>(ppeJson);
            }

            Debug.Log($"[JSONDataManager] Loaded {Modules.Count} modules, {SafetyRules.Count} rules, {Questions.Count} questions.");
        }

        public ScenarioData LoadScenario(string moduleId, string scenarioId)
        {
            string shortMod = moduleId.Replace("MOD_", "");
            // Capitalized first letter for folder structure (Fire, Gas, Machinery, PPE, Emergency)
            string folderName = char.ToUpper(shortMod[0]) + shortMod.Substring(1).ToLower();
            string resourcePath = $"Data/Scenarios/{folderName}/{scenarioId}";

            string json = LoadJsonString(resourcePath);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<ScenarioData>(json);
            }

            Debug.LogError($"[JSONDataManager] Could not find scenario JSON at {resourcePath}");
            return null;
        }

        private string LoadJsonString(string pathWithoutExtension)
        {
            // First check Resources
            TextAsset asset = Resources.Load<TextAsset>(pathWithoutExtension);
            if (asset != null) return asset.text;

            // Fallback check StreamingAssets
            string streamPath = Path.Combine(Application.streamingAssetsPath, pathWithoutExtension + ".json");
            if (File.Exists(streamPath))
            {
                return File.ReadAllText(streamPath);
            }

            return null;
        }
    }

    public static class JsonHelper
    {
        public static List<T> FromJsonList<T>(string json)
        {
            string wrapper = "{\"Items\":" + json + "}";
            Wrapper<T> wrapped = JsonUtility.FromJson<Wrapper<T>>(wrapper);
            return wrapped != null && wrapped.Items != null ? new List<T>(wrapped.Items) : new List<T>();
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
