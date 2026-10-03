using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TrinetraKavach.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public string CurrentLanguage { get; private set; } = "en"; // "en", "hi", "sat"
        private Dictionary<string, string> localizedStrings = new Dictionary<string, string>();

        public event Action OnLanguageChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(CurrentLanguage);
        }

        public void SetLanguage(string langCode)
        {
            if (CurrentLanguage == langCode && localizedStrings.Count > 0) return;
            CurrentLanguage = langCode;
            LoadLanguage(langCode);
            OnLanguageChanged?.Invoke();
        }

        private void LoadLanguage(string langCode)
        {
            localizedStrings.Clear();
            string jsonText = null;

            TextAsset textAsset = Resources.Load<TextAsset>($"Data/Localization/{langCode}");
            if (textAsset != null)
            {
                jsonText = textAsset.text;
            }
            else
            {
                string streamPath = Path.Combine(Application.streamingAssetsPath, $"Data/Localization/{langCode}.json");
                if (File.Exists(streamPath))
                {
                    jsonText = File.ReadAllText(streamPath);
                }
            }

            if (string.IsNullOrEmpty(jsonText) && langCode != "en")
            {
                Debug.LogWarning($"[LocalizationManager] Language file {langCode} missing, falling back to en");
                LoadLanguage("en");
                return;
            }

            if (!string.IsNullOrEmpty(jsonText))
            {
                ParseJsonIntoDict(jsonText);
            }

            Debug.Log($"[LocalizationManager] Loaded {localizedStrings.Count} localized keys for language '{langCode}'");
        }

        private void ParseJsonIntoDict(string json)
        {
            string trimmed = json.Trim();
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
                string[] lines = trimmed.Split(new[] { ",\n", ",\r\n", "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    int colonIdx = line.IndexOf(':');
                    if (colonIdx > 0)
                    {
                        string k = line.Substring(0, colonIdx).Trim().Trim('"');
                        string v = line.Substring(colonIdx + 1).Trim().Trim('"');
                        localizedStrings[k] = v;
                    }
                }
            }
        }

        public static string Get(string key)
        {
            if (Instance == null) return key;
            if (Instance.localizedStrings.TryGetValue(key, out string val))
            {
                return val;
            }
            return key;
        }
    }
}
