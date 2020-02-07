using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour {

	private DisruptiveApi.ApiClient api = new DisruptiveApi.ApiClient();

    public static Dictionary<string, Config> configs = new Dictionary<string, Config>();

	public ParticleSystem ice;
	public ParticleSystem explosion;
	public TextMesh temperatureLabel;
    public PathFinder pathFinder;

	public TextMesh roomTempFarLeft;
	public TextMesh roomTempFarRight;
	public TextMesh roomTempNearLeft;
	public TextMesh roomTempNearRight;

    private string targetSensor1 = "";
    private string targetSensor2 = "";
    public string targetSensorId;
	public Light roomLightFarLeft;
	public Light roomLightFarRight;
	public Light roomLightNearLeft;
	public Light roomLightNearRight;

	public Color warmColor;
	public Color coldColor;

    public float targetTemperature = 22;
	public float maxTemperature = 40;
	public float minTemperature = 0;

	public string tempSuffix = "°c";
	public string sensorId = "";

	public float smoothing = 1f;
	public Vector3 target;

	public float maxX = 4.5f;
	public float maxZ = 9.5f;

    private RoomSensor roomSensor1 = new RoomSensor()
    {
        name = "FarLeft",    
        id = "",
        temperature = 30f,
        pos = new Vector3(-4.5f, 1, -9.5f)
    };

    private RoomSensor roomSensor2 = new RoomSensor()
    {
        name = "NearLeft",
        id = "",
        temperature = 5f,
        pos = new Vector3(4.5f, 1, -9.5f)
    };

    private RoomSensor roomSensor3 = new RoomSensor()
    {
        name = "FarRight",
        id = "",
        temperature = 18f,
        pos = new Vector3(4.5f, 1, 9.5f)
    };

    private RoomSensor roomSensor4 = new RoomSensor()
    {
        name = "NearRight",
        id = "",
        temperature = 27f,
        pos = new Vector3(-4.5f, 1, 9.5f)
    };

    void Start()
    {

        var config = configs[name];
        targetTemperature = config.targetTemp;
        minTemperature = config.minTemp;
        maxTemperature = config.maxTemp;
        targetSensorId = config.sensorId;

        roomSensor1.SetConfig(configs[Config.FarLeftRoomSensorName]);
        roomSensor2.SetConfig(configs[Config.NearLeftRoomSensorName]);
        roomSensor3.SetConfig(configs[Config.FarRightRoomSensorName]);
        roomSensor4.SetConfig(configs[Config.NearRightRoomSensorName]);

        explosion.Stop ();
		ice.Stop ();
		SetupRoomSensorLabels();
		SetupLightColors ();
        StartCoroutine(pollBrewSensor());
        StartCoroutine(pollRoomSensor(roomSensor1));
        StartCoroutine(pollRoomSensor(roomSensor2));
        StartCoroutine(pollRoomSensor(roomSensor3));
        StartCoroutine(pollRoomSensor(roomSensor4));
        StartCoroutine(MovementCoroutine());
        StartCoroutine(pollTargetSensor());        
    }

    IEnumerator pollTargetSensor()
    {
        while (true)
        {
            StartCoroutine(api.GetSensors(targetSensorId, (temp) =>
            {
                targetTemperature = temp;
            }));
            yield return new WaitForSeconds(3f);
        }
    }

	void SetupRoomSensorLabels()
	{
		roomSensor1.tempLabel = roomTempNearRight;
		roomSensor1.tempLight = roomLightNearRight;
		roomSensor2.tempLabel = roomTempFarRight;
		roomSensor2.tempLight = roomLightFarRight;
		roomSensor3.tempLabel = roomTempFarLeft;
		roomSensor3.tempLight = roomLightFarLeft;
		roomSensor4.tempLabel = roomTempNearLeft;
		roomSensor4.tempLight = roomLightNearLeft;
	}

	void SetupLightColors()
	{
		warmColor = new Color ();
		warmColor.r= 1f;
		warmColor.g= 0f;
		warmColor.b= 0f;
		warmColor.a= 1f;

		coldColor = new Color ();
		coldColor.r = 0f;
		coldColor.g = 0f;
		coldColor.b = 1f;
		coldColor.a = 1f;
	}

    IEnumerator MovementCoroutine ()
	{
        while (target == null)
        {            
            yield return new WaitForSeconds(1f);
        }
                
        while (true)
        {
            var endTarget = pathFinder.AllocateEnd(sensorId, target);
            print("NEW ET: " + JsonUtility.ToJson(endTarget));
            while (Vector3.Distance(transform.position, endTarget) < 0.02f)
            {
                endTarget = pathFinder.AllocateEnd(sensorId, target);
                yield return new WaitForSeconds(2f);
            }
            var nextTarget = pathFinder.GetNextGrid(sensorId, transform.position, endTarget);
            while (Vector3.Distance(transform.position, nextTarget) > 0.02f)
            {
                transform.position = Vector3.Lerp(transform.position, nextTarget, smoothing * Time.deltaTime);

                yield return null;                
            }
            
        }

		print("MovementCoroutine is now finished.");
	}

    void ResponseHandler(float temperature)
    {
        target = GetTarget(temperature);
		temperatureLabel.text = (temperature + tempSuffix);
		if (temperature > maxTemperature) {
			explosion.Play();
		} else if (explosion.isEmitting) {
			explosion.Stop ();
		}
		if (temperature < minTemperature) {
			ice.Play();
		} else if (ice.isEmitting) {
			ice.Stop ();
		}
    }

    private IEnumerator pollBrewSensor()
    {
        while (true)
        {
            StartCoroutine(api.GetSensors(sensorId, ResponseHandler));
            yield return new WaitForSeconds(5f);
        }
    }


    private IEnumerator pollRoomSensor(RoomSensor sensor)
    {
        while (true)
        {
            StartCoroutine(api.GetSensors(sensor.id, (temp) =>
            {
                sensor.temperature = temp;
					sensor.tempLabel.text = (temp + tempSuffix);
					if (temp > 15) {
						sensor.tempLight.color = warmColor;
					} else {
						sensor.tempLight.color = coldColor;
					}

            }));
            yield return new WaitForSeconds(3f);
        }
    }

    private Vector3 GetTarget(float inputTemperature)
    {
        var targetTemperature = CreateTargetRoomTemperature(inputTemperature);
        Vector3 one = GetPositionFromTemperature(roomSensor1, roomSensor3, targetTemperature);
        Vector3 two = GetPositionFromTemperature(roomSensor2, roomSensor4, targetTemperature);
        var distance = Vector3.Distance(one, two);
        Vector3 midPoint = Vector3.MoveTowards(one, two, distance / 2);
        return midPoint;
    }

    private float CreateTargetRoomTemperature(float inputTemperature)
    {

        var diff = targetTemperature - inputTemperature;

        if (inputTemperature > targetTemperature)
        {
            return targetTemperature - diff / 2 < targetTemperature ? targetTemperature : targetTemperature - 2;
        } else
        {
            return targetTemperature + diff / 2 > targetTemperature ? targetTemperature : targetTemperature + 2;
        }
    }

    private Vector3 GetPositionFromTemperature(RoomSensor s1, RoomSensor s2, float temperature)
    {
        var sensor1Pos = s1.pos;
        var sensor2Pos = s2.pos;
        var sensor1temp = s1.temperature;
        var sensor2temp = s2.temperature;

        var sensorDistance = Vector3.Distance(sensor1Pos, sensor2Pos);
        var distanceFromSensor1 = sensorDistance * (temperature - sensor1temp) / (sensor2temp - sensor1temp);

        var target = Vector3.MoveTowards(sensor1Pos, sensor2Pos, distanceFromSensor1);

        return target;

    }

    private Vector3 createmidPoint(Vector3 one, Vector3 two)
    {
        return Vector3.MoveTowards(one, two, Vector3.Distance(one, two));
    }

    public class RoomSensor
    {
        public string name;
        public string id;
        public float temperature;
        public Vector3 pos;
		public TextMesh tempLabel;
		public Light tempLight;

        public void SetConfig(Config config)
        {
            id = config.sensorId;
            temperature = config.currentTemp;
        }
    }

    public class Config
    {
        public static string FarLeftRoomSensorName = "FarLeft";
        public static string FarRightRoomSensorName = "FarRight";
        public static string NearLeftRoomSensorName = "NearLeft";
        public static string NearRightRoomSensorName = "NearRight";

        public static string Container1 = "Container BrewL";
        public static string Container2 = "Container BrewR";

        public string sensorId;
        public float targetTemp;
        public float currentTemp;
        public float maxTemp;
        public float minTemp;
    }
}
