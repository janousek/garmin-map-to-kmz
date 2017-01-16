
using Core;
using ImageMagick;
using Newtonsoft.Json;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapTransform
{
    class Program
    {
        static void Main(string[] args)
        {

            string mapName = "Ordesa";



            DirectoryInfo workDir = new DirectoryInfo("./tmp");
            if (!workDir.Exists)
            {
                workDir.Create();
            }






            // https://github.com/timwaters/mapwarper/blob/9c7e8cac0d2296b28638f4dc45e018101632be23/app/models/map.rb


            // https://github.com/timwaters/mapwarper/blob/9c7e8cac0d2296b28638f4dc45e018101632be23/app/controllers/maps_controller.rb
            

            List<double> gcp = new List<double>() {
                2182.0480714032 // x
                ,1079.1186600104 // y
                ,-0.027731359 // lat
                ,42.683687447 // lng

                ,4836.3574218623
                ,188.9133262387
                ,0.1948544383
                ,42.7325371163

                ,4562.3597533423
                ,3221.43362442239
                ,0.1628556848
                ,42.5478163951

                ,901.54534623294
                ,3603.72123486013
                ,-0.1400944591
                ,42.5326387248

                ,2833.7686364819
                ,3896.20743004992
                ,0.0180914998
                ,42.5107194551
            };


            string srcFilename = "mapa.jpg";
            string destJpg = "result.jpg";

            DirectoryInfo gdalDir = new DirectoryInfo("../../../gdal/");

            MapImageBuilder mapBuilder = new MapImageBuilder();

            string infoJson = mapBuilder.Build(gdalDir, srcFilename, destJpg, gcp, workDir);

            string infoJsonName = "info.json";
            File.WriteAllText(Path.Combine(workDir.FullName, infoJsonName), infoJson);
            
            GarminKmzBuilder kmzBuilder = new GarminKmzBuilder();
            kmzBuilder.Build(workDir, infoJsonName, destJpg, mapName);

        }


        
        
        
    }
}
