using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contrôle le mouvement et le comportement d'un train
/// </summary>
public class TrainController : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public string trainId;
    public TrainData data;

    [Header("Movement Settings")]
    public RoutePlanner currentRoute;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stopDuration = 2f;

    [Header("Security")]
    public float detectionDistance = 3.0f;
    public LayerMask trainLayer;

    [Header("Internal State")]
    public Transform currentTarget;
    public bool isMoving = false;
    public bool isWaiting = false;

    // --- UPDATED LOGIC START ---

    private void Start()
    {
        // We don't do anything in Start anymore because the Line might not exist yet!
    }

    private void Update()
    {
        // 1. SURVIVAL MODE: If we have no route, keep looking for one!
        if (currentRoute == null)
        {
            AttemptToFindRoute();
            return; // Don't try to move if we are lost
        }

        // 2. If we found a route but haven't started moving yet, Kickstart!
        if (currentTarget == null && !isWaiting)
        {
            InitializeMovement();
            return;
        }

        // 3. Normal Movement Logic
        if (isMoving)
        {
            UpdateMovement(Time.deltaTime);
        }
    }

    // New helper to keep searching for the line
    private void AttemptToFindRoute()
    {
        currentRoute = FindObjectOfType<RoutePlanner>();

        if (currentRoute != null)
        {
            Debug.Log($"✅ Train found the line: {currentRoute.name}");
            InitializeMovement();
        }
    }

    // Setup the first target once the route is found
    private void InitializeMovement()
    
    [Tooltip("Durée d'arrêt en station (secondes)")]
    public float stationStopDuration = 2f;
    
    [Header("Visual")]
    public Renderer trainRenderer;
    
    [Header("Colors")]
    public Color movingColor = Color.blue;
    public Color stoppedColor = Color.gray;
    public Color maintenanceColor = new Color(1f, 0.5f, 0f); // Orange
    
    // Références privées
    private MetroSystemManager metroSystem;
    private MetroSimulator simulator;
    private List<StationDataSO> lineStations;
    private int currentStationIndex = 0;
    private bool isInitialized = false;
    
    /// <summary>
    /// Initialise le train avec ses données
    /// </summary>
    public void Initialize(TrainData trainData)
    {
        data = trainData;
        trainId = trainData.trainId;
        moveSpeed = trainData.speed;
        
        Debug.Log($"🚂 Initializing train: {trainId}");
        Debug.Log($"   → Line: {trainData.lineId}");
        Debug.Log($"   → Speed: {trainData.speed}");
        
        // Récupérer les références
        metroSystem = GameManager.Instance.metroSystem;
        simulator = FindObjectOfType<MetroSimulator>();
        
        if (metroSystem == null)
        {
            Debug.LogError($"TrainController {trainId}: MetroSystemManager not found!");
            return;
        }
        
        // Récupérer la liste des stations de la ligne
        LineDataSO lineConfig = GetLineConfig(trainData.lineId);
        
        if (lineConfig != null)
        {
            lineStations = new List<StationDataSO>(lineConfig.stations);
            currentStationIndex = 0;
            
            Debug.Log($"✅ Train {trainId} initialized on line {trainData.lineId} with {lineStations.Count} stations");
        }
        else
        {
            Debug.LogError($"TrainController {trainId}: Line config not found for {trainData.lineId}");
            return;
        }
        
        // Définir la couleur initiale
        UpdateVisuals();
        
        // Démarrer le mouvement après une petite pause
        isInitialized = true;
        StartCoroutine(DelayedStart());
    }
    
    /// <summary>
    /// Démarre le mouvement après un délai aléatoire
    /// Pour éviter que tous les trains partent en même temps
    /// </summary>
    private IEnumerator DelayedStart()
    {
        float randomDelay = Random.Range(0.5f, 3f);
        Debug.Log($"Train {trainId} will start in {randomDelay:F1} seconds");
        
        yield return new WaitForSeconds(randomDelay);
        
        // Aller vers la prochaine station
        MoveToNextStation();
    }
    
    void Update()
    {
        if (!isInitialized || data.status == TrainStatus.Maintenance)
            return;
        
        if (isMoving)
        {
            UpdateMovement(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Met à jour le mouvement du train chaque frame
    /// </summary>
    public void UpdateMovement(float deltaTime)
    {

    // Orienter le train vers sa cible
    if (currentTarget != transform.position)
    {
        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
        if (!isMoving) return;
        
        // Déplacer vers la cible
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            moveSpeed * deltaTime
        );
        
        // Vérifier si on est arrivé
        float distance = Vector3.Distance(transform.position, currentTarget);
        
        if (distance < 0.2f)
        {
            // Arrivé à destination !
            ArrivedAtStation();
        }
    }
    
    /// <summary>
    /// Appelé quand le train arrive à une station
    /// </summary>
    private void ArrivedAtStation()
    {
        isMoving = false;
        data.status = TrainStatus.Stopped;
        UpdateVisuals();
        
        // Récupérer la station actuelle
        if (currentStationIndex >= 0 && currentStationIndex < lineStations.Count)
        {
            StationDataSO currentStationConfig = lineStations[currentStationIndex];
            data.currentStationId = currentStationConfig.stationId;
            
            Debug.Log($"🚂 Train {trainId} arrived at {currentStationConfig.stationName}");
            
            // Embarquer/débarquer des passagers
            if (simulator != null)
            {
                simulator.ProcessTrainArrival(trainId, data.currentStationId);
            }
            else
            {
                Debug.LogWarning($"Train {trainId}: MetroSimulator not found, skipping passenger processing");
            }
        }
        else
        {
            Debug.LogError($"Train {trainId}: Invalid station index {currentStationIndex}");
        }
        
        // Attendre puis repartir
        StartCoroutine(WaitAndContinue());
    }
    
    /// <summary>
    /// Attend en station puis repart
    /// </summary>
    private IEnumerator WaitAndContinue()
    {
        // Attendre
        yield return new WaitForSeconds(stationStopDuration);
        
        // Si le train n'est pas en maintenance, repartir
        if (data.status != TrainStatus.Maintenance)
        {
            MoveToNextStation();
        }
        else
        {
            Debug.Log($"🔧 Train {trainId} is in maintenance, not moving");
        }
    }
    
    /// <summary>
    /// Commence le mouvement vers la prochaine station
    /// </summary>
    private void MoveToNextStation()
    {
        if (lineStations == null || lineStations.Count == 0)
        {
            Debug.LogError($"Train {trainId}: No stations in line!");
            return;
        }
        
        // Passer à la station suivante
        currentStationIndex++;
        
        // Boucler en fin de ligne
        if (currentStationIndex >= lineStations.Count)
        {
            currentStationIndex = 0;
            Debug.Log($"🔄 Train {trainId} looping back to start of line");
        }
        
        // Récupérer la prochaine station
        StationDataSO nextStationConfig = lineStations[currentStationIndex];
        data.nextStationId = nextStationConfig.stationId;
        
        // Vérifier si la station est en panne
        StationData nextStation = metroSystem.GetStation(data.nextStationId);
        
        if (nextStation != null && nextStation.status == StationStatus.Broken)
        {
            Debug.LogWarning($"⚠️ Train {trainId}: Station {nextStationConfig.stationName} is broken, skipping!");
            // Sauter cette station et aller à la suivante
            MoveToNextStation();
            return;
        }
        
        // Définir la cible
        currentTarget = nextStationConfig.worldPosition;
        currentTarget.y = 0.5f; // Hauteur du train au-dessus du sol
        
        // Démarrer le mouvement
        isMoving = true;
        data.status = TrainStatus.Moving;
        UpdateVisuals();
        
        Debug.Log($"🚂 Train {trainId} moving to {nextStationConfig.stationName}");
    }
    
    /// <summary>
    /// Déplace le train vers une position donnée (legacy - pour compatibilité)
    /// </summary>
    public void MoveTo(Vector3 targetPosition)
    {
        currentTarget = targetPosition;
        isMoving = true;
        data.status = TrainStatus.Moving;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Arrête le train (pour les pannes)
    /// </summary>
    public void Stop()
    {
        isMoving = false;
        data.status = TrainStatus.Maintenance;
        UpdateVisuals();
        
        Debug.Log($"🔧 Train {trainId} stopped for maintenance");
    }
    
    /// <summary>
    /// Redémarre le train après une panne
    /// </summary>
    public void Resume()
    {
        if (data.status == TrainStatus.Maintenance)
        {
            data.status = TrainStatus.Moving;
            MoveToNextStation();
            
            Debug.Log($"✅ Train {trainId} resumed");
        }
    }
    
    /// <summary>
    /// Met à jour l'apparence visuelle selon le status
    /// </summary>
    private void UpdateVisuals()
    {
        if (trainRenderer == null)
        {
            Debug.LogWarning($"Train {trainId}: trainRenderer is null!");
            return;
        }
        
        switch (data.status)
        {
            case TrainStatus.Moving:
                trainRenderer.material.color = movingColor;
                break;
                
            case TrainStatus.Stopped:
                trainRenderer.material.color = stoppedColor;
                break;
                
            case TrainStatus.Maintenance:
                trainRenderer.material.color = maintenanceColor;
                break;
        }
    }
    
    /// <summary>
    /// Récupère la configuration de ligne depuis MetroSystemManager
    /// </summary>
    private LineDataSO GetLineConfig(string lineId)
    {
        if (metroSystem == null)
        {
            Debug.LogError("GetLineConfig: metroSystem is null!");
            return null;
        }
        
        if (metroSystem.lineConfigs == null)
        {
            Debug.LogError("GetLineConfig: lineConfigs is null!");
            return null;
        }
        
        foreach (var lineConfig in metroSystem.lineConfigs)
        {
            if (lineConfig != null && lineConfig.lineId == lineId)
            {
                return lineConfig;
            }
        }
        
        Debug.LogError($"GetLineConfig: Line {lineId} not found!");
        return null;
    }
    
    /// <summary>
    /// Appelé quand on clique sur le train (legacy)
    /// </summary>
    public void OnTrainClicked()
    {
        OnSelected();
    }
    
    // ========================================
    // INTERFACE IINTERACTABLE
    // ========================================
    
    /// <summary>
    /// Appelé quand on clique sur le train
    /// </summary>
    public void OnSelected()
    {
        if (currentRoute != null && currentRoute.stations.Count > 0)
        {
            // Teleport to first station
            transform.position = currentRoute.stations[0].position;

            // Set target to next station
            Transform nextStop = currentRoute.GetNextStation(currentRoute.stations[0]);
            if (nextStop != null)
            {
                currentTarget = nextStop;
                MoveTo(nextStop.position);
            }
        }
    }

    // --- UPDATED LOGIC END ---

    public void MoveTo(Vector3 targetPosition)
    {
        isMoving = true;
        if (data != null) data.status = TrainStatus.Moving;
    }

    public void UpdateMovement(float deltaTime)
    {
        // A. DATA CHECK
        if (data != null && data.status != TrainStatus.Moving) return;

        if (!isMoving || isWaiting || currentTarget == null) return;

        // B. ANTI-COLLISION
        if (!CheckTrackClear()) return;

        // C. ROTATION & MOVEMENT
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * deltaTime);

        // D. ARRIVAL
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            StartCoroutine(HandleArrival());
        }
    }

    private bool CheckTrackClear()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance, trainLayer))
        {
            if (hit.collider.gameObject != this.gameObject) return false;
        }
        return true;
    }

    private IEnumerator HandleArrival()
    {
        isMoving = false;
        isWaiting = true;
        yield return new WaitForSeconds(stopDuration);

        if (currentRoute != null)
        {
            Transform nextStation = currentRoute.GetNextStation(currentTarget);
            if (nextStation != null)
            {
                currentTarget = nextStation;
                MoveTo(nextStation.position);
            }
        }
        isWaiting = false;
    }

    // Required Interface Methods
    public void Initialize(TrainData trainData) { this.data = trainData; }
    public void OnTrainClicked() { }
    public void SwitchRoute(RoutePlanner newRoute) { currentRoute = newRoute; } // Simplified for now

    // IInteractable
    public void OnSelected() { Debug.Log($"Selected Train {trainId}"); }
    public void OnDeselected() { }
    public void OnAction()
    {
        if (data != null && data.status == TrainStatus.Maintenance) data.status = TrainStatus.Moving;
        Debug.Log($"🚂 Train {trainId} sélectionné");
        Debug.Log($"   - Ligne: {data.lineId}");
        Debug.Log($"   - État: {data.status}");
        Debug.Log($"   - Passagers: {data.currentPassengers}/{data.passengerCapacity}");
        Debug.Log($"   - Position actuelle: {data.currentStationId}");
        Debug.Log($"   - Prochaine station: {data.nextStationId}");
        
        // TODO: Ouvrir le panel train (Personne 3)
        // UIManager uiManager = GameManager.Instance.uiManager;
        // if (uiManager != null)
        // {
        //     uiManager.ShowTrainPanel(data);
        // }
    }
    
    /// <summary>
    /// Appelé quand on désélectionne le train
    /// </summary>
    public void OnDeselected()
    {
        Debug.Log($"❌ Train {trainId} désélectionné");
    }
    
    /// <summary>
    /// Action spéciale sur le train (touche E)
    /// </summary>
    public void OnAction()
    {
        Debug.Log($"🔧 Action sur train {trainId}");
        
        // Redémarrer un train en panne
        if (data.status == TrainStatus.Maintenance)
        {
            Resume();
            Debug.Log($"✅ Train {trainId} redémarré manuellement!");
        }
        else
        {
            Debug.Log($"Train {trainId} fonctionne normalement");
        }
    }
    
    /// <summary>
    /// Retourne une description du train
    /// </summary>
    public string GetInteractionInfo()
    {
        return $"Train {trainId} - Ligne {data.lineId} - État: {data.status} - Passagers: {data.currentPassengers}/{data.passengerCapacity}";
    }
    public string GetInteractionInfo() { return "Train"; }
}