using UnityEngine;
[System.Serializable]
[CreateAssetMenu(
    fileName = "lighting Preset",
    menuName = "Scriptables/Lighting Preset",
    order = 1)]

public class LightingPreset : ScriptableObject
{
    [SerializeField] private Gradient ambientColor;

    public Gradient AmbientColor { get { return ambientColor; } }
}

   
