using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Kirali.Framework;

namespace KiraliMauvaModelAdapter.IO
{
    public class AsciiFBXReader
    {
        public string[] NotesCollection   = new string[0];
        public int[]    NotesLinePosition = new int[0];

        public FBXStructure[] FileContents = new FBXStructure[0];
        private FBXStructure CurrentStructure = null;
        private int[] CurrentStructureAddress = new int[0];

        public AsciiFBXReader(string filepath)
        {
            if (File.Exists(filepath))
            {
                FileStream stream = new FileStream(filepath, FileMode.Open);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string[] content = reader.ReadToEnd().Split('\n');
                    for (int ix = 0; ix < content.Length; ix++)
                    {
                        content[ix] = RemoveTrailingSpace(RemoveLeadingSpace(content[ix]));
                        if(content[ix].Length > 0)
                        {
                            if(content[ix][0] == ';')
                            {
                                // is comment line.
                                NotesCollection = ArrayHandler.append(NotesCollection, content[ix]);
                                NotesLinePosition = ArrayHandler.append(NotesLinePosition, ix);
                            }
                            else
                            {
                                // not a comment line
                                if (content[ix].Contains('{') || content[ix].Contains('}'))
                                {
                                    // is structure open/close line!
                                    if(content[ix] != "}")
                                    {
                                        // is an opening structure line!
                                        // open a new structure with the new structure tag!
                                        string StrucName = ReadUntil(content[ix], ':');
                                        ClumpValue[] HeadCont = new ClumpValue[0];
                                        ReadAfterUntil(content[ix].ToCharArray(), ':', '{', out string param);
                                        if (!IsBlankSpace(param))
                                        {
                                            // param section does have content -
                                            HeadCont = GetClumpValue(param);
                                        }

                                        // advance
                                        AdvanceStructure(StrucName, HeadCont);
                                    }
                                    else
                                    {
                                        // is a closing stucture line!
                                        // close structure and recess structure address
                                        RecessStructure();
                                    }
                                }
                                else
                                {
                                    // line is an info line!
                                    ClumpContentLine clump = new ClumpContentLine();
                                    clump.Ident = ReadUntil(content[ix], ':');
                                    clump.Value = GetClumpValue(ReadAfter(content[ix], ':'));

                                    // add content to current structure.
                                    CurrentStructure.Contents = ArrayHandler.append(CurrentStructure.Contents, clump);
                                }
                            }
                        }
                        // else empty line :'(
                    }
                    
                }
            }
        }


        private void AdvanceStructure(string ident, ClumpValue[] HeadCont)
        {
            FBXStructure newstruc = new FBXStructure();
            newstruc.StructureIdent = ident;
            newstruc.HeadContent = HeadCont;
            BumpForwardAddress();
            newstruc.AddressInt = CurrentStructureAddress;
            if(CurrentStructure != null)
            {
                newstruc.linkParent = CurrentStructure;
                newstruc.AddressString = CurrentStructure.AddressString + "." + ident;
                CurrentStructure.children = ArrayHandler.append(CurrentStructure.children, newstruc);
            }
            else
            {
                newstruc.linkParent = null;
                newstruc.AddressString = ident;
                FileContents = ArrayHandler.append(FileContents, newstruc);
                CurrentStructureAddress = new int[] { FileContents.Length - 1 };
            }
            UpdateCurrentStructure();
        }

        private void RecessStructure()
        {
            BumpBackAddress();
            UpdateCurrentStructure();
        }

        private void UpdateCurrentStructure()
        {
            if (CurrentStructureAddress.Length == 0)
            {
                CurrentStructure = null;
            }
            else if(CurrentStructureAddress.Length == 1 && FileContents.Length >= 1)
            {
                if(CurrentStructureAddress[0] < FileContents.Length)
                {
                    CurrentStructure = FileContents[CurrentStructureAddress[0]];
                }
            }
            else if(CurrentStructureAddress.Length > 1 && FileContents.Length >= 1)
            {
                FBXStructure hold = new FBXStructure();
                for(int ix = 0; ix < CurrentStructureAddress.Length; ix++)
                {
                    if (ix == 0 && CurrentStructureAddress[ix] < FileContents.Length)
                    { hold = FileContents[CurrentStructureAddress[0]]; }
                    else if (CurrentStructureAddress[ix] < hold.children.Length)
                    {
                        hold = hold.children[CurrentStructureAddress[ix]];
                    }
                    else
                    {
                        // ERROR!!!
                        CurrentStructure = new FBXStructure();
                        break;
                    }
                }
                CurrentStructure = hold;
            }
            else
            {
                CurrentStructure = null;
                CurrentStructureAddress = new int[0];
            }
        }

