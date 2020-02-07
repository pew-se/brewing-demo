using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

	private DisruptiveApi.ApiClient api = new DisruptiveApi.ApiClient();

	public Dropdown roomFarLeft;
	public Dropdown roomFarRight;
	public Dropdown roomNearLeft;
	public Dropdown roomNearRight;

	public Dropdown containerOne;
	public Dropdown containerTwo;

	public Text c1Target;
	public Text c1Min;
	public Text c1Max;

	public Text c2Target;
	public Text c2Min;
	public Text c2Max;

	public Button startButton;
	public Button exitButton;

	void Start () {
		StartCoroutine(api.GetSensors(GetSensors));
		startButton.onClick.AddListener (StartButton);
		exitButton.onClick.AddListener (()=>{Application.Quit();});
	}

	private void GetSensors(List<Sensor> sensors)
	{
		roomFarLeft.options.Clear();
		foreach (Sensor sensor in sensors) {
			string sensorLabel = (sensor.id + " (" + sensor.temperature + "°c) '" + sensor.name + "'");

			// Populate room sensor dropdowns
			roomFarLeft.options.Add (new Dropdown.OptionData(){ text=sensorLabel });
			roomFarRight.options.Add (new Dropdown.OptionData(){ text=sensorLabel });
			roomNearLeft.options.Add (new Dropdown.OptionData(){ text=sensorLabel });
			roomNearRight.options.Add (new Dropdown.OptionData(){ text=sensorLabel });

			// Populate container sensor dropdowns
			containerOne.options.Add (new Dropdown.OptionData(){ text=sensorLabel });
			containerTwo.options.Add (new Dropdown.OptionData(){ text=sensorLabel });
		}


		print (sensors.Count);
	}

	private void StartButton() {
		print ("hej");
		print(roomFarLeft.options [roomFarLeft.value].text);

		var farLeft = ParseText(roomFarLeft.options [roomFarLeft.value].text);
		ContainerController.configs [ContainerController.Config.FarLeftRoomSensorName] = farLeft;

		var farRight = ParseText(roomFarRight.options [roomFarRight.value].text);
		ContainerController.configs [ContainerController.Config.FarRightRoomSensorName] = farRight;

		var nearLeft = ParseText(roomNearLeft.options [roomNearLeft.value].text);
		ContainerController.configs [ContainerController.Config.NearLeftRoomSensorName] = nearLeft;

		var nearRight = ParseText(roomNearRight.options [roomNearRight.value].text);
		ContainerController.configs [ContainerController.Config.NearRightRoomSensorName] = nearRight;


		var containerOne1 = ParseText(containerOne.options [containerOne.value].text);
		ContainerController.configs [ContainerController.Config.Container1] = containerOne1;
		containerOne1.targetTemp = float.Parse(c1Target.text);
		containerOne1.minTemp = float.Parse(c1Min.text);
		containerOne1.maxTemp = float.Parse(c1Max.text);

		var containerTwo2 = ParseText(containerTwo.options [containerTwo.value].text);
		ContainerController.configs [ContainerController.Config.Container2] = containerTwo2;
		containerTwo2.targetTemp = float.Parse(c2Target.text);
		containerTwo2.minTemp = float.Parse(c2Min.text);
		containerTwo2.maxTemp = float.Parse(c2Max.text);
		SceneManager.LoadScene ("MainScene");
	}

	private ContainerController.Config ParseText(string text)
	{
		int endOfId = text.IndexOf (" (");
		int endOfTemp = text.IndexOf("°c");

		string id = text.Substring (0, endOfId);
		string temp = text.Substring (endOfId+2, endOfTemp-endOfId-2);

		print (id);
		print (temp);

		return new ContainerController.Config(){
			sensorId=id,
			currentTemp=float.Parse(temp)
		};
	}
}
