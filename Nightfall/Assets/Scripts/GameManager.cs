using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] TMP_Text clockText;
    [SerializeField] TMP_Text dayText;
    [SerializeField] float dayLengthSeconds;
    [SerializeField] int nightStart;
    [SerializeField] int nightEnd;
    [SerializeField] Image dayImage;
    [SerializeField] Image nightImage;

    //public GameObject player;
    //public PlayerController playerScript;
    //public Image playerHPBar;

    float timeScaleOrig;
    float timeOfDay = 5;
    int day = 1;

    public bool isPaused;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        //player = GameObject.FindWithTag("Player");
        //playerScript = player.GetComponent<PlayerController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
        updateGameClock();

    }
    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;

    }
    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    void updateGameClock()
    {
        if(!isPaused)
        {
            float hoursPerSec = 24 / Mathf.Max(0.01f, dayLengthSeconds);
            timeOfDay += hoursPerSec * Time.deltaTime;

        }

        if(timeOfDay >= 24)
        {
            timeOfDay -= 24;
            day += 1;
        }

        int hour = Mathf.FloorToInt(timeOfDay);
        int minute = Mathf.FloorToInt((timeOfDay - hour) * 60);

        clockText.text = hour.ToString("00") + ":" + minute.ToString("00");
        dayText.text = "Day " + day.ToString();

        bool isNight = IsNightHour(hour, nightStart, nightEnd);
        
    }

    bool IsNightHour(int hour, int startHour, int endHour)
    {
        if (startHour == endHour)
            return false;
        if (startHour < endHour)
            return hour >= startHour && hour < endHour;
        else
            return hour>= startHour || hour < endHour;
    }

}
