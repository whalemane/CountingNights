using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rgEnemy;
    private float activeTime = 0.0f;
    private float actionLength;
    public int currentAction;
    public int illegalAction = -1;
    private int currSound = -1;
    private float speed = 0.9f;
    private float targetLocation;
    public bool hunting = false;
    private bool investigating = false;
    private float direction;
    private float walkingDirection;
    public float huntingTimer = 0.0f;
    private bool firstTime = true;
    public float detectionTimer = 0.0f;
    private float detectionThreshold = 1.0f;
    public float detectionRange = 1.5f;
    public bool playerFound = false;
    public bool combat = false;
    public float combatTimer = 0.0f;
    public float combatLimit = 1.0f;
    private bool stunned = false;
    private float stunTimer = 0.0f;
    private float stunLimit = 0.0f;

    private float doorTimer = 0.0f;
    private float doorLimit = 0.5f;

    private float changingRoomTimer = 0.0f;
    private float changingRoomLimit = 1.0f;

    public PlayerControl player;
    private float facingDirection = -1.0f;

    private Animator animatorMonster;
    public Door door;
    public float hp;
    public float damage;
    private bool dead = false;
    private float deathTimer = 0.0f;

    public Room room;
    public bool changingRoom = false;

    private LevelManager level;

    private bool playSound = true;
    private bool playRunSound = true;
    private float runTimer = 0.0f;
    public AudioClip notice;
    public AudioClip hurt;
    public AudioClip run;
    private bool playGroanSound = true;
    private float groanTimer = 0.0f;
    public AudioClip[] groan;

    // Start is called before the first frame update
    void Start()
    {
        hp = 100.0f;
        animatorMonster = GetComponent<Animator>();
        rgEnemy = GetComponent<Rigidbody2D>();
        //player = GameObject.Find("Player").GetComponent<PlayerControl>();
        Physics2D.IgnoreLayerCollision(8, 8);
        level = GameObject.Find("Level").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movement = new Vector2(0.0f, 0.0f);
        if (!dead)
        {
            if (!combat && !stunned)
            {
                if (hunting && !stunned)
                {
                    detectionRange = 5.0f;
                    huntingTimer += Time.deltaTime;
                    if (huntingTimer > 6.0f)
                    {
                        detectionThreshold = 1.0f;
                        detectionRange = 1.5f;
                        huntingTimer = 0.0f;
                        hunting = false;
                        playSound = false;
                    }
                }
                //Player is in the same room as the enemy
                if (room == player.room)
                {
                    targetLocation = player.transform.position.x;
                    direction = targetLocation - transform.position.x;
                    if (!hunting) //Unaware, but checking if the enemy sees the player
                    {

                        if (facingDirection < 0 && direction < 0 && !player.hidden && Mathf.Abs(direction) < detectionRange)
                        {
                            detectionTimer += Time.deltaTime;
                            if (detectionTimer > detectionThreshold && !playerFound)
                            {
                                detectionTimer = 0.0f;
                                walkingDirection = -1.0f;
                                playerFound = true;
                            }

                        }
                        else if (facingDirection > 0 && direction > 0 && !player.hidden && Mathf.Abs(direction) < detectionRange)
                        {
                            detectionTimer += Time.deltaTime;
                            if (detectionTimer > detectionThreshold && !playerFound)
                            {
                                detectionTimer = 0.0f;
                                walkingDirection = 1.0f;
                                playerFound = true;
 
                            }
                        }
                        else
                        {
                            detectionTimer = 0.0f;
                            playerFound = false;
                        }
                    }
                    else //Hunting player, will detect even if not facing
                    {

                        if (!player.hidden && Mathf.Abs(direction) < detectionRange)
                        {
                            playerFound = true;
                            if (direction > 0)
                            {
                                walkingDirection = 1.0f;
                                facingDirection = 1.0f;
                            }
                            else
                            {
                                walkingDirection = -1.0f;
                                facingDirection = -1.0f;
                            }

                        }
                        else
                        {
                            detectionTimer = 0.0f;
                            playerFound = false;
                        }
                    }
                    if (playerFound)
                    {
                        if (playSound)
                        {
                            SoundManager.instance.monsterSound.PlayOneShot(notice);
                            playSound = false;
                        }
                        changingRoom = false;
                        detectionThreshold = 0.0f;
                        hunting = true;
                        huntingTimer = 0.0f;
                        speed = 1.5f;
                        movement = new Vector2(walkingDirection, 0.0f);
                    }
                    //Walking towards a sound
                    if (hunting && !playerFound)
                    {
                        movement = new Vector2(walkingDirection, 0.0f);
                    }
                }
                //Player ran in to a new room, enemy should follow
                if (hunting && room != player.room)
                {
                    combat = false;
                    playerFound = false;
                    foreach (Transform t in room.transform)
                    {
                        if (t.tag == "Door" && t.gameObject.GetComponent<Door>().getDestinationRoom() == player.room)
                        {
                            door = t.gameObject.GetComponent<Door>();
                            changingRoom = true;
                            break;
                        }
                    }

                }
                //Going to target door and switching rooms
                if (changingRoom)
                {

                    targetLocation = door.getDoorLocation();
                    direction = targetLocation - transform.position.x;
                    if (Mathf.Abs(direction) > 0.1f)
                    {
                        if (direction > 0.0f)
                        {
                            walkingDirection = 1.0f;
                            facingDirection = walkingDirection;
                        }
                        else if (direction < 0.0f)
                        {
                            walkingDirection = -1.0f;
                            facingDirection = walkingDirection;
                        }
                    }
                    else
                    {

                        walkingDirection = 0.01f;
                    }

                    movement = new Vector2(walkingDirection, 0.0f);

                }
                //Enemy is unaware of player and is not currently changing room
                if (hunting == false && !changingRoom)
                {
                    investigating = false;
                    playSound = true;
                    currSound = -1;
                    playerFound = false;
                    speed = 0.9f;


                    if (activeTime == 0.0f)
                    {

                        actionLength = Random.Range(1.0f, 5.0f);
                        currentAction = Random.Range(0, 5);
                        while (currentAction == illegalAction)
                        {
                            currentAction = Random.Range(0, 5);
                        }
                        if (currentAction == 1 || currentAction == 2 || changingRoom == true)
                        {
                            illegalAction = -1;
                        }

                        firstTime = true;
                    }
                    changingRoomTimer += Time.deltaTime;
                    movement = new Vector2(0.0f, 0.0f);
                    if (activeTime < actionLength && currentAction < 4)
                    {
                        switch (currentAction)
                        {
                            case 0:
                                movement = new Vector2(0.0f, 0.0f);
                                break;
                            case 1:
                                movement = new Vector2(1.0f, 0.0f);
                                facingDirection = 1.0f;
                                break;
                            case 2:
                                movement = new Vector2(-1.0f, 0.0f);
                                facingDirection = -1.0f;
                                break;
                            case 3:
                                if (firstTime)
                                {
                                    animatorMonster.SetTrigger("face");
                                    facingDirection = -facingDirection;
                                    firstTime = false;
                                }

                                movement = new Vector2(0.0f, 0.0f);
                                break;
                            default:
                                break;
                        }
                        activeTime += Time.deltaTime;

                    }
                    else
                    {
                        if (changingRoomTimer > changingRoomLimit)
                        {
                            int i = Random.Range(0, room.doors.Count);
                            door = room.doors[i];
                            changingRoom = true;
                            changingRoomTimer = 0.0f;
                        }
                    }

                    if (activeTime > actionLength)
                        activeTime = 0.0f;

                }

                //Animation
                if (Mathf.Abs(movement.x) > 0.02f)
                {
                    if (movement.x > 0)
                    {
                        if (hunting && !investigating)
                        {
                            animatorMonster.SetInteger("Action", (int)Action.RunningRight);
                        }
                        else if(investigating)
                        {
                            animatorMonster.SetInteger("Action", (int)Action.WalkingRight);
                        }
                        else
                        {
                            animatorMonster.SetInteger("Action", (int)Action.WalkingRight);
                        }
                    }
                    else
                    {
                        if (hunting && !investigating)
                        {
                            animatorMonster.SetInteger("Action", (int)Action.RunningLeft);
                        }
                        else if (investigating)
                        {
                            animatorMonster.SetInteger("Action", (int)Action.WalkingLeft);
                        }
                        else
                        {
                            animatorMonster.SetInteger("Action", (int)Action.WalkingLeft);
                        }
                    }
                }
                else
                {
                    animatorMonster.SetInteger("Action", (int)Action.Idle);
                }

            }
            if (stunned)
            {

                animatorMonster.SetInteger("Action", (int)Action.Idle);
                animatorMonster.SetBool("Stunned", true);
                combat = false;
                combatTimer = 0.0f;
                stunTimer += Time.deltaTime;
                rgEnemy.simulated = false;
                if (stunTimer > stunLimit - 0.2f)
                {
                    animatorMonster.SetBool("Rise", true);
                    animatorMonster.SetBool("Stunned", false);
                }
                if (stunTimer > stunLimit)
                {
                    animatorMonster.SetBool("Rise", false);
                    rgEnemy.simulated = true;
                    stunned = false;
                    stunTimer = 0.0f;
                }
            }

            if (combat)
            {
                if (facingDirection < 0)
                {
                    animatorMonster.SetInteger("Action", (int)Action.MeleeLeft);
                }
                else
                {
                    animatorMonster.SetInteger("Action", (int)Action.MeleeRight);
                }
                combatTimer += Time.deltaTime;
                if(combatTimer > combatLimit)
                {
                    combatTimer = 0.0f;
                    player.dealDamage(damage);
                }
            }

            rgEnemy.MovePosition(rgEnemy.position + (movement * Time.deltaTime * speed));
            if ((!hunting && room == player.room) || investigating)
            {
                if (playGroanSound)
                {
                    int rnd = Random.Range(0, 3);
                    SoundManager.instance.playMonsterSound(groan[rnd]);
                    playGroanSound = false;
                    groanTimer = 0.0f;
                }
                else
                {
                    groanTimer += Time.deltaTime;
                    if (groanTimer > 5.0f)
                    {
                        playGroanSound = true;
                    }
                }
            }
            if (hunting && Mathf.Abs(movement.x) > 0)
            {
                if (playRunSound)
                {
                    SoundManager.instance.lowVolumeSound.PlayOneShot(run);
                    playRunSound = false;
                    runTimer = 0.0f;
                }
                else
                {
                    runTimer += Time.deltaTime;
                    if (runTimer > 1.5f)
                    {
                        playRunSound = true;
                    }
                }
            }
        }
        else
        {
            deathTimer += Time.deltaTime;
            if(deathTimer > 15.0f)
            {
                Destroy(gameObject);
            }
        }




        

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {

            if (!hunting && !stunned && !player.combat)
            {
                SoundManager.instance.playPlayerSound(player.attack);
                this.dealDamage(player.damage * 4.5f);
                stunned = true;
                stunLimit = 0.5f;
                level.emitSound(0, room, room, player.transform.position);
                
            }
            else if (combat)
            {
                level.emitSound(2, room, room, player.transform.position);
                dealDamage(0.0f);
                stunned = true;
                animatorMonster.SetFloat("FacingDirection", facingDirection);
                float rndStunLimit = Random.Range(1.5f, 2.5f);
                stunLimit = rndStunLimit;
            }
            else if(!dead)
            {
                SoundManager.instance.playPlayerSound(player.attack);
                this.dealDamage(player.damage);
                stunned = true;
                animatorMonster.SetFloat("FacingDirection", facingDirection);
                stunLimit = Random.Range(0.5f, 1.0f);
                level.emitSound(2, room, room, player.transform.position);
            }
        }


    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.gameObject.GetComponent<Door>() == door && changingRoom && !combat)
        {
            animatorMonster.SetInteger("Action", (int)Action.Idle);
            doorTimer += Time.deltaTime;
            if (doorTimer > doorLimit)
            {
                Door door = collision.GetComponent<Door>();


                room.leaveRoom(transform.gameObject);
                rgEnemy.position = door.getDestinationLocation();

                room = door.getDestinationRoom();
                room.enterRoom(transform.gameObject);

                changingRoom = false;
                doorTimer = 0.0f;
            }
        }


    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            if (!playerFound && !stunned)
            {
                hunting = true;
                stunned = true;
                stunTimer = 0.5f;
            }
            else
            {
                if (player.room == room && !stunned)
                {
                    combat = true;
                }

            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            if (!playerFound && !stunned)
            {
                hunting = true;
                stunned = true;
                stunTimer = 0.5f;
            }
            else
            {
                if(player.room == room && !stunned)
                {
                    combat = true;
                }

            }

            
        }
        if (collision.gameObject.tag == "Room" && currentAction < 4)
        {
            float posInRoom = room.transform.position.x - transform.position.x;
            if(posInRoom > 0)
            {
                illegalAction = 2;
            }
            else
            {
                illegalAction = 1;
            }
            if (hunting && huntingTimer > 3.0f)
            {
                Debug.Log("This?");
                huntingTimer = 6.0f;
            }

            activeTime = 0.0f;
        }
    }


    public void goToSound(Room destination, Vector3 playerPos, int soundLevel)
    {
        if(currSound < soundLevel)
        {
            currSound = soundLevel;
            if (room == destination)
            {
                //SoundManager.instance.loopedSound.Stop();
                //playRunSound = true;
                direction = playerPos.x - transform.position.x;
                if (direction < 0)
                {
                    facingDirection = -1.0f;
                    walkingDirection = facingDirection;
                    playerFound = true;
                    hunting = true;
                    speed = 1.5f;
                }
                else
                {
                    facingDirection = 1.0f;
                    walkingDirection = facingDirection;
                    playerFound = true;
                    hunting = true;
                    speed = 1.5f;
                }
                if (player.hidden)
                {
                    speed = 0.9f;
                    investigating = true;
                }
                else
                {
                    investigating = false;
                }
            }
            else
            {
                if(soundLevel > 0)
                {
                    //if (playRunSound)
                    //{
                        //SoundManager.instance.lowVolumeSound.PlayOneShot(hurt);
                        //playRunSound = false;
                    //}
                    hunting = true;
                    speed = 1.5f;
                }
                changingRoom = true;
                foreach (Transform t in room.transform)
                {
                    if (t.tag == "Door")
                    {
                        if (t.gameObject.GetComponent<Door>().getDestinationRoom() == destination)
                        {
                            door = t.gameObject.GetComponent<Door>();
                        }
                    }
                }
            }
        }

    }

    public void dealDamage(float dmg)
    {

        hp -= dmg;
        if (hp <= 0.0f)
        {
            dead = true;
            animatorMonster.SetFloat("facingDirection", facingDirection);
            animatorMonster.SetBool("Stunned", false);
            animatorMonster.SetTrigger("Dead");
            rgEnemy.simulated = false;
            level.enemies.Remove(gameObject);
            room.leaveRoom(gameObject);
            SoundManager.instance.playMonsterSound(hurt);
        }
        else
        {
            SoundManager.instance.playMonsterSound(hurt);
        }
    }
}

