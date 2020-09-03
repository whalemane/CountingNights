using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Door> doors;
    //public List<Spawn> spawns;
    public List<GameObject> enemies;
    //public List<Lootable> lootables;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.tag == "Door")
            {
                doors.Add(t.gameObject.GetComponent<Door>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<GameObject> getEnemies()
    {
        return enemies;
    }

    public void enterRoom(GameObject other)
    {
        enemies.Add(other);
    }
    public void leaveRoom(GameObject other)
    {
        enemies.Remove(other);
    }

}
