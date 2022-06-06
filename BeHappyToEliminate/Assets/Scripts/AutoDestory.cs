using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestory : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}