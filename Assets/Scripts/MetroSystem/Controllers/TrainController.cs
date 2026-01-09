using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    public string GetInteractionInfo() { return "Train"; }
}