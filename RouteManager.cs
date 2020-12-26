using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class RouteManager : MonoBehaviour
{
    // all functions for routing moving objects


    public static GameObject Track_Layer;
    public static Tilemap track_tilemap;

    public static Tilemap city_tilemap;
    public static Tilemap shipyard_track_tilemap;
    public static Tilemap shipyard_track_tilemap2;
    public static Tilemap exit_north_tilemap;
    public static Tilemap exit_south_tilemap;
    public static Tilemap exit_east_tilemap;
    public static Tilemap exit_west_tilemap;
    public static float cell_width = .88f;
    public static float offset_amount = .1f;

    public static Vector2 offset_right = new Vector2(cell_width / 4, 0);
    public static Vector2 offset_left = new Vector2(-cell_width / 4, 0);
    public static Vector2 offset_up = new Vector2(0, cell_width / 4);
    public static Vector2 offset_down = new Vector2(0, -cell_width / 4);
    public static Vector2 offset_diag_ne = new Vector2(cell_width / 4, cell_width / 4);
    public static Vector2 offset_diag_sw = new Vector2(-cell_width / 4, -cell_width / 4);
    public static Vector2 offset_diag_se = new Vector2(-cell_width / 4, cell_width / 4);
    public static Vector2 no_offset = new Vector2(0, 0);
    public static Dictionary<Vector3Int, Dictionary<string, Vector2>> offset_route_map;

    public enum Orientation
    {
        None,
        North,
        East,
        West,
        South,
        ne_SteepCurve,  // one of the four steep curve tracks
        ne_LessSteepCurve,  // one of the four steep curve tracks
        nw_SteepCurve,
        nw_LessSteepCurve,
        se_SteepCurve,
        se_LessSteepCurve,
        sw_SteepCurve,
        sw_LessSteepCurve,
        Right_Angle,
        Steep_Angle,
        Less_Steep_Angle
    }

    private void Awake()
    {
        Track_Layer = GameObject.Find("Top Track Layer");
        track_tilemap = Track_Layer.GetComponent<Tilemap>();
        exit_north_tilemap = GameObject.Find("Shipyard Track Exit North").GetComponent<Tilemap>();
        exit_south_tilemap = GameObject.Find("Shipyard Track Exit South").GetComponent<Tilemap>();
        exit_west_tilemap = GameObject.Find("Shipyard Track Exit West").GetComponent<Tilemap>();
        exit_east_tilemap = GameObject.Find("Shipyard Track Exit East").GetComponent<Tilemap>();
        shipyard_track_tilemap = GameObject.Find("Shipyard Track").GetComponent<Tilemap>();
        shipyard_track_tilemap2 = GameObject.Find("Shipyard Track 2").GetComponent<Tilemap>();
        city_tilemap = GameObject.Find("Structure").GetComponent<Tilemap>();

        offset_route_map = new Dictionary<Vector3Int, Dictionary<string, Vector2>>()
        {
            // a dictionary of offsets for person leave offset calculations
            {
                // no offset for the curves, because this is already applied in bezier movement
                CityManager.north_start_outer,
                    new Dictionary<string, Vector2>(){
                        {
                            "hor", offset_down
                        },
                        {
                            "vert", offset_left
                        },
                        {
                            "NE", offset_left
                        }
                    }
            },
            {
                CityManager.north_start_inner,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        },
                        {
                            "NE", offset_right
                        },
                        {
                            "WS", offset_up
                        }
                    }
            },
            {
                CityManager.east_start_outer,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        },
                        {
                            "ES", offset_right
                        },
                        {
                            "vert", offset_right
                        }
                    }
            },
            {
                CityManager.east_start_inner,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        }
                    }
            },
            {
                CityManager.west_start_outer,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_up
                        },
                        {
                            "WN", offset_up
                        },
                        {
                            "vert", offset_left
                        }
                    }
            },
            {
                CityManager.west_start_inner,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        }
                    }
            },
            {
                CityManager.south_start_outer,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "vert", offset_right
                        },
                        {
                            "WS", offset_right
                        },
                        {
                            "hor", offset_up
                        }
                    }
            },
            {
                CityManager.south_start_inner,
                    new Dictionary<string, Vector2>()
                    {
                        {
                            "hor", offset_down
                        },
                        {
                            "vert", offset_left
                        },
                        {
                            "NE", offset_down
                        },
                        {
                            "WS", offset_left
                        }
                    }
            }
        };
    }

    public static Vector2 get_straight_final_dest(Orientation orientation, Vector2 tile_world_coord)
    {
        //destination absolute position is offset from center of destination tile
        switch (orientation)
        {
            case Orientation.North:
                return new Vector2(tile_world_coord[0], tile_world_coord[1] + cell_width / 2);
            case Orientation.East:
                return new Vector2(tile_world_coord[0] + cell_width / 2, tile_world_coord[1]);
            case Orientation.West:
                return new Vector2(tile_world_coord[0] - cell_width / 2, tile_world_coord[1]);
            case Orientation.South:
                return new Vector2(tile_world_coord[0], tile_world_coord[1] - cell_width / 2);
            default:
                throw new Exception("orientation not valid");
        }
    }

    public static Orientation get_orientation_from_tile_position(Vector2Int start_tile_pos, Vector2Int end_tile_pos)
    {
        if (start_tile_pos.x == end_tile_pos.x && end_tile_pos.y < start_tile_pos.y) return Orientation.South;
        else if (start_tile_pos.x == end_tile_pos.x && end_tile_pos.y > start_tile_pos.y) return Orientation.North;
        else if (start_tile_pos.y == end_tile_pos.y && end_tile_pos.x > start_tile_pos.x) return Orientation.East;
        else if (start_tile_pos.y == end_tile_pos.y && end_tile_pos.x < start_tile_pos.x) return Orientation.West;
        else
        {
            throw new Exception("cannot find orientation based on tile position");
        }
    }

    public static PositionPair get_next_tile_pos(Tilemap tilemap, Tile track_tile, Simple_Moving_Object moving_thing, Vector3Int tile_coord, Vector2 offset)
    {
        // get the next tile position for train movement AND placing boxcars (reverse orientation)
        Vector2Int next_tilemap_pos = new Vector2Int(tile_coord.x, tile_coord.y);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        Vector2 final_cell_dest = moving_thing.transform.position; // end of track default destination ( dont move )
        try
        {
            string tile_name = track_tile.name;
            switch (tile_name)
            {
                //tricky curve tile updates. the train has already arrived in the tile so only adjust one coordinate
                case "ES":
                    if (moving_thing.orientation == Orientation.West || moving_thing.orientation == Orientation.South)
                    {
                        moving_thing.final_orientation = Orientation.South;
                        final_cell_dest = get_straight_final_dest(Orientation.South, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    else if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.East) // 2nd condition is for opposite direction when placing boxcars using flipped orientation
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "NE":
                    if (moving_thing.orientation == Orientation.South || moving_thing.orientation == Orientation.East)
                    {
                        moving_thing.final_orientation = Orientation.East;
                        final_cell_dest = get_straight_final_dest(Orientation.East, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
                    }
                    else if (moving_thing.orientation == Orientation.West || moving_thing.orientation == Orientation.North)
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "WN":
                    if (moving_thing.orientation == Orientation.East || moving_thing.orientation == Orientation.North)
                    {
                        moving_thing.final_orientation = Orientation.North;
                        final_cell_dest = get_straight_final_dest(Orientation.North, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
                    }
                    else if (moving_thing.orientation == Orientation.South || moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "WS":
                    if (moving_thing.orientation == Orientation.East || moving_thing.orientation == Orientation.South)
                    {
                        moving_thing.final_orientation = Orientation.South;
                        final_cell_dest = get_straight_final_dest(Orientation.South, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
                    }
                    else if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.West)
                    {
                        moving_thing.final_orientation = Orientation.West;
                        final_cell_dest = get_straight_final_dest(Orientation.West, tile_world_coord);
                        next_tilemap_pos = new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "vert":
                    if (moving_thing.orientation == Orientation.North || moving_thing.orientation == Orientation.South)
                    {
                        final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                        next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "hor":
                    if (moving_thing.orientation == Orientation.East || moving_thing.orientation == Orientation.West)
                    {
                        final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                        next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
                    }
                    else { throw new NullReferenceException(); }
                    break;
                case "ne_diag":
                    moving_thing.final_orientation = Orientation.ne_SteepCurve;
                    break;
                case "nw_diag":
                    moving_thing.final_orientation = Orientation.nw_SteepCurve;
                    break;
                case "se_diag":
                    moving_thing.final_orientation = Orientation.se_SteepCurve;
                    break;
                case "sw_diag":
                    moving_thing.final_orientation = Orientation.sw_SteepCurve;
                    break;
                case "less_diag_ne_turn":
                    moving_thing.final_orientation = Orientation.ne_LessSteepCurve;
                    break;
                case "less_diag_nw_turn":
                    moving_thing.final_orientation = Orientation.nw_LessSteepCurve;
                    break;
                case "less_diag_se_turn":
                    moving_thing.final_orientation = Orientation.se_LessSteepCurve;
                    break;
                case "less_diag_sw_turn":
                    moving_thing.final_orientation = Orientation.sw_LessSteepCurve;
                    break;
                default:
                    moving_thing.final_orientation = Orientation.None;
                    //print("none of the track tiles matched"); // return current position
                    break;
            }
            if (tile_name == "ne_diag" || tile_name == "nw_diag" || tile_name == "se_diag" || tile_name == "sw_diag" || tile_name == "less_diag_ne_turn" ||
                tile_name == "less_diag_nw_turn" || tile_name == "less_diag_se_turn" || tile_name == "less_diag_sw_turn")
            {
                final_cell_dest = get_straight_final_dest(moving_thing.orientation, tile_world_coord);
                next_tilemap_pos = get_straight_next_tile_pos(moving_thing.orientation, next_tilemap_pos);
            }
        }
        catch (NullReferenceException e)
        {
            final_cell_dest = tile_world_coord;
            //print("Vehicle Should not reach end of track due to look ahead. tilemap " + tilemap + " position of " + moving_thing.name + " is " + moving_thing.tile_position);
            print(e.Message);
        }
        //print("final cell dest without offset is " + final_cell_dest + " + with offset is " + (final_cell_dest+offset));

        final_cell_dest += offset;
        return new PositionPair(final_cell_dest, next_tilemap_pos);
    }

    public static PositionPair get_destination(Simple_Moving_Object moving_thing, Tilemap tilemap, Vector2 offset)
    {
        // modify by offset for a person boarding a train (so hes not standing on the tracks)
        Vector3Int tile_coord = new Vector3Int(moving_thing.tile_position[0], moving_thing.tile_position[1], 0);
        Tile track_tile = (Tile)tilemap.GetTile(tile_coord);
        Vector2 tile_world_coord = tilemap.GetCellCenterWorld(tile_coord);
        PositionPair pos_pair = get_next_tile_pos(tilemap, track_tile, moving_thing, tile_coord, offset);
        if (!moving_thing.in_city) //not inside a city, so check if arrived at city
        {
            Tile city_tile = (Tile)city_tilemap.GetTile(tile_coord);
            if (city_tile != null) //check if arriving at city
            {
                City city = GameManager.city_manager.gameobject_board[tile_coord.x, tile_coord.y].GetComponent<City>(); // check if city arrived at is not the same city we're leaving
                if (city != moving_thing.prev_city)
                {
                    pos_pair.abs_dest_pos = tile_world_coord; // destination is the center of the tile
                    moving_thing.prepare_to_arrive_at_city(city);
                }
            }
        }
        return pos_pair;
    }

    public static Vector2Int get_straight_next_tile_pos_multiple(Orientation orientation, Vector2Int next_tilemap_pos, int times)
    {
        Vector2Int next_pos = next_tilemap_pos;
        for (int i = 0; i < times; i++)
        {
            next_pos = get_straight_next_tile_pos(orientation, next_pos);
        }
        return next_pos;
    }

    public static Vector2Int get_straight_next_tile_pos(Orientation orientation, Vector2Int next_tilemap_pos)
    {
        switch (orientation)
        {
            case Orientation.North:
                return new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y + 1);
            case Orientation.East:
                return new Vector2Int(next_tilemap_pos.x + 1, next_tilemap_pos.y);
            case Orientation.West:
                return new Vector2Int(next_tilemap_pos.x - 1, next_tilemap_pos.y);
            case Orientation.South:
                return new Vector2Int(next_tilemap_pos.x, next_tilemap_pos.y - 1);
            default:
                throw new Exception("orientation not valid");
        }
    }
}