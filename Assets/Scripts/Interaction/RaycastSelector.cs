using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSelector : MonoBehaviour
{
    [Header("Settings")]
    public float maxRayDistance = 100f;
    public LayerMask selectableLayer;
    
    [Header("Visual Feedback")]
    public GameObject selectionHighlight;
    public Color highlightColor = Color.cyan;
    
    public GameObject GetObjectUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRayDistance, selectableLayer))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
    
    public void ShowHighlight(GameObject target) { }
    public void HideHighlight() { }
}