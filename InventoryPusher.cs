using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InventoryPusher : EventDetector
{
    Tilemap inventory_tilemap;
    GameObject dummy_boxcar;
    public GameObject dummy_bomb_boxcar;
    public GameObject dummy_supply_boxcar;
    public GameObject dummy_troop_boxcar;
    bool tile_in_station;
    Vector2Int selected_tile_pos;
    Tile selected_tile;

    // Start is called before the first frame update
    void Start()
    {
        inventory_tilemap = GetComponent<Tilemap>();
        tile_in_station = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        Vector2Int selected_tile_pos = GameManager.get_selected_tile(Input.mousePosition);
        selected_tile = (Tile) inventory_tilemap.GetTile((Vector3Int)selected_tile_pos);
        print("begin drag inventory " + selected_tile.name);
        if (selected_tile.name=="supply_boxcar")
        {
            dummy_boxcar = Instantiate(dummy_supply_boxcar);
        }
        else if (selected_tile.name=="bomb_boxcar")
        {
            dummy_boxcar = Instantiate(dummy_bomb_boxcar);
        }
        else if (selected_tile.name=="troop_boxcar")
        {
            dummy_boxcar = Instantiate(dummy_troop_boxcar);
        }
        else
        {
            throw new System.Exception("not a valid boxcar " + selected_tile.name);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        print("during drag inventory");
        Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
        dummy_boxcar.transform.position = world_position;
        selected_tile_pos = GameManager.get_selected_tile(Input.mousePosition);
        tile_in_station = CityManager.is_tile_in_station(selected_tile_pos);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        print("end drag inventory");
        if (tile_in_station)
        {
            GameManager.city_manager.add_boxcar_to_station(selected_tile.name, selected_tile_pos);
            // add boxcar to stations
        }
        Destroy(dummy_boxcar);
    }
}
