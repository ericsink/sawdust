
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using EricSinkMultiCoreLib;

namespace sd
{
    public partial class Step
    {
        public bool bDefaultRot = true;
        public bool bDefaultZoom = true;

        private double view_rot_y = 0;
        private double view_rot_x = 0;
        private double view_rot_z = 0;
        private double view_zoom = 1;

        public List<Annotation_PointToPoint> annotations_PP = new List<Annotation_PointToPoint>();
        public List<Annotation_FaceToFace> annotations_FF = new List<Annotation_FaceToFace>();

        public List<Face> facesToBeLabeled = new List<Face>();

        public double ViewRotZ
        {
            get
            {
                return view_rot_z;
            }
            set
            {
                view_rot_z = value;
            }
        }

        public double ViewRotY
        {
            get
            {
                return view_rot_y;
            }
            set
            {
                view_rot_y = value;
            }
        }

        public double ViewRotX
        {
            get
            {
                return view_rot_x;
            }
            set
            {
                view_rot_x = value;
            }
        }

        public double ViewZoom
        {
            get
            {
                return view_zoom;
            }
            set
            {
                view_zoom = value;
            }
        }

        internal void GuessPreferredView()
        {
            if (!bDefaultRot)
            {
                return;
            }

            if (action == Action.INTRO)
            {
                view_rot_y = 30;
                view_rot_x = 45;
                return;
            }

            if (action == Action.NEW_BOARD)
            {
                view_rot_y = 30;
                view_rot_x = 68;
                return;
            }

            if (Result == null)
            {
                view_rot_y = 45;
                view_rot_x = 30;
                return;
            }

            xyz camera_LookDirection = null;

            // do a proper guess for each kind of action so the view is looking at what happened

            switch (action)
            {
                case Action.DRILL:
                    {
                        string path = Get_String("path");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s, s_f));

                        camera_LookDirection = -f1.UnitNormal();

                        break;
                    }
                case Action.RABBET:
                case Action.ROUNDOVER:
                    {
                        string path = Get_String("path");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s, s_f));
                        Face f2 = _result.FindFace(string.Format("{0}.{1}", s_s, s_e));

                        xyz v = f1.UnitNormal() + f2.UnitNormal();
                        v.normalize_in_place();

                        camera_LookDirection = -v;

