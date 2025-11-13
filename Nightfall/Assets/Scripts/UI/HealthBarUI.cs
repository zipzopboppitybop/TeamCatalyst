using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI instance;

    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private Catalyst.Player.PlayerData playerData;
    [SerializeField] private Cycles cycles;

    private VisualElement root;
    private VisualElement HUD;
    private VisualElement healthBar;
    private VisualElement staminaBar;
    private VisualElement LoseNote;
    private VisualElement _weaponContainer;
    private VisualElement _reticleContainer;
    private Label currencyLabel;
    private Label cropsDestroyedText;
    private Button okButton;
    //private Label healthBarLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        if (uIDocument == null)
            uIDocument = GetComponent<UIDocument>();

        root = uIDocument.rootVisualElement; ;
        HUD = root.Q<VisualElement>("HUDContainer");
        healthBar = root.Q<VisualElement>("HealthBarGREEN");
        staminaBar = root.Q<VisualElement>("StamBarFront");
        currencyLabel = root.Q<Label>("moneyText");
        cropsDestroyedText = root.Q<Label>("CropsDestroyed");
        LoseNote = root.Q<VisualElement>("YouLose");

        _weaponContainer = root.Q<VisualElement>("WeaponContainer");
        _reticleContainer = root.Q<VisualElement>("ReticleContainer");

        HUD.style.display = DisplayStyle.Flex;
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateCurrency();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateCurrency();

    }

    private void UpdateHealthBar()
    {
        float healthPercent = playerData.Health / playerData.HealthMax;
        if (healthBar != null)
        {
            healthBar.style.width = Length.Percent(healthPercent * 100);
        }
    }

    private void UpdateStaminaBar()
    {
        RegenStamina();

        float staminaPercent = (float)playerData.Stamina / playerData.StaminaMax;
        if (staminaBar != null)
        {
            staminaBar.style.width = Length.Percent(staminaPercent * 100);
        }
    }
    public void ShowWeaponUI()
    {
        StartCoroutine(ShowWeaponCoroutine());
    }
    public void HideWeaponUI()
    {
        StartCoroutine(HideWeaponCoroutine());
    }
    private IEnumerator ShowWeaponCoroutine()
    {

        _weaponContainer.style.display = DisplayStyle.Flex;
        _reticleContainer.style.display = DisplayStyle.Flex;
        yield return null;
    }
    private IEnumerator HideWeaponCoroutine()
    {

        _weaponContainer.style.display = DisplayStyle.None;
        _reticleContainer.style.display = DisplayStyle.None;
        yield return null;


    }

    private void RegenStamina()
    {
        if (playerData.Stamina < playerData.StaminaMax)
        {
            playerData.Stamina += (int)(playerData.StaminaRegen * Time.deltaTime);
        }
    }
    private void UpdateCurrency()
    {
        if (currencyLabel != null)
        {
            currencyLabel.text = $"${playerData.Currency}";
        }
    }
    public void ShowLoseScreen()
    {
        StartCoroutine(CloseScreen());
    }

    private void OnButtonClicked()
    {
        LoseNote.style.display = DisplayStyle.None;
    }

    private IEnumerator CloseScreen()
    {
        if (LoseNote != null)
        {
            LoseNote.style.display = DisplayStyle.Flex;
            cropsDestroyedText.text = $"Crops Destroyed: {GameManager.instance.cropsDestroyed}";
        }
        yield return new WaitForSeconds(2f);

        LoseNote.style.display = DisplayStyle.None;
    }
}
