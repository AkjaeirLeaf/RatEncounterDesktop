using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Kirali.Framework;
using Kirali.MathR;

namespace KiraliMauvaModelAdapter.IO
{
    public class FBXStructure
    {
        public string StructureIdent;
        public ClumpValue[] HeadContent = new ClumpValue[0];
        public string[] NotesCollection = new string[0];
        public string AddressString;
        public int[] AddressInt = new int[0];
        public ClumpContentLine[] Contents = new ClumpContentLine[0];
        public FBXStructure linkParent = null;
        public FBXStructure[] children = new FBXStructure[0];
    }

    // OBJECTS AND GEOMETRY (major 5)
    public struct NodeAttributeCollection
    {
        public int NodeLinkID;
        public string internalID;
        public string Category;

        public bool isEmpty;

        public string TypeFlags;

        public double Size;

    }
    public struct ModelCollection
    {
        public int NodeLinkID;
        public string ModelID;
        public string Category;

        //public ClumpContentLine[] Properties70;
        // ex:
        //  P: "Size", "double", "Number", "",3.30000004917383
        //  P: "InheritType", "enum", "", "",1
        //  P: "DefaultAttributeIndex", "int", "Integer", "",0
        //	P: "Lcl Translation", "Lcl Translation", "", "A+",8.16287517547607,-4.50417852401733,-2.94154691696167
        //	P: "Lcl Rotation", "Lcl Rotation", "", "A+",-9.55710548387355,89.999995674289,0
        //	P: "Lcl Scaling", "Lcl Scaling", "", "A+",0.99999988079071,0.99999988079071,0.999999821186066

        public bool HasProperties;

        public int Version;
        public int InheritType;
        public int DefaultAttribIndex;
        public bool useL_Translation; public Vector3 LCL_Translation;
        public bool useL_Rotation; public Vector3 LCL_Rotation;
        public bool useL_Scaling; public Vector3 LCL_Scaling;

        public bool Culling;
    }
    public struct PoseData
    {
        // head
        public int NodeLinkID;
        public string PoseName;
        public string Category;

        // body
        public string Type;
        public int Version;
        public int NbPoseNodes;

        // children
        public PoseNode[] Nodes;
    }
    public struct PoseNode
    {
        // body
        public int NodeLinkID;

        // child (singular)
        public double[] pose_matrix; // always 16 long.
    }
    public struct DeformerNode
    {
        // header
        public int NodeLinkID;
        public string DeformerName;
        public string DeformerType;

        // body
        public int Version;
        public int Link_DeformAccuracy;
    }
    public struct SubdeformerNode
    {
        // header
        public int NodeLinkID;
        public string DeformerName;
        public string DeformerType;

        // body
        public int Version;
        public string[] UserData;

        public bool[] HasProperties; // always four long for: indexes, weights, transform, translink

        public int[] Indexes;
        public double[] Weights;

        public double[] Transform;     // 16 long
        public double[] TransformLink; // 16 long
    }

    // AnimationStack: 80598536, "AnimStack::ratbuddy|IK_Handles|Take 001|BaseLayer", "" {
    //     Properties70:  {
    //     		P: "LocalStop", "KTime", "Time", "",119314241500
    //     		P: "ReferenceStop", "KTime", "Time", "",119314241500
    //     }
    // }
    public struct AnimationStack
    {
        public int NodeLinkID;
        /// <summary>
        /// <tooltip>Contains: model, category, take, layer</tooltip>
        /// </summary>
        public string[] AnimStackHeader; // length of four.
        public int LocalStop;
        public int ReferenceStop;
    }

    //  AnimationLayer: 80851960, "AnimLayer::ratbuddy|IK_Handles|Take 001|BaseLayer", "" {
    //  }
    public struct AnimationLayer
    {
        public int NodeLinkID;
        /// <summary>
        /// <tooltip>Contains: model, category, take, layer</tooltip>
        /// </summary>
        public string[] AnimStackHeader; // length of four.
    }

    // AnimationCurveNode: 80283352, "AnimCurveNode::T", "" {
    //  	Properties70:  {
    //  		P: "d|X", "Number", "", "A",-0.363200694322586
    //  		P: "d|Y", "Number", "", "A",0.604769706726074
    //  		P: "d|Z", "Number", "", "A",-0.277375042438507
    //  	}
    // }
    public struct AnimationCurveNode
    {
        public int NodeLinkID;
        public string TRS;
        public Vector3 Vector;
    }

    //  AnimationCurve: 82430808, "AnimCurve::", "" {
    //  	Default: -1.55057603024034e-006
    //  	KeyVer: 4008
    //  	KeyTime: *63 {
    //  		a: 0,1924423250,3848846500,5773269750,7697693000,9622116250,11546539500,13470962750,15395386000,17319809250,19244232500,21168655750,23093079000,25017502250,26941925500,28866348750,30790772000,32715195250,34639618500,36564041750,38488465000,40412888250,42337311500,44261734750,46186158000,48110581250,50035004500,51959427750,53883851000,55808274250,57732697500,59657120750,61581544000,63505967250,65430390500,67354813750,69279237000,71203660250,73128083500,75052506750,76976930000,78901353250,80825776500,82750199750,84674623000,86599046250,88523469500,90447892750,92372316000,94296739250,96221162500,98145585750,100070009000,101994432250,103918855500,105843278750,107767702000,109692125250,111616548500,113540971750,115465395000,117389818250,119314241500
    //  	} 
    //  	KeyValueFloat: *63 {
    //  		a: -1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006,-1.550576e-006
    //  	} 
    //  	;KeyAttrFlags: Linear
    //  	KeyAttrFlags: *1 {
    //  		a: 24836
    //  	} 
    //  	;KeyAttrDataFloat: RightAuto:0, NextLeftAuto:0
    //  	KeyAttrDataFloat: *4 {
    //  		a: 0,0,255790911,0
    //  	} 
    //  	KeyAttrRefCount: *1 {
    //  		a: 63
    //  	} 
    //  }
    public struct AnimationCurve
    {
        // head
        public int NodeLinkID;

        // body
        public double Default;
        public int KeyVer;
        public int[] KeyTime;

        // comment in body
        public string[] KeyAttrFlag_Names;
        public string[] KeyAttrDataFloat_Names;

        // body again
        public int[] KeyAttrFlags;
        public int[] KeyAttrDataFloat;
        public int[] KeyAttrRefCount;

    }


    // CONNECTIONS (major 6)
    public struct ConnectionNode
    {
        // from body comments
        //;NodeAttribute::ratbuddy, Model::ratbuddy
        string connectA_category;
        string connectB_category;
        string A_identifier;
        string B_identifier;

        // content
        //C: "OO",77721240,77812968
        string connectType;
        int A_NodeLinkID;
        int B_NodeLinkID;
        string suffix; // OPTIONAL
        bool hasSuffix;
    }

    // TAKES (major 7)

    // Current: ""
    // Take: "ratbuddy|IK_Handles|Take 001|BaseLayer" {
    //      FileName: "ratbuddy|IK_Handles|Take_001|BaseLayer.tak"
    //      	LocalTime: 0,119314241500
    //      	ReferenceTime: 0,119314241500
    // }
    public struct TakeNode
    {
        public string[] Name; // always four long,
        public string File; // same as Name but spaces are underscores and ends with .tak
        public int LocalStart; public int LocalEnd;
        public int ReferenceStart; public int ReferenceEnd;
    }


    // CLUMP CONTENT
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
