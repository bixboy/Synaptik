using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InteractableHighlighter : MonoBehaviour
{
    [SerializeField]
    private Renderer[] renderers = System.Array.Empty<Renderer>();

    [SerializeField, ColorUsage(false, true)]
    private Color defaultHighlightColor = Color.cyan;

    [SerializeField, Min(0f)]
    private float emissionBoost = 2f;

    [SerializeField, Range(0f, 1f)]
    private float colorBlend = 0.35f;

    private readonly List<RendererState> rendererStates = new();
    private bool isInitialized;
    private bool isHighlighted;
    private Color currentColor;
    private float currentEmissionBoost;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        InitializeRenderers();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            isInitialized = false;
            InitializeRenderers();
            ApplyHighlight(false, defaultHighlightColor, emissionBoost);
        }
        else if (isHighlighted)
        {
            ApplyHighlight(true, currentColor, currentEmissionBoost);
        }
    }

    private void OnDisable()
    {
        if (isHighlighted)
        {
            ApplyHighlight(false, currentColor, currentEmissionBoost);
        }
    }

    public void RefreshRenderers()
    {
        isInitialized = false;
        InitializeRenderers();
        if (isHighlighted)
        {
            ApplyHighlight(true, currentColor, currentEmissionBoost);
        }
    }

    public void SetHighlighted(bool highlighted, Color color, float emissionMultiplier)
    {
        InitializeRenderers();

        isHighlighted = highlighted;
        currentColor = highlighted ? color : defaultHighlightColor;
        currentEmissionBoost = emissionMultiplier;

        ApplyHighlight(highlighted, color, emissionMultiplier);
    }

    private void InitializeRenderers()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        if (renderers == null)
        {
            renderers = System.Array.Empty<Renderer>();
        }

        if (isInitialized)
            return;

        rendererStates.Clear();
        foreach (var renderer in renderers)
        {
            if (!renderer)
                continue;

            var state = new RendererState(renderer);
            rendererStates.Add(state);
        }

        isInitialized = true;
    }

    private void ApplyHighlight(bool highlighted, Color color, float emissionMultiplier)
    {
        if (rendererStates.Count == 0)
            return;

        foreach (var state in rendererStates)
        {
            state.ApplyHighlight(highlighted, color, emissionMultiplier, colorBlend);
        }
    }

    private sealed class RendererState
    {
        public readonly Renderer Renderer;
        public readonly MaterialPropertyBlock Block;
        public readonly bool HasEmission;
        public readonly Color BaseEmission;
        public readonly bool HasColorProperty;
        public readonly int ColorPropertyId;
        public readonly Color BaseColor;

        public RendererState(Renderer renderer)
        {
            Renderer = renderer;
            Block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(Block);

            if (renderer.sharedMaterial && renderer.sharedMaterial.HasProperty(EmissionColorId))
            {
                HasEmission = true;
                BaseEmission = renderer.sharedMaterial.GetColor(EmissionColorId);
            }
            else
            {
                HasEmission = false;
                BaseEmission = Color.black;
            }

            if (renderer.sharedMaterial && renderer.sharedMaterial.HasProperty(BaseColorId))
            {
                HasColorProperty = true;
                ColorPropertyId = BaseColorId;
                BaseColor = renderer.sharedMaterial.GetColor(BaseColorId);
            }
            else if (renderer.sharedMaterial && renderer.sharedMaterial.HasProperty(ColorId))
            {
                HasColorProperty = true;
                ColorPropertyId = ColorId;
                BaseColor = renderer.sharedMaterial.GetColor(ColorId);
            }
            else
            {
                HasColorProperty = false;
                ColorPropertyId = -1;
                BaseColor = Color.white;
            }
        }

        public void ApplyHighlight(bool highlighted, Color color, float emissionMultiplier, float blend)
        {
            if (Renderer == null)
                return;

            Renderer.GetPropertyBlock(Block);

            if (HasEmission)
            {
                var emission = highlighted
                    ? BaseEmission + color * Mathf.Max(0f, emissionMultiplier)
                    : BaseEmission;

                Block.SetColor(EmissionColorId, emission);

                if (highlighted)
                    Renderer.EnableKeyword("_EMISSION");
                else if (BaseEmission.maxColorComponent <= 0f)
                    Renderer.DisableKeyword("_EMISSION");
            }

            if (HasColorProperty)
            {
                var targetColor = highlighted
                    ? Color.Lerp(BaseColor, color, Mathf.Clamp01(blend))
                    : BaseColor;

                Block.SetColor(ColorPropertyId, targetColor);
            }

            Renderer.SetPropertyBlock(Block);
            Renderer.UpdateGIMaterials();
        }
    }
}
