using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MapImageBuilder
    {
        public string Build(DirectoryInfo gdalDir, string srcFilePath, string destJpg, List<double> gcp, DirectoryInfo workDir)
        {



            string resampleOptions = "near";

            string srcFilename = "ordesa.jpg";

            List<string> gcpItems = new List<string>();
            for (int i = 0; i < gcp.Count; i += 4)
            {
                gcpItems.Add(" -gcp " + gcp[i + 0].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 1].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 2].ToString(CultureInfo.InvariantCulture) + ", " + gcp[i + 3].ToString(CultureInfo.InvariantCulture));
            }

            string gcpString = string.Join(" ", gcpItems);

            File.Copy(srcFilePath, Path.Combine(workDir.FullName, srcFilename), true);

            string tempFilename = "temp";


            Execute(gdalDir, workDir, "gdal_translate", $"-a_srs '+init=epsg:4326' -of VRT {srcFilename} {tempFilename}.vrt {gcpString}");

            string transformOptions = "";
            string maskOptions = "";
            string destFilename = "result.tiff";

            if (File.Exists(Path.Combine(workDir.FullName, destFilename)))
            {
                File.Delete(Path.Combine(workDir.FullName, destFilename));
            }


            Execute(gdalDir, workDir, "gdalwarp", $"-dstalpha {maskOptions} {transformOptions} -r {resampleOptions} -s_srs EPSG:4326 {tempFilename}.vrt {destFilename} -co TILED=YES -co COMPRESS=LZW ");


            Execute(gdalDir, workDir, "gdaladdo", $"-r average {destFilename} 2 4 8 16 32 64");


            string infoJson = Execute(gdalDir, workDir, "gdalinfo", $"-json {destFilename}");

            // options for "-co"
            // JPG http://www.gdal.org/frmt_jpeg.html
            // PNG: http://www.gdal.org/frmt_various.html#PNG

            Execute(gdalDir, workDir, "gdal_translate", $"-of JPEG -scale -co worldfile=yes {destFilename} {destJpg}");

            return infoJson;
        }



        private string Execute(DirectoryInfo gdalDir, DirectoryInfo workDir, string toolName, string args)
        {
            
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
