using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("MainMenu")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _tutorialPanel;

    [Header("Ammo Bar")]
    [SerializeField] private GameObject _weaponBar;
    [SerializeField] private Image _weaponIconActive;
    [SerializeField] private Sprite[] _weaponIcons;
    [SerializeField] private TextMeshProUGUI _ammoClipQty;
    [SerializeField] private TextMeshProUGUI _ammoTotalQty;


    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _damageImpactImage;
    [SerializeField] private Image _deathScreenImage;
    [SerializeField] private GameObject _deathMessage;
    [SerializeField] private GameObject _endGameMessage;
    [SerializeField] private GameObject _pausePanel;
    private float _damageImpactLength = 1.5f;
    private float _damageImpactTimer;
    private float _deathScreenLength = 2;
    private float _deathScreenTimer;

    private List<float> frames;
    private float fps = 0;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1 && GameManager.instance.weaponController.CurrentWeapon == null)
        {
            _weaponBar.SetActive(false);
        }
        frames = new List<float>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pausePanel != null && !_pausePanel.activeInHierarchy)
            {
                Pause();
            }
            else if(_pausePanel != null && _tutorialPanel.activeInHierarchy)
            {
                BackToMenu();
            }
            else if(_pausePanel != null)
            {
                Continue();
            }
        }

        fps = 1 / Time.unscaledDeltaTime;
        frames.Add(fps);
        if (Input.GetKeyDown(KeyCode.I))
        {
            AverageFPS(frames);
        }
    }

    private void AverageFPS(List<float> frames)
    {
        float FPStotal = 0;
        float averageFPS;
        for(int i = 0; i <frames.Count; i++)
        {
            FPStotal += frames[i];
        }
        averageFPS = FPStotal / frames.Count;
        Debug.LogError($"average fps is {averageFPS}");
    }

    public void UpdateAmmoUI(int clipQty, int totalQty, int activeWeapon)
    {
        if (!_weaponBar.activeInHierarchy)
        {
            _weaponBar.SetActive(true);
        }
        _ammoClipQty.text = clipQty.ToString();
        _ammoTotalQty.text = totalQty.ToString();
        _weaponIconActive.sprite = _weaponIcons[activeWeapon];
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        float healthSliderValue = (float)currentHealth / (float)maxHealth;
        _healthSlider.value = healthSliderValue;
    }

    public void PlayDamageImpact()
    {
        StartCoroutine(DamageImpact());
    }

    private IEnumerator DamageImpact()
    {
        Color tempColor = _damageImpactImage.color;
        tempColor.a = 1;
        _damageImpactImage.color = tempColor;

        while (_damageImpactTimer < _damageImpactLength)
        {
            _damageImpactTimer += Time.deltaTime;
            tempColor.a = 1 - _damageImpactTimer / _damageImpactLength;
            _damageImpactImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 0;
        _damageImpactImage.color = tempColor;
        _damageImpactTimer = 0;
    }

    public void PlayDeathScreen()
    {
        StartCoroutine(Blackout(_deathMessage));
    }

    public void PlayEndOfLevelScreen()
    {
        StartCoroutine(Blackout(_endGameMessage));
    }

    private IEnumerator Blackout(GameObject message)
    {
        Color tempColor = _deathScreenImage.color;
        tempColor.a = 0;
        _deathScreenImage.color = tempColor;

        while (_deathScreenTimer < _deathScreenLength)
        {
            _deathScreenTimer += Time.deltaTime;
            tempColor.a = _deathScreenTimer / _deathScreenLength;
            _deathScreenImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 1;
        _deathScreenImage.color = tempColor;
        _damageImpactTimer = 0;

        message.SetActive(true);
        TurnOnCursor();
    }

    //Buttons main menu

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartGame()
    {
        Continue();
        SceneManager.LoadScene(1);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenTutorial()
    {
        _tutorialPanel.SetActive(true);
        _mainMenuPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        _tutorialPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    //buttons pause menu

    private void Pause()
    {
        if (!GameManager.instance.playerHealth.IsDead)
        {
            Time.timeScale = 0;
            _pausePanel.SetActive(true);
            GameManager.instance.playerController._canMove = false;
            GameManager.instance.weaponController.CanShoot(false);
            TurnOnCursor();
        }
    }

    public void Continue()
    {
        Time.timeScale = 1;
        _pausePanel.SetActive(false);
        GameManager.instance.playerController._canMove = true;
        GameManager.instance.weaponController.CanShoot(true);
        TurnOffCursor();
    }

    public void TurnOnCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void TurnOffCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
