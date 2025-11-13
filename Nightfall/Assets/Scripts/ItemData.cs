using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]

public class ItemData : ScriptableObject
{
    public enum ItemType { Tool, Seed, Weapon, Resource, Livestock}
    public Sprite Icon;
    public ItemType itemType;
    public GameObject dropPrefab;
    public AudioClip pickupSound;
    [Range(0, 1)] public float audVol;
    public string displayName;
    public int id;
    public int sellValue;
    public int price;
    public int maxStackSize;
}
