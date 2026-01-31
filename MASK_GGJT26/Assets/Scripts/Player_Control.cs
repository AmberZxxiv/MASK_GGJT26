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

    #region /// DETECTION ///
    public Slider detectionSlider;
    public float susMax;
    public float susLevel;
    public float susRate;
    public float drainDetection;
    public float detectDistance;
    public LayerMask alienLayer;
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
        detectionSlider.maxValue = susMax;
        detectionSlider.value = susLevel;
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

        DetectAliens();
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

    public bool IsMaskedCorrectly(Alien_Controler.AlienType alienType)
    {
        switch (alienType)
        {
            case Alien_Controler.AlienType.PichoAlien:
                return _MM.maskType == Mask_Manager.MaskType.PichoMask;
            case Alien_Controler.AlienType.EyeAlien:
                return _MM.maskType == Mask_Manager.MaskType.EyeMask;
            case Alien_Controler.AlienType.SullyAlien:
                return _MM.maskType == Mask_Manager.MaskType.SullyMask;
            default:
                return false;
        }
    }
    void DetectAliens()
    {
        bool detected = false;

        Vector3 boxCenter = transform.position + transform.forward * (detectDistance / 2) + Vector3.up * 1f;
        Vector3 boxHalfExtents = new Vector3(4f, 2f, detectDistance / 2); // ancho=4, alto=2, largo=detectDistance
        Quaternion boxRotation = transform.rotation;

        // DEBUG: dibujar la caja
        DebugDrawBox(boxCenter, boxHalfExtents, boxRotation, Color.green);

        Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, boxRotation, alienLayer);
        foreach (Collider hit in hits)
        {
            Alien_Controler alien = hit.GetComponent<Alien_Controler>();
            if (alien != null)
            {
                Debug.Log("Alien detectado: " + alien.name);
                if (!IsMaskedCorrectly(alien.alienType))
                {
                    susLevel += susRate * Time.deltaTime;
                    detected = true;
                }
            }
        }

        // Decay de sospecha
        if (!detected && susLevel > 0f)
            susLevel -= drainDetection * Time.deltaTime;

        susLevel = Mathf.Clamp(susLevel, 0, susMax);
        detectionSlider.value = susLevel;

        if (susLevel >= susMax) AlertAliens();
    }


    void AlertAliens()
    {
        Debug.Log("¡Has sido descubierto por los aliens!");
        // Aquí puedes disparar eventos globales de alarma o cambiar comportamiento de enemigos
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

    void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
    {
        Vector3 c = center;
        Vector3 he = halfExtents;

        Vector3[] points = new Vector3[8];
        points[0] = c + rotation * new Vector3(-he.x, -he.y, -he.z);
        points[1] = c + rotation * new Vector3(he.x, -he.y, -he.z);
        points[2] = c + rotation * new Vector3(he.x, -he.y, he.z);
        points[3] = c + rotation * new Vector3(-he.x, -he.y, he.z);

        points[4] = c + rotation * new Vector3(-he.x, he.y, -he.z);
        points[5] = c + rotation * new Vector3(he.x, he.y, -he.z);
        points[6] = c + rotation * new Vector3(he.x, he.y, he.z);
        points[7] = c + rotation * new Vector3(-he.x, he.y, he.z);

        // Dibujar líneas de abajo
        Debug.DrawLine(points[0], points[1], color);
        Debug.DrawLine(points[1], points[2], color);
        Debug.DrawLine(points[2], points[3], color);
        Debug.DrawLine(points[3], points[0], color);

        // Dibujar líneas de arriba
        Debug.DrawLine(points[4], points[5], color);
        Debug.DrawLine(points[5], points[6], color);
        Debug.DrawLine(points[6], points[7], color);
        Debug.DrawLine(points[7], points[4], color);

        // Conectar arriba y abajo
        for (int i = 0; i < 4; i++)
            Debug.DrawLine(points[i], points[i + 4], color);
    }
}
