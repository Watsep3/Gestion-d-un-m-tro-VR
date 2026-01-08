using UnityEngine;

[CreateAssetMenu(fileName = "Train", menuName = "Metro/Train")]
public class TrainDataSO : ScriptableObject
{
    [Header("Identification")]
    public string trainId;
    
    [Header("Stats")]
    public int passengerCapacity = 200;
    public float speed = 5f;
    
    [Header("Visual")]
    public Color trainColor;
}
