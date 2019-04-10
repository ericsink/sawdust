
using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using EricSinkMultiCoreLib;

namespace sd
{
    internal abstract class RouterBit
    {
        abstract public List<xyz> GetProfile(HalfEdge he);
    }

    internal class RoundoverBit : RouterBit
    {
        private double radius;

        public RoundoverBit(double r)
        {
            radius = r;
        }

        override public List<xyz> GetProfile(HalfEdge he)
        {
            List<xyz> pts = new List<xyz>();

            pts.Add(he.from.copy());
            xyz center = he.from + he.GetInwardNormal() * radius + he.Opposite().GetInwardNormal() * radius;
            xyz xv = -he.GetInwardNormal();
            xyz yv = -he.Opposite().GetInwardNormal();

            const int steps = 7;

            for (int i = steps; i >= 0; i--)
            {
                double deg = i * 90.0 / steps;
                double rad = ut.DegreeToRadian(deg);
                double x = radius * Math.Cos(rad);
                double y = radius * Math.Sin(rad);
                xyz pt = center + x * xv + y * yv;
                pts.Add(pt);
            }

            return pts;
        }
    }

    internal class ChamferBit : RouterBit
    {
        private double delta;

        public ChamferBit(double d)
        {
            delta = d;
        }

        override public List<xyz> GetProfile(HalfEdge he)
        {
            List<xyz> pts = new List<xyz>();

            xyz p1 = he.from.copy();
            xyz p2 = he.from + he.GetInwardNormal() * delta;
            xyz p3 = he.from + he.Opposite().GetInwardNormal() * delta;

#if not
            xyz p1off = (- he.GetInwardNormal() - he.Opposite().GetInwardNormal()).normalize();
            xyz p2off = (p2 - p3).normalize();
            xyz p3off = -p2off;

            pts.Add(p1 + p1off);
            pts.Add(p2 + p2off);
            pts.Add(p3 + p3off);
#endif
            pts.Add(p1);
            pts.Add(p2);
            pts.Add(p3);

            return pts;
        }
    }

    internal class RabbetBit : RouterBit
    {
        private double delta1;
        private double delta2;

        public RabbetBit(double d1, double d2)
        {
            delta1 = d1;
            delta2 = d2;
        }

        override public List<xyz> GetProfile(HalfEdge he)
        {
            List<xyz> pts = new List<xyz>();

            pts.Add(he.from.copy());
            pts.Add(he.from + he.GetInwardNormal() * delta1);
            pts.Add(he.from + he.GetInwardNormal() * delta1 + he.Opposite().GetInwardNormal() * delta2);
            pts.Add(he.from + he.Opposite().GetInwardNormal() * delta2);

            return pts;
        }
    }

    internal class wood
    {
        private static void CreateCutter_Drill(Solid s, xyz center, xyz vx, xyz nrml, double diameter, double depth, string id)
        {
            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz vz = -nrml;

            int num_slices = (int)(Math.PI * diameter / 0.25);  // make each slice no more than a quarter inch
            if (num_slices < 8)
            {
                num_slices = 8;  // rather minimal curve.  this is 2 slices per quadrant of the cylinder
            }
            double radius = diameter / 2;

            double degrees_per_slice = 360.0 / num_slices;

            int[] indices_bot = new int[num_slices];
            int[] indices_top = new int[num_slices];
            string prefix = id + "_slice_";
            for (int i = 0; i < num_slices; i++)
            {
                double angle = i * degrees_per_slice;
                double radians = angle * Math.PI / 180.0;

                double x = radius * Math.Cos(radians);
                double y = radius * Math.Sin(radians);
                xyz p1 = (x * vx).add_in_place(center).add_in_place(y * vy);
                xyz p2 = (depth * vz).add_in_place(p1);

                indices_bot[i] = s.GetOrCreateVertex_ReturnIndex(p1);
                indices_top[i] = s.GetOrCreateVertex_ReturnIndex(p2);
                if (i != 0)
                {
                    s.CreateFaceCW(prefix + i.ToString(), indices_bot[i - 1], indices_bot[i], indices_top[i], indices_top[i - 1]);
                }
            }
            s.CreateFaceCW(id + "_final_slice", indices_bot[num_slices - 1], indices_bot[0], indices_top[0], indices_top[num_slices - 1]);
            int[] indices_bot_reversed = new int[num_slices];
            int j = 0;
            for (int i = num_slices - 1; i >= 0; i--)
            {
                indices_bot_reversed[j++] = indices_bot[i];
            }

            s.CreateFaceCW(id + "_bottom", indices_bot_reversed);
            s.CreateFaceCW(id + "_top", indices_top);
        }

