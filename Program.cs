using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DB2StructToC__
{
    class Program
    {
        static void Main(string[] args)
        {
            ConvertCPP dbStruct = new ConvertCPP("ItemDisplayInfo.txt");
            dbStruct.Convert();
        }
    }

    /// <summary>
    /// Read the structure, according to WoWDBDefs struct and fill two list, one who contains field name and the another one contains the field type. After that, create a C++ file with db2 struct in c
    /// </summary>
    public class ConvertCPP
    {
        private Regex typePattern = new Regex(@"(\<[A-z0-9]{1,}\>)");

        private List<string> db2FieldName = new List<string>();

        private List<string> db2FieldType = new List<string>();

        private StreamWriter cppWriter;

        private Stopwatch watch = new Stopwatch();
        private string Filename { get;}

        private string HotfixRequest { get; }
        private string StructName { get; }

        public ConvertCPP(string file)
        {
            string userInput = null;

            Filename = file;

            Console.Write("Specify Hotfix Request : ");
            userInput = Console.ReadLine();
            HotfixRequest = userInput;

            Console.Write("Specify Structure Name (default is the filename without the extension) : ");
            userInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(userInput) && !string.IsNullOrEmpty(userInput))
                this.StructName = userInput;
            else
                this.StructName = file;
                
        }

        public void Convert()
        {
            Read();
            Write();
        }

        #region Read
        /// <summary>
        /// Read, format & fill List.
        /// </summary>
        /// <param name="file">The file who contain the struct.</param>
        private void Read()
        {

            watch.Start();
            string[] lineReader = File.ReadAllLines(Filename);

            for (int i = 0; i < lineReader.Length; i++)
            {
                Match mBracketPattern = Regex.Match(lineReader[i], @"(\[[0-9]*\])");

                // -- first line of the file.
                if (lineReader[i].Contains("$noninline,id$"))
                    lineReader[i] = lineReader[i].Replace("$noninline,id$", string.Empty).Trim();

                if (lineReader[i].Contains('$'))
                    lineReader[i] = lineReader[i].Replace("$", string.Empty).Trim();

                if (lineReader[i].Contains('<') && lineReader[i].Contains('>') || lineReader[i].Contains('[') || lineReader[i].Contains(']') || lineReader[i].Contains(string.Empty) || db2FieldType[i].Any(char.IsDigit))
                {
                    if (!lineReader[i].Contains('[') && !lineReader[i].Contains(']'))
                    {
                        db2FieldName.Add(lineReader[i]);

                    }
                    if (mBracketPattern.Success)
                    {
                        if (lineReader[i].Contains('[') && lineReader[i].Contains(']'))
                        {
                            int bracketIndexStart = lineReader[i].IndexOf('[');

                            ushort occurence = ushort.Parse(lineReader[i].Substring(bracketIndexStart).Replace("[", string.Empty).Replace("]", string.Empty).Trim());

                            lineReader[i] = Regex.Replace(lineReader[i], @"(\[[0-9]{1,}\])", string.Empty);

                            if (lineReader[i].Contains("Field"))
                                lineReader[i] = lineReader[i] + "_";

                            for (ushort j = 1; j <= occurence; j++)
                            {
                                db2FieldName.Add(lineReader[i] + $"{j}");
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < db2FieldName.Count; i++)
            {
                Match mTypePattern = Regex.Match(db2FieldName[i], typePattern.ToString());

                if (mTypePattern.Success)
                {
                    db2FieldType.Add(mTypePattern.Value);

                    db2FieldName[i] = Regex.Replace(db2FieldName[i], typePattern.ToString(), string.Empty);
                }
                else
                    db2FieldType.Add("STRING|FLOAT?");
            }

            watch.Stop();

            Console.WriteLine($"List Field/Type completed with {db2FieldName.Count} entry !. Reading finished in {watch.ElapsedMilliseconds / 1000} seconds.");
        }
        #endregion

        #region Write

        /// <summary>
        /// Write the structure
        /// </summary>
        private void Write()
        {
            watch.Start();

            string output = $"struct {StructName}LoadInfo\n" + "{\n     static DB2LoadInfo const* Instance()\n      {\n         static DB2FieldMeta const fields[] =\n          {\n";

            Console.WriteLine("Translating shit into C++");

            for (int i = 0; i < db2FieldType.Count; i++)
            {
                output += "             {" + $"{TypeToBoolean(db2FieldType[i]).ToString().ToLower()}, {GetCorrectType(db2FieldType[i], db2FieldName[i])}, {db2FieldName[i]}" + "},\n";
            }

            output += "          };\n static DB2LoadInfo const loadInfo($fields[0], std::extent<decltype(fields)>::value," + $"{StructName}::Instance(), {HotfixRequest})\n return $loadInfo;";

            using (cppWriter = new StreamWriter("output.cpp", false))
            {
                cppWriter.WriteLine(output);
            }

            
            Console.Clear();
            watch.Stop();
            Console.WriteLine($"Struct was written in \"output.cpp\" !\n\nTask finished in {watch.ElapsedMilliseconds / 1000} seconds.");
        }
        #endregion

        #region Method/Helper

        /// <summary>
        /// Convert type to boolean.
        /// </summary>
        /// <param name="type">Type. Can be u32, 32. If nothing is specified, the method return false because the type is probably a string or a float.</param>
        /// <returns>True or false, according to the type.</returns>
        private  bool TypeToBoolean(string type)
        {
            if (type.Contains('u'))
                return false;
            else if (type.Contains("STRING|FLOAT?"))
                return false;
            else
                return true;
        }

        /// <summary>
        /// This method change the current type (u32, 32, etc...) to C++ type according to the struct of DB2LoadInfo.h (cf. TrinityCore)
        /// </summary>
        /// <param name="type">Type, can be u32, 32, etc... If the type is not recognized, the program ask you to specify the type manually.</param>
        /// <returns>Return type in C++</returns>

        private  string GetCorrectType(string type, string field)
        {
            if (type.Contains("32"))
                return "FT_INT";
            else if (type.Contains("16"))
                return "FT_SHORT";
            else if (type.Contains("8"))
                return "FT_BYTE";
            else if (type.Contains("STRING|FLOAT?"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"/!\\ Warning! Can't determine if this field \"{field}\" is a FT_STRING or FT_FLOAT. Please, specify it manually. Field type ?  :  ");
                Console.ForegroundColor = ConsoleColor.White;
                return Console.ReadLine();
            }


            return null;
        }
        #endregion
    }

}
