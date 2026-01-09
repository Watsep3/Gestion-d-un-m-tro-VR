using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationPanelUI : MonoBehaviour
{
    public TextMeshProUGUI stationNameText;
    public TextMeshProUGUI stationStatusText;
    public Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void ShowStation(string name, string status)
    {
        gameObject.SetActive(true);
        stationNameText.text = name;
        stationStatusText.text = status;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
