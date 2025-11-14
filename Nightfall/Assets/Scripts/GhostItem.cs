using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostItem : MonoBehaviour
{

    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] Tilemap map;
    [SerializeField] RuleTile ghostPrefab;
    [SerializeField] GameObject ghostModel;
    [SerializeField] int offset;
    public bool hiding = true;
    GameObject ghostObj;
    
    Vector3Int lastTilePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        UpdateGhostObject(ghostPrefab.m_DefaultGameObject);

    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20, ~ignoreLayer))
        {

            if (map.WorldToCell(hit.point) != lastTilePos)
            {            
                lastTilePos = map.WorldToCell(hit.point);
                UpdateGhostPos();
            }

        }

    }

    public void ShowGhost(RuleTile tile)
    {

        hiding = false;
        UpdateGhostObject(tile.m_DefaultGameObject);

    }

    public void HideGhost()
    {

        hiding = true;
        UpdateGhostObject(ghostPrefab.m_DefaultGameObject);

    }

    public void UpdateGhostObject(GameObject model)
    {

        ghostModel = model;
        if (ghostObj != null) 
            Destroy(ghostObj);
        if (!hiding)
            ghostObj = Instantiate(ghostModel, transform.position, transform.rotation);

    }

    public void UpdateGhostPos()
    {

        Vector3 translatePos = new Vector3((lastTilePos.x * map.cellSize.x) + offset, lastTilePos.z * map.cellSize.z, (lastTilePos.y * map.cellSize.y) + offset);
        transform.position = translatePos;
        if (ghostObj)
            ghostObj.transform.position = translatePos;

    }

}
