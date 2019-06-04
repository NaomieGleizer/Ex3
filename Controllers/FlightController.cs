using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using Ajax_Minimal.Models;
using System.IO;
using System.Reflection;
using System.Web.Configuration;

namespace Ajax_Minimal.Controllers
{
    public class FlightController : Controller
    {
        // GET: Flight

        private static CommandsModel model = new CommandsModel();
        private string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "App_Data/flight1.txt");
        private static StreamReader reader= null;


        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

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

        [HttpGet]
        public ActionResult map(string ip, int port, int seconds)
        {
            ClosePreviousView();

            model.Connect();
            Session["time"] = 1.0 / seconds;

            return View();
        }

        [HttpGet]
        public ActionResult Position(string ip, int port)
        {
            ClosePreviousView();
            model.Connect();

            return View("Position");
        }

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

        [HttpGet]
        public ActionResult Save(string ip, int port, int perSeconds, int duration, string file)
        {
            ClosePreviousView();
       
            model.Connect();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            FileStream flight= System.IO.File.Create(path);
            flight.Close();
            Session["perSec"] = 1.0 / perSeconds;
            Session["duration"] = duration;

            return View();
        }

        [HttpGet]
        public ActionResult Load(string file, int seconds)
        {
            ClosePreviousView();

            Session["perSec"] = 1.0 / seconds;
            reader = new StreamReader(path);
            return View("Load");
        }

        [HttpGet]
        public JsonResult GetLanLot()
        {
            double lon;
            double lat;
            model.SetCommands(1);

            do
            {
                lon = model.Longtitude;
                lat = model.Latitude;
            } while (!model.GetFlagFinish() && !model.GetToStop());
            if (model.GetToStop())
            {
                return null;
            }
            model.SetFlagToFalse();
            return Json(new { x = lon, y = lat }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SaveValuesOfPlan()
        {
            double lon, lat, alt, speed;
            model.SetCommands(2);
            do
            {
                lon = model.Longtitude;
                lat = model.Latitude;
                speed = model.Speed;
                alt = model.Altitude;
            } while (!model.GetFlagFinish() && !model.GetToStop());
            if (model.GetToStop())
            {
                return null;
            }
            model.SetFlagToFalse();
            string[] lines = { lon.ToString(), lat.ToString(), speed.ToString(), alt.ToString() };
            //string filePath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"App_Data", @"flight1.txt");
            System.IO.File.AppendAllLines(path, lines);
            return Json(new { x = lon, y = lat }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult LoadValuesFromFile()
        {
            string lon = reader.ReadLine();
            if (lon == null)
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