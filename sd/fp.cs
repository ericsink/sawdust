
using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class fp
    {
        public static bool eq_inches(xy a, xy b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            return fp.eq_inches(a.x, b.x)
                && fp.eq_inches(a.y, b.y);
        }

        public static bool eq_inches(xyz a, xyz b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            return fp.eq_inches(a.x, b.x)
                && fp.eq_inches(a.y, b.y)
                && fp.eq_inches(a.z, b.z);
        }

        public static bool eq_unitvec(xy a, xy b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            const double eps = 0.0001; // TODO

            return fp.eq_tol(a.x, b.x, eps)
                && fp.eq_tol(a.y, b.y, eps);
        }

        public static bool eq_unitvec(xyz a, xyz b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            const double eps = 0.0001; // TODO

            return fp.eq_tol(a.x, b.x, eps)
                && fp.eq_tol(a.y, b.y, eps)
                && fp.eq_tol(a.z, b.z, eps);
        }

#if ROUND
		public const int PRECISION_UNITS_PER_INCH = 100000;

		public static void round(List<xy> a)
		{
			foreach (xy p in a)
			{
				p.round();
			}
		}

        public static void round(List<xy> a, int prec)
        {
            foreach (xy p in a)
            {
                p.round(prec);
            }
        }

        public static double round(double d, int precision)
		{
			double d2 = d * precision;

			if (d >= 0)
			{
				d2 += 0.5;
			}
			else 
			{
				d2 -= 0.5;
			}

			int q = (int) d2;

			double q3 = q / ((double) precision);

			return q3;
		}

		public static double round(double d)
		{
			return round(d, PRECISION_UNITS_PER_INCH);
		}

        public static List<xyz> round(List<xyz> p)
        {
            foreach (xyz v in p)
            {
                v.round();
            }
            return p;
        }
#endif

        public static bool eq_tol(double a, double b, double eps)
        {
            return Math.Abs(a - b) < eps;
        }

        public const double EPSILON_INCHES = 0.0005;

        public static bool eq_inches(double a, double b)
        {
            return Math.Abs(a - b) < EPSILON_INCHES;
        }

        public static bool eq_dot_distancetoplane(double a, double b)
        {
            return Math.Abs(a - b) < EPSILON_INCHES;
        }

        public static bool eq_area(double a, double b)
        {
            double eps = Math.Sqrt(Math.Max(a, b)) * EPSILON_INCHES * 2;
            return Math.Abs(a - b) < eps;
        }

#if DEBUG
        public static bool eq_volume(double a, double b)
        {
            // cube root
            double eps = Math.Pow(Math.Max(a, b), 1.0 / 3) * EPSILON_INCHES * 3;
            return Math.Abs(a - b) < eps;
        }

        public static bool lt_volume(double a, double b)
        {
            if (eq_volume(a, b))
            {
                return false;
            }
            return a < b;
        }
#endif

        public const double EPSILON_DEGREES = 0.01;

        public static bool eq_degrees(double a, double b)
        {
            return Math.Abs(a - b) < EPSILON_DEGREES;
        }

        public static bool eq_radians(double a, double b)
        {
            return Math.Abs(a - b) < EPSILON_DEGREES * Math.PI / 180.0;
        }

        public static bool eq_dot_unit(double a, double b)
        {
            return Math.Abs(a - b) < 0.0001; // TODO
        }

        public static bool lt_dot_unit(double a, double b)
        {
            if (eq_dot_unit(a, b))
            {
                return false;
            }
            return a < b;
        }

#if not
        public static bool lt_dot_distancetoplane(double a, double b)
        {
            if (eq_dot_distancetoplane(a, b))
            {
                return false;
            }
            return a < b;
        }

        public static bool gt_dot_distancetoplane(double a, double b)
        {
            if (eq_dot_distancetoplane(a, b))
            {
                return false;
            }
            return a > b;
        }
#endif

#if true
        public static bool eq_unknowndata(xyz a, xyz b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            return fp.eq_unknowndata(a.x, b.x)
                && fp.eq_unknowndata(a.y, b.y)
                && fp.eq_unknowndata(a.z, b.z);
        }

        public static bool eq_unknowndata(double a, double b)
        {
            return Math.Abs(a - b) < 0.0001; // TODO we don't know if this number is right
        }

        public static bool lt_unknowndata(double a, double b)
        {
            if (eq_unknowndata(a, b))
            {
                return false;
            }
            return a < b;
        }

        public static bool gt_unknowndata(double a, double b)
        {
            if (eq_unknowndata(a, b))
            {
                return false;
            }
            return a > b;
        }
#endif

        public static bool lte_inches(double a, double b)
        {
            if (eq_inches(a, b))
            {
                return true;
            }
            return a < b;
        }

        public static bool gte_inches(double a, double b)
        {
            if (eq_inches(a, b))
            {
                return true;
            }
            return a > b;
        }

        public static bool lt_inches(double a, double b)
        {
            if (eq_inches(a, b))
            {
                return false;
            }
            return a < b;
        }

        public static bool gt_inches(double a, double b)
        {
            if (eq_inches(a, b))
            {
                return false;
            }
            return a > b;
        }

        internal static int getsign_dot_distancetoplane(double d)
        {
            if (eq_dot_distancetoplane(d, 0))
            {
                return 0;
            }
            else if (d > 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
