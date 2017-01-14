
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


            string transform_options = "auto";
            string resample_options = "near";

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




            List<string> gcpItems = new List<string>();
            for(int i = 0; i < gcp.Count; i += 4)
            {
                gcpItems.Add(" -gcp " + gcp[i + 0].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 1].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 2].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 3].ToString(CultureInfo.InvariantCulture));
            }

            string gcp_string = string.Join(" ", gcpItems);

            string src_filename = "mapa.jpg";

            string dest_jpg = "result.jpg";

            
            File.Copy(src_filename, Path.Combine(workDir.FullName, src_filename), true);

            string temp_filename = "temp";
            

            Execute(workDir, "gdal_translate", $"-a_srs '+init=epsg:4326' -of VRT {src_filename} {temp_filename}.vrt {gcp_string}");

            transform_options = "";
            string mask_options = "";
            string dest_filename = "result.tiff";

            if (File.Exists(Path.Combine(workDir.FullName, dest_filename)))
            {
                File.Delete(Path.Combine(workDir.FullName, dest_filename));
            }

            Execute(workDir, "gdalwarp", $"-dstalpha {mask_options} {transform_options} -r {resample_options} -s_srs EPSG:4326 {temp_filename}.vrt {dest_filename} -co TILED=YES -co COMPRESS=LZW");


            Execute(workDir, "gdaladdo", $"-r average {dest_filename} 2 4 8 16 32 64");


            string infoJson = Execute(workDir, "gdalinfo", $"-json {dest_filename}");
            File.WriteAllText(Path.Combine(workDir.FullName, "info.json"), infoJson);
            
            // options for "-co"
            // JPG http://www.gdal.org/frmt_jpeg.html
            // PNG: http://www.gdal.org/frmt_various.html#PNG
            
            Execute(workDir, "gdal_translate", $"-of JPEG -scale -co worldfile=yes {dest_filename} {dest_jpg}");
            
    


            List<GroundOverlay> overlays = PrepareOverlays(workDir, dest_jpg);

            BuildKmz(workDir, mapName, overlays);

        }



        private static string Execute(DirectoryInfo workDir, string toolName, string args) {

            DirectoryInfo gdalDir = new DirectoryInfo("../../../gdal/");
            FileInfo exeFile = new FileInfo(Path.Combine(gdalDir.FullName, toolName + ".exe"));
            DirectoryInfo gdalData = new DirectoryInfo(Path.Combine(gdalDir.FullName, "data"));
            string dataPath = gdalData.FullName.Replace('\\', '/');

            if (toolName == "gdal_translate" || toolName == "gdalwarp") { 
                args = $"--config GDAL_DATA \"{dataPath}\" " + args;
            }

            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = workDir.FullName;
                startInfo.UseShellExecute = false;
                startInfo.FileName = exeFile.FullName;
                startInfo.Arguments = args;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                process.StartInfo = startInfo;
                process.Start();

                StringBuilder result = new StringBuilder();
                result.Append(process.StandardOutput.ReadToEnd());
                
                process.WaitForExit();

                if(process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new ApplicationException(error);
                }

                result.Append(process.StandardOutput.ReadToEnd());


                return result.ToString();
            }
        }


        private static List<GroundOverlay> PrepareOverlays(DirectoryInfo workDir, string map) {

            string jsonInfo = File.ReadAllText(Path.Combine(workDir.FullName, "info.json"));
            TiffJsonInfo info = JsonConvert.DeserializeObject<TiffJsonInfo>(jsonInfo);
            List<GroundOverlay> result = new List<GroundOverlay>();
            



            double w = 1024.0;
            double h = 768.0;
            


            using (MagickImage img = new MagickImage(new FileInfo(Path.Combine(workDir.FullName, map))))
            {

                int mapWidth = img.Width;
                int mapHeight = img.Height;

                int xTileCount = (int)Math.Floor((double)mapWidth / w);
                int yTileCount = (int)Math.Floor((double)mapHeight / h);

                int tileWidth = mapWidth / xTileCount;
                int tileHeight = mapHeight / yTileCount;



                double pxGpsWidth = (info.CornerCoordinates.LowerRight[0] - info.CornerCoordinates.UpperLeft[0]) / mapWidth;
                double pxGpsHeight = (info.CornerCoordinates.LowerRight[1] - info.CornerCoordinates.UpperLeft[1]) / mapHeight;
               
                var tiles = img.CropToTiles(tileWidth, tileHeight);

                int i = 0;
                foreach (MagickImage tile in tiles)
                {
                    int x = i % xTileCount;
                    int y = i / xTileCount;

                    string fileName = "files/test-" + x + "-" + y + ".jpg";
                    tile.Write(Path.Combine(workDir.FullName, fileName));



                    result.Add(new GroundOverlay()
                    {
                        Icon = new SharpKml.Dom.Icon()
                        {
                            Href = new Uri(fileName, UriKind.Relative)
                        },
                        Bounds = new LatLonBox()
                        {
                            North = info.CornerCoordinates.UpperLeft[1] + pxGpsHeight * tile.Page.Y,
                            South = info.CornerCoordinates.UpperLeft[1] + pxGpsHeight * (tile.Page.Y + tile.Height),
                            East = info.CornerCoordinates.UpperLeft[0] + pxGpsWidth * (tile.Page.X + tile.Width),
                            West = info.CornerCoordinates.UpperLeft[0] + pxGpsWidth * tile.Page.X
                        }
                    });

                    i++;


                    tile.Dispose();
                }
                
            }
    
            return result;
        }


        public static void BuildKmz(DirectoryInfo workDir, string mapName, List<GroundOverlay> overlays) {

            Document doc = new Document()
            {
                Name = mapName
            };

            for (int i = 0; i < overlays.Count; i++)
            {
                Folder folder = new Folder()
                {
                    Name = mapName + i
                };

                GroundOverlay group = overlays[i];
                group.DrawOrder = 1;

                folder.AddFeature(group);

                doc.AddFeature(folder);
            }
            

            KmlFile kml = KmlFile.Create(doc, false);
            using (FileStream stream = File.OpenWrite(Path.Combine(workDir.FullName, "result.kml")))
            {
                stream.SetLength(0);
                stream.Seek(0, SeekOrigin.Begin);

                kml.Save(stream);
            }


            using (FileStream zipToOpen = new FileStream(Path.Combine(workDir.FullName, "result.kmz"), FileMode.OpenOrCreate))
            {
                zipToOpen.SetLength(0);
                zipToOpen.Seek(0, SeekOrigin.Begin);

                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(Path.Combine(workDir.FullName, "result.kml"), "result.kml");
                    foreach (GroundOverlay overlay in overlays)
                    {
                        string path = overlay.Icon.Href.GetPath();
                        archive.CreateEntryFromFile(Path.Combine(workDir.FullName, path), path);
                    }
                }
            }
        

    }
    }
}
