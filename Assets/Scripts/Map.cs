using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform t in transform)
        {
            float threshold = 11.0f;
            int order = 0;
            while(threshold > -10.0f)
            {
                if (t.localPosition.y > threshold)
                {
                    t.GetComponent<SpriteRenderer>().sortingOrder = order;
                    threshold = -11.0f;
                }
                order++;
                threshold -= 0.01f;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
