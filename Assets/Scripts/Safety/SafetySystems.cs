using System;
using System.Collections.Generic;
using UnityEngine;
using TrinetraKavach.Data;

namespace TrinetraKavach.Safety
{
    public struct SafetyEvaluationResult
    {
        public bool IsCorrect;
        public int ScoreChange;
        public string RiskLevel;
        public string FeedbackKey;
    }

    public class SafetyEngine : MonoBehaviour
    {
        public static SafetyEngine Instance { get; private set; }

        private SafetyRuleEvaluator ruleEvaluator;
        private RiskEvaluator riskEvaluator;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ruleEvaluator = new SafetyRuleEvaluator();
            riskEvaluator = new RiskEvaluator();
        }

        public SafetyEvaluationResult EvaluateAction(string moduleId, string action, string context)
        {
            return ruleEvaluator.Evaluate(moduleId, action, context);
        }

        public int AssessRiskLevel(string hazardType, float proximityDistance)
        {
            return riskEvaluator.CalculateRiskScore(hazardType, proximityDistance);
        }
    }

    public class SafetyRuleEvaluator
    {
        public SafetyEvaluationResult Evaluate(string moduleId, string action, string context)
        {
            List<SafetyRuleData> rules = JSONDataManager.Instance.SafetyRules;
            SafetyRuleData matched = rules.Find(r => r.module_id == moduleId && r.action == action && r.context == context);

            if (matched != null)
            {
                return new SafetyEvaluationResult
                {
                    IsCorrect = matched.is_correct,
                    ScoreChange = matched.score_change,
                    RiskLevel = matched.risk_level,
                    FeedbackKey = matched.feedback_key
                };
            }

            return new SafetyEvaluationResult
            {
                IsCorrect = true,
                ScoreChange = 5,
                RiskLevel = "SAFE",
                FeedbackKey = "feedback_action_accepted"
            };
        }
    }

    public class RiskEvaluator
    {
        public int CalculateRiskScore(string hazardType, float proximityDistance)
        {
            if (proximityDistance < 1.0f) return 100; // Critical danger
            if (proximityDistance < 2.5f) return 60;  // High danger
            return 20; // Guarded zone
        }
    }
}