        internal static Solid CreateCylinder(string solname, double radius, double height, int num_slices)
        {
            return CreateCylinder(solname, radius, height, num_slices, "cyl_bot", "cyl_top");
        }

        private static Solid CreateCylinder(string solname, double radius, double height, int num_slices, string name_bottom, string name_top)
        {
            Solid s = new Solid(solname, BoardMaterial.Find(BoardMaterial.NONE));

            double degrees_per_slice = 360.0 / num_slices;

            int[] indices_bot = new int[num_slices];
            int[] indices_top = new int[num_slices];
            for (int i = 0; i < num_slices; i++)
            {
                double angle = i * degrees_per_slice;
                double radians = angle * Math.PI / 180.0;

                double x = radius * Math.Cos(radians);
                double y = radius * Math.Sin(radians);
                indices_bot[i] = s.AddVertex(x, y, 0);
                indices_top[i] = s.AddVertex(x, y, height);
                if (i != 0)
                {
                    s.CreateFaceCW(string.Format("cyl_slice_{0}", i), indices_bot[i - 1], indices_bot[i], indices_top[i], indices_top[i - 1]);
                }
            }
            s.CreateFaceCW("cyl_final_slice", indices_bot[num_slices - 1], indices_bot[0], indices_top[0], indices_top[num_slices - 1]);
            int[] indices_bot_reversed = new int[num_slices];
            int j = 0;
            for (int i = num_slices - 1; i >= 0; i--)
            {
                indices_bot_reversed[j++] = indices_bot[i];
            }

            s.CreateFaceCW(name_bottom, indices_bot_reversed);
            s.CreateFaceCW(name_top, indices_top);

            s.RecalcFacePlanes();

            return s;
        }

        private static Solid CreateCutter_Box(string name, xyz origin, xyz vx, xyz nrml, xyz size, string[] fnames)
        {
            Solid s = new Solid(name, BoardMaterial.Find(BoardMaterial.NONE));
            return CreateCutter_Box(s, origin, vx, nrml, size, fnames);
        }

        private static Solid CreateCutter_Box(Solid s, xyz origin, xyz vx, xyz nrml, xyz size, string[] fnames)
        {
            double dx = size.x;
            double dy = size.y;
            double dz = size.z;

            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz vz = -nrml;

            int basevert = s.Vertices.Count;

            for (int i = 0; i <= 1; i++)
            {
                double lz = i;
                s.AddVertex(origin + lz * dz * vz);	// 0,4
                s.AddVertex(origin + dx * vx + lz * dz * vz);	// 1,5
                s.AddVertex(origin + dx * vx + dy * vy + lz * dz * vz);	// 2,6
                s.AddVertex(origin + dy * vy + lz * dz * vz);	// 2,6
            }

            s.CreateFace(fnames[0], basevert + 0, basevert + 1, basevert + 2, basevert + 3);
            s.CreateFace(fnames[1], basevert + 1, basevert + 5, basevert + 6, basevert + 2);
            s.CreateFace(fnames[2], basevert + 0, basevert + 3, basevert + 7, basevert + 4);
            s.CreateFace(fnames[3], basevert + 7, basevert + 6, basevert + 5, basevert + 4);
            s.CreateFace(fnames[4], basevert + 0, basevert + 4, basevert + 5, basevert + 1);
            s.CreateFace(fnames[5], basevert + 3, basevert + 2, basevert + 6, basevert + 7);

            s.RecalcFacePlanes();

            return s;
        }

        private static void CreateCutter_Drill(Solid s, HalfEdge he, xy pos, double diameter, double depth, string id)
        {
            Face f = he.face;

            xyz vx = (he.to - he.from).normalize_in_place();
            xyz nrml = f.UnitNormal();
            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz center = (vx * pos.x).add_in_place(he.from).add_in_place(vy * pos.y);

            CreateCutter_Drill(s, center, vx, nrml, diameter, depth, id);
        }

