using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI instance;

    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private Catalyst.Player.PlayerData playerData;

    private VisualElement root;
    private VisualElement HUD;
    private VisualElement healthBar;
    private VisualElement staminaBar;
    private VisualElement takingDamage;
    private VisualElement lowHealth;
   
    private VisualElement _weaponContainer;
    private VisualElement _reticleContainer;
    private Label currencyLabel;

    private float prevHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        if (uIDocument == null)
            uIDocument = GetComponent<UIDocument>();

        prevHealth = playerData.Health;

        root = uIDocument.rootVisualElement;;
        root = uIDocument.rootVisualElement; ;
        HUD = root.Q<VisualElement>("HUDContainer");
        healthBar = root.Q<VisualElement>("HealthBarGREEN");
        staminaBar = root.Q<VisualElement>("StamBarFront");
        currencyLabel = root.Q<Label>("moneyText");

        takingDamage = root.Q<VisualElement>("TakingDamage");
        lowHealth = root.Q<VisualElement>("LowHealth");
        
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
        UpdateDamageTaken();
        UpdateLowHealthAlert();

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
            currencyLabel.text = $"${(float)playerData.Currency}";
        }
    }
    private void UpdateDamageTaken()
    {   
        if (playerData.Health < prevHealth)
        {
            takingDamage.style.display = DisplayStyle.Flex;
        }
        else
        {
            takingDamage.style.display= DisplayStyle.None;
        }
        prevHealth = playerData.Health;
    }
    private void UpdateLowHealthAlert()
    {
        float healthPercent = ((float)playerData.Health / (float)playerData.HealthMax) * 100;

        if (healthPercent <= 20)
            lowHealth.style.display = DisplayStyle.Flex;
        else
            lowHealth.style.display = DisplayStyle.None;
    }
}
