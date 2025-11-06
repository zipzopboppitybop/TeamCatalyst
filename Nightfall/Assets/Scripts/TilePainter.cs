using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePainter : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] RuleTile testTile;
    [SerializeField] Tilemap map;

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
                currentCell = map.WorldToCell(hit.point);
                RuleTile selectedTile = (RuleTile)map.GetTile(currentCell);
                map.SetTile(currentCell, testTile);
            }
}
    }
}
