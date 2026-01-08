using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [Header("Data")]
    public string trainId;
    public TrainData data;
    
    [Header("Movement")]
    public Transform currentTarget;
    public float moveSpeed = 5f;
    public bool isMoving = false;
    
    [Header("Visual")]
    public Renderer trainRenderer;
    
    public void Initialize(TrainData trainData) { }
    public void MoveTo(Vector3 targetPosition) { }
    public void Stop() { }
    public void UpdateMovement(float deltaTime) { }
    public void OnTrainClicked() { }
}