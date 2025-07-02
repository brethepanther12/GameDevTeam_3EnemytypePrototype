using UnityEngine;
using System.Collections;

public class Temp : MonoBehaviour
{
    public [SerializeField] int tempvar;
    public int five = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //This is just a temporary push to test WorkHooks
        tempvar = five;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