                        break;
                    }
                case Action.CROSSCUT:
                case Action.RIP:
                    {
                        List<Face> faces = this.GetHighlightedFacesForThisStep();
                        xyz v = new xyz(0, 0, 0);
                        foreach (Face f in faces)
                        {
                            v += f.UnitNormal();
                        }
                        v /= faces.Count;

                        camera_LookDirection = -v;

                        break;
                    }
                case Action.DADO:
                    {
                        string path = Get_String("path");
                        string id = Get_String("id");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face fBottom = _result.FindFace(string.Format("{0}.{1}_bottom", s_s, id));
                        HalfEdge he = fBottom.FindEdge(string.Format("{0}_side_above", id));

                        camera_LookDirection = -fBottom.UnitNormal();

                        break;
                    }
                case Action.DOVETAIL_TAILS:
                    {
                        string path2 = Get_String("path2");

                        string s_s2, s_f2, s_e2;
                        ut.ParsePath(path2, out s_s2, out s_f2, out s_e2);

                        Solid s = _result.FindSub(s_s2);

                        List<Face> tails = s.FindFacesByOriginalName(s_f2);

                        Face f2 = tails[0];
                        HalfEdge he2 = f2.FindEdge(s_e2);
                        Face f2b = he2.Opposite().face;

                        camera_LookDirection = -(f2.UnitNormal() + f2b.UnitNormal());

                        break;
                    }
                case Action.DOVETAIL_PINS:
                    {
                        string id = Get_String("id");
                        Dovetail dt = plan.Dovetails[id];

                        string s_s1, s_f1, s_e1;
                        string s_s2, s_f2, s_e2;

                        ut.ParsePath(dt.path1, out s_s1, out s_f1, out s_e1);
                        ut.ParsePath(dt.path2, out s_s2, out s_f2, out s_e2);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}_1", s_s1, s_f1));

                        HalfEdge he1 = f1.FindEdge(s_e1);
                        Face f1b = he1.Opposite().face;

                        camera_LookDirection = -(f1.UnitNormal() + f1b.UnitNormal());

                        break;
                    }
                case Action.DOVETAIL_JOIN:
                    {
                        string id = Get_String("id");
                        Dovetail dt = plan.Dovetails[id];

                        string s_s1, s_f1, s_e1;
                        string s_s2, s_f2, s_e2;

                        ut.ParsePath(dt.path1, out s_s1, out s_f1, out s_e1);
                        ut.ParsePath(dt.path2, out s_s2, out s_f2, out s_e2);

                        Face f1 = _result.FindFace(s_s1, s_e1);
                        Face f2 = _result.FindFace(s_s2, s_e2);

                        camera_LookDirection = f2.UnitNormal();

                        break;
                    }
                case Action.MORTISE:
                    {
                        string path = Get_String("path");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s, s_f));

                        camera_LookDirection = -f1.UnitNormal();

                        break;
                    }
                case Action.TENON:
                    {
                        string path = Get_String("path");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s, s_f));
                        Face f2 = _result.FindFace(string.Format("{0}.{1}", s_s, s_e));

                        xyz v = f1.UnitNormal() + f2.UnitNormal();
                        v.normalize_in_place();

                        camera_LookDirection = -v;

                        break;
                    }
                case Action.CHAMFER:
                    {
                        string path = Get_String("path");

                        string s_s, s_f, s_e;
                        ut.ParsePath(path, out s_s, out s_f, out s_e);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s, s_f));
                        Face f2 = _result.FindFace(string.Format("{0}.{1}", s_s, s_e));

                        xyz v = f1.UnitNormal() + f2.UnitNormal();
                        v.normalize_in_place();

                        camera_LookDirection = -v;
                        break;
                    }
                case Action.JOIN_MT:
                    {
                        string path1 = Get_String("mortisepath");
                        string path2 = Get_String("tenonpath");

                        string s_s1, s_f1, s_e1;
                        ut.ParsePath(path1, out s_s1, out s_f1, out s_e1);
                        string s_s2, s_f2, s_e2;
                        ut.ParsePath(path2, out s_s2, out s_f2, out s_e2);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s1, s_f1));
                        HalfEdge he1 = f1.FindEdge(s_e1);
                        Face f2 = _result.FindFace(string.Format("{0}.{1}", s_s2, s_f2));

                        camera_LookDirection = -(f1.UnitNormal() + -he1.GetInwardNormal());
                        break;
                    }
                case Action.JOIN:
                    {
                        string path1 = Get_String("path1");
                        string path2 = Get_String("path2");

                        string s_s1, s_f1, s_e1;
                        ut.ParsePath(path1, out s_s1, out s_f1, out s_e1);
                        string s_s2, s_f2, s_e2;
                        ut.ParsePath(path2, out s_s2, out s_f2, out s_e2);

                        Face f1 = _result.FindFace(string.Format("{0}.{1}", s_s1, s_f1));
                        HalfEdge he1 = f1.FindEdge(s_e1);
                        Face f2 = _result.FindFace(string.Format("{0}.{1}", s_s2, s_f2));

                        camera_LookDirection = -(f1.UnitNormal() + -he1.GetInwardNormal());
#if not
                        List<Solid> moving;
                        List<Solid> notmoving;

                        xyz v = CalcJoinVector(false, out moving, out notmoving);

                        Face f = null;
                        double a = 0;

                        foreach (Solid s in moving)
                        {
                            foreach (Face fac in s.Faces)
                            {
                                if (
                                    (!fp.eq_unitvec(fac.UnitNormal(), v))
                                    && (!fp.eq_unitvec(fac.UnitNormal(), -v))
                                    )
                                {
                                    double a2 = fac.Area();
                                    if (
                                        (f == null)
                                        || (a2 > a)
                                        )
                                    {
                                        f = fac;
                                        a = a2;
                                    }
                                }
                            }
                        }

                        xyz camera_UpDirection = xyz.cross(f.UnitNormal(), v);
                        camera_LookDirection = -(f.UnitNormal() * 25 + camera_UpDirection * 8 + v * 10);
#endif
                        break;
                    }
            }

            camera_LookDirection.normalize_in_place();

            xyz look = new xyz(0, 0, -1);

            // rot_x rotates around the X axis
            xyz look_yz = new xyz(0, camera_LookDirection.y, camera_LookDirection.z);
            if (fp.gt_inches(look_yz.magnitude(), 0))
            {
                look_yz.normalize_in_place();
                view_rot_x = ut.HowManyDegreesToRotateN2ToEqualBaseVec(look_yz, look, new xyz(1, 0, 0));
            }
            else
            {
                view_rot_x = 0;
            }

            xyz m2 = ut.RotateUnitVector(camera_LookDirection, view_rot_x, new xyz(1, 0, 0));

            // rot_y rotates around the Y axis.
            xyz look_xz = new xyz(m2.x, 0, m2.z);
            if (fp.gt_inches(look_xz.magnitude(), 0))
            {
                look_xz.normalize_in_place();
                view_rot_y = ut.HowManyDegreesToRotateN2ToEqualBaseVec(look_xz, look, new xyz(0, 1, 0));
            }
            else
            {
                view_rot_y = 0;
            }

#if true
            if (fp.eq_degrees(view_rot_x, 180))
            {
                view_rot_x -= 30;
            }
            else
            {
                view_rot_x += 30;
                if (view_rot_x > 180)
                {
                    view_rot_x = 180;
                }
            }
            if (fp.eq_degrees(view_rot_y, 180))
            {
                view_rot_y -= 30;
            }
            else
            {
                view_rot_y += 30;
                if (view_rot_y > 180)
                {
                    view_rot_y = 180;
                }
            }
