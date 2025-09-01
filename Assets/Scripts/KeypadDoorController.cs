using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeypadDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator doorAnimator;        // Drag your door Animator here
    public string openTrigger = "DoorOpen";
    public string closeTrigger = "DoorClose";

    [Header("Keypad Settings")]
    public string correctCode = "1234";  // Set your door code here
    public GameObject keypadUI;          // Assign your keypad UI panel
    public TMP_Text displayText;         // TMP text to show entered numbers

    private string enteredCode = "";
    private bool isPlayerNear = false;
    private bool isDoorOpen = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            ShowKeypad(true);
        }
    }

    public void ButtonPressed(string number)
    {
        enteredCode += number;
        displayText.text = enteredCode;
    }

    public void ClearCode()
    {
        enteredCode = "";
        displayText.text = "";
    }

    public void EnterCode()
    {
        if (enteredCode == correctCode)
        {
            if (!isDoorOpen)
            {
                doorAnimator.SetTrigger(openTrigger);
                isDoorOpen = true;
            }
            else
            {
                doorAnimator.SetTrigger(closeTrigger);
                isDoorOpen = false;
            }

            ShowKeypad(false);
        }
        else
        {
            Debug.Log("Incorrect Code");
            ClearCode();
        }
    }

    public void ShowKeypad(bool state)
    {
        keypadUI.SetActive(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        if (!state) ClearCode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            ShowKeypad(false);
        }
    }
}