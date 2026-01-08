using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Fournit les données de configuration du métro
/// Peut charger depuis JSON ou depuis ScriptableObjects
/// </summary>
public class DataProvider : MonoBehaviour
{
    [Header("Configuration Source")]
    [Tooltip("Si true, charge depuis JSON. Si false, utilise les ScriptableObjects")]
    public bool loadFromJSON = false;
    
    [Header("JSON Configuration")]
    public string jsonFileName = "MetroConfig.json";
    
    [Header("ScriptableObject Configuration")]
    public List<StationDataSO> stationConfigs;
    public List<LineDataSO> lineConfigs;
    
    private MetroConfig metroConfig;
    
    /// <summary>
    /// Charge la configuration complète du métro
    /// </summary>
    public MetroConfig LoadMetroConfig()
    {
        if (loadFromJSON)
        {
            return LoadFromJSON();
        }
        else
        {
            return LoadFromScriptableObjects();
        }
    }
    
    /// <summary>
    /// Charge les données depuis un fichier JSON
    /// </summary>
    private MetroConfig LoadFromJSON()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"DataProvider: Fichier JSON introuvable à {filePath}");
            return new MetroConfig();
        }
        
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            MetroConfig config = JsonUtility.FromJson<MetroConfig>(jsonContent);
            
            Debug.Log($"DataProvider: Configuration chargée depuis JSON - {config.stations.Count} stations, {config.lines.Count} lignes");
            return config;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DataProvider: Erreur lors du chargement JSON - {e.Message}");
            return new MetroConfig();
        }
    }
    
    /// <summary>
    /// Charge les données depuis les ScriptableObjects
    /// </summary>
    private MetroConfig LoadFromScriptableObjects()
    {
        MetroConfig config = new MetroConfig();
        
        // Convertir les StationDataSO en StationData
        foreach (var stationSO in stationConfigs)
        {
            if (stationSO == null)
            {
                Debug.LogWarning("DataProvider: Un ScriptableObject de station est null, ignoré");
                continue;
            }
            
            StationData stationData = new StationData
            {
                stationId = stationSO.stationId,
                stationName = stationSO.stationName,
                position = stationSO.worldPosition,
                status = StationStatus.Normal,
                passengerCount = 0,
                maxPassengers = stationSO.maxPassengers,
                connectedStations = new List<string>() // ← CORRIGÉ
            };
            
            config.stations.Add(stationData);
        }
        
        // Convertir les LineDataSO en LineData
        foreach (var lineSO in lineConfigs)
        {
            if (lineSO == null)
            {
                Debug.LogWarning("DataProvider: Un ScriptableObject de ligne est null, ignoré");
                continue;
            }
            
            LineData lineData = new LineData
            {
                lineId = lineSO.lineId,
                lineName = lineSO.lineName,
                lineColor = lineSO.lineColor,
                status = LineStatus.Active,
                trainCount = lineSO.defaultTrainCount,
                stationIds = new List<string>()
            };
            
            // Extraire les IDs des stations de la ligne
            foreach (var station in lineSO.stations)
            {
                if (station != null)
                {
                    lineData.stationIds.Add(station.stationId);
                }
            }
            
            config.lines.Add(lineData);
        }
        
        // Créer les connexions entre stations basées sur les lignes
        BuildStationConnections(config);
        
        Debug.Log($"DataProvider: Configuration chargée depuis ScriptableObjects - {config.stations.Count} stations, {config.lines.Count} lignes");
        return config;
    }
    
    /// <summary>
    /// Construit les connexions entre stations basées sur les lignes
    /// </summary>
    private void BuildStationConnections(MetroConfig config)
    {
        // Créer un dictionnaire pour accès rapide
        Dictionary<string, StationData> stationDict = new Dictionary<string, StationData>();
        foreach (var station in config.stations)
        {
            stationDict[station.stationId] = station;
        }
        
        // Pour chaque ligne, connecter les stations adjacentes
        foreach (var line in config.lines)
        {
            for (int i = 0; i < line.stationIds.Count; i++)
            {
                string currentStationId = line.stationIds[i];
                
                if (!stationDict.ContainsKey(currentStationId))
                    continue;
                
                StationData currentStation = stationDict[currentStationId];
                
                // Connecter à la station précédente
                if (i > 0)
                {
                    string prevStationId = line.stationIds[i - 1];
                    if (!currentStation.connectedStations.Contains(prevStationId)) // ← CORRIGÉ
                    {
                        currentStation.connectedStations.Add(prevStationId); // ← CORRIGÉ
                    }
                }
                
                // Connecter à la station suivante
                if (i < line.stationIds.Count - 1)
                {
                    string nextStationId = line.stationIds[i + 1];
                    if (!currentStation.connectedStations.Contains(nextStationId)) // ← CORRIGÉ
                    {
                        currentStation.connectedStations.Add(nextStationId); // ← CORRIGÉ
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Sauvegarde la configuration actuelle en JSON
    /// </summary>
    public void SaveToJSON(MetroConfig config)
    {
        string jsonContent = JsonUtility.ToJson(config, true);
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        try
        {
            // Créer le dossier StreamingAssets s'il n'existe pas
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"DataProvider: Configuration sauvegardée en JSON à {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DataProvider: Erreur lors de la sauvegarde JSON - {e.Message}");
        }
    }
    
    /// <summary>
    /// Récupère les données d'une station par son ID
    /// </summary>
    public StationData GetStationById(string stationId)
    {
        if (metroConfig == null)
        {
            metroConfig = LoadMetroConfig();
        }
        
        return metroConfig.stations.Find(s => s.stationId == stationId);
    }
    
    /// <summary>
    /// Récupère les données d'une ligne par son ID
    /// </summary>
    public LineData GetLineById(string lineId)
    {
        if (metroConfig == null)
        {
            metroConfig = LoadMetroConfig();
        }
        
        return metroConfig.lines.Find(l => l.lineId == lineId);
    }
    
    /// <summary>
    /// Valide la configuration chargée
    /// </summary>
    public bool ValidateConfig(MetroConfig config)
    {
        if (config == null)
        {
            Debug.LogError("DataProvider: Configuration null");
            return false;
        }
        
        if (config.stations.Count == 0)
        {
            Debug.LogError("DataProvider: Aucune station dans la configuration");
            return false;
        }
        
        if (config.lines.Count == 0)
        {
            Debug.LogError("DataProvider: Aucune ligne dans la configuration");
            return false;
        }
        
        // Vérifier que toutes les stations référencées dans les lignes existent
        HashSet<string> stationIds = new HashSet<string>();
        foreach (var station in config.stations)
        {
            stationIds.Add(station.stationId);
        }
        
        foreach (var line in config.lines)
        {
            foreach (var stationId in line.stationIds)
            {
                if (!stationIds.Contains(stationId))
                {
                    Debug.LogError($"DataProvider: Station '{stationId}' référencée dans la ligne '{line.lineId}' n'existe pas");
                    return false;
                }
            }
        }
        
        Debug.Log("DataProvider: Configuration validée avec succès");
        return true;
    }
}