using System;
using System.Collections.Generic;
using UnityEngine;
using TrinetraKavach.Data;

namespace TrinetraKavach.Assessment
{
    public class AssessmentResult
    {
        public string ModuleId;
        public string ScenarioId;
        public int Score;
        public bool Passed;
        public List<string> Mistakes = new List<string>();
        public DateTime StartTime;
    }

    public class AssessmentEngine : MonoBehaviour
    {
        public static AssessmentEngine Instance { get; private set; }
        public ScoreManager ScoreManager { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ScoreManager = new ScoreManager();
        }

        public int CalculateWeightedScore(int practicalScore, int knowledgeScore, int evacuationScore)
        {
            // Knowledge: 20%, Practical: 60%, Evacuation: 20%
            float finalScore = (knowledgeScore * 0.20f) + (practicalScore * 0.60f) + (evacuationScore * 0.20f);
            return Mathf.Clamp(Mathf.RoundToInt(finalScore), 0, 100);
        }
    }

    public class ScoreManager
    {
        public int CurrentScore { get; private set; } = 100;

        public void ResetScore() => CurrentScore = 100;

        public void ApplyDelta(int delta)
        {
            CurrentScore = Mathf.Clamp(CurrentScore + delta, 0, 100);
        }
    }
}
