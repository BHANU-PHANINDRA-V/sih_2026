using System;
using UnityEngine;
using TrinetraKavach.Data;
using TrinetraKavach.Database;
using TrinetraKavach.Assessment;

namespace TrinetraKavach.Certification
{
    public class CertificationResult
    {
        public bool IsPassed;
        public int FinalScore;
        public int PassThreshold;
        public CertificateEntity Certificate;
        public string StatusMessage;
    }

    public class CertificationManager : MonoBehaviour
    {
        public static CertificationManager Instance { get; private set; }

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

        public bool IsPassed(int score, int threshold)
        {
            return score >= threshold;
        }

        public CertificationResult EvaluateCertification(AssessmentResult assessmentResult)
        {
            var config = JSONDataManager.Instance.CertificationConfig;
            int threshold = config != null ? config.pass_threshold : 70;

            bool passed = IsPassed(assessmentResult.Score, threshold);
            assessmentResult.Passed = passed;

            CertificationResult certResult = new CertificationResult
            {
                IsPassed = passed,
                FinalScore = assessmentResult.Score,
                PassThreshold = threshold
            };

            var worker = DatabaseManager.Instance.Workers.GetCurrentWorker();
            string workerId = worker != null ? worker.worker_id : "WK-TEMP";

            // Record attempt in SQLite
            int attemptId = DatabaseManager.Instance.Attempts.RecordAttempt(new AttemptEntity
            {
                worker_id = workerId,
                module_id = assessmentResult.ModuleId,
                scenario_id = assessmentResult.ScenarioId,
                started_at = assessmentResult.StartTime.ToString("o"),
                completed_at = DateTime.UtcNow.ToString("o"),
                score = assessmentResult.Score,
                passed = passed
            });

            if (passed)
            {
                Debug.Log($"[CertificationManager] Assessment PASSED ({assessmentResult.Score} >= {threshold}). Generating Certificate.");

                CertificateEntity cert = CertificateManager.Instance.IssueCertificate(
                    workerId,
                    assessmentResult.ModuleId,
                    assessmentResult.ScenarioId,
                    assessmentResult.Score
                );

                DatabaseManager.Instance.Progress.UpdateProgress(workerId, assessmentResult.ModuleId, assessmentResult.Score, true);

                certResult.Certificate = cert;
                certResult.StatusMessage = "PASS: Digital Safety Certificate Issued";
            }
            else
            {
                Debug.LogWarning($"[CertificationManager] Assessment FAILED ({assessmentResult.Score} < {threshold}). Certificate Withheld.");

                DatabaseManager.Instance.Progress.UpdateProgress(workerId, assessmentResult.ModuleId, assessmentResult.Score, false);

                certResult.Certificate = null;
                certResult.StatusMessage = "FAIL: Safety Standard Not Met. Retry Required.";
            }

            return certResult;
        }
    }

    public class CertificateManager : MonoBehaviour
    {
        public static CertificateManager Instance { get; private set; }

        private CertificateIdGenerator idGen = new CertificateIdGenerator();
        private VerificationCodeGenerator codeGen = new VerificationCodeGenerator();

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

        public CertificateEntity IssueCertificate(string workerId, string moduleId, string scenarioId, int score)
        {
            string prefix = JSONDataManager.Instance.CertificationConfig?.certificate_prefix ?? "TK";
            string certId = idGen.GenerateUniqueId(prefix, moduleId);
            string verifyCode = codeGen.GenerateVerificationCode();
            string issuedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            CertificateEntity cert = new CertificateEntity
            {
                worker_id = workerId,
                module_id = moduleId,
                scenario_id = scenarioId,
                certificate_id = certId,
                verification_code = verifyCode,
                score = score,
                issued_at = issuedAt
            };

            DatabaseManager.Instance.Certificates.InsertCertificate(cert);
            return cert;
        }
    }

    public class CertificateIdGenerator
    {
        public string GenerateUniqueId(string prefix, string moduleId)
        {
            string cleanMod = moduleId.Replace("MOD_", "");
            int year = DateTime.UtcNow.Year;
            int randomSeq = UnityEngine.Random.Range(100000, 999999);
            return $"{prefix}-{cleanMod}-{year}-{randomSeq}";
        }
    }

    public class VerificationCodeGenerator
    {
        public string GenerateVerificationCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }
    }

    public class CertificateVerificationManager
    {
        public bool VerifyLocalCertificate(string certId, string verifyCode, out CertificateEntity cert)
        {
            cert = DatabaseManager.Instance.Certificates.Verify(certId, verifyCode);
            return cert != null;
        }
    }

    public interface IQRCodeGenerator
    {
        Texture2D GenerateQRCode(string payload, int width, int height);
    }

    public class QRCodeGenerator : MonoBehaviour, IQRCodeGenerator
    {
        public static QRCodeGenerator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Texture2D GenerateQRCode(string payload, int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color dark = Color.black;
            Color light = Color.white;

            int hash = payload.GetHashCode();
            System.Random rnd = new System.Random(hash);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsFinder(x, y, width, height))
                    {
                        tex.SetPixel(x, y, dark);
                    }
                    else if (IsFinderBorder(x, y, width, height))
                    {
                        tex.SetPixel(x, y, light);
                    }
                    else
                    {
                        bool bit = (rnd.Next(0, 100) % 2 == 0);
                        tex.SetPixel(x, y, bit ? dark : light);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private bool IsFinder(int x, int y, int w, int h)
        {
            int s = 28;
            return (x < s && y < s) || (x > w - s && y < s) || (x < s && y > h - s);
        }

        private bool IsFinderBorder(int x, int y, int w, int h)
        {
            int s = 38;
            return ((x < s && y < s) || (x > w - s && y < s) || (x < s && y > h - s)) && !IsFinder(x, y, w, h);
        }
    }
}
