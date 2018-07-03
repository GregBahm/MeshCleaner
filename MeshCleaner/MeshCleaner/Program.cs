using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            MeshCleaner.Execute();
        }
    }

    public class MeshCleaner
    {
        private static readonly string RootDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        private static readonly string HeaderFile = RootDir + "\\header.txt";
        private static readonly string SourceFolder = RootDir + "\\Source\\";
        private static readonly string OutputFolder = RootDir + "\\Output\\";

        private static byte[] HeaderContent;

        public static void Execute()
        {
            if(!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            HeaderContent = File.ReadAllBytes(HeaderFile);

            int progress = 1;
            string[] files = Directory.GetFiles(SourceFolder);
            DateTime startTime = DateTime.Now;
            foreach (string file in files)
            {
                CleanThatMesh(file);
                UpdateProgress(progress, files.Length, startTime);
                progress++;
            }
        }

        private static void UpdateProgress(int progress, int length, DateTime startTime)
        {
            float prog = (float)progress / length;
            int progPercent = (int)(prog * 100);
            long averageCompletionTime = (DateTime.Now - startTime).Ticks / progress;
            long timeRemaining = averageCompletionTime * (length - progress);
            TimeSpan remaining = new TimeSpan(timeRemaining);
            Console.Clear();
            Console.WriteLine("{0} of {1} - {2}% Complete. Estimated {3} remaining", progress, length, progPercent, remaining);
        }

        private static string GetOutputPath(string inputPath)
        {
            string fileName = Path.GetFileName(inputPath);
            return OutputFolder + fileName;
        }

        private static byte[] GetNewHeader(int vertCount)
        {
            string header = File.ReadAllText(HeaderFile);
            header = header.Replace("118364", vertCount.ToString());
            return Encoding.ASCII.GetBytes(header);
        }

        private static void CleanThatMesh(string file)
        {

            byte[] fileBytes = File.ReadAllBytes(file);
            List<byte> outputData = new List<byte>();
            int stride = sizeof(float) * 3 + sizeof(byte) * 3;

            StringBuilder s = new StringBuilder();
            int newVertCount = 0;
            for (int k = HeaderContent.Length; k < fileBytes.Length; k += stride)
            {
                float x = BitConverter.ToSingle(fileBytes, k);
                float y = BitConverter.ToSingle(fileBytes, k + sizeof(float));
                float z = BitConverter.ToSingle(fileBytes, k + sizeof(float) * 2);
                bool keep = ShouldKeep(x, y, z);
                if (keep)
                {
                    newVertCount++;
                    for (int i = k; i < (k + stride); i++)
                    {
                        outputData.Add(fileBytes[i]);
                    }
                }
            }
            byte[] newHeader = GetNewHeader(newVertCount);

            string outputPath = GetOutputPath(file);
            File.WriteAllBytes(outputPath, newHeader.Concat(outputData).ToArray());
        }

        private static bool ShouldKeep(float x, float y, float z)
        {
            // Moved a sphere around in Maya and then wrote down the transforms to get what I want
            x *= 0.0008014f;
            y *= 0.0008014f;
            z *= 0.0008014f;
            x += 0.057f;
            y += 0.26f;
            z += -1.163f;
            double dist = Math.Sqrt(x * x + y * y + z * z);
            return dist < 1;
        }
    }
}
