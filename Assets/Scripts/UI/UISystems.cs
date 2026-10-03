using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TrinetraKavach.Core;
using TrinetraKavach.Localization;
using TrinetraKavach.Database;
using TrinetraKavach.Data;
using TrinetraKavach.Certification;
using TrinetraKavach.Training;

namespace TrinetraKavach.UI
{
    public class LanguageSelectionUI : MonoBehaviour
    {
        [SerializeField] private Button btnEnglish;
        [SerializeField] private Button btnHindi;
        [SerializeField] private Button btnSantali;

        private void Start()
        {
            btnEnglish?.onClick.AddListener(() => SelectLanguage("en"));
            btnHindi?.onClick.AddListener(() => SelectLanguage("hi"));
            btnSantali?.onClick.AddListener(() => SelectLanguage("sat"));
        }

        private void SelectLanguage(string code)
        {
            LocalizationManager.Instance.SetLanguage(code);
            NavigationManager.Instance.GoToLogin();
        }
    }

    public class LoginUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputWorkerName;
        [SerializeField] private TMP_InputField inputWorkerId;
        [SerializeField] private TMP_Dropdown dropdownSector;
        [SerializeField] private Button btnLogin;

        private void Start()
        {
            btnLogin?.onClick.AddListener(OnLoginClicked);
        }

