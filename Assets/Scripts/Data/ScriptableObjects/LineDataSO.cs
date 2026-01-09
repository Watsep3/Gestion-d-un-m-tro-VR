
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Line", menuName = "Metro/Line")]
public class LineDataSO : ScriptableObject
{
    [Header("Identification")]
    public string lineId;
    public string lineName;
    
    [Header("Visual")]
    public Color lineColor;
    
    [Header("Route")]
    public List<StationDataSO> stations;
    
    [Header("Trains")]
    public int defaultTrainCount = 2;
    public float trainSpeed = 5f;
}
