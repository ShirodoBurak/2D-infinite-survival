using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TileData {
    public int[] x;
    public int[] y;
    public string[] TileType;
}


public class data_controller {
    public void saveData(Dictionary<Vector2Int, string> block_positions, Vector2Int chunk_pos) {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TerrariaCloneSave");

        if(!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        FileStream file = null;
        BinaryFormatter bf = new BinaryFormatter();
        TileData data = new TileData();

        data.x=new int[block_positions.Count];
        data.y=new int[block_positions.Count];
        data.TileType=new string[block_positions.Count];
        int index = 0;
        foreach(var item in block_positions) {
            data.x[index] = item.Key.x;
            data.y[index] = item.Key.y;
            data.TileType[index] = item.Value;
            index++;
        }
        if(File.Exists(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat")) {
            File.Delete(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat");
            using(file=File.Create(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat")) {
                bf.Serialize(file, data);
            }
        } else {
            using(file=File.Create(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat")) {
                bf.Serialize(file, data);
            }
        }
    }

    public TileData loadData(Vector2Int chunk_pos) {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TerrariaCloneSave");
        BinaryFormatter bf = new BinaryFormatter();
        TileData loaded_data;
        using(FileStream file = File.Open(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat", FileMode.Open, FileAccess.Read, FileShare.None)) { 
            loaded_data = (TileData)bf.Deserialize(file);
            file.Close();
            return loaded_data;
        }
    }
    public bool checkChunkFileExists(Vector2Int chunk_pos) {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TerrariaCloneSave");
        return File.Exists(path+"/"+chunk_pos.x+"-"+chunk_pos.y+".dat");
    }
}