#endif
        }

        internal void ReadPreferredView(XmlNode n)
        {
            bDefaultRot = ut.ParseBool(n["DefaultRot"].InnerText, false);
            if (!bDefaultRot)
            {
                view_rot_z = ut.ParseDouble(n["Rot_Z"].InnerText, 0);
                view_rot_y = ut.ParseDouble(n["Rot_Y"].InnerText, 0);
                view_rot_x = ut.ParseDouble(n["Rot_X"].InnerText, 0);
            }
            bDefaultZoom = ut.ParseBool(n["DefaultZoom"].InnerText, false);
            if (!bDefaultRot)
            {
                view_zoom = ut.ParseDouble(n["Zoom"].InnerText, 0);
            }
        }

        internal List<FieldDefinition> fields = new List<FieldDefinition>();

        public Plan plan;
        public Action action;
        public string name;
        public string document;
        public string prose;

        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();

        private CompoundSolid _result;

        internal List<piecerec> pieces = new List<piecerec>();
        internal Dictionary<string, Step> dependsOn = new Dictionary<string, Step>();
        internal List<Step> dependingOnMe = new List<Step>();

        public Step Clone()
        {
            Step copy = new Step(this.name, this.action);

            copy.plan = plan;
            document = plan.document; // TODO clone this
            copy.prose = null; // will be set when we execute it
            copy._result = null; // will be set when we execute it

            copy.pieces = pieces;
            copy.dependingOnMe = dependingOnMe;
            copy.dependsOn = dependsOn;

            for (int i = 0; i < fields.Count; i++)
            {
                copy.fields[i].Value = fields[i].Value;
            }

            return copy;
        }

        private static bool FindGlueJoint(List<GlueJoint> a, GlueJoint j)
        {
            foreach (GlueJoint aj in a)
            {
                if (
                    (aj.f1.FullName == j.f1.FullName)
                    && (aj.f2.FullName == j.f2.FullName)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public GlueJointScore GetGlueJointScore()
        {
            if (!IsJoin())
            {
                return null;
            }

            GlueJointScore score = new GlueJointScore(this);

            CompoundSolid cs_result = _result;
            CompoundSolid cs_1 = null;
            CompoundSolid cs_2 = null;

            switch (action)
            {
                case Action.JOIN:
                    {
                        string path1 = Get_String("path1");
                        string path2 = Get_String("path2");
                        Lookup(path1, out cs_1);
                        Lookup(path2, out cs_2);
                        break;
                    }

                case Action.DOVETAIL_JOIN:
                    {
                        string id = Get_String("id");
                        Dovetail dt = plan.Dovetails[id];
                        Lookup(dt.path1, out cs_1);
                        Lookup(dt.path2, out cs_2);
                        break;
                    }
                case Action.JOIN_MT:
                    {
                        string path1 = Get_String("mortisepath");
                        string path2 = Get_String("tenonpath");
                        Lookup(path1, out cs_1);
                        Lookup(path2, out cs_2);
                        break;
                    }
            }

            List<GlueJoint> joints_result = cs_result.FindGlueJoints();
            List<GlueJoint> joints_1 = cs_1.FindGlueJoints();
            List<GlueJoint> joints_2 = cs_2.FindGlueJoints();

            foreach (GlueJoint gj in joints_result)
            {
                if (
                    FindGlueJoint(joints_1, gj)
                    || FindGlueJoint(joints_2, gj)
                    )
                {
                    continue;
                }

                score.faces.Add(gj);
            }

            return score;
        }

        public static string GetActionString(Action a)
        {
            switch (a)
            {
                case Action.INTRO: return "Intro";
                case Action.DRILL: return "Drill";
                case Action.TENON: return "Tenon";
                case Action.MORTISE: return "Mortise";
                case Action.DADO: return "Dado";
                case Action.CHAMFER: return "Chamfer";
                case Action.ROUNDOVER: return "Roundover";
                case Action.RABBET: return "Rabbet";
                case Action.CROSSCUT: return "Crosscut";
                case Action.NEW_BOARD: return "New Board";
                case Action.JOIN: return "Join";
                case Action.JOIN_MT: return "Mortise/Tenon Join";
                case Action.DOVETAIL_TAILS: return "Dovetail Tails";
                case Action.DOVETAIL_PINS: return "Dovetail Pins";
                case Action.DOVETAIL_JOIN: return "Dovetail Join";
                case Action.RIP: return "Rip";
            }

            Debug.Assert(false);

            return null;
        }

        public static Action ParseAction(string s)
        {
            Action[] actions = (Action[])Enum.GetValues(typeof(Action));
            foreach (Action a in actions)
            {
                if (a.ToString() == s)
                {
                    return a;
                }
#if not // TODO not sure this is really a good idea
                if (GetActionString(a) == s)
                {
                    return a;
                }
#endif
            }
            throw new Exception();
        }

        public bool IsJoin()
        {
            return (
                (action == Action.JOIN)
                || (action == Action.DOVETAIL_JOIN)
                || (action == Action.JOIN_MT)
                );
        }

        public xyz CalcJoinVector(out List<Solid> moving, out List<Solid> notmoving)
        {
            // TODO this was used by the old guess code for join.  we might still need it

            Debug.Assert(action == Action.JOIN);

            // find the solids which got glued on during this step
            string str_added = Get_String("path2");
            CompoundSolid sol2;
            Lookup(str_added, out sol2);
            moving = new List<Solid>();
            notmoving = new List<Solid>();
            foreach (Solid s in Result.Subs)
            {
                if (null != sol2.FindSub(s.name))
                {
                    moving.Add(s);
                }
                else
                {
                    notmoving.Add(s);
                }
            }

            xyz vec = CalcJoinVector(moving, notmoving);

            return vec;
        }

        public xyz CalcJoinVector(List<Solid> moving, List<Solid> notmoving)
        {
            // now find the join vector for those solids
            // for every solid in moving, find all the faces it shares
            // with the rest of the solids in workingsolid.  for each
            // one, get the unit normal.  Sum them all up.  The result
            // is the way to move the piece away from the join

            List<xyz> vectors = new List<xyz>();
            foreach (Solid s1 in moving)
            {
                // TODO modify this code to use the GlueJoints stuff?
                foreach (Face f1 in s1.Faces)
                {
                    xyz n1 = f1.UnitNormal();
                    foreach (Solid s2 in notmoving)
                    {
                        foreach (Face f2 in s2.Faces)
                        {
                            xyz n2 = f2.UnitNormal();

                            if (fp.eq_unitvec(n1, -n2))
                            {
                                if (f1.IsCoPlanarWith(f2))
                                {
                                    if (f1.IntersectsWith_SamePlane(f2))
                                    {
                                        vectors.Add(n1);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            xyz vec = null;
            if (vectors.Count == 1)
            {
                vec = vectors[0];
            }
            else
            {
                List<xyz> choices = new List<xyz>();
                foreach (Solid s in moving)
                {
                    choices.Add(s.board_u.normalize());
                    choices.Add(s.board_v.normalize());
                    choices.Add(s.board_w.normalize());
                    choices.Add(-(s.board_u.normalize()));
                    choices.Add(-(s.board_v.normalize()));
                    choices.Add(-(s.board_w.normalize()));
                }
                vec = ut.SelectExplodeVector(choices, vectors);
                if (vec == null)
                {
                    // TODO add a warning to the step, indicating that this is physically impossible
                    vec = ut.CalculateExplodeVector(vectors);
                }
            }

            Debug.Assert(vec != null);
            if (fp.eq_inches(vec.magnitude_squared(), 0))
            {
                vec = null;
            }
            else
            {
                vec = -(vec.normalize());
            }

            return vec;
        }

        private BunchOfTriBags AnimateDovetailJoin()
        {
            Debug.Assert(action == Action.DOVETAIL_JOIN);

            string id = Get_String("id");
            Dovetail dt = plan.Dovetails[id];

            CompoundSolid cs1;
            CompoundSolid cs2;

            Lookup(dt.path1, out cs1);
            Lookup(dt.path2, out cs2);

            List<Solid> moving = new List<Solid>();
            List<Solid> notmoving = new List<Solid>();
            foreach (Solid s in Result.Subs)
            {
                if (null != cs2.FindSub(s.name))
                {
                    moving.Add(s);
                }
                else
                {
                    notmoving.Add(s);
                }
            }

            xyz vec = CalcJoinVector(moving, notmoving);

            BunchOfTriBags bunch = new BunchOfTriBags();
            if (notmoving.Count > 0)
            {
                bunch.notmoving = new List<TriBag>();
                foreach (Solid s in notmoving)
                {
                    bunch.notmoving.Add(new TriBag(s));
                }
            }
            if (moving.Count > 0)
            {
                bunch.vec = vec;
                bunch.moving = new List<TriBag>();
                foreach (Solid s in moving)
                {
                    bunch.moving.Add(new TriBag(s));
                }
            }
            return bunch;
        }

        private BunchOfTriBags AnimateJoin()
        {
            Debug.Assert((action == Action.JOIN) || (action == Action.JOIN_MT));

            // find the solids which got glued on during this step
            string str_added;
            if (action == Action.JOIN)
            {
                str_added = Get_String("path2");
            }
            else
            {
                str_added = Get_String("tenonpath");
            }

            CompoundSolid sol2;
            Lookup(str_added, out sol2);
            List<Solid> moving = new List<Solid>();
            List<Solid> notmoving = new List<Solid>();
            foreach (Solid s in Result.Subs)
            {
                if (null != sol2.FindSub(s.name))
                {
                    moving.Add(s);
                }
                else
                {
                    notmoving.Add(s);
                }
            }

            xyz vec = CalcJoinVector(moving, notmoving);

            BunchOfTriBags bunch = new BunchOfTriBags();
            if (notmoving.Count > 0)
            {
                bunch.notmoving = new List<TriBag>();
                foreach (Solid s in notmoving)
                {
                    bunch.notmoving.Add(new TriBag(s));
                }
            }
            if (moving.Count > 0)
            {
                bunch.vec = vec;
                bunch.moving = new List<TriBag>();
                foreach (Solid s in moving)
                {
                    bunch.moving.Add(new TriBag(s));
                }
            }
            return bunch;
        }

        public string GetProse()
        {
            return prose;
        }

        public string Notes
        {
            get
            {
                if (action == Action.INTRO)
                {
                    if (plan.document == null)
                    {
                        return "";
                    }

                    return plan.document;
                }
                else
                {
                    if (document == null)
                    {
                        return "";
                    }

                    return document;
                }
            }
        }

        public string Description
        {
            get
            {
                if (action == Action.INTRO)
                {
                    return string.Format("{0}: {1}", GetActionString(action), plan.name);
                }
                else if (action == Action.NEW_BOARD)
                {
                    Inches width = Get_Eval("width");
                    Inches length = Get_Eval("length");
                    Inches thickness = Get_Eval("thickness");
                    return string.Format("{0}: {1}: {2} x {3} x {4} in",
                        GetActionString(action),
                        Get_String("newname"),
                        width.GetStringWithoutUnits(),
                        length.GetStringWithoutUnits(),
                        thickness.GetStringWithoutUnits()
                        );
                }
                else
                {
                    int ndx = plan.Steps.IndexOf(this);
                    if (plan.Steps[0].action != Action.INTRO)
                    {
                        ndx++;
                    }

                    return string.Format("({0}) {1}: {2}", ndx, GetActionString(action), name);
                }
            }
        }

        internal void InitFieldDescriptions()
        {
            if (action == Action.INTRO)
            {
                return;
            }

            Debug.Assert(this.fields.Count == 0);
            switch (action)
            {
                case Action.DRILL:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge for specifying where the hole will be"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Drill ID"));
                        this.fields.Add(new FieldDefinition(this, "depth", "Dimension", null, "Depth of the hole"));
                        this.fields.Add(new FieldDefinition(this, "x", "Coord", null, "X coordinate of the center of the hole"));
                        this.fields.Add(new FieldDefinition(this, "y", "Coord", null, "Y coordinate of the center of the hole"));
                        this.fields.Add(new FieldDefinition(this, "diam", "Dimension", null, "Diameter of the hole"));
                        this.fields.Add(new FieldDefinition(this, "count", "Dimension", "1", "Number of holes"));
                        this.fields.Add(new FieldDefinition(this, "dx", "Coord", "0", "delta x"));
                        this.fields.Add(new FieldDefinition(this, "dy", "Coord", "0", "delta y"));
                        break;
                    }
                case Action.MORTISE:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Mortise ID"));
                        this.fields.Add(new FieldDefinition(this, "depth", "Dimension", null, "Depth of mortise"));
                        this.fields.Add(new FieldDefinition(this, "x", "Coord", null, "X coordinate of the corner of the mortise"));
                        this.fields.Add(new FieldDefinition(this, "y", "Coord", null, "Y coordinate of the corner of the mortise"));
                        this.fields.Add(new FieldDefinition(this, "xsize", "Dimension", null, "Width of mortise"));
                        this.fields.Add(new FieldDefinition(this, "ysize", "Dimension", null, "Height of mortise"));
                        this.fields.Add(new FieldDefinition(this, "count", "Dimension", "1", "Number of holes"));
                        this.fields.Add(new FieldDefinition(this, "dx", "Coord", "0", "delta x"));
                        this.fields.Add(new FieldDefinition(this, "dy", "Coord", "0", "delta y"));
                        break;
                    }
                case Action.TENON:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Tenon ID"));
                        this.fields.Add(new FieldDefinition(this, "x", "Coord", null, "X coordinate of the corner of the tenon"));
                        this.fields.Add(new FieldDefinition(this, "y", "Coord", null, "Y coordinate of the corner of the tenon"));
                        this.fields.Add(new FieldDefinition(this, "xsize", "Dimension", null, "Width of tenon"));
                        this.fields.Add(new FieldDefinition(this, "ysize", "Dimension", null, "Length of tenon"));
                        this.fields.Add(new FieldDefinition(this, "depth", "Dimension", null, "Height of tenon"));
                        break;
                    }
                case Action.DADO:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge, dado will be cut parallel to this"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Dado ID"));
                        this.fields.Add(new FieldDefinition(this, "dist", "Coord", null, "How far from the reference edge"));
                        this.fields.Add(new FieldDefinition(this, "width", "Dimension", null, "How wide will the dado be"));
                        this.fields.Add(new FieldDefinition(this, "depth", "Dimension", null, "How deep will the dado be?"));
                        break;
                    }
                case Action.CHAMFER:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Chamfer ID"));
                        this.fields.Add(new FieldDefinition(this, "inset", "Dimension", null, "Distance from the edge to the new edge"));
                        break;
                    }
                case Action.RABBET:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Rabbet ID"));
                        this.fields.Add(new FieldDefinition(this, "inset", "Dimension", null, "inset"));
                        this.fields.Add(new FieldDefinition(this, "depth", "Dimension", null, "depth"));
                        break;
                    }
                case Action.ROUNDOVER:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Reference edge"));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Roundover ID"));
                        this.fields.Add(new FieldDefinition(this, "radius", "Dimension", null, "Radius of the roundover"));
                        break;
                    }
                case Action.RIP:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Parallel edge")); // TODO should this be called parallel edge?
                        this.fields.Add(new FieldDefinition(this, "dist", "Dimension", null, "Distance from to side of the reference edge"));
                        this.fields.Add(new FieldDefinition(this, "taper", "Angle", "0", "Taper angle"));
                        this.fields.Add(new FieldDefinition(this, "tilt", "Angle", "0", "Blade tilt angle"));
                        break;
                    }
                case Action.CROSSCUT:
                    {
                        this.fields.Add(new FieldDefinition(this, "path", "Path", null, "Parallel edge")); // TODO should this be called parallel edge?
                        this.fields.Add(new FieldDefinition(this, "dist", "Dimension", null, "Distance from to side of the reference edge"));
                        this.fields.Add(new FieldDefinition(this, "miter", "Angle", "0", "Miter angle"));
                        this.fields.Add(new FieldDefinition(this, "tilt", "Angle", "0", "Blade tilt angle"));
                        break;
                    }
                case Action.NEW_BOARD:
                    {
                        this.fields.Add(new FieldDefinition(this, "material", "BoardMaterial", null, "What kind of board"));
                        this.fields.Add(new FieldDefinition(this, "newname", "NewPieceName", null, "Name of the new board"));
                        this.fields.Add(new FieldDefinition(this, "width", "Dimension", null, "Width of the new board"));
                        this.fields.Add(new FieldDefinition(this, "length", "Dimension", null, "Length of the new board"));
                        this.fields.Add(new FieldDefinition(this, "thickness", "Dimension", null, "Thickness of the new board"));
                        break;
                    }
                case Action.JOIN:
                    {
                        this.fields.Add(new FieldDefinition(this, "path1", "Path", null, "Path1"));
                        this.fields.Add(new FieldDefinition(this, "path2", "Path", null, "Path2"));
                        this.fields.Add(new FieldDefinition(this, "align", "Alignment", null, "Alignment of the two edges"));
                        this.fields.Add(new FieldDefinition(this, "offset1", "Coord", "0", "Offset"));
                        this.fields.Add(new FieldDefinition(this, "offset2", "Coord", "0", "Offset"));
                        break;
                    }
                case Action.JOIN_MT:
                    {
                        this.fields.Add(new FieldDefinition(this, "mortisepath", "Path", null, "Mortise Path"));
                        this.fields.Add(new FieldDefinition(this, "tenonpath", "Path", null, "Tenon Path"));
                        break;
                    }
                case Action.DOVETAIL_TAILS:
                    {
                        this.fields.Add(new FieldDefinition(this, "path1", "Path", null, "Path1")); // tails
                        this.fields.Add(new FieldDefinition(this, "path2", "Path", null, "Path2")); // pins
                        this.fields.Add(new FieldDefinition(this, "numtails", "Integer", "0", "Number of tails"));
                        this.fields.Add(new FieldDefinition(this, "tailwidth", "Dimension", null, "Width of the tails.  Zero for auto."));
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Dovetail ID"));
                        break;
                    }
                case Action.DOVETAIL_PINS:
                    {
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Dovetail ID"));
                        break;
                    }
                case Action.DOVETAIL_JOIN:
                    {
                        this.fields.Add(new FieldDefinition(this, "id", "ID", null, "Dovetail ID"));
                        break;
                    }
            }
            Debug.Assert(this.fields.Count > 0);
        }

        internal Step(string nam, Action act)
        {
            name = nam;
            action = act;

            InitFieldDescriptions();
        }

        internal Step(XmlNode nod)
        {
            name = nod["name"].InnerText;
            action = ParseAction(nod["action"].InnerText);

            InitFieldDescriptions();
        }

        internal void Set(XmlNode n)
        {
            foreach (FieldDefinition fd in fields)
            {
                Set(fd.name, n[fd.name].InnerText);
            }
        }

