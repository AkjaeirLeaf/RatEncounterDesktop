using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RatEncounterDesktop.IO;
using RatEncounterDesktop.Render;

using Kirali.Framework;
using Kirali.MathR;

namespace KiraliMauvaModelAdapter.IO
{
    public class AsciiFBXReader
    {
        public string[] NotesCollection   = new string[0];
        public int[]    NotesLinePosition = new int[0];

        public FBXStructure[] FileContents = new FBXStructure[0];
        private FBXStructure CurrentStructure = null;
        private int[] CurrentStructureAddress = new int[0];

        public Object3D[] MeshCollection = new Object3D[0];
        public NodeAttributeCollection[] NodeAttributes = new NodeAttributeCollection[0];
        public ModelCollection[] ModelCollections = new ModelCollection[0];
        public PoseData[] PoseCollections = new PoseData[0];
        public DeformerNode[] DeformCollections = new DeformerNode[0];
        public SubdeformerNode[] SubdeformerCollections = new SubdeformerNode[0];

        public AsciiFBXReader(string filepath)
        {
            if (File.Exists(filepath))
            {
                FileStream stream = new FileStream(filepath, FileMode.Open);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string[] content = reader.ReadToEnd().Split('\n');
                    bool lastLineWasContent = false;
                    for (int ix = 0; ix < content.Length; ix++)
                    {
                        content[ix] = RemoveTrailingSpace(RemoveLeadingSpace(content[ix]));
                        if(content[ix].Length > 0)
                        {
                            if(content[ix][0] == ';')
                            {
                                // is comment line.
                                if(CurrentStructure != null)
                                {
                                    // add comment to structure.
                                    CurrentStructure.NotesCollection = 
                                        ArrayHandler.append(CurrentStructure.NotesCollection, content[ix]);
                                }
                                NotesCollection = ArrayHandler.append(NotesCollection, content[ix]);
                                NotesLinePosition = ArrayHandler.append(NotesLinePosition, ix);
                            }
                            else
                            {
                                // not a comment line
                                if (content[ix].Contains('{') || content[ix].Contains('}') || content[ix] == "} ")
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
                                    else if(content[ix] == "}" || content[ix] == "} " || content[ix] == " ")
                                    {
                                        // is a closing stucture line!
                                        // close structure and recess structure address
                                        RecessStructure();
                                    }
                                    lastLineWasContent = false;
                                }
                                else
                                {
                                    if(ContentEndsOnEmpty && lastLineWasContent)
                                    {
                                        // line is a CONTINUED content line!
                                        ClumpValue[] clumpValues = GetClumpValue(content[ix]);
                                        CurrentStructure.Contents[CurrentStructure.Contents.Length - 1].Value =
                                            MergeClumps(CurrentStructure.Contents[CurrentStructure.Contents.Length - 1].Value, clumpValues);
                                        lastLineWasContent = true;
                                    }
                                    else
                                    {
                                        // line is an info line!
                                        ClumpContentLine clump = new ClumpContentLine();
                                        clump.Ident = ReadUntil(content[ix], ':');
                                        clump.Value = GetClumpValue(ReadAfter(content[ix], ':'));

                                        // add content to current structure.
                                        CurrentStructure.Contents = ArrayHandler.append(CurrentStructure.Contents, clump);
                                        lastLineWasContent = true;
                                    }
                                }
                            }
                        }
                        // else empty line :'(
                    }
                    
                }
            }
        }

        public void CacheAllModelContents()
        {
            CollectGeometries(out FBXStructure[] geo_blocks);
            MeshCollection = new Object3D[geo_blocks.Length];
            for (int ix = 0; ix < MeshCollection.Length; ix++)
            {
                CompileObject(geo_blocks[ix], out MeshCollection[ix]);
            }

            CollectNodeAttrib(out NodeAttributes);
            CollectModelTransforms(out ModelCollections);
            CollectPoseData(out PoseCollections);
            CollectDeformerNodes(out DeformCollections, out SubdeformerCollections);
        }
        
        // COMPILE OBJECT3D
        public bool CompileObject(FBXStructure geoStruct, out Object3D obj)
        {
            obj = new Object3D();
            obj.Force_MeshFileSync(GetClumpValue<int>(geoStruct.HeadContent[0], 0));
            obj.Force_MeshID(GetClumpValue<string>(geoStruct.HeadContent[1], 0).Remove(0, "Geometry::".Length));
            
            geoStruct.children[0].Contents[0].Value = new ClumpValue[] {
                CollapseThreads(geoStruct.children[0].Contents[0].Value, out double[] vao) };

            geoStruct.children[1].Contents[0].Value = new ClumpValue[] {
                CollapseThreads(geoStruct.children[1].Contents[0].Value, out int[] vpa) };

            geoStruct.children[3].children[0].Contents[0].Value = new ClumpValue[] {
                CollapseThreads(geoStruct.children[3].children[0].Contents[0].Value, out double[] pnormal) };

            geoStruct.children[4].children[0].Contents[0].Value = new ClumpValue[] {
                CollapseThreads(geoStruct.children[4].children[0].Contents[0].Value, out double[] UV_obj) };

            geoStruct.children[4].children[1].Contents[0].Value = new ClumpValue[] {
                CollapseThreads(geoStruct.children[4].children[1].Contents[0].Value, out int[] UV_ptr) };

            if (vao.Length % 3 == 0 && vpa.Length % 3 == 0 && pnormal.Length == 3 * vpa.Length)
            {
                Vector3[] PointCache = Object3D.V3FromDoubleArray(vao);
                Vector3[] PointNorms = Object3D.V3FromDoubleArray(pnormal);
                Vector2[] UVMap      = Object3D.V2FromDoubleArray(UV_obj);
                obj.Force_PointCache(PointCache);
                obj.Force_PointPtr(vpa);
                obj.Force_PointNormal(PointNorms);
                obj.Force_UVMap(UVMap);
                obj.Force_UVPointers(UV_ptr);

                
            }

            return false;
        }
        
        // COLLECTING SECTION 5
        public bool CollectNodeAttrib(out NodeAttributeCollection[] result)
        {
            SetMajorSection(FBXMajorSection.Objects);
            FBXStructure[] res = SearchInCurrent("NodeAttribute");
            

            // COMPILE
            NodeAttributeCollection[] nacs = new NodeAttributeCollection[res.Length];
            for (int ix = 0; ix < res.Length; ix++)
            {
                NodeAttributeCollection nc = new NodeAttributeCollection();
                nc.NodeLinkID = GetClumpValue<int>(res[ix].HeadContent[0], 0);
                nc.internalID =      GetClumpValue<string>(res[ix].HeadContent[1], 0).Remove(0, "NodeAttribute::".Length);
                nc.Category   =      GetClumpValue<string>(res[ix].HeadContent[1], 1);
                
                if(res[ix].children.Length > 0)
                {
                    nc.isEmpty = false;
                    nc.Size = GetClumpValue<double>(res[ix].children[0].Contents[0].Value[1], 0);
                }
                else
                {
                    nc.isEmpty = true;
                }
                
                nc.TypeFlags   =     GetClumpValue<string>(res[ix].Contents[0].Value[0], 2);
                nacs[ix] = nc;
            }


            result = nacs;
            return true;
        }
        public bool CollectGeometries(out FBXStructure[] result)
        {
            SetMajorSection(FBXMajorSection.Objects);
            FBXStructure[] res = SearchInCurrent("Geometry");
            result = res;

            return true;
        }
        public bool CollectModelTransforms(out ModelCollection[] result)
        {
            SetMajorSection(FBXMajorSection.Objects);
            FBXStructure[] res = SearchInCurrent("Model");


            // COMPILE
            ModelCollection[] mc = new ModelCollection[res.Length];
            for (int ix = 0; ix < res.Length; ix++)
            {
                ModelCollection mcoll = new ModelCollection();
                mcoll.NodeLinkID = GetClumpValue<int>(res[ix].HeadContent[0], 0);
                mcoll.ModelID  = GetClumpValue<string>(res[ix].HeadContent[1], 0).Remove(0, "Model::".Length);
                mcoll.Category = GetClumpValue<string>(res[ix].HeadContent[1], 1);

                mcoll.Version = GetClumpValue<int>(res[ix].Contents[0].Value[0], 0);

                if (res[ix].children.Length > 0)
                {
                    mcoll.HasProperties = true;
                    mcoll.InheritType = GetClumpValue<int>(res[ix].children[0].Contents[0].Value[1], 0);
                    mcoll.DefaultAttribIndex = GetClumpValue<int>(res[ix].children[0].Contents[1].Value[1], 0);
                    
                    for(int look = 0; look < res[ix].children[0].Contents.Length; look++)
                    {
                        string propname = GetClumpValue<string>(res[ix].children[0].Contents[look].Value[0], 0);
                        if (propname == "Lcl Translation")
                        {
                            mcoll.useL_Translation = true;
                            CollapseAsObjectArray(res[ix].children[0].Contents[look].Value, out object[] cv_trans);
                            mcoll.LCL_Translation =
                                new Vector3(ObjectAsDouble(cv_trans[4]), ObjectAsDouble(cv_trans[5]), ObjectAsDouble(cv_trans[6]));
                        }
                        if (propname == "Lcl Rotation")
                        {
                            mcoll.useL_Rotation = true;
                            CollapseAsObjectArray(res[ix].children[0].Contents[look].Value, out object[] cv_rot);
                            mcoll.LCL_Rotation =
                                new Vector3(ObjectAsDouble(cv_rot[4]), ObjectAsDouble(cv_rot[5]), ObjectAsDouble(cv_rot[6]));
                        }
                        if (propname == "Lcl Scaling")
                        {
                            mcoll.useL_Scaling = true;
                            CollapseAsObjectArray(res[ix].children[0].Contents[look].Value, out object[] cv_scal);
                            mcoll.LCL_Scaling =
                                new Vector3(ObjectAsDouble(cv_scal[4]), ObjectAsDouble(cv_scal[5]), ObjectAsDouble(cv_scal[6]));
                        }
                    }

                }
                else
                {
                    mcoll.HasProperties = false;
                }

                string culling = GetClumpValue<string>(res[ix].Contents[1].Value[0], 0);
                if (culling == "CullingOff")
                {
                    mcoll.Culling = false;
                }
                else mcoll.Culling = true;
                mc[ix] = mcoll;
            }
            result = mc;

            return true;
        }
        public bool CollectPoseData(out PoseData[] result)
        {
            SetMajorSection(FBXMajorSection.Objects);
            FBXStructure[] res = SearchInCurrent("Pose");



            // COMPILE
            PoseData[] poseDataCollect = new PoseData[res.Length];
            for (int ix = 0; ix < res.Length; ix++)
            {
                PoseData pose = new PoseData();
                // header contents
                pose.NodeLinkID  = GetClumpValue<int>(res[ix].HeadContent[0], 0);
                pose.PoseName = GetClumpValue<string>(res[ix].HeadContent[1], 0).Remove(0, "Pose::".Length);
                pose.Category = GetClumpValue<string>(res[ix].HeadContent[1], 1);

                // body contents
                pose.Type        = GetClumpValue<string>(res[ix].Contents[0].Value[0], 0);
                pose.Version     = GetClumpValue<int>(res[ix].Contents[1].Value[0], 0);
                pose.NbPoseNodes = GetClumpValue<int>(res[ix].Contents[2].Value[0], 0);

                pose.Nodes = new PoseNode[res[ix].children.Length];
                if (res[ix].children.Length > 0)
                {
                    for(int v = 0; v < res[ix].children.Length; v++)
                    {
                        PoseNode nooodle = new PoseNode();
                        nooodle.NodeLinkID = GetClumpValue<int>(res[ix].children[v].Contents[0].Value[0], 0);
                        nooodle.pose_matrix = new double[16];
                        CollapseAsObjectArray(res[ix].children[v].children[0].Contents[0].Value, out object[] pose_mat);
                        for(int p = 0; p < 16; p++)
                        {
                            nooodle.pose_matrix[p] = ObjectAsDouble(pose_mat[p]);
                        }
                        pose.Nodes[v] = nooodle;
                    }
                }
                else
                {
                    // ok well ig ;-;
                }

                poseDataCollect[ix] = pose;
            }
            result = poseDataCollect;

            return true;
        }
        public bool CollectDeformerNodes(out DeformerNode[] result1, out SubdeformerNode[] result2)
        {
            SetMajorSection(FBXMajorSection.Objects);
            int deformerct = 0;
            int subdeforct = 0;

            // next count geometries:
            for (int ix = 0; ix < CurrentStructure.children.Length; ix++)
            {
                if (CurrentStructure.children[ix].StructureIdent == "Deformer")
                {
                    if (GetClumpValue<string>(CurrentStructure.children[ix].HeadContent[1], 0).Contains("SubDeformer::"))
                    {
                        subdeforct++;
                    }
                    else if (GetClumpValue<string>(CurrentStructure.children[ix].HeadContent[1], 0).Contains("Deformer::"))
                    {
                        deformerct++;
                    }
                }
            }

            // REPEAT
            FBXStructure[] st_def = new FBXStructure[deformerct];
            FBXStructure[] st_sub = new FBXStructure[subdeforct];
            int ct1 = 0;
            int ct2 = 0;
            for (int ix = 0; ix < CurrentStructure.children.Length; ix++)
            {
                if (CurrentStructure.children[ix].StructureIdent == "Deformer")
                {
                    if (GetClumpValue<string>(CurrentStructure.children[ix].HeadContent[1], 0).Contains("SubDeformer::"))
                    {
                        st_sub[ct2] = CurrentStructure.children[ix];
                        ct2++;
                    }
                    else if (GetClumpValue<string>(CurrentStructure.children[ix].HeadContent[1], 0).Contains("Deformer::"))
                    {
                        st_def[ct1] = CurrentStructure.children[ix];
                        ct1++;
                    }
                }
            }



            // COMPILE DEFORMER
            DeformerNode[] deformCollect = new DeformerNode[st_def.Length];
            for (int ix = 0; ix < st_def.Length; ix++)
            {
                DeformerNode deform = new DeformerNode();
                // header contents
                deform.NodeLinkID = GetClumpValue<int>(st_def[ix].HeadContent[0], 0);
                deform.DeformerName = GetClumpValue<string>(st_def[ix].HeadContent[1], 0).Remove(0, "Deformer::".Length);
                deform.DeformerType = GetClumpValue<string>(st_def[ix].HeadContent[1], 1);

                // body contents
                deform.Version = GetClumpValue<int>(st_def[ix].Contents[0].Value[0], 0);
                deform.Link_DeformAccuracy = GetClumpValue<int>(st_def[ix].Contents[1].Value[0], 0);

                deformCollect[ix] = deform;
            }
            result1 = deformCollect;

            // COMPILE SUBDEFORMER
            SubdeformerNode[] subdefCollect = new SubdeformerNode[st_sub.Length];
            for (int ix = 0; ix < st_sub.Length; ix++)
            {
                SubdeformerNode deform = new SubdeformerNode();
                // header contents
                deform.NodeLinkID = GetClumpValue<int>(st_sub[ix].HeadContent[0], 0);
                deform.DeformerName = GetClumpValue<string>(st_sub[ix].HeadContent[1], 0).Remove(0, "SubDeformer::".Length);
                deform.DeformerType = GetClumpValue<string>(st_sub[ix].HeadContent[1], 1);

                // body contents
                deform.Version = GetClumpValue<int>(st_sub[ix].Contents[0].Value[0], 0);
                deform.UserData = new string[] { GetClumpValue<string>(st_sub[ix].Contents[1].Value[0], 0),
                    GetClumpValue<string>(st_sub[ix].Contents[1].Value[0], 1) };

                // Indexes
                // Weights
                // Transform
                // TransformLink
                deform.HasProperties = new bool[4];

                // fill optional properties
                for (int p = 0; p < st_sub[ix].children.Length; p++)
                {
                    if(st_sub[ix].children[p].StructureIdent == "Indexes")
                    {
                        CollapseThreads(st_sub[ix].children[p].Contents[0].Value, out int[] ind_arr);
                        deform.Indexes = ind_arr;
                        deform.HasProperties[0] = true;
                    }
                    else if (st_sub[ix].children[p].StructureIdent == "Weights")
                    {
                        CollapseThreads(st_sub[ix].children[p].Contents[0].Value, out double[] weight_arr);
                        deform.Weights = weight_arr;
                        deform.HasProperties[1] = true;
                    }
                    else if (st_sub[ix].children[p].StructureIdent == "Transform")
                    {
                        CollapseThreads(st_sub[ix].children[p].Contents[0].Value, out double[] transform_arr);
                        deform.Transform = transform_arr;
                        deform.HasProperties[2] = true;
                    }
                    else if (st_sub[ix].children[p].StructureIdent == "TransformLink")
                    {
                        CollapseThreads(st_sub[ix].children[p].Contents[0].Value, out double[] tlink_arr);
                        deform.TransformLink = tlink_arr;
                        deform.HasProperties[3] = true;
                    }
                }

                subdefCollect[ix] = deform;
            }
            result2 = subdefCollect;

            return true;
        }
        public bool CollectAnimationStacks(out AnimationStack[] result)
        {
            SetMajorSection(FBXMajorSection.Objects);
            FBXStructure[] res = SearchInCurrent("AnimationStack");

            // COMPILE
            AnimationStack[] astakscoll = new AnimationStack[res.Length];
            for (int ix = 0; ix < res.Length; ix++)
            {
                AnimationStack astak = new AnimationStack();
                astak.NodeLinkID = GetClumpValue<int>(res[ix].HeadContent[0], 0);
                astak.LocalStop     = GetClumpValue<int>(res[ix].children[0].Contents[0].Value[1], 0);
                astak.ReferenceStop = GetClumpValue<int>(res[ix].children[0].Contents[1].Value[1], 0);

                astakscoll[ix] = astak;
            }
            result = astakscoll;

            return true;
        }
        public bool CollectAnimationLayers()
        {

            return false;
        }
        public bool CollectAnimationCurves()
        {
            // COLLECTS BOTH:
            //   CollectAnimationCurveNode
            //   CollectAnimationCurve

            return false;
        }

        // COLLECTING SECTION 6
        public bool CollectConnectionNodes()
        {
            return false;
        }

        // COLLECTING SECTION 7
        public bool CollectTakeNodes()
        {
            return false;
        }

        private bool SetMajorSection(FBXMajorSection section)
        {
            ResetPlacer();
            string searchTerm = "";
            switch (section)
            {
                case FBXMajorSection.FBXHeaderExtension  :
                    searchTerm = "FBXHeaderExtension";
                    break;
                case FBXMajorSection.GlobalSettings      :
                    searchTerm = "GlobalSettings";
                    break;
                case FBXMajorSection.Documents           :
                    searchTerm = "Documents";
                    break;
                case FBXMajorSection.References          :
                    searchTerm = "References";
                    break;
                case FBXMajorSection.Definitions         :
                    searchTerm = "Definitions";
                    break;
                case FBXMajorSection.Objects             :
                    searchTerm = "Objects";
                    break;
                case FBXMajorSection.Connections         :
                    searchTerm = "Connections";
                    break;
                case FBXMajorSection.Takes               :
                    searchTerm = "Takes";
                    break;
                default:
                    return false;
            }

            // set position:
            for (int ix = 0; ix < FileContents.Length; ix++)
            {
                if (FileContents[ix].StructureIdent == searchTerm)
                { CurrentStructure = FileContents[ix]; CurrentStructureAddress = CurrentStructure.AddressInt; }
            }
            if (CurrentStructure == null) { return false; }

            return true;
        }
        private FBXStructure[] SearchInCurrent(string term)
        {
            int ct = 0;
            // next count geometries:
            for (int ix = 0; ix < CurrentStructure.children.Length; ix++)
            {
                if (CurrentStructure.children[ix].StructureIdent == term)
                {
                    ct++;
                }
            }

            // REPEAT
            FBXStructure[] res = new FBXStructure[ct];
            int ct2 = 0;
            for (int ix = 0; ix < CurrentStructure.children.Length; ix++)
            {
                if (CurrentStructure.children[ix].StructureIdent == term)
                {
                    res[ct2] = CurrentStructure.children[ix];
                    ct2++;
                }
            }
            return res;
        }

        private enum FBXMajorSection
        {
            FBXHeaderExtension    = 0,
            GlobalSettings        = 1,
            Documents             = 2,
            References            = 3,
            Definitions           = 4,
            Objects               = 5,
            Connections           = 6,
            Takes                 = 7
        }

        #region helpfultools
        public T GetClumpValue<T>(ClumpValue clump, int index = 0) where T : IConvertible
        {
            T[] collection = (T[])(clump.Data as T[]);
            int count = 0;
            foreach(T obj in collection)
            {
                if(count == index)
                {
                    return (T)Convert.ChangeType(obj, typeof(T));
                }
                count++;
            }

            return default(T);
        }

        private ClumpValue CollapseAsObjectArray(ClumpValue[] cv, out object[] arr)
        {
            arr = new object[0];
            for(int ix = 0; ix < cv.Length; ix++)
            {
                object[] fill = new object[0];

                switch(cv[ix].dataType)
                {
                    case ClumpDataType.unknown   :
                        object[] curr_obj = cv[ix].Data as object[];
                        fill = curr_obj;
                        break;
                    case ClumpDataType.cd_char   :
                        char[] curr_char = cv[ix].Data as char[];
                        fill = new object[curr_char.Length];
                        for(int ctr = 0; ctr < curr_char.Length; ctr++)
                        {
                            fill[ctr] = curr_char[ctr];
                        }
                        break;
                    case ClumpDataType.cd_string :
                        string[] curr_string = cv[ix].Data as string[];
                        fill = new object[curr_string.Length];
                        for (int ctr = 0; ctr < curr_string.Length; ctr++)
                        {
                            fill[ctr] = curr_string[ctr];
                        }
                        break;
                    case ClumpDataType.cd_int    :
                        int[] curr_int = cv[ix].Data as int[];
                        fill = new object[curr_int.Length];
                        for (int ctr = 0; ctr < curr_int.Length; ctr++)
                        {
                            fill[ctr] = curr_int[ctr];
                        }
                        break;
                    case ClumpDataType.cd_double :
                        double[] curr_double = cv[ix].Data as double[];
                        fill = new object[curr_double.Length];
                        for (int ctr = 0; ctr < curr_double.Length; ctr++)
                        {
                            fill[ctr] = curr_double[ctr];
                        }
                        break;
                }

                // Merge!
                arr = CombineObjArrays(arr, fill);
            }

            ClumpValue cvr = new ClumpValue();
            cvr.dataType = ClumpDataType.unknown;
            cvr.Data = arr;
            return cvr;
        }

        private static object[] CombineObjArrays(object[] o1, object[] o2)
        {
            object[] o_return = new object[o1.Length + o2.Length];
            for(int ix = 0; ix < o1.Length; ix++)
            {
                o_return[ix] = o1[ix];
            }
            for (int ix = 0; ix < o2.Length; ix++)
            {
                o_return[ix + o1.Length] = o2[ix];
            }
            return o_return;
        }

        private static double ObjectAsDouble(object d1)
        {
            return (double)(Convert.ChangeType(d1, typeof(double)));
        }

        /// <summary>
        /// <tooltip>Used to collapse several values into one composite value of type T or T[].</tooltip>
        /// </summary>
        /// <param name="cv"></param>
        /// <returns></returns>
        private ClumpValue CollapseThreads<T>(ClumpValue[] cv, out T[] arr) where T : IConvertible
        {
            ClumpValue CVOUT = new ClumpValue();
            T[] c_res;

            switch (typeof(T))
            {
                case Type intType when intType == typeof(int):
                    CVOUT.dataType = ClumpDataType.cd_int;
                    break;
                case Type doubleType when doubleType == typeof(double):
                    CVOUT.dataType = ClumpDataType.cd_double;
                    break;
                case Type stringType when stringType == typeof(string):
                    CVOUT.dataType = ClumpDataType.cd_string;
                    break;
                case Type charType when charType == typeof(char):
                    CVOUT.dataType = ClumpDataType.cd_char;
                    break;
                default:
                    CVOUT.dataType = ClumpDataType.unknown;
                    break;
            }

            int countup = 0;
            for(int ix = 0; ix < cv.Length; ix++)
            {
                object[] cv_objectholder;
                char[] cv_charholder;
                string[] cv_stringholder;
                int[] cv_intholder;
                double[] cv_doubleholder;

                switch(cv[ix].Data.GetType())
                {
                    case Type objecttype when objecttype == typeof(object[]):
                        cv_objectholder = (cv[ix].Data as object[]); 
                        countup += cv_objectholder.Length;
                        break;
                    case Type chartype when chartype == typeof(char[]):
                        cv_charholder = (cv[ix].Data as char[]);
                        countup += cv_charholder.Length;
                        break;
                    case Type stringtype when stringtype == typeof(string[]):
                        cv_stringholder = (cv[ix].Data as string[]);
                        countup += cv_stringholder.Length;
                        break;
                    case Type intType when intType == typeof(int[]):
                        cv_intholder = (cv[ix].Data as int[]); 
                        countup += cv_intholder.Length;
                        break;
                    case Type doubletype when doubletype == typeof(double[]):
                        cv_doubleholder = (cv[ix].Data as double[]); 
                        countup += cv_doubleholder.Length;
                        break;
                    default:
                        break;
                }
            }
            c_res = new T[countup];

            int absolute = 0;
            for (int ix = 0; ix < cv.Length; ix++)
            {
                ClumpDataType hold = CVOUT.dataType;
                //List<object> collection = (cv[ix].Data as IEnumerable<object>).ToList<object>();

                object[] cv_objectholder;
                char[] cv_charholder;
                string[] cv_stringholder;
                int[] cv_intholder;
                double[] cv_doubleholder;

                switch (cv[ix].Data.GetType())
                {
                    case Type objecttype when objecttype == typeof(object[]):
                        cv_objectholder = (cv[ix].Data as object[]);
                        for (int c = 0; c < cv_objectholder.Length; c++)
                        {
                            switch (hold)
                            {
                                case ClumpDataType.unknown:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_objectholder[c], typeof(object));
                                    break;
                                case ClumpDataType.cd_char:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_objectholder[c], typeof(char));
                                    break;
                                case ClumpDataType.cd_string:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_objectholder[c], typeof(string));
                                    break;
                                case ClumpDataType.cd_int:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_objectholder[c], typeof(int));
                                    break;
                                case ClumpDataType.cd_double:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_objectholder[c], typeof(double));
                                    break;
                            }
                            absolute++;
                        }
                        break;
                    case Type chartype when chartype == typeof(char[]):
                        cv_charholder = (cv[ix].Data as char[]);
                        for (int c = 0; c < cv_charholder.Length; c++)
                        {
                            switch (hold)
                            {
                                case ClumpDataType.unknown:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_charholder[c], typeof(object));
                                    break;
                                case ClumpDataType.cd_char:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_charholder[c], typeof(char));
                                    break;
                                case ClumpDataType.cd_string:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_charholder[c], typeof(string));
                                    break;
                                case ClumpDataType.cd_int:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_charholder[c], typeof(int));
                                    break;
                                case ClumpDataType.cd_double:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_charholder[c], typeof(double));
                                    break;
                            }
                            absolute++;
                        }
                        break;
                    case Type stringtype when stringtype == typeof(string[]):
                        cv_stringholder = (cv[ix].Data as string[]);
                        for (int c = 0; c < cv_stringholder.Length; c++)
                        {
                            switch (hold)
                            {
                                case ClumpDataType.unknown:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_stringholder[c], typeof(object));
                                    break;
                                case ClumpDataType.cd_char:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_stringholder[c], typeof(char));
                                    break;
                                case ClumpDataType.cd_string:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_stringholder[c], typeof(string));
                                    break;
                                case ClumpDataType.cd_int:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_stringholder[c], typeof(int));
                                    break;
                                case ClumpDataType.cd_double:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_stringholder[c], typeof(double));
                                    break;
                            }
                            absolute++;
                        }
                        break;
                    case Type intType when intType == typeof(int[]):
                        cv_intholder = (cv[ix].Data as int[]);
                        for (int c = 0; c < cv_intholder.Length; c++)
                        {
                            switch (hold)
                            {
                                case ClumpDataType.unknown:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_intholder[c], typeof(object));
                                    break;
                                case ClumpDataType.cd_char:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_intholder[c], typeof(char));
                                    break;
                                case ClumpDataType.cd_string:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_intholder[c], typeof(string));
                                    break;
                                case ClumpDataType.cd_int:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_intholder[c], typeof(int));
                                    break;
                                case ClumpDataType.cd_double:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_intholder[c], typeof(double));
                                    break;
                            }
                            absolute++;
                        }
                        break;
                    case Type doubletype when doubletype == typeof(double[]):
                        cv_doubleholder = (cv[ix].Data as double[]);
                        for(int c = 0; c < cv_doubleholder.Length; c++)
                        {
                            switch (hold)
                            {
                                case ClumpDataType.unknown:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_doubleholder[c], typeof(object));
                                    break;
                                case ClumpDataType.cd_char:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_doubleholder[c], typeof(char));
                                    break;
                                case ClumpDataType.cd_string:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_doubleholder[c], typeof(string));
                                    break;
                                case ClumpDataType.cd_int:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_doubleholder[c], typeof(int));
                                    break;
                                case ClumpDataType.cd_double:
                                    c_res[absolute] = (T)Convert.ChangeType(cv_doubleholder[c], typeof(double));
                                    break;
                            }
                            absolute++;
                        }
                        break;
                    default:
                        break;
                }
                
                /*
                foreach (object obj in collection)
                {
                    switch (hold)
                    {
                        case ClumpDataType.unknown   :
                            c_res[absolute] = (T)Convert.ChangeType(obj, typeof(object));
                            break;
                        case ClumpDataType.cd_char   :
                            c_res[absolute] = (T)Convert.ChangeType(obj, typeof(char));
                            break;
                        case ClumpDataType.cd_string :
                            c_res[absolute] = (T)Convert.ChangeType(obj, typeof(string));
                            break;
                        case ClumpDataType.cd_int    :
                            c_res[absolute] = (T)Convert.ChangeType(obj, typeof(int));
                            break;
                        case ClumpDataType.cd_double :
                            c_res[absolute] = (T)Convert.ChangeType(obj, typeof(double));
                            break;
                    }
                    absolute++;
                }
                */
            }
            arr = c_res;
            CVOUT.Data = c_res;

            return CVOUT;
        }
        private void ResetPlacer()
        {
            CurrentStructure = null;
            CurrentStructureAddress = new int[0];
        }
        #endregion helpfultools

        #region fileparsing
        private bool SearchForStructure(string fullname)
        {
            ResetPlacer();
            string[] broken = fullname.Split('.');
            for(int ix = 0; ix < broken.Length; ix++)
            {
                if(ix == 0)
                {
                    for (int iy = 0; iy < FileContents.Length; iy++)
                    {
                        if(FileContents[iy].StructureIdent == broken[ix])
                        {
                            CurrentStructure = FileContents[iy];
                            break;
                        }
                        return false;
                    }
                }
                else
                {
                    for (int iy = 0; iy < CurrentStructure.children.Length; iy++)
                    {
                        if (CurrentStructure.children[iy].StructureIdent == broken[ix])
                        {
                            CurrentStructure = CurrentStructure.children[iy];
                            break;
                        }
                        return false;
                    }
                }
            }
            return true;
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

        private static ClumpValue[] MergeClumps(ClumpValue[] a, ClumpValue[] b)
        {
            ClumpValue[] r = new ClumpValue[a.Length + b.Length];

            int absolute = 0;
            for(int ix = 0; ix < a.Length; ix++)
            {
                r[absolute] = a[ix];
                absolute++;
            }
            for (int ix = 0; ix < b.Length; ix++)
            {
                r[absolute] = b[ix];
                absolute++;
            }

            return r;
        }

        private static bool ContentEndsOnEmpty = false;
        private static ClumpValue[] GetClumpValue(string clump_contents)
        {
            // create uninit'd array
            ClumpValue[] vals;
            ContentEndsOnEmpty = false;
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
                else if(ix == list_dat_types.Length - 1 && list_dat_types[ix] == ClumpDataType.unknown && String.IsNullOrEmpty(st_br[ix]))
                {
                    ContentEndsOnEmpty = true;
                }
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
            bool has_Exponential     = false;
            bool has_MoreExp         = false;
            bool has_Unknown     = false;

            bool reader_indoublequote = false;
            bool reader_insinglequote = false;

            for (int ix = 0; ix < clump_broken.Length; ix++)
            {
                if (Char.IsDigit(clump_broken[ix])) { has_digit = true; }
                else if (Char.IsLetter(clump_broken[ix])) 
                {
                    if((clump_broken[ix] == 'e' || clump_broken[ix] == 'E') && has_digit)
                    {
                        if (!has_Exponential) { has_Exponential = true; }
                        else { has_MoreExp = true; }
                    }
                } // has letter
                else if (clump_broken[ix] == '\'' && !reader_insinglequote) { reader_insinglequote = true; }
                else if (clump_broken[ix] == '\"' && !reader_indoublequote) { reader_indoublequote = true; }
                else if (clump_broken[ix] == '\'' && reader_insinglequote) { reader_insinglequote = false; has_singlequotepair = true; }
                else if (clump_broken[ix] == '\"' && reader_indoublequote) { reader_indoublequote = false; has_doublequotepair = true; }
                else if (clump_broken[ix] == '.' && !has_decimal) { has_decimal = true; }
                else if (clump_broken[ix] == '.' && has_decimal) { has_moredecimals = true; }
                else if (IsEqualAny(clump_broken[ix], new char[] { '*' }) && clump_broken[ix] != '-' && clump_broken[ix] != '.' || (Char.IsPunctuation(clump_broken[ix]) && clump_broken[ix] != '-' && clump_broken[ix] != '.'))
                {
                    has_Unknown = true;
                }
                if (!Char.IsWhiteSpace(clump_broken[ix]) && clump_broken[ix] != '\"' && clump_broken[ix] != '\''
                    && !(reader_insinglequote || reader_indoublequote)) { has_contentoutsidequotes = true; }
            }

            if(has_doublequotepair && !has_contentoutsidequotes) { dattype = ClumpDataType.cd_string; }
            else if(has_singlequotepair && !has_contentoutsidequotes) { dattype = ClumpDataType.cd_char; }
            else if(has_digit &&  has_decimal && !has_moredecimals && !has_Unknown && !has_MoreExp) { dattype = ClumpDataType.cd_double; }
            else if(has_digit && !has_decimal && !has_moredecimals && !has_Unknown && !has_MoreExp) { dattype = ClumpDataType.cd_int; }

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
            int stop = line.Length;
            for (int ix = charArray.Length; ix > 0; ix--)
            {
                // count spaces until char not match
                if (IsEqualAny(charArray[ix - 1], new char[]
                { ' ', '\n', '\t', '\r' }
                )) { stop--; }
                else { break; }
            }
            string build = "";
            for(int ix = 0; ix < stop; ix++)
            {
                build += charArray[ix];
            }

            return build;
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
        #endregion fileparsing
    }
}
