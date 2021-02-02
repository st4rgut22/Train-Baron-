using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoorPerson : Person
{
    void Awake()
    {
        base.Awake();
        ticket_cost_map = new Dictionary<string, int>()
        {
            { "work_thought_bubble", 75 },
            {"home_thought_bubble", 50 },
            {"restaurant_thought_bubble", 100 },
            {"vacation_thought_bubble", 300 }
        };

        activity_likelihood_map = new Dictionary<string, int>()
        {
            { "work_thought_bubble", 50 },
            {"home_thought_bubble", 30 },
            {"restaurant_thought_bubble", 15 },
            {"vacation_thought_bubble", 5 }
        };

        activity_duration_map = new Dictionary<string, int>(){
            { "home_thought_bubble", 5}, 
            {"restaurant_thought_bubble", 10 },
            {"vacation_thought_bubble", 60 },
            {"work_thought_bubble", 30 }
        }; // the time it takes to complete an action (if destinatikon4 matches thought bubble)
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
