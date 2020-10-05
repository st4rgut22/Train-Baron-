using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MenuManager : EventDetector
{
    public GameObject ne_curve;
    public GameObject es_curve;
    public GameObject wn_curve;
    public GameObject ws_curve;
    public GameObject hor_track;
    public GameObject vert_track;
    public GameObject train;
    public Camera camera;


    GameObject clicked_item;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Vector3 convert_screen_to_world_coord(Vector3 position)
    {
        Vector3 world_position = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(camera.transform.position.z)));
        position.z = 0;
        return world_position;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        string item_name = eventData.pointerCurrentRaycast.gameObject.name;
        print("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        Vector3 position = convert_screen_to_world_coord(eventData.position);
        switch (item_name)
        {
            case "ES":
                clicked_item = Instantiate(es_curve, position, Quaternion.identity);
                break;
            case "NE":
                clicked_item = Instantiate(ne_curve, position, Quaternion.identity);
                break;
            case "WN":
                clicked_item = Instantiate(wn_curve, position, Quaternion.identity);
                break;
            case "WS":
                clicked_item = Instantiate(ws_curve, position, Quaternion.identity);
                break;
            case "hor":
                clicked_item = Instantiate(hor_track, position, Quaternion.identity);
                break;
            case "vert":
                clicked_item = Instantiate(vert_track, position, Quaternion.identity);
                break;
            case "train":
                clicked_item = Instantiate(train, position, Quaternion.identity);
                break;
            default:
                print("You did not click a store item");
                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Vector3 world_position = convert_screen_to_world_coord(eventData.position);
        clicked_item.transform.position = world_position;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        GameObject bm = GameObject.Find("BoardManager");
        BoardManager board_manager = bm.GetComponent<BoardManager>();
        Vector3 final_world_position = convert_screen_to_world_coord(eventData.position);
        print("final world position of tile is " + final_world_position);
        try
        {
            board_manager.place_tile(final_world_position, clicked_item.name);
            print("tile placed");
            GameObject old_clicked_item = clicked_item;
            Destroy(old_clicked_item); // remove the prefab from scene
        } catch (NullReferenceException e){
            print(e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
