using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePainter : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] RuleTile[] selectedTile;
    [SerializeField] Tilemap map;
    [SerializeField] GameObject ghost;

    [SerializeField] int placeDist;

    //int tileType = 0;

    // 0 = Farmland
    // 1 = Crop

    Vector3Int currentCell;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        map = (Tilemap)FindAnyObjectByType(typeof(Tilemap));

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

    public void TryPlaceTile(GameObject heldItem)
    {
        if (heldItem == null)
        {
            return;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, placeDist, ~ignoreLayer))
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * placeDist, Color.blue);

            currentCell = map.WorldToCell(hit.point);

            GameObject existing = map.GetInstantiatedObject(currentCell);
            TowerBase existingTower = existing ? existing.GetComponent<TowerBase>() : null;

            if (heldItem.name.Contains("Rake"))
            {
                if (existingTower == null)
                {
                    map.SetTile(currentCell, selectedTile[0]);
                    Debug.Log("Placed farmland tile!");
                }
            }
            else if(heldItem.name.Contains("Seed"))
            {
                if (existingTower != null && existingTower.typeTower == TowerBase.TowerType.Farmland && existingTower.isFertilized)
                {
                    map.SetTile(currentCell, selectedTile[1]);
                    Catalyst.Player.PlayerController player = GameManager.instance.player.GetComponent<Catalyst.Player.PlayerController>();

                    if (player != null)
                    {
                        PlayerInventoryUI hotbar = player.GetHotBar();
                        InventorySlot slot = hotbar.GetSelectedSlot();
                        if (slot != null)
                        {
                            slot.RemoveFromStack(1);
                            GameObject crop = map.GetInstantiatedObject(currentCell);
                            GameManager.instance.AddCrop(crop);


                            //towerScript = null;
                            //Destroy(tower);
                            //map.SetTile(currentCell, null);
                            //map.SetTile(currentCell, selectedTile[tileType]);
                            if (slot.StackSize <= 0)
                            {
                                slot.UpdateInventorySlot(null, 0);
                            }

                            hotbar.inventory.OnInventorySlotChanged?.Invoke(slot);

                            hotbar.RefreshInventory();
                        }
                    }
                }
                else
                {       
                    Debug.Log("Can't plant seed — need fertilized farmland.");
                }
            }
        }
    }

}
