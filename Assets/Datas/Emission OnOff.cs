using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SimpleEmissionToggler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private bool emissionOn = true;

    private Material targetMaterial;
    private bool lastToggleState;

    private void Awake()
    {
        InitMaterial();
    }

    private void InitMaterial()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            targetMaterial = Application.isPlaying ? targetRenderer.material : targetRenderer.sharedMaterial;
        }

        if (targetMaterial != null)
        {
            UpdateEmission(emissionOn);
            lastToggleState = emissionOn;
        }
    }

    private void Update()
    {
        if (emissionOn != lastToggleState)
        {
            UpdateEmission(emissionOn);
            lastToggleState = emissionOn;
        }
    }

    // --- THIS IS THE NEW PIECE FOR YOUR BLUEPRINT/GRAPH ---
    /// <summary>
    /// Call this from Unity Events or Graph Runners to turn emission on (true) or off (false).
    /// </summary>
    public void SetEmissionActive(bool isActive)
    {
        emissionOn = isActive;
        UpdateEmission(isActive);
    }

    private void UpdateEmission(bool isOn)
    {
        if (targetMaterial == null) return;

        if (isOn)
        {
            targetMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            targetMaterial.DisableKeyword("_EMISSION");
        }
    }

    private void OnValidate()
    {
        InitMaterial();
    }
}