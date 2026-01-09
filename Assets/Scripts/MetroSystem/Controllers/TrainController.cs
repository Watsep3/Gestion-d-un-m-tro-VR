using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public string trainId;
    public TrainData data;

    [Header("Movement")]
    public Vector3 currentTarget;
    public float moveSpeed = 5f;
    public bool isMoving = false;
    public float stationStopDuration = 2f;

    [Header("Visual")]
    public Renderer trainRenderer;

    [Header("Colors")]
    public Color movingColor = Color.blue;
    public Color stoppedColor = Color.gray;
    public Color maintenanceColor = new Color(1f, 0.5f, 0f);
    public Color selectionColor = Color.yellow;

    // Sélection
    private bool isSelected = false;
    private Color currentStatusColor;

    // Références
    private MetroSystemManager metroSystem;
    private MetroSimulator simulator;
    private List<StationDataSO> lineStations;
    private int currentStationIndex = 0;
    private bool isInitialized = false;

    // =========================
    // INITIALISATION
    // =========================
    public void Initialize(TrainData trainData)
    {
        data = trainData;
        trainId = trainData.trainId;
        moveSpeed = trainData.speed;

        metroSystem = GameManager.Instance.metroSystem;
        simulator = FindObjectOfType<MetroSimulator>();

        LineDataSO lineConfig = GetLineConfig(data.lineId);
        if (lineConfig == null)
        {
            Debug.LogError($"Train {trainId}: line not found");
            return;
        }

        lineStations = new List<StationDataSO>(lineConfig.stations);
        currentStationIndex = 0;

        UpdateVisuals();

        isInitialized = true;
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        MoveToNextStation();
    }

    // =========================
    // UPDATE
    // =========================
    void Update()
    {
        if (!isInitialized || data.status == TrainStatus.Maintenance)
            return;

        if (isMoving)
        {
            UpdateMovement(Time.deltaTime);
        }

        // Mise à jour UI si sélectionné
        if (isSelected && Time.frameCount % 30 == 0)
        {
            GameManager.Instance.uiManager?.ShowTrainPanel(data);
        }
    }

    // =========================
    // MOUVEMENT
    // =========================
    private void UpdateMovement(float deltaTime)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            moveSpeed * deltaTime
        );

        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            StartCoroutine(HandleArrival());
        }
    }

    private IEnumerator HandleArrival()
    {
        isMoving = false;
        data.status = TrainStatus.Stopped;
        UpdateVisuals();

        StationDataSO station = lineStations[currentStationIndex];
        data.currentStationId = station.stationId;

        simulator?.ProcessTrainArrival(trainId, data.currentStationId);

        yield return new WaitForSeconds(stationStopDuration);

        if (data.status != TrainStatus.Maintenance)
        {
            MoveToNextStation();
        }
    }

    private void MoveToNextStation()
    {
        currentStationIndex = (currentStationIndex + 1) % lineStations.Count;
        StationDataSO nextStation = lineStations[currentStationIndex];

        StationData stationData = metroSystem.GetStation(nextStation.stationId);
        if (stationData != null && stationData.status == StationStatus.Broken)
        {
            MoveToNextStation();
            return;
        }

        data.nextStationId = nextStation.stationId;

        currentTarget = nextStation.worldPosition;
        currentTarget.y = 0.5f;

        isMoving = true;
        data.status = TrainStatus.Moving;
        UpdateVisuals();
    }

    // =========================
    // VISUELS (COMME STATION)
    // =========================
    private void UpdateVisuals()
    {
        if (trainRenderer == null || data == null)
            return;

        if (isSelected)
        {
            trainRenderer.material.color = selectionColor;
            return;
        }

        switch (data.status)
        {
            case TrainStatus.Moving:
                currentStatusColor = movingColor;
                break;
            case TrainStatus.Stopped:
                currentStatusColor = stoppedColor;
                break;
            case TrainStatus.Maintenance:
                currentStatusColor = maintenanceColor;
                break;
        }

        trainRenderer.material.color = currentStatusColor;
    }

    // =========================
    // INTERACTION
    // =========================
    public void OnSelected()
    {
        isSelected = true;
        UpdateVisuals();

        GameManager.Instance.uiManager?.ShowTrainPanel(data);

        Debug.Log($"🚆 Train {trainId} sélectionné");
    }

    public void OnDeselected()
    {
        isSelected = false;
        UpdateVisuals();

        GameManager.Instance.uiManager?.HideTrainPanel();

        Debug.Log($"❌ Train {trainId} désélectionné");
    }

    public void OnAction()
    {
        if (data.status == TrainStatus.Maintenance)
        {
            data.status = TrainStatus.Moving;
            MoveToNextStation();
        }
    }

    // =========================
// MAINTENANCE / INCIDENTS
// =========================

/// <summary>
/// Met le train en maintenance (appelé par incidents)
/// </summary>
public void Stop()
{
    isMoving = false;

    if (data != null)
    {
        data.status = TrainStatus.Maintenance;
    }

    UpdateVisuals();

    Debug.Log($"🔧 Train {trainId} arrêté (maintenance)");
}

/// <summary>
/// Redémarre le train après maintenance
/// </summary>
public void Resume()
{
    if (data == null || data.status != TrainStatus.Maintenance)
        return;

    Debug.Log($"✅ Train {trainId} redémarré");

    data.status = TrainStatus.Moving;
    UpdateVisuals();

    MoveToNextStation();
}


    public string GetInteractionInfo()
    {
        return $"Train {trainId} | Ligne {data.lineId} | {data.status}";
    }

    // =========================
    // UTILS
    // =========================
    private LineDataSO GetLineConfig(string lineId)
    {
        foreach (var line in metroSystem.lineConfigs)
        {
            if (line.lineId == lineId)
                return line;
        }
        return null;
    }
}
