#if UNITY_EDITOR
using UnityEngine;

public class FindLocalID : MonoBehaviour
{
    void Awake()
    {
        Debug.Log(FileIDUtil.Compute(typeof(SobelRain)));
    }
}

#endif