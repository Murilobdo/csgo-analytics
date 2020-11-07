using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DemoInfo;
using System.IO;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using csgo_analytics.DemoExtensions;

namespace csgo_analytics.Controllers
{
    public class HomeController : Controller
    {
        
        FileStream LDemo = System.IO.File.OpenRead("C:\\Downloads_hltv\\dust2.dem");
        Map MapDeDust2;
        
        public HomeController(ILogger<HomeController> logger)
        {
            MapDeDust2 = MakeMap("de_dust2", -2476, 3239, 4.4f);
        }

        public IActionResult Index(string name = "")
        {
            ViewBag.FilterName = name;
            DemoParser demoParser = new DemoParser(LDemo);
            demoParser.ParseHeader();

            List<Vector2> shootingPositions = new List<Vector2>();
            List<Vector2> deathPositions = new List<Vector2>();
            bool hasMatchStarted = false;

            demoParser.MatchStarted += (sender, e) => {
                hasMatchStarted = true;
            };

            demoParser.PlayerKilled += (sender, e) => {
                if (e.Victim.Name.Contains("SILVASSAURO") && hasMatchStarted){
                    Vector2 vet = TrasnlateScale(e.Victim.LastAlivePosition.X, e.Victim.LastAlivePosition.Y);
                    deathPositions.Add(vet);
                }
            };
            demoParser.WeaponFired += (sender, e) => {
                if (e.Shooter.Name.Contains(name) && hasMatchStarted 
                   && e.Weapon.Weapon != EquipmentElement.Knife && e.Weapon.Weapon != EquipmentElement.Molotov
                   && e.Weapon.Weapon != EquipmentElement.Smoke && e.Weapon.Weapon != EquipmentElement.Flash
                   && e.Weapon.Weapon != EquipmentElement.Decoy && e.Weapon.Weapon != EquipmentElement.HE){
                    Vector2 vet = TrasnlateScale(e.Shooter.Position.X, e.Shooter.Position.Y);
                    shootingPositions.Add(vet);
                }
            };

            demoParser.ParseToEnd();

            DrawingPoints(shootingPositions);
            //DrawingPoints(deathPositions);
            
            return View(demoParser.ReadPlayersName());
        }

        private void DrawingPoints(List<Vector2> APositions)
        {
            
            using (var image = System.IO.File.Open("wwwroot\\images\\de_dust2.jpg", FileMode.Open))
            {
                var bitmap = new Bitmap(image);
                Graphics graph = Graphics.FromImage(bitmap);

                Brush brush= new SolidBrush(Color.Red);
                foreach (Vector2 Position in APositions)
                {
                    graph.FillEllipse(brush, Position.X, Position.Y, 10, 10);

                }

                bitmap.Save("wwwroot/images/heatmap/heat_map.png", ImageFormat.Png);
                
                graph.Dispose();
                bitmap.Dispose();
                image.Dispose();
            }
            Dispose();
        }

        private Vector2 TrasnlateScale(float x, float y)
        {
            Vector2 v = Translate(x, y);
            return new Vector2(v.X / MapDeDust2.Scale, v.Y / MapDeDust2.Scale);
        }

        private Vector2 Translate(float x, float y) => new Vector2(x - MapDeDust2.PZero.X, MapDeDust2.PZero.Y - y);

        private Map MakeMap(string name, float x, float y, float scale){
            return  new Map(name, new Vector2(x,y) ,scale);
        }   

        public class Map {
            public string name { get; set; }

            public Vector2 PZero { get; set; }

            public float Scale { get; set; }    

            public Map(string name, Vector2 PZero, float Scale)
            {
                this.name = name;
                this.PZero = PZero;
                this.Scale = Scale;
            }
        }
    }
}
