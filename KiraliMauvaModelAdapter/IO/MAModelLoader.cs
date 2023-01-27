using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Kirali.Framework;

namespace KiraliMauvaModelAdapter.IO
{
    public class MAModelLoader
    {
        // Maya ASCII 2022 scene
        // Name: ratbuddy_no_anim.ma
        // Last modified: Thu, Jan 26, 2023 03:12:23 PM
        // Codeset: UTF-8

        public string SceneType;
        public string SceneName;
        public string LastModify;
        public string Codeset;

        public string[] MeshRequires;

        public string SceneUnits;

        public string ExportedFrom;
        public string Application;
        public string Product;
        public string EditorVersion;
        public string CutIdentifier;
        public string OperSystem;
        public string Model_UUID;


        public string[] UnboundFileDataTags;
        public string[] UnboundFileDataData;


        public MAModelLoader(string filepath)
        {
            if (File.Exists(filepath))
            {
                FileStream stream = new FileStream(filepath, FileMode.Open);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    string remaining = content;
                    
                    // COMMENTS
                    string[] comments = GetCommentsHeader(remaining, out remaining);
                    SceneType  = comments[0];
                    SceneName  = comments[1].Remove(0, 6);
                    LastModify = comments[2].Replace("Last modified: ", "");
                    Codeset    = comments[3].Replace("Codeset: ", "");

                    // REQUIREMENTS
                    string[] requirements = GetRequirementsHeader(remaining, out remaining);
                    MeshRequires = requirements;

                    // UNIT
                    // currentUnit -l centimeter -a degree -t film;
                    string units = GetUnitsInfoHeader(remaining, out remaining);

                    // FILE INFO
                    string[] file_info = GetFileInfoHeader(remaining, out remaining);
                    for(int ix = 0; ix < file_info.Length; ix++)
                    {
                        string[] parse = ParseProperties(file_info[ix]);
                        switch (parse[0])
                        {
                            case "exportedFrom":
                                ExportedFrom = parse[1];
                                break;
                            case "application":
                                Application = parse[1];
                                break;
                            case "product":
                                Product = parse[1];
                                break;
                            case "version":
                                EditorVersion = parse[1];
                                break;
                            case "cutIdentifier":
                                CutIdentifier = parse[1];
                                break;
                            case "osv":
                                OperSystem = parse[1];
                                break;
                            case "UUID":
                                Model_UUID = parse[1];
                                break;
                            default:
                                UnboundFileDataTags = ArrayHandler.append(UnboundFileDataTags, parse[0]);
                                UnboundFileDataData = ArrayHandler.append(UnboundFileDataData, parse[1]);
                                break;
                        }
                    }

                    // Begin Nodes
                    string[] content_lines = remaining.Split('\n');
                }
            }
        }

