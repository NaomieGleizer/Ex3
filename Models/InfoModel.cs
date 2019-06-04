using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Ajax_Minimal.Models
{
    /// <summary>
    /// InfoModel : model of Flight Board.
    /// Opens a data server that gets location from simulator
    /// </summary>
    public class InfoModel
    {

        private bool stop = false;
       // public event PropertyChangedEventHandler PropertyChanged;
        private string ip = "127.0.0.1";
        private int port = 5400;


        /// <summary>
        /// Constructor :  gets an ApplicationSettingsModel for server information. 
        /// </summary>
        /// <param name="applicationSettingsModel"></param>
        public InfoModel()
        {
        }

        public bool SimulatorConnection
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
        /// Longitude of plane
        /// </summary>
        public double Longitude
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
        /// Altitude of plane
        /// </summary>
        public double Altitude
        {
            get; set;
        }

        /// <summary>
        /// Server : opens a server that reads location from simulator.
        /// </summary>
        public void Server()
        {
            SimulatorConnection = false;
            Longitude = 0;
            Latitude = 0;
           
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient(); // accept a client

            using (NetworkStream stream = client.GetStream())
            using (BinaryReader reader = new BinaryReader(stream))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                double prevLat = -1;
                double prevLon = -1;
                StreamReader sr = new StreamReader(stream);
                // while not stop, read location from simulator
                while (!stop)
                {
                    // gets info from client
                    string input = sr.ReadLine();
                    string[] values = input.Split(',');
                    Longitude = Convert.ToDouble(values[0]);
                    Latitude = Convert.ToDouble(values[1]);
                    Speed = Convert.ToDouble(values[2]);
                    Altitude = Convert.ToDouble(values[3]);

                    SimulatorConnection = true;
                    input = "";
                    // if location is different from prvious location, 
                    // update location and notify it changed
                    if (prevLat != Latitude || prevLon != Longitude)
                    {
                        prevLon = Longitude;
                        prevLat = Latitude;

                    }

                }
            }
            client.Close();
            listener.Stop();
        }

        /// <summary>
        /// OpenServer : opens a task that opens a server and gets information from simulator
        /// </summary>
        public void OpenServer()
        {
            Task t = new Task(() => { Server(); });
            t.Start();
        }



        /// <summary>
        /// CloseServer : close the server.
        /// </summary>
        public void CloseServer()
        {
            stop = true;
        }
    }
}