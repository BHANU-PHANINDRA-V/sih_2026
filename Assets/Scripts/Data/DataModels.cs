using System;
using System.Collections.Generic;

namespace TrinetraKavach.Data
{
    [Serializable]
    public class CertificationConfig
    {
        public int pass_threshold = 70;
        public string certificate_prefix = "TK";
        public bool allow_retry = true;
        public bool generate_qr = true;
        public bool allow_export = true;
        public string organization_name = "TRINETRA VOCATIONAL SAFETY COUNCIL";
        public string qr_payload_format = "TRINETRA_KAVACH|{CERT_ID}|{VERIFY_CODE}|{SCORE}|{DATE}";
    }

    [Serializable]
    public class ModuleData
    {
        public string id;
        public int order;
        public string title_key;
        public string desc_key;
        public string sector;
        public string[] scenarios;
        public string icon_resource;
    }

    [Serializable]
    public class ScenarioData
    {
        public string scenario_id;
        public string module_id;
        public string title_key;
        public string initial_state;
        public ScenarioStateData[] states;
    }

    [Serializable]
    public class ScenarioStateData
    {
        public string state_id;
        public string instruction_key;
        public bool hazard_active;
        public string hazard_type;
        public string[] allowed_actions;
        public string next_state;
    }

    [Serializable]
    public class SafetyRuleData
    {
        public string rule_id;
        public string module_id;
        public string action;
        public string context;
        public bool is_correct;
        public int score_change;
        public string risk_level;
        public string feedback_key;
    }

    [Serializable]
    public class QuestionData
    {
        public string question_id;
        public string module_id;
        public string question_key;
        public string[] options_keys;
        public int correct_option_index;
        public int points;
    }

    [Serializable]
    public class PPEItemData
    {
        public string id;
        public string name_key;
        public string category;
        public string protection_level;
        public string icon;
    }
}
