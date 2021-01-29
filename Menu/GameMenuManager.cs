using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class GameMenuManager : EventDetector
{
    public GameObject ne_curve;
    public GameObject es_curve;
    public GameObject wn_curve;
    public GameObject ws_curve;
    public GameObject hor_track;
    public GameObject vert_track;
    public GameObject boxcar;
    public GameObject restaurant;
    public GameObject scenery;
    public GameObject mansion;
    public GameObject diner;
    public GameObject factory;
    public GameObject apartment;
    public GameObject business;

    public Camera camera;

    public Tile ES_tile;
    public Tile NE_tile;
    public Tile WN_tile;
    public Tile hor_tile;
    public Tile WS_tile;
    public Tile vert_tile;
    public Tile clicked_tile;

    public Tile wealthy_tile;
    public Tile poor_tile;
    public Tile diner_tile;
    public Tile factory_tile;
    public Tile pond_tile;
    public Tile entrance_tile;
    public Tile restaurant_tile;
    public Tile business_tile;

    string item_name;

    GameObject clicked_item;

    public Texture empty_inventory_bubble;
    public Texture food_inventory_bubble;
    public Texture home_inventory_bubble;
    public Texture work_inventory_bubble;
    public Texture track_inventory_bubble;

    GameObject clicked_go;

    protected GameObject train_object; // the train is referenced in TrainDisplay which has MenuManager as a base class
    Building building_component;


    City city;
    void Start()
    {
        camera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void update_inventory()
    {
        List<GameObject> Track_Gameobject_List = new List<GameObject>();
        random_algos.dfs_find_child_objects(transform, Track_Gameobject_List, new string[] { "Text" });
        foreach (GameObject inventory_item in Track_Gameobject_List)
        {
            Text item_count = inventory_item.GetComponent<Text>();
            string item_name = inventory_item.transform.parent.name;
            switch (item_name)
            {
                case "vert":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "hor":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "NE":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "WS":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "WN":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "ES":
                    item_count.text = "x" + TrackManager.get_track_count(item_name).ToString();
                    break;
                case "restaurant":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "factory":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "Apartment":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "business":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "mansion":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "diner":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                case "scenery":
                    item_count.text = "x" + CityManager.get_building_count(item_name).ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        List<List<int[]>> track_action_coord = new List<List<int[]>>();
        List<string> track_hint_list = new List<string>();
        item_name = eventData.pointerCurrentRaycast.gameObject.name;
        Tilemap structure_tilemap = GameManager.Structure.GetComponent<Tilemap>();
        List<int[]> track_action_list = new List<int[]>();
        track_hint_list.Add("track");
        switch (item_name)
        {
            case "ES":
                clicked_tile = ES_tile;
                break;
            case "NE":
                clicked_tile = NE_tile;
                break;
            case "WN":
                clicked_tile = WN_tile;
                break;
            case "WS":
                clicked_tile = WS_tile;
                break;
            case "hor":
                clicked_tile = hor_tile;
                break;
            case "vert":
                clicked_tile = vert_tile;
                break;
            default:
                print("not a valid track selected in game menu manager");
                break;
        }
        for (int i=0; i < BoardManager.track_width; i++)
        {
            for (int j = 0; j < BoardManager.track_height; j++)
            {
                Tile struct_tile = (Tile)structure_tilemap.GetTile(new Vector3Int(i, j, 0));
                if (struct_tile == null) // if no city on this tile you can place it here
                {
                    track_action_list.Add(new int[] { i, j });
                }
            }
        }
        track_action_coord.Add(track_action_list);
        GameObject.Find("GameManager").GetComponent<GameManager>().mark_tile_as_eligible(track_action_coord, track_hint_list, gameObject);
    }

    public void add_inventory_texture(string item_name)
    {
        GameObject menu_go = gameObject.transform.Find(item_name).gameObject;
        if (item_name == "hor" || item_name == "WN" || item_name == "NE" || item_name == "WS" || item_name == "ES" || item_name == "vert")
        {
            menu_go.GetComponent<RawImage>().texture = track_inventory_bubble; 
        }
        else // structure
        {
            if (item_name == "Apartment" || item_name == "mansion")
            {
                menu_go.GetComponent<RawImage>().texture = home_inventory_bubble;
            }
            else if (item_name == "factory" || item_name == "business")
            {

                menu_go.GetComponent<RawImage>().texture = work_inventory_bubble;
            }
            else if (item_name == "diner" || item_name == "restaurant")
            {
                menu_go.GetComponent<RawImage>().texture = food_inventory_bubble;
            }
            else
            {
                throw new Exception("cannot find a structure with name " + item_name);
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        try
        {
            clicked_go = eventData.pointerCurrentRaycast.gameObject;
            item_name = clicked_go.name;
            string tag = eventData.pointerCurrentRaycast.gameObject.tag;
            Vector3 position = MenuManager.convert_screen_to_world_coord(eventData.position);
            clicked_tile = null; // reset this variable
            if ((tag == "track" && TrackManager.get_track_count(item_name) <= 0) || // cancel the drag if none available
                (tag == "structure" && CityManager.get_building_count(item_name) <= 0))
            {
                eventData.pointerDrag = null;
                return;
            }
            switch (item_name)
            {
                case "ES":
                    clicked_item = Instantiate(es_curve, position, Quaternion.identity);
                    clicked_tile = ES_tile;
                    break;
                case "NE":
                    clicked_item = Instantiate(ne_curve, position, Quaternion.identity);
                    clicked_tile = NE_tile;
                    break;
                case "WN":
                    clicked_item = Instantiate(wn_curve, position, Quaternion.identity);
                    clicked_tile = WN_tile;
                    break;
                case "WS":
                    clicked_item = Instantiate(ws_curve, position, Quaternion.identity);
                    clicked_tile = WS_tile;
                    break;
                case "hor":
                    clicked_item = Instantiate(hor_track, position, Quaternion.identity);
                    clicked_tile = hor_tile;
                    break;
                case "vert":
                    clicked_item = Instantiate(vert_track, position, Quaternion.identity);
                    clicked_tile = vert_tile;
                    break;
                case "boxcar":
                    clicked_item = Instantiate(boxcar, position, Quaternion.identity);
                    break;
                case "restaurant": 
                    clicked_item = Instantiate(restaurant, position, Quaternion.identity);
                    clicked_tile = restaurant_tile;
                    building_component = clicked_item.GetComponent<Restaurant>();
                    break;
                case "mansion": 
                    clicked_item = Instantiate(mansion, position, Quaternion.identity);
                    clicked_tile = wealthy_tile;
                    building_component = clicked_item.GetComponent<Mansion>();
                    break;
                case "diner":
                    clicked_item = Instantiate(diner, position, Quaternion.identity);
                    clicked_tile = diner_tile;
                    building_component = clicked_item.GetComponent<Diner>();
                    break;
                case "factory": 
                    clicked_item = Instantiate(factory, position, Quaternion.identity);
                    clicked_tile = factory_tile;
                    building_component = clicked_item.GetComponent<Factory>();
                    break;
                case "Apartment":
                    clicked_item = Instantiate(apartment, position, Quaternion.identity);
                    building_component = clicked_item.GetComponent<Apartment>();
                    clicked_tile = poor_tile;
                    break;
                case "business":
                    clicked_item = Instantiate(business, position, Quaternion.identity);
                    building_component = clicked_item.GetComponent<Business>();
                    clicked_tile = business_tile;
                    break;
                default:
                    break;
            }
            if (tag == "structure")
            {
                building_component.enabled = false;
            }
        }
        catch (NullReferenceException)
        {
            print("null");
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Vector3 world_position = MenuManager.convert_screen_to_world_coord(eventData.position);
        clicked_item.transform.position = world_position;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (clicked_go.tag == "structure")
            building_component.enabled = true;
        TrackManager track_manager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        Vector2Int final_tilemap_position = GameManager.get_selected_tile(eventData.position);
        try
        {
            string tag = clicked_item.tag;
            string item_name = clicked_item.name.Replace("(Clone)", ""); // remove clone from the game object name
            Tilemap structure_tilemap = GameManager.Structure.GetComponent<Tilemap>();
            List<Tile> track_tile = GameManager.track_manager.track_grid[final_tilemap_position.x,final_tilemap_position.y];
            Tile city_tile = (Tile)structure_tilemap.GetTile((Vector3Int)final_tilemap_position);
            if (GameManager.is_position_in_bounds(final_tilemap_position))
            {
                if (tag == "structure")
                {
                    if (city_tile == null && track_tile.Count == 0)
                    {
                        RouteManager.city_tilemap.SetTile((Vector3Int)final_tilemap_position, clicked_tile);
                        GameManager.city_manager.create_city((Vector3Int)final_tilemap_position);
                        CityManager.update_building_count(item_name, -1);
                        if (CityManager.get_building_count(item_name) == 0)
                        {
                            RawImage raw_image = clicked_go.GetComponent<RawImage>();
                            raw_image.texture = empty_inventory_bubble;
                        }
                    }
                }
                else if (tag == "track")
                {
                    if (city_tile == null)
                    {
                        track_manager.place_tile(final_tilemap_position, clicked_tile, true);
                        TrackManager.update_track_count(item_name, -1);
                        if (TrackManager.get_track_count(item_name) == 0)
                        {
                            RawImage raw_image = clicked_go.GetComponent<RawImage>();
                            raw_image.texture = empty_inventory_bubble;
                        }
                    }
                }
            }
            if (clicked_item != null)
            {
                Destroy(clicked_item);
            }
        }
        catch (NullReferenceException e)
        {
            print(e.StackTrace);
        }
        catch (MissingReferenceException e)
        {
            print(e.Message); // tried to drag something that is not draggable
        }
        update_inventory();
    }

}
