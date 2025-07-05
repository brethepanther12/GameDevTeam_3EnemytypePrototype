using UnityEngine;

public class cameraController : MonoBehaviour
{
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;

    float rotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
<<<<<<< Updated upstream
=======
        
>>>>>>> Stashed changes
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< Updated upstream
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;
=======
        

        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;

        if (invertY)
        {
            rotX += mouseY;
        } else
        {
            rotX -= mouseY;
        }
            
>>>>>>> Stashed changes

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
<<<<<<< Updated upstream
=======

    
>>>>>>> Stashed changes
}
