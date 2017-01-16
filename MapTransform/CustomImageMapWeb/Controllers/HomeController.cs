using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CustomImageMapWeb.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile map)
        {
            using (Stream fileStream = map.OpenReadStream()) {
                using (Stream outputStream = System.IO.File.Open(this.hostingEnvironment.WebRootFileProvider.GetFileInfo("mapa.jpg").PhysicalPath, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.Write)) {
                    fileStream.CopyTo(outputStream);
                }
            }

            return RedirectToAction("Index");
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
