using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    [Serializable]
    public class SensorsResponse
    {
        public List<Sensor> things;
    }

    [Serializable]
    public class Sensor
    {
        public string id;
        public string name;
        public State state;
        public float temperature { get { return state.properties.temperature; } }
        public bool hasTemperature
        {
			get { 
				if (state == null)
					return false;
				if (state.properties == null)
					return false;
				return state.properties.temperature != 0;
			}
        }

        [Serializable]
        public class State
        {
            public Properties properties;
        }

        [Serializable]
        public class Properties
        {
            public float temperature;
        }
    }
}
