using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHolder : MonoBehaviour {
    
    [Header("Textures")]
    public TileBase grass;
    public TileBase dirt;
    public TileBase stone;
    public TileBase iron;
    [Header("Experimental")]
    public TileBase lantern;
    public Dictionary<string, TileBase> Tiles = new Dictionary<string, TileBase>();
    private void Start() {
        Tiles.Add("default:grass", grass);
        Tiles.Add("default:dirt", dirt);
        Tiles.Add("default:stone", stone);
        Tiles.Add("default:iron", iron);
        Tiles.Add("default:lantern", lantern);
    }
}
