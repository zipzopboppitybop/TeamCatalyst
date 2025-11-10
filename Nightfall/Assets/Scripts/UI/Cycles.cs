using System;
using UnityEngine;

[CreateAssetMenu]

public class Cycles : ScriptableObject
{
    enum CurrentCycle
    { 

    }


    [SerializeField] private string clockText;
    [SerializeField] private string dayText;
    [SerializeField] private Sprite dayImage;
    [SerializeField] private Sprite nightImage;
    [System.NonSerialized] public int cropsDestroyed;



    public string ClockText { get => clockText; set => clockText = value;}
    public string DayText { get => dayText; set => dayText = value; }

    public Sprite DayImage { get => dayImage; set => dayImage = value; }

    public Sprite NightImage { get => nightImage; set => nightImage = value; }  

  }