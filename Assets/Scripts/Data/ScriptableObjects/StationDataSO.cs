using UnityEngine;

[CreateAssetMenu(fileName = "Station", menuName = "Metro/Station")]
public class StationDataSO : ScriptableObject
{
    [Header("Identification")]
    public string stationId;
    public string stationName;
    
    [Header("Position")]
    public Vector3 worldPosition;
    
    [Header("Capacity")]
    public int maxPassengers = 500;
    
    [Header("Visual")]
    public Sprite stationIcon;
}