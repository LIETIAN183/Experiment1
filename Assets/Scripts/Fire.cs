using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public Transform prefab;
    public int force = 1000;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var n = Instantiate(prefab, transform.position, transform.rotation);
            n.GetComponent<Rigidbody>().AddForce(mouseRay.direction * force);
        }
    }
}
