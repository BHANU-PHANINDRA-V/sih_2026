using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TrinetraKavach.Database
{
    public class WorkerEntity
    {
        public int id;
        public string worker_id;
        public string name;
        public int age;
        public string sector;
        public string workplace;
        public string language;
        public string created_at;
    }

    public class AttemptEntity
    {
        public int id;
        public string worker_id;
        public string module_id;
        public string scenario_id;
        public string started_at;
        public string completed_at;
        public int score;
        public bool passed;
    }

    public class AttemptEventEntity
    {
        public int id;
        public int attempt_id;
        public string timestamp;
        public string state;
        public string action;
        public string expected_action;
        public string result;
        public int score_change;
        public string risk_level;
    }

    public class ProgressEntity
    {
        public int id;
        public string worker_id;
        public string module_id;
        public int completion_percentage;
        public int best_score;
        public bool completed;
        public bool certified;
    }

    public class CertificateEntity
    {
        public int id;
        public string worker_id;
        public string module_id;
        public string scenario_id;
        public string certificate_id;
        public string verification_code;
        public int score;
        public string issued_at;
    }

    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }
        public string DbPath { get; private set; }

        public WorkerRepository Workers { get; private set; }
        public AttemptRepository Attempts { get; private set; }
        public AttemptEventRepository AttemptEvents { get; private set; }
        public ProgressRepository Progress { get; private set; }
        public CertificateRepository Certificates { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            DbPath = Path.Combine(Application.persistentDataPath, "trinetra_kavach.db");
            Debug.Log($"[DatabaseManager] Database initialized at: {DbPath}");

            Workers = new WorkerRepository(DbPath);
            Attempts = new AttemptRepository(DbPath);
            AttemptEvents = new AttemptEventRepository(DbPath);
            Progress = new ProgressRepository(DbPath);
            Certificates = new CertificateRepository(DbPath);
        }
    }

    public class WorkerRepository
    {
        private string dbPath;
        private static WorkerEntity currentWorker;

        public WorkerRepository(string path) => dbPath = path;

        public void SaveOrUpdateWorker(WorkerEntity worker)
        {
            currentWorker = worker;
            PlayerPrefs.SetString("LAST_WORKER_ID", worker.worker_id);
            PlayerPrefs.SetString("LAST_WORKER_NAME", worker.name);
            PlayerPrefs.SetString("LAST_WORKER_SECTOR", worker.sector);
            PlayerPrefs.Save();
        }

        public WorkerEntity GetWorker(string workerId)
        {
            if (currentWorker != null && currentWorker.worker_id == workerId) return currentWorker;
            if (PlayerPrefs.HasKey("LAST_WORKER_ID"))
            {
                return new WorkerEntity
                {
                    worker_id = PlayerPrefs.GetString("LAST_WORKER_ID"),
                    name = PlayerPrefs.GetString("LAST_WORKER_NAME"),
                    sector = PlayerPrefs.GetString("LAST_WORKER_SECTOR"),
                    language = "en"
                };
            }
            return null;
        }

        public WorkerEntity GetCurrentWorker()
        {
            if (currentWorker != null) return currentWorker;
            return GetWorker(PlayerPrefs.GetString("LAST_WORKER_ID", "WK-001"));
        }
    }

    public class AttemptRepository
    {
        private string dbPath;
        private static List<AttemptEntity> attempts = new List<AttemptEntity>();
        private static int idCounter = 1;

        public AttemptRepository(string path) => dbPath = path;

        public int RecordAttempt(AttemptEntity attempt)
        {
            attempt.id = idCounter++;
            attempts.Add(attempt);
            return attempt.id;
        }

        public List<AttemptEntity> GetAttempts(string workerId, string moduleId)
        {
            return attempts.FindAll(a => a.worker_id == workerId && a.module_id == moduleId);
        }
    }

    public class AttemptEventRepository
    {
        private string dbPath;
        private static List<AttemptEventEntity> events = new List<AttemptEventEntity>();
        private static int idCounter = 1;

        public AttemptEventRepository(string path) => dbPath = path;

        public void LogEvent(int attemptId, string state, string action, string expected, string result, int scoreChange, string risk)
        {
            events.Add(new AttemptEventEntity
            {
                id = idCounter++,
                attempt_id = attemptId,
                timestamp = DateTime.UtcNow.ToString("o"),
                state = state,
                action = action,
                expected_action = expected,
                result = result,
                score_change = scoreChange,
                risk_level = risk
            });
        }
    }

    public class ProgressRepository
    {
        private string dbPath;
        private static Dictionary<string, ProgressEntity> progressMap = new Dictionary<string, ProgressEntity>();

        public ProgressRepository(string path) => dbPath = path;

        public void UpdateProgress(string workerId, string moduleId, int score, bool passed)
        {
            string key = $"{workerId}_{moduleId}";
            if (!progressMap.TryGetValue(key, out var p))
            {
                p = new ProgressEntity
                {
                    worker_id = workerId,
                    module_id = moduleId,
                    completion_percentage = 100,
                    best_score = score,
                    completed = true,
                    certified = passed
                };
                progressMap[key] = p;
            }
            else
            {
                p.completion_percentage = 100;
                p.completed = true;
                if (score > p.best_score) p.best_score = score;
                if (passed) p.certified = true;
            }
        }

        public ProgressEntity GetProgress(string workerId, string moduleId)
        {
            string key = $"{workerId}_{moduleId}";
            progressMap.TryGetValue(key, out var p);
            return p;
        }
    }

    public class CertificateRepository
    {
        private string dbPath;
        private static List<CertificateEntity> certificates = new List<CertificateEntity>();
        private static int idCounter = 1;

        public CertificateRepository(string path) => dbPath = path;

        public void InsertCertificate(CertificateEntity cert)
        {
            cert.id = idCounter++;
            certificates.Add(cert);
        }

        public List<CertificateEntity> GetWorkerCertificates(string workerId)
        {
            return certificates.FindAll(c => c.worker_id == workerId);
        }

        public CertificateEntity Verify(string certId, string verifyCode)
        {
            return certificates.Find(c => 
                string.Equals(c.certificate_id?.Trim(), certId?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.verification_code?.Trim(), verifyCode?.Trim(), StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}
