using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class light_calculator : MonoBehaviour {
    //
    world_generator w_gen;
    List<Vector2Int> block_pos;
    Tilemap tilemap;
    Vector2Int campos;
    //
    void Start() {
        this.block_pos=w_gen.light_positions;
        this.tilemap=w_gen.tilemap;
        this.campos=w_gen.chunk_pos;
    }

    // Update is called once per frame
    void Update() {

    }
}
