using UnityEngine;
using TrinetraKavach.Training;
using TrinetraKavach.Routing;
using TrinetraKavach.Core;

namespace TrinetraKavach.Utilities
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance { get; private set; }
        public bool ShowDevOverlay = false;

        [SerializeField] private ScenarioStateMachine stateMachine;

        private void Awake() => Instance = this;

        private void OnGUI()
        {
            if (!ShowDevOverlay) return;

            GUI.Box(new Rect(10, 10, 240, 320), "TRINETRA DEV CONTROLS");

            if (GUI.Button(new Rect(20, 40, 220, 30), "Simulate PASS (Score 85)"))
            {
                if (stateMachine != null)
                {
                    stateMachine.SubmitWorkerAction("SELECT_CO2_EXTINGUISHER");
                    stateMachine.SubmitWorkerAction("CONFIRM_ASSEMBLY_REACHED");
                }
            }

            if (GUI.Button(new Rect(20, 80, 220, 30), "Simulate FAIL (Score 40)"))
            {
                if (stateMachine != null)
                {
                    stateMachine.SubmitWorkerAction("SELECT_WATER_EXTINGUISHER");
                    stateMachine.SubmitWorkerAction("FORCE_UNSAFE_ENTRY");
                }
            }

            if (GUI.Button(new Rect(20, 120, 220, 30), "Trigger Hazard Block"))
            {
                GridManager.Instance?.MarkHazard(3, 3, true);
            }

            if (GUI.Button(new Rect(20, 160, 220, 30), "Clear Hazards"))
            {
                GridManager.Instance?.MarkHazard(3, 3, false);
            }

            if (GUI.Button(new Rect(20, 200, 220, 30), "Return to Dashboard"))
            {
                NavigationManager.Instance?.GoToDashboard();
            }
        }
    }
}
