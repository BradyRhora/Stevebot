using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Stevebot
{
    public static class Functions
    {
        public static Process DalleProcess = null;
        public static async Task<Stream> GenerateImage(string prompt)
        {
            if (DalleProcess == null)
                InitDalle();

            if (File.Exists("generated.png")) File.Delete("generated.png");
            DalleProcess.StandardInput.WriteLine(prompt);

            //Console.WriteLine(DalleProcess.StandardOutput.ReadToEnd());
            return await Task.Run(AwaitGeneration);
        }

        private static Stream AwaitGeneration()
        {
            while (true)
            {
                if (File.Exists("generated.png"))
                {
                    try
                    {
                        return new FileStream("generated.png", FileMode.Open);
                    }
                    catch (IOException e)
                    {
                        if (e.HResult != -2147024864)
                            throw;
                    }
                }
            }
        }

        private static void InitDalle()
        {
            string cmd = "dallemini.py";
            var start = new ProcessStartInfo();
            start.FileName = @"C:\Python310\python.exe";
            start.Arguments = cmd;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = false; //set false later? maybe
            start.RedirectStandardInput = true;
            DalleProcess = Process.Start(start);
        }
    }
}
