using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class player_controller : MonoBehaviour {
    public Tilemap world;
    public world_generator Generator;
    public GameObject _outline;
    public TileHolder tileholder;
    public TileBase selectedTile;
    public GameObject chat;
    public GameObject commandline;
    bool commandLineEnabled = false;
    float moveSpeed = 0.3f;
    void Start() {
        selectedTile=tileholder.Tiles["default:dirt"];
    }
    void Update() {
        commandLineController();
        if(!commandLineEnabled) {
            moveCamera();
            outline();
            breakAndPlace();
        }
    }
    void commandLineController() {
        if(Input.GetKeyDown(KeyCode.T)) {
            commandline.SetActive(true);
            commandline.GetComponent<InputField>().Select();
            commandLineEnabled=true;
        }
        if(Input.GetKeyDown(KeyCode.Return)&&commandLineEnabled) {
            commandline.GetComponent<InputField>().Select();
            commandline.SetActive(false);
            commandLineEnabled=false;
            if(commandline.GetComponent<InputField>().text!=null&&commandline.GetComponent<InputField>().text!="") {
                string[] args = commandline.GetComponent<InputField>().text.Split(' ');
                if(args[0]=="selectTile"||args[0]=="st") {
                    if(tileholder.Tiles.ContainsKey(args[1])) {
                        selectedTile=tileholder.Tiles[args[1]];
                        chat.GetComponent<Text>().text+="\n"+args[1]+" is selected.";
                    } else {
                        chat.GetComponent<Text>().text+="\nTile doesn't exist or you mistyped the tilename. Please use 'tilelist' command to list all existing tiles.";
                    }
                } else if(args[0]=="tilelist"||args[0]=="tl") {
                    string result = "\nTile list :\n";
                    foreach(var item in tileholder.Tiles) {
                        result+=item.Key+",";
                    }
                    chat.GetComponent<Text>().text+=result;
                } else if(args[0]=="help") {
                    chat.GetComponent<Text>().text+="\nExisting commands : \nselecttile,st <tilename> - Changes selected tile\ntilelist,tl - List all existing tile types\nclearchat,cc - Clears all text in chat";
                } else if(args[0]=="clearchat"||args[0]=="cc") {
                    chat.GetComponent<Text>().text="";
                } else {
                    chat.GetComponent<Text>().text+="\nThis command doesn't exist, try using 'help' command.";
                }
            }
            commandline.GetComponent<InputField>().text="";
        }
    }
    void OnSubmit() {

    }
    void breakAndPlace() {
        Vector3Int pos = mousePosition();
        Vector2Int cpos = new Vector2Int(pos.x/16, pos.y/16);
        if(Input.GetMouseButton(1)) {
            if(checkPlacable()[0]&&!checkPlacable()[1]) {
                if((cpos.x+cpos.y)%2==0) {
                    world.SetTile(pos, selectedTile);
                } else {
                    world.SetTile(pos, selectedTile);
                }
            }
        }
        if(Input.GetMouseButton(0)) {
            if(checkPlacable()[1]) {
                world.SetTile(pos, null);
            }
        }
    }
    void outline() {
        _outline.transform.position=new Vector2(world.CellToWorld(mousePosition()).x+.5f, world.CellToWorld(mousePosition()).y+.5f);
    }
    bool[] checkPlacable() {
        var outline_pos = mousePosition();
        var right = world.GetTile(new Vector3Int(outline_pos.x+1, outline_pos.y, 0))!=null;
        var left = world.GetTile(new Vector3Int(outline_pos.x-1, outline_pos.y, 0))!=null;
        var up = world.GetTile(new Vector3Int(outline_pos.x, outline_pos.y+1, 0))!=null;
        var down = world.GetTile(new Vector3Int(outline_pos.x, outline_pos.y-1, 0))!=null;
        bool hasBlockAround = false;
        if(right||left||up||down) {
            hasBlockAround=true;
        }
        bool hasBlockOnPosition = false;
        if(world.GetTile(new Vector3Int((int)outline_pos.x, (int)outline_pos.y, 0))!=null) {
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
