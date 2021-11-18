using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class player_controller : MonoBehaviour
{
    public Tilemap world;
    public world_generator Generator;
    public GameObject _outline;
    public TileHolder tileholder;
    float moveSpeed = 0.3f;
    void Update()
    {
        moveCamera();
        outline();
        breakAndPlace();
    }
    void breakAndPlace() {
        Vector3Int pos = mousePosition();
        Vector2Int cpos = new Vector2Int(pos.x/16, pos.y/16);
        if(Input.GetMouseButton(1)) {
            if(checkPlacable()[0]&&!checkPlacable()[1]) {
                world.SetTile(pos, tileholder.dirt);
                Generator.IsModified.TryGetValue(cpos, out bool check);
                if(Generator.IsModified.ContainsKey(cpos)&&!check) {
                    Generator.IsModified.Remove(cpos);
                    Generator.IsModified.Add(cpos, true);
                }
            }
        }
        if(Input.GetMouseButton(0)) {
            if(checkPlacable()[1]) {
                world.SetTile(pos, null);
                Generator.IsModified.TryGetValue(cpos, out bool check);
                if(Generator.IsModified.ContainsKey(cpos) && !check) {
                    Generator.IsModified.Remove(cpos);
                    Generator.IsModified.Add(cpos, true);
                }
                
            }
        }
    }
    void outline() {
        _outline.transform.position=new Vector2(world.CellToWorld(mousePosition()).x+.5f, world.CellToWorld(mousePosition()).y+.5f);
    }
    bool[] checkPlacable() {
        var outline_pos = mousePosition();
        var right = world.GetTile(new Vector3Int(outline_pos.x+1, outline_pos.y, 0)) != null;
        var left = world.GetTile(new Vector3Int(outline_pos.x-1, outline_pos.y, 0))!=null;
        var up = world.GetTile(new Vector3Int(outline_pos.x, outline_pos.y+1, 0))!=null;
        var down = world.GetTile(new Vector3Int(outline_pos.x, outline_pos.y-1, 0))!=null;
        bool hasBlockAround = false;
        if(right || left || up || down) {
            hasBlockAround=true;
        }
        bool hasBlockOnPosition = false;
        if(world.GetTile(new Vector3Int((int)outline_pos.x, (int)outline_pos.y,0)) != null) {
            hasBlockOnPosition=true;
        }
        return new bool[] { hasBlockAround, hasBlockOnPosition };
    }

    Vector3Int mousePosition() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y), 0);
    }
    void moveCamera() {
        if(Input.GetAxis("Mouse ScrollWheel")>0f&&moveSpeed<1) // forward
        {
            moveSpeed=moveSpeed+.1f;
        } else if(Input.GetAxis("Mouse ScrollWheel")<0f&&moveSpeed>0.1f) // backwards
          {
            moveSpeed=moveSpeed-.1f;
        }
        if(Input.GetKey(KeyCode.W)) {
            this.transform.position=new Vector3(this.transform.position.x, this.transform.position.y+moveSpeed, this.transform.position.z);
        }
        if(Input.GetKey(KeyCode.A)) {
            this.transform.position=new Vector3(this.transform.position.x-moveSpeed, this.transform.position.y, this.transform.position.z);
        }
        if(Input.GetKey(KeyCode.S)) {
            this.transform.position=new Vector3(this.transform.position.x, this.transform.position.y-moveSpeed, this.transform.position.z);
        }
        if(Input.GetKey(KeyCode.D)) {
            this.transform.position=new Vector3(this.transform.position.x+moveSpeed, this.transform.position.y, this.transform.position.z);
        }
    }
}
