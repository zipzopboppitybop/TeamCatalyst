using Catalyst.GamePlay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI instance;

    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSource;

    private VisualElement root;
    private VisualElement pauseMenu;
    private VisualElement settingsMenu;
    private VisualElement LoseScreen;
    private VisualElement WinScreen;
    private VisualElement SavePopup;

    private Button resumeButton;
    private Button settingsButton;
    private Button restartButton;
    private Button quitButton;
    private Button backButton;
    private Button winOkButton;
    private Button loseOkButton;
    private Button saveButton;
    private Button loadButton;
    private Button dontSaveButton;
    private Button mainMenuButton;

    private Label winDayLabel;
    private Label winCropLabel;
    private Label winMoneyLabel;
    private Label loseCropLabel;


    private void Awake()
    {
        instance = this;

        if (uIDocument == null)
            uIDocument = GetComponent<UIDocument>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

        root = uIDocument.rootVisualElement;

        pauseMenu = root.Q<VisualElement>("PauseMenu");
        settingsMenu = root.Q<VisualElement>("SettingsMenu");
        LoseScreen = root.Q<VisualElement>("YouLose");
        WinScreen = root.Q<VisualElement>("YouWin");
        SavePopup = root.Q<VisualElement>("SavePopup");

        resumeButton = root.Q<Button>("resumeButton");
        settingsButton = root.Q<Button>("settingsButton");
        restartButton = root.Q<Button>("restartButton");
        quitButton = root.Q<Button>("quitButton");
        backButton = root.Q<Button>("backButton");
        winOkButton = root.Q<Button>("winOk");
        loseOkButton = root.Q<Button>("loseOk");
        mainMenuButton = root.Q<Button>("mainMenuButton");

        saveButton = root.Q<Button>("saveButton");
        loadButton = root.Q<Button>("loadButton");
        dontSaveButton = root.Q<Button>("dontSaveButton");


        winDayLabel = root.Q<Label>("DayCycle");
        winCropLabel = root.Q<Label>("CropCount");
        winMoneyLabel = root.Q<Label>("MoneyEarned");
        loseCropLabel = root.Q<Label>("CropsDestroyed");


        Hide();


        if (resumeButton != null)
            resumeButton.clicked += () => { OnClickSound(); OnResumeButtonClicked(); };

        if (settingsButton != null)
            settingsButton.clicked += () => { OnClickSound(); OnSettingsButtonClicked(); };

        if (restartButton != null)
            restartButton.clicked += () => { OnClickSound(); OnRestartButtonClicked(); };

        if (quitButton != null)
            quitButton.clicked += () => { OnClickSound(); OnQuitButtonClicked(); };

        if (backButton != null)
            backButton.clicked += () => { OnClickSound(); OnBackButtonClicked(); };
        if (winOkButton != null)
            winOkButton.clicked += () => { OnClickSound(); OnWinOkClicked(); };
        if (loseOkButton != null)
            loseOkButton.clicked += () => { OnClickSound(); OnLoseOkClicked(); };
        if (mainMenuButton != null)
            mainMenuButton.clicked += () => { OnClickSound(); OnMainMenuClicked(); };



        if (saveButton != null)
            saveButton.clicked += () => { OnClickSound(); SaveSystem.Save(); OnQuitConfirmed(); };
        if (loadButton != null)
            loadButton.clicked += () => { OnClickSound(); SaveSystem.Load(); };

        if (dontSaveButton != null)
            dontSaveButton.clicked += () => { OnClickSound(); OnQuitConfirmed(); };

    }
    public bool IsScreenOpen =>
        (pauseMenu.style.display == DisplayStyle.Flex) ||
        (settingsMenu.style.display == DisplayStyle.Flex) ||
        (LoseScreen.style.display == DisplayStyle.Flex) ||
        (WinScreen.style.display == DisplayStyle.Flex);

    private void OnClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    private void OnResumeButtonClicked()
    {
        GameManager.instance.StateUnpause();
        Hide();
    }
    private void OnSettingsButtonClicked()
    {
        Hide();
        settingsMenu.style.display = DisplayStyle.Flex;
    }
    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void OnBackButtonClicked()
    {
        settingsMenu.style.display = DisplayStyle.None;
        Show();
    }
    private void OnQuitButtonClicked()
    {
        //ShowSavePopup();
        OnQuitConfirmed();

    }

    private void OnQuitConfirmed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void OnWinOkClicked()
    {
        GameManager.instance.LoseStateReset();
        GameManager.instance.StateUnpause();
        Hide();

    }
    private void OnLoseOkClicked()
    {
        GameManager.instance.LoseStateReset();
        GameManager.instance.StateUnpause();
        LoseScreen.style.display = DisplayStyle.None;
    }
    public void Show()
    {
        settingsMenu.style.display = DisplayStyle.None;
        LoseScreen.style.display = DisplayStyle.None;
        WinScreen.style.display = DisplayStyle.None;

        pauseMenu.style.display = DisplayStyle.Flex;
    }
    public void ShowLoseScreen()
    {
        Hide();
        GameManager.instance.StatePause();
        UpdateStats();
        LoseScreen.style.display = DisplayStyle.Flex;
    }
    public void ShowWinScreen()
    {
        Hide();
        GameManager.instance.StatePause();
        UpdateStats();
        WinScreen.style.display = DisplayStyle.Flex;
    }
    public void OnMainMenuClicked()
    {
        Hide();
        SceneManager.LoadScene(1);
    }

    public void ShowSavePopup()
    {
        SavePopup.style.display = DisplayStyle.Flex;
    }
    public void Hide()
    {
        pauseMenu.style.display = DisplayStyle.None;
        settingsMenu.style.display = DisplayStyle.None;
        LoseScreen.style.display = DisplayStyle.None;
        WinScreen.style.display = DisplayStyle.None;
    }
    private void UpdateStats()
    {
        int day = GameManager.instance.GetDay();
        int cropCount = GameManager.instance.GetCropCount();
        float moneyEarned = GameManager.instance.UpdateMoneyEarned();
        int cropsDestroyed = GameManager.instance.UpdateCropsDestroyed();

        winDayLabel.text = $"Day Cycle: {day}";
        winCropLabel.text = $"Total Crops: {cropCount}";
        winMoneyLabel.text = $"Money Earned Today: ${moneyEarned}";
        loseCropLabel.text = $"Crops Destroyed: {cropsDestroyed}";
    }
}
