using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour, IInteractable
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

    // ========================================
    // INTERFACE IINTERACTABLE
    // ========================================
    
    /// <summary>
    /// Appelé quand on clique sur le train
    /// </summary>
    public void OnSelected()
    {
        Debug.Log($"🚂 Train {trainId} sélectionné");
        
        // Afficher les infos
        Debug.Log($"   - Ligne: {data.lineId}");
        Debug.Log($"   - État: {data.status}");
        Debug.Log($"   - Passagers: {data.currentPassengers}/{data.passengerCapacity}");
        
        // TODO: Plus tard, ouvrir le panel train
        // UIManager uiManager = FindObjectOfType<UIManager>();
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
    /// Action spéciale sur le train
    /// </summary>
    public void OnAction()
    {
        Debug.Log($"🔧 Action sur train {trainId}");
        
        // Exemple: redémarrer un train en panne
        if (data.status == TrainStatus.Maintenance)
        {
            data.status = TrainStatus.Moving;
            Debug.Log($"✅ Train {trainId} redémarré!");
        }
    }
    
    /// <summary>
    /// Retourne une description du train
    /// </summary>
    public string GetInteractionInfo()
    {
        return $"Train {trainId} - Ligne {data.lineId} - État: {data.status}";
    }
}