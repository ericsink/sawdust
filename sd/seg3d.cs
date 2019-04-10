using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class seg3d
    {
        public xyz a;
        public xyz b;
        public HalfEdge origin;

        public override string ToString()
        {
            return string.Format("{0} to {1}", a, b);
        }

        // TODO the following constructor could be removed if we ALWAYS knew the origin
        public seg3d(xyz _a, xyz _b)
        {
            a = _a;
            b = _b;
            origin = null;
        }

        public seg3d(xyz _a, xyz _b, HalfEdge _origin)
        {
            a = _a;
            b = _b;
            origin = _origin;
        }

        public double Length
        {
            get
            {
                xyz v = b - a;
                return v.magnitude();
            }
        }

        public static seg3d find_seg_a(List<seg3d> segs, xyz p, out xyz next)
        {
            next = null;
            seg3d result = null;
            foreach (seg3d s in segs)
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

        public static bool Contains(List<seg3d> segs, seg3d s)
        {
            foreach (seg3d seg in segs)
            {
                if (eq(seg, s))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Add(List<seg3d> segs, seg3d s)
        {
            if (!Contains(segs, s))
            {
                segs.Add(s);
            }
        }

        public static bool eq(seg3d s1, seg3d s2)
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
    }
}

