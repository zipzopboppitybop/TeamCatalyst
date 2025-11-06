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
    [SerializeField] float dayLengthMinutes;
    [SerializeField] int nightStart;
    [SerializeField] int nightEnd;
    [SerializeField] Image dayImage;
    [SerializeField] Image nightImage;

    //public GameObject player;
    //public PlayerController playerScript;
    //public Image playerHPBar;

    float timeScaleOrig;
    float timeOfDay = 7;
    int day = 1;

    public bool isPaused;
    bool wasNight;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //instance = this;
        timeScaleOrig = Time.timeScale;

        //player = GameObject.FindWithTag("Player");
       // playerScript = player.GetComponent<PlayerController>();
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
            float totalTimePerGameDay = Mathf.Max(60f, dayLengthMinutes * 60f);
            float hoursPerSec = 24f / totalTimePerGameDay;
            timeOfDay += hoursPerSec * Time.deltaTime;

        }

        if(timeOfDay >= 24)
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

        clockText.text = hourTwelve.ToString("00") + ":" + minute.ToString("00") +(isAm ? " AM" : " PM");
        
        bool isNight = IsNightHour(hour, nightStart, nightEnd);
        if(wasNight && !isNight)
        {
            day += 1;
        }

        if(dayImage) dayImage.gameObject.SetActive(!isNight);
        if(nightImage) nightImage.gameObject.SetActive(isNight);  

        dayText.text = "Day " + day.ToString();
        wasNight = isNight;
        
    }

    bool IsNightHour(int hour, int startHour, int endHour)
    {
        int nightStartPM = (startHour == 12) ? 12 : startHour +12;
        int nightEndAM = (endHour == 12) ? 0 : endHour;

       if(nightStartPM < nightEndAM)
        {
            return hour >= nightStartPM && hour < nightEndAM;
        }
       else
        {
            return hour >=nightStartPM || hour < nightEndAM;
        }
    }

}
