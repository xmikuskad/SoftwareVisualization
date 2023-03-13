using UnityEngine;

public class SingletonManager : MonoBehaviour
{
    private static SingletonManager _instance;

    public static SingletonManager Instance { get { return _instance; } }
    public DataManager dataManager { get; private set; }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        dataManager = GetComponentInChildren<DataManager>();
    }
}