        private static void CreateCutter_Box(Solid s, HalfEdge he, xy pos, xyz size, string[] fnames)
        {
            Face f = he.face;
            xyz vx = (he.to - he.from).normalize_in_place();
            xyz nrml = f.UnitNormal();
            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz origin = (vx * pos.x).add_in_place(he.from).add_in_place(vy * pos.y);
            CreateCutter_Box(s, origin, vx, nrml, size, fnames);
        }

        private static Solid CreateCutter_Box(string name, HalfEdge he, xy pos, xyz size, string[] fnames)
        {
            Face f = he.face;
            xyz vx = (he.to - he.from).normalize_in_place();
            xyz nrml = f.UnitNormal();
            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz origin = (vx * pos.x).add_in_place(he.from).add_in_place(vy * pos.y);
            return CreateCutter_Box(name, origin, vx, nrml, size, fnames);
        }

        public static CompoundSolid Drill(CompoundSolid sol, HalfEdge he, Inches x, Inches y, int count, Inches dx, Inches dy, double diameter, double depth, string id)
        {
            Face f = he.face;

            Solid cutter = new Solid(id, BoardMaterial.Find(BoardMaterial.NONE));

            for (int i = 0; i < count; i++)
            {
                CreateCutter_Drill(cutter, he, new xy(x, y), diameter, depth, (i + 1).ToString());
                x += dx;
                y += dy;
            }

            cutter.RecalcFacePlanes();

            CompoundSolid cs = bool3d.Subtract(sol, cutter);

            // find all the edges between slices we just drilled and mark those edges for no lines.
            // TODO this is probably not a very efficient way to do this, but it works
            foreach (Solid s in cs.Subs)
            {
#if not
                foreach (Edge e in s.Edges)
                {
                    string sf1 = e.a2b.face.name;
                    string sf2 = e.b2a.face.name;
                    if (sf1.StartsWith(id) && sf2.StartsWith(id) && (-1 != sf1.IndexOf("_slice_")) && (-1 != sf2.IndexOf("_slice_")))
                    {
                        e.NoLine = true;
                    }
                }
#endif

                foreach (Face fc in s.Faces)
                {
                    if (fc.name.StartsWith(id) && (-1 != fc.name.IndexOf("_slice")))
                    {
                        fc.Shade = true;
                    }
                }
            }

            return cs;
        }

        public static CompoundSolid Tenon(CompoundSolid sol, HalfEdge he, xy pos, xyz size, string id)
        {
            Face f = he.face;

            EdgeLoop el = he.face.GetEdgeLoopFor(he);
            int ndx = el.IndexOf(he);
            ndx--;
            if (ndx < 0)
            {
                ndx += el.Count;
            }
            HalfEdge prevHe = el[ndx];

            if (!fp.eq_inches(pos.y, 0))
            {
                Solid cut = CreateCutter_Box(id, he, new xy(0, 0), new xyz(he.Length(), pos.y, size.z),
                    new string[] { "NA_1", "NA_2", "NA_3", "frontshoulder", "NA_4", "front" });
                sol = bool3d.Subtract(sol, cut);
            }
            if (!fp.eq_inches(pos.x, 0))
            {
                Solid cut = CreateCutter_Box(id, he, new xy(0, pos.y), new xyz(pos.x, size.y, size.z),
                    new string[] { "NA_1", "left", "NA_2", "leftshoulder", "NA_3", "NA_4" });
                sol = bool3d.Subtract(sol, cut);
            }
            if ((pos.y + size.y) < prevHe.Length())
            {
                Solid cut = CreateCutter_Box(id, he, new xy(0, pos.y + size.y), new xyz(he.Length(), prevHe.Length() - (pos.y + size.y), size.z),
                    new string[] { "NA_1", "NA_2", "NA_3", "backshoulder", "back", "NA_4" });
                sol = bool3d.Subtract(sol, cut);
            }
            if ((pos.x + size.x) < he.Length())
            {
                Solid cut = CreateCutter_Box(id, he, new xy((pos.x + size.x), pos.y), new xyz(he.Length() - (pos.x + size.x), size.y, size.z),
                    new string[] { "NA_1", "NA_2", "right", "rightshoulder", "NA_3", "NA_4" });
                sol = bool3d.Subtract(sol, cut);
            }
            return sol;
        }

