using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]

public class ItemData : ScriptableObject
{
    public Sprite Icon;
    public GameObject dropPrefab;
    public AudioClip pickupSound;
    [Range(0, 1)] public float audVol;
    public string displayName;
    public int id;
    public float sellValue;
    public float price;
    public int maxStackSize;
    public enum itemType { Crop };
}
