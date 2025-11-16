using UnityEngine;

[CreateAssetMenu]

public class Cycles : ScriptableObject
{
   
    [SerializeField] private string clockText;
    [SerializeField] private string dayText;

    public string ClockText { get => clockText; set => clockText = value;}
    public string DayText { get => dayText; set => dayText = value; }


}