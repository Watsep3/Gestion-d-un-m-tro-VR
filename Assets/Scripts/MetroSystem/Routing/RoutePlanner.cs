using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutePlanner : MonoBehaviour
{
    [Header("Route Configuration")]
    public string lineId; // e.g., "Ligne Bleue"
    public Color lineColor = Color.blue; // Visual color for the line
    public List<Transform> stations = new List<Transform>();
    public bool loopLine = true; // ✅ If true, train goes Station Z -> Station A

    // --- NEW: AUTO-FILL FIX FOR MISSING STATIONS ---
    private void Start()
    {
        // 🚨 SAFETY CHECK: If the list is empty, find the stations in the scene automatically
        // This fixes the issue where the Spawner forgets to link stations to the line.
        if (stations.Count == 0)
        {
            // Look for the folder named "Stations" in the Hierarchy (Case Sensitive!)
            GameObject stationsParent = GameObject.Find("Stations");

            if (stationsParent != null)
            {
                stations.Clear();
                // Add every child object (Gare Centrale, etc.) to the list
                foreach (Transform child in stationsParent.transform)
                {
                    stations.Add(child);
                }
                Debug.Log($"🧩 {name} auto-found {stations.Count} stations.");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find a 'Stations' object in the Hierarchy. Route is empty. Make sure your station container is named 'Stations'.");
            }
        }
    }

    /// <summary>
    /// Returns the next station based on the current one.
    /// Handles the "Loop" logic automatically.
    /// </summary>
    public Transform GetNextStation(Transform currentStation)
    {
        if (stations == null || stations.Count == 0)
        {
            Debug.LogWarning($"Route {name} has no stations!");
            return null;
        }

        // 1. If we are just starting (no current station), go to the first one.
        if (currentStation == null)
        {
            return stations[0];
        }

        // 2. Find where we are in the list
        int currentIndex = stations.IndexOf(currentStation);

        // If the station isn't on this line, default to the first one
        if (currentIndex == -1)
        {
            return stations[0];
        }

        // 3. Calculate next index
        int nextIndex = currentIndex + 1;

        // 4. Handle End of Line (Looping)
        if (nextIndex >= stations.Count)
        {
            if (loopLine)
            {
                return stations[0]; // 🔄 Loop back to start
            }
            else
            {
                // If not looping, stay at the last station
                return stations[stations.Count - 1];
            }
        }

        return stations[nextIndex];
    }

    /// <summary>
    /// Helper to get specific coordinates
    /// </summary>
    public Vector3 GetStationPosition(int index)
    {
        if (index >= 0 && index < stations.Count && stations[index] != null)
        {
            return stations[index].position;
        }
        return Vector3.zero;
    }

    // --- VISUAL DEBUGGING ---
    // This draws colored lines in the Editor so you can see the route!
    private void OnDrawGizmos()
    {
        if (stations == null || stations.Count < 2) return;

        Gizmos.color = lineColor;

        // Draw lines between stations
        for (int i = 0; i < stations.Count - 1; i++)
        {
            if (stations[i] != null && stations[i + 1] != null)
            {
                Gizmos.DrawLine(stations[i].position, stations[i + 1].position);
                Gizmos.DrawSphere(stations[i].position, 0.2f);
            }
        }

        // Draw the closing loop line
        if (loopLine && stations.Count > 1)
        {
            if (stations[stations.Count - 1] != null && stations[0] != null)
            {
                Gizmos.DrawLine(stations[stations.Count - 1].position, stations[0].position);
            }
        }
    }
}