using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject postPoPicho;
    public GameObject postPoEye;
    public GameObject postPoSully;
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
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            _currentMask = (_currentMask + 1) % totalMasks;
            SwitchMask();
        }
        else if (scroll < 0f)
        {
            _currentMask--;
            if (_currentMask < 0) _currentMask = totalMasks - 1;
            SwitchMask();
        }
        UpdateMaskState();
    }
    public void SwitchMask()
    {
        float targetAngle = _currentMask * _angleMask;
        maskSelector.rectTransform.localEulerAngles = new Vector3(0f, 0f, targetAngle);
        EquipMask(_currentMask);
    }
    void EquipMask(int index)
    {
        maskType = (MaskType)index;
        Debug.Log("Máscara equipada: " + maskType);
        EquipPostPos();
    }
    void UpdateMaskState()
    {
        float percent = GetBatteryPercent();

        MaskState newState;

        if (percent <= offThreshold)
            newState = MaskState.Off;
        else if (percent <= lowThreshold)
            newState = MaskState.Low;
        else
            newState = MaskState.Strong;

        if (newState != maskState)
        {
            maskState = newState;
            OnMaskStateChanged();
        }
    }
    void OnMaskStateChanged()
    {
        Debug.Log($"Mask {maskType} ahora está en estado {maskState}");

        switch (maskState)
        {
            case MaskState.Strong:
                // comportamiento normal
                break;

            case MaskState.Low:
                // efectos reducidos
                break;

            case MaskState.Off:
                // máscara inútil / penalizaciones
                break;
        }
    }
    void EquipPostPos()
    {
        postPoPicho.SetActive(false);
        postPoEye.SetActive(false);
        postPoSully.SetActive(false);
        if (maskState == MaskState.Off) return;
        switch (maskType)
        {
            case MaskType.HumanMask:
                postPoPicho.SetActive(false);
                postPoEye.SetActive(false);
                postPoSully.SetActive(false);
                break;
            case MaskType.PichoMask:
                postPoPicho.SetActive(true);
                break;
            case MaskType.EyeMask:
                postPoEye.SetActive(true);
                break;
            case MaskType.SullyMask:
                postPoSully.SetActive(true);
                break;
        }
    }
}
