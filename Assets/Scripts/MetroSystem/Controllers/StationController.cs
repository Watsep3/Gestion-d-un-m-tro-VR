using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class StationController : MonoBehaviour
{
    [Header("Data")]
    public string stationId;
    public StationData data;
    
    [Header("Visual")]
    public GameObject visualModel;
    public Renderer statusRenderer;
    public TextMeshPro stationLabel;
    
    [Header("Status Colors")]
    public Color normalColor = Color.green;
    public Color delayedColor = Color.yellow;
    public Color brokenColor = Color.red;
    
    public void Initialize(StationData stationData) { }
    public void UpdateVisuals() { }
    public void SetStatus(StationStatus newStatus) { }
    public void OnStationClicked() { }
}