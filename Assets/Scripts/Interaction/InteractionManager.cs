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
    public LayerMask interactableLayer;
    
    [Header("Input")]
    [Tooltip("Touche pour sélectionner (clic gauche)")]
    public KeyCode selectKey = KeyCode.Mouse0;
    
    [Tooltip("Touche pour action spéciale")]
    public KeyCode actionKey = KeyCode.E;
    
    // Référence au RaycastSelector
    private RaycastSelector raycastSelector;
    
    void Start()
    {
        // Forcer l'assignation de la caméra
        mainCamera = Camera.main;
        
        // Si Camera.main ne marche pas, chercher par nom
        if (mainCamera == null)
        {
            GameObject cameraObj = GameObject.Find("Main Camera");
            if (cameraObj != null)
            {
                mainCamera = cameraObj.GetComponent<Camera>();
            }
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("InteractionManager: Pas de caméra trouvée!");
        }
        else
        {
            Debug.Log($"InteractionManager: Caméra trouvée ✅ ({mainCamera.name})");
        }
        
        // Récupérer le RaycastSelector sur le même GameObject
        raycastSelector = GetComponent<RaycastSelector>();
        
        if (raycastSelector == null)
        {
            Debug.LogError("InteractionManager: RaycastSelector manquant! Ajoute-le sur GameManager.");
        }
        else
        {
            Debug.Log("InteractionManager: RaycastSelector trouvé ✅");
        }
    }
    
    void Update()
    {
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
    }
    
    /// <summary>
    /// Gère la sélection d'un objet au clic
    /// </summary>
    private void HandleSelection()
    {
        if (raycastSelector == null)
        {
            Debug.LogWarning("RaycastSelector manquant!");
            return;
        }
        
        // Détecter l'objet sous la souris
        GameObject clickedObject = raycastSelector.GetObjectUnderMouse();
        
        if (clickedObject != null)
        {
            Debug.Log($"🖱️ Clic détecté sur: {clickedObject.name}");
            
            // Vérifier si l'objet est interactable
            IInteractable interactable = clickedObject.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                Debug.Log($"✅ {clickedObject.name} est interactable!");
                // Sélectionner cet objet
                SelectObject(clickedObject);
            }
            else
            {
                Debug.Log($"❌ {clickedObject.name} n'est pas interactable");
                // Clic sur quelque chose de non-interactable = désélectionner
                DeselectObject();
            }
        }
        else
        {
            // Clic dans le vide = désélectionner
            Debug.Log("🖱️ Clic dans le vide");
            DeselectObject();
        }
    }
    
    /// <summary>
    /// Sélectionne un objet
    /// </summary>
    public void SelectObject(GameObject obj)
    {
        if (obj == null) return;
        
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
        }
        else
        {
            Debug.Log("⚠️ Aucun objet sélectionné pour effectuer une action");
        }
    }
}