using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    
    [Header("Selection")]
    public GameObject currentSelection;
    public LayerMask interactableLayer;
    
    [Header("Input")]
    public KeyCode selectKey = KeyCode.Mouse0;
    public KeyCode actionKey = KeyCode.E;
    
    private void Update() 
    {
        HandleInput();
    }
    
    private void HandleInput() { }
    
    public void SelectObject(GameObject obj) { }
    public void DeselectObject() { }
    public void PerformAction() { }
}
