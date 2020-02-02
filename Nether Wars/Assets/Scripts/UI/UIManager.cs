using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public GameResources resourceFile;

    [Space]
    public GameObject genericUI;
    public Camera cameraGeneric;
    public CameraController genericCamController;

    [Space]
    public AlchemyUI alchemyUI;

    [Space]
    public GameObject menuUI;

    public static UIManager local;
    public static GameObject activeUI;
    public static Camera activeCamera;
    public static Menues activeMenu = Menues.generic;
    static bool isHidden = false;

    public static int screenW, screenH;
    public static Vector3 mainCameraFocusPoint { get => local.genericCamController.focusPoint.position; }
    public static float mainCameraFieldOfView { get => local.cameraGeneric.fieldOfView; }

    void Awake()
    {
        local = this;
        SwitchMenu(Menues.generic);
    }

    void Start()
    {
        UpdateUIContext();
        StartCoroutine(StartSceneRoutine());
    }

    void Update()
    {
        UpdateUIContext();
    }

    IEnumerator StartSceneRoutine()
    {
        FadeIn();
        HideUI();
        Player.controlEnabled = false;
        yield return new WaitForSeconds(3);
        Player.controlEnabled = true;
        ShowUI();
    }

    static void UpdateUIContext()
    {
        screenW = activeCamera.pixelWidth;
        screenH = activeCamera.pixelHeight;
    }

    public static void HideUI()
    {
        ShowUI(false);
    }

    public static void ShowUI()
    {
        ShowUI(true);
    }

    static void ShowUI(bool mode)
    {
        activeUI.SetActive(mode);
        isHidden = !mode;
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public static void SwitchMenu (Menues newMenu)
    {
        if (activeUI)
            activeUI.SetActive(false);
        if (activeCamera)
            activeCamera.gameObject.SetActive(false);

        switch (activeMenu)
        {
            case Menues.generic:
                Player.mouseOverUI = false;
                break;
            case Menues.alchemy:
                Alchemy.ClearReceipt();
                break;
            case Menues.pause:
                ResumeGame();
                break;
        }

        switch(newMenu)
        {
            case Menues.generic:
                activeUI = local.genericUI;
                activeCamera = local.cameraGeneric;
                break;
            case Menues.alchemy:
                activeUI = local.alchemyUI.UIObject;
                activeCamera = local.alchemyUI.camera;
                ClearCreatureName();
                ClearErrorMessage();
                break;
            case Menues.pause:
                activeUI = local.menuUI;
                PauseGame();
                break;
        }
        if (!isHidden || activeUI == local.menuUI)
            activeUI.SetActive(true);
        activeCamera.gameObject.SetActive(true);
        activeMenu = newMenu;
    }

    public static void SwitchCamera(Camera toCamera)
    {
        if (activeCamera)
            activeCamera.gameObject.SetActive(false);
        activeCamera = toCamera;
        activeCamera.gameObject.SetActive(true);
    }

    #region Звуки и эффекты

    public static void PlaySound(AudioClip sound)
    {
        local.audioSource.PlayOneShot(sound);
    }

    public static void FadeOut()
    {
        local.animator.SetTrigger("fadeOut");
    }

    public static void FadeIn()
    {
        local.animator.SetTrigger("fadeIn");
    }

    #endregion

    #region Интерфейс меню Алхимии
    public static string GetErrorMessage (ErrCodesAlchemy errCode)
    {
        switch (errCode)
        {
            case ErrCodesAlchemy.singleElement:
                return "1 правило Алхимии: Неутолимость - одного элемента недостаточно";
            case ErrCodesAlchemy.wrongProportion:
                return "2 правило Алхимии: Дисбаланс - элементы нельзя соединять в равных пропорциях";
            case ErrCodesAlchemy.BoneContrShadow:
                return "3 правило Алхимии: Антипатия - Кости не смешиваются с Мраком";
            case ErrCodesAlchemy.SlimeContrCinder:
                return "3 правило Алхимии: Антипатия - Желчь не смешивается с Пеплом";
            case ErrCodesAlchemy.notEnoughLevel:
                return "Уровень Владыки слишком мал";
            case ErrCodesAlchemy.notEnoughMana:
                return "У Владыки не хватает маны";
            case ErrCodesAlchemy.notEnoughSupply:
                return "Недостаточный уровень источника";
            case ErrCodesAlchemy.notEnoughLimit:
                return "Слишком много существ";
            default:
                return "u r mom geh";
        }
    }

    public static void LogErrorMessage(ErrCodesAlchemy errCode)
    {
        Debug.Log(GetErrorMessage(errCode));
    }

    public static void ShowErrorMessage(ErrCodesAlchemy errCode)
    {
        ClearCreatureName();
        local.alchemyUI.errorLabel.text = GetErrorMessage(errCode);
        local.alchemyUI.errorLabelAnimator.SetTrigger("animate");
    }
    public static void ClearErrorMessage()
    {
        local.alchemyUI.errorLabel.text = "";
        local.alchemyUI.errorLabelAnimator.SetTrigger("hide");
    }

    public static void ShowCreatureName(string name, Perk perk)
    {
        ClearErrorMessage();

        local.alchemyUI.creatureLabelName.text = name;

        local.alchemyUI.creatureLabelPerk.text = perk.label;
        local.alchemyUI.creatureLabelHint1.text = perk.hint;
        Color perkcolor = local.resourceFile.GetElementColor(perk.element);
        perkcolor.a = 0;
        local.alchemyUI.creatureLabelPerk.color = perkcolor;
        local.alchemyUI.creatureLabelHint1.color = perkcolor;

        local.alchemyUI.creatureLabelPerk1.text = "";
        local.alchemyUI.creatureLabelPerk2.text = "";
        local.alchemyUI.creatureLabelHint2.text = "";

        local.alchemyUI.creatureLabelAnimator.SetTrigger("animate");
    }
    public static void ShowCreatureName(string name, Perk perk1, Perk perk2)
    {
        ClearErrorMessage();

        local.alchemyUI.creatureLabelName.text = name;
        local.alchemyUI.creatureLabelPerk.text = "";

        local.alchemyUI.creatureLabelPerk1.text = perk1.label;
        local.alchemyUI.creatureLabelHint1.text = perk1.hint;
        Color perk1color = local.resourceFile.GetElementColor(perk1.element);
        perk1color.a = 0;
        local.alchemyUI.creatureLabelPerk1.color = perk1color;
        local.alchemyUI.creatureLabelHint1.color = perk1color;

        local.alchemyUI.creatureLabelPerk2.text = perk2.label;
        local.alchemyUI.creatureLabelHint2.text = perk2.hint;
        Color perk2color = local.resourceFile.GetElementColor(perk2.element);
        perk2color.a = 0;
        local.alchemyUI.creatureLabelPerk2.color = perk2color;
        local.alchemyUI.creatureLabelHint2.color = perk2color;

        local.alchemyUI.creatureLabelAnimator.SetTrigger("animate");
    }
    public static void ClearCreatureName()
    {
        local.alchemyUI.creatureLabelName.text = "";
        local.alchemyUI.creatureLabelPerk.text = "";
        local.alchemyUI.creatureLabelPerk1.text = "";
        local.alchemyUI.creatureLabelPerk2.text = "";
        local.alchemyUI.creatureLabelHint1.text = "";
        local.alchemyUI.creatureLabelHint2.text = "";

        local.alchemyUI.creatureLabelAnimator.SetTrigger("hide");
    }
    #endregion
}

public enum Menues { generic, alchemy, pause}

[System.Serializable]
public class AlchemyUI: object
{
    public GameObject UIObject;
    public Camera camera;

    public Text errorLabel;
    public Animator errorLabelAnimator;

    public Text creatureLabelName;
    public Text creatureLabelPerk;
    public Text creatureLabelPerk1;
    public Text creatureLabelPerk2;
    public Text creatureLabelHint1;
    public Text creatureLabelHint2;

    public Animator creatureLabelAnimator;
}
