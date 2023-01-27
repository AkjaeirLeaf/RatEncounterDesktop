using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Kirali.Framework;

namespace KiraliMauvaModelAdapter.IO
{
    public class FBXStructure
    {
        public string StructureIdent;
        public ClumpValue[] HeadContent = new ClumpValue[0];
        public string AddressString;
        public int[] AddressInt = new int[0];
        public ClumpContentLine[] Contents = new ClumpContentLine[0];
        public FBXStructure linkParent = null;
        public FBXStructure[] children = new FBXStructure[0];
    }

    public struct ClumpContentLine
    {
        public string Ident;
        public ClumpValue[] Value; // either one val or broken by comma.
    }

    public struct ClumpValue
    {
        public object Data;
        public bool isDataArray;
        public ClumpDataType dataType;
    }

    public enum ClumpDataType
    {
        unknown,
        cd_char,
        cd_string,
        cd_int,
        cd_double,
        
    }
}
