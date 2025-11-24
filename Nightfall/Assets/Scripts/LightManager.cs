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
        if (gameManager == null)
        {
            gameManager = GameManager.instance;
        }
    }


    private void OnValidate()
    {
        if (directionalLight != null)
            return;
        if (RenderSettings.sun != null)
        {
            directionalLight = RenderSettings.sun;
        }
        else
        {
            //Light[] lights = GameObject.FindObjectsOfType<Light>();
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        if (preset == null || gameManager == null)
            return;

        float timePercent = gameManager.TimePercent;
        //timePercent %= 24;
        UpdateLighting(timePercent / 24f);
    }


    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = preset.DirectionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }
}

//private void UpdateLighting(float timePerecent)
//{
//    RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePerecent);

//   if(directionalLight != null)
//    {
//        directionalLight.color = preset.AmbientColor.Evaluate(timePerecent);
//        directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePerecent * 360f) - 90f, 170f, 0));
//    }
//}
//}
