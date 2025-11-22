using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]

public class ItemData : ScriptableObject
{
    public enum ItemType { Tool, Seed, Weapon, Resource, Livestock, Building}
    public Sprite Icon;
    public ItemType itemType;
    public GameObject dropPrefab;
    public AudioClip pickupSound;
    public AudioClip interactSound;
    [Range(0, 1)] public float audVol;
    public string displayName;
    public int id;
    public float price;
    public int maxStackSize;
    public int harvestAmount;
}