        private static string[] GetCommentsHeader(string content, out string remaining)
        {
            string[] lines = content.Split('\n');
            string[] comments = new string[4]; int count = 0;
            remaining = content;
            for(int ix = 0; ix < lines.Length; ix++)
            {
                if(!String.IsNullOrEmpty(lines[ix]) && lines[ix].Length >= 2)
                {
                    if (lines[ix][0] == '/' && lines[ix][1] == '/')
                    {
                        //is comment line
                        comments[count] = lines[ix].Remove(0, 2);
                        remaining = remaining.Remove(0, lines[ix].Length + 1);
                        count++; if (count == 4) { return comments; }
                    }
                    else if (count == 0) { remaining = remaining.Remove(0, lines[0].Length + 1); }
                }
            }
            return comments;
        }
        private static string[] GetRequirementsHeader(string content, out string remaining)
        {
            string[] lines = content.Split(';');
            string[] requirements = new string[0]; int count = 0;
            remaining = content;
            for (int ix = 0; ix < lines.Length; ix++)
            {
                if (!String.IsNullOrEmpty(lines[ix]))
                {
                    if (lines[ix][0] == '\n') { lines[ix] = lines[ix].Remove(0, 1); }
                    if (lines[ix].Contains("requires ") && lines[ix].Substring(0, 9) == "requires ")
                    {
                        //is req line
                        string c; ReadAfterUntil((lines[ix] + ';').ToCharArray(), ' ', ';', out c);
                        requirements = ArrayHandler.append(requirements, c);
                        remaining = remaining.Remove(0, lines[ix].Length + 2);
                        count++;
                    }
                    
                    else if(count > 1 && requirements.Length >= 1) { return requirements; }
                }
            }
            return requirements;
        }
        private static string GetUnitsInfoHeader(string content, out string remaining)
        {
            string[] lines = content.Split('\n');
            string[] units = new string[0];
            remaining = content;
            for (int ix = 0; ix < lines.Length; ix++)
            {
                if (!String.IsNullOrEmpty(lines[ix]))
                {
                    if (lines[ix].Contains("currentUnit ") && lines[ix].Substring(0, 12) == "currentUnit ")
                    {
                        //is req line
                        string c; ReadAfterUntil((lines[ix]).ToCharArray(), ' ', ';', out c);
                        units = ArrayHandler.append(units, c);
                        remaining = remaining.Remove(0, lines[ix].Length + 1);
                        return units[0];
                    }
                    //else if (count == 0) { remaining = remaining.Remove(0, lines[0].Length + 1); }
                }
            }
            return "";
        }
        private static string[] GetFileInfoHeader(string content, out string remaining)
        {
            string[] lines = content.Split('\n');
            string[] f_info = new string[0]; int count = 0;
            remaining = content;
            for (int ix = 0; ix < lines.Length; ix++)
            {
                if (!String.IsNullOrEmpty(lines[ix]))
                {
                    if (lines[ix].Contains("fileInfo ") && lines[ix].Substring(0, 9) == "fileInfo ")
                    {
                        //is req line
                        string c; ReadAfterUntil((lines[ix]).ToCharArray(), ' ', ';', out c);
                        f_info = ArrayHandler.append(f_info, c);
                        remaining = remaining.Remove(0, lines[ix].Length + 1);
                        count++;
                    }
                    else if (count == 0) { remaining = remaining.Remove(0, lines[0].Length + 1); }
                    else if (count > 1 && f_info.Length >= 1)
                    {
                        return f_info; }
                    
                }
            }
            return f_info;
        }
        private static string[] ParseProperties(string str)
        {
            string[] str_breakat = str.Split('\"');
            string[] props = new string[0];
            for(int ix = 0; ix < str_breakat.Length; ix++)
            {
                if (!String.IsNullOrEmpty(str_breakat[ix]) && str_breakat[ix] != " " && str_breakat[ix] != ":")
                { props = ArrayHandler.append(props, str_breakat[ix]); }
            }
            return props;
        }
        private static string ReadUntil(string line, char stop)
        {
            string build = "";
            char[] array = line.ToCharArray();
            if (line.Length == 0) { return ""; }
            else
            {
                for (int ix = 0; ix < array.Length; ix++)
                {
                    if (array[ix] == stop) { break; }
                    else { build += array[ix]; }
                }
                return build;
            }
        }
        private static bool ReadAfterUntil(char[] line, char begin, char end, out string result, int encounter = 0)
        {
            // here I will not add in the start/end
            string build = "";
            bool isReading = false;
            bool doneReading = false;
            int en_counter = 0; // how many times to start/end seach before reading...
            bool in_Encounter = false;

            bool roundPerformAction = false;

            for (int ix = 0; ix < line.Length; ix++)
            {
                roundPerformAction = false; // rpa makes sure that we don't open a new encounter on the same character as we close one, without making an ioobe

                if ((in_Encounter || isReading) && !doneReading && line[ix] == end)
                {
                    // end reading or encounter.
                    if (encounter == en_counter)
                    {
                        isReading = false; in_Encounter = false; doneReading = true;
                        break;
                    }
                    else
                    {
                        en_counter++;
                    }
                    in_Encounter = false;
                    roundPerformAction = true;
                }
                if (isReading && !doneReading)
                {
                    // read.
                    build += line[ix];
                }
                if (!isReading && !doneReading && line[ix] == begin && !roundPerformAction)
                {
                    // start reading, or only increase en_counter.
                    if (encounter == en_counter)
                    {
                        isReading = true;
                    }
                    in_Encounter = true;
                    roundPerformAction = true;
                }
            }

            if (doneReading)
            {
                result = build;
                return true;
            }

            result = "";
            return false;
        }
    }
}
