using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class seg2d
    {
        public xy a;
        public xy b;

        public static void GetSegs(List<seg2d> segs, List<xy> p)
        {
            for (int i = 0; i < p.Count; i++)
            {
                int j = (i + 1) % p.Count;
                segs.Add(new seg2d(p[i], p[j]));
            }
        }

        public static List<seg2d> Convert(List<xy> p)
        {
            List<seg2d> segs = new List<seg2d>();
            GetSegs(segs, p);
            return segs;
        }

        public static List<seg2d> Convert(List<List<xy>> p)
        {
            List<seg2d> segs = new List<seg2d>();

            foreach (List<xy> loop in p)
            {
                GetSegs(segs, loop);
            }

            return segs;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", a, b);
        }

        public seg2d(xy _a, xy _b)
        {
            a = _a;
            b = _b;

            Debug.Assert(!fp.eq_inches(a, b));
        }

        public double Length
        {
            get
            {
                xy v = b - a;
                return v.magnitude();
            }
        }

        public static seg2d find_seg_a(List<seg2d> segs, xy p, out xy next)
        {
            next = null;
            seg2d result = null;
            foreach (seg2d s in segs)
            {
                if (fp.eq_inches(s.a, p))
                {
                    next = s.b;
                    result = s;
                    break;
                }
            }
            return result;
        }

#if not
        public static bool Contains(List<seg2d> segs, seg2d s)
        {
            foreach (seg2d seg in segs)
            {
                if (eq(seg, s))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Add(List<seg2d> segs, seg2d s)
        {
            if (!Contains(segs, s))
            {
                segs.Add(s);
            }
        }

        public static bool eq(seg2d s1, seg2d s2)
        {
            if (
                fp.eq_inches(s1.a, s2.a)
                && fp.eq_inches(s1.b, s2.b)
                )
            {
                return true;
            }
            if (
                fp.eq_inches(s1.a, s2.b)
                && fp.eq_inches(s1.b, s2.a)
                )
            {
                return true;
            }
            return false;
        }
#endif

        internal seg2d reverse()
        {
            return new seg2d(b, a);
        }
    }
}

