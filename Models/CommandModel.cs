using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ajax_Minimal.Models
{
    /// <summary>
    /// CommandsModel : connects as a client to the flight simulator, 
    /// and asks for data when it required.
    /// </summary>
    public class CommandsModel 
    {
        private bool stop = true;
        private int commands = 0;
        // flagFinish : false as long as model reads data from simulator, 
        // true when its done. flag that tells the controller it can take the data.
        private bool flagFinish;
        private string ip = "127.0.0.1";
        private int port = 5402;
        private string getLat = "get /position/latitude-deg\r\n";
        private string getLon = "get /position/longitude-deg\r\n";
        private string getAlt = "get /instrumentation/altimeter/indicated-altitude-ft\r\n";
        private string getSpeed = "get /instrumentation/airspeed-indicator/indicated-speed-kt\r\n";
       
        /// <summary>
        /// Longtitude of palne
        /// </summary>
        public double Longtitude
        {
            get; set;
        }

        /// <summary>
        /// Latitude of plane
        /// </summary>
        public double Latitude
        {
            get; set;
        }

        /// <summary>
        /// Altitude of plane
        /// </summary>
        public double Altitude
        {
            get; set;
        }

        /// <summary>
        /// Speed of plane
        /// </summary>
        public double Speed
        {
            get; set;
        }

        /// <summary>
        /// SetCommands : sets the command flag to 1 if longtitude and latitude are required,
        /// and 2 if longtitude, latitude, altitude and speed are required.
        /// </summary>
        /// <param name="val"></param>
        public void SetCommands(int val)
        {
            commands = val;
        }

        /// <summary>
        /// SetFlagToFalse : sets the finish flag to false when controller got 
        /// the data from model.
        /// </summary>
        public void SetFlagToFalse()
        {
            flagFinish = false;
        }

        /// <summary>
        /// GetFlagFinish : returns the finish flag.
        /// </summary>
        /// <returns></returns>
        public bool GetFlagFinish()
        {
            return flagFinish;
        }

        /// <summary>
        /// GetToStop : returns the stop flag.
        /// </summary>
        /// <returns></returns>
        public bool GetToStop()
        {
            return stop;
        }

        /// <summary>
        /// OpenSocket : opens the client socket and connects to the simulator.
        /// </summary>
        public void OpenSocket()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            using (NetworkStream stream = client.GetStream())
            using (BinaryReader reader = new BinaryReader(stream))
            {
                StreamReader sr = new StreamReader(stream);
                string input;
                string[] values;
                // while not stop connection, asks the simulator for data when it requires
                while (!stop)
                {
                    // if commans is 1 or 2, read data from simulator
                    if (commands == 1 || commands == 2)
                    {
                        // reads longtitude and latitude
                        Byte[] data = new Byte[100];
                        flagFinish = false;
                        data = Encoding.ASCII.GetBytes(getLon);
                        stream.Write(data, 0, data.Length); // write to simulator
                        input = sr.ReadLine(); // get data
                        values = input.Split('\'');
                        Longtitude = Convert.ToDouble(values[1]);
                        data = Encoding.ASCII.GetBytes(getLat);
                        stream.Write(data, 0, data.Length); // write to simulator
                        input = sr.ReadLine(); // get data
                        values = input.Split('\'');
                        Latitude = Convert.ToDouble(values[1]);
                        // if command is 2, reads altitude and speed
                        if (commands == 2)
                        {
                            data = Encoding.ASCII.GetBytes(getSpeed);
                            stream.Write(data, 0, data.Length); // write to simulator
                            input = sr.ReadLine(); // get data
                            values = input.Split('\'');
                            Speed = Convert.ToDouble(values[1]);
                            data = Encoding.ASCII.GetBytes(getAlt);
                            stream.Write(data, 0, data.Length); // write to simulator
                            input = sr.ReadLine(); // get data
                            values = input.Split('\'');
                            Altitude = Convert.ToDouble(values[1]);
                        }
                        commands = 0;
                        flagFinish = true; // finish reading
                    }
                }
            }
            client.Close();
        }


        /// <summary>
        /// Connect : opens a task that connects and gets data from the simulator
        /// </summary>
        public void Connect()
        {
            stop = false;
            Task t = new Task(() => { OpenSocket(); });
            t.Start();
        }

        /// <summary>
        /// Disconnect : disconnect the simulator
        /// </summary>
        public void Disconnect()
        {
            stop = true;
        }
    } 
}