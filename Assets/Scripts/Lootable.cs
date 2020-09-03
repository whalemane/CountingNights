using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lootable : MonoBehaviour
{

    public int numberOfItems = 15;
    public int maxItems;

    public Slider lootBar;


    // Start is called before the first frame update
    void Start()
    {
        maxItems = numberOfItems;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Loot()
    {
        int loot = 0;

        if(numberOfItems > 0)
        {
            float rnd = Random.Range(0, 200);
            if(rnd < 5)
            {
                loot = 1;
            }
            else if(rnd < 30)
            {
                loot = 2;
            }
            else if(rnd < 70)
            {
                loot = 3;
            }
            numberOfItems--;
        }

        return loot;
    }
}
