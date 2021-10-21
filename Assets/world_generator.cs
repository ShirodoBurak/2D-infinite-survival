using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class world_generator : MonoBehaviour{

    public Vector3Int up = new Vector3Int(0,1,0);
    public Vector3Int left = new Vector3Int(-1,0,0);
    public Vector3Int right = new Vector3Int(1,0,0);
    public Vector3Int down = new Vector3Int(0,-1,0);

    [System.Serializable]
    public class noise_settings{
        public bool Ingore;
        [Space(5)]
        [Header("Settings")]
        public int Seed;
        public enum noise_type { Surface, Caves, oreGeneration }
        public enum merge_type { add, subtract, multiply}
        public merge_type mergeType;
        public noise_type noiseType;
        public float size = 1;
        public float Amplifier = 1;
    }
    public noise_settings[] _settings;
    public Vector2Int offset;
    [Header("Tilemaps")]
    public Tilemap tilemap;
    public TileHolder tileholder;
    public int additionalHeight = 20;
    void Update(){
        if (Input.GetKeyDown(KeyCode.M)) { 
            GenerateWorld();
        }
        else {
            return;
        }
    }
    public int mul = 1;
    void GenerateWorld(){
        tilemap.ClearAllTiles();

        //First of all, get the count of ignored layers.
        int ignored = countIgnored();
        //If any noise layer exists, execute
        if (_settings.Length > 0){
            //Loop for X value to draw the surface.
            for (int x = 0; x < 512; x++){
                float noise1d = 0;
                for (int k = 0; k < _settings.Length; k++){
                    var settings = _settings[k];
                    if (!settings.Ingore && settings.noiseType == noise_settings.noise_type.Surface){
                        noise1d += Mathf.PerlinNoise(
                        (offset.x + .5f + x + (settings.Seed * 128)) * settings.size,
                        1) / (_settings.Length - ignored) * settings.Amplifier;
                    }
                }
                //Find the cave layer
                int caveLayer = findCaveLayer();
                for (int y = 0; y < noise1d*mul+additionalHeight; y++){
                    //Prepare a float variable for cave check.
                    float noise2d = 0;
                    float subtract2d = 0;
                    int cave_layer_amount = 0;
                    //Loop for X and Y values to generate cave variable.
                    foreach (var item in _settings){
                        if (item.noiseType == noise_settings.noise_type.Caves && item.mergeType == noise_settings.merge_type.add ){
                            noise2d += Mathf.PerlinNoise(
                        (offset.x + .5f + x + (item.Seed * 128)) * item.size,
                        (offset.y + .5f + y + (item.Seed * 128)) * item.size) * item.Amplifier;
                            cave_layer_amount++;
                        }else if (item.noiseType == noise_settings.noise_type.Caves && item.mergeType == noise_settings.merge_type.subtract && !item.Ingore){
                            subtract2d+= Mathf.PerlinNoise(
                        (offset.x + .5f + x + (item.Seed * 128)) * item.size,
                        (offset.y + .5f + y + (item.Seed * 128)) * item.size) * item.Amplifier;
                        }
                    }
                    noise2d = noise2d / cave_layer_amount;
                    noise2d = noise2d - subtract2d;
                    //Place the tile onto the tilemap
                    if (noise2d < 0.20f){
                        placeTile(x, y, tileholder.stone);
                    }
                } 
            }
            //Loop through the chunk again to decorate it.
            Decorate();
        }
    }
    //This function counts and returns the
    //number of ignored layers in the array.
    int countIgnored() {
        int count = 0;
        foreach (var settings in _settings){
            if (settings.Ingore)
                count++;
        }
        return count;
    }
    void Decorate()
    {
        int ignored = countIgnored();
        for (int x = 0; x < 512; x++){
            float noise1d = 0;
            for (int k = 0; k < _settings.Length; k++){
                var settings = _settings[k];
                if (!settings.Ingore && settings.noiseType == noise_settings.noise_type.Surface){
                    noise1d += Mathf.PerlinNoise(
                    (offset.x + .5f + x + (settings.Seed * 128)) * settings.size,
                    1) / (_settings.Length - ignored) * settings.Amplifier;
                }
            }
            for (int y = Mathf.RoundToInt(noise1d * mul + additionalHeight); y > 0; y--){
                if (y > noise1d * mul + additionalHeight - (noise1d * 256f)){
                    Vector3Int currentpos = new Vector3Int(x, y, 0);
                    if (getTile(currentpos) != null){
                        placeTile(x, y, tileholder.grass);
                    }
                    else { }

                }
            }
        }
    }
    //A function that simplifies the tile data checking.
    TileBase getTile(Vector3Int vec)
    {
        return tilemap.GetTile(vec);
    }
    //A function that simplifies the tile placement.
    void placeTile(int x, int y, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(x,y,0), tile);
    }
    int findCaveLayer()
    {
        int index = 0;
        foreach (var item in _settings)
        {
            if (item.noiseType != noise_settings.noise_type.Caves)
            {
                index++;
            }
            else
            {
                return index;
            }
        }
        return index;
    }
}