#if DEBUG
        internal void AssertAllFieldsHaveValues()
        {
            foreach (FieldDefinition fd in this.fields)
            {
                Debug.Assert(fd.ValueOrDefault != null);
            }
        }
#endif

        public CompoundSolid Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        internal bool Done
        {
            get
            {
                return _result != null;
            }
        }

        internal bool DependenciesAllDone
        {
            get
            {
                foreach (Step st in dependsOn.Values)
                {
                    if (!st.Done)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal bool ReadyToExecute
        {
            get
            {
                return !Done && (Errors.Count == 0) && DependenciesAllDone;
            }
        }

        internal FieldDefinition FindField(string f)
        {
            foreach (FieldDefinition fd in this.fields)
            {
                if (fd.name == f)
                {
                    return fd;
                }
            }
            return null;
        }

        internal void Set(FieldDefinition fd, string v)
        {
            // TODO check the type?
            fd.Value = v;
        }

        internal void Set(string f, string v)
        {
            FieldDefinition fd = FindField(f);
            Debug.Assert(fd != null);
            Set(fd, v);
        }

        internal void Set(Dictionary<string, string> parms)
        {
            if (parms != null)
            {
                foreach (string f in parms.Keys)
                {
                    Set(f, parms[f]);
                }
            }
        }

        internal string Get_String(FieldDefinition fd)
        {
            return fd.ValueOrDefault;
        }

        public string Get_String(string f)
        {
            FieldDefinition fd = FindField(f);
            Debug.Assert(fd != null);

            return Get_String(fd);
        }

        internal string Get_JustTheSolidName(string f)
        {
            FieldDefinition fd = FindField(f);
            Debug.Assert(fd != null);

            string path = Get_String(fd);
            string s, fc, e;
            ut.ParsePath(path, out s, out fc, out e);
            return s;
        }

        internal EdgeAlignment Get_EdgeAlignment(string f)
        {
            EdgeAlignment align;
            string s_align = Get_String(f);
            Debug.Assert(
                (s_align == "right")
                || (s_align == "center")
                || (s_align == "left")
                );
            if (s_align == "right")
            {
                align = EdgeAlignment.Right;
            }
            else if (s_align == "center")
            {
                align = EdgeAlignment.Center;
            }
            else // left
            {
                align = EdgeAlignment.Left;
            }
            return align;
        }

        internal int Get_Eval_Integer(string f)
        {
            string s = Get_String(f);
            double d = Expr.Evaluate(s, this.plan);
            int i = (int)d; // TODO error check this is really an integer?
            return i;
        }

        internal double Get_Eval_Angle(string f)
        {
            string s = Get_String(f);
            return Expr.Evaluate(s, this.plan);
        }

        public Inches Get_Eval(string f)
        {
            string s = Get_String(f);
            return Expr.Evaluate(s, this.plan);
        }

        internal void Write(XmlWriter xw)
        {
            xw.WriteStartElement("step");
            xw.WriteElementString("action", action.ToString());
            xw.WriteElementString("name", name);

            xw.WriteStartElement("view");
            xw.WriteElementString("DefaultRot", this.bDefaultRot.ToString());
            xw.WriteElementString("DefaultZoom", this.bDefaultZoom.ToString());
            if (!bDefaultRot)
            {
                xw.WriteElementString("Rot_Z", this.view_rot_z.ToString());
                xw.WriteElementString("Rot_Y", this.view_rot_y.ToString());
                xw.WriteElementString("Rot_X", this.view_rot_x.ToString());
            }
            if (!bDefaultZoom)
            {
                xw.WriteElementString("Zoom", this.view_zoom.ToString());
            }
            xw.WriteEndElement();

            foreach (FieldDefinition fd in fields)
            {
                xw.WriteElementString(fd.name, Get_String(fd));
            }

            xw.WriteStartElement("annotations");
            foreach (Annotation_FaceToFace a in annotations_FF)
            {
                xw.WriteStartElement("Annotation_FaceToFace");
                xw.WriteElementString("path1", a.path1);
                xw.WriteElementString("path2", a.path2);
                xw.WriteElementString("offset", a.offset.ToString());
                xw.WriteElementString("size", a.size.ToString());
                xw.WriteEndElement();
            }
            xw.WriteEndElement();

            xw.WriteFullEndElement();
        }

        public bool Involves(string csName)
        {
            if (action == Action.INTRO)
            {
                return false;
            }

            // TODO maybe there's still a way to figure out if this step really involves this solid?
            return (Result.FindSub(csName) != null);
        }

        public List<Face> GetHighlightedFacesForThisStep()
        {
            switch (action)
            {
                case Action.INTRO:
                case Action.NEW_BOARD:
                case Action.DOVETAIL_JOIN:
                case Action.JOIN_MT:
                case Action.JOIN:   // TODO I suppose we could highlight the joined faces so we could see where the glue goes
                    {
                        return null;
                    }
            }

            // now get a list of all the faces in the current result which
            // were not present after the previous step

            List<Face> faces = new List<Face>();
            CompoundSolid csMine = this.Result;
            List<string> names = new List<string>();
            foreach (Solid s in csMine.Subs)
            {
                foreach (Face f in s.Faces)
                {
                    string name = string.Format("{0}.{1}", s.name, f.name);
                    bool bfound = false;
                    foreach (Step stdep in dependsOn.Values)
                    {
                        CompoundSolid csdep = stdep.Result;
                        Face f2 = csdep.FindFace(name);
                        if (f2 != null)
                        {
                            bfound = true;
                            break;
                        }
                    }
                    if (!bfound)
                    {
                        faces.Add(f);
                    }
                }
            }

            // now, for certain cases, delete some faces which don't
            // really deserve to be highlighted

            List<Face> rm = new List<Face>();
            switch (action)
            {
                case Action.DADO:
                    {
                        string path = Get_String("path");

                        CompoundSolid cs;
                        Solid sol;
                        Face f1;
                        HalfEdge he;
                        Lookup(path, out cs, out sol, out f1, out he);

                        string f1name = string.Format("{0}.{1}", f1.solid.name, f1.name);
                        foreach (Face f in faces)
                        {
                            string fname = string.Format("{0}.{1}", f.solid.name, f.name);
                            if (fname.StartsWith(f1name))
                            {
                                rm.Add(f);
                            }
                        }
                        break;
                    }
            }
            foreach (Face rf in rm)
            {
                faces.Remove(rf);
            }

            return faces;
        }

        private piecerec FindPiece(Step st)
        {
            foreach (piecerec p in pieces)
            {
                if (p.st == st)
                {
                    return p;
                }
            }
            throw new Exception(string.Format("Could not find piece from step: {0}", st.Description));
        }

        private piecerec FindPiece(string s)
        {
            foreach (piecerec p in pieces)
            {
                foreach (string sol in p.subs)
                {
                    if (sol == s)
                    {
                        return p;
                    }
                }
            }
            throw new Exception(string.Format("Could not find piece for {0}", s));
        }

        private piecerec Mine(piecerec p)
        {
            piecerec p2 = new piecerec();
            p2.st = this;
            p2.subs = new List<string>();
            foreach (string s in p.subs)
            {
                p2.subs.Add(s);
            }
            return p2;
        }

        internal void CalcDependencies()
        {
            Errors.Clear();
            Warnings.Clear();
            pieces.Clear();
            dependsOn.Clear();
            dependingOnMe.Clear();

            if (plan.Steps.IndexOf(this) == 0)
            {
                dependsOn["__intro__"] = plan.LastStep;
                return;
            }

            try
            {
                foreach (piecerec p in PreviousStep.pieces)
                {
                    this.pieces.Add(p);
                }
                switch (action)
                {
                    case Action.DRILL:
                    case Action.TENON:
                    case Action.MORTISE:
                    case Action.DADO:
                    case Action.RIP:
                    case Action.CROSSCUT:
                    case Action.CHAMFER:
                    case Action.RABBET:
                    case Action.ROUNDOVER:
                        {
                            string solname = Get_JustTheSolidName("path");
                            piecerec p = FindPiece(solname);
                            dependsOn[solname] = p.st;
                            this.pieces.Remove(p);
                            p = Mine(p);
                            this.pieces.Add(p);
                            break;
                        }
                    case Action.NEW_BOARD:
                        {
                            piecerec p = new piecerec();
                            p.st = this;
                            p.subs = new List<string>();
                            p.subs.Add(Get_String("newname"));
                            pieces.Add(p);
                            break;
                        }
                    case Action.JOIN:
                        {
                            string solname1 = Get_JustTheSolidName("path1");
                            string solname2 = Get_JustTheSolidName("path2");
                            piecerec p1 = FindPiece(solname1);
                            piecerec p2 = FindPiece(solname2);
                            dependsOn[solname1] = p1.st;
                            dependsOn[solname2] = p2.st;
                            this.pieces.Remove(p1);
                            this.pieces.Remove(p2);

                            piecerec p = new piecerec();
                            p.st = this;
                            p.subs = new List<string>();
                            foreach (string s in p1.subs)
                            {
                                p.subs.Add(s);
                            }
                            foreach (string s in p2.subs)
                            {
                                p.subs.Add(s);
                            }
                            pieces.Add(p);
                            break;
                        }
                    case Action.JOIN_MT:
                        {
                            string solname1 = Get_JustTheSolidName("mortisepath");
                            string solname2 = Get_JustTheSolidName("tenonpath");
                            piecerec p1 = FindPiece(solname1);
                            piecerec p2 = FindPiece(solname2);
                            dependsOn[solname1] = p1.st;
                            dependsOn[solname2] = p2.st;
                            this.pieces.Remove(p1);
                            this.pieces.Remove(p2);

                            piecerec p = new piecerec();
                            p.st = this;
                            p.subs = new List<string>();
                            foreach (string s in p1.subs)
                            {
                                p.subs.Add(s);
                            }
                            foreach (string s in p2.subs)
                            {
                                p.subs.Add(s);
                            }
                            pieces.Add(p);
                            break;
                        }
                    case Action.DOVETAIL_TAILS:
                        {
                            string solname1 = Get_JustTheSolidName("path1");
                            string solname2 = Get_JustTheSolidName("path2");
                            piecerec p1 = FindPiece(solname1);
                            piecerec p2 = FindPiece(solname2);
                            dependsOn[solname1] = p1.st;
                            dependsOn[solname2] = p2.st;
                            this.pieces.Remove(p2);
                            piecerec p = Mine(p2);
                            this.pieces.Add(p);
                            break;
                        }
                    case Action.DOVETAIL_PINS:
                        {
                            Step stTails = PreviousStep;
                            while (true)
                            {
                                if (
                                    (stTails.action == Action.DOVETAIL_TAILS)
                                    && (stTails.Get_String("id") == Get_String("id"))
                                    )
                                {
                                    break;
                                }
                                stTails = stTails.PreviousStep;
                            }
                            string solname1 = stTails.Get_JustTheSolidName("path1");
                            string solname2 = stTails.Get_JustTheSolidName("path2");
                            piecerec p1 = FindPiece(solname1);
                            piecerec p2 = FindPiece(solname2);
                            dependsOn[solname1] = p1.st;
                            dependsOn[solname2] = stTails;
                            this.pieces.Remove(p1);
                            piecerec p = Mine(p1);
                            this.pieces.Add(p);
                            break;
                        }
                    case Action.DOVETAIL_JOIN:
                        {
                            Step stPins = null;
                            Step stTails = null;
                            Step st = PreviousStep;
                            while (true)
                            {
                                if (
                                    (st.action == Action.DOVETAIL_PINS)
                                    && (st.Get_String("id") == Get_String("id"))
                                    )
                                {
                                    stPins = st;
                                }
                                else if (
                                    (st.action == Action.DOVETAIL_TAILS)
                                    && (st.Get_String("id") == Get_String("id"))
                                    )
                                {
                                    stTails = st;
                                }

                                if (
                                    (stTails != null)
                                    && (stPins != null)
                                    )
                                {
                                    break;
                                }

                                st = st.PreviousStep;
                            }
                            string solname1 = stTails.Get_JustTheSolidName("path1");
                            string solname2 = stTails.Get_JustTheSolidName("path2");
                            piecerec p1 = FindPiece(solname1);
                            piecerec p2 = FindPiece(solname2);
                            dependsOn[solname1] = p1.st;
                            dependsOn[solname2] = p2.st;
                            this.pieces.Remove(p1);
                            this.pieces.Remove(p2);
                            piecerec p = new piecerec();
                            p.st = this;
                            p.subs = new List<string>();
                            foreach (string s in p1.subs)
                            {
                                p.subs.Add(s);
                            }
                            foreach (string s in p2.subs)
                            {
                                p.subs.Add(s);
                            }
                            pieces.Add(p);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                this.Errors.Add(e.Message);
            }
        }

        internal void Lookup(string path, out CompoundSolid cs)
        {
            Solid sol;
            Face f;
            HalfEdge he;
            Lookup(path.Split('.')[0], out cs, out sol, out f, out he);
        }

        internal void Lookup(string path, out CompoundSolid cs, out Solid sol, out Face f, out HalfEdge he)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.Length > 0);

            cs = null;
            sol = null;
            f = null;
            he = null;

            string s_sol = null;
            string s_face = null;
            string s_edge = null;

            ut.ParsePath(path, out s_sol, out s_face, out s_edge);

            if (!dependsOn.ContainsKey(s_sol))
            {
                throw new Exception(string.Format("Piece {0} could not be found", s_sol));
            }

            Step st = dependsOn[s_sol];
            cs = st.Result;

            // If cs is null, there is a dependency violation of some kind
            Debug.Assert(cs != null);

            sol = cs.FindSub(s_sol);

            if (sol == null)
            {
                throw new Exception(string.Format("Piece {0} could not be found", s_sol));
            }

            if (s_face != null)
            {
                f = sol.FindFace(s_face);
                if (f == null)
                {
                    throw new Exception(string.Format("Face {0} could not be found inside piece {1}", s_face, s_sol));
                }

                if (s_edge != null)
                {
                    he = f.FindEdge(s_edge);
                    if (he == null)
                    {
                        throw new Exception(string.Format("Edge {0} could not be found adjacent to face {1} inside piece {2}", s_edge, s_face, s_sol));
                    }
                }
            }
        }

        internal Step NextStep
        {
            get
            {
                int ndx = plan.Steps.IndexOf(this);
                if (ndx == plan.Steps.Count - 1)
                {
                    return null;
                }
                else
                {
                    return plan.Steps[ndx + 1];
                }
            }
        }

        internal Step PreviousStep
        {
            get
            {
                int ndx = plan.Steps.IndexOf(this);
                Debug.Assert(ndx >= 1); // never call PreviousStep on the Intro step
                return plan.Steps[ndx - 1];
            }
        }

        internal static Dictionary<string, string> CreateParms(params string[] pairs)
        {
            Debug.Assert(0 == (pairs.Length % 2));

            Dictionary<string, string> parms = new Dictionary<string, string>();
            int ndx = 0;
            for (int i = 0; i < pairs.Length / 2; i++)
            {
                string f = pairs[ndx++];
                string v = pairs[ndx++];
                parms[f] = v;
            }

            return parms;
        }

        internal void FixDependingOnMeList()
        {
            foreach (Step other in plan.Steps)
            {
                if (other != this)
                {
                    if (other.dependsOn.ContainsValue(this))
                    {
                        dependingOnMe.Add(other);
                    }
                }
            }
        }

        internal void ReadAnnotations(XmlElement top)
        {
            foreach (XmlNode nod in top.ChildNodes)
            {
                string path1 = nod["path1"].InnerText;
                string path2 = nod["path2"].InnerText;
                double offset = ut.ParseDouble(nod["offset"].InnerText, 2);
                double size = ut.ParseDouble(nod["size"].InnerText, 1);

                this.annotations_FF.Add(new Annotation_FaceToFace(path1, path2, offset, size));
            }
        }

        public BunchOfTriBags GetBunch(bool p)
        {
            if (p && (action == Action.JOIN))
            {
                return AnimateJoin();
            }
            else if (p && (action == Action.JOIN_MT))
            {
                return AnimateJoin();
            }
            else if (p && (action == Action.DOVETAIL_JOIN))
            {
                return AnimateDovetailJoin();
            }
            else
            {
                return Result.CreateBags();
            }
        }
    }
}
