using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public House south;
    public House north;
    public House west;
    public House east;

    public string level;

    private SpriteRenderer sr;
    private Color startColor;
    private bool active = false;
    private float activeTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            sr.color = startColor;
            activeTimer += Time.deltaTime;
            if(activeTimer > 0.5f && activeTimer < 0.75f)
            {
                sr.color = Color.black;

            }
            else if( activeTimer >= 0.75f)
            {
                sr.color = startColor;
                activeTimer = 0.0f;
            }
        }
    }

    public House MoveToNewHouse(int dir)
    {
        House returnHouse = null;
        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");
        if(dir == 0)
        {
            returnHouse = east;
        }
        else if(dir == 2)
        {
            returnHouse = west;
        }
        else if(dir == 3)
        {
            returnHouse = north;
        }
        else if(dir == 1)
        {
            returnHouse = south;
        }
        return returnHouse;
            
    }

    public void Activate(bool newActive)
    {
        active = newActive;
        if (!active)
        {
            sr.color = startColor;
        }
    }
}
