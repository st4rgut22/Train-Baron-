using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClick : EventDetector
{
    public TutorialManager tutorial_manager;

    // Start is called before the first frame update
    void Start()
    {
        tutorial_manager = transform.parent.GetComponent<TutorialManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        print("activate next tutorial");
        StartCoroutine(tutorial_manager.activate_next_tutorial_step());
    }
}
