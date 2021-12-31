using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener
{
    public static class Log
    {
        public const bool Enabled = true; //używać PRZED skręceniem stringa do zalogowania

        static string? filePath = null;

        public static bool Entry(string text)
        {
            if (filePath == null)
                filePath = String.Concat(GetPathMakeFolder(@"\Logs"), @"\", DateTime.Now.Date.ToShortDateString(), ".txt");

            FileStream fileStream = null;
            bool success = true;

            text = String.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), " ", text, "\n");

            try
            {
                fileStream = new FileStream(filePath, FileMode.Append);
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8, 1024))
                {
                    streamWriter.Write(text);
                }
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine("Logger.Save() exception caught: " + e);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();
            }
            return success;
        }


        private static string GetPathMakeFolder(string relativePath)
        {
            if (relativePath[0] != @"\"[0])
            {
                relativePath = @"\" + relativePath;
            }
            if (relativePath[relativePath.Length - 1] != @"\"[0])
            {
                relativePath += @"\";
            }


            string exeFolderPath = System.Reflection.Assembly.GetEntryAssembly().Location.ToString();
            exeFolderPath = exeFolderPath.Substring(0, exeFolderPath.LastIndexOf('.'));
            string relativeFolderPath = relativePath;



            string fullFolderPath = exeFolderPath + relativeFolderPath;

            if (!System.IO.Directory.Exists(fullFolderPath))
            {
                System.IO.Directory.CreateDirectory(fullFolderPath);
            }
            return fullFolderPath;
        }
    }
}
