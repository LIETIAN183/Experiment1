using UnityEngine;
using UnityEngine.UI;

namespace Project.Deprecated
{
    public class Exit : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            this.GetComponent<Button>().onClick.AddListener(() => Application.Quit());
        }

    }
}