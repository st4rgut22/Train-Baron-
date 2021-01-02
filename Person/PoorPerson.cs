using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoorPerson : Person
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rent = 10;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
