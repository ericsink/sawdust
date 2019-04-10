
using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal enum SegIntersection
    {
        None,
        Point,
        Overlap
    }

    internal enum SegmentsOverlap3d
    {
        None,
        OppositeDirection,
        SameSegment,
        P_On_Q,
        Q_On_P,
        Overlap
    }

    public class utpub
    {
        public static string FormatDimension(double d)
        {
            Inches i = (Inches)d;
            int whole;
            int num;
            int den;
            i.ToFraction(out whole, out num, out den);
            if (num == 0)
            {
                return whole.ToString();
            }
            else if (whole == 0)
            {
                return string.Format("{0}/{1}", num, den);
            }
            else
            {
                return string.Format("{0} {1}/{2}", whole, num, den);
            }
        }

    }

    internal class ut
    {
        public static void ParsePath(string path, out string sol, out string face, out string edge)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.Length > 0);

            string[] parts = path.Split('.');

            sol = parts[0];
            if (parts.Length > 1)
            {
                face = parts[1];
                if (parts.Length > 2)
                {
                    edge = parts[2];
                }
                else
                {
                    edge = null;
                }
            }
            else
            {
                face = null;
                edge = null;
            }

        }

        public static double ParseDouble(string s, double d)
        {
            try
            {
                return double.Parse(s);
            }
            catch
            {
                return d;
            }
        }

        public static bool AnySegmentsTouch(List<xy> p1, List<xy> p2)
        {
            for (int i = 0; i < p1.Count; i++)
            {
                xy p1a = p1[i];
                xy p1b = p1[(i + 1) % p1.Count];
                for (int j = 0; j < p2.Count; j++)
                {
                    xy p2a = p2[j];
                    xy p2b = p2[(j + 1) % p2.Count];

                    if (SegIntersection.None != ut.GetSegIntersection(p1a, p1b, p2a, p2b))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TouchesAnySegment(List<xy> p1, xy p2a, xy p2b)
        {
            for (int i = 0; i < p1.Count; i++)
            {
                xy p1a = p1[i];
                xy p1b = p1[(i + 1) % p1.Count];
                if (SegIntersection.None != ut.GetSegIntersection(p1a, p1b, p2a, p2b))
                {
                    return true;
                }
            }
            return false;
        }

        public static double RadianToDegree(double d)
        {
            return d * 180.0 / Math.PI;
        }

        public static double DegreeToRadian(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static List<xyz> MakePoly(params xyz[] verts)
        {
            List<xyz> a = new List<xyz>();
            foreach (xyz p in verts)
            {
                a.Add(p);
            }
            return a;
        }

        public static List<xy> MakePoly(params xy[] verts)
        {
            List<xy> a = new List<xy>();
            foreach (xy p in verts)
            {
                a.Add(p);
            }
            return a;
        }

        public static bool SamePoly2d(List<xy> a1, List<xy> a2)
        {
            if (a1.Count != a2.Count)
            {
                return false;
            }

            int i1 = 0;
            int i2 = -1;
            for (int i = 0; i < a2.Count; i++)
            {
                if (fp.eq_inches(a1[0], a2[i]))
                {
                    i2 = i;
                    break;
                }
            }
            if (i2 < 0)
            {
                return false;
            }

            for (int i = 0; i < a2.Count; i++)
            {
                if (!fp.eq_inches(a1[i1], a2[i2]))
                {
                    return false;
                }
                i1 = (i1 + 1) % a1.Count;
                i2 = (i2 + 1) % a2.Count;
            }

            return true;
        }

        public static bool SamePoly3d(List<xyz> a1, List<xyz> a2)
        {
            if (a1.Count != a2.Count)
            {
                return false;
            }

            int i1 = 0;
            int i2 = -1;
            for (int i = 0; i < a2.Count; i++)
            {
                if (fp.eq_inches(a1[0], a2[i]))
                {
                    i2 = i;
                    break;
                }
            }
            if (i2 < 0)
            {
                return false;
            }

            for (int i = 0; i < a2.Count; i++)
            {
                if (!fp.eq_inches(a1[i1], a2[i2]))
                {
                    return false;
                }
                i1 = (i1 + 1) % a1.Count;
                i2 = (i2 + 1) % a2.Count;
            }

            return true;
        }

        public static double GetMinX(List<xy> a)
        {
            double v = double.MaxValue;
            foreach (xy p in a)
            {
                if (p.x < v)
                {
                    v = p.x;
                }
            }
            return v;
        }

        public static double GetMinY(List<xy> a)
        {
            double v = double.MaxValue;
            foreach (xy p in a)
            {
                if (p.y < v)
                {
                    v = p.y;
                }
            }
            return v;
        }

        public static double GetMaxX(List<xy> a)
        {
            double v = double.MinValue;
            foreach (xy p in a)
            {
                if (p.x > v)
                {
                    v = p.x;
                }
            }
            return v;
        }

        public static double GetMaxY(List<xy> a)
        {
            double v = double.MinValue;
            foreach (xy p in a)
            {
                if (p.y > v)
                {
                    v = p.y;
                }
            }
            return v;
        }

        public static string GetPoly2dAsCode(List<xy> a)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ut.MakePoly( ");
            for (int i = 0; i < a.Count; i++)
            {
                xy p = a[i];
                sb.AppendFormat("new xy({0}, {1})", p.x, p.y);
                if (i != (a.Count - 1))
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        public static string GetPoly3dAsCode(List<xyz> a)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ut.MakePoly3d( ");
            for (int i = 0; i < a.Count; i++)
            {
                xyz p = a[i];
                sb.AppendFormat("new xyz({0}, {1}, {2})", p.x, p.y, p.z);
                if (i != (a.Count - 1))
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        internal static void DumpSegments3d(string label, List<seg3d> a)
        {
            Console.Out.WriteLine("{0}:", label);
            foreach (seg3d s in a)
            {
                Console.Out.WriteLine("    {0}", s);
            }
        }

        public static void DumpPoly2d(string label, List<xy> a)
        {
            Console.Out.Write(string.Format("{0}: ", label));
            DumpPoly2d(a);
        }

        public static void DumpPoly2d(List<xy> a)
        {
            Console.Out.WriteLine("DumpPoly2d:");
            foreach (xy p in a)
            {
                Console.Out.WriteLine("  {0}", p);
            }
        }

        public static void DumpPoly3d(string label, List<xyz> a)
        {
            Console.Out.Write(string.Format("{0}: ", label));
            DumpPoly3d(a);
        }

        public static void DumpPoly3d(List<xyz> a)
        {
            Console.Out.WriteLine("DumpPoly3d:");
            foreach (xyz p in a)
            {
                Console.Out.WriteLine("  {0}", p);
            }
        }

        public static void TranslatePoints(List<xyz> a, double x, double y, double z)
        {
            foreach (xyz p in a)
            {
                p.Translate(x, y, z);
            }
        }

        public static xyz RotateUnitVector(xyz p, double deg, xyz r)
        {
            double costheta = Math.Cos(ut.DegreeToRadian(deg));
            double sintheta = Math.Sin(ut.DegreeToRadian(deg));
            xyz q = new xyz(0, 0, 0);

            q.x += (costheta + (1 - costheta) * r.x * r.x) * p.x;
            q.x += ((1 - costheta) * r.x * r.y - r.z * sintheta) * p.y;
            q.x += ((1 - costheta) * r.x * r.z + r.y * sintheta) * p.z;

            q.y += ((1 - costheta) * r.x * r.y + r.z * sintheta) * p.x;
            q.y += (costheta + (1 - costheta) * r.y * r.y) * p.y;
            q.y += ((1 - costheta) * r.y * r.z - r.x * sintheta) * p.z;

            q.z += ((1 - costheta) * r.x * r.z - r.y * sintheta) * p.x;
            q.z += ((1 - costheta) * r.y * r.z + r.x * sintheta) * p.y;
            q.z += (costheta + (1 - costheta) * r.z * r.z) * p.z;

            return q;
        }

        public static List<xyz> RotatePoints(List<xyz> a, double radians, xyz p1, xyz u)
        {
            List<xyz> b = new List<xyz>();
            double costheta = Math.Cos(radians);
            double sintheta = Math.Sin(radians);

            a.ForEach(delegate (xyz p)
            {
                b.Add(ut.RotatePointAboutLine(p, costheta, sintheta, p1, u));
            }
            );

            return b;
        }

        public static xyz RotateVectorAboutLine(xyz v, xyz b1, double costheta, double sintheta, xyz p1, xyz u)
        {
            xyz b2 = b1 + v;

            xyz q1 = RotatePointAboutLine(b1, costheta, sintheta, p1, u);
            xyz q2 = RotatePointAboutLine(b2, costheta, sintheta, p1, u);

            return q2.subtract_in_place(q1);
        }

        // http://astronomy.swin.edu.au/~pbourke/geometry/rotate/
        public static xyz RotatePointAboutLine(xyz p, double costheta, double sintheta, xyz p1, xyz r)
        {
            xyz q = p1.copy();

            p = p - p1;

            q.x += (costheta + (1 - costheta) * r.x * r.x) * p.x;
            q.x += ((1 - costheta) * r.x * r.y - r.z * sintheta) * p.y;
            q.x += ((1 - costheta) * r.x * r.z + r.y * sintheta) * p.z;

            q.y += ((1 - costheta) * r.x * r.y + r.z * sintheta) * p.x;
            q.y += (costheta + (1 - costheta) * r.y * r.y) * p.y;
            q.y += ((1 - costheta) * r.y * r.z - r.x * sintheta) * p.z;

            q.z += ((1 - costheta) * r.x * r.z - r.y * sintheta) * p.x;
            q.z += ((1 - costheta) * r.y * r.z + r.x * sintheta) * p.y;
            q.z += (costheta + (1 - costheta) * r.z * r.z) * p.z;

            return q;
        }

        // http://astronomy.swin.edu.au/~pbourke/geometry/rotate/
        public static void RotatePointAboutLine_InPlace(xyz p, double costheta, double sintheta, xyz p1, xyz r)
        {
            double x = p.x - p1.x;
            double y = p.y - p1.y;
            double z = p.z - p1.z;

            p.setTo(p1);

            p.x += (costheta + (1 - costheta) * r.x * r.x) * x;
            p.x += ((1 - costheta) * r.x * r.y - r.z * sintheta) * y;
            p.x += ((1 - costheta) * r.x * r.z + r.y * sintheta) * z;

            p.y += ((1 - costheta) * r.x * r.y + r.z * sintheta) * x;
            p.y += (costheta + (1 - costheta) * r.y * r.y) * y;
            p.y += ((1 - costheta) * r.y * r.z - r.x * sintheta) * z;

            p.z += ((1 - costheta) * r.x * r.z - r.y * sintheta) * x;
            p.z += ((1 - costheta) * r.y * r.z + r.x * sintheta) * y;
            p.z += (costheta + (1 - costheta) * r.z * r.z) * z;
        }

        public static bool AllVerticesPlanar(List<xyz> a)
        {
            xyz n = ut.GetRawNormalFromPointList(a);
            if (fp.eq_area(0, n.magnitude_squared()))
            {
                // TODO note:  eq_area is used above because we use magnitude_squared.  it used to be eq_inches, but that caused failure on a 1/16 inch hole
                // TODO is this right?
                return false;
            }

            n.normalize_in_place();

            for (int i = 1; i < a.Count; i++)
            {
                xyz v2 = a[i] - a[0];
                if (!fp.eq_inches(0, v2.magnitude_squared()))
                {
                    //v2.normalize();
                    double d = xyz.dot(v2, n);
                    if (!fp.eq_dot_distancetoplane(d, 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool AllVerticesPlanar(Triangle3d t1, Triangle3d t2)
        {
            List<xyz> a = new List<xyz>();
            a.Add(t1.a);
            a.Add(t1.b);
            a.Add(t1.c);
            a.Add(t2.a);
            a.Add(t2.b);
            a.Add(t2.c);
            return ut.AllVerticesPlanar(a);
        }

        public static double SumTriangleAreas2d(List<Triangle2d> a)
        {
            double area = 0;
            foreach (Triangle2d t in a)
            {
                area += t.Area();
            }
            return area;
        }

        public static double HowManyDegreesToRotateN2ToEqualBaseVec(xyz basevec, xyz n2, xyz axis)
        {
            if (fp.eq_unitvec(basevec, n2))
            {
                return 0;
            }
            else if (fp.eq_unitvec(basevec, -n2))
            {
                return 180;
            }
            else
            {
                double dot = xyz.dot(basevec, n2);
                double radians = Math.Acos(dot);
                double degrees = ut.RadianToDegree(radians);
                xyz n2b = ut.RotateUnitVector(n2, degrees, axis);
                if (fp.eq_unitvec(basevec, n2b))
                {
                    return degrees;
                }
                else
                {
                    return -degrees;
                }
            }
        }

        public static double GetAngleBetweenTwoNormalizedVectorsInRadians(xyz n1, xyz n2)
        {
            if (fp.eq_unitvec(n1, n2))
            {
                return 0;
            }
            else if (fp.eq_unitvec(n1, -n2))
            {
                return Math.PI;
            }
            else
            {
                double dot = xyz.dot(n1, n2);
                double radians = Math.Acos(dot);
                return radians;
            }
        }

        public static double SumTriangleAreas3d(List<Triangle3d> a)
        {
            double area = 0;
            foreach (Triangle3d t in a)
            {
                area += t.Area();
            }
            return area;
        }

        public static xy ConvertPointTo2d(xyz p, xyz origin, xyz i, xyz j)
        {
            xy pt2d = new xy(xyz.dotsub(i, p, origin), xyz.dotsub(j, p, origin));
            pt2d.orig = p;
            return pt2d;
        }

        internal static xy ConvertPointTo2d(xyz v, xyz i, xyz j)
        {
            xy pt2d = new xy(xyz.dot(v, i), xyz.dot(v, j));
            pt2d.orig = v;
            return pt2d;
        }

        internal static bool PointsAreCollinear2d(xy a, xy b, xy c)
        {
            return 0 == ut.PointSideOfLine(a, b, c);
        }

        internal static bool PointOnSegment(xyz p, seg3d s)
        {
            return PointOnSegment(p, s.a, s.b);
        }

        internal static bool PointOnSegment(xyz p, xyz a, xyz b)
        {
            if (!PointsAreCollinear3d(a, b, p))
            {
                return false;
            }
            if (fp.lt_inches(p.x, Math.Min(a.x, b.x)))
            {
                return false;
            }
            else if (fp.gt_inches(p.x, Math.Max(a.x, b.x)))
            {
                return false;
            }
            else if (fp.lt_inches(p.y, Math.Min(a.y, b.y)))
            {
                return false;
            }
            else if (fp.gt_inches(p.y, Math.Max(a.y, b.y)))
            {
                return false;
            }
            else if (fp.lt_inches(p.z, Math.Min(a.z, b.z)))
            {
                return false;
            }
            else if (fp.gt_inches(p.z, Math.Max(a.z, b.z)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool PointOnSegment(xy p, xy a, xy b)
        {
            // this is basically PointSideOfLine
            double d = (p.x - a.x) * (b.y - a.y) - (p.y - a.y) * (b.x - a.x);
            if (!fp.eq_tol(d, 0, 0.0001))
            {
                return false;
            }

            double u;
            if (fp.eq_inches(b.x, a.x))
            {
                u = (p.y - a.y) / (b.y - a.y);
            }
            else
            {
                u = (p.x - a.x) / (b.x - a.x);
            }
            if (fp.lt_inches(u, 0))
            {
                return false;
            }
            if (fp.gt_inches(u, 1))
            {
                return false;
            }
            return true;
        }

        public static bool PointsAreCollinear3d(xyz a, xyz b, xyz c)
        {
            xyz d3 = xyz.cross_subs(b, a, c);
            return fp.eq_tol(d3.magnitude_squared(), 0, 0.00001 * 0.00001);
        }

        public static double PolygonArea2d(List<List<xy>> apts2d)
        {
            double a = 0;
            foreach (List<xy> pts2d in apts2d)
            {
                a += PolygonArea2d(pts2d);
            }
            return a;
        }

        public static double PolygonArea2d(List<xy> pts2d)
        {
            // http://geometryalgorithms.com/Archive/algorithm_0101/

            // http://astronomy.swin.edu.au/~pbourke/geometry/polyarea/

            double area = 0;

            for (int i = 1, j = 2, k = 0; i <= pts2d.Count; i++, j++, k++)
            {
                area += (pts2d[i % pts2d.Count]).x * ((pts2d[j % pts2d.Count]).y - (pts2d[k % pts2d.Count]).y);
            }
            return area / 2.0;
        }

        public static bool PointInsideTriangle(xy p, xy a, xy b, xy c)
        {
            return (
                (ut.PointSideOfLine(p, a, b) <= 0)
                && (ut.PointSideOfLine(p, b, c) <= 0)
                && (ut.PointSideOfLine(p, c, a) <= 0)
                );
        }

        public static int PointSideOfLine(xy p, xy v1, xy v2)
        {
            // return 0 if the point is on the line (not necessarily on the line segment)
            // return -1 if the point is left of the line
            // return 1 if the point is right of the line

#if oldcode
			xy diff = (v2 - v1).normalize_in_place();
			xy other = p - v1;
			double d = kross(other, diff);
#else
            // this saves a normalize and two subtractions vs. the oldcode above
            double d = (p.x - v1.x) * (v2.y - v1.y) - (p.y - v1.y) * (v2.x - v1.x);
#endif
            // Abs(d) is the distance from the point to the line
            if (fp.eq_tol(d, 0, 0.0001))
            {
                return 0;
            }
            else
            {
                if (d < 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        internal static SegmentsOverlap3d CalcSegmentsOverlap3d(seg3d p, seg3d q, List<seg3d> onlyp, List<seg3d> onlyq, List<seg3d> common)
        {
            xyz up = (p.b - p.a).normalize_in_place();
            xyz uq = (q.b - q.a).normalize_in_place();

            if (fp.eq_unitvec(up, -uq))
            {
                return SegmentsOverlap3d.OppositeDirection;
            }

            if (!fp.eq_unitvec(up, uq))
            {
                return SegmentsOverlap3d.None;
            }

            if (
                fp.eq_inches(p.a, q.a)
                && fp.eq_inches(p.b, q.b)
                )
            {
                return SegmentsOverlap3d.SameSegment;
            }

            bool pa_on_q = PointOnSegment(p.a, q);
            bool pb_on_q = PointOnSegment(p.b, q);

            if (pa_on_q && pb_on_q)
            {
                if ((onlyq != null) && !fp.eq_inches(q.a, p.a))
                {
                    onlyq.Add(new seg3d(q.a, p.a, q.origin));
                }
                if (common != null)
                {
                    common.Add(p);
                }
                if ((onlyq != null) && !fp.eq_inches(p.b, q.b))
                {
                    onlyq.Add(new seg3d(p.b, q.b, q.origin));
                }
                return SegmentsOverlap3d.P_On_Q;
            }

            bool qa_on_p = PointOnSegment(q.a, p);
            bool qb_on_p = PointOnSegment(q.b, p);

            if (qa_on_p && qb_on_p)
            {
                if ((onlyp != null) && !fp.eq_inches(p.a, q.a))
                {
                    onlyp.Add(new seg3d(p.a, q.a, p.origin));
                }
                if (common != null)
                {
                    common.Add(q);
                }
                if ((onlyp != null) && !fp.eq_inches(q.b, p.b))
                {
                    onlyp.Add(new seg3d(q.b, p.b, p.origin));
                }
                return SegmentsOverlap3d.Q_On_P;
            }

            if (
                (pa_on_q || pb_on_q)
                && (qa_on_p || qb_on_p)
                )
            {
                if (pa_on_q && qb_on_p)
                {
                    if ((onlyq != null) && !fp.eq_inches(q.a, p.a))
                    {
                        onlyq.Add(new seg3d(q.a, p.a, q.origin));
                    }
                    if ((common != null) && !fp.eq_inches(p.a, q.b))
                    {
                        common.Add(new seg3d(p.a, q.b, p.origin));
                    }
                    if ((onlyp != null) && !fp.eq_inches(q.b, p.b))
                    {
                        onlyp.Add(new seg3d(q.b, p.b, p.origin));
                    }
                    return SegmentsOverlap3d.Overlap;
                }
                else // if (pb_on_q && qa_on_p)
                {
                    Debug.Assert((pb_on_q && qa_on_p));
                    if ((onlyp != null) && !fp.eq_inches(p.a, q.a))
                    {
                        onlyp.Add(new seg3d(p.a, q.a, p.origin));
                    }
                    if ((common != null) && !fp.eq_inches(q.a, p.b))
                    {
                        common.Add(new seg3d(q.a, p.b, q.origin));
                    }
                    if ((onlyq != null) && !fp.eq_inches(p.b, q.b))
                    {
                        onlyq.Add(new seg3d(p.b, q.b, q.origin));
                    }
                    return SegmentsOverlap3d.Overlap;
                }
            }
            else
            {
                return SegmentsOverlap3d.None;
            }
        }

        internal static seg3d CalcUndirectedSegmentsOverlap3d(seg3d p, seg3d qorig)
        {
            seg3d q = qorig;

            xyz up = (p.b - p.a).normalize_in_place();
            xyz uq = (q.b - q.a).normalize_in_place();

            if (fp.eq_unitvec(up, -uq))
            {
                // TODO this is a poor way to do this I think?
                q = new seg3d(q.b, q.a, q.origin);
            }
            else if (!fp.eq_unitvec(up, uq))
            {
                return null;
            }

            if (
                fp.eq_inches(p.a, q.a)
                && fp.eq_inches(p.b, q.b)
                )
            {
                return p;
            }

            bool pa_on_q = PointOnSegment(p.a, q);
            bool pb_on_q = PointOnSegment(p.b, q);
            bool qa_on_p = PointOnSegment(q.a, p);
            bool qb_on_p = PointOnSegment(q.b, p);

            if (pa_on_q && pb_on_q)
            {
                return p;
            }
            else if (qa_on_p && qb_on_p)
            {
                return qorig;
            }

            if (
                (pa_on_q || pb_on_q)
                && (qa_on_p || qb_on_p)
                )
            {
                if (pa_on_q && qb_on_p)
                {
                    if (!fp.eq_inches(p.a, q.b))
                    {
                        return new seg3d(p.a, q.b, p.origin);
                    }
                }
                else // if (pb_on_q && qa_on_p)
                {
                    Debug.Assert((pb_on_q && qa_on_p));
                    if (!fp.eq_inches(q.a, p.b))
                    {
                        return new seg3d(q.a, p.b, q.origin);
                    }
                }
            }

            return null;
        }

        /// <summary>
		/// return true iff the segment p1,p2 is a part of the segment q1,q2
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="q1"></param>
		/// <param name="q2"></param>
		/// <returns></returns>
		public static bool IsSubsegment3d(xyz p1, xyz p2, xyz q1, xyz q2)
        {
            if (!PointOnSegment(p1, q1, q2))
            {
                return false;
            }
            if (!PointOnSegment(p2, q1, q2))
            {
                return false;
            }
            return true;
        }

        public static bool SegmentInCone(xy v0, xy v1, xy vm, xy vp)
        {
            xy diff = v1 - v0;
            xy edgeR = vm - v0;
            xy edgeL = vp - v0;
            if (fp.lt_unknowndata(xy.kross(edgeR, edgeL), 0))
            {
                return (fp.gt_unknowndata(xy.kross(diff, edgeR), 0)) && (fp.lt_unknowndata(xy.kross(diff, edgeL), 0));
            }
            else
            {
                double k1 = xy.kross(diff, edgeL);
                double k2 = xy.kross(diff, edgeR);
                return fp.lt_unknowndata(k1, 0) || fp.gt_unknowndata(k2, 0);
            }
        }

        public static SegIntersection GetSegIntersection(seg2d s1, seg2d s2, out xy intpt1, out xy intpt2)
        {
            return GetSegIntersection(s1.a, s1.b, s2.a, s2.b, out intpt1, out intpt2);
        }

        public static SegIntersection GetSegIntersection(xy p0, xy q0, xy p1, xy q1)
        {
            xy intpt1;
            xy intpt2;
            return GetSegIntersection(p0, q0, p1, q1, out intpt1, out intpt2);
        }

        // http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
        public static SegIntersection GetSegIntersection(xy p1, xy p2, xy p3, xy p4, out xy intpt1, out xy intpt2)
        {
            double denom = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
            double num_a = (p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x);
            double num_b = (p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x);

            if (fp.eq_inches(denom, 0))
            {
                if (fp.eq_inches(num_a, 0) && fp.eq_inches(num_b, 0))
                {
                    // coincident

                    // TODO the code below is copied from the previous implementation.  there is probably a faster way              
                    xy e = p3 - p1;
                    xy d0 = p2 - p1;
                    xy d1 = p4 - p3;
                    double sqrLen0 = d0.x * d0.x + d0.y * d0.y;
                    double sqrLen1 = d1.x * d1.x + d1.y * d1.y;

                    double s0 = xy.dot(d0, e) / sqrLen0;
                    double s1 = s0 + xy.dot(d0, d1) / sqrLen0;
                    double smin = Math.Min(s0, s1);
                    double smax = Math.Max(s0, s1);

                    double w0;
                    double w1;
                    SegIntersection si = FindIntersection(0.0, 1.0, smin, smax, out w0, out w1);
                    if (si == SegIntersection.None)
                    {
                        intpt1 = null;
                        intpt2 = null;
                    }
                    else if (si == SegIntersection.Point)
                    {
                        intpt1 = p1 + w0 * d0;
                        intpt2 = null;
                    }
                    else
                    {
                        intpt1 = p1 + w0 * d0;
                        intpt2 = p1 + w1 * d0;
                    }
                    return si;
                }
                else
                {
                    // parallel
                    intpt1 = null;
                    intpt2 = null;
                    return SegIntersection.None;
                }
            }
            else
            {
                double ua = num_a / denom;
                double ub = num_b / denom;

                if (
                    fp.gte_inches(ua, 0)
                    && fp.lte_inches(ua, 1)
                    && fp.gte_inches(ub, 0)
                    && fp.lte_inches(ub, 1)
                    )
                {
                    // intersecting
                    intpt1 = new xy(p1.x + ua * (p2.x - p1.x), p1.y + ua * (p2.y - p1.y));
                    intpt2 = null;
                    return SegIntersection.Point;
                }
                else
                {
                    // not intersecting
                    intpt1 = null;
                    intpt2 = null;
                    return SegIntersection.None;
                }
            }
        }

        public static SegIntersection FindIntersection(double u0, double u1, double v0, double v1, out double w0, out double w1)
        {
            if (
                (fp.lt_unknowndata(u1, v0))
                || (fp.gt_unknowndata(u0, v1))
                )
            {
                w0 = 0;
                w1 = 0;
                return SegIntersection.None;
            }

            if (fp.gt_unknowndata(u1, v0))
            {
                if (fp.lt_unknowndata(u0, v1))
                {
                    if (u0 < v0)
                    {
                        w0 = v0;
                    }
                    else
                    {
                        w0 = u0;
                    }
                    if (u1 > v1)
                    {
                        w1 = v1;
                    }
                    else
                    {
                        w1 = u1;
                    }
                    return SegIntersection.Overlap;
                }
                else
                {
                    w0 = u0;
                    w1 = 0;
                    return SegIntersection.Point;
                }
            }
            else
            {
                w0 = u1;
                w1 = 0;
                return SegIntersection.Point;
            }
        }

        public static bool IsDiagonal(List<xy> pts, int i0, int i1)
        {
            int n = pts.Count;
            int iM = i0 - 1;
            if (iM < 0)
            {
                iM = n - 1;
            }
            int iP = (i0 + 1) % n;

            if (!SegmentInCone(pts[i0], pts[i1], pts[iM], pts[iP]))
            {
                return false;
            }
            for (int j0 = 0, j1 = n - 1; j0 < n; j1 = j0, j0++)
            {
                if (
                    (j0 != i0)
                    && (j0 != i1)
                    && (j1 != i0)
                    && (j1 != i1)
                    )
                {
                    if (SegIntersection.None != ut.GetSegIntersection(pts[i0], pts[i1], pts[j0], pts[j1]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsCutSegment(List<xy> main, List<List<xy>> holes, int mainpt, int hole, int holept)
        {
            xy a = main[mainpt];
            List<xy> hol = holes[hole];
            xy b = hol[holept];

            /*
				this segment (a---b) is safe iff it does not
				intersect with any segment of the main polygon
				or with any segment of any hole.
			*/

            for (int i = 0; i < main.Count; i++)
            {
                int j = (i + 1) % main.Count;
                xy c = main[i];
                xy d = main[j];
                if (
                    (i != mainpt)
                    && (j != mainpt)
                    )
                {
                    if (SegIntersection.None != ut.GetSegIntersection(a, b, c, d))
                    {
                        return false;
                    }
                }
                else
                {
                    if (SegIntersection.Overlap == ut.GetSegIntersection(a, b, c, d))
                    {
                        return false;
                    }
                }
            }

            for (int iCurHole = 0; iCurHole < holes.Count; iCurHole++)
            {
                List<xy> curhole = holes[iCurHole];

                if (iCurHole == hole)
                {
                    for (int i = 0; i < curhole.Count; i++)
                    {
                        int j = (i + 1) % curhole.Count;
                        if (
                            (i != holept)
                            && (j != holept)
                            )
                        {
                            xy c = curhole[i];
                            xy d = curhole[j];
                            if (SegIntersection.None != ut.GetSegIntersection(a, b, c, d))
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < curhole.Count; i++)
                    {
                        int j = (i + 1) % curhole.Count;

                        xy c = curhole[i];
                        xy d = curhole[j];
                        if (SegIntersection.None != ut.GetSegIntersection(a, b, c, d))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool PolygonIsSimple3d(List<xyz> pts)
        {
            List<xy> pts2d = ut.Convert3dPointsTo2d(pts);
            return PolygonIsSimple2d(pts2d);
        }

        public static bool PolygonIsSimple2d(List<xy> pts)
        {
            if (pts.Count < 3)
            {
                return false;
            }

            if (
                (pts.Count == 3)
                && ut.PointsAreCollinear2d(pts[0], pts[1], pts[2])
                )
            {
                return false;
            }

            for (int i = 0; i < pts.Count; i++)
            {
                int j = (i + 1) % pts.Count;

                xy a = pts[i];
                xy b = pts[j];

                for (int q = 0; q < pts.Count; q++)
                {
                    int r = (q + 1) % pts.Count;

                    xy c = pts[q];
                    xy d = pts[r];

                    if (
                        (q != i)
                        && (q != j)
                        && (r != i)
                        && (r != j)
                        )
                    {
                        // this pp_segment is somewhere else
                        if (SegIntersection.None != ut.GetSegIntersection(a, b, c, d))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool PointOnAnySegment(xy p, List<xy> a)
        {
            for (int i = 0; i < a.Count; i++)
            {
                int j = (i + 1) % a.Count;
                xy a1 = a[i];
                xy a2 = a[j];
                if (PointOnSegment(p, a1, a2))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PointOnAnySegment(xyz p, List<xyz> a)
        {
            for (int i = 0; i < a.Count; i++)
            {
                int j = (i + 1) % a.Count;
                xyz a1 = a[i];
                xyz a2 = a[j];
                if (PointOnSegment(p, a1, a2))
                {
                    return true;
                }
            }
            return false;
        }

        // see this: http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
        // see this: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        public static bool PointInsidePoly_NoEdgeCheck(List<xy> poly, xy pt)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((((poly[i].y <= pt.y) && (pt.y < poly[j].y)) ||
                     ((poly[j].y <= pt.y) && (pt.y < poly[i].y))) &&
                    (pt.x < (poly[j].x - poly[i].x) * (pt.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))

                    c = !c;
            }
            return c;
        }

        // see this: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        public static bool PointInsidePoly(List<xy> poly, xy pt)
        {
            BoundingBox2d bb = BoundingBox2d.FromArrayOfPoints(poly);
            if (bb.PointOutside(pt))
            {
                return false;
            }

            if (ut.PointOnAnySegment(pt, poly))
            {
                return true;
            }

            return PointInsidePoly_NoEdgeCheck(poly, pt);
        }

        public static int FindIndexOfClosestPoint(List<xy> a, xy p)
        {
            double result_dist = double.MaxValue;
            int result_ndx = -1;
            for (int i = 0; i < a.Count; i++)
            {
                xy q = a[i];
                double dist = (q - p).magnitude_squared();
                if (dist < result_dist)
                {
                    result_dist = dist;
                    result_ndx = i;
                }
            }
            return result_ndx;
        }

        public static bool FindRayIntersectionWithPlane(List<xyz> poly, xyz d, xyz v, out double result)
        {
            xyz n = ut.GetUnitNormalFromPointList(poly);
            xyz a = poly[0];

            double dist_d = xyz.dotsub(n, d, a);
            if (fp.eq_dot_distancetoplane(dist_d, 0))
            {
                result = 0;
                return true;
            }

            double dist_e = xyz.dot(n, (d + v).subtract_in_place(a));
            if (fp.eq_dot_distancetoplane(dist_d, dist_e))
            {
                // parallel to the plane
                result = 0;
                return false;
            }

            // find the point where the segment intersects the plane
            // http://astronomy.swin.edu.au/~pbourke/geometry/planeline/
            result = xyz.dotsub(n, a, d) / xyz.dot(n, v);

            return true;
        }

        public static int FindIndexOfFarthestPoint(List<xy> a, xy p)
        {
            double result_dist = double.MinValue;
            int result_ndx = -1;
            for (int i = 0; i < a.Count; i++)
            {
                xy q = a[i];
                double dist = (q - p).magnitude_squared();
                if (dist > result_dist)
                {
                    result_dist = dist;
                    result_ndx = i;
                }
            }
            return result_ndx;
        }

        public static void RemoveHoles(List<List<xy>> newloops, List<xy> main, List<List<xy>> holes)
        {
            if (
                (holes == null)
                || (holes.Count == 0)
                )
            {
                newloops.Add(main);
                return;
            }

            /*
				We need to carve this poly up into pieces.
				The goal is to eliminate the first hole in the list.
					1.  Find a cut segment
					2.  Find a diagonal from that point across the hole
					3.  Find a cut segment involving the other point
					4.  Split the poly, keeping any other holes in the correct sub-poly
					5.  Recurse on each subpoly
			*/

            List<xy> curhole = holes[0];

            bool bFoundCut1 = false;
            int cut1_main = -1;
            int cut1_hole = -1;

            // we start at point of the main loop
            for (int i = 0; i < main.Count; i++)
            {
                int curj = ut.FindIndexOfClosestPoint(curhole, main[i]);
                for (int j = 0; j < curhole.Count; j++)
                {
                    if (ut.IsCutSegment(main, holes, i, 0, curj))
                    {
                        cut1_main = i;
                        cut1_hole = curj;
                        bFoundCut1 = true;
                        break;
                    }
                    curj = (curj + 1) % curhole.Count;
                }
                if (bFoundCut1)
                {
                    break;
                }
            }

            bool bFoundCut2 = false;
            int cut2_main = -1;
            int cut2_hole = -1;

            int curi = ut.FindIndexOfFarthestPoint(main, main[0]);
            for (int i = 0; i < main.Count; i++)
            {
                if (curi != cut1_main)
                {
                    int curj = ut.FindIndexOfClosestPoint(curhole, main[curi]);
                    for (int j = 0; j < curhole.Count; j++)
                    {
                        if (curj != cut1_hole)
                        {
                            if (ut.IsCutSegment(main, holes, curi, 0, curj))
                            {
                                // the cut lines cannot intersect
                                if (SegIntersection.None == ut.GetSegIntersection(main[cut1_main], curhole[cut1_hole], main[curi], curhole[curj]))
                                {
                                    cut2_main = curi;
                                    cut2_hole = curj;
                                    bFoundCut2 = true;
                                    break;
                                }
                            }
                        }
                        curj = (curj + 1) % curhole.Count;
                    }
                }
                if (bFoundCut2)
                {
                    break;
                }
                curi = (curi + 1) % main.Count;
            }

            // assert (cut2_main <= cut1_main)

            // found two cut points.  now break it up.
            List<xy> newmain1 = new List<xy>();

            newmain1.AddRange(main.GetRange(0, cut1_main + 1));
            int q = cut1_hole;
            while (true)
            {
                newmain1.Add(curhole[q]);
                if (q == cut2_hole)
                {
                    break;
                }
                q = (q + 1) % curhole.Count;
            }
            newmain1.AddRange(main.GetRange(cut2_main, main.Count - cut2_main));

            List<xy> newmain2 = new List<xy>();
            newmain2.AddRange(main.GetRange(cut1_main, cut2_main - cut1_main + 1));
            int r = cut2_hole;
            while (true)
            {
                newmain2.Add(curhole[r]);
                if (r == cut1_hole)
                {
                    break;
                }
                r = (r + 1) % curhole.Count;
            }

            // ok, we have two polys.  but where do the other holes go?

            List<List<xy>> holes1 = new List<List<xy>>();
            List<List<xy>> holes2 = new List<List<xy>>();

            if (holes.Count > 1)
            {
                for (int b = 1; b < holes.Count; b++)
                {
                    List<xy> h = holes[b];
                    if (PointInsidePoly(newmain1, h[0]))
                    {
                        holes1.Add(h);
                    }
                    else
                    {
                        holes2.Add(h);
                    }
                }
            }

            RemoveHoles(newloops, newmain1, holes1);
            RemoveHoles(newloops, newmain2, holes2);
        }

        public static List<List<xy>> Convert3dLoopsTo2d(List<List<xyz>> loops3d)
        {
            xyz pt1;
            xyz iv;
            xyz jv;
            return Convert3dLoopsTo2d(loops3d, out pt1, out iv, out jv);
        }

        public static List<List<xy>> Convert3dLoopsTo2d(List<List<xyz>> loops3d, xyz pt1, xyz iv, xyz jv)
        {
            List<List<xy>> loops2d = new List<List<xy>>();

            for (int i = 0; i < loops3d.Count; i++)
            {
                List<xyz> loop3d = loops3d[i];
                loops2d.Add(ut.Convert3dPointsTo2d(loop3d, pt1, iv, jv));
            }

            return loops2d;
        }

        public static List<List<xy>> Convert3dLoopsTo2d(List<List<xyz>> loops3d, out xyz pt1, out xyz iv, out xyz jv)
        {
            List<List<xy>> loops2d = new List<List<xy>>();

            List<xyz> first_loop = loops3d[0];

            loops2d.Add(ut.Convert3dPointsTo2d(first_loop, out pt1, out iv, out jv));

            for (int i = 1; i < loops3d.Count; i++)
            {
                List<xyz> loop3d = loops3d[i];
                loops2d.Add(ut.Convert3dPointsTo2d(loop3d, pt1, iv, jv));
            }

            return loops2d;
        }

        public static List<xy> Convert3dPointsTo2d(List<xyz> pts3d, out xyz pt1, out xyz i, out xyz j)
        {
            xyz n;
            ut.GetPlaneFromArrayOfCoplanarPoints(pts3d, out pt1, out n, out i, out j);

            List<xy> pts2d = new List<xy>();

            foreach (xyz pt3d in pts3d)
            {
                xy pt2d = ut.ConvertPointTo2d(pt3d, pt1, i, j);
                pts2d.Add(pt2d);
            }

            return pts2d;
        }

        public static List<xy> Convert3dPointsTo2d(List<xyz> pts3d, xyz pt1, xyz i, xyz j)
        {
            List<xy> pts2d = new List<xy>();

            foreach (xyz pt3d in pts3d)
            {
                xy pt2d = ut.ConvertPointTo2d(pt3d, pt1, i, j);
                pts2d.Add(pt2d);
            }

            return pts2d;
        }

        public static List<xy> Convert3dPointsTo2d(List<xyz> pts3d)
        {
            xyz pt1;
            xyz i;
            xyz j;

            return Convert3dPointsTo2d(pts3d, out pt1, out i, out j);
        }

        public static xyz GetRawNormalFromPointList(List<xyz> pts)
        {
            /*
				This is called the Newell method of getting the
				normal.
				
				N = (0, 0, 0)
				for each polygon edge Vi Vj
					( don t forget to close the loop with V(n-1) V(0) )
				{
					N[0] += (Vi[1] - Vj[1]) * (Vi[2] + Vi[2]);
					N[1] += (Vi[2] - Vj[2]) * (Vi[0] + Vi[0]);
					N[2] += (Vi[0] - Vj[0]) * (Vi[1] + Vi[1]);
				}

				Normalize N 
			*/

            xyz n = new xyz(0, 0, 0);
            for (int i = 0; i < pts.Count; i++)
            {
                int j = (i + 1) % pts.Count;

                xyz Vi = pts[i];
                xyz Vj = pts[j];

                double x = (Vi.y - Vj.y) * (Vi.z + Vj.z);
                double y = (Vi.z - Vj.z) * (Vi.x + Vj.x);
                double z = (Vi.x - Vj.x) * (Vi.y + Vj.y);

                n.x += x;
                n.y += y;
                n.z += z;
            }

            return n;
        }

        public static xyz GetUnitNormalFromPointList(List<xyz> pts)
        {
            xyz n = GetRawNormalFromPointList(pts).normalize_in_place();

            return n;
        }

        public static void GetPlaneFromArrayOfCoplanarPoints(List<xyz> pts, out xyz origin, out xyz nv, out xyz iv, out xyz jv)
        {
            xyz n = GetUnitNormalFromPointList(pts);

            xyz i = ((pts[1]) - (pts[0])).normalize_in_place();
            xyz j = xyz.cross(n, i).normalize_in_place();

            origin = (pts[0]);
            nv = n;
            iv = i;
            jv = j;
        }

        public static xyz Convert2dPointTo3d(xy pt2d, xyz origin, xyz iv, xyz jv)
        {
            xyz pt3d = (iv * pt2d.x).add_in_place(origin).add_in_place(jv * pt2d.y);
            return pt3d;
        }

        public static List<xyz> Convert2dPointsTo3d(List<xy> pts2d, xyz origin, xyz iv, xyz jv)
        {
            List<xyz> pts3d = new List<xyz>();
            foreach (xy pt2d in pts2d)
            {
                xyz pt3d = (iv * pt2d.x).add_in_place(origin).add_in_place(jv * pt2d.y);
                pts3d.Add(pt3d);
            }
            return pts3d;
        }

        public static void Convert3dPointsTo2d(List<xyz> main, List<List<xyz>> holes, List<xy> main2d, List<List<xy>> holes2d)
        {
            xyz origin;
            xyz n;
            xyz i;
            xyz j;
            ut.GetPlaneFromArrayOfCoplanarPoints(main, out origin, out n, out i, out j);

            foreach (xyz pt3d in main)
            {
                xy pt2d = ut.ConvertPointTo2d(pt3d, origin, i, j);
                main2d.Add(pt2d);
            }

            foreach (List<xyz> h in holes)
            {
                List<xy> h2d = new List<xy>();
                foreach (xyz pt3d in h)
                {
                    xy pt2d = ut.ConvertPointTo2d(pt3d, origin, i, j);
                    h2d.Add(pt2d);
                }
                holes2d.Add(h2d);
            }
        }

        internal static bool HasNonNegativeDotProductWithAll(xyz v, List<xyz> vectors)
        {
            foreach (xyz v2 in vectors)
            {
                double d = xyz.dot(v, v2);
                if (fp.lt_unknowndata(d, 0))
                {
                    return false;
                }
            }
            return true;
        }

        internal static double SumDotProductWithAll(xyz v, List<xyz> vectors)
        {
            double sum = 0;
            foreach (xyz v2 in vectors)
            {
                double d = xyz.dot(v, v2);
                sum += d;
            }
            return sum;
        }

        internal static xyz SelectExplodeVector(List<xyz> choices, List<xyz> vectors)
        {
            List<xyz> possibles = new List<xyz>();
            foreach (xyz v1 in choices)
            {
                if (ut.HasNonNegativeDotProductWithAll(v1, vectors))
                {
                    possibles.Add(v1);
                }
            }
            if (possibles.Count == 0)
            {
                return null;
            }
            if (possibles.Count == 1)
            {
                return possibles[0];
            }
            double max = 0;
            xyz vec = null;
            foreach (xyz v1 in possibles)
            {
                double d = ut.SumDotProductWithAll(v1, vectors);
                if (
                    (vec == null)
                    || (d > max)
                    )
                {
                    vec = v1;
                    max = d;
                }
            }
            return vec;
        }

#if true
        internal static xyz CalculateExplodeVector(List<xyz> vectors)
        {
            // TODO find a vector which has a >=0 dot product with each of the vectors in the list

            xyz v = new xyz(0, 0, 0);
            foreach (xyz v2 in vectors)
            {
                v += v2;
            }
            return v;
        }
#endif

        internal static List<xyz> Reverse(List<xyz> p1)
        {
            List<xyz> p2 = new List<xyz>();
            for (int i = p1.Count - 1; i >= 0; i--)
            {
                p2.Add(p1[i]);
            }
            return p2;
        }

        internal static double SignedDistance(xyz origin, xyz direction, xyz pt)
        {
            xyz v = pt - origin;
            double dist = v.magnitude();
            if (fp.eq_tol(dist, 0, 0.000001))
            {
                return 0;
            }
            v.x /= dist;
            v.y /= dist;
            v.z /= dist;
            if (fp.eq_unitvec(-v, direction))
            {
                dist = -dist;
            }
            return dist;
        }

        private class compare_points : IComparer<xyz>
        {
            private xyz origin;
            private xyz direction;

            public compare_points(xyz o, xyz d)
            {
                origin = o;
                direction = d;
            }

            public int Compare(xyz p, xyz q)
            {
                double dp = SignedDistance(origin, direction, p);
                double dq = SignedDistance(origin, direction, q);

                if (dp < dq)
                {
                    return -1;
                }
                else if (dp > dq)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal static void SortPointList(List<xyz> pts, xyz origin, xyz dir)
        {
            pts.Sort(new compare_points(origin, dir));
        }

        internal static void SortPointList(List<xyz> pts)
        {
            Debug.Assert(pts.Count >= 2);
            xyz p1 = null;
            for (int i = 1; i < pts.Count; i++)
            {
                if (!fp.eq_inches(pts[i], pts[0]))
                {
                    p1 = pts[i];
                    break;
                }
            }
            Debug.Assert(p1 != null);
            pts.Sort(new compare_points(pts[0], (p1 - pts[0]).normalize_in_place()));
        }

        internal static void RemoveDuplicates(List<xyz> pts)
        {
            List<int> rm = new List<int>();

            for (int i = pts.Count - 1; i >= 1; i--)
            {
                if (fp.eq_inches(pts[i], pts[i - 1]))
                {
                    rm.Add(i);
                }
            }

            foreach (int q in rm)
            {
                pts.RemoveAt(q);
            }
        }

        internal static bool ParseBool(string p, bool bDefault)
        {
            p = p.Trim().ToLower();
            switch (p)
            {
                case "true":
                case "yes":
                case "on":
                    return true;
            }
            return false;
        }
    }
}

