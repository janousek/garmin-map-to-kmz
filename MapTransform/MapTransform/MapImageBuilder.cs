using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTransform
{
    class MapImageBuilder
    {
        public string Build(string src_filename, string dest_jpg, List<double> gcp, DirectoryInfo workDir)
        {
            string resample_options = "near";



            List<string> gcpItems = new List<string>();
            for (int i = 0; i < gcp.Count; i += 4)
            {
                gcpItems.Add(" -gcp " + gcp[i + 0].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 1].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 2].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 3].ToString(CultureInfo.InvariantCulture));
            }

            string gcp_string = string.Join(" ", gcpItems);
            

            File.Copy(src_filename, Path.Combine(workDir.FullName, src_filename), true);

            string temp_filename = "temp";


            Execute(workDir, "gdal_translate", $"-a_srs '+init=epsg:4326' -of VRT {src_filename} {temp_filename}.vrt {gcp_string}");

            string transform_options = "";
            string mask_options = "";
            string dest_filename = "result.tiff";

            if (File.Exists(Path.Combine(workDir.FullName, dest_filename)))
            {
                File.Delete(Path.Combine(workDir.FullName, dest_filename));
            }

            Execute(workDir, "gdalwarp", $"-dstalpha {mask_options} {transform_options} -r {resample_options} -s_srs EPSG:4326 {temp_filename}.vrt {dest_filename} -co TILED=YES -co COMPRESS=LZW");


            Execute(workDir, "gdaladdo", $"-r average {dest_filename} 2 4 8 16 32 64");


            string infoJson = Execute(workDir, "gdalinfo", $"-json {dest_filename}");

            // options for "-co"
            // JPG http://www.gdal.org/frmt_jpeg.html
            // PNG: http://www.gdal.org/frmt_various.html#PNG

            Execute(workDir, "gdal_translate", $"-of JPEG -scale -co worldfile=yes {dest_filename} {dest_jpg}");

            return infoJson;
        }



        private string Execute(DirectoryInfo workDir, string toolName, string args)
        {

            DirectoryInfo gdalDir = new DirectoryInfo("../../../gdal/");
            FileInfo exeFile = new FileInfo(Path.Combine(gdalDir.FullName, toolName + ".exe"));
            DirectoryInfo gdalData = new DirectoryInfo(Path.Combine(gdalDir.FullName, "data"));
            string dataPath = gdalData.FullName.Replace('\\', '/');

            if (toolName == "gdal_translate" || toolName == "gdalwarp")
            {
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

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new ApplicationException(error);
                }

                result.Append(process.StandardOutput.ReadToEnd());


                return result.ToString();
            }
        }

    }
}
