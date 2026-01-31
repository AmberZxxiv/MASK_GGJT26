using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;


public class Mask_Manager : MonoBehaviour
{// script en EMPTY MASK_MAN
 // SINGLETON script
    public static Mask_Manager instance;
    public Player_Control _PC; //pillo SINGLE del PC

    public MaskType maskType;
    public MaskState maskState;
    public enum MaskType
    {
        HumanMask,
        PichoMask,
        EyeMask,
        SullyMask
    }
    public enum MaskState
    {
        Strong,
        Low,
        Off
    }

    #region /// SELECTOR ///
    public Image maskSelector;
    public int totalMasks;
    int _currentMask;
    float _angleMask;
    #endregion

    #region /// POSTPOS ///
    public PostProcessVolume postPoPicho;
    public PostProcessVolume postPoEye;
    public PostProcessVolume postPoSully;
    #endregion



    [Header("Mask Energy State")]
    [Range(0f, 1f)] public float lowThreshold = 0.5f;
    [Range(0f, 1f)] public float offThreshold = 0.05f;

    void Awake()// singleton sin superponer
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }

    void Start()
    {
        _PC = Player_Control.instance;
        totalMasks = System.Enum.GetValues(typeof(MaskType)).Length;
        _angleMask = 360f / totalMasks;
    }
    float GetBatteryPercent()
    {
        return _PC.currentEnergy / _PC.maxEnergy;
    }

    void Update()
    {
        HandleScroll();
        UpdateMaskState();
        UpdatePostProcessing();
    }
    void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        _currentMask = (_currentMask + 1) % totalMasks;
        else if (scroll < 0f)
        _currentMask = (_currentMask - 1 + totalMasks) % totalMasks;
        else return;

        SwitchMask();
    }
    public void SwitchMask()
    {
        float targetAngle = _currentMask * _angleMask;
        maskSelector.rectTransform.localEulerAngles = new Vector3(0f, 0f, targetAngle);
        maskType = (MaskType)_currentMask;
    }
    void UpdateMaskState()
    {
        float percent = GetBatteryPercent();

        if (percent <= offThreshold)
        maskState = MaskState.Off;
        else if (percent <= lowThreshold)
        maskState = MaskState.Low;
        else
        maskState = MaskState.Strong;
    }
    float GetPostProcessWeight()
    {
        float percent = GetBatteryPercent();

        if (percent <= offThreshold)
            return 0f;

        if (percent <= lowThreshold)
            return Mathf.InverseLerp(offThreshold, lowThreshold, percent);

        return 1f;
    }
    void UpdatePostProcessing()
    {
        float weight = GetPostProcessWeight();

        postPoPicho.weight = 0f;
        postPoEye.weight = 0f;
        postPoSully.weight = 0f;

        if (maskState == MaskState.Off)
            return;

        switch (maskType)
        {
            case MaskType.PichoMask:
                postPoPicho.weight = weight;
                break;
            case MaskType.EyeMask:
                postPoEye.weight = weight;
                break;
            case MaskType.SullyMask:
                postPoSully.weight = weight;
                break;
        }
    }
}
