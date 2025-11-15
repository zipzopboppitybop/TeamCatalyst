using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/FixedTile")]

public class FixedTile : RuleTile
{
    // Fix your rotation here, I think in most cases using 270 will fix the issue, unless you're doing a weird setup side-ways inverted? Yeah this could get the grid and rotate based on swizzle, feel free make it fancy:
    private Vector3 rotationEuler = new Vector3(270f, 0, 0);

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (go != null)
        {
            go.transform.Rotate(rotationEuler);
        }

        return base.StartUp(position, tilemap, go);
    }
}
