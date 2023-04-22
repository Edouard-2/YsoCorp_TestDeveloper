using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartFromCredit : MonoBehaviour
{
    public void Restart()
    {
        SystemManager.Instance?.RestartGame();
    }
}
