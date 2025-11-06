
using UnityEngine;
//using Player.Controller;


namespace Catalyst.GamePlay
{
    public class GameManager : MonoBehaviour
    {

        public static GameManager Instance;

        public GameObject player;

        public bool isPaused = false;
        float timeScaleOrig;

        //public PlayerController playerScript;
        public GameObject playerSpawnPos;
        public bool playerIsDead = false;
        void Awake()
        {
            Instance = this;
            timeScaleOrig = Time.timeScale;

            player = GameObject.FindWithTag("Player");

            //playerScript = player.GetComponent<PlayerController>();

            playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
        }

        void Start()
        {
            Debug.Log("Starting GameManager");
            StateUnPause();

        }

        public void StatePause()
        {
            isPaused = !isPaused;
            Time.timeScale = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //playerScript.enabled = false;
        }
        public void StateUnPause()
        {
            isPaused = !isPaused;
            Time.timeScale = timeScaleOrig;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //playerScript.enabled = true;
        }

        public void UpdateGameGoal(int goal)
        {

        }
        public void YouLose()
        {
            StatePause();
            // Logic for lost condition activating LoseMenu
        }

        public void YouWin()
        {
            StatePause();
        }
    }
}

