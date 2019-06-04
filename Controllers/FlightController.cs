using Ajax_Minimal.Models;
using System;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Ajax_Minimal.Controllers
{
    public class FlightController : Controller
    {
        private static CommandsModel model = new CommandsModel();
        private string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "App_Data/flight1.txt");
        private static StreamReader reader= null;

        /// <summary>
        /// Return the Index view.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// The function gets the arguments for the view. If the string is an IP adrees, the function returns 
        /// the Position view and otherwise returns the Load view.
        /// </summary>
        [HttpGet]
        public ActionResult PositionOrLoad(string s, int num)
        {
            IPAddress ip;
            if (IPAddress.TryParse(s, out ip))
            {
                return Position(s, num);
            }
            return Load(s, num);
        }

        /// <summary>
        /// Return the Path view.
        /// </summary>
        [HttpGet]
        public ActionResult Path(string ip, int port, int seconds)
        {
            ClosePreviousView();

            model.Connect();
            Session["time"] = 1.0 / seconds; // the amount of time per seconds

            return View();
        }

        /// <summary>
        /// Return the Position view.
        /// </summary>
        [HttpGet]
        public ActionResult Position(string ip, int port)
        {
            ClosePreviousView();
            model.Connect();

            return View("Position");
        }

        /// <summary>
        /// Open the file flight1.txt and return the Save view.
        /// </summary>
        [HttpGet]
        public ActionResult Save(string ip, int port, int perSeconds, int duration, string file)
        {
            ClosePreviousView();
            model.Connect();

            if (System.IO.File.Exists(path)) // if the file exists, delete it
            {
                System.IO.File.Delete(path);
            }
            FileStream stream = System.IO.File.Create(path); // create the file
            stream.Close();
            Session["perSec"] = 1.0 / perSeconds; 
            Session["duration"] = duration;

            return View();
        }
        
        /// <summary>
        /// Return the Load view.
        [HttpGet]
        public ActionResult Load(string file, int seconds)
        {
            ClosePreviousView();

            Session["perSec"] = 1.0 / seconds;
            reader = new StreamReader(path); // open flight.txt
            return View("Load");
        }

        /// <summary>
        /// The function disconnect the connection to the simulator if the model is still connected, 
        /// and close flight1.txt if it's open.
        /// </summary>
        public void ClosePreviousView()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (!model.GetToStop())
            {
                model.Disconnect();
                model.SetFlagToFalse();
            }
        }
     
        /// <summary>
        /// The function is called from the view and returns the longtitude and latitude
        /// valus of the plan from the model.
        /// </summary>
        [HttpGet]
        public JsonResult GetLatLon()
        {
            double lon;
            double lat;
            // send 1 for getting the latitude and longtitude
            model.SetCommands(1);
            // wait untill the model returns the values (getFlagFinish) 
            do
            {
                lon = model.Longtitude;
                lat = model.Latitude;
            } while (!model.GetFlagFinish() && !model.GetToStop());

            if (model.GetToStop()) // if the user changes the view
            {
                return null;
            }
            model.SetFlagToFalse(); // change the flag of the model back to false
            return Json(new { x = lon, y = lat }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The function is called from the view, returns the longtitude, latitude, speed and altitude
        /// valus of the plan from the model and save them in the file.
        /// </summary>
        [HttpGet]
        public JsonResult SaveValuesOfPlan()
        {
            double lon, lat, alt, speed;
            // send 2 for getting the latitude, longtitude, speed and altitude
            model.SetCommands(2);
            // wait untill the model returns the values (getFlagFinish) 
            do
            {
                lon = model.Longtitude;
                lat = model.Latitude;
                speed = model.Speed;
                alt = model.Altitude;
            } while (!model.GetFlagFinish() && !model.GetToStop());

            if (model.GetToStop()) // if the user changes the view
            {
                return null;
            }
            model.SetFlagToFalse(); // change the flag of the model back to false

            string[] lines = { lon.ToString(), lat.ToString(), speed.ToString(), alt.ToString() };
            System.IO.File.AppendAllLines(path, lines); // save values in file
            return Json(new { x = lon, y = lat }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The function is called from the view, gets the longtitude, latitude, speed and altitude
        /// valus of the plan from the file and rerurns them.
        /// /// </summary>
        [HttpGet]
        public JsonResult LoadValuesFromFile()
        {
            // read the 4 values from file
            string lon = reader.ReadLine();
            if (lon == null) // if it is the end of file, return empty object
            {
                return Json(new object[] { new object() }, JsonRequestBehavior.AllowGet); ;
            }
            string lat = reader.ReadLine();
            string speed = reader.ReadLine();
            string alt = reader.ReadLine();
            return Json(new { x = Convert.ToDouble(lon), y = Convert.ToDouble(lat)
                , z = Convert.ToDouble(speed), w = Convert.ToDouble(alt) }, JsonRequestBehavior.AllowGet);
        }
    }
}