        private void OnLoginClicked()
        {
            string name = string.IsNullOrEmpty(inputWorkerName?.text) ? "Bhanu Trainee" : inputWorkerName.text;
            string id = string.IsNullOrEmpty(inputWorkerId?.text) ? "WK-2026-01" : inputWorkerId.text;
            string sector = dropdownSector != null ? dropdownSector.options[dropdownSector.value].text : "Mining";

            WorkerEntity worker = new WorkerEntity
            {
                worker_id = id,
                name = name,
                sector = sector,
                language = LocalizationManager.Instance.CurrentLanguage,
                created_at = DateTime.UtcNow.ToString("o")
            };

            DatabaseManager.Instance.Workers.SaveOrUpdateWorker(worker);
            NavigationManager.Instance.GoToDashboard();
        }
    }

    public class DashboardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtWorkerInfo;
        [SerializeField] private TextMeshProUGUI txtFireStatus;
        [SerializeField] private TextMeshProUGUI txtGasStatus;
        [SerializeField] private Button btnContinueTraining;
        [SerializeField] private Button btnModules;
        [SerializeField] private Button btnCertificates;
        [SerializeField] private Button btnSettings;

        private void Start()
        {
            btnContinueTraining?.onClick.AddListener(() => NavigationManager.Instance.GoToARTraining());
            btnModules?.onClick.AddListener(() => NavigationManager.Instance.GoToModuleSelection());
            btnCertificates?.onClick.AddListener(() => NavigationManager.Instance.GoToCertificatesList());
            btnSettings?.onClick.AddListener(() => NavigationManager.Instance.GoToSettings());

            RefreshDashboard();
        }

        public void RefreshDashboard()
        {
            var worker = DatabaseManager.Instance.Workers.GetCurrentWorker();
            if (worker != null && txtWorkerInfo != null)
            {
                txtWorkerInfo.text = $"{worker.name} ({worker.worker_id}) - {worker.sector}";
            }

            if (txtFireStatus != null)
            {
                var p = DatabaseManager.Instance.Progress.GetProgress(worker?.worker_id ?? "WK-001", "MOD_FIRE");
                txtFireStatus.text = p != null && p.certified ? LocalizationManager.Get("certified") : LocalizationManager.Get("not_certified");
                txtFireStatus.color = p != null && p.certified ? Color.green : Color.yellow;
            }
        }
    }

    public class ModuleSelectionUI : MonoBehaviour
    {
        [SerializeField] private Button btnFireModule;
        [SerializeField] private Button btnGasModule;
        [SerializeField] private Button btnBack;

        private void Start()
        {
            btnFireModule?.onClick.AddListener(() => SelectModule("MOD_FIRE", "electrical_fire"));
            btnGasModule?.onClick.AddListener(() => SelectModule("MOD_GAS", "gas_leak"));
            btnBack?.onClick.AddListener(() => NavigationManager.Instance.GoToDashboard());
        }

        private void SelectModule(string modId, string scenarioId)
        {
            GameManager.Instance.ActiveModuleId = modId;
            GameManager.Instance.ActiveScenarioId = scenarioId;
            NavigationManager.Instance.GoToInstructions();
        }
    }

    public class InstructionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtInstructions;
        [SerializeField] private Button btnStartAR;
        [SerializeField] private Button btnBack;

        private void Start()
        {
            btnStartAR?.onClick.AddListener(() => NavigationManager.Instance.GoToARTraining());
            btnBack?.onClick.AddListener(() => NavigationManager.Instance.GoToModuleSelection());

            string modId = GameManager.Instance.ActiveModuleId;
            if (txtTitle != null)
            {
                txtTitle.text = LocalizationManager.Get(modId == "MOD_FIRE" ? "mod_fire_title" : "mod_gas_title");
            }
        }
    }

    public class TrainingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtInstruction;
        [SerializeField] private TextMeshProUGUI txtScore;
        [SerializeField] private Button btnAction1;
        [SerializeField] private Button btnAction2;
        [SerializeField] private ScenarioStateMachine stateMachine;

        private void Start()
        {
            if (stateMachine != null)
            {
                stateMachine.OnScoreUpdated += (score, feedback) =>
                {
                    if (txtScore != null) txtScore.text = $"SCORE: {score}";
                    if (txtInstruction != null) txtInstruction.text = LocalizationManager.Get(feedback);
                };
            }
        }
    }

    public class AssessmentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtQuestion;
        [SerializeField] private Button[] optionButtons;

        public void DisplayQuestion(QuestionData q)
        {
            if (txtQuestion != null) txtQuestion.text = LocalizationManager.Get(q.question_key);
        }
    }

    public class ResultUI : MonoBehaviour
    {
        public static ResultUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtFinalScore;
        [SerializeField] private TextMeshProUGUI txtPassMark;
        [SerializeField] private TextMeshProUGUI txtStatus;
        [SerializeField] private TextMeshProUGUI txtMistakesList;

        [SerializeField] private Button btnViewCertificate;
        [SerializeField] private Button btnRetryTraining;
        [SerializeField] private Button btnBackDashboard;

        private CertificationResult cachedResult;

        private void Awake()
        {
            Instance = this;
            btnViewCertificate?.onClick.AddListener(OnViewCertificateClicked);
            btnRetryTraining?.onClick.AddListener(OnRetryClicked);
            btnBackDashboard?.onClick.AddListener(OnDashboardClicked);
        }

        public void DisplayResult(CertificationResult result, List<string> mistakes)
        {
            cachedResult = result;
            gameObject.SetActive(true);

            if (txtFinalScore != null) txtFinalScore.text = $"{LocalizationManager.Get("final_score")}: {result.FinalScore} / 100";
            if (txtPassMark != null) txtPassMark.text = $"{LocalizationManager.Get("pass_mark")}: {result.PassThreshold} / 100";

            if (result.IsPassed)
            {
                if (txtTitle != null) { txtTitle.text = "CONGRATULATIONS"; txtTitle.color = Color.green; }
                if (txtStatus != null) { txtStatus.text = LocalizationManager.Get("status_passed"); txtStatus.color = Color.green; }
                if (btnViewCertificate != null) btnViewCertificate.gameObject.SetActive(true);
                if (txtMistakesList != null) txtMistakesList.text = "All industrial safety protocols observed safely.";
            }
            else
            {
                if (txtTitle != null) { txtTitle.text = "ASSESSMENT NOT PASSED"; txtTitle.color = Color.red; }
                if (txtStatus != null) { txtStatus.text = LocalizationManager.Get("status_failed"); txtStatus.color = Color.red; }
                if (btnViewCertificate != null) btnViewCertificate.gameObject.SetActive(false);

                string mText = "CRITICAL SAFETY MISTAKES:\n";
                foreach (var m in mistakes) mText += $"• {m}\n";
                if (txtMistakesList != null) txtMistakesList.text = mText;
            }
        }

        private void OnViewCertificateClicked()
        {
            NavigationManager.Instance.GoToCertificate();
        }

        private void OnRetryClicked()
        {
            NavigationManager.Instance.GoToARTraining();
        }

        private void OnDashboardClicked()
        {
            NavigationManager.Instance.GoToDashboard();
        }
    }

    public class CertificateUI : MonoBehaviour
    {
        public static CertificateUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI txtWorkerName;
        [SerializeField] private TextMeshProUGUI txtWorkerId;
        [SerializeField] private TextMeshProUGUI txtSector;
        [SerializeField] private TextMeshProUGUI txtModuleName;
        [SerializeField] private TextMeshProUGUI txtScore;
        [SerializeField] private TextMeshProUGUI txtCertId;
        [SerializeField] private TextMeshProUGUI txtVerificationCode;
        [SerializeField] private TextMeshProUGUI txtIssueDate;
        [SerializeField] private RawImage imgQRCode;

        [SerializeField] private Button btnExportPNG;
        [SerializeField] private Button btnBack;

        private CertificateEntity currentCert;

        private void Awake()
        {
            Instance = this;
            btnExportPNG?.onClick.AddListener(ExportCertificate);
            btnBack?.onClick.AddListener(() => NavigationManager.Instance.GoToDashboard());
        }

        public void DisplayCertificate(CertificateEntity cert)
        {
            currentCert = cert;
            var worker = DatabaseManager.Instance.Workers.GetCurrentWorker();

            if (txtWorkerName != null) txtWorkerName.text = worker?.name ?? "Trainee";
            if (txtWorkerId != null) txtWorkerId.text = $"WORKER ID: {cert.worker_id}";
            if (txtSector != null) txtSector.text = $"SECTOR: {worker?.sector ?? "Mining"}";
            if (txtModuleName != null) txtModuleName.text = $"MODULE: {cert.module_id}";
            if (txtScore != null) txtScore.text = $"FINAL SCORE: {cert.score} / 100 (PASSED)";
            if (txtCertId != null) txtCertId.text = $"CERTIFICATE ID: {cert.certificate_id}";
            if (txtVerificationCode != null) txtVerificationCode.text = $"VERIFICATION CODE: {cert.verification_code}";
            if (txtIssueDate != null) txtIssueDate.text = $"ISSUED: {cert.issued_at}";

            if (imgQRCode != null && QRCodeGenerator.Instance != null)
            {
                string payload = $"TRINETRA_KAVACH|{cert.certificate_id}|{cert.verification_code}";
                imgQRCode.texture = QRCodeGenerator.Instance.GenerateQRCode(payload, 256, 256);
            }
        }

        private void ExportCertificate()
        {
            string filename = $"Cert_{currentCert?.certificate_id ?? "TK"}.png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"[CertificateUI] Saved certificate screenshot {filename}");
        }
    }

    public class CertificatesListUI : MonoBehaviour
    {
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button btnBack;

        private void Start()
        {
            btnBack?.onClick.AddListener(() => NavigationManager.Instance.GoToDashboard());
            LoadCertificates();
        }

        private void LoadCertificates()
        {
            var worker = DatabaseManager.Instance.Workers.GetCurrentWorker();
            if (worker != null)
            {
                var certs = DatabaseManager.Instance.Certificates.GetWorkerCertificates(worker.worker_id);
                Debug.Log($"[CertificatesListUI] Found {certs.Count} earned certificates for worker.");
            }
        }
    }

    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider sliderVolume;
        [SerializeField] private Toggle toggleHighContrast;
        [SerializeField] private Button btnBack;

        private void Start()
        {
            btnBack?.onClick.AddListener(() => NavigationManager.Instance.GoToDashboard());
        }
    }
}