using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParkingLotDetector : EventDetector
{
    // Start is called before the first frame update

    public override void OnPointerDown(PointerEventData eventData)
    {
        print("clicked on " + gameObject.name);
        CityManager.Activated_City_Component.expand_building(gameObject.name);
    }
}
