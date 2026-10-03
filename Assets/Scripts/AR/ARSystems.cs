using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace TrinetraKavach.AR
{
    public class ARManager : MonoBehaviour
    {
        public static ARManager Instance { get; private set; }

        public ARSession ARSession;
        public ARPlacementManager PlacementManager;
        public ARHazardManager HazardManager;
        public ARWaypointRenderer WaypointRenderer;
        public ARInstructionRenderer InstructionRenderer;

        private void Awake() => Instance = this;
    }

    public class ARPlacementManager : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private GameObject scenarioAnchorPrefab;

        public bool IsPlaced { get; private set; } = false;
        private GameObject spawnedAnchor;
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

        public event Action<GameObject> OnPlaced;

        private void Update()
        {
            if (IsPlaced) return;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Touch touch = Input.GetTouch(0);
                if (raycastManager != null && raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    PlaceEnvironment(hitPose.position, hitPose.rotation);
                }
                else
                {
                    // Fallback raycast for desktop editor testing
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        PlaceEnvironment(hit.point, Quaternion.identity);
                    }
                }
            }
        }

        public void PlaceEnvironment(Vector3 pos, Quaternion rot)
        {
            if (scenarioAnchorPrefab != null)
            {
                spawnedAnchor = Instantiate(scenarioAnchorPrefab, pos, rot);
            }
            IsPlaced = true;
            SetPlaneDetection(false);
            OnPlaced?.Invoke(spawnedAnchor);
        }

        public void SetPlaneDetection(bool enabled)
        {
            if (planeManager != null)
            {
                planeManager.enabled = enabled;
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(enabled);
                }
            }
        }
    }

    public class ARHazardManager : MonoBehaviour
    {
        [SerializeField] private GameObject fireHazardPrefab;
        [SerializeField] private GameObject gasHazardPrefab;

        public void SpawnHazard(string hazardType, Vector3 position)
        {
            GameObject prefab = hazardType.Contains("FIRE") ? fireHazardPrefab : gasHazardPrefab;
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }
    }

    public class ARWaypointRenderer : MonoBehaviour
    {
        [SerializeField] private GameObject waypointPrefab;

        public void PlaceWaypoint(Vector3 position, string label)
        {
            if (waypointPrefab != null)
            {
                GameObject wp = Instantiate(waypointPrefab, position, Quaternion.identity);
                wp.name = $"Waypoint_{label}";
            }
        }
    }

    public class ARInstructionRenderer : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshPro worldInstructionText;

        public void SetInstruction(string text)
        {
            if (worldInstructionText != null)
            {
                worldInstructionText.text = text;
            }
        }
    }
}
