using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    Vector3 camOriginalPos;
    Quaternion camOriginalRot;
    Transform currentContainerPoint;
    public Transform[] spawnMaps;
    public GameObject shipLobby;
    public GameObject pauseMenu;
    public GameObject victoryMenu;
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
    public GameObject alertPanel;
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
        Time.timeScale = 0f;
        shipLobby.SetActive(true);
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 0.2f);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.CompareTag("exitdoor"))
                {
                    if (_LC.allCollected == true)
                    {
                        Time.timeScale = 0f;
                        victoryMenu.SetActive(true);
                    }
                    else BackToShip();
                }

                if (hit.collider.CompareTag("container"))
                {
                    EnterContainer(hit.collider.transform);
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
            if (isHidout) return;
            Vector3 targetPosition = transform.position + transform.forward * floorDistance;
            Vector3 rayOrigin = targetPosition + Vector3.up * floorHeight;
            Ray groundRay = new Ray(rayOrigin, Vector3.down);
            Debug.DrawRay(rayOrigin, Vector3.down * floorDistance, Color.blue, 0.5f);
            if (Physics.Raycast(groundRay, floorDistance, floorLayer))
            { transform.position = targetPosition; }
            currentEnergy -= 5f;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isHidout) return;
            Vector3 targetPosition = transform.position + transform.forward * -floorDistance;
            Vector3 rayOrigin = targetPosition + Vector3.up * floorHeight;
            Ray groundRay = new Ray(rayOrigin, Vector3.down);
            Debug.DrawRay(rayOrigin, Vector3.down * floorDistance, Color.blue, 0.5f);
            if (Physics.Raycast(groundRay, floorDistance, floorLayer))
            { transform.position = targetPosition; }
            currentEnergy -= 5f;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (isHidout) return;
            transform.Rotate (0f, 90f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (isHidout) return;
            transform.Rotate(0f, -90f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            { QuitPause(); }
            else
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
    void EnterContainer(Transform container)
    {
        if (isHidout) return;
        Transform camPoint = container.Find("HideCam");
        camOriginalPos = _camera.transform.position;
        camOriginalRot = _camera.transform.rotation;

        _camera.transform.SetPositionAndRotation
        ( camPoint.position,camPoint.rotation);

        currentContainerPoint = camPoint;
        containHideout.SetActive(true);
        isHidout = true;
    }
    public void ExitContainer()
    {
        if (!isHidout) return;

        _camera.transform.position = camOriginalPos;
        _camera.transform.rotation = camOriginalRot;

        containHideout.SetActive(false);
        isHidout = false;
        currentContainerPoint = null;
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
            default: return false;
        }
    }
    void DetectAliens()
    {
        if (isHidout) return;

        bool detected = false;

        detected |= DetectBox(
        transform.forward,new Vector3(4f, 2f, detectDistance / 2),Color.red );

        detected |= DetectBox(
        -transform.forward,new Vector3(4f, 2f, detectDistance / 2),Color.red);

        detected |= DetectBox(
        transform.right, new Vector3(2f, 2f, 2f),Color.red);

        detected |= DetectBox(
        -transform.right,new Vector3(2f, 2f, 2f),Color.red);

        // Decay sospecha
        if (!detected && susLevel > 0f)
        susLevel -= drainDetection * Time.deltaTime;

        susLevel = Mathf.Clamp(susLevel, 0, susMax);
        detectionSlider.value = susLevel;

        if (susLevel >= susMax) AlertAliens();
    }
    bool DetectBox(Vector3 direction, Vector3 halfExtents, Color debugColor)
    {
        bool detected = false;

        Vector3 center =
        transform.position + direction * halfExtents.z +  Vector3.up * 1f;

        Quaternion rotation = transform.rotation;

        DebugDrawBox(center, halfExtents, rotation, debugColor);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, alienLayer);

        foreach (Collider hit in hits)
        {
            Alien_Controler alien = hit.GetComponent<Alien_Controler>();
            if (alien != null && !IsMaskedCorrectly(alien.alienType))
            {
                susLevel += susRate * Time.deltaTime;
                detected = true;
            }
        }
        return detected;
    }
    void AlertAliens()
    {
        Time.timeScale = 0f;
        alertPanel.SetActive(true);
    }

    public void BackToShip()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void StartGame()
    {
        if (_LC.listed == true)
        {
            if (spawnMaps.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnMaps.Length);
                Transform spawn = spawnMaps[randomIndex];
                transform.position = spawn.position + Vector3.up * 1.25f;
                transform.rotation = spawn.rotation;
            }

            Time.timeScale = 1f;
            shipLobby.SetActive(false);
        }
    }
    public void QuitPause()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }
    public void GenerateList()      
    { _LC.SelectRandomItems(); }

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
