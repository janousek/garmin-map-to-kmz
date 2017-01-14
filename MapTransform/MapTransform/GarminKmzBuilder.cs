﻿using ImageMagick;
using Newtonsoft.Json;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTransform
{
    class GarminKmzBuilder
    {
        public void Build(DirectoryInfo workDir, string infoJsonName, string mapJpg, string mapName)
        {
            List<GroundOverlay> overlays = PrepareOverlays(workDir, infoJsonName, mapJpg);

            BuildKmz(workDir, mapName, overlays);
        }



        private List<GroundOverlay> PrepareOverlays(DirectoryInfo workDir, string infoJsonName, string mapJpg)
        {

            string jsonInfo = File.ReadAllText(Path.Combine(workDir.FullName, infoJsonName));
            TiffJsonInfo info = JsonConvert.DeserializeObject<TiffJsonInfo>(jsonInfo);
            List<GroundOverlay> result = new List<GroundOverlay>();




            double w = 800;
            double h = 600;



            using (MagickImage img = new MagickImage(new FileInfo(Path.Combine(workDir.FullName, mapJpg))))
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



        public void BuildKmz(DirectoryInfo workDir, string mapName, List<GroundOverlay> overlays)
        {

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
