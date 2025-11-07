using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePainter : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] RuleTile testTile;
    [SerializeField] Tilemap map;
    [SerializeField] GameObject ghost;

    [SerializeField] int placeDist;

    Vector3Int currentCell;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Interact"))
        {        
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, placeDist, ~ignoreLayer))
            {
                
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * placeDist, Color.blue);

                currentCell = map.WorldToCell(hit.point);

               // if (currentCell != null)
               // {
                    //GameObject tower = map.GetInstantiatedObject(currentCell);
                    //TowerBase towerScript = tower.GetComponent<TowerBase>();

                    //if (testTile.GetComponent<GameObject>().GetComponent<TowerBase>().typeTower == TowerBase.TowerType.Crop)
                    //{
                       // if (towerScript.typeTower == TowerBase.TowerType.Farmland)
                       // {

                          //  map.SetTile(currentCell, testTile);

                        //}
                   // }
                //}
               // else
                //{
                    map.SetTile(currentCell, testTile);
                //}
                
                

            }
}
    }
}
