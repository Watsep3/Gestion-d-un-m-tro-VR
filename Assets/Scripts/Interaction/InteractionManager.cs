using UnityEngine;

/// <summary>
/// Gère toutes les interactions utilisateur
/// Détecte les clics et gère la sélection des objets
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    
    [Header("Selection")]
    [Tooltip("Objet actuellement sélectionné")]
    public GameObject currentSelection;
    
    [Tooltip("Layer des objets cliquables")]
    public LayerMask interactableLayer = -1; // -1 = tous les layers
    
    [Header("Input")]
    [Tooltip("Touche pour sélectionner (clic gauche)")]
    public KeyCode selectKey = KeyCode.Mouse0;
    
    [Tooltip("Touche pour action spéciale")]
    public KeyCode actionKey = KeyCode.E;
    
    [Header("Raycast Settings")]
    [Tooltip("Distance maximale du raycast")]
    public float maxRayDistance = 1000f;
    
    // Référence au RaycastSelector
    private RaycastSelector raycastSelector;
    
    void Start()
    {
        // Forcer l'assignation de la caméra
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Si Camera.main ne marche pas, chercher par nom
        if (mainCamera == null)
        {
            GameObject cameraObj = GameObject.Find("Main Camera");
            if (cameraObj != null)
            {
                mainCamera = cameraObj.GetComponent<Camera>();
            }
        }
        
        // Dernier recours : chercher n'importe quelle caméra
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("❌ InteractionManager: Pas de caméra trouvée!");
        }
        else
        {
            Debug.Log($"✅ InteractionManager: Caméra trouvée ({mainCamera.name})");
        }
        
        // Récupérer le RaycastSelector sur le même GameObject
        raycastSelector = GetComponent<RaycastSelector>();
        
        if (raycastSelector == null)
        {
            Debug.LogWarning("⚠️ InteractionManager: RaycastSelector manquant! Ajout automatique...");
            raycastSelector = gameObject.AddComponent<RaycastSelector>();
        }
        
        Debug.Log("✅ InteractionManager: Initialisé avec succès");
    }
    
    void Update()
    {
        // Ne pas gérer les interactions si le jeu est en pause ou en Game Over
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentState == AppState.GameOver)
            {
                return;
            }
            // Les interactions restent actives même en pause
        }
        
        // Appeler HandleInput chaque frame
        HandleInput();
    }
    
    /// <summary>
    /// Gère les inputs du joueur chaque frame
    /// </summary>
    private void HandleInput()
    {
        // Clic gauche = sélectionner un objet
        if (Input.GetMouseButtonDown(0)) // 0 = clic gauche
        {
            HandleSelection();
        }
        
        // Touche E = action sur l'objet sélectionné
        if (Input.GetKeyDown(actionKey))
        {
            PerformAction();
        }
        
        // ESC = désélectionner
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
    }
    
    /// <summary>
    /// Gère la sélection d'un objet au clic
    /// </summary>
    private void HandleSelection()
    {
        if (mainCamera == null)
        {
            Debug.LogError("❌ HandleSelection: Caméra manquante!");
            return;
        }
        
        // Créer un rayon depuis la caméra vers la position de la souris
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Debug visuel du rayon
        Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.yellow, 0.5f);
        
        // Lancer le raycast
        if (Physics.Raycast(ray, out hit, maxRayDistance, interactableLayer))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log($"🖱️ Clic détecté sur: {clickedObject.name} (Layer: {LayerMask.LayerToName(clickedObject.layer)})");
            
            // Vérifier si l'objet est interactable
            IInteractable interactable = clickedObject.GetComponent<IInteractable>();
            
            // Si pas trouvé directement, chercher dans le parent
            if (interactable == null)
            {
                interactable = clickedObject.GetComponentInParent<IInteractable>();
            }
            
            if (interactable != null)
            {
                Debug.Log($"✅ {clickedObject.name} est interactable!");
                // Sélectionner cet objet (ou son parent avec l'interface)
                MonoBehaviour interactableMB = interactable as MonoBehaviour;
if (interactableMB != null)
{
    SelectObject(interactableMB.gameObject);
}
            }
            else
            {
                Debug.Log($"❌ {clickedObject.name} n'est pas interactable (pas d'interface IInteractable)");
                // Clic sur quelque chose de non-interactable = désélectionner
                DeselectObject();
            }
        }
        else
        {
            // Clic dans le vide = désélectionner
            Debug.Log("🖱️ Clic dans le vide (aucun collider touché)");
            DeselectObject();
        }
    }
    
    /// <summary>
    /// Sélectionne un objet
    /// </summary>
    public void SelectObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("⚠️ SelectObject: objet null!");
            return;
        }
        
        // Si on a déjà quelque chose de sélectionné et que c'est différent
        if (currentSelection != null && currentSelection != obj)
        {
            DeselectObject();
        }
        
        // Ne pas re-sélectionner le même objet
        if (currentSelection == obj)
        {
            Debug.Log($"⚠️ {obj.name} est déjà sélectionné");
            return;
        }
        
        // Nouveau objet sélectionné
        currentSelection = obj;
        
        // Appeler OnSelected() sur l'objet
        IInteractable interactable = obj.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.OnSelected();
            
            // Afficher les infos dans la console
            string info = interactable.GetInteractionInfo();
            if (!string.IsNullOrEmpty(info))
            {
                Debug.Log($"ℹ️ {info}");
            }
        }
        
        // Afficher la surbrillance
        if (raycastSelector != null)
        {
            raycastSelector.ShowHighlight(obj);
        }
        
        Debug.Log($"✅ Objet sélectionné: {obj.name}");
    }
    
    /// <summary>
    /// Désélectionne l'objet actuel
    /// </summary>
    public void DeselectObject()
    {
        if (currentSelection != null)
        {
            // Appeler OnDeselected() sur l'objet
            IInteractable interactable = currentSelection.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnDeselected();
            }
            
            // Retirer la surbrillance
            if (raycastSelector != null)
            {
                raycastSelector.HideHighlight();
            }
            
            Debug.Log($"❌ Objet désélectionné: {currentSelection.name}");
            
            currentSelection = null;
        }
    }
    
    /// <summary>
    /// Effectue l'action spéciale sur l'objet sélectionné (touche E)
    /// </summary>
    public void PerformAction()
    {
        if (currentSelection != null)
        {
            Debug.Log($"🎬 Action sur: {currentSelection.name}");
            
            IInteractable interactable = currentSelection.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnAction();
            }
            else
            {
                Debug.LogWarning($"⚠️ {currentSelection.name} n'a pas d'interface IInteractable");
            }
        }
        else
        {
            Debug.Log("⚠️ Aucun objet sélectionné pour effectuer une action");
        }
    }
    
    /// <summary>
    /// Retourne l'objet actuellement sélectionné
    /// </summary>
    public GameObject GetCurrentSelection()
    {
        return currentSelection;
    }
    
    /// <summary>
    /// Vérifie si un objet est sélectionné
    /// </summary>
    public bool HasSelection()
    {
        return currentSelection != null;
    }
}