using UnityEngine;
using static Mask_Manager;

public class Alien_Controler : MonoBehaviour
{
    public AlienType alienType;
    public enum AlienType
    {
        PichoAlien,
        EyeAlien,
        SullyAlien
    }
    public Mask_Manager _MM; //pillo SINGLE del MM
    
    #region /// RUTAS ///
    public Transform[] route;
    public float speed;
    public float minDistance;
    int _currentPoint = 0;
    #endregion

    #region /// DIRECCION ///
    int _direction = 1;
    private float yOffset;
    private SpriteRenderer spriteRenderer;
    Animator _animator;
    #endregion

    void Start()
    {
        _animator = GetComponent<Animator>();
        _MM = Mask_Manager.instance;
        spriteRenderer = GetComponent<SpriteRenderer>();

        switch (alienType)
        {
            case AlienType.PichoAlien: yOffset = 2.5f; break;
            case AlienType.EyeAlien: yOffset = 1.5f; break;
            case AlienType.SullyAlien: yOffset = 2.4f; break;
        }

        if (route.Length > 0)
        {
            Vector3 pos = route[0].position;
            pos.y += yOffset;
            transform.position = pos;
        }
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (route.Length == 0) return;

        Transform target = route[_currentPoint];
        Vector3 targetPos = target.position;
        targetPos.y += yOffset; // ajustamos la altura para los pies

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Comprobamos si llegamos al punto
        if (Vector3.Distance(transform.position, targetPos) <= minDistance)
        {
            // Sumar dirección (hacia adelante o hacia atrás)
            _currentPoint += _direction;

            // Cambiar dirección si llegamos al principio o al final
            if (_currentPoint >= route.Length)
            {
                _currentPoint = route.Length - 2; // Retrocedemos al penúltimo
                _direction = -1; // Cambiar dirección a retroceder
            }
            else if (_currentPoint < 0)
            {
                _currentPoint = 1; // Avanzamos al segundo
                _direction = 1; // Cambiar dirección a avanzar
            }

            // Cambiar el parámetro "IsMovingBackward" según la dirección
            _animator.SetBool("Espaldas", _direction < 0);  // Si es negativo, es retrocediendo
        }
    }
}
