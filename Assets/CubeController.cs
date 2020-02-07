using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{


    private DisruptiveApi.ApiClient api = new DisruptiveApi.ApiClient();
    // Use this for initialization
    void Start()
    {
        StartCoroutine(api.GetSensors(Callback));


    }
   

    private void Callback(List<Sensor> response)
    {
        GetComponent<Dropdown>().options.Add(new Dropdown.OptionData() { text = "HEJ" });
        print(response.Count);
        ContainerController.configs[""] = new ContainerController.Config();
    }
}
