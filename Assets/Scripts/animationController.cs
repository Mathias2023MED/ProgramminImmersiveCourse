using UnityEngine;
using UnityEngine.InputSystem;

public class animationController : MonoBehaviour
{

    public InputActionProperty triggerAction;
    public InputActionProperty grabAction;

    public Animator myAnimator;
    // Update is called once per frame
    void Update()
    {
        float grabValue = grabAction.action.ReadValue<float>();
        myAnimator.SetFloat("Grip", grabValue);

        float triggerValue = triggerAction.action.ReadValue<float>();
        myAnimator.SetFloat("Trigger", triggerValue);
    }
}
