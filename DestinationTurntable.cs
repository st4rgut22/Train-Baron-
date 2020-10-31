using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DestinationTurntable : EventDetector
{
    // Start is called before the first frame update
    private void Start()
    {

    }

    private void Update()
    {
        
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        print("pointer clicked " + gameObject.name);
        if (gameObject.name == "Shipyard Track Exit North")
            GameManager.city_manager.set_destination_track(RouteManager.Orientation.North);
        else if (gameObject.name == "Shipyard Track Exit South")
            GameManager.city_manager.set_destination_track(RouteManager.Orientation.South);
        else if (gameObject.name == "Shipyard Track Exit West")
            GameManager.city_manager.set_destination_track(RouteManager.Orientation.West);
        else if (gameObject.name == "Shipyard Track Exit East")
            GameManager.city_manager.set_destination_track(RouteManager.Orientation.East);
    }
}
