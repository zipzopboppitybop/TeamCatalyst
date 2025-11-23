using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clickSound;

    private VisualElement root;

    private VisualElement mail;
    private VisualElement grabSupplies;
    private VisualElement plantCrops;
    private VisualElement surviveNight;
    private VisualElement sellNote;
    private VisualElement fenceNote;
    private VisualElement tutorial;

    private Button okTutorial;
    private Button cancelTutorial;
    private Button harvestNoteClose;
    private Button fenceNoteClose;
    private Button surviveNightClose;

    private bool tutorialEnabled = true;
    private bool tutorialHasRun = false;
    private const string TutorialSeen = "TutorialSeen";
    private bool mailOpened;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;

        mail = root.Q<VisualElement>("YouHaveMail");
        grabSupplies = root.Q<VisualElement>("TutorialMailBox");
        plantCrops = root.Q<VisualElement>("TutorialPlantCrops");
        surviveNight = root.Q<VisualElement>("TutorialSurvive");
        sellNote = root.Q<VisualElement>("NoteHarvestSell");
        fenceNote = root.Q<VisualElement>("NoteDefense");
        tutorial = root.Q<VisualElement>("Tutorial");


        okTutorial = root.Q<Button>("TutorialOkay");
        cancelTutorial = root.Q<Button>("TutorialSkip");
        harvestNoteClose = root.Q<Button>("HarvestNoteClosed");
        fenceNoteClose = root.Q<Button>("FenceNoteClosed");
        surviveNightClose = root.Q<Button>("SurviveClosed");

        okTutorial.clicked += () => { OnClickSound(); OnOkButtonClicked(); };
        cancelTutorial.clicked += () => { OnClickSound(); OnSkipButtonClicked(); };
        harvestNoteClose.clicked += () => { OnClickSound(); OnHarvestNoteButtonClicked(); };
        fenceNoteClose.clicked += () => { OnClickSound(); OnFenceNoteButtonClicked(); };
        surviveNightClose.clicked += () => { OnClickSound(); OnSurviveButtonClicked(); };

        HideAll();

        //bool seen = PlayerPrefs.GetInt(TutorialSeen, 0) != 0;

        //if(seen)
        //{
        //    tutorialEnabled = false;
        //    tutorialHasRun = true;
        //    return;
        //}

        StartTutorial();

    }
    private void OnClickSound()
    {
        if(audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    private void OnOkButtonClicked()
    {
        tutorial.style.display = DisplayStyle.None;
        plantCrops.style.display = DisplayStyle.Flex;

        GameManager.instance.StateUnpause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;
    }
    private void OnSkipButtonClicked()
    {
        tutorial.style.display = DisplayStyle.None;

        SkipTutorial();
        MarkAsSeen();

        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;

        tutorialHasRun = true;
        tutorialEnabled = false;
    }
    private void OnHarvestNoteButtonClicked()
    {
        sellNote.style.display = DisplayStyle.None;
        GameManager.instance.StateUnpause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;
    }
    private void OnFenceNoteButtonClicked()
    {
        fenceNote.style.display = DisplayStyle.None;

        Debug.Log("Calling fence not");
        MarkAsSeen();

        GameManager.instance.StateUnpause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;

        tutorialEnabled = false;
        tutorialHasRun = true;
    }
    private void OnSurviveButtonClicked()
    {
        Debug.Log("Calling survive button");
        MarkAsSeen();
        surviveNight.style.display = DisplayStyle.None;
        fenceNote.style.display = DisplayStyle.Flex;

        GameManager.instance.StatePause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;

    }
   


   
  
     private void HideAll()
    {
        mail.style.display = DisplayStyle.None;
        grabSupplies.style.display = DisplayStyle.None;
        plantCrops.style.display = DisplayStyle.None;
        surviveNight.style.display = DisplayStyle.None;
        sellNote.style.display = DisplayStyle.None;
        fenceNote.style.display = DisplayStyle.None;
    }



    private void StartTutorial()
    {
        tutorialEnabled = true;
        mail.style.display = DisplayStyle.Flex;
    }

    private void SkipTutorial()
    {
        tutorial.style.display = DisplayStyle.None;

        GameManager.instance.StateUnpause();
        tutorialEnabled = false;
        tutorialHasRun = true;
        HideAll();
    }

    public void OnMailboxOpened()
    {
        if (!tutorialEnabled || tutorialHasRun || mailOpened) return;
        
            mail.style.display= DisplayStyle.None;

            grabSupplies.style.display = DisplayStyle.Flex;

    }

    public void OnMailboxClosed()
    {
        
        if (!tutorialEnabled || tutorialHasRun || mailOpened) return;

        grabSupplies.style.display = DisplayStyle.None;
        tutorial.style.display = DisplayStyle.Flex;
        
        GameManager.instance.StatePause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;
        mailOpened = true;
    }
    
    public void OnCropFullyPlanted()
    {
        if (!tutorialEnabled || tutorialHasRun) return;
        plantCrops.style.display = DisplayStyle.None;

        GameManager.instance.StatePause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;
        sellNote.style.display = DisplayStyle.Flex;
    }

    public void OnNight()
    {
        Debug.Log("Calling on night");
        if (!tutorialEnabled || tutorialHasRun) return;
        surviveNight.style.display = DisplayStyle.Flex;
        GameManager.instance.StatePause();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;

    }

    //public void OnNightEnd()
    //{
    //    Debug.Log("Calling night end");
    //    if (!tutorialEnabled || tutorialHasRun) return;
    //    surviveNight.style.display = DisplayStyle.None;
    //    fenceNote.style.display = DisplayStyle.Flex;

    //    GameManager.instance.StatePause();
    //    UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    //    UnityEngine.Cursor.visible = true;
      
    //}
    private void MarkAsSeen()
    {
        tutorialEnabled = false;
        tutorialHasRun = true;
        PlayerPrefs.SetInt(TutorialSeen, 1);
        PlayerPrefs.Save();
    }
   

}
