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

        public IActionResult Index()
        {

            DemoParser demoParser = new DemoParser(LDemo);
            demoParser.ParseHeader();

            

            List<Vector2> posicoes = new List<Vector2>();
            bool hasMatchStarted = false;
            

            demoParser.MatchStarted += (sender, e) => {
                hasMatchStarted = true;
            };

            demoParser.PlayerHurt += (sender, e) => {
                if(hasMatchStarted) {
                }
            };
            
            demoParser.WeaponFired += (sender, e) => {
                if (e.Shooter.Name.Contains("SILVASSAURO") && hasMatchStarted){
                    Vector2 vet = TrasnlateScale(e.Shooter.Position.X, e.Shooter.Position.Y);
                    posicoes.Add(vet);
                }
            };

            demoParser.ParseToEnd();

            DrawingPoints(posicoes);

            return View(demoParser.ReadPlayersName());
        }

        private void DrawingPoints(List<Vector2> APositions)
        {
            
            using (var image = System.IO.File.Open("wwwroot\\images\\de_dust2.jpg", FileMode.Open))
            {
                var bitmap = new Bitmap(image);
                Graphics graph = Graphics.FromImage(bitmap);

                Pen pen = new Pen(Color.Red);
                foreach (Vector2 Position in APositions)
                {
                    graph.DrawEllipse(pen, Position.X, Position.Y, 10, 10);
                }

                bitmap.Save("wwwroot/images/head_map.png", ImageFormat.Png);
            }
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
