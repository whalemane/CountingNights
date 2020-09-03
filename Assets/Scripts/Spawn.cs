using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private float spawnCoolDownTimer = 0.0f;
    private float spawnCoolDownLimit = 10.0f;
    private float spawnTimer = 0.0f;
    private bool spawnCoolDown = false;
    public float hp = 100.0f;
    public float hpMax = 100.0f;
    public GameObject enemy;
    public SpriteRenderer srWindow;
    public Sprite[] sprites;

    public bool spawnNextUpdate = false;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GameObject.Find("Enemy");

    }

    // Update is called once per frame
    void Update()
    {
        if (spawnCoolDown)
        {
            spawnNextUpdate = false;
            spawnCoolDownTimer += Time.deltaTime;
            if(spawnCoolDownTimer > spawnCoolDownLimit)
            {
                spawnCoolDown = false;
            }
        }
        srWindow = GetComponent<SpriteRenderer>();
        if (hp / hpMax > 0.75f)
            srWindow.sprite = sprites[4];
        else if (hp / hpMax > 0.5f)
        {
            srWindow.sprite = sprites[3];
        }
        else if (hp / hpMax > 0.25f)
        {
            srWindow.sprite = sprites[2];
        }
        else if (hp / hpMax > 0.0f)
        {
            srWindow.sprite = sprites[1];
        }
        else
        {
            srWindow.sprite = sprites[0];
        }
    }

    public GameObject spawnEnemy()
    {
        if (spawnNextUpdate && !spawnCoolDown)
        {
            spawnTimer += Time.deltaTime;
            if(spawnTimer > 3.0f) {
                spawnTimer = 0.0f;
                spawnCoolDown = true;
                spawnCoolDownTimer = 0.0f;
                spawnNextUpdate = false;
                if (hp > 0)
                {
                    hp -= 25.0f;
                    if (hp <= 0)
                    {
                        SoundManager.instance.playLowSound(SoundManager.instance.crashing);
                        hp = 0.0f;
                        //SoundManager.instance.playLowSound(SoundManager.instance.creak);
                        GameObject spawnedEnemy = Instantiate(enemy, transform);
                        transform.parent.GetComponent<Room>().enterRoom(spawnedEnemy);
                        spawnedEnemy.transform.position = transform.GetChild(0).transform.position;
                        spawnedEnemy.GetComponent<EnemyController>().room = transform.parent.GetComponent<Room>();
                        spawnedEnemy.transform.localScale = new Vector2(0.7f, 0.6f);
                        spawnedEnemy.SetActive(true);

                        return spawnedEnemy;
                    }

                }
                else
                {
                    SoundManager.instance.playLowSound(SoundManager.instance.creak);
                    GameObject spawnedEnemy = Instantiate(enemy, transform);
                    transform.parent.GetComponent<Room>().enterRoom(spawnedEnemy);
                    spawnedEnemy.transform.position = transform.GetChild(0).transform.position;
                    spawnedEnemy.GetComponent<EnemyController>().room = transform.parent.GetComponent<Room>();
                    spawnedEnemy.transform.localScale = new Vector2(0.7f, 0.6f);
                    spawnedEnemy.SetActive(true);

                    return spawnedEnemy;
                }
            }

        }
        return null;
    }

    public void repairSpawn()
    {
        if(hp < hpMax)
        {
            hp += 25.0f;
            if (hp / hpMax > 0.75f)
                srWindow.sprite = sprites[4];
            else if (hp / hpMax > 0.5f)
            {
                srWindow.sprite = sprites[3];
            }
            else if (hp / hpMax > 0.25f)
            {
                srWindow.sprite = sprites[2];
            }
            else if (hp / hpMax > 0.0f)
            {
                srWindow.sprite = sprites[1];
            }
            else
            {
                srWindow.sprite = sprites[0];
            }
        }
        else
        {
            hp = hpMax;
        }

    }
}
