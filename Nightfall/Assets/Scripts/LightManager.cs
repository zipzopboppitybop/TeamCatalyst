using JetBrains.Annotations;
using UnityEngine;

[ExecuteAlways]
public class LightManager : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightingPreset preset;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if(gameManager == null )
        {
            gameManager = GameManager.instance;
        }
    }

    private void Update()
    {
       if (preset == null || gameManager == null)
            return;

        float timePercent = gameManager.TimePercent;
        UpdateLighting(timePercent);
    }
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(0.3f, 1.3f, preset.LightIntensity.Evaluate(timePercent)));
        float t = preset.LightIntensity.Evaluate(timePercent);
        Color tint = Color.Lerp(Color.black, preset.AmbientColor.Evaluate(timePercent), t);
        RenderSettings.skybox.SetColor("_Tint", tint);

        if (directionalLight != null)
        {
            directionalLight.color = preset.DirectionalColor.Evaluate(timePercent);
            directionalLight.intensity = preset.LightIntensity.Evaluate(timePercent);

            directionalLight.transform.localRotation = Quaternion.Euler( new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }
}
