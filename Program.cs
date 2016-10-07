using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cdf2csv
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 0)
            {
                Console.WriteLine("Usage: cdf2csv {input_file}");
            }
            else
            {
                String path = args[0];

                if (File.Exists(path))
                {
                    try
                    {
                        Console.Write("Loading CDF file... ");
                        var cdffile = CDFFile.Load(path);
                        Console.WriteLine("loaded!");

                        Console.Write("Writing CSV file... ");
                        cdffile.SaveCSV(path + ".csv");
                        Console.WriteLine("saved!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The file could not be converted:");
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("The file " + path + " doesn't exist.");
                }
            }
        }
    }
}
