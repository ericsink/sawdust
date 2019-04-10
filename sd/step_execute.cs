
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
        internal void Execute()
        {
            Errors.Clear();
            Warnings.Clear();
            annotations_PP.Clear();
            facesToBeLabeled.Clear();
            prose = null;

            try
            {
                /*
                 * This switch statement must have one case for
                 * every kind of step action.
                 * 
                 * Each case in this switch statement needs to:
                 * 
                 * 1.  retrieve the parameters for the step
                 * 2.  do error checking on those parameters
                 * 3.  calculate the prose description of the step
                 * 4.  execute the step
                 * 5.  add faces to be labeled
                 * 6.  add annotations
                 * */

                switch (action)
                {
                    case Action.INTRO:
                        {
                            prose = "";
                            _result = plan.LastStep.Result;
                            break;
                        }
                    case Action.DRILL:
                        {
                            Inches x = Get_Eval("x");
                            Inches y = Get_Eval("y");
                            Inches diam = Get_Eval("diam");
                            Inches depth = Get_Eval("depth");
                            string id = Get_String("id");
                            Inches dx = Get_Eval("dx");
                            Inches dy = Get_Eval("dy");
                            int count = Get_Eval_Integer("count");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            if (depth > Limits.MAX_DRILL_DEPTH)
                            {
                                Warnings.Add(string.Format("Drill depth is absurd"));
                            }

                            StringBuilder sb = new StringBuilder();
                            if (count > 1)
                            {
                                sb.AppendFormat("Drill {2} holes\r\n{0} in diameter and {1} deep\r\n", diam.GetProse(), depth.GetProse(), count);
                                sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                                sb.AppendFormat("\r\nstarting at {0}, {1} inches from\r\nthe edge between faces '{2}' and '{3}'.\r\n", x.GetStringWithoutUnits(), y.GetStringWithoutUnits(), f.name, he.Opposite().face.name);
                                // TODO what if dx and dy are both non-zero?
                                if (dx > 0)
                                {
                                    sb.AppendFormat("{0} apart", dx.GetProse());
                                }
                                else
                                {
                                    sb.AppendFormat("{0} apart", dy.GetProse());
                                }
                            }
                            else
                            {
                                sb.AppendFormat("Drill a hole\r\n{0} in diameter and {1} deep\r\n", diam.GetProse(), depth.GetProse());
                                sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                                sb.AppendFormat("\r\nat {0}, {1} inches from\r\nthe edge between faces '{2}' and '{3}'.\r\n", x.GetStringWithoutUnits(), y.GetStringWithoutUnits(), f.name, he.Opposite().face.name);
                            }
                            prose = sb.ToString();

                            _result = wood.Drill(cs, he, x, y, count, dx, dy, diam, depth, id);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));

                            break;
                        }
                    case Action.TENON:
                        {
                            Inches x = Get_Eval("x");
                            Inches y = Get_Eval("y");
                            Inches xsize = Get_Eval("xsize");
                            Inches ysize = Get_Eval("ysize");
                            Inches depth = Get_Eval("depth");
                            string id = Get_String("id");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Cut a tenon\r\n{0} by {1}\r\n{2} deep\r\n", xsize.GetProse(), ysize.GetProse(), depth.GetProse());
                            sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                            sb.AppendFormat("at {0}, {1} inches from\r\nthe edge between faces '{2}' and '{3}'.\r\n", x.GetStringWithoutUnits(), y.GetStringWithoutUnits(), f.name, he.Opposite().face.name);
                            sb.AppendFormat("Call this tenon '{0}'.", id);
                            prose = sb.ToString();

                            _result = wood.Tenon(cs, he, new xy(x, y), new xyz(xsize, ysize, depth), id);

                            string _s, _f, _e;
                            ut.ParsePath(Get_String("path"), out _s, out _f, out _e);

                            Face newendface = _result.FindFace(_s, _f);
                            if (newendface != null)
                            {
                                facesToBeLabeled.Add(newendface);

                                HalfEdge heother = newendface.FindEdge(_e);
                                if (heother != null)
                                {
                                    facesToBeLabeled.Add(heother.Opposite().face);
                                }

                                HalfEdge hea = newendface.FindEdge(string.Format("{0}_left", id));
                                if (hea == null)
                                {
                                    hea = newendface.FindEdge(string.Format("{0}_right", id));
                                }
                                if (hea != null)
                                {
                                    this.annotations_PP.Add(new Annotation_PointToPoint(hea.from, hea.to, newendface.UnitNormal(), 2));
                                }

                                hea = newendface.FindEdge(string.Format("{0}_front", id));
                                if (hea == null)
                                {
                                    hea = newendface.FindEdge(string.Format("{0}_back", id));
                                }
                                if (hea != null)
                                {
                                    this.annotations_PP.Add(new Annotation_PointToPoint(hea.from, hea.to, newendface.UnitNormal(), 2));
                                }
                            }

                            // TODO more annotations here for the dimensions of the shoulders?

                            break;
                        }
                    case Action.MORTISE:
                        {
                            Inches x = Get_Eval("x");
                            Inches y = Get_Eval("y");
                            Inches xsize = Get_Eval("xsize");
                            Inches ysize = Get_Eval("ysize");
                            Inches depth = Get_Eval("depth");
                            string id = Get_String("id");
                            Inches dx = Get_Eval("dx");
                            Inches dy = Get_Eval("dy");
                            int count = Get_Eval_Integer("count");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            if (depth > Limits.MAX_MORTISE_DEPTH)
                            {
                                Warnings.Add(string.Format("Mortise depth is absurd"));
                            }

                            StringBuilder sb = new StringBuilder();
                            if (count == 1)
                            {
                                sb.AppendFormat("Cut a mortise\r\n{0} by {1}\r\n{2} deep\r\n", xsize.GetProse(), ysize.GetProse(), depth.GetProse());
                                sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                                sb.AppendFormat("at {0}, {1} inches from\r\nthe edge between faces '{2}' and '{3}'.\r\n", x.GetStringWithoutUnits(), y.GetStringWithoutUnits(), f.name, he.Opposite().face.name);
                                //sb.AppendFormat("Call this mortise '{0}'.", id);
                            }
                            else
                            {
                                sb.AppendFormat("Cut {3} mortises\r\n{0} by {1}\r\n{2} deep\r\n", xsize.GetProse(), ysize.GetProse(), depth.GetProse(), count);
                                sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                                sb.AppendFormat("starting at {0}, {1} inches from\r\nthe edge between faces '{2}' and '{3}'\r\n", x.GetStringWithoutUnits(), y.GetStringWithoutUnits(), f.name, he.Opposite().face.name);
                                // TODO what if dx and dy are both non-zero?
                                if (dx > 0)
                                {
                                    sb.AppendFormat("{0} apart", dx.GetProse());
                                }
                                else
                                {
                                    sb.AppendFormat("{0} apart", dy.GetProse());
                                }
                                //sb.AppendFormat("Call this mortise '{0}'.", id);
                            }
                            prose = sb.ToString();
                            _result = wood.Mortise(cs, he, new xy(x, y), new xyz(xsize, ysize, depth), count, dx, dy, id);

                            string _s, _f, _e;
                            ut.ParsePath(Get_String("path"), out _s, out _f, out _e);
                            Face newf = _result.FindFace(_s, _f);
                            if (newf != null)
                            {
                                facesToBeLabeled.Add(newf);
                                HalfEdge heother = newf.FindEdge(_e);
                                if (heother != null)
                                {
                                    facesToBeLabeled.Add(heother.Opposite().face);
                                }
                            }

                            if (count == 1)
                            {
                                // label the dimensions of the mortise.  first across the front or back
                                HalfEdge hea = _result.FindEdge(sol.name, string.Format("{0}_front", id), f.name);
                                if (hea == null)
                                {
                                    hea = _result.FindEdge(sol.name, string.Format("{0}_back", id), f.name);
                                }
                                if (hea != null)
                                {
                                    this.annotations_PP.Add(new Annotation_PointToPoint(hea.from, hea.to, f.UnitNormal(), 2));
                                }

                                // now along the left or right
                                hea = _result.FindEdge(sol.name, string.Format("{0}_left", id), f.name);
                                if (hea == null)
                                {
                                    hea = _result.FindEdge(sol.name, string.Format("{0}_right", id), f.name);
                                }
                                if (hea != null)
                                {
                                    this.annotations_PP.Add(new Annotation_PointToPoint(hea.from, hea.to, f.UnitNormal(), 2));
                                }

                                // now label the location of the mortise
                                if (newf != null)
                                {

                                    hea = newf.FindEdge(string.Format("{0}_front", id));

                                    if (hea != null)
                                    {
                                        xyz p1 = hea.from;
                                        xyz p2 = newf.Measure(hea, p1);
                                        if (p2 != null)
                                        {
                                            this.annotations_PP.Add(new Annotation_PointToPoint(p1, p2, newf.UnitNormal(), 2));
                                        }
                                    }

                                    hea = newf.FindEdge(string.Format("{0}_right", id));
                                    if (hea != null)
                                    {
                                        xyz p1 = hea.from;
                                        xyz p2 = newf.Measure(hea, p1);
                                        if (p2 != null)
                                        {
                                            this.annotations_PP.Add(new Annotation_PointToPoint(p1, p2, newf.UnitNormal(), 2));
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    case Action.DADO:
                        {
                            Inches dist = Get_Eval("dist");
                            Inches width = Get_Eval("width");
                            Inches depth = Get_Eval("depth");
                            string path = Get_String("path");
                            string id = Get_String("id");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(path, out cs, out sol, out f, out he);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Cut a dado\r\n{0} wide and {1} deep\r\n", width.GetProse(), depth.GetProse());
                            sb.AppendFormat("in the '{0}' face of the board named '{1}'\r\n", f.name, sol.name);
                            sb.AppendFormat("parallel to and {0} from the edge between faces '{1}' and '{2}'.\r\n", dist.GetProse(), f.name, he.Opposite().face.name);
                            sb.AppendFormat("Call this dado '{0}'.", id);
                            prose = sb.ToString();

                            _result = wood.Dado(cs, he, dist, width, depth, id);

                            facesToBeLabeled.Add(_result.FindFace(string.Format("{0}.{1}_1", sol.name, f.name)));
                            facesToBeLabeled.Add(_result.FindFace(string.Format("{0}.{1}_2", sol.name, f.name)));

                            break;
                        }
                    case Action.RIP:
                        {
                            Inches dist = Get_Eval("dist");
                            double taper = Get_Eval_Angle("taper");
                            double tilt = Get_Eval_Angle("tilt");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            if (he.Opposite().face.GetQuality() == FaceQuality.EndGrain)  // TODO is this how we want this to work?
                            {
                                Warnings.Add("This is not really a rip.");
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("TODO");
                            prose = sb.ToString();

                            _result = wood.Crosscut_Or_Rip(cs, he, dist, taper, tilt);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));
                            facesToBeLabeled.Add(_result.FindFace(sol.name, he.Opposite().face.name));

                            break;
                        }
                    case Action.CROSSCUT:
                        {
                            Inches dist = Get_Eval("dist");
                            double miter = Get_Eval_Angle("miter");
                            double tilt = Get_Eval_Angle("tilt");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            if (he.Opposite().face.GetQuality() != FaceQuality.EndGrain) // TODO is this how we want this to work?
                            {
                                Warnings.Add("This is not really a crosscut.");
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("TODO");
                            prose = sb.ToString();

                            _result = wood.Crosscut_Or_Rip(cs, he, dist, miter, tilt);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));
                            facesToBeLabeled.Add(_result.FindFace(sol.name, he.Opposite().face.name));

                            break;
                        }
                    case Action.CHAMFER:
                        {
                            Inches inset = Get_Eval("inset");
                            string id = Get_String("id");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Chamfer\r\nthe edge between the faces '{0}' and '{1}'\r\non the board '{2}'\r\n", f.name, he.Opposite().face.name, sol.name);
                            sb.AppendFormat("at a depth of {0}.", inset.GetProse());
                            prose = sb.ToString();

                            _result = wood.DoChamfer(cs, he, inset, id);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));
                            facesToBeLabeled.Add(_result.FindFace(sol.name, he.Opposite().face.name));

                            xyz p1 = he.Center();

                            Face chamface = _result.FindFace(sol.name, string.Format("{0}_2", id));
                            HalfEdge he1 = chamface.FindEdge(f.name);

                            this.annotations_PP.Add(new Annotation_PointToPoint(p1, he1.Center(), he1.Opposite().face.UnitNormal(), 2));

                            he1 = chamface.FindEdge(he.Opposite().face.name);
                            this.annotations_PP.Add(new Annotation_PointToPoint(p1, he1.Center(), he1.Opposite().face.UnitNormal(), 2));

                            break;
                        }
                    case Action.RABBET:
                        {
                            Inches inset = Get_Eval("inset");
                            Inches depth = Get_Eval("depth");
                            string id = Get_String("id");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("TODO");
                            prose = sb.ToString();

                            _result = wood.DoRabbet(cs, he, inset, depth, id);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));
                            facesToBeLabeled.Add(_result.FindFace(sol.name, he.Opposite().face.name));

                            break;
                        }
                    case Action.ROUNDOVER:
                        {
                            Inches radius = Get_Eval("radius");
                            string id = Get_String("id");

                            CompoundSolid cs;
                            Solid sol;
                            Face f;
                            HalfEdge he;
                            Lookup(Get_String("path"), out cs, out sol, out f, out he);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("TODO");
                            prose = sb.ToString();

                            _result = wood.DoRoundover(cs, he, radius, id);

                            facesToBeLabeled.Add(_result.FindFace(sol.name, f.name));
                            facesToBeLabeled.Add(_result.FindFace(sol.name, he.Opposite().face.name));

                            break;
                        }
                    case Action.NEW_BOARD:
                        {
                            Inches width = Get_Eval("width");
                            Inches length = Get_Eval("length");
                            Inches thickness = Get_Eval("thickness");
                            string newname = Get_String("newname");
                            string material = Get_String("material");
                            BoardMaterial bm = BoardMaterial.Find(material);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Cut a new board made of {0}\r\n", bm.GetProse());
                            sb.AppendFormat("{0} wide\r\n{1} long\r\n{2} thick\r\n", width.GetProse(), length.GetProse(), thickness.GetProse());
                            sb.AppendFormat("Call this board '{0}'.", newname);
                            prose = sb.ToString();

                            _result = wood.CreateBoard(bm, newname, width, length, thickness).ToCompoundSolid();

                            Face ftop = _result.Subs[0].FindFace("top");
                            HalfEdge he = ftop.FindEdge("end1");
                            this.annotations_PP.Add(new Annotation_PointToPoint(he.from, he.to, ftop.UnitNormal(), 2));

                            he = ftop.FindEdge("left");
                            this.annotations_PP.Add(new Annotation_PointToPoint(he.from, he.to, ftop.UnitNormal(), 2));

                            Face fend1 = _result.Subs[0].FindFace("end1");
                            he = fend1.FindEdge("left");
                            this.annotations_PP.Add(new Annotation_PointToPoint(he.from, he.to, fend1.UnitNormal(), 2));

                            this.facesToBeLabeled.AddRange(_result.Subs[0].Faces);

                            break;
                        }
                    case Action.JOIN:
                        {
                            Inches offset1 = Get_Eval("offset1");
                            Inches offset2 = Get_Eval("offset2");
                            string path1 = Get_String("path1");
                            string path2 = Get_String("path2");
                            EdgeAlignment align = Get_EdgeAlignment("align");

                            CompoundSolid s1;
                            CompoundSolid s2;
                            Solid sol1;
                            Solid sol2;
                            Face f1;
                            Face f2;
                            HalfEdge he1;
                            HalfEdge he2;

                            Lookup(path1, out s1);
                            Lookup(path2, out s2);

                            CompoundSolid snew = s1.ShallowClone();     // s1 doesn't get modified by the join, so we don't need full clone
                            CompoundSolid sadd = s2.Clone();            // s2 gets rotated and moved, so this step needs its own copy of every subsolid

                            snew.Lookup(path1, out sol1, out f1, out he1);
                            sadd.Lookup(path2, out sol2, out f2, out he2);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Join the board named '{0}'\r\nto the board named '{1}'\r\nas follows:\r\n",
                                sol2.name,
                                sol1.name);
                            sb.AppendFormat("On '{0}' find the face '{1}'\r\nand the edge shared between that face and '{2}'.\r\n",
                                sol2.name, f2.name, he2.Opposite().face.name);
                            sb.AppendFormat("On '{0}' find the face '{1}'\r\nand the edge shared between that face and '{2}'.\r\n",
                                sol1.name, f1.name, he1.Opposite().face.name);
                            sb.AppendFormat("Join these two faces together by matching up the two edges described above");
                            // TODO align
                            if (offset1 > 0)
                            {
                                sb.AppendFormat("at an offset of {0} along the edges", offset1.GetProse());
                            }
                            if (offset2 > 0)
                            {
                                sb.AppendFormat("at an offset of {0} perpendicular to the edges", offset2.GetProse());
                            }
                            prose = sb.ToString();

                            orient.Edges(sadd, f1, f2, he1, he2, align, offset1, offset2);
                            snew.AddSub(sadd);

