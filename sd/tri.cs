
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class Triangle2d
    {
        public xy a;
        public xy b;
        public xy c;

        public Triangle2d(xy _a, xy _b, xy _c)
        {
            a = _a;
            b = _b;
            c = _c;

            Debug.Assert(ut.PointSideOfLine(c, a, b) < 0);
#if false
            if (ut.PointSideOfLine(c, a, b) >= 0)
			{
				throw new GeomCheckException("Triangle must be counterclockwise, not collinear");
			}
#endif
        }

        public double Area()
        {
            // abs((xB*yA-xA*yB)+(xC*yB-xB*yC)+(xA*yC-xC*yA))/2
            return Math.Abs((b.x * a.y - a.x * b.y) + (c.x * b.y - b.x * c.y) + (a.x * c.y - c.x * a.y)) / 2;
        }

        internal class TriSegIntersect
        {
            public xy q1;
            public xy q2;

            public void Add(xy p)
            {
                Debug.Assert(
                    (q1 == null)
                    || fp.eq_inches(q1, p)
                    || (q2 == null)
                    || fp.eq_inches(q2, p)
                    );

                if (q1 == null)
                {
                    q1 = p;
                    return;
                }
                if (fp.eq_inches(q1, p))
                {
                    return;
                }
                if (q2 == null)
                {
                    q2 = p;
                    return;
                }
                if (fp.eq_inches(q2, p))
                {
                    return;
                }
            }
        }

        internal bool SegmentIntersection(xy p, xy q, out xy pt1, out xy pt2)
        {
            TriSegIntersect ti = new TriSegIntersect();
            if (this.PointInside(p))
            {
                ti.Add(p);
            }
            if (this.PointInside(q))
            {
                ti.Add(q);
            }
            xy z1, z2;
            SegIntersection si = ut.GetSegIntersection(a, b, p, q, out z1, out z2);
            if (si == SegIntersection.Overlap)
            {
                pt1 = z1;
                pt2 = z2;
                return true;
            }
            if (si == SegIntersection.Point)
            {
                ti.Add(z1);
            }

            si = ut.GetSegIntersection(b, c, p, q, out z1, out z2);
            if (si == SegIntersection.Overlap)
            {
                pt1 = z1;
                pt2 = z2;
                return true;
            }
            if (si == SegIntersection.Point)
            {
                ti.Add(z1);
            }

            si = ut.GetSegIntersection(c, a, p, q, out z1, out z2);
            if (si == SegIntersection.Overlap)
            {
                pt1 = z1;
                pt2 = z2;
                return true;
            }
            if (si == SegIntersection.Point)
            {
                ti.Add(z1);
            }

            if (ti.q1 == null)
            {
                pt1 = null;
                pt2 = null;
                return false;
            }

            pt1 = ti.q1;
            if (ti.q2 != null)
            {
                pt2 = ti.q2;
            }
            else
            {
                pt2 = null;
            }
            return true;
        }

        public bool PointInside(xy p)
        {
            // this assumes that the triangle (a,b,c) is ccw
            return (
                (ut.PointSideOfLine(p, a, b) <= 0)
                && (ut.PointSideOfLine(p, b, c) <= 0)
                && (ut.PointSideOfLine(p, c, a) <= 0)
                );
        }

        public Triangle3d ConvertBackTo3d()
        {
            return new Triangle3d(a.orig, b.orig, c.orig);
        }
    }

    public class BunchOfTriBags
    {
        // TODO extend this to allow more than one move vector
        public xyz vec;
        public List<TriBag> moving;
        public List<TriBag> notmoving;
    }

    public class TriBag
    {
        public Solid solid;
        public List<Triangle3d> tris = new List<Triangle3d>();
        public List<Line3d> lines = new List<Line3d>();

        public TriBag(Solid s)
        {
            solid = s;
            solid.GetTriangles(tris);
            solid.GetLines(lines);
        }
    }

    public class Triangle3d
    {
        public xyz a;
        public xyz b;
        public xyz c;

        public xyz n;

        // TODO if mem usage becomes a problem, take these back out
        internal xyz iv;
        internal xyz jv;

        public Face face;

        internal Triangle3d(xyz _a, xyz _b, xyz _c)
        {
#if true // we need this so triangles can share xyz instances so we can share them in wpf so wpf can shade them properly
            a = _a;
            b = _b;
            c = _c;
#else
			a = _a.copy();
			b = _b.copy();
			c = _c.copy();
#endif

            n = xyz.normal(a, b, c).normalize_in_place();
            iv = (b - a).normalize_in_place();
            jv = xyz.cross(n, iv).normalize_in_place();

            Debug.Assert(!ut.PointsAreCollinear3d(a, b, c));
#if not
			if (ut.PointsAreCollinear3d(a, b, c))
			{
				throw new GeomCheckException("Attempt to create triangle with 3 collinear points.");
			}
#endif
        }

        internal double Area()
        {
            // TODO is this calculation the same as something else, like the normal?
            xyz v1 = b - a;
            xyz v2 = c - a;
            xyz cr = xyz.cross(v1, v2);
            double mag = cr.magnitude();
            return mag / 2;
        }

        internal bool PointInside(xyz p)
        {
#if false
			Triangle2d t2d = this.ConvertTo2d(a, iv, jv);
			xy pt2d = ut.ConvertPointTo2d(p, a, iv, jv);

			return t2d.PointInside(pt2d);
#else
            if (fp.lt_unknowndata(xyz.dot(xyz.cross_subs(p, a, b), n), 0))
            {
                return false;
            }
            else if (fp.lt_unknowndata(xyz.dot(xyz.cross_subs(p, b, c), n), 0))
            {
                return false;
            }
            else if (fp.lt_unknowndata(xyz.dot(xyz.cross_subs(p, c, a), n), 0))
            {
                return false;
            }
            else
            {
                return true;
            }
#endif
        }

        internal Triangle2d ConvertTo2d()
        {
            // we assume a is the origin
            return ConvertTo2d(a, iv, jv);
        }

        internal Triangle2d ConvertTo2d(xyz i, xyz j)
        {
            // we assume a is the origin
            return ConvertTo2d(a, i, j);
        }

        internal void Classify(xyz a, out WhichSideOfPlane sa, xyz b, out WhichSideOfPlane sb, xyz c, out WhichSideOfPlane sc)
        {
            sa = Classify(a);
            sb = Classify(b);
            sc = Classify(c);
        }

        internal WhichSideOfPlane Classify(xyz p)
        {
            double ktest = xyz.dotsub(n, p, a);
            if (fp.eq_dot_distancetoplane(ktest, 0))
            {
                return WhichSideOfPlane.Coplanar;
            }
            else if (ktest > 0)
            {
                return WhichSideOfPlane.Outside;
            }
            else
            {
                return WhichSideOfPlane.Inside;
            }
        }

        internal int GetSide(xyz p)
        {
            double ktest = xyz.dotsub(n, p, a);
            return fp.getsign_dot_distancetoplane(ktest);
        }

        private bool FindSegmentIntersectionWithPlane(xyz d, xyz e, out xyz pt1)
        {
            // get the signed distance from each point to the plane of the triangle
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the triangle.

            double dist_d = xyz.dotsub(n, d, a);
            double dist_e = xyz.dotsub(n, e, a);

            int sign_d = fp.getsign_dot_distancetoplane(dist_d);
            int sign_e = fp.getsign_dot_distancetoplane(dist_e);

            if (
                (sign_d > 0)
                && (sign_e > 0)
                )
            {
                pt1 = null;
                return false;
            }

            if (
                (sign_d < 0)
                && (sign_e < 0)
                )
            {
                pt1 = null;
                return false;
            }

            Debug.Assert(!(
                (sign_d == 0)
                && (sign_e == 0)
                ));

            if (sign_d == 0)
            {
                pt1 = null;
                return false;
            }
            else if (sign_e == 0)
            {
                pt1 = null;
                return false;
            }
            else
            {
                // find the point where the segment intersects the plane
                // http://astronomy.swin.edu.au/~pbourke/geometry/planeline/
                double u = xyz.dotsub(n, a, d) / xyz.dotsub(n, e, d);
                pt1 = (e - d).multiply_in_place(u).add_in_place(d);
                return true;
            }
        }

        // TODO note that this method isn't used except in the unit tests
        internal void Split(Triangle3d t, List<Triangle3d> tris_in, List<Triangle3d> tris_out)
        {
            Debug.Assert(tris_in != null);
            Debug.Assert(tris_out != null);
            Debug.Assert(tris_in.Count == 0);
            Debug.Assert(tris_out.Count == 0);

            WhichSideOfPlane sa;
            WhichSideOfPlane sb;
            WhichSideOfPlane sc;

            Classify(t.a, out sa, t.b, out sb, t.c, out sc);

            Debug.Assert(this.Classify(t) == WhichSideOfPlane.Split);

            xyz ab;
            xyz bc;
            xyz ca;

            FindSegmentIntersectionWithPlane(t.a, t.b, out ab);
            FindSegmentIntersectionWithPlane(t.b, t.c, out bc);
            FindSegmentIntersectionWithPlane(t.c, t.a, out ca);

            Triangle3d t1, t2, t3;

            if (
                (ab == null)
                && (bc != null)
                && (ca == null)
                )
            {
                // the plane passes through a.  the result is one tri on each side.
                t1 = new Triangle3d(t.a, t.b, bc);
                t2 = new Triangle3d(t.a, bc, t.c);

                if (sb == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                }
                else
                {
                    tris_in.Add(t2);
                    tris_out.Add(t1);
                }
            }
            else if (
                (ab != null)
                && (bc == null)
                && (ca == null)
                )
            {
                // the plane passes through t.c
                t1 = new Triangle3d(t.a, ab, t.c);
                t2 = new Triangle3d(t.b, t.c, ab);

                if (sa == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                }
                else
                {
                    tris_in.Add(t2);
                    tris_out.Add(t1);
                }
            }
            else if (
                (ab == null)
                && (bc == null)
                && (ca != null)
                )
            {
                // the plane passes through t.b
                t1 = new Triangle3d(t.a, t.b, ca);
                t2 = new Triangle3d(t.c, ca, t.b);

                if (sa == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                }
                else
                {
                    tris_in.Add(t2);
                    tris_out.Add(t1);
                }
            }
            else if (
                (ab != null)
                && (bc == null)
                && (ca != null)
                )
            {
                // the plane cuts through ab and ca.  t.b and t.c are one side, t.a is on the other
                t1 = new Triangle3d(t.a, ab, ca);
                t2 = new Triangle3d(t.b, ca, ab);
                t3 = new Triangle3d(t.c, ca, t.b);

                if (sa == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                    tris_out.Add(t3);
                }
                else
                {
                    tris_out.Add(t1);
                    tris_in.Add(t2);
                    tris_in.Add(t3);
                }
            }
            else if (
                (ab != null)
                && (bc != null)
                && (ca == null)
                )
            {
                // the plane cuts through ab and bc.  t.a and t.c are one side, t.b is on the other
                t1 = new Triangle3d(t.b, bc, ab);
                t2 = new Triangle3d(t.a, ab, bc);
                t3 = new Triangle3d(t.c, t.a, bc);

                if (sb == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                    tris_out.Add(t3);
                }
                else
                {
                    tris_out.Add(t1);
                    tris_in.Add(t2);
                    tris_in.Add(t3);
                }
            }
            else if (
                (ab == null)
                && (bc != null)
                && (ca != null)
                )
            {
                // the plane cuts through bc and ca.  t.a and t.b are one side, t.c is on the other
                t1 = new Triangle3d(t.c, ca, bc);
                t2 = new Triangle3d(t.a, t.b, bc);
                t3 = new Triangle3d(t.a, bc, ca);

                if (sc == WhichSideOfPlane.Inside)
                {
                    tris_in.Add(t1);
                    tris_out.Add(t2);
                    tris_out.Add(t3);
                }
                else
                {
                    tris_out.Add(t1);
                    tris_in.Add(t2);
                    tris_in.Add(t3);
                }
            }

            Debug.Assert(tris_in.Count > 0);
            Debug.Assert(tris_out.Count > 0);

#if BSP_DEBUG
			xyz n = t.normal().normalize();
			foreach (Triangle3d tq in tris_in)
			{
				Console.Out.WriteLine("Split:  inside: {0} -- {1} -- {2}", tq.a, tq.b, tq.c);
				xyz nq = tq.normal().normalize();
				if (!ut.eq(n,nq))
				{
					throw new ShouldNeverHappenException("normals wrong");
				}
				WhichSideOfPlane wsop = this.Classify(tq);
				if (wsop != WhichSideOfPlane.Inside)
				{
					throw new ShouldNeverHappenException("should be inside");
				}
			}
			foreach (Triangle3d tq in tris_out)
			{
				Console.Out.WriteLine("Split:  outside: {0} -- {1} -- {2}", tq.a, tq.b, tq.c);
				xyz nq = tq.normal().normalize();
				if (!ut.eq(n,nq))
				{
					throw new ShouldNeverHappenException("normals wrong");
				}
				WhichSideOfPlane wsop = this.Classify(tq);
				if (wsop != WhichSideOfPlane.Outside)
				{
					throw new ShouldNeverHappenException("should be Outside");
				}
			}
			Console.Out.WriteLine();
#endif
        }

        internal WhichSideOfPlane Classify(Triangle3d t)
        {
            WhichSideOfPlane sa;
            WhichSideOfPlane sb;
            WhichSideOfPlane sc;

            Classify(t.a, out sa, t.b, out sb, t.c, out sc);

            if (
                (sa == sb)
                && (sa == sc)
                )
            {
                return sa;
            }

            bool bOutside;
            bool bInside;

            if (
                (sa == WhichSideOfPlane.Outside)
                || (sb == WhichSideOfPlane.Outside)
                || (sc == WhichSideOfPlane.Outside)
                )
            {
                bOutside = true;
            }
            else
            {
                bOutside = false;
            }

            if (
                (sa == WhichSideOfPlane.Inside)
                || (sb == WhichSideOfPlane.Inside)
                || (sc == WhichSideOfPlane.Inside)
                )
            {
                bInside = true;
            }
            else
            {
                bInside = false;
            }

            if (bOutside && bInside)
            {
                return WhichSideOfPlane.Split;
            }
            else if (bOutside)
            {
                return WhichSideOfPlane.Outside;
            }
            else
            {
                return WhichSideOfPlane.Inside;
            }
        }

        internal Triangle2d ConvertTo2d(xyz origin, xyz i, xyz j)
        {
            xy a2d = ut.ConvertPointTo2d(a, origin, i, j);
            xy b2d = ut.ConvertPointTo2d(b, origin, i, j);
            xy c2d = ut.ConvertPointTo2d(c, origin, i, j);

            return new Triangle2d(a2d, b2d, c2d);
        }
    }

    internal class tri
    {
        internal static void Triangulate3d_WithHoles(List<Triangle3d> tris, List<List<xyz>> loops)
        {
            List<xyz> main3d = loops[0];

            if (loops.Count > 1)
            {
                List<List<xyz>> holes3d = loops.GetRange(1, loops.Count - 1);
                List<xy> main2d = new List<xy>();
                List<List<xy>> holes2d = new List<List<xy>>();
                ut.Convert3dPointsTo2d(main3d, holes3d, main2d, holes2d);

                List<Triangle2d> mytris2d = new List<Triangle2d>();
                Triangulate2d_WithHoles(mytris2d, main2d, holes2d);
                Convert2dTrianglesBackTo3d(mytris2d, tris);
            }
            else
            {
                switch (main3d.Count)
                {
                    case 3:
                        {
                            tris.Add(new Triangle3d(main3d[0], main3d[1], main3d[2]));
                            break;
                        }
                    case 4:
                        {
                            tris.Add(new Triangle3d(main3d[0], main3d[1], main3d[2]));
                            tris.Add(new Triangle3d(main3d[0], main3d[2], main3d[3]));
                            break;
                        }
                    default:
                        {
                            Triangulate3d(tris, main3d);
                            break;
                        }
                }
            }
        }

        internal static void Triangulate2d_WithHoles(List<Triangle2d> tris, List<xy> main, List<List<xy>> holes)
        {
            List<List<xy>> loops = new List<List<xy>>();
            ut.RemoveHoles(loops, main, holes);
            foreach (List<xy> q in loops)
            {
                Triangulate2d(tris, q);
            }
        }

        internal static void Triangulate2d(List<Triangle2d> tris, List<xy> pts)
        {
            if (pts.Count == 3)
            {
                if (!ut.PointsAreCollinear2d(pts[0], pts[1], pts[2]))
                {
                    tris.Add(new Triangle2d(pts[0], pts[1], pts[2]));
                }
                return;
            }

            // TODO we could optimize the 4 pt case here

            for (int i0 = 0, i1 = 1, i2 = 2; i0 < pts.Count; i0++, i1 = (i1 + 1) % pts.Count, i2 = (i2 + 1) % pts.Count)
            {
                if (
                    (!ut.PointsAreCollinear2d(pts[i0], pts[i1], pts[i2]))
                    && (ut.IsDiagonal(pts, i0, i2))
                    )
                {
                    tris.Add(new Triangle2d(pts[i0], pts[i1], pts[i2]));
                    List<xy> sublist = new List<xy>();
                    for (int j = 0; j < pts.Count; j++)
                    {
                        if (j != i1)
                        {
                            sublist.Add(pts[j]);
                        }
                    }
                    Triangulate2d(tris, sublist);
                    return;
                }
            }
        }

        internal static void Convert2dTrianglesBackTo3d(List<Triangle2d> tris2d, List<Triangle3d> tris3d)
        {
            foreach (Triangle2d tri2d in tris2d)
            {
                Triangle3d t = tri2d.ConvertBackTo3d();
                tris3d.Add(t);
            }
        }

        internal static void Triangulate3d(List<Triangle3d> tris3d, List<xyz> pts)
        {
            List<Triangle2d> tris2d = new List<Triangle2d>();
            List<xy> pts2d = ut.Convert3dPointsTo2d(pts);
            Triangulate2d(tris2d, pts2d);
            Convert2dTrianglesBackTo3d(tris2d, tris3d);
        }
    }
}
