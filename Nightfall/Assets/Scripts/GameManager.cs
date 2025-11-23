using System.Collections.Generic;
using Catalyst.Player;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [SerializeField] Cycles cycle;
    [SerializeField] PlayerData playerData;
    [SerializeField] private Spawner spawner;

    [SerializeField] private PauseMenuUI menuPause;

    [SerializeField] float dayLengthMinutes;
    [SerializeField] int nightStart;
    [SerializeField] int nightEnd;

    public GameObject player;
    public PlayerController playerController;
    public GameObject playerSpawnPos;

    public float timeScaleOrig;
    float timeOfDay = 7;
    int day = 1;

    public int cropCount = 0;
    private float moneyOnStart = 0;
    private int cropsDestroyed = 0;

    public bool isPaused = false;
    private bool Won = false;
    private bool Lost = false;
    public bool wasNight;
    public bool IsNight;

    public List<GameObject> crops;

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
        playerData = playerController.GetPlayerData();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crops = playerData.crops;
        cropCount = playerData.crops.Count;
        moneyOnStart = playerData.Currency;
        playerData.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (playerController.isInventoryOpen)
            {
                playerController.playerInventory.toggleInventory = false;
                playerController.playerInventory.CloseChest();
                playerController.isInventoryOpen = false;

                StatePause();
                if (PauseMenuUI.instance != null)
                {
                    PauseMenuUI.instance.Show();
                }

                return;
            }

            if (PauseMenuUI.instance != null && PauseMenuUI.instance.IsScreenOpen)
            {
                StateUnpause();
                PauseMenuUI.instance.Hide();
                if (menuPause != null)
                {
                    menuPause.Hide();
                }

                return;
            }

            StatePause();
            if (PauseMenuUI.instance != null)
            {
                PauseMenuUI.instance.Show();
            }

            return;
        }

        if (isPaused && playerController.isInventoryOpen)
        {
            playerController.playerInventory.toggleInventory = false;
            playerController.playerInventory.CloseChest();
            playerController.isInventoryOpen = false;
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
        if (Won)
            return;

        if (day >= 2 && PauseMenuUI.instance != null && !PauseMenuUI.instance.IsScreenOpen)
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
        //timeOfDay = nightEnd;

        StatePause();



        GameObject spawnerObject = GameObject.FindGameObjectWithTag("Spawner");
        if (spawnerObject != null)
        {
            Spawner spawner = spawnerObject.GetComponent<Spawner>();
            if (spawner != null)
            {
                int totalEnemies = 0;
                totalEnemies += spawner.GetCurrentEnemyCount();
                spawner.DespawnAll();

                int cropsToDestroy = Mathf.Min(playerData.crops.Count, totalEnemies);

                cropsDestroyed = cropsToDestroy;

                for (int i = 0; i < cropsToDestroy; i++)
                {
                    GameObject crop = playerData.crops[playerData.crops.Count - 1];
                    playerData.crops.RemoveAt(playerData.crops.Count - 1);

                    if (crop != null)
                    {
                        Destroy(crop);
                    }
                }
            }
        }
        PauseMenuUI.instance.ShowLoseScreen();
        if (player != null && playerSpawnPos != null)
        {
            player.transform.position = playerSpawnPos.transform.position;
            player.transform.rotation = playerSpawnPos.transform.rotation;
        }
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
        if(isNight)
        {
            Debug.Log("Calling tutorial is night");
            TutorialManager.Instance.OnNight();
        }

        if (wasNight && !isNight)
        {
            day += 1;
            moneyOnStart = playerData.Currency;
            ShopUI.instance.SellItems();
            Livestock[] allLivestock = Object.FindObjectsByType<Livestock>(FindObjectsSortMode.None);
            spawner.DespawnAll();


            foreach (Livestock livestock in allLivestock)
            {
                livestock.OnDayStart();
            }

            if (!Lost && !Won)
            {
                YouWin();
            }
        }

        cycle.DayText = "Day " + day.ToString();

        wasNight = isNight;

        if (day == 2 && PlayerInventoryUI.Instance != null)
        {
            PlayerInventoryUI.Instance.OnDayOneAchieved();
        }

    }

    public void UpdateCropCount(int amt)
    {
        cropCount += amt;
        playerData.CropCount = cropCount;
        crops = playerData.crops;
    }
    public int GetDay()
    {

        return day;

    }

    public float UpdateMoneyEarned()
    {
        return playerData.Currency - moneyOnStart;
    }

    public int GetCropCount()
    {
        return playerData.crops.Count;
    }

    public int UpdateCropsDestroyed()
    {
        return cropsDestroyed;
    }
    public float TimePercent { get { return timeOfDay / 24f; } }

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
        playerData.crops.Add(crop);
    }

}