        private void BumpForwardAddress()
        {
            int[] Bumped;
            if(CurrentStructureAddress.Length == 0)
            {
                Bumped = new int[] { 0 };
            }
            else
            {
                Bumped = new int[CurrentStructureAddress.Length + 1];
                for (int ix = 0; ix < CurrentStructureAddress.Length; ix++)
                {
                    Bumped[ix] = CurrentStructureAddress[ix];
                }
                Bumped[CurrentStructureAddress.Length] = CurrentStructure.children.Length;
            }
            CurrentStructureAddress = Bumped;
        }

        private void BumpBackAddress()
        {
            if(CurrentStructureAddress.Length == 1)
            {
                CurrentStructureAddress = new int[0];
            }
            else
            {
                int[] Bumped = new int[CurrentStructureAddress.Length - 1];
                for (int ix = 0; ix < Bumped.Length; ix++)
                {
                    Bumped[ix] = CurrentStructureAddress[ix];
                }
                CurrentStructureAddress = Bumped;
            }
        }

        private static ClumpValue[] GetClumpValue(string clump_contents)
        {
            // create uninit'd array
            ClumpValue[] vals;

            // break data if needed
            string[] st_br; bool hasMultipleValues = false;
            if (clump_contents.Contains(','))
            {
                st_br = clump_contents.Split(','); hasMultipleValues = true;
            }
            else { st_br = new string[] { clump_contents }; }

            ClumpDataType[] list_dat_types = new ClumpDataType[st_br.Length];

            ClumpDataType[] type_collapse = new ClumpDataType[0];
            int[] typecounter = new int[0];
            int current_tc = 0;

            // use type identifier to list the types in the array
            for(int ix = 0; ix < list_dat_types.Length; ix++)
            {
                list_dat_types[ix] = IdentifyStringData(st_br[ix]);
                if(ix == 0) { current_tc++; }
                else if(list_dat_types[ix - 1] == list_dat_types[ix])
                { current_tc++; }
                else
                {
                    // current type is not equal to that of the last!
                    type_collapse = ArrayHandler.append(type_collapse, list_dat_types[ix-1]);
                    typecounter = ArrayHandler.append(typecounter, current_tc);
                    current_tc = 1; // restart at 1 for the new type.
                }
            }
            // append last int.
            typecounter = ArrayHandler.append(typecounter, current_tc);
            type_collapse = ArrayHandler.append(type_collapse, list_dat_types[list_dat_types.Length - 1]);

            // nested FOR loop
            int absolute = 0;
            vals = new ClumpValue[typecounter.Length];
            for(int ix = 0; ix < vals.Length; ix++)
            {
                vals[ix] = new ClumpValue();
                if (typecounter[ix] > 1) { vals[ix].isDataArray = true; }
                else { vals[ix].isDataArray = false; }
                vals[ix].dataType = type_collapse[ix];

                object[] curr_obj     = new object[typecounter[ix]];
                char[] curr_char      = new   char[typecounter[ix]];
                string[] curr_string  = new string[typecounter[ix]];
                int[] curr_int        = new    int[typecounter[ix]];
                double[] curr_double  = new double[typecounter[ix]];

                for (int iy = 0; iy < typecounter[ix]; iy++)
                {
                    switch (vals[ix].dataType)
                    {
                        case ClumpDataType.unknown:
                            curr_obj[iy] = (object)st_br[absolute];
                            break;
                        case ClumpDataType.cd_char:
                            ReadAfterUntil(st_br[absolute].ToCharArray(), '\'', '\'', out string strchar);
                            if(strchar.Length > 0) { curr_char[iy] = strchar[0]; }
                            else { curr_char[iy] = ' '; }
                            break;
                        case ClumpDataType.cd_string:
                            ReadAfterUntil(st_br[absolute].ToCharArray(), '\"', '\"', out string strstring);
                            if (strstring.Length > 0) { curr_string[iy] = strstring; }
                            else { curr_string[iy] = ""; }
                            break;
                        case ClumpDataType.cd_int:
                            Int32.TryParse(st_br[absolute], out int strint);
                            curr_int[iy] = strint;
                            break;
                        case ClumpDataType.cd_double:
                            Double.TryParse(st_br[absolute], out double strdub);
                            curr_double[iy] = strdub;
                            break;
                    }
                    absolute++;
                }

                // set obj vals
                switch (vals[ix].dataType)
                {
                    case ClumpDataType.unknown:
                        vals[ix].Data = curr_obj;
                        break;
                    case ClumpDataType.cd_char:
                        vals[ix].Data = curr_char;
                        break;
                    case ClumpDataType.cd_string:
                        vals[ix].Data = curr_string;
                        break;
                    case ClumpDataType.cd_int:
                        vals[ix].Data = curr_int;
                        break;
                    case ClumpDataType.cd_double:
                        vals[ix].Data = curr_double;
                        break;
                }
            }

            return vals;
        }
        private static ClumpDataType IdentifyStringData(string dat)
        {
            ClumpDataType dattype = ClumpDataType.unknown;
            char[] clump_broken = dat.ToCharArray();

            bool has_doublequotepair = false;
            bool has_singlequotepair = false;
            bool has_digit       = false;
            bool has_decimal     = false;
            bool has_moredecimals     = false;
            bool has_contentoutsidequotes     = false;

            bool reader_indoublequote = false;
            bool reader_insinglequote = false;

            for (int ix = 0; ix < clump_broken.Length; ix++)
            {
                if (Char.IsDigit(clump_broken[ix])) { has_digit = true; }
                else if (Char.IsLetter(clump_broken[ix])) {  } // has letter
                else if (clump_broken[ix] == '\'' && !reader_insinglequote) { reader_insinglequote = true; }
                else if (clump_broken[ix] == '\"' && !reader_indoublequote) { reader_indoublequote = true; }
                else if (clump_broken[ix] == '\'' &&  reader_insinglequote) { reader_insinglequote = false; has_singlequotepair = true; }
                else if (clump_broken[ix] == '\"' &&  reader_indoublequote) { reader_indoublequote = false; has_doublequotepair = true; }
                else if (clump_broken[ix] == '.'  &&  !has_decimal) { has_decimal = true; }
                else if (clump_broken[ix] == '.'  &&   has_decimal) { has_moredecimals = true; }
                if (!Char.IsWhiteSpace(clump_broken[ix]) && clump_broken[ix] != '\"' && clump_broken[ix] != '\''
                    && !(reader_insinglequote || reader_indoublequote)) { has_contentoutsidequotes = true; }
            }

            if(has_doublequotepair && !has_contentoutsidequotes) { dattype = ClumpDataType.cd_string; }
            else if(has_singlequotepair && !has_contentoutsidequotes) { dattype = ClumpDataType.cd_char; }
            else if(has_digit &&  has_decimal && !has_moredecimals) { dattype = ClumpDataType.cd_double; }
            else if(has_digit && !has_decimal && !has_moredecimals) { dattype = ClumpDataType.cd_int; }

            return dattype;
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
        private static string ReadAfter(string line, char start)
        {
            string build = "";
            char[] array = line.ToCharArray();
            bool read = false;
            if (line.Length == 0) { return ""; }
            else
            {
                for (int ix = 0; ix < array.Length; ix++)
                {
                    if (read) { build += array[ix]; }
                    if (array[ix] == start) 
                    {
                        read = true; 
                    }
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
        private static string RemoveLeadingSpace(string line)
        {
            char[] charArray = line.ToCharArray();
            int removeCount = 0;
            for (int ix = 0; ix < charArray.Length; ix++)
            {
                // count spaces until char not match
                if (IsEqualAny(charArray[ix], new char[]
                { ' ', '\n', '\t', '\r' }
                )) { removeCount++; }
                else { break; }
            }
            if (removeCount > 0) { return line.Remove(0, removeCount); } else { return line; }
        }
        private static string RemoveTrailingSpace(string line)
        {
            char[] charArray = line.ToCharArray();
            int removeCount = 0;
            for (int ix = charArray.Length - 1; ix >= 0; ix--)
            {
                // count spaces until char not match
                if (IsEqualAny(charArray[ix], new char[]
                { ' ', '\n', '\t', '\r' }
                )) { removeCount++; }
                else { break; }
            }
            if (removeCount > 0) { return line.Remove(0, removeCount); } else { return line; }
        }
        private static bool IsEqualAny(char compare, char[] set)
        {
            for (int ix = 0; ix < set.Length; ix++)
            {
                if (set[ix] == compare) { return true; }
            }
            return false;
        }
        private static bool IsBlankSpace(string s)
        {
            if(s.Length >= 0)
            {
                char[] arr = s.ToCharArray();
                for (int ix = 0; ix < s.Length; ix++)
                {
                    if (!IsEqualAny(arr[ix], new char[] { ' ', '\n', '\r', '\t' } )) { return false; }
                }
            }
            return true;
        }
    }
}
