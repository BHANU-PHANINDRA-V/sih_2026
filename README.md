# TRINETRA KAVACH (त्रिनेत्र कवच)
### Offline-First Mobile AR Vocational Safety Training & Digital Certification Platform

---

## 1. Project Overview
**TRINETRA KAVACH** is an offline-first mobile Augmented Reality (AR) vocational safety training and certification system engineered in Unity (C#) for mid-range Android 10+ smartphones. 

Designed specifically for frontline industrial trainees in **Mining, Steel Manufacturing, and Mica Processing**, the application empowers workers to practice realistic safety procedures in augmented reality, assesses their practical safety behaviors, evaluates their compliance against industrial safety rules, and generates verifiable digital safety certificates **strictly upon achieving a passing score**.

---

## 2. Problem & Solution
* **The Problem:** Vocational workers in hazardous industries face high-risk environments (electrical fires, methane gas leaks, conveyor pinch points, toxic dust) with minimal hands-on safety training. Traditional classroom training lacks practical muscle memory, while physical emergency simulations are costly, dangerous, or logistically impractical. Existing mobile training tools either require continuous internet connectivity (unusable underground or in remote mines) or generate superficial certificates upon video playback without validating actual safety behavior.
* **The Solution:** TRINETRA KAVACH provides an offline-first, mobile AR safety environment. Trainees place digital equipment in their physical environment using standard Android phone cameras, identify hazards, perform time-sensitive interventions (e.g., P.A.S.S. fire extinguisher technique, LOTO sequence, atmospheric gas monitoring), follow dynamically calculated A* escape paths around hazards, and undergo strict practical and knowledge assessments. A digital certificate with a verifiable QR code is issued **only if the trainee achieves >= 70%**.

---

## 3. Technology Stack & Architecture
* **Game Engine:** Unity 2022.3 LTS (or 2023.2 LTS)
* **Language:** C# (.NET Standard 2.1)
* **Mobile Platform:** Android 10+ (API Level 29 minimum, API 33/34 target, 64-bit ARM64)
* **AR Subsystem:** Unity AR Foundation 5.1+ & Google ARCore XR Plugin (Plane detection, surface anchoring, raycasting)
* **Computer Vision / AI:** Unity Sentis 1.3+ running lightweight YOLO ONNX models (with development fallback interface `IObjectDetector`)
* **Pathfinding:** Dynamic Grid A* Pathfinding with real-time hazard cost recalculation and AR route rendering
* **Database:** Local SQLite (`trinetra_kavach.db`) with modular repositories
* **Localization:** Trilingual support (English, Hindi, and Santali) driven by externalized JSON

```
                     TRINETRA KAVACH ARCHITECTURE
                                   |
         ┌─────────────────────────┼─────────────────────────┐
         |                         |                         |
     UI LAYER               TRAINING ENGINE             DATA LAYER
  Language Selection          Scenario FSM             JSON Data Manager
  Worker Login                Safety Engine            SQLite DB (Offline)
  Dashboard & Cards           Risk Evaluator           - workers
  Instructions & AR HUD       Assessment Engine        - attempts
  Assessment & Results        Score Manager            - attempt_events
  Certificate & QR Display    Certification Gate       - progress
                                                       - certificates
         |                         |                         |
         └─────────────────────────┼─────────────────────────┘
                                   |
                             AR + AI LAYER
                                   |
                 ┌─────────────────┴─────────────────┐
                 |                                   |
           AR FOUNDATION                        YOLO + SENTIS
           Plane Detection                     Object Detection
           Anchor Placement                    Hazard Recognition
           Hazard Particles                    PPE Verification
           AR Waypoint & Arrows                (Mock Fallback included)
                 |                                   |
                 └─────────────────┬─────────────────┘
                                   |
                              A* ROUTING
                                   |
                        Dynamic Hazard Avoidance
                        Safe Evacuation Paths
                                   |
                           ASSESSMENT GATE
                                   |
                      [ Score >= Pass Threshold? ]
                                   |
                 ┌─────────────────┴─────────────────┐
                 | YES                               | NO
                 ▼                                   ▼
          PASS / CERTIFIED                     FAIL / UNCERTIFIED
       Unique Certificate ID                No Certificate Generated
       Verification Code & QR               Module Marked Not Certified
       Saved in SQLite Database             Display Critical Mistakes
       Progress Marked Certified            Offer Training Retry
```

---

## 4. Subsystems Breakdown

### 4.1 Strict Certification Gate (Hard Requirement)
A certificate is **never** issued merely for completing a module. The central gatekeeper (`CertificationManager.cs`) enforces:
```csharp
bool passed = IsPassed(assessmentResult.Score, threshold); // Configurable threshold (default: 70)
```
* **If Passed (Score >= 70):**
  1. Issues a unique, non-duplicable Certificate ID (e.g. `TK-FIRE-2026-849102`).
  2. Generates a secure verification code.
  3. Produces a procedural offline QR code (`TRINETRA_KAVACH|CERT_ID|VERIFICATION_CODE`).
  4. Stores certificate metadata in the local SQLite `certificates` table.
  5. Updates module status to `CERTIFIED`.
  6. Enables the `VIEW CERTIFICATE` button and allows local image export.
* **If Failed (Score < 70):**
  1. Certificate generation is completely skipped.
  2. Module remains `NOT CERTIFIED` on the dashboard.
  3. Result screen clearly displays `ASSESSMENT NOT PASSED` in red.
  4. Shows specific safety mistakes, incorrect actions, and weak areas.
  5. The `VIEW CERTIFICATE` button is **hidden**.
  6. Trainee is given options to `RETRY TRAINING` or `REVIEW MISTAKES`. Each retry logs a separate attempt in SQLite.

### 4.2 Playable Training Modules
1. **Module 1: Fire & Explosion Response (Complete & Playable):**
   * Scan floor -> Anchor industrial electrical panel -> Fire outbreak.
   * Trainee identifies fire hazard type (electrical).
   * Selection between Water, CO2, and Foam extinguishers.
   * Penalty rule: Using Water on electrical fire incurs a **-25 point penalty** and flags a critical fatal shock hazard.
   * Selecting CO2 and applying the P.A.S.S. technique suppresses fire (+20 points).
   * Emergency evacuation trigger -> Hazard blocks primary door -> Dynamic A* recalculates secondary safe route -> Trainee follows AR arrows to assembly point.
2. **Module 2: Gas Leak & Confined Space Safety (Complete & Playable):**
   * Toxic methane/H2S gas cloud detected.
   * Trainee positions upwind (+15 points).
   * PPE selection: Selecting a standard dust mask triggers a **-30 point violation** (dust mask cannot filter toxic gases). Selecting SCBA provides complete protection (+20 points).
   * Atmospheric 4-gas testing -> Trainee evaluates permit clearance and aborts unsafe entry (+25 points).
3. **Module 3: Machinery Safety & LOTO (Architecture & Starter Scenarios):**
   * Lockout/Tagout protocol on heavy conveyor/dumper equipment.
   * State sequence: `NORMAL -> MACHINE_STOP -> ISOLATION_REQUIRED -> LOCK_APPLIED -> TAG_APPLIED -> ENERGY_VERIFIED -> SAFE_TO_WORK`. Sequence violations trigger immediate score reductions.
4. **Module 4: PPE & Workplace Safety:**
   * Hazard-appropriate PPE mapping (Helmet, Dielectric Boots, SCBA, Particulate Respirator, Insulated Gloves, Chemical Goggles, High-Vis Vest).
5. **Module 5: Emergency Evacuation:**
   * Mine collapse / obstacle scenario with dynamic AR pathfinding to muster stations.

### 4.3 A* Pathfinding & AR Route Renderer
* Real-time grid-based pathfinding (`GridManager.cs`, `AStarPathfinder.cs`).
* Dynamic hazard blocking: When fire or gas hazards activate, hazard nodes are assigned prohibitive movement costs (`MovementCost = 999`) or blocked (`IsWalkable = false`), prompting automatic recalculation of the safest route to the assembly point.
* Path is rendered in augmented reality via `ARRouteRenderer.cs` using directional green waypoints and line renderers.

### 4.4 Computer Vision & Unity Sentis AI Pipeline
* Supports YOLO ONNX execution via Unity Sentis 1.3+.
* Detects objects: `person`, `fire_extinguisher`, `electrical_panel`, `exit`, `gas_cylinder`, `machine`, `helmet`, `gloves`, `safety_shoes`, `goggles`.
* Pre-configured development fallback: `MockObjectDetector.cs` operates cleanly without requiring external binary downloads. To use real ONNX inference, place your YOLO model at `Assets/Models/YOLO/yolo.onnx`.

### 4.5 Trilingual Offline Localization
* Supports **English (`en.json`)**, **Hindi (`hi.json`)**, and **Santali (`sat.json`)**.
* Complete separation of UI presentation and gameplay logic. All scenario instructions, rules, and question prompts reference semantic translation keys (e.g. `instruction_choose_extinguisher`, `feedback_water_on_electrical_hazard`).

### 4.6 Offline SQLite Schema
Local database stored at `Application.persistentDataPath/trinetra_kavach.db`:
* `workers`: Trainee profile, ID, name, sector, language.
* `attempts`: Every simulation run with started/completed timestamps, score, and pass/fail state.
* `attempt_events`: Fine-grained log of every action, expected action, penalty, and risk rating.
* `progress`: Tracks module completion and certification status per worker.
* `certificates`: Contains unique certificate IDs, verification hashes, scores, and issue dates for passed assessments **only**.

---

## 5. Directory Structure
```
Assets/
├── Data/
│   ├── Modules/modules.json
│   ├── Scenarios/
│   │   ├── Fire/ (electrical_fire.json, evacuation.json)
│   │   ├── Gas/ (gas_leak.json, confined_space.json)
│   │   ├── Machinery/ (machinery_safety.json)
│   │   ├── PPE/ (ppe_safety.json)
│   │   └── Emergency/ (emergency_evacuation.json)
│   ├── Rules/safety_rules.json
│   ├── Questions/questions.json
│   ├── PPE/ppe_items.json
│   ├── Certification/certification_config.json
│   └── Localization/ (en.json, hi.json, sat.json)
├── Models/YOLO/README_YOLO.md
├── Prefabs/ (AR, Fire, Gas, Machinery, PPE, Emergency, UI, Certificate)
├── Resources/Data/ (Runtime mirrored datasets)
├── Scenes/ (Boot, LanguageSelection, Login, MainMenu, ModuleSelection, Instructions, ARTraining, Assessment, Result, Certificate, CertificatesList, Settings)
├── Scripts/
│   ├── Core/ (CoreSystems.cs: AppBootstrap, GameManager, NavigationManager, SettingsManager)
│   ├── Data/ (DataModels.cs, JSONDataManager.cs)
│   ├── Localization/ (LocalizationManager.cs)
│   ├── Database/ (DatabaseSystems.cs)
│   ├── Safety/ (SafetySystems.cs: SafetyEngine, SafetyRuleEvaluator, RiskEvaluator)
│   ├── Assessment/ (AssessmentSystems.cs: AssessmentEngine, ScoreManager)
│   ├── Certification/ (CertificationSystems.cs: CertificationManager, CertificateManager, QRCodeGenerator)
│   ├── Training/ (TrainingSystems.cs: ModuleManager, ScenarioManager, ScenarioStateMachine)
│   ├── Routing/ (RoutingSystems.cs: AStarPathfinder, GridManager, ARRouteRenderer)
│   ├── AR/ (ARSystems.cs: ARPlacementManager, ARHazardManager)
│   ├── AI/ (AISystems.cs: ObjectDetectionManager, SentisInferenceManager, MockObjectDetector)
│   ├── Audio/ (AudioSystems.cs: AudioManager)
│   ├── UI/ (UISystems.cs: All Canvas Controllers)
│   └── Utilities/ (DebugManager.cs)
└── StreamingAssets/Data/ (Offline JSON filesystem mirror)
```

---

## 6. How to Build & Run for Android

### Prerequisites
* Unity 2022.3 LTS or 2023.2 LTS.
* Android Build Support module installed in Unity Hub (OpenJDK, Android SDK & NDK tools).
* ARCore-supported Android smartphone running Android 10.0+ (API Level 29+).

### Step-by-Step Instructions
1. **Open Project:**
   * Open Unity Hub, click **Add**, and select the root repository directory (`sih_2026`).
2. **Configure Build Settings:**
   * Open `File` -> `Build Settings`.
   * Add all scenes from `Assets/Scenes/` in order (`Boot.unity` as index 0).
   * Select **Android** platform and click **Switch Platform**.
3. **Configure Player Settings:**
   * Open `Project Settings` -> `Player` -> `Other Settings`.
   * **Minimum API Level:** Android 10.0 (API Level 29).
   * **Target API Level:** Android 13 (API 33) or Android 14 (API 34).
   * **Scripting Backend:** IL2CPP.
   * **Target Architectures:** ARM64 enabled (ARMv7 optional).
   * Open `Project Settings` -> `XR Plug-in Management` -> Enable **Google ARCore**.
4. **Build APK:**
   * Connect your Android phone via USB with USB Debugging enabled.
   * Click **Build and Run** to compile `TrinetraKavach.apk` directly to the phone.
   * Grant Camera permissions when prompted on launch.

---

## 7. Developer & Debug Controls
Enable the developer overlay anytime via `DebugManager.cs`:
* **Simulate PASS (Score 85):** Triggers correct actions, verifies certification gate, produces certificate and QR code.
* **Simulate FAIL (Score 40):** Triggers safety penalties, verifies certificate withholding, presents mistake breakdown and retry button.
* **Trigger Hazard Block:** Dynamically spawns a hazard on grid cell (3,3) to test A* path rerouting.
* **Reset Database:** Clears worker attempts and progress for clean testing.

---

## 8. Future Roadmap & Backend Synchronization
While TRINETRA KAVACH operates 100% offline, its SQLite database schema is strictly synchronized with standard ISO 45001 occupational safety telemetry formats. When network connectivity becomes available, the records from `attempts`, `attempt_events`, and `certificates` can be securely synced to a centralized enterprise safety analytics dashboard via standard REST/JSON endpoints.