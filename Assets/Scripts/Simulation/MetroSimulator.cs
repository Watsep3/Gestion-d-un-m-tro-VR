using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetroSimulator : MonoBehaviour
{
    [Header("Settings")]
    public float incidentInterval = 30f; // secondes
    public float incidentChance = 0.3f;
    
    private float timer = 0f;
    
    public void UpdateSimulation(float deltaTime) 
    {
        timer += deltaTime;
        if (timer >= incidentInterval)
        {
            TryGenerateIncident();
            timer = 0f;
        }
    }
    
    private void TryGenerateIncident() { }
    public void GenerateRandomIncident() { }
}
