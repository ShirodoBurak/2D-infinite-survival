using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class world_generator : MonoBehaviour {
    [System.Serializable]
    public class noise_settings {
        public bool Ingore;
        public enum noise_type { Surface, Caves, oreGeneration }
        public enum merge_type { add, subtract, multiply }
        public merge_type mergeType;
        public noise_type noiseType;
        public float size = 1;
        public float Amplifier = 1;
    }
    public int Seed;
    public noise_settings[] _settings;
    public Vector2Int offset;

    public data_controller controller;
    public Dictionary<Vector2Int, bool> IsModified = new Dictionary<Vector2Int, bool>();

    [Header("Tilemaps")]

    public Tilemap tilemap;
    public TileHolder tileholder;
    public int additionalHeight = 20;
    Vector2Int pos;
    public Vector2Int chunk_pos;
    public List<Vector2Int> light_positions;
    bool first;
    public List<Vector2Int> loadedChunks;
    
    Camera cam;
    float screenHeight;
    float screenWidth;
    void Start() {
        controller=new data_controller();
        cam =Camera.main;
        screenWidth=cam.orthographicSize*cam.aspect*2;
        screenHeight=cam.orthographicSize*2;
        loadedChunks=new List<Vector2Int>();
        if(Seed==0) Seed=Seed+1;
        chunk_pos=new Vector2Int((int)Camera.main.transform.position.x/16,(int)Camera.main.transform.position.y/16);
        pos=new Vector2Int(chunk_pos.x, chunk_pos.y);
        first=true;
    }
    void Update() {
        Vector2Int chunk_pos = new Vector2Int((int)Camera.main.transform.position.x/16,(int)Camera.main.transform.position.y/16);
        // Check if Main camera's chunk position is changed.
        // If it does, regenerate the chunks on the determined position(s).
        if(pos!=new Vector2Int(chunk_pos.x, chunk_pos.y)||first) {
            StartCoroutine(_GenerateView(chunk_pos,loadedChunks));
            pos=new Vector2Int(chunk_pos.x, chunk_pos.y);
            first=false;
        } else {
            return;
        }

    }
    IEnumerator _GenerateView(Vector2Int cpos, List<Vector2Int> vectors) {
        for(int i = -3; i<Mathf.CeilToInt(screenWidth/16)-1; i++) {
            for(int j = -2; j<Mathf.CeilToInt(screenHeight/16); j++) {
                if(!vectors.Contains(new Vector2Int(cpos.x+i, cpos.y+j))) {
                    float distance = Vector2.Distance(new Vector2Int(cpos.x+i, cpos.y+j)*16, Camera.main.transform.position);
                    if(!(distance>screenHeight*2f)||!(distance>screenWidth*2f)) {
                        ViewChunk(cpos.x+i, cpos.y+j);
                        loadedChunks.Add(new Vector2Int(cpos.x+i, cpos.y+j));
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        StartCoroutine(ClearGarbageChunks());
    }
    IEnumerator ClearGarbageChunks() {
        //foreach(Vector2Int chunk in loadedChunks)
        for(int i = 0; i<loadedChunks.Count; i++)
        {
            float distance = Vector2.Distance(loadedChunks[i]*16,Camera.main.transform.position);
            if(distance>screenHeight*2f || distance>screenWidth*2f) {
                RemoveChunk(loadedChunks[i].x, loadedChunks[i].y);
                loadedChunks.Remove(loadedChunks[i]);
            } else {}
            yield return new WaitForEndOfFrame();
        }
    }
    public int mul = 1;
    void ViewChunk(int chunkX, int chunkY) {
        if(controller.checkChunkFileExists(new Vector2Int(chunkX,chunkY))) {
            TileData tile_data = controller.loadData(new Vector2Int(chunkX,chunkY));
            for(int i = 0; i<tile_data.x.Length; i++) {
                if(tile_data.TileType[i]!="air") {
                    placeTile(tile_data.x[i], tile_data.y[i], tile_data.TileType[i]);
                } else;
            }
        } else {
            GenerateChunk(chunkX,chunkY);
        }
    }
    void GenerateChunk(int chunkX, int chunkY) {
        //
        //First of all, get the count of ignored layers.
        int ignored = countIgnored();
        //If any noise layer exists, execute
        if(_settings.Length>0) {
            //
            //Loop for X value to draw the surface.
            for(int x = 0+chunkX*16; x<16+chunkX*16; x++) {
                float noise1d = 0;
                for(int k = 0; k<_settings.Length; k++) {
                    var settings = _settings[k];
                    if(!settings.Ingore&&settings.noiseType==noise_settings.noise_type.Surface) {
                        noise1d+=Mathf.PerlinNoise(
                        (offset.x+.5f+x+(Seed*128))*settings.size,
                        1)/(_settings.Length-ignored)*settings.Amplifier;
                    }
                }
                for(int y = 0+chunkY*16; y<16+chunkY*16; y++) {
                    if(y<noise1d*mul+additionalHeight) {
                        //
                        //Prepare 2 float variable for cave check.
                        float noise2d = 0;
                        float subtract2d = 0;
                        int cave_layer_amount = 0;
                        //
                        //Loop for X and Y values to generate cave variable.
                        foreach(var item in _settings) {
                            if(item.noiseType==noise_settings.noise_type.Caves&&item.mergeType==noise_settings.merge_type.add) {
                                noise2d+=Mathf.PerlinNoise(
                            (offset.x+.5f+x+(Seed*128))*item.size,
                            (offset.y+.5f+y+(Seed*128))*item.size)*item.Amplifier;
                                cave_layer_amount++;
                            } else if(item.noiseType==noise_settings.noise_type.Caves&&item.mergeType==noise_settings.merge_type.subtract&&!item.Ingore) {
                                subtract2d+=Mathf.PerlinNoise(
                            (offset.x+.5f+x+(Seed*128))*item.size,
                            (offset.y+.5f+y+(Seed*128))*item.size)*item.Amplifier;
                            }
                        }
                        //
                        // Do some stupid math, so noise2d turns into a reasonable variable for generation.
                        // I am serious
                        noise2d=noise2d/cave_layer_amount;
                        noise2d=noise2d-subtract2d;
                        //Place the tile onto the tilemap
                        if(noise2d<0.20f) {
                            placeTile(x, y, "stone");
                        }
                    }
                }
            }
            //Loop through the chunk again to decorate it.
            Decorate(chunkX, chunkY);
        }
    }
    void RemoveChunk(int chunkX, int chunkY) {
        Dictionary<Vector2Int, string> block_positions = new Dictionary<Vector2Int, string>();
        for(int i = 0; i<16; i++) {
            for(int j = 15; j>-1; j--) {
                Vector3Int tilepos = new Vector3Int(i+chunkX*16, j+chunkY*16, 0);
                if(tilemap.GetTile(tilepos) != null) {
                    block_positions.Add(new Vector2Int(tilepos.x, tilepos.y), tilemap.GetTile(tilepos).name);
                } else {
                    block_positions.Add(new Vector2Int(tilepos.x, tilepos.y), "air");
                }
                tilemap.SetTile(tilepos, null);
            }
        }
        controller.saveData(block_positions, new Vector2Int(chunkX, chunkY));
    }
    void Decorate(int chunkX, int chunkY) {
        Dictionary<Vector2Int, TileBase> block_positions = new Dictionary<Vector2Int, TileBase>();
        light_positions.Clear();
        //bool lightplaced = false;
        int ignored = countIgnored();
        for(int x = 0+chunkX*16; x<16+chunkX*16; x++) {
            float noise1d = 0;
            for(int k = 0; k<_settings.Length; k++) {
                var settings = _settings[k];
                if(!settings.Ingore&&settings.noiseType==noise_settings.noise_type.Surface) {
                    noise1d+=Mathf.PerlinNoise((offset.x+.5f+x+(Seed*128))*settings.size,1)/(_settings.Length-ignored)*settings.Amplifier;
                }
            }
            for(int y = 0+chunkY*16; y<16+chunkY*16; y++) {
                //Conditional decorator
                if(y>noise1d*mul+additionalHeight-(noise1d*256f)) {
                    Vector3Int currentpos = new Vector3Int(x, y, 0);
                    if(tilemap.GetTile(currentpos)!=null) {
                        placeTile(x, y, "dirt");
                        block_positions.Add(new Vector2Int(x, y), tileholder.dirt);
                    } else {}
                    if(y>noise1d*mul+additionalHeight&&y<noise1d*mul+additionalHeight+1&&tilemap.GetTile(new Vector3Int(x, y-1, 0))!=null) {
                        placeTile(x, y, "grass");
                        block_positions.Add(new Vector2Int(x, y), tileholder.grass);
                    }
                } else {
                    foreach(var item in _settings) {
                        if(item.noiseType==noise_settings.noise_type.oreGeneration) {
                            float value = Mathf.PerlinNoise(
                                (offset.x+.5f+x+(Seed*128))*item.size,
                                (offset.y+.5f+y+(Seed*128))*item.size)*item.Amplifier;
                            if(value<0.20f&&tilemap.GetTile(new Vector3Int(x, y, 0))!=null&&tilemap.GetTile(new Vector3Int(x, y, 0))==tileholder.stone) {
                                placeTile(x, y, "iron");
                                block_positions.Add(new Vector2Int(x, y), tileholder.iron);
                            } 
                        }
                    }
                }
                // FOR LIGHTNING : Basically, adds a vector3Int data to public list. For 
                // generate_lightning script to use.
                // I will continue doing the lightning system later.
                /*if(Random.Range(0, 1000)<1&&!lightplaced) {
                    placeTile(x, y, tileholder.lantern);
                   light_positions.Add(new Vector2Int(x, y));
                    lightplaced=true;
                }*/
            }
        }
    }
    #region Utility
    //This function counts and returns the
    //number of ignored layers in the array.
    int countIgnored() {
        int count = 0;
        foreach(var settings in _settings) {
            if(settings.Ingore)
                count++;
        }
        return count;
    }
    //A function that simplifies the tile placement.
    void placeTile(int x, int y, string tile) {
        tilemap.SetTile(new Vector3Int(x, y, 0), tileholder.Tiles["default:"+tile]);
    }
    #endregion
}