using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject stationPanel;
    public GameObject trainPanel;
    public GameObject globalDashboard;
    
    [Header("Station Panel")]
    public TextMeshProUGUI stationNameText;
    public TextMeshProUGUI stationStatusText;
    public TextMeshProUGUI passengerCountText;
    public Button repairButton;
    
    [Header("Dashboard")]
    public TextMeshProUGUI totalPassengersText;
    public TextMeshProUGUI delayCountText;
    public TextMeshProUGUI gameTimeText;
    
    [Header("Messages")]
    public ToastMessage toastPrefab;
    public Transform toastParent;
    
    public void ShowStationPanel(StationData station) { }
    public void HideStationPanel() { }
    public void ShowTrainPanel(TrainData train) { }
    public void HideTrainPanel() { }
    public void UpdateDashboard() { }
    public void ShowToast(string message, Color color) { }
}
