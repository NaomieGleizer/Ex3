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
    /// CommandsModel : model of Auto Pilot, and Joystick. 
    /// Connected to the flight simulator and send it commands. 
    /// </summary>
    public class CommandsModel 
    {
        private bool stop = true;
        private int commands = 0;
        private bool flagFinish;
        private string ip = "127.0.0.1";
        private int port = 5402;
        private string getLat = "get /position/latitude-deg\r\n";
        private string getLon = "get /position/longitude-deg\r\n";
        private string getAlt = "get /instrumentation/altimeter/indicated-altitude-ft\r\n";
        private string getSpeed = "get /instrumentation/airspeed-indicator/indicated-speed-kt\r\n";
        //private Mutex mtx;

        public CommandsModel()
        {

        }

        public double Longtitude
        {
            get; set;
        }

        public double Latitude
        {
            get; set;
        }

        public double Altitude
        {
            get; set;
        }
        public double Speed
        {
            get; set;
        }

        public void SetCommands(int val)
        {
            commands = val;
        }

        public void SetFlagToFalse()
        {
            flagFinish = false;
        }

        public bool GetFlagFinish()
        {
            return flagFinish;
        }

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
                // while not stop connection, send given commands to the simulator 
                while (!stop)
                {
                    Byte[] data = new Byte[100];
                    // send every command to the simulator
                    if (commands == 1 || commands == 2)
                    {
                        flagFinish = false;
                        data = Encoding.ASCII.GetBytes(getLon);
                        stream.Write(data, 0, data.Length); // write to simulator
                        input = sr.ReadLine();
                        values = input.Split('\'');
                        Longtitude = Convert.ToDouble(values[1]);
                        data = Encoding.ASCII.GetBytes(getLat);
                        stream.Write(data, 0, data.Length); // write to simulator
                        input = sr.ReadLine();
                        values = input.Split('\'');
                        Latitude = Convert.ToDouble(values[1]);
                        if (commands == 2)
                        {
                            data = Encoding.ASCII.GetBytes(getSpeed);
                            stream.Write(data, 0, data.Length); // write to simulator
                            input = sr.ReadLine();
                            values = input.Split('\'');
                            Speed = Convert.ToDouble(values[1]);
                            data = Encoding.ASCII.GetBytes(getAlt);
                            stream.Write(data, 0, data.Length); // write to simulator
                            input = sr.ReadLine();
                            values = input.Split('\'');
                            Altitude = Convert.ToDouble(values[1]);
                        }
                        commands = 0;
                        flagFinish = true;
                    }
                }
            }
            client.Close();
        }


        /// <summary>
        /// Connect : opens a task that connects and sends given commands to the simulator
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