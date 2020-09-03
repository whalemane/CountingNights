using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public struct HouseStats
{
    public string name;
    public List<int> loot;
    public List<float> windows;
};

public class GameManager : MonoBehaviour
{
    public Canvas HUD;
    public List<HouseStats> houseStats;
    public LevelManager currentLevel;
    private PlayerControl player;
    private bool resting = false;
    public Image restingScreen;
    public Image loadingScreen;
    public Image nightImage;
    public Image pauseScreen;
    public Text inGameTimeDisplay;
    public House currentHouse;
    public GameObject enemy;
    
    public int hours = 12;
    private float minutes = 0.0f;
    private int nights = 0;
    public bool close = false;
    private float closeTimer = 0.0f;

    private string hourString;
    private string minutesString;

    bool switchLevel = false;
    bool inHouse = false;
    bool inMap = false;
    bool inMenu = true;
    bool loading = false;
    bool firstTime = true;
    private bool pause = false;
    private bool inTutorial = false;
    private bool hungry = true;
    private bool changingHouse = false;
    private float changingHouseTimer = 0.0f;

    private float loadTime = 0.0f;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        houseStats = new List<HouseStats>();
        DontDestroyOnLoad(this.gameObject);
        currentLevel = GameObject.Find("Level").GetComponent<LevelManager>();
        player = GameObject.Find("Player").GetComponent<PlayerControl>();
        player.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!pause)
        {
            if (Input.GetAxis("Cancel") > 0)
            {
                Debug.Break();
            }

            if (loading)
            {
                loadTime += Time.deltaTime;
                if (loadTime > 0.5f)
                {
                    loadingScreen.CrossFadeAlpha(0, 1.0f, true);
                    loading = false;
                    loadTime = 0.0f;
                }
            }
            minutes += Time.deltaTime * 2;
            if (minutes > 60.0f)
            {
                hours += 1;
                minutes = 0.0f;
                if (hours > 23)
                {
                    hours = 0;
                }

            }
            if (hours < 10)
            {
                hourString = "0" + hours.ToString();
            }
            else
            {
                hourString = hours.ToString();
            }

            if (minutes < 10)
            {
                minutesString = "0" + ((int)minutes).ToString();
            }
            else
            {
                minutesString = ((int)minutes).ToString();
            }
            inGameTimeDisplay.text = hourString + ":" + minutesString;
            if (hours >= 17 && hours < 18)
            {
                close = true;
                nightImage.CrossFadeAlpha(0.7f, 30.0f, true);
            }
            if (hours >= 18)
            {
                SoundManager.instance.playNightMusic();
                close = false;
                nightImage.CrossFadeAlpha(0.7f, 5.0f, true);
                inGameTimeDisplay.color = Color.red;
            }
            if (hours >= 5 && hours < 6)
            {
                close = true;
                nightImage.CrossFadeAlpha(0.0f, 60.0f, true);
            }
            if (hours == 6 && close)
            {
                SoundManager.instance.playDayMusic();
                nights++;
                close = false;
                inGameTimeDisplay.color = Color.green;
            }
            if (close)
            {
                closeTimer += Time.deltaTime;
                if (closeTimer > 0.5f && closeTimer < 1.0f)
                {
                    inGameTimeDisplay.color = Color.red;
                }
                else if (closeTimer > 1.0f)
                {
                    inGameTimeDisplay.color = Color.green;
                    closeTimer = 0.0f;
                }
            }
            if (hours % 6 == 0 && hungry)
            {
                hungry = false;
                player.supplies--;
                if (player.supplies < 0)
                {
                    player.supplies = 0;
                    player.dealDamage(10.0f);
                }
            }
            if (!hungry && hours % 6 != 0)
            {
                hungry = true;
            }

            if (inHouse)
            {
                if (Input.GetButtonDown("Submit"))
                {
                    pause = true;
                }
                if (switchLevel)
                {
                    cam.orthographicSize = 2;
                    currentLevel = GameObject.Find("Level").GetComponent<LevelManager>();
                    player.gameObject.SetActive(true);
                    player.level = currentLevel;
                    currentLevel.passEnemy(enemy);
                    for (int i = 0; i < houseStats.Count; i++)
                    {

                        if (houseStats[i].name == SceneManager.GetActiveScene().name)
                        {
                            currentLevel.loadHouse(houseStats[i]);
                            i = houseStats.Count;
                        }
                    }
                    GameObject startLocation = GameObject.Find("Exit");
                    player.room = startLocation.GetComponentInParent<Room>();
                    player.transform.position = startLocation.GetComponent<BoxCollider2D>().transform.position;
                    currentLevel.nights = nights;
                    switchLevel = false;
                }
                if (player.alive && !player.exiting)
                {

                    if (hours >= 6 && hours < 18)
                    {
                        player.nightMultiplier = 1.0f;
                        currentLevel.SetSpawning(false);

                    }
                    else
                    {
                        player.nightMultiplier = 2.0f;
                        currentLevel.SetSpawning(true);
                    }

                    if (Input.GetAxis("R1") > 0 && currentLevel.enemies.Count == 0)
                    {
                        resting = true;
                    }
                    else
                    {
                        resting = false;
                    }
                    if (resting)
                    {
                        inGameTimeDisplay.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        inGameTimeDisplay.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        inGameTimeDisplay.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        inGameTimeDisplay.rectTransform.localScale = new Vector3(3, 3, 1);
                        restingScreen.CrossFadeAlpha(1, 0.5f, false);
                        Time.timeScale = 20.0f;

                        player.resting = true;
                    }
                    else
                    {
                        inGameTimeDisplay.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                        inGameTimeDisplay.rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                        inGameTimeDisplay.rectTransform.pivot = new Vector2(1.0f, 0.0f);
                        inGameTimeDisplay.rectTransform.localScale = new Vector3(1, 1, 1);
                        restingScreen.CrossFadeAlpha(0, 0.5f, false);
                        player.resting = false;
                        Time.timeScale = 1.0f;
                    }



                }
                else if (player.exiting && player.alive)
                {
                    if (hours > 5 && hours < 18)
                    {
                        exitToMap();
                    }
                    else
                    {
                        player.exiting = false;
                    }

                }
                else
                {
                    restingScreen.CrossFadeAlpha(1, 0.2f, true);
                    inGameTimeDisplay.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    inGameTimeDisplay.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    inGameTimeDisplay.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    inGameTimeDisplay.rectTransform.localScale = new Vector3(3, 3, 1);
                    inGameTimeDisplay.color = Color.red;
                    inGameTimeDisplay.text = "You died after " + nights.ToString() + " nights";
                    SoundManager.instance.music.Stop();

                    if (!firstTime)
                    {
                        GameObject.Find("Status").SetActive(false);
                        firstTime = true;
                    }
                    Time.timeScale = 0;

                    if (Input.GetButtonDown("Submit"))
                    {
                        showMenu();
                    }

                }
            }
            else if (inMap)
            {
                if (firstTime)
                {
                    currentHouse = GameObject.Find("StartHouse").GetComponent<House>();
                    currentHouse.Activate(true);
                    firstTime = false;
                }
                if (switchLevel)
                {
                    GameObject[] houses = GameObject.FindGameObjectsWithTag("House");
                    for (int i = 0; i < houses.Length; i++)
                    {
                        if (houses[i].transform.position == currentHouse.transform.position)
                        {

                            House house = houses[i].GetComponent<House>();
                            Destroy(currentHouse.gameObject);
                            currentHouse = house;
                            currentHouse.Activate(true);
                        }
                    }
                    switchLevel = false;
                }

                currentLevel = null;
                if (!changingHouse)
                {
                    int direction = -1;
                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        direction = 0;
                    }
                    else if (Input.GetAxis("Horizontal") < 0)
                    {
                        direction = 2;
                    }
                    else if (Input.GetAxis("Vertical") < 0)
                    {
                        direction = 1;
                    }
                    else if (Input.GetAxis("Vertical") > 0)
                    {
                        direction = 3;
                    }
                    House newHouse = currentHouse.MoveToNewHouse(direction);
                    if (newHouse != null)
                    {
                        currentHouse.Activate(false);
                        currentHouse = newHouse;
                        currentHouse.Activate(true);
                        changingHouse = true;
                    }

                }
                else
                {
                    changingHouseTimer += Time.deltaTime;
                    if (changingHouseTimer > 0.5f)
                    {
                        changingHouse = false;
                        changingHouseTimer = 0.0f;
                    }
                }


                if (Input.GetButtonDown("X") || Input.GetButtonDown("Square") || hours >= 18)
                {
                    loadNewLevel(currentHouse.level);
                }
            }
            else if (inMenu)
            {
                HUD.gameObject.SetActive(false);
                Time.timeScale = 0;
                player.gameObject.SetActive(false);
                currentLevel = null;
                if (Input.GetButtonDown("Submit"))
                {
                    Time.timeScale = 1;
                    HUD.gameObject.SetActive(true);
                    restingScreen.gameObject.SetActive(true);
                    restingScreen.CrossFadeAlpha(0, 0, true);
                    loadingScreen.gameObject.SetActive(true);
                    nightImage.gameObject.SetActive(true);
                    nightImage.CrossFadeAlpha(0, 0, true);
                    exitToMap();
                }
                if (Input.GetButtonDown("Circle"))
                {
                    showTutorial();
                }
                if (Input.GetButtonDown("Cancel"))
                {
                    Application.Quit();
                }

            }
            else if (inTutorial)
            {
                if (Input.GetButtonDown("Circle"))
                {
                    showMenu();
                }
            }
        }
        else
        {
            SoundManager.instance.music.Pause();
            Time.timeScale = 0;
            pauseScreen.gameObject.SetActive(true);
            //loadingScreen.CrossFadeAlpha(1.0f, 0, true);
            if (Input.GetButtonDown("Submit"))
            {
                SoundManager.instance.music.UnPause();
                pause = false;
                Time.timeScale = 1;
                pauseScreen.gameObject.SetActive(false);
                //pauseScreen.CrossFadeAlpha(0.0f, 0, true);
            }
            if (Input.GetButtonDown("Cancel"))
            {
                Application.Quit();
            }
        }
        
        
    }

    public void loadNewLevel(string levelName)
    {
        hours++;
        DontDestroyOnLoad(currentHouse);
        currentHouse.gameObject.SetActive(false);
        Time.timeScale = 1;
        loadingScreen.CrossFadeAlpha(1, 0, true);
        SceneManager.LoadScene(levelName);
        switchLevel = true;
        inHouse = true;
        inMenu = false;
        inMap = false;
        loading = true;
    }

    private void exitToMap()
    {
        if (inHouse)
        {
            HouseStats currentHouseStats = currentLevel.GetHouse();
            currentHouseStats.name = SceneManager.GetActiveScene().name;

            bool found = false;
            for(int i = 0; i < houseStats.Count; i++)
            {
                if (houseStats[i].name == currentHouseStats.name)
                {
                    houseStats[i] = currentHouseStats;
                    i = houseStats.Count;
                    found = true;
                }

            }
            if (!found)
            {
                houseStats.Add(currentHouseStats);
            }
        }
        cam.orthographicSize = 10;
        cam.transform.position = new Vector3(0, 0, -1);
        loadingScreen.CrossFadeAlpha(1, 0, true);
        Time.timeScale = 1;
        SceneManager.LoadScene("Map");
        player.gameObject.SetActive(false);
        inHouse = false;
        inMenu = false;
        inMap = true;
        player.exiting = false;
        loading = true;
        if (!firstTime)
        {
            switchLevel = true;
        }

    }

    private void showTutorial()
    {
        inMenu = false;
        inTutorial = true;
        SceneManager.LoadScene("Help");
    }
    

    private void showMenu()
    {
        inTutorial = false;

        inMenu = true;

        Destroy(player.gameObject);
        Destroy(this.gameObject);
        SceneManager.LoadScene("Menu");
        SoundManager.instance.playDayMusic();
    }
}


