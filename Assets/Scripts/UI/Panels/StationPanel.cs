using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // ? POUR Button et Image
using TMPro;           // ? POUR TextMeshProUGUI

public class StationPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI stationName;
    public TextMeshProUGUI status;
    public TextMeshProUGUI passengers;
    public Image statusIcon;
    
    [Header("Actions")]
    public Button repairButton;
    public Button rerouteButton;
    public Button closeButton;
    
    private StationData currentStation;
    
    public void Show(StationData station) { }
    public void Hide() { }
    public void OnRepairClicked() { }
    public void OnRerouteClicked() { }
}
