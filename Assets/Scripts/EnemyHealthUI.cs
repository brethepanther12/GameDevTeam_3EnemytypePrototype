using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{

    [SerializeField] GameObject enemyHealthBar;
    [SerializeField] Vector3 offset = new Vector3(0, 2f, 0);

    private Transform cam;
    private Image healthFill;
    private GameObject hbInstance;


    void Start()
    {
        cam = Camera.main.transform;
        hbInstance = Instantiate(enemyHealthBar, transform.position + offset, Quaternion.identity);
        hbInstance.transform.SetParent(null);
        healthFill = hbInstance.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        
    }

    void LateUpdate()
    {
        if(hbInstance != null)
        {
            hbInstance.transform.position = transform.position + offset;
            hbInstance.transform.LookAt(cam);
            hbInstance.transform.Rotate(0, 180, 0);
        }
    }

    public void UpdateHealthBar(float current, float max)
    {
        if(healthFill != null)
        {
            healthFill.fillAmount = current / max;
        }
    }

    private void OnDestroy()
    {
        if(hbInstance!= null)
        {
            Destroy(hbInstance);
        }
    }
}
