using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cdf2csv
{
    class CDFFile
    {
        private Dictionary<String, List<String>> dimensions = new Dictionary<string, List<string>>();
        private Dictionary<String, List<String>> vars = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// Save the CDF data in memory to a CSV file
        /// </summary>
        /// <param name="outpath"></param>
        public void SaveCSV(String outpath)
        {
            using (StreamWriter sw = new StreamWriter(outpath))
            {
                // Writing column headers
                sw.WriteLine(String.Join(",", dimensions.Keys) + "," + String.Join(",", vars.Keys));

                List<String> scomb = Combinations(dimensions);

                // Writing data
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

                    if (index % (scomb.Count / 10) == 0 && index > 0) { Console.Write(((index / (scomb.Count / 10)) * 10) + "% "); } // TODO: Remove from here

                    sw.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Generate all header combinations
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private static List<String> Combinations(Dictionary<String, List<String>> dic)
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

        /// <summary>
        /// Load the cdf file in memory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CDFFile Load(String path)
        {
            var ret = new CDFFile();

            using (StreamReader sr = new StreamReader(path))
            {
                String line = sr.ReadLine();
                bool indimensions = false;

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
                            ret.dimensions.Add(dimname, new List<string>());
                        }
                        else throw new Exception("Format error on dimensions");
                    }
                }

                while (sr.ReadLine().Trim().ToLowerInvariant() != "}")
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

                    if (ret.dimensions.ContainsKey(varname))
                    {
                        ret.dimensions[varname] = values;
                    }
                    else
                    {
                        ret.vars.Add(varname, values);
                    }
                }
            }

            return ret;
        }
    }
}
