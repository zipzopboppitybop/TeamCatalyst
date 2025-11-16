using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePainter : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] RuleTile[] selectedTile;
    [SerializeField] Tilemap map;
    [SerializeField] PlayerInventoryUI inv;

    [SerializeField] GhostItem ghostPlacer;

    [SerializeField] int placeDist;

    //int tileType = 0;

    // 0 = Farmland
    // 1 = Crop

    Vector3Int currentCell;
    string currentItemName;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        map = (Tilemap)FindAnyObjectByType(typeof(Tilemap));
        if (inv != null)
        {
            inv.OnSelectedItemChanged += SelectedItemChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {



        //if (Input.GetButtonDown("Interact"))
        //{

        //    GameObject selectedTower = selectedTile[tileType].m_DefaultGameObject;

        //    RaycastHit hit;

        //    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, placeDist, ~ignoreLayer))
        //    {
        //        Debug.Log("Made it this far 0!");
        //        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * placeDist, Color.blue);

        //        currentCell = map.WorldToCell(hit.point);

        //        GameObject tower = map.GetInstantiatedObject(currentCell);
        //        if (tower != null)
        //        {
        //            Debug.Log("Made it this far 1!");
        //            TowerBase towerScript = tower.GetComponent<TowerBase>();

        //            if (towerScript.typeTower == TowerBase.TowerType.Farmland && towerScript.isFertilized)
        //            {
        //                Debug.Log("Made it this far 2!");
        //                if (selectedTower.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
        //                {

        //                    Debug.Log("Made it this far 3!");
        //                    towerScript = null;
        //                    Destroy(tower);
        //                    map.SetTile(currentCell, null);
        //                    map.SetTile(currentCell, selectedTile[tileType]);
        //                    tileType = 0;

        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (selectedTower.GetComponent<TowerBase>().typeTower != TowerBase.TowerType.Crop)
        //            {
        //                map.SetTile(currentCell, selectedTile[0]);
        //                tileType = 1;
        //            }

        //        }



        //    }
        //}
    }

    public void UpdateCurrentItem(GameObject item = null)
    {

        if (item)
        {
            currentItemName = item.name;
            ghostPlacer.canShowObj = true;

            if (currentItemName.Contains("Rake"))
            {
                ghostPlacer.ShowGhost(selectedTile[0]);
            }
            else if (currentItemName.Contains("Seed"))
            {
                ghostPlacer.ShowGhost(selectedTile[1]);
                InventorySlot slot = inv.GetSelectedSlot();

                slot.RemoveFromStack(1);

                if (slot.StackSize <= 0)
                    slot.UpdateInventorySlot(null, 0);

                inv.hotBarInventory.OnInventorySlotChanged?.Invoke(slot);
                inv.RefreshInventory();
            }
            else if (currentItemName.Contains("Fence"))
            {
                ghostPlacer.ShowGhost(selectedTile[2]);
                InventorySlot slot = inv.GetSelectedSlot();

                slot.RemoveFromStack(1);

                if (slot.StackSize <= 0)
                    slot.UpdateInventorySlot(null, 0);

                inv.hotBarInventory.OnInventorySlotChanged?.Invoke(slot);
                inv.RefreshInventory();
            }
            else
            {
                ghostPlacer.HideGhost();
            }
        }
        else
        {
            ghostPlacer.canShowObj = false;
            ghostPlacer.HideGhost();
        }


    }

    private void SelectedItemChanged(ItemData newItem)
    {
        GameObject itemPrefab = newItem?.dropPrefab;
        UpdateCurrentItem(itemPrefab);

    }

    public void TryHarvestCrop()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, placeDist, ~ignoreLayer))
        {
            //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * placeDist, Color.blue);

            currentCell = map.WorldToCell(hit.point);

            GameObject existing = map.GetInstantiatedObject(currentCell);
            TowerBase existingTower = existing ? existing.GetComponent<TowerBase>() : null;

            if (existing)
                //Debug.Log(existing.name);
            if (existingTower != null && existingTower.typeTower == TowerBase.TowerType.Crop && existingTower.isFullyGrown)
            {

                Debug.Log("Start harvesting Crop!");
                existingTower.HarvestCrop(inv.playerInventory);
                Debug.Log("Harvested Crop!");
                map.SetTile(currentCell, null);
                Debug.Log("Removed Crop!");
                map.SetTile(currentCell, selectedTile[0]);
                Debug.Log("Replaced Farmland!");

            }
        }
    }

    public void TryPlaceTile(GameObject heldItem)
    {
        if (heldItem == null)
            return;

        if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, placeDist, ~ignoreLayer))
            return;

        currentCell = map.WorldToCell(hit.point);

        GameObject existing = map.GetInstantiatedObject(currentCell);
        TowerBase existingTower = existing ? existing.GetComponent<TowerBase>() : null;

        string item = heldItem.name;

        if (item.Contains("Rake"))
        {
            if (existingTower == null)
            {
                map.SetTile(currentCell, selectedTile[0]);
            }
            return;
        }

        if (item.Contains("Fence"))
        {
            if (existingTower == null)
            {
                map.SetTile(currentCell, selectedTile[2]);
            }
            return;
        }

        if (item.Contains("Seed"))
        {
            if (existingTower != null && existingTower.typeTower == TowerBase.TowerType.Farmland && existingTower.isFertilized)
            {
                map.SetTile(currentCell, selectedTile[1]);
                Catalyst.Player.PlayerController player = GameManager.instance.player.GetComponent<Catalyst.Player.PlayerController>();

                InventorySlot slot = inv.GetSelectedSlot();

                slot.RemoveFromStack(1);

                GameObject crop = map.GetInstantiatedObject(currentCell);
                GameManager.instance.AddCrop(crop);

                if (slot.StackSize <= 0)
                    slot.UpdateInventorySlot(null, 0);

                inv.hotBarInventory.OnInventorySlotChanged?.Invoke(slot);
                inv.RefreshInventory();
            }
            else
            {
                Debug.Log("Need fertilized farmland.");
            }
            return;
        }

        if (item.Contains("Watering"))
        {
            if (existingTower != null && existingTower.typeTower == TowerBase.TowerType.Crop)
                existingTower.WaterCrop();
        }
    }

}
