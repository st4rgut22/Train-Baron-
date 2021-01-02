using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichPerson : Person
{
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
