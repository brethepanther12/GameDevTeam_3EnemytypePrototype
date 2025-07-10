using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReticleController : MonoBehaviour
{

    [SerializeField] float pulseScale = 1.2f;
    [SerializeField] float pulseDuration = 0.1f;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color enemyColor = Color.red;

    private RectTransform rt;
    private Image img;
    private Vector3 origScale;
    private Coroutine pulseRoutine;

  
    void Start()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        origScale = rt.localScale;
        img.color = normalColor;
        
    }

  public void Pulse(bool isHit)
    {
        if(pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }
        pulseRoutine = StartCoroutine(PulseEffect(isHit));
    }

    private IEnumerator PulseEffect(bool isHit)
    {
        float scale = isHit ? pulseScale * 1.5f : pulseScale;
        rt.localScale = origScale * scale;
        yield return new WaitForSeconds(pulseDuration);
        rt.localScale = origScale;
    }

    public void SetEnemyAim(bool isAimingAtEnemy)
    {
        img.color = isAimingAtEnemy ? enemyColor : normalColor;
    }
}
