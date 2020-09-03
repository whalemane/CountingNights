using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    public Door destination;
    
    private Transform location;
    
    // Start is called before the first frame update
    void Start()
    {
        location = destination.transform;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Vector2 getDestinationLocation()
    {
        return destination.transform.GetChild(0).transform.position;
    }

    public Room getDestinationRoom()
    {
        return destination.transform.parent.gameObject.GetComponent<Room>();
    }

    public float getDoorLocation()
    {
        Vector2 vec = transform.position;
        vec += GetComponent<BoxCollider2D>().offset;
        return vec.x;
    }
}
