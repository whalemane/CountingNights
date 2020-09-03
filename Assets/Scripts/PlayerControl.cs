using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum Action {Idle, WalkingLeft, RunningLeft, WalkingRight, RunningRight, ReadyLeft, MeleeLeft, ReadyRight, MeleeRight, StruggleLeft, StruggleRight, AimLeft, ShootLeft, AimRight, ShootRight, RepairLeft, RepairRight, Loot};



public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D rgPlayer;
    private SpriteRenderer srPlayer;
    private Animator animator;
    private Camera cam;
    public bool hidden = false;
    private int hideFailCounter = 0;
    public bool resting = false;
    private bool busy = false;
    private bool running = false;
    private bool peeking = false;
    public bool combat = false;
    private bool attacking = false;
    private bool aiming = false;

    private float busyTimer = 0.0f;
    private float busyLimit = 0.0f;

    private float attackChargeTime = 0.0f;
    public float damage = 0.0f;
    private int struggleCounter = 0;

    public int facingDirection = 1;

    public LevelManager level;
    public Room room;
    public bool seen = false;
    public BoxCollider2D meleeCollider;

    public float hp = 100.0f;
    public bool alive = true;
    private float speed = 1.5f;
    public float walkingSpeed = 1.2f;
    public float readySpeed = 1.0f;
    public float runningSpeed = 2.5f;

    public bool canHide = false;
    public Transform currentHideout;
    private float timeHidden = 0.0f;

    public bool canEnterDoor = false;
    public Door currentDoor;
    private bool changingRoom;
    public Image roomTransition;

    public bool canRepair;
    private float repairTimer = 0.6f;
    private int repairCounter = 0;
    private Spawn currentSpawn;

    public bool canExit = false;
    public bool exiting = false;

    public bool canLoot = false;
    private Lootable currentLootable;
    private float lootCounter = 0.0f;

    private bool reloading = false;
    private float reloadTime = 5.0f;
    private float reloadCounter = 0.0f;

    public Slider lootBar;
    public Slider healthBar;
    public Slider breathBar;
    public Image peekImage;
    public Image xbutton;
    public Image squarebutton;

    public Text suppliesText;
    public Text ammoText;
    public Text materialText;
    public int supplies;
    public int ammo;
    public int material;
    private float consumeSupplyCounter = 0.0f;
    public float nightMultiplier = 1.0f;

    //Sound
    public AudioClip gunShot;
    public AudioClip attack;
    public AudioClip breath;
    public AudioClip run;
    public AudioClip[] struggles;
    private bool playRunSound = true;
    public AudioClip hurt;
    private float hurtTimer = 0.0f;
    private bool playHurtSound = true;
    public AudioClip loot;
    private bool playLootSound = true;


    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
        animator = GetComponent<Animator>();
        rgPlayer = GetComponent<Rigidbody2D>();
        srPlayer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        level = GameObject.Find("Level").GetComponent<LevelManager>();
        meleeCollider = GetComponent<BoxCollider2D>();
        meleeCollider.size = new Vector2(0.0f, 0.0f);
        meleeCollider.enabled = true;
        breathBar.gameObject.SetActive(false);
        lootBar.gameObject.SetActive(false);
        //roomTransition.CrossFadeAlpha(0, 0, false);
        healthBar.value = hp;
        suppliesText.text = supplies.ToString();
        ammoText.text = ammo.ToString();
        materialText.text = material.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        xbutton.gameObject.SetActive(false);
        //squarebutton.gameObject.SetActive(false);
        suppliesText.text = supplies.ToString();
        ammoText.text = ammo.ToString();
        materialText.text = material.ToString();
        peeking = false;
        running = false;
        int sound = -1;
        if (!busy && !resting)//Busy animation placeholder
        {

            float moveHorizontal = Input.GetAxisRaw("Horizontal");
            checkCombat();
            if (combat) // Locked in combat
            {
                xbutton.gameObject.SetActive(true);
                peekImage.gameObject.SetActive(false);
                canHide = false;
                
                if(facingDirection == 1)
                {
                    animator.SetInteger("Action", (int)Action.StruggleRight);
                }
                else if(facingDirection == -1)
                {
                    animator.SetInteger("Action", (int)Action.StruggleLeft);
                }
                if (Input.GetButtonDown("X"))
                {

                    struggleCounter++;
                    if (struggleCounter % 2 == 0)
                    {
                        int rnd = Random.Range(0, 10);
                        SoundManager.instance.playerSound.PlayOneShot(struggles[rnd]);
                    }
                    if(struggleCounter > 6)
                    {
                        animator.SetTrigger("Shove");
                        animator.SetInteger("Action", (int)Action.Idle);
                        busyLimit = 0.3f;
                        busy = true;
                        meleeCollider.size = new Vector2(0.7f, 0.3f);
                        struggleCounter = 0;
                    }

                }
            }
            else if (attacking) //Preparing to swing weapon
            {
                if (moveHorizontal > 0)
                {
                    animator.SetBool("Walking", true);
                    facingDirection = 1;
                }
                else if(moveHorizontal < 0)
                {
                    animator.SetBool("Walking", true);
                    facingDirection = -1;
                }
                else
                {
                    animator.SetBool("Walking", false);
                }
                if (Input.GetButton("L2"))
                {
                    if(facingDirection > 0)
                    {
                        animator.SetInteger("Action", (int)Action.ReadyRight);
                    }
                    else
                    {
                        animator.SetInteger("Action", (int)Action.ReadyLeft);
                    }

                    attackChargeTime += Time.deltaTime;
                }
                else
                {
                    animator.SetInteger("Action", (int)Action.Idle);
                    attacking = false;
                }
                if(Input.GetButton("L2") && Input.GetButtonDown("R2"))
                {
                    if (attackChargeTime > 1.0f)
                    {
                        attackChargeTime = 1.0f;
                    }
                    else if(attackChargeTime < 0.25f)
                    {
                        attackChargeTime = 0.0f;
                    }
                    if(facingDirection > 0)
                        animator.SetInteger("Action", (int)Action.MeleeRight);
                    else
                        animator.SetInteger("Action", (int)Action.MeleeLeft);
                    damage = attackChargeTime * 30.0f;
                    attacking = false;
                    busy = true;
                    busyLimit = 0.15f;
                    meleeCollider.size = new Vector2(0.7f, 0.3f);
                }
                Vector2 movement = new Vector2(moveHorizontal * 0.5f, 0.0f);
                speed = readySpeed;
                rgPlayer.MovePosition(rgPlayer.position + movement * Time.deltaTime * speed);
            }
            else if (aiming)
            {
                if (moveHorizontal > 0)
                {
                    animator.SetBool("Walking", true);
                    facingDirection = 1;
                }
                else if (moveHorizontal < 0)
                {
                    animator.SetBool("Walking", true);
                    facingDirection = -1;
                }
                if (Input.GetButton("L1"))
                {
                    if (facingDirection > 0)
                    {
                        animator.SetInteger("Action", (int)Action.AimRight);
                    }
                    else
                    {
                        animator.SetInteger("Action", (int)Action.AimLeft);
                    }
                }
                else
                {
                    animator.SetInteger("Action", (int)Action.Idle);
                    aiming = false;
                }
                if(Input.GetButton("L1") && Input.GetButtonDown("R2"))
                {
                    if(ammo > 0 && (reloadCounter > 1.0f || !reloading))
                    {
                        SoundManager.instance.playPlayerSound(gunShot);
                        if (facingDirection > 0)
                        {
                            animator.SetInteger("Action", (int)Action.ShootRight);
                        }
                        else
                        {
                            animator.SetInteger("Action", (int)Action.ShootLeft);
                        }

                        List<EnemyController> shotEnemies = new List<EnemyController>();
                        foreach (GameObject e in room.enemies)
                        {
                            float distance = e.transform.position.x - transform.position.x;
                            if (distance > 0 && facingDirection > 0 || distance < 0 && facingDirection < 0)
                            {
                                shotEnemies.Add(e.GetComponent<EnemyController>());
                            }
                        }
                        foreach (EnemyController e in shotEnemies)
                        {
                            e.dealDamage(100);
                        }
                        ammo--;
                        reloading = true;
                        reloadCounter = 0.0f;
                    }

                    busy = true;
                    busyLimit = 0.05f;
                    aiming = false;
                }
            }
            else if(hidden)// In hiding spot
            {
                xbutton.gameObject.SetActive(true);
                float timeModifier;
                timeHidden += Time.deltaTime;
                if (timeHidden > 20.0f)
                {
                    timeModifier = 2.0f;
                }
                else
                {
                    timeModifier = (timeHidden / 20.0f);
                }
                bool inTheRoom = false;
                foreach (GameObject e in level.enemies)
                {
                    if (e.GetComponent<EnemyController>().room == room)
                    {
                        inTheRoom = true;
                        Debug.Log("This");
                        break;

                    }
                }

                float breathModifier = (0.5f - 0.5f*(1.0f - (hp / 100.0f))) + timeModifier + (room.getEnemies().Count * 0.1f);
                if (Input.GetButtonDown("X"))
                {
                    if(breathBar.value > 0.25f)
                    {
                        SoundManager.instance.playerSound.PlayOneShot(SoundManager.instance.creak);
                        if (inTheRoom)
                        {
                            hideFailCounter++;
                        }

                        sound = 0;
                    }
                }
                else if (Input.GetButtonUp("X"))
                {
                    if(breathBar.value < 0.75f)
                    {
                        SoundManager.instance.playerSound.PlayOneShot(SoundManager.instance.creak);
                        if (inTheRoom)
                        {
                            hideFailCounter++;
                        }
                        sound = 0;
                    }
                }

                if(breathBar.value == 1.0f || breathBar.value == 0.0f)
                {
                    SoundManager.instance.playerSound.PlayOneShot(SoundManager.instance.creak);
                    hideFailCounter = 3;
                    sound = 0;
                }
                if (Input.GetAxis("X") > 0)
                {
                    breathBar.value += (Time.deltaTime * breathModifier);

                }
                else
                {
                    breathBar.value -= (Time.deltaTime * breathModifier);
                }
                
                if (Input.GetButtonDown("Square") || hideFailCounter > 2)
                {
                    SoundManager.instance.lowVolumeSound.PlayOneShot(SoundManager.instance.creak);
                    SoundManager.instance.loopedSound.Stop();
                    breathBar.value = 0.9f;
                    timeHidden = 0.0f;
                    hidden = false;
                    rgPlayer.simulated = true;
                    srPlayer.enabled = true;
                    busy = true;
                    busyLimit = 0.3f;
                    breathBar.enabled = false;
                    breathBar.gameObject.SetActive(false);
                    peekImage.gameObject.SetActive(false);
                    hideFailCounter = 0;


                }
            }
            else // Free movement
            {
                peekImage.gameObject.SetActive(false);
                Vector2 movement = new Vector2(moveHorizontal, 0);
                speed = walkingSpeed;
                if (Input.GetButton("X")) // Running
                {
                    running = true;
                    speed = runningSpeed;

                }
                if (moveHorizontal != 0 && !peeking)
                {
                    if (moveHorizontal > 0)
                    {
                        facingDirection = 1;
                        if (running)
                        {
                            if (playRunSound)
                            {
                                SoundManager.instance.playLooped(run);
                                playRunSound = false;
                            }
                            sound = 1;
                            animator.SetInteger("Action", (int)Action.RunningRight);
                        }
                        else
                        {
                            animator.SetInteger("Action", (int)Action.WalkingRight);
                        }
                    }
                    else if (moveHorizontal < 0)
                    {
                        facingDirection = -1;
                        if (running)
                        {
                            if (playRunSound)
                            {
                                SoundManager.instance.playLooped(run);
                                playRunSound = false;
                            }
                            sound = 1;
                            animator.SetInteger("Action", (int)Action.RunningLeft);
                        }
                        else
                        {
                            animator.SetInteger("Action", (int)Action.WalkingLeft);
                        }

                    }
                }
                else
                {
                    animator.SetInteger("Action", (int)Action.Idle);
                }
                if (Input.GetButton("L2"))
                {
                    attacking = true;

                }
                if (Input.GetButton("L1"))
                {
                    aiming = true;
                }
                if(Input.GetButtonDown("Square") && canExit)
                {
                    exiting = true;
                    SoundManager.instance.loopedSound.Stop();
                }
                else if (Input.GetButtonDown("Square") && canEnterDoor)
                {

                    rgPlayer.transform.position = currentDoor.getDestinationLocation();
                    room = currentDoor.getDestinationRoom();
                    busy = true;

                    busyLimit = 0.3f;
                    roomTransition.CrossFadeAlpha(1, 0.0f, false);
                    roomTransition.CrossFadeAlpha(0, 0.1f, false);

                    animator.SetInteger("Action", (int)Action.Idle);
                }
                else if (Input.GetButton("Circle") && canRepair && material > 0 && moveHorizontal == 0 && nightMultiplier < 2.0f && currentSpawn.hp < currentSpawn.hpMax)
                {
                    if (facingDirection > 0)
                    {
                        animator.SetInteger("Action", (int)Action.RepairRight);
                    }
                    else
                    {
                        animator.SetInteger("Action", (int)Action.RepairLeft);
                    }
                    repairTimer += Time.deltaTime;
                    if(repairTimer > 0.5f)
                    {
                        repairCounter++;
                        repairTimer = 0.0f;
                        SoundManager.instance.lowVolumeSound.PlayOneShot(attack);
                    } 
                    if (repairCounter > 7)
                    {

                        currentSpawn.repairSpawn();
                        material--;
                        repairCounter = 0;
                        sound = 1;
                    }
                    if (currentSpawn.hp == 100.0f)
                    {
                        canRepair = false;
                    }

                }
                else if (Input.GetButton("Triangle") && canEnterDoor)
                {
                    peeking = true;
                }

                else if (Input.GetButtonDown("Square") && canHide)
                {
                    seen = false;
                    foreach (GameObject e in level.enemies)
                    {
                        if (e.GetComponent<EnemyController>().playerFound)
                        {
                            seen = true;
                            break;
                        }
                    }
                    if (!seen)
                    {
                        SoundManager.instance.playLooped(breath);
                        peekImage.gameObject.SetActive(true);
                        breathBar.gameObject.SetActive(true);
                        animator.SetInteger("Action", (int)Action.Idle);
                        busy = true;
                        hidden = true;
                        busyLimit = 1.0f;
                        rgPlayer.transform.position = new Vector3(currentHideout.position.x, transform.position.y, transform.position.z);
                        rgPlayer.simulated = false;
                        srPlayer.enabled = false;
                    }
                }


                else if(Input.GetButton("Circle") && canLoot && moveHorizontal == 0 && currentLootable.numberOfItems > 0)
                {
                    if (playLootSound)
                    {
                        SoundManager.instance.playLooped(loot);
                        playLootSound = false;
                    }
                    animator.SetInteger("Action", (int)Action.Loot);
                    lootCounter += Time.deltaTime;
                    if(lootCounter > (2.0f * nightMultiplier))
                    {
                        int loot = currentLootable.Loot();
                        switch (loot)
                        {
                            case 1:
                                {
                                    ammo++;
                                    break;
                                }
                            case 2:
                                {
                                    supplies++;
                                    break;
                                }
                            case 3:
                                {
                                    material++;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        lootCounter = 0.0f;
                    }
                    lootBar.value = (float)currentLootable.numberOfItems / (float)currentLootable.maxItems;
                }


                rgPlayer.MovePosition(rgPlayer.position + movement * Time.deltaTime * speed);
            }

        }
        else // Transitioning/Busy
        {
            busyTimer += Time.deltaTime;
            if (busyTimer > busyLimit)
            {
                animator.SetInteger("Action", (int)Action.Idle);
                busyLimit = 0.0f;
                busyTimer = 0.0f;

                busy = false;
                attacking = false;
                meleeCollider.size = new Vector2(0.0f, 0.0f);
                damage = 0.0f;
                attackChargeTime = 0.0f;

            }
        }

        if (resting)
        {
            if(hp < 100.0f && supplies > 0)
            {
                consumeSupplyCounter += Time.deltaTime;
                if(consumeSupplyCounter > 30.0f)
                {
                    supplies--;
                    consumeSupplyCounter = 0.0f;
                }
                dealDamage(-Time.deltaTime / 2.0F);
            }

        }
        //Might be redundant later
        if(facingDirection > 0 && !combat)
        {
            meleeCollider.offset = new Vector2(0.15f, 0.05f);
        }
        else if(facingDirection < 0 && !combat)
        {
            meleeCollider.offset = new Vector2(-0.15f, 0.05f);
        }

        if (!peeking)
        {
            cam.transform.position = new Vector3(rgPlayer.position.x, rgPlayer.position.y, cam.transform.position.z);
        }
        else
        {
            Vector2 peekLocation = currentDoor.getDestinationLocation();
            cam.transform.position = new Vector3(peekLocation.x, peekLocation.y, cam.transform.position.z);
            peekImage.gameObject.SetActive(true);
        }
        if (!running && !hidden && !Input.GetButton("Circle"))
        {
            SoundManager.instance.loopedSound.Stop();
            playRunSound = true;
        }
        if (Input.GetButtonUp("Circle"))
        {
            SoundManager.instance.loopedSound.Stop();
            playLootSound = true;
        }

        if (!playHurtSound)
        {
            hurtTimer += Time.deltaTime;
            if(hurtTimer > 1.5f)
            {
                playHurtSound = true;
            }
        }

        
        if (reloading)
        {
            sound = 5;
            reloadCounter += Time.deltaTime;
            if(reloadCounter > reloadTime)
            {
                reloading = false;
                reloadCounter = 0.0f;
            }
        }
        level.emitSound(sound, room, room, rgPlayer.transform.position);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        checkCombat();
        if(collision.tag == "Hideout" && !busy)
        {
            squarebutton.gameObject.SetActive(true);
            canHide = true;
            currentHideout = collision.transform;
            if (currentHideout.GetComponent<Lootable>() != null)
            {
                canLoot = true;
                currentLootable = currentHideout.GetComponent<Lootable>();
                lootBar.gameObject.SetActive(true);
                lootBar.value = (float)currentLootable.numberOfItems / (float)currentLootable.maxItems;
            }
        }
        if(collision.tag == "Lootable" && !busy)
        {
            canLoot = true;
            currentLootable = collision.GetComponent<Lootable>();
            lootBar.gameObject.SetActive(true);
            lootBar.value = (float)currentLootable.numberOfItems / (float)currentLootable.maxItems;
        }
        if (collision.tag == "Door" && !busy)
        {
            canEnterDoor = true;
            currentDoor = collision.GetComponent<Door>();
        }
        if(collision.tag == "Spawn" && !busy)
        {
            canRepair = true;
            currentSpawn = collision.GetComponent<Spawn>();
        }
        if(collision.tag == "Exit" && !busy)
        {
            canExit = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Hideout")
        {
            squarebutton.gameObject.SetActive(false);
            canHide = false;
            if(collision.GetComponent<Lootable>() != null)
            {
                canLoot = false;
                lootBar.gameObject.SetActive(false);
            }
        }
        if(collision.tag == "Lootable")
        {
            canLoot = false;
            lootBar.gameObject.SetActive(false);
        }
        if(collision.tag == "Door")
        {
            canEnterDoor = false;
        }
        if(collision.tag == "Spawn")
        {
            canRepair = false;
            repairCounter = 0;
            repairTimer = 0.6f;
        }
        if (collision.tag == "Exit" && !busy)
        {
            canExit = false;
        }
    }

    private void checkCombat()
    {
        combat = false;
        foreach (GameObject e in level.enemies)
        {
            if (e.GetComponent<EnemyController>().combat)
            {
                combat = true;
                attacking = false;
                aiming = false;
                meleeCollider.offset = new Vector2(0.0f, 0.05f);
                if (e.transform.position.x - transform.position.x > 0)
                {
                    facingDirection = 1;
                }
                else
                {
                    facingDirection = -1;
                }
                break;
            }
        }

    }

    public void dealDamage(float dmg)
    {
        hp -= dmg;
        if(hp > 100.0f)
        {
            hp = 100.0f;
        }
        healthBar.value = hp;

        if(hp < 0.0f)
        {
            alive = false;
        }
        if(dmg > 0)
        {
            if (playHurtSound)
            {
                SoundManager.instance.playerSound.PlayOneShot(hurt);
                playHurtSound = false;
                hurtTimer = 0.0f;
            }
        }
    }

}