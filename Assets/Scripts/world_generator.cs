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
    [Header("Tilemaps")]
    public Tilemap tilemap;
    public TileHolder tileholder;
    public int additionalHeight = 20;
    Vector2Int pos;
    int orthoSize;
    public Vector2Int chunk_pos;
    public List<Vector2Int> light_positions;
    bool first;
    void Start() {
        orthoSize=(int)Camera.main.orthographicSize;
        if(Seed==0) Seed=Seed+1;
        chunk_pos=new Vector2Int(
            (int)Camera.main.transform.position.x/16,
            (int)Camera.main.transform.position.y/16
        );
        pos=new Vector2Int(chunk_pos.x, chunk_pos.y);
        first=true;
    }
    void Update() {
        //
        // Set the chunkX and chunkY position. 
        // 15/5/2021 : I am stupid, both of these values were singular integer variables.
        // So, I turned them into a Vector2Int. Could someone donate a reasonable braincell for me
        //To make sure stupid past mistakes doesn't repeat itself, I will keep it.
        /*
        int chunkX = (int)Camera.main.transform.position.x/16;
        int chunkY = (int)Camera.main.transform.position.y/16;
        */

        Vector2Int chunk_pos = new Vector2Int(
            (int)Camera.main.transform.position.x/16,
            (int)Camera.main.transform.position.y/16
        );

        // Check if Main camera's chunk position is changed.
        // If it does, regenerate the chunks on the determined position(s).
        if(pos!=new Vector2Int(chunk_pos.x, chunk_pos.y)||first) {
            tilemap.ClearAllTiles();
            GenerateChunk(chunk_pos.x, chunk_pos.y);
            pos=new Vector2Int(chunk_pos.x, chunk_pos.y);
            first=false;
        } else {
            return;
        }
    }
    public int mul = 1;
    void GenerateChunk(int chunkX, int chunkY) {
        setOrthoSize();
        //
        //First of all, get the count of ignored layers.
        int ignored = countIgnored();
        //If any noise layer exists, execute
        if(_settings.Length>0) {
            //
            //Loop for X value to draw the surface.
            for(int x = 0+chunkX*16-getExtraWidth()-orthoSize; x<16+chunkX*16+getExtraWidth(); x++) {
                float noise1d = 0;
                for(int k = 0; k<_settings.Length; k++) {
                    var settings = _settings[k];
                    if(!settings.Ingore&&settings.noiseType==noise_settings.noise_type.Surface) {
                        noise1d+=Mathf.PerlinNoise(
                        (offset.x+.5f+x+(Seed*128))*settings.size,
                        1)/(_settings.Length-ignored)*settings.Amplifier;
                    }
                }
                for(int y = 0+chunkY*16-getExtraHeight()-orthoSize; y<16+chunkY*16+getExtraHeight(); y++) {
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
                            placeTile(x, y, tileholder.stone);
                        }
                    }
                }
            }
            //Loop through the chunk again to decorate it.
            Decorate(chunkX, chunkY);
        }
    }

    void Decorate(int chunkX, int chunkY) {
        light_positions.Clear();
        bool lightplaced = false;
        int ignored = countIgnored();
        for(int x = 0+chunkX*16-getExtraWidth()-orthoSize; x<16+chunkX*16+getExtraWidth(); x++) {
            float noise1d = 0;
            for(int k = 0; k<_settings.Length; k++) {
                var settings = _settings[k];
                if(!settings.Ingore&&settings.noiseType==noise_settings.noise_type.Surface) {
                    noise1d+=Mathf.PerlinNoise(
                    (offset.x+.5f+x+(Seed*128))*settings.size,
                    1)/(_settings.Length-ignored)*settings.Amplifier;
                }
            }
            for(int y = 0+chunkY*16-getExtraHeight()-orthoSize; y<16+chunkY*16+getExtraHeight(); y++) {
                //
                //Conditional decorator
                if(y>noise1d*mul+additionalHeight-(noise1d*256f)) {
                    Vector3Int currentpos = new Vector3Int(x, y, 0);
                    if(getTile(currentpos)!=null) {
                        placeTile(x, y, tileholder.dirt);
                    } else { }
                    foreach(var item in _settings) {
                        if(item.noiseType==noise_settings.noise_type.oreGeneration) {
                            float value = Mathf.PerlinNoise(
                                (offset.x+.5f+x+(Seed*128))*item.size,
                                (offset.y+.5f+y+(Seed*128))*item.size)*item.Amplifier;
                            if(value<0.20f&&tilemap.GetTile(new Vector3Int(x, y, 0))!=null&&tilemap.GetTile(new Vector3Int(x, y, 0))==tileholder.stone) {
                                placeTile(x, y, tileholder.iron);
                            }
                        }
                    }
                }
                //
                //Not sure what to do with this. Still can't generate grasses :'(
                //I am stupid : (
                /*
                if(y>noise1d*mul+additionalHeight) {
                    placeTile(x, y, tileholder.grass);
                    return;
                }
                */
                //

                //
                // FOR LIGHTNING : Basically, adds a vector3Int data to public list. For 
                // generate_lightning script to use.
                if(Random.Range(0, 1000)<1&&!lightplaced) {
                    placeTile(x, y, tileholder.lantern);
                    light_positions.Add(new Vector2Int(x, y));
                    lightplaced=true;
                }
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
    //A function that simplifies the tile data checking.
    TileBase getTile(Vector3Int vec) {
        return tilemap.GetTile(vec);
    }
    //A function that simplifies the tile placement.
    void placeTile(int x, int y, TileBase tile) {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
    int getExtraWidth() {
        return (int)(Screen.width/32);
    }
    void setOrthoSize() {
        orthoSize=(int)Camera.main.orthographicSize;
    }
    int getExtraHeight() {
        return (int)(Screen.height/32);
    }
    #endregion
}