using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public List<Room> rooms;
    public List<GameObject> enemies;
    public List<Spawn> spawns;
    public List<Lootable> lootables;
    public GameObject enemy;
    public bool spawn = true;
    private float rndSpawnTimer = 0.0f;
    private float rndSpawnLimit = 10.0f;
    public int maxEnemies = 3;
    public int spawnChanceMultiplier = 5;
    public int nights;


    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform t in transform)
        {

            if(t.tag == "Room")
            {
                rooms.Add(t.gameObject.GetComponent<Room>());
                foreach(Transform t2 in t)
                {
                    if (t2.tag == "Spawn")
                    {

                        spawns.Add(t2.gameObject.GetComponent<Spawn>());

                    }
                    else if (t2.tag == "Hideout")
                    {
                        if (t2.gameObject.GetComponent<Lootable>() != null)
                        {

                            lootables.Add(t2.gameObject.GetComponent<Lootable>());
                        }
                    }
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (spawn)
        {
            spawnChanceMultiplier = 5 + (nights * 5);
            if(spawnChanceMultiplier > 35)
            {
                spawnChanceMultiplier = 35;
            }
                rndSpawnTimer += Time.deltaTime;
                if (rndSpawnTimer > rndSpawnLimit)
                {
                    rndSpawnTimer = 0.0f;
                    if (Random.Range(0, 100) > (100 - ((maxEnemies - enemies.Count) * (spawnChanceMultiplier))))
                    {

                        int spawn = Random.Range(0, spawns.Count);
                        spawns[spawn].spawnNextUpdate = true;
                    }

                }

                spawnEnemies();
        } 
    }

    public void spawnEnemies()
    {
        foreach (Spawn s in spawns)
        {
            GameObject spawnedEnemy = s.spawnEnemy();
            if (spawnedEnemy != null)
            {
                enemies.Add(spawnedEnemy);
            }
        }
    }

    public void SetSpawning(bool spawning)
    {
        spawn = spawning;
    }

    public void emitSound(int soundLevel, Room startingRoom, Room currentRoom, Vector3 pos)
    {
        if (soundLevel > -1)
        {
            List<GameObject> currEnemies = currentRoom.getEnemies();
            foreach (GameObject e in currEnemies)
            {
                e.GetComponent<EnemyController>().goToSound(startingRoom, pos, soundLevel);
            }
            foreach (Transform t in currentRoom.transform)
            {
                if (t.tag == "Door")
                {
                    Room nextRoom;
                    nextRoom = t.gameObject.GetComponent<Door>().getDestinationRoom();
                    if(nextRoom != startingRoom)
                    {
                        emitSound(soundLevel - 1, currentRoom, nextRoom, pos);
                    }
                }
                if(t.tag == "Spawn")
                {
                   if(Random.Range(0, 100) > (100 - ((soundLevel + 1) * (maxEnemies - (enemies.Count)))) && spawn)
                    {
                        t.gameObject.GetComponent<Spawn>().spawnNextUpdate = true;
                    }
                }
            }
        }
    }

    public void loadHouse(HouseStats house)
    {
        for(int i = 0; i < lootables.Count; i++)
        {
            lootables[i].numberOfItems = house.loot[i];
        }
        for (int j = 0; j < spawns.Count; j++)
        {
            spawns[j].hp = house.windows[j];
        }
    }

    public HouseStats GetHouse()
    {
        HouseStats house = new HouseStats();
        house.name = "";
        house.loot = new List<int>();
        house.windows = new List<float>();
        foreach(Lootable l in lootables)
        {

            house.loot.Add(l.numberOfItems);
        }
        foreach(Spawn s in spawns)
        {
            house.windows.Add(s.hp);
        }

        return house;
    }
    
    public void passEnemy(GameObject enemy)
    {
        foreach(Spawn s in spawns)
        {
            s.enemy = enemy;
        }
    }
}