#if DEBUG
                            snew.AssertNoNameClashes();
#endif

                            if (!snew.IsValidWithNoSubOverlaps())
                            {
                                Errors.Add("Invalid join:  Two boards cannot occupy the same space.");
                            }

                            _result = snew;

                            facesToBeLabeled.Add(_result.FindFace(sol1.name, f1.name));
                            facesToBeLabeled.Add(_result.FindFace(sol1.name, he1.Opposite().face.name));

                            facesToBeLabeled.Add(_result.FindFace(sol2.name, f2.name));
                            facesToBeLabeled.Add(_result.FindFace(sol2.name, he2.Opposite().face.name));

                            break;
                        }
                    case Action.JOIN_MT:
                        {
                            string path1 = Get_String("mortisepath");
                            string path2 = Get_String("tenonpath");

                            CompoundSolid s1;
                            CompoundSolid s2;
                            Solid sol1;
                            Solid sol2;
                            Face f1;
                            Face f2;
                            HalfEdge he1;
                            HalfEdge he2;

                            Lookup(path1, out s1);
                            Lookup(path2, out s2);

                            CompoundSolid snew = s1.ShallowClone();     // s1 doesn't get modified by the join, so we don't need full clone
                            CompoundSolid sadd = s2.Clone();            // s2 gets rotated and moved, so this step needs its own copy of every subsolid

                            snew.Lookup(path1, out sol1, out f1, out he1);
                            sadd.Lookup(path2, out sol2, out f2, out he2);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Assemble a mortise/tenon joint\r\njoining the board named '{0}' to the board named '{1}' as follows:\r\n",
                                sol2.name,
                                sol1.name);
                            sb.AppendFormat(" On '{0}' find the face '{1}' and the edge shared between that face and '{2}'.",
                                sol2.name, f2.name, he2.Opposite().face.name);
                            sb.AppendFormat(" On '{0}' find the face '{1}' and the edge shared between that face and '{2}'.",
                                sol1.name, f1.name, he1.Opposite().face.name);
                            sb.AppendFormat(" Join these two faces together by matching up the two edges described above.");
                            prose = sb.ToString();

                            orient.Edges(sadd, f1, f2, he1, he2, EdgeAlignment.Center, 0, 0);
                            snew.AddSub(sadd);

                            // TODO verify that the gluejoint has at least 3 pairs of faces (but what about face pairs not-local to this joint?)
