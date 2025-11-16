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
    private void UpdateLighting(float timePerecent)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePerecent);

       if(directionalLight != null)
        {
            directionalLight.color = preset.AmbientColor.Evaluate(timePerecent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePerecent * 360f) - 90f, 170f, 0));
        }
    }
}
