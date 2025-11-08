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

    int tileType = 0;

    Vector3Int currentCell;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        map = (Tilemap)FindAnyObjectByType(typeof(Tilemap));

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Interact"))
        {

            GameObject selectedTower = selectedTile[tileType].m_DefaultGameObject;

            RaycastHit hit;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, placeDist, ~ignoreLayer))
            {
                Debug.Log("Made it this far 0!");
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * placeDist, Color.blue);

                currentCell = map.WorldToCell(hit.point);

                GameObject tower = map.GetInstantiatedObject(currentCell);
                if (tower != null)
                {
                    Debug.Log("Made it this far 1!");
                    TowerBase towerScript = tower.GetComponent<TowerBase>();

                    if (towerScript.typeTower == TowerBase.TowerType.Farmland && towerScript.isFertilized)
                    {
                        Debug.Log("Made it this far 2!");
                        if (selectedTower.GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
                        {

                            Debug.Log("Made it this far 3!");
                            towerScript = null;
                            Destroy(tower);
                            map.SetTile(currentCell, null);
                            map.SetTile(currentCell, selectedTile[tileType]);
                            tileType = 0;

                        }
                    }
                }
                else
                {
                    if (selectedTower.GetComponent<TowerBase>().typeTower != TowerBase.TowerType.Crop)
                    {
                        map.SetTile(currentCell, selectedTile[0]);
                        tileType = 1;
                    }
                        
                }
                
                

            }
}
    }

    void ChangeTile()
    {



    }

}
