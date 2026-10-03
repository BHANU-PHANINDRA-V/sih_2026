using System;
using System.Collections.Generic;
using UnityEngine;
using TrinetraKavach.Data;
using TrinetraKavach.Safety;
using TrinetraKavach.Assessment;
using TrinetraKavach.Certification;

namespace TrinetraKavach.Training
{
    public class ModuleManager : MonoBehaviour
    {
        public static ModuleManager Instance { get; private set; }

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

        public List<ModuleData> GetAllModules()
        {
            return JSONDataManager.Instance.Modules;
        }

        public ModuleData GetModule(string moduleId)
        {
            return JSONDataManager.Instance.Modules.Find(m => m.id == moduleId);
        }
    }

    public class ScenarioManager : MonoBehaviour
    {
        public static ScenarioManager Instance { get; private set; }

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

        public ScenarioData LoadScenario(string moduleId, string scenarioId)
        {
            return JSONDataManager.Instance.LoadScenario(moduleId, scenarioId);
        }
    }

    public class ScenarioStateMachine : MonoBehaviour
    {
        public string CurrentStateId { get; private set; }
        public ScenarioData ActiveScenario { get; private set; }
        public int RunningScore { get; private set; } = 100;
        public List<string> MistakesMade { get; private set; } = new List<string>();
        public DateTime StartTime { get; private set; }

        public event Action<string> OnStateChanged;
        public event Action<int, string> OnScoreUpdated;

        public void InitializeScenario(ScenarioData data)
        {
            ActiveScenario = data;
            RunningScore = 100;
            MistakesMade.Clear();
            StartTime = DateTime.UtcNow;
            TransitionTo(data.initial_state);
        }

        public void TransitionTo(string nextState)
        {
            CurrentStateId = nextState;
            Debug.Log($"[ScenarioFSM] State transitioned to: {CurrentStateId}");
            OnStateChanged?.Invoke(CurrentStateId);

            if (CurrentStateId == "ASSESSMENT")
            {
                TriggerAssessmentPhase();
            }
        }

        public void SubmitWorkerAction(string actionId)
        {
            if (ActiveScenario == null) return;
            ScenarioStateData state = Array.Find(ActiveScenario.states, s => s.state_id == CurrentStateId);
            if (state == null) return;

            string context = state.hazard_type ?? "GENERAL";
            var eval = SafetyEngine.Instance.EvaluateAction(ActiveScenario.module_id, actionId, context);

            RunningScore = Mathf.Clamp(RunningScore + eval.ScoreChange, 0, 100);
            OnScoreUpdated?.Invoke(RunningScore, eval.FeedbackKey);

            if (!eval.IsCorrect)
            {
                MistakesMade.Add($"{CurrentStateId}: {eval.FeedbackKey} ({eval.RiskLevel})");
            }

            if (!string.IsNullOrEmpty(state.next_state))
            {
                TransitionTo(state.next_state);
            }
        }

        private void TriggerAssessmentPhase()
        {
            var assessment = new AssessmentResult
            {
                ModuleId = ActiveScenario.module_id,
                ScenarioId = ActiveScenario.scenario_id,
                Score = RunningScore,
                Mistakes = MistakesMade,
                StartTime = StartTime
            };

            CertificationResult result = CertificationManager.Instance.EvaluateCertification(assessment);
            Debug.Log($"[ScenarioFSM] Certification Outcome: Passed={result.IsPassed}, FinalScore={result.FinalScore}");
        }
    }
}
