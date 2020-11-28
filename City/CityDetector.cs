using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityDetector : EventDetector
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool is_track_contain_valid_boxcar(bool is_outer, Station station, string building_type)
    {
        Station_Track adjacent_station_track;
        if (is_outer) adjacent_station_track = station.outer_track;
        else { adjacent_station_track = station.inner_track; }
        GameObject train_object = adjacent_station_track.train;
        if (train_object != null)
        {
            Train train = train_object.GetComponent<Train>();
            List<GameObject>boxcar_list = train.boxcar_squad;
            foreach (GameObject boxcar_object in boxcar_list)
            {
                Boxcar boxcar = boxcar_object.GetComponent<Boxcar>();
                List<string> valid_structure = CityManager.cargo_to_structure[boxcar.boxcar_type];
                if (valid_structure.Contains(building_type))
                    return true;
            }
        }
        return false;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("clicked city");
    }

    public void click_city(PointerEventData eventData)
    {
        if (GameManager.hint_context_list.Count == 0)
        {
            List<List<int[]>> city_action_coord = new List<List<int[]>>();
            print("set a city hint in frame " + Time.frameCount);
            // get station
            // get adjacent boarding track
            // if train is on track, then search for appropriate boxcars on track
            List<string> train_hint_list = new List<string>();
            Vector2Int selected_tile = GameManager.get_selected_tile(eventData.position);
            Building cb = CityManager.Activated_City_Component.city_building_grid[selected_tile.x, selected_tile.y];
            Station cb_station = CityManager.Activated_City_Component.get_station_track(selected_tile).station;
            RouteManager.Orientation orientation = cb_station.orientation;
            bool[] outer_inner_arr = TrackManager.is_city_building_inner(selected_tile, orientation); // 0 means outer, 1 means inner
            bool is_outer_track_valid = false;
            bool is_inner_track_valid = false;
            if (outer_inner_arr[0]) is_outer_track_valid = is_track_contain_valid_boxcar(true, cb_station, cb.building_type);
            if (outer_inner_arr[1]) is_inner_track_valid = is_track_contain_valid_boxcar(false, cb_station, cb.building_type);
            if (is_outer_track_valid || is_inner_track_valid)
            {
                if (is_outer_track_valid)
                    city_action_coord.Add(TrackManager.add_to_train_coord_map[orientation][0]);
                if (is_inner_track_valid)
                    city_action_coord[0].AddRange(TrackManager.add_to_train_coord_map[orientation][1]); // add to the same action

                train_hint_list.Add("board");
                GameObject.Find("GameManager").GetComponent<GameManager>().mark_tile_as_eligible(city_action_coord, train_hint_list, gameObject, true);
            }
        }
    }
}