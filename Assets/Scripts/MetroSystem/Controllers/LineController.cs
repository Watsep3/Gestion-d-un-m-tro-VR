using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineController : MonoBehaviour
{
    [Header("Data")]
    public string lineId;
    public LineData data;
    
    [Header("Visual")]
    public LineRenderer lineRenderer;
    public float lineWidth = 0.2f;
    
    private List<StationDataSO> stationConfigs;
    
    // ? MÉTHODE INITIALIZE QUI MANQUAIT
    public void Initialize(LineData lineData, List<StationDataSO> stations)
    {
        data = lineData;
        lineId = lineData.lineId;
        stationConfigs = stations;
        
        // Setup LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        
        if (lineRenderer != null)
        {
            // Configure LineRenderer
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = data.lineColor;
            lineRenderer.endColor = data.lineColor;
            
            // Set positions
            lineRenderer.positionCount = stations.Count;
            
            for (int i = 0; i < stations.Count; i++)
            {
                Vector3 pos = stations[i].worldPosition;
                pos.y = 0.1f; // Légèrement au-dessus du sol
                lineRenderer.SetPosition(i, pos);
            }
            
            Debug.Log($"LineController initialized: {data.lineName} with {stations.Count} stations");
        }
        else
        {
            Debug.LogError($"LineRenderer not found on {gameObject.name}");
        }
    }
    
    public void UpdateLineStatus(LineStatus newStatus)
    {
        data.status = newStatus;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (lineRenderer == null) return;
        
        // Change color based on status
        Color color = data.lineColor;
        
        switch (data.status)
        {
            case LineStatus.Active:
                // Normal color
                break;
            case LineStatus.Delayed:
                color = Color.Lerp(data.lineColor, Color.yellow, 0.5f);
                break;
            case LineStatus.Closed:
                color = Color.gray;
                break;
        }
        
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}