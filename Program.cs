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
            if (args.Length < 0) // CAMBIAR
            {
                Console.WriteLine("Usage: cdf2csv {input_file}");
            }
            else
            {
                //String path = args[0];
                String path = @"C:\\tmp\\in.cdl";

                if (File.Exists(path))
                {
                    Console.WriteLine("Leyendo el archivo cdl...");
                    ConvertFile(path);
                    Console.WriteLine("Escribiendo el archivo csv..." + path + ".csv");
                    WriteCsv(path + ".csv");
                    Console.WriteLine("Archivo convertido!");
                }
                else
                {
                    Console.WriteLine("The file " + path + " doesn't exist.");
                }
            }
        }

        private static bool indimensions = false;
        private static Dictionary<String, List<String>> dimensions = new Dictionary<string, List<string>>();
        private static Dictionary<String, List<String>> vars = new Dictionary<string, List<string>>();

        static void ConvertFile(String path)
        {

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(path))
                {
                    String line = sr.ReadLine();

                    if (!line.StartsWith("netcdf")) throw new Exception("Format error");

                    while ((line = sr.ReadLine().Trim().ToLowerInvariant()) != "data:")
                    {
                        if (line == "dimensions:") indimensions = true;
                        else if (line == "variables:") indimensions = false;
                        else if (indimensions)
                        {
                            if (line.Contains('=') && line.Contains(";"))
                            {
                                var dimname = line.Split('=').First().Trim();
                                dimensions.Add(dimname, new List<string>());
                                Console.WriteLine("Dimension detectada: " + dimname);
                            }
                            else throw new Exception("Format error on dimensions");
                        }
                    }

                    Console.WriteLine(dimensions.Count + " dimensiones detectadas.");

                    while (sr.ReadLine().Trim().ToLowerInvariant() != "}")
                    {
                        LoadVar(sr);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        static void WriteCsv(String outpath)
        {
            using (StreamWriter sw = new StreamWriter(outpath))
            {
                // Escribimos las cabeceras
                sw.WriteLine(String.Join(",", dimensions.Keys) + "," + String.Join(",", vars.Keys));

                List<String> scomb = Combinations(dimensions);
                Console.WriteLine("Se van a escribir " + scomb.Count + " registros.");

                // Escribimos los datos
                for (var index = 0; index < scomb.Count; index++)
                {
                    var line = scomb[index];

                    foreach (var varArray in vars.Values)
                    {
                        if (index >= varArray.Count)
                        {
                            line += ",?";
                        }
                        else
                        {
                            if (varArray[index] == "_")
                            {
                                line += ",?";
                            }
                            else
                            {
                                line += "," + varArray[index];
                            }
                        }
                    }

                    if (index % (scomb.Count / 10) == 0 && index > 0) { Console.WriteLine(((index / (scomb.Count / 10))*10)+ "%"); }

                    sw.WriteLine(line);
                }
            }
        }

        static List<String> Combinations(Dictionary<String, List<String>> dic)
        {
            if (dic.Count == 1)
            {
                return dic.First().Value;
            }
            else
            {
                List<String> ret = new List<string>();
                var dicFirst = dic.First();
                dic.Remove(dicFirst.Key);
                var dicRest = Combinations(dic);

                foreach (var felem in dicFirst.Value)
                {
                    foreach (var selem in dicRest)
                    {
                        ret.Add(felem + "," + selem);
                    }
                }

                return ret;
            }
        }

        static void LoadVar(StreamReader sr)
        {
            String varname = String.Empty;
            Boolean varloaded = false;
            List<String> values = new List<string>();
            Boolean finnished = false;
            String currentLine = null;

            while (!finnished)
            {
                currentLine = sr.ReadLine();

                if (!varloaded && currentLine.Contains('='))
                {
                    varname = currentLine.Split('=').First().Trim().ToLowerInvariant();
                    varloaded = true;
                    currentLine = currentLine.Split('=').Skip(1).First();
                }

                if (currentLine.Contains(';'))
                {
                    currentLine = currentLine.Split(';').First();
                    finnished = true;
                }

                values.AddRange(currentLine
                    .Split(',')
                    .Select((elem) => elem.Trim())
                    .Where((elem) => elem != String.Empty)
                    .ToList());
            }

            if (dimensions.ContainsKey(varname))
            {
                dimensions[varname] = values;
            }
            else
            {
                vars.Add(varname, values);
            }
        }
    }
}
