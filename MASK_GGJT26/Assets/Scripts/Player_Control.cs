using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class Player_Control : MonoBehaviour
{// script en EMPTY MASK_MAN
 // SINGLETON script
    public static Player_Control instance;
    public Mask_Manager _MM; //pillo SINGLE del MM
    public Loot_Control _LC; //pillo SINGLE del LC

    #region /// MOVIMIENTO ///
    Camera _camera;
    public LayerMask floorLayer;
    public float floorHeight;
    public float floorDistance;
    #endregion

    #region /// INTERACCIONES ///
    public float interactDistance;
    public GameObject shipLobby;
    #endregion

    #region /// BATERY CHARGE ///
    public Slider baterySlider;
    public float maxEnergy;
    public float currentEnergy;
    public float drainRate;
    public float chargeRate;
    public float humanMaskCharge;
    public GameObject containHideout;
    public bool isHidout;
    #endregion

    void Awake()// singleton sin superponer
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }

    void Start()
    {
        _MM = Mask_Manager.instance;
        _LC = Loot_Control.instance;
        _camera = Camera.main;
        isHidout = false;
        currentEnergy = maxEnergy;
        baterySlider.maxValue = maxEnergy;
        baterySlider.value = currentEnergy;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 0.2f);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.CompareTag("exitdoor"))
                {
                   shipLobby.SetActive(true);
                }

                if (hit.collider.CompareTag("container"))
                {
                    containHideout.SetActive(true);
                    isHidout = true;
                }

                if (hit.collider.CompareTag("collectable"))
                {
                    Loot_Item loot = hit.collider.GetComponent<Loot_Item>();
                    if (loot != null)
                    {
                        Loot_Control.instance.CollectItem(loot.index, hit.collider.gameObject);
                    }
                }
            }
        }

        float energyDelta = 0f;
        // Drenaje base (siempre)
        energyDelta -= drainRate;
        // Recarga por container
        if (isHidout)
            energyDelta += chargeRate;
        // Recarga por máscara humana
        if (_MM.maskType == Mask_Manager.MaskType.HumanMask)
            energyDelta += chargeRate * humanMaskCharge;
        // Aplicar
        currentEnergy += energyDelta * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        baterySlider.value = currentEnergy;

        if (Input.GetKeyDown(KeyCode.W))
        {
            Vector3 targetPosition = transform.position + transform.forward * floorDistance;
            Vector3 rayOrigin = targetPosition + Vector3.up * floorHeight;
            Ray groundRay = new Ray(rayOrigin, Vector3.down);
            Debug.DrawRay(rayOrigin, Vector3.down * floorDistance, Color.blue, 0.5f);
            if (Physics.Raycast(groundRay, floorDistance, floorLayer))
            { transform.position = targetPosition; }
            else Debug.Log("NO HAY SUELO");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Vector3 targetPosition = transform.position + transform.forward * -floorDistance;
            Vector3 rayOrigin = targetPosition + Vector3.up * floorHeight;
            Ray groundRay = new Ray(rayOrigin, Vector3.down);
            Debug.DrawRay(rayOrigin, Vector3.down * floorDistance, Color.blue, 0.5f);
            if (Physics.Raycast(groundRay, floorDistance, floorLayer))
            { transform.position = targetPosition; }
            else Debug.Log("NO HAY SUELO");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Rotate (0f, 90f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.Rotate(0f, -90f, 0f);
        }
    }

    public void ExitContainer()
    {
        containHideout.SetActive(false);
        isHidout = false;
    }

    public void StartGame()
    {
        shipLobby.SetActive(false);
    }

    public void GenerateList()
    {
        print("Phone Picked");
    }
}