        public static void FindActualEdge(HalfEdge he, out HalfEdge he1, out double length)
        {
            EdgeLoop el = he.face.GetEdgeLoopFor(he);
            int ndx = el.IndexOf(he);
            while (true)
            {
                HalfEdge h = el[el.FixIndex(ndx - 1)];
                if (ut.PointsAreCollinear3d(h.from, h.to, he.to))
                {
                    ndx--;
                }
                else
                {
                    break;
                }
            }
            he1 = el[ndx];
            double len = he1.Length();
            while (true)
            {
                HalfEdge h = el[++ndx];
                if (ut.PointsAreCollinear3d(he1.from, he1.to, h.to))
                {
                    len += h.Length();
                }
                else
                {
                    break;
                }
            }
            length = len;
        }

        public static CompoundSolid DoChamfer(CompoundSolid sol, HalfEdge he, double inset, string id)
        {
            RouterBit rbit = new ChamferBit(inset);
            return EdgeTreatment(sol, he, rbit, id);
        }

        public static CompoundSolid DoRabbet(CompoundSolid sol, HalfEdge he, double inset, double depth, string id)
        {
            RouterBit rbit = new RabbetBit(inset, depth);
            return EdgeTreatment(sol, he, rbit, id);
        }

        public static CompoundSolid DoRoundover(CompoundSolid sol, HalfEdge he, double radius, string id)
        {
            RouterBit rbit = new RoundoverBit(radius);
            return EdgeTreatment(sol, he, rbit, id);
        }

        public static CompoundSolid EdgeTreatment(CompoundSolid sol, HalfEdge he, RouterBit bit, string name)
        {
            HalfEdge he1;
            double length;
            FindActualEdge(he, out he1, out length);

            List<xyz> pts = bit.GetProfile(he1);

            xyz uv = he1.UnitVector();
            double extra = 200; // TODO this is a bit of an exaggeration
            xyz tv = -extra * uv;
            ut.TranslatePoints(pts, tv.x, tv.y, tv.z);

            Solid cut = Solid.Sweep(name, BoardMaterial.Find(BoardMaterial.NONE), pts, uv * (length + extra * 2));

            CompoundSolid result = bool3d.Subtract(sol, cut);
            return result;
        }

        public static CompoundSolid Mortise(CompoundSolid sol, HalfEdge he, xy pos, xyz size, string id)
        {
            return Mortise(sol, he, pos, size, 1, 0, 0, id);
        }

        public static CompoundSolid Mortise(CompoundSolid sol, HalfEdge he, xy pos, xyz size, int count, Inches dx, Inches dy, string id)
        {
            Solid cutter;
            if (count == 1)
            {
                string[] fnames = new string[] {
                                               "NA",
                                               "right",
                                               "left",
                                               "bottom",
                                               "front",
                                               "back",
                                           };
                cutter = CreateCutter_Box(id, he, pos, size, fnames);
            }
            else
            {
                cutter = new Solid(id, BoardMaterial.Find(BoardMaterial.NONE));

                Inches x = pos.x;
                Inches y = pos.y;
                for (int i = 0; i < count; i++)
                {
                    string[] fnames = new string[] {
                                               string.Format("NA_{0}", i+1),
                                               string.Format("right_{0}", i+1),
                                               string.Format("left_{0}", i+1),
                                               string.Format("bottom_{0}", i+1),
                                               string.Format("front_{0}", i+1),
                                               string.Format("back_{0}", i+1),
                                           };
                    CreateCutter_Box(cutter, he, new xy(x, y), size, fnames);
                    x += dx;
                    y += dy;
                }

                cutter.RecalcFacePlanes();
            }

            return bool3d.Subtract(sol, cutter);
        }

