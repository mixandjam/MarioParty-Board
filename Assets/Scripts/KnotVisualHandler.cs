using DG.Tweening;
using UnityEngine;

public class KnotVisualHandler : MonoBehaviour
{
    private SplineKnotData knotData;
    private Renderer knotRenderer;

    private Color originalEmissionColor;
    private Color landEmissionColor;
    [SerializeField] private float glowIntensity = 2;
    [SerializeField] private float glowDelay = .2f;


    void Start()
    {
        knotData = GetComponentInParent<SplineKnotData>();
        knotRenderer = GetComponentInChildren<Renderer>();

        originalEmissionColor = knotRenderer.materials[1].color;
        float factor = Mathf.Pow(2, glowIntensity);
        landEmissionColor = new Color(originalEmissionColor.r * factor, originalEmissionColor.g * factor, originalEmissionColor.b * factor);

        knotData.OnLand.AddListener(OnLand);
    }

    private void OnLand(int coinGain)
    {
        knotRenderer.materials[1].DOColor(landEmissionColor, .4f).OnComplete(() => knotRenderer.materials[1].DOColor(originalEmissionColor, .4f)).SetDelay(glowDelay);
    }

}
