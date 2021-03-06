﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PersonRouteManager : RouteManager
{
    static string rest_animation_name = "player_idle_front";

    public static Vector2 get_doorstep_offset(Orientation exit_home_orientation, Vector2 door_position, Vector2 dest_tile_center)
    {
        Vector2 offset = new Vector2(0, 0);
        if (exit_home_orientation == Orientation.North || exit_home_orientation == Orientation.South)
        {
            if (door_position.x < dest_tile_center.x)
                offset.x = -cell_width / 4;
            else { offset.x = cell_width / 4; }
        }
        else
        {
            if (door_position.y < dest_tile_center.y)
                offset.y = -cell_width / 4;
            else { offset.y = cell_width / 4; }
        }
        return offset;
    }

    public static Vector2 get_straight_walking_position(Vector2 middle_pos, Orientation orientation)
    {
        // todo: incorporate rotation here!
        switch (orientation)
        {
            case Orientation.North:
                middle_pos.y -= cell_width / 4;
                break;
            case Orientation.East:
                middle_pos.x -= cell_width / 4;
                break;
            case Orientation.West:
                middle_pos.x += cell_width / 4;
                break;
            case Orientation.South:
                middle_pos.y += cell_width / 4;
                break;
        }
        return middle_pos;
    }

    public static GameObject get_exit_door(Boxcar boxcar, Room room)
    {
        // get door that exits to the boxcar
        // RIGHT DOOR TOP RIGHT ALWAYS FACES THE OUTER TRACK (RIGHT DOOR TOP RIGHT IS ON BOTTOM OF ROOM. OUTER TRACK IS RIGHT IN FRONT OF IT
        GameObject door_parent;
        int is_inner = boxcar.station_track.inner;
        //print("is station track with boxcar inner? " + is_inner);
        //if (room.outer_door != null && room.inner_door != null) // 2 doors to choose from
        //{ // choose the door that is on the right side of the track
        if (is_inner == 0)
        {
            door_parent = room.outer_door_container;
            bool is_outer = room.outer_door_container.GetComponent<Door>().is_outer;
            Sprite door_sprite = room.outer_door_container.GetComponent<Door>().door_sprite;
        }
        else
        {
            door_parent = room.inner_door_container;
        }
        //}
        //else {
        //    if (room.outer_door != null) door_parent = room.outer_door_container;
        //    else { door_parent = room.inner_door_container; }
        //}
        return door_parent;
    }

    public static void get_hor_flip_with_location(Vector3 start_pos, Vector3 end_pos, GameObject person_go_instance)
    {
        if (end_pos.x > start_pos.x)
        {
            person_go_instance.GetComponent<SpriteRenderer>().flipX = false;
        }
        else {
            person_go_instance.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public static string get_animation_from_orientation(Orientation end_orientation, string action)
    {
        if (end_orientation == Orientation.North)
        {
            return "player_" + action + "_back";
        }
        else if (end_orientation == Orientation.South)
        {
            return "player_" + action + "_front";
        }
        else if (end_orientation == Orientation.West || end_orientation == Orientation.East)
        {
            return "player_" + action + "_hor";
        }
        else
        {
            throw new Exception("not a valid orientation for getting animation from orientation");
        }
    }

    public IEnumerator board_train(Boxcar boxcar, Room room, GameObject occupant_go, Vector3Int destination)
    {
        Person occupant = occupant_go.GetComponent<Person>();
        GameManager.update_game_money_text(occupant.ticket_cost_map[occupant.desired_activity]);
        occupant.pop_thought_bubble();
        occupant.board_train();
        occupant.is_board_boxcar = true;
        boxcar.passenger_go = occupant_go;
        boxcar.is_occupied = true; // prevent another pserson from boarding the same boxcar
        occupant.boxcar_go = boxcar.gameObject;
        boxcar.is_being_boarded = true;
        room.booked = false;
        //print("set room instance to NULL");
        room.person_go_instance = null;
        boxcar.passenger_go = occupant_go;
        GameObject door = get_exit_door(boxcar, room);
        room.unlocked_door = door;
        room.booked = false; // new stuff
        Door unlocked_door = room.unlocked_door.GetComponent<Door>();
        // 2 checkpoints. In first go to doorstep and rotate accordingly. In the next only rotate to face track direction
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>();
        Vector3Int train_start_location = boxcar.train.station_track.start_location; // id of track
        Orientation exit_orientation = CityManager.station_track_boarding_map[train_start_location];
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_orientation, room.tile_position);
        string track_name = shipyard_track_tilemap.GetTile((Vector3Int)doorstep_position).name;
        occupant.final_dest_tile_pos = boxcar.tile_position;
        occupant.final_dest_pos = boxcar.transform.position;
        //start_orientation = CityManager.board_train_orientation_dict[boxcar.station_track.inner, 0];
        Station boxcar_station = boxcar.station_track.station;
        Orientation start_orientation = CityManager.board_train_orientation_dict[boxcar_station.orientation][boxcar.station_track.inner, 0];
        Orientation end_orientation = CityManager.board_train_orientation_dict[boxcar_station.orientation][boxcar.station_track.inner, 1];
        occupant.arrived_at_room = false; 
        yield return StartCoroutine(unlocked_door.rotate(true));
        string go_to_door_animation_name = get_animation_from_orientation(end_orientation,"walk");
        if (end_orientation == Orientation.West) occupant_go.GetComponent<SpriteRenderer>().flipX = true;
        else if (end_orientation == Orientation.East) occupant_go.GetComponent<SpriteRenderer>().flipX = false;
        yield return StartCoroutine(occupant.set_animation_clip(go_to_door_animation_name));
        yield return StartCoroutine(occupant.bezier_move(occupant.transform, start_orientation, end_orientation));
        StartCoroutine(unlocked_door.rotate(false)); 
        // go to doorstep (update tile position and orientation)
        Vector3 original_euler = occupant.transform.eulerAngles;
        occupant.transform.eulerAngles = new Vector3(0, 0, occupant.orient_angle); // use angle just to get direction of travelfor offset
        Vector3 offset_pos = occupant.transform.position + occupant.transform.up * .25f;
        occupant.transform.eulerAngles = original_euler; // restore angle
        Checkpoint go_to_doorstep_cp = new Checkpoint(offset_pos, doorstep_position, end_orientation, end_orientation, "walk");
        board_train_checkpoints.Add(go_to_doorstep_cp);
        Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, occupant.transform.position, boxcar.transform.position, boxcar);
        Checkpoint align_track_cp = new Checkpoint(offset_pos, doorstep_position, end_orientation, align_track_orientation, "walk"); // dont move just rotate
            board_train_checkpoints.Add(align_track_cp); // no need to align with track if destination is same tile as exit
        // if destination is reached immediately move to boxcar
        yield return StartCoroutine(occupant.move_checkpoints(board_train_checkpoints)); // wait for all preboarding movements to end before going to boxcar
        room.has_person = false; // egghead is outside the room
        occupant.in_tile = false; // allow person to follow the track to the destination boxcar
                                  // if all goes well then boarding is all that's left
        occupant.exit_home();
        occupant.final_orientation = occupant.orientation;
    }

    public IEnumerator step_on_boxcar(GameObject person_go_instance, GameObject boxcar_go)
    {
        Person person = person_go_instance.GetComponent<Person>();
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Vector2 boxcar_position = boxcar_go.transform.position;
        Orientation final_orientation = TrackManager.enter_boxcar_orientation(boxcar_position, person_go_instance.transform.position);
        //print("enter boxcar cp with person orientation " + person.orientation + " final orientation " + final_orientation + " boxcar tile pos " + boxcar.tile_position);
        Checkpoint step_on_boxcar_cp = new Checkpoint(boxcar.transform.position, (Vector2Int)boxcar.tile_position, person.orientation, final_orientation, "walk");
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>() { step_on_boxcar_cp };
        yield return StartCoroutine(person.move_checkpoints(board_train_checkpoints));
        StartCoroutine(person.set_animation_clip(rest_animation_name));
        person.transform.parent = boxcar.transform; // make person a passenger of boxcar
        boxcar.is_being_boarded = false;
    }

    public IEnumerator enter_home(GameObject person_go_instance, Room room)
    {
        List<Checkpoint> enter_home_checkpoints = new List<Checkpoint>();
        Person person = person_go_instance.GetComponent<Person>();
        Door unlocked_door = room.unlocked_door.GetComponent<Door>();
        Vector2 enter_door_location = room.unlocked_door.transform.position;
        Vector2 room_position = track_tilemap.GetCellCenterWorld((Vector3Int)room.tile_position);
        Orientation enter_home_orientation = get_orientation_from_position(person, room_position);
        Checkpoint enter_door_cp = new Checkpoint(enter_door_location, room.tile_position, person.orientation, enter_home_orientation, "walk");
        Checkpoint enter_home_cp = new Checkpoint(room_position, room.tile_position, enter_home_orientation, enter_home_orientation, "walk");
        enter_home_checkpoints.Add(enter_door_cp);
        enter_home_checkpoints.Add(enter_home_cp);
        yield return StartCoroutine(unlocked_door.rotate(true));
        yield return StartCoroutine(person.move_checkpoints(enter_home_checkpoints));
        StartCoroutine(person.set_animation_clip(rest_animation_name));
        StartCoroutine(unlocked_door.rotate(false));
        person.arrived_at_room = true;
        City city = person.room.building.city;
        person.finish_trip();
        if (city == CityManager.Activated_City_Component)
            city.apply_reputation(); // only add/subtract rooms when city is active
        StartCoroutine(person.schedule_activity()); // once arrived at home do something for scheduled time
        room.has_person = true;
    }

    public IEnumerator unload_train(Boxcar boxcar, Room room, Orientation exit_orientation)
    {
        List<Checkpoint> go_home_checkpoints = new List<Checkpoint>();
        GameObject person_go_instance = boxcar.passenger_go;
        boxcar.is_occupied = false;
        //print("unload train boxcar " + boxcar.boxcar_id +  " is NOT occupied");
        person_go_instance.transform.parent = null;
        Person person = person_go_instance.GetComponent<Person>();
        boxcar.passenger_go = null;
        person.boxcar_go = null;
        person.room = null;
        room.booked = true;
        room.person_go_instance = person_go_instance;
        //print("set room instance to PERSON " + person_go_instance.GetComponent<Person>().name);
        room.unlocked_door = get_exit_door(boxcar, room);
        // the final dest tile is offset from the person's route, so find the offset using city map
        Orientation home_orientation = CityManager.station_track_boarding_map[boxcar.station_track.start_location];
        person.room = room; // assign room to person
        person.city = room.building.city;
        person.final_dest_tile_pos = (Vector3Int)get_straight_next_tile_pos(home_orientation, room.tile_position);//(Vector3Int) room.tile_position;
        person.enter_home_orientation = TrackManager.flip_straight_orientation(home_orientation); // reverse direction of exiting the home
        Orientation flip_exit_orientation = TrackManager.flip_straight_orientation(exit_orientation); // need to flip because offset is in the opposite direction the person is traveling
        Vector2 exit_boxcar_dest = get_straight_walking_position(boxcar.transform.position, flip_exit_orientation);
        Checkpoint exit_boxcar_checkpoint = new Checkpoint(exit_boxcar_dest, (Vector2Int)boxcar.tile_position, person.orientation, exit_orientation, "walk");
        string track_name = shipyard_track_tilemap.GetTile(boxcar.tile_position).name;
        go_home_checkpoints.Add(exit_boxcar_checkpoint);
        Orientation exit_home_orientation = CityManager.station_track_boarding_map[boxcar.station_track.start_location];
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_home_orientation, room.tile_position);
        Vector2 room_abs_position = track_tilemap.GetCellCenterWorld((Vector3Int)doorstep_position); //one tile offset from home in the center. Because boxcars always stop at tile edges, so center tells us which direction person should exit boxcar
        person.final_dest_pos = room.unlocked_door.GetComponent<Door>().door_sprite_go.transform.position;
        yield return StartCoroutine(person.move_checkpoints(go_home_checkpoints));
        go_home_checkpoints.Clear();
        Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, boxcar.transform.position, room_abs_position, boxcar);
        Checkpoint align_track_cp = new Checkpoint(exit_boxcar_checkpoint.dest_pos, exit_boxcar_checkpoint.tile_position, exit_boxcar_checkpoint.end_orientation, align_track_orientation, "walk");
        go_home_checkpoints.Add(align_track_cp);
        yield return StartCoroutine(person.move_checkpoints(go_home_checkpoints));
        //boxcar.passenger_go = null; // TODOED TEST MOVE TO BEGIN OF FUNCTION
        //person.boxcar_go = null;
        person.is_exit_boxcar = true; // set off follow track back home sequence
    }
}