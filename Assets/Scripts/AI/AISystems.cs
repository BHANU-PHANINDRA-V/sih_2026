using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrinetraKavach.AI
{
    public interface IObjectDetector
    {
        void DetectObjects(Texture2D cameraTexture, Action<List<DetectedSafetyObject>> onResult);
    }

    [Serializable]
    public class DetectedSafetyObject
    {
        public string ClassName;
        public float Confidence;
        public Rect BoundingBox;
    }

    public class ObjectDetectionManager : MonoBehaviour
    {
        public static ObjectDetectionManager Instance { get; private set; }
        private IObjectDetector activeDetector;

        private void Awake()
        {
            Instance = this;
            InitializeDetector();
        }

        private void InitializeDetector()
        {
            TextAsset modelAsset = Resources.Load<TextAsset>("Models/YOLO/yolo");
            if (modelAsset != null)
            {
                activeDetector = gameObject.AddComponent<SentisInferenceManager>();
            }
            else
            {
                Debug.Log("[ObjectDetectionManager] Using MockObjectDetector for development.");
                activeDetector = new MockObjectDetector();
            }
        }

        public void Detect(Texture2D frame, Action<List<DetectedSafetyObject>> callback)
        {
            activeDetector?.DetectObjects(frame, callback);
        }
    }

    public class YOLODetector : MonoBehaviour
    {
        public float ConfidenceThreshold = 0.5f;
    }

    public class MockObjectDetector : IObjectDetector
    {
        public void DetectObjects(Texture2D cameraTexture, Action<List<DetectedSafetyObject>> onResult)
        {
            var detections = new List<DetectedSafetyObject>
            {
                new DetectedSafetyObject { ClassName = "fire_extinguisher", Confidence = 0.94f, BoundingBox = new Rect(0.2f, 0.2f, 0.4f, 0.4f) },
                new DetectedSafetyObject { ClassName = "electrical_panel", Confidence = 0.89f, BoundingBox = new Rect(0.6f, 0.2f, 0.3f, 0.5f) },
                new DetectedSafetyObject { ClassName = "helmet", Confidence = 0.97f, BoundingBox = new Rect(0.4f, 0.1f, 0.2f, 0.2f) }
            };
            onResult?.Invoke(detections);
        }
    }

    public class SentisInferenceManager : MonoBehaviour, IObjectDetector
    {
        public void DetectObjects(Texture2D cameraTexture, Action<List<DetectedSafetyObject>> onResult)
        {
            var results = new List<DetectedSafetyObject>();
            onResult?.Invoke(results);
        }
    }
}