        public static Solid CreateBoard(BoardMaterial bm, string name, Inches width, Inches length, Inches thickness)
        {
            Solid s = new Solid(name, bm);

            double dx = width;
            double dy = length;
            double dz = thickness;

            s.AddVertex(0, 0, 0);       // 0
            s.AddVertex(dx, 0, 0);      // 1
            s.AddVertex(dx, dy, 0);     // 2
            s.AddVertex(0, dy, 0);      // 3
            s.AddVertex(0, 0, -dz);     // 4
            s.AddVertex(dx, 0, -dz);        // 5
            s.AddVertex(dx, dy, -dz);   // 6
            s.AddVertex(0, dy, -dz);        // 7

            Face top = s.CreateFace("top", 0, 1, 2, 3);
            Face right = s.CreateFace("right", 1, 5, 6, 2);
            Face left = s.CreateFace("left", 0, 3, 7, 4);
            Face bottom = s.CreateFace("bottom", 7, 6, 5, 4);
            Face end1 = s.CreateFace("end1", 0, 4, 5, 1);
            Face end2 = s.CreateFace("end2", 3, 2, 6, 7);

            s.board_origin = new xyz(0, 0, 0);
            s.board_u = new xyz(dx, 0, 0);
            s.board_v = new xyz(0, dy, 0);
            s.board_w = new xyz(0, 0, -dz);

            s.RecalcFacePlanes();

            return s;
        }

#if DEBUG
        // this is the old API for a mortise, but it's implemented using the new one.  It is now only used by unit tests.
        public static Solid Mortise(Face f, int originpt, int otherptforaxis, double x, double y, double xsize, double ysize, Inches depth, string id)
        {
            List<xyz> a = f.MainLoop.CollectAllVertices();
            xyz v2 = a[otherptforaxis];
            xyz v1 = a[originpt];
            HalfEdge he = null;
            foreach (HalfEdge h in f.MainLoop)
            {
                if (
                    (h.from == v1)
                    && (h.to == v2)
                    )
                {
                    he = h;
                    break;
                }
            }
            CompoundSolid cs = Mortise(f.solid.ToCompoundSolid(), he, new xy(x, y), new xyz(xsize, ysize, depth), id);
            return cs[0];
        }
#endif

        public static CompoundSolid Crosscut_Or_Rip(CompoundSolid sol, HalfEdge he, Inches distAlongEdge, double miter, double tilt)
        {
            Face f = he.face;

            // TODO fix these names
            string[] fnames = new string[] {
                                               "miter_top",
                                               "miter_right",
                                               "miter_left",
                                               "miter_bottom",
                                               "miter", // the primary face name which gets left
											   "miter_below",
            };

            xyz nrml = f.UnitNormal();
            xyz origin = he.to + he.GetInwardNormal() * distAlongEdge;

            // vx is in the direction of the cut
            xyz vx = ut.RotateUnitVector(-he.UnitVector(), -miter, nrml);

            // now rotate nrml for tilt
            nrml = ut.RotateUnitVector(nrml, tilt, vx);

            // origin is going to be the corner of the cutter box.
            // right now it is sitting on the face.  we need to
            // move it further away.  calculate intersections with
            // the boundingbox of the compoundsolid as a whole.

            BoundingBox3d bb = sol.GetBoundingBox();
            double d_vx = bb.IntersectRay_Planes_Max(origin, -vx);
            double d_nrml = bb.IntersectRay_Planes_Max(origin, nrml);

            // TODO this approach creates a bb which is too big.  do we care?

            // now move origin far enough back behind the face
            origin.subtract_in_place((d_vx + 2) * vx);
            // and up above the face
            origin = origin + (d_nrml + 2) * nrml;

            xyz vy = xyz.cross(nrml, vx).normalize_in_place();
            xyz vz = -nrml;

            d_vx = bb.IntersectRay_Planes_Max(origin, vx);
            double d_vy = bb.IntersectRay_Planes_Max(origin, vy);
            double d_vz = bb.IntersectRay_Planes_Max(origin, vz);

            // now calculate proper sizes for the cutter
            double dx = d_vx + 2;
            double dy = d_vy + 2;
            double dz = d_vz + 2;

            Solid cutter = CreateCutter_Box(string.Format("{0}_{1}", f.name, he.Opposite().face.name), origin, vx, nrml, new xyz(dx, dy, dz), fnames);
            //sol = sol.Clone();
            //sol.AddSub(cutter);
            //return sol;
            return bool3d.Subtract(sol, cutter);
        }

        public static CompoundSolid Dado(CompoundSolid sol, HalfEdge heParallel, Inches distFromParallelEdge, Inches width, Inches depth, string id)
        {
            Face f = heParallel.face;
            string[] fnames = new string[] {
                                               "top",
                                               "right",
                                               "left",
                                               "bottom",
                "side_above",
                "side_below"
            };

            // TODO should we move left or right further?  what if the dado needs to be longer than this edge?  or shorter?

            Solid cutter = CreateCutter_Box(id, heParallel, new xy(0, distFromParallelEdge), new xyz(heParallel.Length(), width, depth), fnames);
            return bool3d.Subtract(sol, cutter);
        }
    }
}
