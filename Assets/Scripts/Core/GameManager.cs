using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    public MetroSystemManager metroSystem;
    public UIManager uiManager;
    public InteractionManager interactionManager;
    
    [Header("Prefabs")]
    public GameObject stationPrefab;
    public GameObject trainPrefab;
    public GameObject linePrefab;
    
    [Header("Scene References")]
    public Transform metroNetworkParent;
    public Transform stationsParent;
    public Transform linesParent;
    public Transform trainsParent;
    
    [Header("Game State")]
    public AppState currentState;
    public float gameTime;
    public int totalPassengers;
    public int delayCount;
    
    [Header("Game Settings")]
    public float gameDuration = 600f; // 10 minutes
    public int maxDelayThreshold = 5;
    public int maxPassengerThreshold = 2000;
    
    // Propriété pour vérifier si le jeu est en pause
    public bool IsPaused => currentState == AppState.Paused;
    
    private void Awake() 
    { 
        if (Instance == null)
        {
            Instance = this;
            // ⚠️ Commenté pour le développement - décommenter si vous avez plusieurs scènes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start() 
    { 
        Initialize();
    }
    
    private void Update() 
    { 
        // ⏸️ Les interactions restent actives même en pause
        // Seuls les mécanismes de jeu sont arrêtés
        
        if (currentState == AppState.Running)
        {
            gameTime += Time.deltaTime;
            
            // Update UI dashboard
            UpdateGameMetrics();


            // Vérifications de fin de jeu
            CheckGameOverConditions();
        }
        
        // 🎮 Raccourci clavier pour pause/resume
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    
    private void Initialize()
    {
        Debug.Log("GameManager - Initializing...");
        
        currentState = AppState.Initializing;
        
        // Initialize metro system
        if (metroSystem != null)
        {
            metroSystem.Initialize();
            Debug.Log("MetroSystemManager initialized");
            
            // Créer les stations visuelles
            CreateStations();
            
            // Créer les lignes visuelles
            CreateLines();
            
            // Créer les trains
            CreateTrains();
        }
        else
        {
            Debug.LogError("MetroSystemManager reference is missing!");
        }
        
        // Initialize UI
        if (uiManager != null)
        {
            Debug.Log("UIManager ready");
            // Setup initial UI
            uiManager.UpdateDashboard();
        }
        
        // Initialize Interaction
        if (interactionManager != null)
        {
            Debug.Log("InteractionManager ready");
        }
        
        // Change state to running
        ChangeState(AppState.Running);
        Debug.Log("Game Started!");
    }
    
    // Créer les stations dans la scène
    private void CreateStations()
    {
        if (stationPrefab == null || stationsParent == null)
        {
            Debug.LogError("Station prefab or parent is missing!");
            return;
        }
        
        foreach (var stationConfig in metroSystem.stationConfigs)
        {
            // Instantiate station prefab
            GameObject stationObj = Instantiate(
                stationPrefab, 
                stationConfig.worldPosition, 
                Quaternion.identity,
                stationsParent
            );
            
            stationObj.name = stationConfig.stationName;
            
            // Initialize controller
            StationController controller = stationObj.GetComponent<StationController>();
            
            if (controller != null)
            {
                StationData data = new StationData
                {
                    stationId = stationConfig.stationId,
                    stationName = stationConfig.stationName,
                    position = stationConfig.worldPosition,
                    status = StationStatus.Normal,
                    passengerCount = 0,
                    maxPassengers = stationConfig.maxPassengers,
                    connectedStations = new List<string>()
                };
                
                controller.Initialize(data);
                
                // Enregistrer la station dans le MetroSystemManager
                if (!metroSystem.stations.ContainsKey(stationConfig.stationId))
                {
                    metroSystem.stations.Add(stationConfig.stationId, data);
                }
                
                Debug.Log($"✅ Created station: {stationConfig.stationName}");
            }
        }
    }
    
    // Créer les lignes dans la scène
    private void CreateLines()
    {
        if (linePrefab == null || linesParent == null)
        {
            Debug.LogWarning("Line prefab or parent is missing!");
            return;
        }
        
        foreach (var lineConfig in metroSystem.lineConfigs)
        {
            GameObject lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, linesParent);
            lineObj.name = lineConfig.lineName;
            
            LineController controller = lineObj.GetComponent<LineController>();
            
            if (controller != null)
            {
                LineData data = new LineData
                {
                    lineId = lineConfig.lineId,
                    lineName = lineConfig.lineName,
                    lineColor = lineConfig.lineColor,
                    stationIds = new List<string>(),
                    status = LineStatus.Active,
                    trainCount = lineConfig.defaultTrainCount
                };
                
                // Ajouter les IDs des stations
                foreach (var station in lineConfig.stations)
                {
                    data.stationIds.Add(station.stationId);
                }
                
                controller.Initialize(data, lineConfig.stations);
                
                // Enregistrer la ligne
                if (!metroSystem.lines.ContainsKey(lineConfig.lineId))
                {
                    metroSystem.lines.Add(lineConfig.lineId, data);
                }
                
                Debug.Log($"✅ Created line: {lineConfig.lineName}");
            }
        }
    }
    
    // Créer les trains dans la scène
    private void CreateTrains()
    {
        if (trainPrefab == null || trainsParent == null)
        {
            Debug.LogWarning("Train prefab or parent is missing!");
            return;
        }
        
        int trainCounter = 1;
        
        foreach (var lineConfig in metroSystem.lineConfigs)
        {
            for (int i = 0; i < lineConfig.defaultTrainCount; i++)
            {
                // Position de départ = première station de la ligne
                Vector3 startPosition = lineConfig.stations[0].worldPosition;
                
                GameObject trainObj = Instantiate(trainPrefab, startPosition, Quaternion.identity, trainsParent);
                trainObj.name = $"Train_{trainCounter:000}";
                
                TrainController controller = trainObj.GetComponent<TrainController>();
                
                if (controller != null)
                {
                    TrainData data = new TrainData
                    {
                        trainId = $"train_{trainCounter}",
                        lineId = lineConfig.lineId,
                        status = TrainStatus.Moving,
                        currentStationId = lineConfig.stations[0].stationId,
                        nextStationId = lineConfig.stations.Count > 1 ? lineConfig.stations[1].stationId : lineConfig.stations[0].stationId,
                        passengerCapacity = 200,
                        speed = lineConfig.trainSpeed
                    };
                    
                    controller.Initialize(data);
                    
                    // Enregistrer le train
                    if (!metroSystem.trains.ContainsKey(data.trainId))
                    {
                        metroSystem.trains.Add(data.trainId, data);
                    }
                    
                    Debug.Log($"✅ Created train: {trainObj.name} on {lineConfig.lineName}");
                }
                
                trainCounter++;
            }
        }
    }
    
   /// <summary>
/// Met à jour les métriques du jeu
/// </summary>
private void UpdateGameMetrics()
{
    if (metroSystem == null)
    {
        Debug.LogError("❌ UpdateGameMetrics: metroSystem is NULL!");
        return;
    }
    
    if (metroSystem.stations == null)
    {
        Debug.LogError("❌ UpdateGameMetrics: metroSystem.stations is NULL!");
        return;
    }
    
    // Calculer le total de passagers dans les stations
    int stationPassengers = 0;
    int trainPassengers = 0;
    delayCount = 0;
    
    // Compter les passagers dans les stations
    foreach (var station in metroSystem.stations.Values)
    {
        stationPassengers += station.passengerCount;
        
        if (station.status == StationStatus.Delayed || station.status == StationStatus.Broken)
        {
            delayCount++;
        }
    }
    
    // Compter les passagers dans les trains
    if (metroSystem.trains != null)
    {
        foreach (var train in metroSystem.trains.Values)
        {
            trainPassengers += train.currentPassengers;
        }
    }
    
    // Total = stations + trains
    int previousTotal = totalPassengers;
    totalPassengers = stationPassengers + trainPassengers;
    
    // Debug CHAQUE frame pour voir si ça change
    if (totalPassengers != previousTotal)
    {
        Debug.Log($"📊 CHANGEMENT: {previousTotal} → {totalPassengers} passagers (Stations: {stationPassengers}, Trains: {trainPassengers})");
    }
    
    // Debug périodique (toutes les 5 secondes)
    if (Time.frameCount % 300 == 0)
    {
        Debug.Log($"📊 Métriques: {stationPassengers} passagers en station + {trainPassengers} en train = {totalPassengers} total | Retards: {delayCount} | Temps: {gameTime:F1}s");
    }
    
    // ✅ FORCER la mise à jour du dashboard UI
    if (uiManager != null)
    {
        uiManager.UpdateDashboard();
    }
    else
    {
        Debug.LogWarning("❌ UIManager is NULL!");
    }
}
    
    // Vérifier les conditions de Game Over
    private void CheckGameOverConditions()
    {
        // Condition 1: Trop de retards
        if (delayCount >= maxDelayThreshold)
        {
            Debug.LogWarning($"⚠️ Too many delays: {delayCount}/{maxDelayThreshold}");
            ChangeState(AppState.GameOver);
            return;
        }
        
        // Condition 2: Trop de passagers en attente
        if (totalPassengers >= maxPassengerThreshold)
        {
            Debug.LogWarning($"⚠️ Too many passengers: {totalPassengers}/{maxPassengerThreshold}");
            ChangeState(AppState.GameOver);
            return;
        }
        
        // Condition 3: Temps écoulé
        if (gameTime >= gameDuration)
        {
            Debug.Log("✅ Game duration completed!");
            ChangeState(AppState.GameOver);
            return;
        }
    }
    
    public void ChangeState(AppState newState) 
    { 
        AppState previousState = currentState;
        currentState = newState;
        Debug.Log($"State changed from {previousState} to: {newState}");
        
        if (newState == AppState.GameOver)
        {
            GameOver();
        }
    }
    
    // 🆕 Toggle entre pause et running
    public void TogglePause()
    {
        if (currentState == AppState.Paused)
        {
            ResumeGame();
        }
        else if (currentState == AppState.Running)
        {
            PauseGame();
        }
    }
    
    public void PauseGame() 
    {
        if (currentState != AppState.Running) return;
        
        ChangeState(AppState.Paused);
        // ⚠️ NE PAS utiliser Time.timeScale = 0 pour permettre les interactions
        // Les scripts vont vérifier GameManager.Instance.IsPaused à la place
        
        Debug.Log("⏸️ Game Paused - Interactions still enabled");
        
        if (uiManager != null)
        {
            // uiManager.ShowPauseMenu();
        }
    }
    
    public void ResumeGame() 
    {
        if (currentState != AppState.Paused) return;
        
        ChangeState(AppState.Running);
        
        Debug.Log("▶️ Game Resumed");
        
        if (uiManager != null)
        {
            // uiManager.HidePauseMenu();
        }
    }
    
    private void GameOver()
    {
        Debug.Log("🏁 GAME OVER!");
        Time.timeScale = 0f;
        
        // Afficher les stats finales
        Debug.Log($"📊 Final Stats:");
        Debug.Log($"   - Game Time: {gameTime:F1}s");
        Debug.Log($"   - Total Passengers: {totalPassengers}");
        Debug.Log($"   - Delays: {delayCount}");
        
        // TODO: Afficher l'écran de Game Over dans l'UI
        if (uiManager != null)
        {
            // uiManager.ShowGameOverScreen(gameTime, totalPassengers, delayCount);
        }
    }
    
    // Méthodes utilitaires publiques
    public void AddPassengers(int count)
    {
        totalPassengers += count;
    }
    
    public void RemovePassengers(int count)
    {
        totalPassengers = Mathf.Max(0, totalPassengers - count);
    }
    
    public void IncrementDelayCount()
    {
        delayCount++;
        Debug.LogWarning($"⚠️ Delay count increased: {delayCount}");
    }
    
    public void DecrementDelayCount()
    {
        delayCount = Mathf.Max(0, delayCount - 1);
        Debug.Log($"✅ Delay count decreased: {delayCount}");
    }
}