#if DEBUG
                            snew.AssertNoNameClashes();
#endif

                            if (!snew.IsValidWithNoSubOverlaps())
                            {
                                Errors.Add("Invalid join:  Two boards cannot occupy the same space.");
                            }

                            _result = snew;

                            facesToBeLabeled.Add(_result.FindFace(sol1.name, f1.name));
                            facesToBeLabeled.Add(_result.FindFace(sol1.name, he1.Opposite().face.name));

                            facesToBeLabeled.Add(_result.FindFace(sol2.name, f2.name));
                            facesToBeLabeled.Add(_result.FindFace(sol2.name, he2.Opposite().face.name));

                            break;
                        }
                    case Action.DOVETAIL_JOIN:
                        {
                            string id = Get_String("id");
                            if (!plan.Dovetails.ContainsKey(id))
                            {
                                Errors.Add(string.Format("Dovetail not found: {0}", id));
                            }
                            else
                            {
                                Dovetail dt = plan.Dovetails[id];

                                CompoundSolid cs1;
                                CompoundSolid cs2;

                                Lookup(dt.path1, out cs1);
                                Lookup(dt.path2, out cs2);

                                CompoundSolid snew = cs1.Clone();
                                CompoundSolid sadd = cs2.Clone();

                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("Assemble the dovetail joint called '{0}'\r\n",
                                    id);
                                prose = sb.ToString();

                                dt.Join(snew, sadd);

                                if (!snew.IsValidWithNoSubOverlaps())
                                {
                                    Errors.Add("Invalid join:  Two boards cannot occupy the same space.");
                                }

                                _result = snew;

                                string _s1, _f1, _e1;
                                string _s2, _f2, _e2;
                                ut.ParsePath(dt.path1, out _s1, out _f1, out _e1);
                                ut.ParsePath(dt.path2, out _s2, out _f2, out _e2);

                                facesToBeLabeled.Add(_result.FindFace(_s1, _e1));
                                facesToBeLabeled.Add(_result.FindFace(_s2, _e2));

                            }
                            break;
                        }
                    case Action.DOVETAIL_PINS:
                        {
                            string id = Get_String("id");
                            if (!plan.Dovetails.ContainsKey(id))
                            {
                                Errors.Add(string.Format("Dovetail not found: {0}", id));
                            }
                            else
                            {
                                Dovetail dt = plan.Dovetails[id];
                                CompoundSolid cs1;
                                CompoundSolid cs2;

                                Lookup(dt.path1, out cs1);
                                Lookup(dt.path2, out cs2);

                                string s_s1, s_f1, s_e1;
                                ut.ParsePath(dt.path1, out s_s1, out s_f1, out s_e1);

                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("For the dovetail joint called '{0}',\r\nCut the pins in the board named '{1}'\r\n",
                                    id,
                                    s_s1);
                                sb.AppendFormat("in the face named '{0}'\r\n", s_f1);
                                sb.AppendFormat("with the face named '{0}' to be the outside of the joint.", s_e1);
                                prose = sb.ToString();

                                _result = dt.Pins(cs1);

                                facesToBeLabeled.Add(_result.FindFace(s_s1, s_e1));
                            }
                            break;
                        }
                    case Action.DOVETAIL_TAILS:
                        {
                            string id = Get_String("id");
                            string path1 = Get_String("path1");
                            string path2 = Get_String("path2");
                            int numtails = Get_Eval_Integer("numtails");
                            Inches tailwidth = Get_Eval("tailwidth");
                            // TODO angle/slope

                            CompoundSolid cs1;
                            CompoundSolid cs2;
                            Solid s1;
                            Solid s2;
                            Face f1;
                            Face f2;
                            HalfEdge he1;
                            HalfEdge he2;

                            Lookup(Get_String("path1"), out cs1, out s1, out f1, out he1);
                            Lookup(Get_String("path2"), out cs2, out s2, out f2, out he2);

                            if (he1.face.GetQuality() != FaceQuality.EndGrain)
                            {
                                Warnings.Add("The face/edge for the tails should be endgrain");
                            }
                            if (he2.face.GetQuality() != FaceQuality.EndGrain)
                            {
                                Warnings.Add("The face/edge for the pins should be endgrain");
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Begin a dovetail joint called '{0}'.\r\nCut the tails in the board named '{1}'\r\n",
                                id,
                                s2.name);
                            sb.AppendFormat("in the face named '{0}'\r\n", f1.name);
                            sb.AppendFormat("with the face named '{0}' to be the outside of the joint.", he1.Opposite().face.name);
                            prose = sb.ToString();

                            Dovetail dt = new Dovetail(id, path1, path2, numtails, tailwidth, cs1, s1, f1, he1, cs2, s2, f2, he2);

                            CompoundSolid newcs2 = dt.Tails(cs2);

                            this.plan.Dovetails[id] = dt;

#if DEBUG
                            newcs2.AssertNoNameClashes();
#endif

                            if (!newcs2.IsValidWithNoSubOverlaps())
                            {
                                Errors.Add("Invalid join:  Two boards cannot occupy the same space.");
                            }

                            _result = newcs2;

                            facesToBeLabeled.Add(_result.FindFace(s2.name, he2.Opposite().face.name));

                            break;
                        }
                }
                Debug.Assert(prose != null);
            }
            catch (Exception e)
            {
                this.Errors.Add(e.Message);
            }

            if (_result != null)
            {
                foreach (Annotation_FaceToFace a in annotations_FF)
                {
                    Solid s1;
                    Face f1;
                    HalfEdge he1;

                    Solid s2;
                    Face f2;
                    HalfEdge he2;

                    _result.FindPath(a.path1, out s1, out f1, out he1);
                    _result.FindPath(a.path2, out s2, out f2, out he2);

                    xyz p1 = he1.Center();

                    xyz n = f2.myPlane.n;
                    xyz p0 = f2.myPlane.pts[0];

                    xyz ptfar = p1 - f1.UnitNormal() * 1000;

                    double u = xyz.dotsub(n, p0, p1) / xyz.dotsub(n, ptfar, p1);
                    xyz p2 = (ptfar - p1).multiply_in_place(u).add_in_place(p1);

                    this.annotations_PP.Add(new Annotation_PointToPoint(p1, p2, -he1.GetInwardNormal(), a.offset, a.size));
                }
            }
        }
    }
}
