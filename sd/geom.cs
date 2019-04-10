
using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    public class Inches
    {
        public const int PRECISION = 64;

        private int units;

        public static implicit operator Inches(double d)
        {
            return new Inches(d);
        }

        public static implicit operator double(Inches i)
        {
            return i.units / ((double)PRECISION);
        }

        public Inches(double d)
        {
            if (d >= 0)
            {
                double d2 = d * PRECISION + 0.5;
                units = (int)d2;
            }
            else
            {
                double d2 = d * PRECISION - 0.5;
                units = (int)d2;
            }
        }

        public Inches(int whole, int numerator, int denominator)
        {
            units = whole * PRECISION;
            Debug.Assert(
                (denominator == 2)
                || (denominator == 4)
                || (denominator == 8)
                || (denominator == 16)
                || (denominator == 32)
                || (denominator == 64)
                || (denominator == 128)
                );
            units += (numerator * PRECISION / denominator);
        }

        public void ToFraction(out int whole, out int numerator, out int denominator)
        {
            whole = units / PRECISION;
            denominator = PRECISION;
            numerator = units % PRECISION;
            while (
                ((numerator % 2) == 0)
                && (denominator > 1)
                )
            {
                numerator /= 2;
                denominator /= 2;
            }
        }

        public string GetStringWithoutUnits()
        {
            int whole;
            int numerator;
            int denominator;

            ToFraction(out whole, out numerator, out denominator);
            if (numerator > 0)
            {
                if (whole != 0)
                {
                    return string.Format("{0} {1}/{2}", whole, numerator, denominator);
                }
                else
                {
                    return string.Format("{0}/{1}", numerator, denominator);
                }
            }
            else
            {
                return whole.ToString();
            }
        }

        public string GetProse()
        {
            string s = GetStringWithoutUnits();
            if (
                (units > 0)
                && (units <= PRECISION)
                )
            {
                s += " inch";
            }
            else
            {
                s += " inches";
            }
            return s;
        }

        public override string ToString()
        {
            return GetStringWithoutUnits() + " in";
        }
    }

    internal class BoundingBox2d
    {
        public double xmin = double.MaxValue;
        public double ymin = double.MaxValue;
        public double xmax = double.MinValue;
        public double ymax = double.MinValue;

        public double Area()
        {
            return xsize * ysize;
        }

        public bool PointOutside(xy p)
        {
            if (fp.lt_inches(p.x, xmin))
            {
                return true;
            }
            if (fp.gt_inches(p.x, xmax))
            {
                return true;
            }
            if (fp.lt_inches(p.y, ymin))
            {
                return true;
            }
            if (fp.gt_inches(p.y, ymax))
            {
                return true;
            }
            return false;
        }

        public static bool intersect(BoundingBox2d bb1, BoundingBox2d bb2)
        {
            if (fp.lt_inches(bb1.xmax, bb2.xmin))
            {
                return false;
            }
            if (fp.lt_inches(bb2.xmax, bb1.xmin))
            {
                return false;
            }
            if (fp.lt_inches(bb1.ymax, bb2.ymin))
            {
                return false;
            }
            if (fp.lt_inches(bb2.ymax, bb1.ymin))
            {
                return false;
            }
            return true;
        }

        public double xsize
        {
            get
            {
                return xmax - xmin;
            }
        }

        public double ysize
        {
            get
            {
                return ymax - ymin;
            }
        }

        public void IncludePoint(double x, double y)
        {
            xmin = Math.Min(xmin, x);
            ymin = Math.Min(ymin, y);
            xmax = Math.Max(xmax, x);
            ymax = Math.Max(ymax, y);
        }

        public void IncludePoint(xyz v)
        {
            xmin = Math.Min(xmin, v.x);
            ymin = Math.Min(ymin, v.y);
            xmax = Math.Max(xmax, v.x);
            ymax = Math.Max(ymax, v.y);
        }

        public static BoundingBox2d FromArrayOfPoints(List<xy> a)
        {
            BoundingBox2d bb = new BoundingBox2d();
            foreach (xy v in a)
            {
                bb.xmin = Math.Min(bb.xmin, v.x);
                bb.ymin = Math.Min(bb.ymin, v.y);
                bb.xmax = Math.Max(bb.xmax, v.x);
                bb.ymax = Math.Max(bb.ymax, v.y);
            }
            return bb;
        }
    }

    public class BoundingBox3d
    {
        public double xmin = double.MaxValue;
        public double ymin = double.MaxValue;
        public double zmin = double.MaxValue;
        public double xmax = double.MinValue;
        public double ymax = double.MinValue;
        public double zmax = double.MinValue;

        public List<xyz> Corners
        {
            get
            {
                List<xyz> result = new List<xyz>();

                result.Add(new xyz(xmin, ymin, zmin));
                result.Add(new xyz(xmax, ymin, zmin));
                result.Add(new xyz(xmax, ymax, zmin));
                result.Add(new xyz(xmin, ymax, zmin));

                result.Add(new xyz(xmin, ymin, zmax));
                result.Add(new xyz(xmax, ymin, zmax));
                result.Add(new xyz(xmax, ymax, zmax));
                result.Add(new xyz(xmin, ymax, zmax));

                return result;
            }
        }

        public List<List<xyz>> Faces
        {
            get
            {
                List<List<xyz>> result = new List<List<xyz>>();

                result.Add(ut.MakePoly(new xyz(xmin, ymin, zmin), new xyz(xmax, ymin, zmin), new xyz(xmax, ymax, zmin), new xyz(xmin, ymax, zmin))); // front
                result.Add(ut.MakePoly(new xyz(xmax, ymin, zmin), new xyz(xmax, ymin, zmax), new xyz(xmax, ymax, zmax), new xyz(xmax, ymax, zmin))); // right
                result.Add(ut.MakePoly(new xyz(xmin, ymax, zmin), new xyz(xmax, ymax, zmin), new xyz(xmax, ymax, zmax), new xyz(xmin, ymax, zmax))); // top
                result.Add(ut.MakePoly(new xyz(xmin, ymin, zmax), new xyz(xmin, ymin, zmin), new xyz(xmin, ymax, zmin), new xyz(xmin, ymax, zmax))); // left
                result.Add(ut.MakePoly(new xyz(xmin, ymin, zmin), new xyz(xmin, ymin, zmax), new xyz(xmax, ymin, zmax), new xyz(xmax, ymin, zmin))); // bottom
                result.Add(ut.MakePoly(new xyz(xmin, ymin, zmax), new xyz(xmin, ymax, zmax), new xyz(xmax, ymax, zmax), new xyz(xmax, ymin, zmax))); // back

                return result;
            }
        }

        public void Add(BoundingBox3d b)
        {
            xmin = Math.Min(xmin, b.xmin);
            ymin = Math.Min(ymin, b.ymin);
            zmin = Math.Min(zmin, b.zmin);
            xmax = Math.Max(xmax, b.xmax);
            ymax = Math.Max(ymax, b.ymax);
            zmax = Math.Max(zmax, b.zmax);
        }

        public static BoundingBox3d operator +(BoundingBox3d a, BoundingBox3d b)
        {
            BoundingBox3d r = new BoundingBox3d();

            r.xmin = Math.Min(a.xmin, b.xmin);
            r.ymin = Math.Min(a.ymin, b.ymin);
            r.zmin = Math.Min(a.zmin, b.zmin);
            r.xmax = Math.Max(a.xmax, b.xmax);
            r.ymax = Math.Max(a.ymax, b.ymax);
            r.zmax = Math.Max(a.zmax, b.zmax);

            return r;
        }

        public xyz center
        {
            get
            {
                return new xyz((xmin + xmax) / 2, (ymin + ymax) / 2, (zmin + zmax) / 2);
            }
        }

        public xyz Diagonal()
        {
            xyz pmin = new xyz(xmin, ymin, zmin);
            xyz pmax = new xyz(xmax, ymax, zmax);
            return pmax.subtract_in_place(pmin);
        }

        public bool PointInside(xyz p)
        {
            // TODO should probably unit test this
            if (fp.lt_inches(p.x, xmin))
            {
                return false;
            }
            if (fp.lt_inches(p.y, ymin))
            {
                return false;
            }
            if (fp.lt_inches(p.z, zmin))
            {
                return false;
            }
            if (fp.gt_inches(p.x, xmax))
            {
                return false;
            }
            if (fp.gt_inches(p.y, ymax))
            {
                return false;
            }
            if (fp.gt_inches(p.z, zmax))
            {
                return false;
            }
            return true;
        }

        public static BoundingBox3d CalcIntersection(BoundingBox3d a, BoundingBox3d b)
        {
            BoundingBox3d r = new BoundingBox3d();

            r.xmin = Math.Max(a.xmin, b.xmin);
            r.ymin = Math.Max(a.ymin, b.ymin);
            r.zmin = Math.Max(a.zmin, b.zmin);
            r.xmax = Math.Min(a.xmax, b.xmax);
            r.ymax = Math.Min(a.ymax, b.ymax);
            r.zmax = Math.Min(a.zmax, b.zmax);

            return r;
        }

        public static bool intersect(BoundingBox3d bb1, BoundingBox3d bb2)
        {
            if (fp.lt_inches(bb1.xmax, bb2.xmin))
            {
                return false;
            }
            if (fp.lt_inches(bb2.xmax, bb1.xmin))
            {
                return false;
            }
            if (fp.lt_inches(bb1.ymax, bb2.ymin))
            {
                return false;
            }
            if (fp.lt_inches(bb2.ymax, bb1.ymin))
            {
                return false;
            }
            if (fp.lt_inches(bb1.zmax, bb2.zmin))
            {
                return false;
            }
            if (fp.lt_inches(bb2.zmax, bb1.zmin))
            {
                return false;
            }
            return true;
        }

        public double xsize
        {
            get
            {
                return xmax - xmin;
            }
        }

        public double ysize
        {
            get
            {
                return ymax - ymin;
            }
        }

        public double zsize
        {
            get
            {
                return zmax - zmin;
            }
        }

        public double volume
        {
            get
            {
                return xsize * ysize * zsize;
            }
        }

        // TODO this is only used in the unit tests
        internal static BoundingBox3d FromPoints(params xyz[] a)
        {
            BoundingBox3d bb = new BoundingBox3d();
            foreach (xyz v in a)
            {
                bb.xmin = Math.Min(bb.xmin, v.x);
                bb.ymin = Math.Min(bb.ymin, v.y);
                bb.zmin = Math.Min(bb.zmin, v.z);
                bb.xmax = Math.Max(bb.xmax, v.x);
                bb.ymax = Math.Max(bb.ymax, v.y);
                bb.zmax = Math.Max(bb.zmax, v.z);
            }
            return bb;
        }

        internal static BoundingBox3d FromArrayOfPoints(List<xyz> a)
        {
            BoundingBox3d bb = new BoundingBox3d();
            a.ForEach(delegate (xyz v)
            {
                bb.xmin = Math.Min(bb.xmin, v.x);
                bb.ymin = Math.Min(bb.ymin, v.y);
                bb.zmin = Math.Min(bb.zmin, v.z);
                bb.xmax = Math.Max(bb.xmax, v.x);
                bb.ymax = Math.Max(bb.ymax, v.y);
                bb.zmax = Math.Max(bb.zmax, v.z);
            }
            );
            return bb;
        }

        internal double IntersectRay_Planes_Max(xyz origin, xyz v)
        {
            bool bhit = false;
            double max = 0;

            foreach (List<xyz> face in this.Faces)
            {
                double d;
                if (ut.FindRayIntersectionWithPlane(face, origin, v, out d))
                {
                    if (bhit)
                    {
                        if (d > max)
                        {
                            max = d;
                        }
                    }
                    else
                    {
                        max = d;
                    }
                    bhit = true;
                }
            }

            Debug.Assert(bhit);

            return max;
        }

        internal bool SegmentCannotIntersect(xyz a, xyz b)
        {
            if (fp.lt_inches(a.x, xmin) && fp.lt_inches(b.x, xmin))
            {
                return true;
            }
            if (fp.lt_inches(a.y, ymin) && fp.lt_inches(b.y, ymin))
            {
                return true;
            }
            if (fp.lt_inches(a.z, zmin) && fp.lt_inches(b.z, zmin))
            {
                return true;
            }
            if (fp.gt_inches(a.x, xmax) && fp.gt_inches(b.x, xmax))
            {
                return true;
            }
            if (fp.gt_inches(a.y, ymax) && fp.gt_inches(b.y, ymax))
            {
                return true;
            }
            if (fp.gt_inches(a.z, zmax) && fp.gt_inches(b.z, zmax))
            {
                return true;
            }
            return false;
        }
    }

    public class Line3d
    {
        public xyz p1;
        public xyz p2;

        public Line3d(xyz a1, xyz a2)
        {
            p1 = a1.copy();
            p2 = a2.copy();
        }
    }

    public class xyz
    {
        public double x;
        public double y;
        public double z;

        internal xyz(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        internal xyz copy()
        {
            return new xyz(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        internal void setTo(xyz q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
        }

        internal void Translate(double _x, double _y, double _z)
        {
            x += _x;
            y += _y;
            z += _z;
        }

        internal double magnitude_squared()
        {
            return (x * x + y * y + z * z);
        }

        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        internal xyz subtract_in_place(xyz p)
        {
            x -= p.x;
            y -= p.y;
            z -= p.z;
            return this;
        }

        internal xyz multiply_in_place(double d)
        {
            x *= d;
            y *= d;
            z *= d;
            return this;
        }

        internal xyz divide_in_place(double d)
        {
            x /= d;
            y /= d;
            z /= d;
            return this;
        }

        internal xyz add_in_place(xyz p)
        {
            x += p.x;
            y += p.y;
            z += p.z;
            return this;
        }

        internal xyz normalize_in_place()
        {
            double mag = magnitude();
            Debug.Assert(!fp.eq_tol(mag, 0, 0.000001), "Cannot normalize a zero-length vector");
            x /= mag;
            y /= mag;
            z /= mag;
            return this;
        }

        public xyz normalize()
        {
            double mag = magnitude();
            Debug.Assert(!fp.eq_tol(mag, 0, 0.000001), "Cannot normalize a zero-length vector");
            return new xyz(x / mag, y / mag, z / mag);
        }

        internal static xyz normal(xyz p1, xyz p2, xyz p3)
        {
            // http://www.geocities.com/SiliconValley/2151/math3d.html
            // normal = (p1-p2) x (p3-p2)

            //return cross(p1-p2, p3-p2);

            return cross_subs(p1, p2, p3);
        }

        public static xyz operator -(xyz a)
        {
            return new xyz(-a.x, -a.y, -a.z);
        }

        internal xyz negate_in_place()
        {
            x = -x;
            y = -y;
            z = -z;
            return this;
        }

        public static xyz operator *(double d, xyz a)
        {
            xyz n = new xyz(a.x * d, a.y * d, a.z * d);
            return n;
        }

        public static xyz operator *(xyz a, double d)
        {
            xyz n = new xyz(a.x * d, a.y * d, a.z * d);
            return n;
        }

        public static xyz operator /(xyz a, double d)
        {
            xyz n = new xyz(a.x / d, a.y / d, a.z / d);
            return n;
        }

        public static xyz operator +(xyz a, xyz b)
        {
            xyz n = new xyz(a.x + b.x, a.y + b.y, a.z + b.z);
            return n;
        }

        public static xyz operator -(xyz a, xyz b)
        {
            xyz n = new xyz(a.x - b.x, a.y - b.y, a.z - b.z);
            return n;
        }

        internal double this[int i]
        {
            get
            {
                if (i == 0)
                {
                    return x;
                }
                if (i == 1)
                {
                    return y;
                }
                if (i == 2)
                {
                    return z;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

#if ROUND
		public void round()
		{
			x = ut.round(x);
			y = ut.round(y);
			z = ut.round(z);
		}

        public void round(int prec)
        {
            x = ut.round(x, prec);
            y = ut.round(y, prec);
            z = ut.round(z, prec);
        }
#endif

        internal static double dotsub(xyz v1, xyz p1, xyz p2)
        {
            // calculates dot(v1, (p1-p2))

            return (v1.x * (p1.x - p2.x) + v1.y * (p1.y - p2.y) + v1.z * (p1.z - p2.z));
        }

        internal static double dot(xyz v1, xyz v2)
        {
            return (v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
        }

        internal static xyz cross_subs(xyz a, xyz b, xyz c)
        {
            double x = (b.y - a.y) * (c.z - a.z) - (b.z - a.z) * (c.y - a.y);
            double y = (b.z - a.z) * (c.x - a.x) - (b.x - a.x) * (c.z - a.z);
            double z = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            return new xyz(x, y, z);
        }

        internal static xyz cross(xyz v2, xyz v3)
        {
            double x = v2.y * v3.z - v2.z * v3.y;
            double y = v2.z * v3.x - v2.x * v3.z;
            double z = v2.x * v3.y - v2.y * v3.x;
            return new xyz(x, y, z);
        }
    }

    internal class xy
    {
        public double x;
        public double y;

        public xyz orig;

        public xy copy()
        {
            xy p = new xy(x, y);
            // TODO is the following correct?
            if (orig != null)
            {
                p.orig = orig.copy();
            }
            return p;
        }

#if ROUND
		public void round()
		{
			x = ut.round(x);
			y = ut.round(y);
		}

        public void round(int prec)
        {
            x = ut.round(x, prec);
            y = ut.round(y, prec);
        }
#endif

        public static xy operator /(xy a, double d)
        {
            xy n = new xy(a.x / d, a.y / d);
            return n;
        }

        public static xy operator -(xy a)
        {
            return new xy(-a.x, -a.y);
        }

        public static xy operator *(double d, xy a)
        {
            xy n = new xy(a.x * d, a.y * d);
            return n;
        }

        public static xy operator *(xy a, double d)
        {
            xy n = new xy(a.x * d, a.y * d);
            return n;
        }

        public double magnitude_squared()
        {
            return (x * x + y * y);
        }

        public double magnitude()
        {
#if true
            // TODO these should probably be ut.eq
            if (x == 0)
            {
                return Math.Abs(y);
            }
            else if (y == 0)
            {
                return Math.Abs(x);
            }
            else
#endif
            {
                return Math.Sqrt(x * x + y * y);
            }
        }

        public xy normalize()
        {
            double mag = magnitude();
            Debug.Assert(!fp.eq_inches(mag, 0), "Cannot normalize a zero-length vector");
            return new xy(x / mag, y / mag);
        }

        public xy normalize_in_place()
        {
            double mag = magnitude();
            Debug.Assert(!fp.eq_inches(mag, 0), "Cannot normalize a zero-length vector");
            x /= mag;
            y /= mag;
            return this;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        public static double dot(xy a, xy b)
        {
            // x1*x2 + y1*y2
            return a.x * b.x + a.y * b.y;
        }

        public static double kross(xy a, xy b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static xy operator -(xy a, xy b)
        {
            xy n = new xy(a.x - b.x, a.y - b.y);
            return n;
        }

        public static xy operator +(xy a, xy b)
        {
            xy n = new xy(a.x + b.x, a.y + b.y);
            return n;
        }

        public xy(double _x, double _y)
        {
            x = _x;
            y = _y;
        }
    }
}
