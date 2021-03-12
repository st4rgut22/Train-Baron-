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
    Vector2Int boxcar_selected_tile_pos;
    public Vector2Int selected_tile_pos;
    public Tile selected_tile;

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

    public void click_inventory(PointerEventData eventData)
    {
        // NOT GETTING CALLED
        selected_tile_pos = GameManager.get_selected_tile(eventData.position);
        selected_tile = (Tile)inventory_tilemap.GetTile((Vector3Int)selected_tile_pos);
        if (selected_tile != null)
        {
            List<List<int[]>> boxcar_action_coord = new List<List<int[]>>();
            List<int[]> valid_unloading_pos_list = new List<int[]>();
            Station_Track station_track = CityManager.Activated_City_Component.get_station_track(selected_tile_pos); // inner, outer doesnt matter
            RouteManager.Orientation[] orientation_list = new RouteManager.Orientation[] { RouteManager.Orientation.North, RouteManager.Orientation.East, RouteManager.Orientation.West, RouteManager.Orientation.South };
            foreach (RouteManager.Orientation orientation in orientation_list)
            {
                List<List<int[]>> valid_boxcar_add_pos = TrackManager.add_to_train_coord_map[orientation];

                bool train_in_outer_track = CityManager.Activated_City_Component.is_train_on_track(orientation, true);
                bool train_in_inner_track = CityManager.Activated_City_Component.is_train_on_track(orientation, false);

                //concatenate lists of outer track and inner track positions
                if (train_in_outer_track)
                    valid_unloading_pos_list.AddRange(valid_boxcar_add_pos[0]);
                if (train_in_inner_track)
                    valid_unloading_pos_list.AddRange(valid_boxcar_add_pos[1]);
                boxcar_action_coord.Add(valid_unloading_pos_list);
            }
            List<string> hint_context_list = new List<string>() { "add" };
            GameObject.Find("GameManager").GetComponent<GameManager>().mark_tile_as_eligible(boxcar_action_coord, hint_context_list, gameObject, true);
        }
    }
}

//public override void OnBeginDrag(PointerEventData eventData)
//{
    //boxcar_selected_tile_pos = GameManager.get_selected_tile(Input.mousePosition);
    //selected_tile = (Tile) inventory_tilemap.GetTile((Vector3Int)boxcar_selected_tile_pos);


    //Vector2Int tile_pos = GameManager.get_selected_tile(transform.position);
    //RouteManager.Orientation orientation;
    //if (tile_pos.x < 7 && tile_pos.y < 4) orientation = RouteManager.Orientation.West;
    //else if (tile_pos.x > 9 && tile_pos.y < 4) orientation = RouteManager.Orientation.South;
    //else if (tile_pos.x < 7 && tile_pos.y > 6) orientation = RouteManager.Orientation.North;
    //else if (tile_pos.x > 9 && tile_pos.y > 6) orientation = RouteManager.Orientation.East;
    //else { throw new System.Exception("not a valid tile for parkedboxcar"); }
    //List<int[]> valid_boxcar_add_pos = TrackManager.add_to_train_coord_map[orientation];
    //boxcar_action_coord.Add(valid_boxcar_add_pos);
    //List<string> hint_context_list = new List<string>() { "add" };
    //game_manager.mark_tile_as_eligible(boxcar_action_coord, hint_context_list, gameObject, true);


    //if (selected_tile.name=="supply_boxcar")
    //{
    //    dummy_boxcar = Instantiate(dummy_supply_boxcar);
    //}
    //else if (selected_tile.name=="bomb_boxcar")
    //{
    //    dummy_boxcar = Instantiate(dummy_bomb_boxcar);
    //}
    //else if (selected_tile.name=="troop_boxcar")
    //{
    //    dummy_boxcar = Instantiate(dummy_troop_boxcar);
    //}
    //else
    //{
    //    throw new System.Exception("not a valid boxcar " + selected_tile.name);
    //}
//}

    //public override void OnDrag(PointerEventData eventData)
    //{
    //    Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
    //    dummy_boxcar.transform.position = world_position;
    //    selected_tile_pos = GameManager.get_selected_tile(Input.mousePosition);
    //    tile_in_station = CityManager.is_tile_in_station(selected_tile_pos);
    //    if (tile_in_station)
    //    {
    //        RouteManager.Orientation orientation = CityManager.get_station_orientation(selected_tile_pos);
    //        if (orientation==RouteManager.Orientation.North || orientation == RouteManager.Orientation.West)
    //        {
    //            dummy_boxcar.transform.eulerAngles = new Vector3(0, 0, -90);
    //        }
    //        else
    //        {
    //            dummy_boxcar.transform.eulerAngles = new Vector3(0, 0, 90);
    //        }
    //    }
    //    else
    //    {
    //        dummy_boxcar.transform.eulerAngles = new Vector3(0, 0, 0);
    //    }
    //}

    //public override void OnEndDrag(PointerEventData eventData)
    //{
    //    if (tile_in_station)
    //    {
    //        GameManager.city_manager.add_boxcar_to_station(selected_tile.name, selected_tile_pos, boxcar_selected_tile_pos);
    //        // add boxcar to stations
    //    }
    //    Destroy(dummy_boxcar);
    //}
//}
