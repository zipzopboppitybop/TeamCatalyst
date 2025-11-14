using System.Collections.Generic;
using Catalyst.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [SerializeField] Cycles cycle;
    [SerializeField] PlayerData playerData;

    [SerializeField] private PauseMenuUI menuPause;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] float dayLengthMinutes;
    [SerializeField] int nightStart;
    [SerializeField] int nightEnd;

    public GameObject player;
    public PlayerController playerController;
    public GameObject playerSpawnPos;

    float timeScaleOrig;
    float timeOfDay = 7;
    int day = 1;

    public int cropCount = 0;
    private float moneyOnStart = 0;
    private int cropsDestroyed = 0;

    public bool isPaused = false;
    private bool Won = false;
    private bool Lost = false;
    private bool endByLose = false;
    public bool wasNight;
    public bool IsNight;

    public List<GameObject> crops = new List<GameObject>();

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        player = GameObject.FindWithTag("Player");
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cropCount = crops.Count;
        moneyOnStart = playerData.Currency;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (PauseMenuUI.instance != null && PauseMenuUI.instance.IsScreenOpen)
                return;

            if(!isPaused)
            {
                StatePause();
                if (PauseMenuUI.instance != null) PauseMenuUI.instance.Show();
            }
            else
            {
                StateUnpause();
                if (PauseMenuUI.instance !=null) PauseMenuUI.instance.Hide();
                if (menuPause != null) menuPause.Hide();
            }
        }
        UpdateGameClock();

    }
    public void StatePause()
    {
        isPaused = true;
        Time.timeScale = 0;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void StateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void LoseStateReset()
    {
        Lost = false;
        Won = false;
        player.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;



        cropsDestroyed = 0;
        moneyOnStart = playerData.Currency;
    }
    public void YouWin()
    {
        if (Lost || Won)
            return;

        if(day >= 2 && timeOfDay == nightEnd && PauseMenuUI.instance != null && !PauseMenuUI.instance.IsScreenOpen)
        {
            Won = true;
            StatePause();
            PauseMenuUI.instance.ShowWinScreen();
        }
    }

    public void YouLose()
    {
        if (Won || Lost)
            return;
        Lost = true;
        endByLose = true;
        //StatePause();

        timeOfDay = nightEnd;
        UpdateGameClock();

        StatePause();
        timeOfDay = nightEnd;
        day += 1;
        UpdateGameClock();



        GameObject spawnerObject = GameObject.FindGameObjectWithTag("Spawner");
        if (spawnerObject != null)
        {
            Spawner spawner = spawnerObject.GetComponent<Spawner>();
            if (spawner != null)
            {
                int totalEnemies = 0;
                totalEnemies += spawner.GetCurrentEnemyCount();
                spawner.DespawnAll();

                int cropsToDestroy = Mathf.Min(crops.Count, totalEnemies);

                cropsDestroyed = cropsToDestroy;

                for (int i = 0; i < cropsToDestroy; i++)
                {
                    GameObject crop = crops[crops.Count - 1];
                    crops.RemoveAt(crops.Count - 1);

                    if (crop != null)
                    {
                        Destroy(crop);
                    }
                }
            }
        }
        else
        {
            Debug.Log("No Spawner");
        }
        PauseMenuUI.instance.ShowLoseScreen();
        if (player != null && playerSpawnPos != null)
        {
            player.transform.position = playerSpawnPos.transform.position;
            player.transform.rotation = playerSpawnPos.transform.rotation;
        }
        Debug.Log("Destroying stuff");
    }

    void UpdateGameClock()
    {
        if (!isPaused)
        {
            float totalTimePerGameDay = Mathf.Max(60f, dayLengthMinutes * 60f);
            float hoursPerSec = 24f / totalTimePerGameDay;
            timeOfDay += hoursPerSec * Time.deltaTime;

        }

        if (timeOfDay >= 24)
        {
            timeOfDay -= 24;
        }
        int minutes = (int)(timeOfDay * 60);
        int hour = minutes / 60;
        int minute = minutes % 60;

        bool isAm = hour < 12;
        int hourTwelve = hour % 12;

        if (hourTwelve == 0)
            hourTwelve = 12;

        cycle.ClockText = hourTwelve.ToString("00") + ":" + minute.ToString("00") + (isAm ? " AM" : " PM");


        bool isNight = IsNightHour(hour, nightStart, nightEnd);
        IsNight = isNight;

        if (wasNight && !isNight)
        {
            day += 1;
            moneyOnStart = playerData.Currency;
            ShopUI.instance.SellItems();
        }

        cycle.DayText = "Day " + day.ToString();

        if(!Lost && !Won && !endByLose)
        {
            YouWin();
        }
        wasNight = isNight;

    }

    public void UpdateCropCount(int amt)
    {
        cropCount += amt;
    }
    public int GetDay()
    {

        return day;

    }

    public float UpdateMoneyEarned()
    {
        return playerData.Currency - moneyOnStart;
    }

    public int UpdateCropCount()
    {
        return cropCount;
    }

    public int UpdateCropsDestroyed()
    {
        return cropsDestroyed;
    }



    bool IsNightHour(int hour, int startHour, int endHour)
    {
        int nightStart24 = (startHour < 12) ? startHour + 12 : startHour;
        int nightEnd24 = (endHour == 12) ? 0 : endHour;

        if (nightStart24 > nightEnd24)
        {
            return hour >= nightStart24 || hour < nightEnd24;
        }

        else
        {
            return hour >= nightStart24 && hour < nightEnd24;
        }
    }

    public void AddCrop(GameObject crop)
    {
        crops.Add(crop);
    }



    //public void TogglePlayerController()
    //{

    //    Catalyst.Player.PlayerController = player.GetComponent<Catalyst.Player.PlayerController>();
    //}

}
