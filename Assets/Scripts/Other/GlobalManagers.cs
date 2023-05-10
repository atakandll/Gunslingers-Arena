using UnityEngine;

public class GlobalManagers : MonoBehaviour
{
    public static GlobalManagers instance { get; private set; }

    [SerializeField] private GameObject parentObj;
    [field: SerializeField] public NetworkRunnerController networkRunnerController { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(parentObj);
        }
    }
}