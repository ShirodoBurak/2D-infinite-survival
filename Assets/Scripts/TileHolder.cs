using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHolder : MonoBehaviour {
    
    [Header("Textures")]
    public TileBase[] grass = new TileBase[2];
    public TileBase[] dirt = new TileBase[2];
    public TileBase[] stone = new TileBase[2];
    public TileBase[] iron = new TileBase[2];
    [Header("Experimental")]
    public TileBase[] lantern = new TileBase[1];
    public Dictionary<string, TileBase> Tiles = new Dictionary<string, TileBase>();
    private void Start() {
        Tiles.Add("default:grass", grass[0]);
        Tiles.Add("default:grass_2", grass[1]);
        Tiles.Add("default:dirt", dirt[0]);
        Tiles.Add("default:dirt_2", dirt[0]);
        Tiles.Add("default:stone", stone[0]);
        Tiles.Add("default:stone_2", stone[1]);
        Tiles.Add("default:iron", iron[0]);
        Tiles.Add("default:iron_2", iron[1]);
        Tiles.Add("default:lantern", lantern[0]);
    }
}
