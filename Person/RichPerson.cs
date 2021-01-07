using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichPerson : Person
{
    void Awake()
    {
        base.Awake();
        activity_likelihood_map = new Dictionary<string, int>()
        {
            { "work_thought_bubble", 5 },
            {"home_thought_bubble", 40 },
            {"restaurant_thought_bubble", 30 },
            {"vacation_thought_bubble", 25 }
        };
        activity_duration_map = new Dictionary<string, int>(){
            { "home_thought_bubble", 50},
            {"restaurant_thought_bubble", 30 },
            {"vacation_thought_bubble", 100 },
            {"work_thought_bubble", 15 }
        }; // the time it takes to complete an action (if destination4 matches thought bubble)
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rent = 50;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
