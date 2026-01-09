using UnityEngine;

/// <summary>
/// Script de debug temporaire à ajouter au GameManager pour diagnostiquer les trains
/// </summary>
public class TrainDebugger : MonoBehaviour
{
    void Start()
    {
        Invoke("DebugAllTrains", 2f); // Attendre 2 secondes après le démarrage
    }
    
    void Update()
    {
        // Appuyer sur T pour afficher les infos des trains
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugAllTrains();
        }
    }
    
    void DebugAllTrains()
    {
        Debug.Log("========== DEBUG TRAINS ==========");
        
        // Trouver tous les TrainController dans la scène
        TrainController[] allTrains = FindObjectsOfType<TrainController>();
        
        Debug.Log($"🔍 Nombre de trains trouvés: {allTrains.Length}");
        
        foreach (TrainController train in allTrains)
        {
            Debug.Log($"\n🚂 Train: {train.trainId}");
            Debug.Log($"   → GameObject: {train.gameObject.name}");
            Debug.Log($"   → Position: {train.transform.position}");
            Debug.Log($"   → Actif: {train.gameObject.activeSelf}");
            Debug.Log($"   → Parent: {(train.transform.parent != null ? train.transform.parent.name : "null")}");
            
            // Vérifier le Renderer
            if (train.trainRenderer != null)
            {
                Debug.Log($"   → Renderer: {train.trainRenderer.name}");
                Debug.Log($"   → Renderer actif: {train.trainRenderer.enabled}");
                Debug.Log($"   → Couleur: {train.trainRenderer.material.color}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Renderer NULL!");
            }
            
            // Vérifier le Collider
            Collider col = train.GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log($"   → Collider: {col.GetType().Name}");
                Debug.Log($"   → Collider actif: {col.enabled}");
                Debug.Log($"   → IsTrigger: {col.isTrigger}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Pas de Collider!");
            }
            
            // Vérifier les données
            if (train.data != null)
            {
                Debug.Log($"   → Ligne: {train.data.lineId}");
                Debug.Log($"   → Status: {train.data.status}");
                Debug.Log($"   → Station actuelle: {train.data.currentStationId}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Data NULL!");
            }
        }
        
        Debug.Log("==================================");
        
        // Vérifier aussi dans le MetroSystemManager
        if (GameManager.Instance != null && GameManager.Instance.metroSystem != null)
        {
            Debug.Log($"\n📊 Trains dans MetroSystemManager: {GameManager.Instance.metroSystem.trains.Count}");
            
            foreach (var kvp in GameManager.Instance.metroSystem.trains)
            {
                Debug.Log($"   - {kvp.Key}: {kvp.Value.lineId}");
            }
        }
    }
}