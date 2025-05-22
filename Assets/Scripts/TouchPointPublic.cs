using UnityEngine;

public class TouchPointPublic : MonoBehaviour
{
    public TouchPointInteraction TouchPointInteraction;


    public void FinishTouch()
    {
        TouchPointInteraction.FinishTouch();
    }
}
