using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Core;
using Microsoft.AspNetCore.Hosting;
using CustomImageMapWeb.Core;
using System.Globalization;

namespace CustomImageMapWeb.Controllers
{
    public class WarpController : Controller
    {
        private IHostingEnvironment hostingEnvironment;
        public WarpController(IHostingEnvironment hostingEnvironment) {
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public IActionResult Index(WarpForm form)
        {
            string mapName = "Ordesa";


            string appDataPath = this.hostingEnvironment.ContentRootFileProvider.GetFileInfo("App_Data").PhysicalPath;

            DirectoryInfo workDir = new DirectoryInfo(Path.Combine(appDataPath, "./tmp"));
            if (!workDir.Exists)
            {
                workDir.Create();
            }


            // https://github.com/timwaters/mapwarper/blob/9c7e8cac0d2296b28638f4dc45e018101632be23/app/models/map.rb


            // https://github.com/timwaters/mapwarper/blob/9c7e8cac0d2296b28638f4dc45e018101632be23/app/controllers/maps_controller.rb


            DirectoryInfo gdalDir = new DirectoryInfo(Path.Combine(appDataPath, "./gdal/"));


            string srcFilename = this.hostingEnvironment.WebRootFileProvider.GetFileInfo("mapa.jpg").PhysicalPath;
            string destJpg = "result.jpg";

            MapImageBuilder mapBuilder = new MapImageBuilder();

            string infoJson = mapBuilder.Build(gdalDir, srcFilename, destJpg, form.Gcp.Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToList(), workDir);

            string infoJsonName = "info.json";
            System.IO.File.WriteAllText(Path.Combine(workDir.FullName, infoJsonName), infoJson);

            GarminKmzBuilder kmzBuilder = new GarminKmzBuilder();
            kmzBuilder.Build(workDir, infoJsonName, destJpg, mapName);




            return new JsonResult(new {

            });
        }
        
    }
}
