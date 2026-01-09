using UnityEngine;
using UnityEngine.UI;

public class DashboardUI : MonoBehaviour
{
    public StationPanelUI stationPanelUI;
    public ToastManager toastManager;

    public Button stationAButton;
    public Button stationBButton;
    public Button stationCButton;

    private void Start()
    {
        stationAButton.onClick.AddListener(() => SelectStation("Station A", "OK"));
        stationBButton.onClick.AddListener(() => SelectStation("Station B", "Incident"));
        stationCButton.onClick.AddListener(() => SelectStation("Station C", "Maintenance"));
    }

    void SelectStation(string name, string status)
    {
        if (stationPanelUI != null)
            stationPanelUI.ShowStation(name, status);

        if (toastManager != null)
            toastManager.ShowToast("Station selectionnee : " + name, 2f);
    }
}
