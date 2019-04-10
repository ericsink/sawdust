#undef PP_DEBUG

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class TriangleIntersection2d
    {
        public List<xy> pts;

        public int Count
        {
            get
            {
                if (pts == null)
                {
                    return 0;
                }
                else
                {
                    return pts.Count;
                }
            }
        }

        public IEnumerator<xy> GetEnumerator()
        {
            return pts.GetEnumerator();
        }

        public void Add(xy p)
        {
            if (pts == null)
            {
                pts = new List<xy>();
            }

            foreach (xy q in pts)
            {
                if (fp.eq_inches(p, q))
                {
                    return;
                }
            }

            pts.Add(p);
        }

        public xy this[int i]
        {
            get
            {
                return pts[i];
            }
        }

        public static void find_seg(List<seg2d> segs, xy p, out xy nextpt, out seg2d seg)
        {
            seg = null;
            nextpt = null;
            foreach (seg2d s in segs)
            {
                if (fp.eq_inches(s.a, p))
                {
                    seg = s;
                    nextpt = s.b;
                    break;
                }
                if (fp.eq_inches(s.b, p))
                {
                    seg = s;
                    nextpt = s.a;
                    break;
                }
            }
            Debug.Assert(seg != null);
        }

        public static seg2d find_seg(List<seg2d> segs, xy a, xy b)
        {
            foreach (seg2d s in segs)
            {
                if (
                    (fp.eq_inches(s.a, a))
                    && (fp.eq_inches(s.b, b))
                    )
                {
                    return s;
                }
                if (
                    (fp.eq_inches(s.a, b))
                    && (fp.eq_inches(s.b, a))
                    )
                {
                    return s;
                }
            }
            return null;
        }

        public static void add_seg(List<seg2d> segs, xy a, xy b)
        {
            seg2d already = find_seg(segs, a, b);

            if (already != null)
            {
                return;
            }

            segs.Add(new seg2d(a, b));
        }

        public static void CalcSegmentTriangleIntersection2d(TriangleIntersection2d pts, List<seg2d> segs, xy a, xy b, xy ta, xy tb, xy tc)
        {
            TriangleIntersection2d ti2 = new TriangleIntersection2d();

            SegIntersection si;
            xy q1;
            xy q2;

            if (ut.PointInsideTriangle(a, ta, tb, tc))
            {
                ti2.Add(a);
            }

            si = ut.GetSegIntersection(a, b, ta, tb, out q1, out q2);
            if (si == SegIntersection.Point)
            {
                ti2.Add(q1);
            }
            else if (si == SegIntersection.Overlap)
            {
                ti2.Add(q1);
                ti2.Add(q2);
            }

            si = ut.GetSegIntersection(a, b, tb, tc, out q1, out q2);
            if (si == SegIntersection.Point)
            {
                ti2.Add(q1);
            }
            else if (si == SegIntersection.Overlap)
            {
                ti2.Add(q1);
                ti2.Add(q2);
            }

            si = ut.GetSegIntersection(a, b, tc, ta, out q1, out q2);
            if (si == SegIntersection.Point)
            {
                ti2.Add(q1);
            }
            else if (si == SegIntersection.Overlap)
            {
                ti2.Add(q1);
                ti2.Add(q2);
            }

            if (ut.PointInsideTriangle(b, ta, tb, tc))
            {
                ti2.Add(b);
            }

            Debug.Assert(
                (ti2.Count == 0)
                || (ti2.Count == 1)
                || (ti2.Count == 2)
                );

            if (ti2.Count == 0)
            {
                return;
            }
            else if (ti2.Count == 1)
            {
                pts.Add(ti2[0]);
            }
            else // if (ti2.Count == 2)
            {
                add_seg(segs, ti2[0], ti2[1]);
            }
        }

        public static TriangleIntersection2d CalcTriangleIntersection2d(xy a1, xy b1, xy c1, xy a2, xy b2, xy c2)
        {
#if PP_DEBUG
			Console.Out.WriteLine("tri 1:  a={0}  b={1}  c={2}", a1, b1, c1);
			Console.Out.WriteLine("tri 2:  a={0}  b={1}  c={2}", a2, b2, c2);
#endif

            TriangleIntersection2d pts = new TriangleIntersection2d();
            List<seg2d> segs = new List<seg2d>();

            CalcSegmentTriangleIntersection2d(pts, segs, a1, b1, a2, b2, c2);
            CalcSegmentTriangleIntersection2d(pts, segs, b1, c1, a2, b2, c2);
            CalcSegmentTriangleIntersection2d(pts, segs, c1, a1, a2, b2, c2);

            CalcSegmentTriangleIntersection2d(pts, segs, a2, b2, a1, b1, c1);
            CalcSegmentTriangleIntersection2d(pts, segs, b2, c2, a1, b1, c1);
            CalcSegmentTriangleIntersection2d(pts, segs, c2, a2, a1, b1, c1);

            TriangleIntersection2d ti = new TriangleIntersection2d();
            Debug.Assert(
                (segs.Count >= 0)
                && (segs.Count <= 6)
                );
            switch (segs.Count)
            {
                case 0:
                    {
                        Debug.Assert(
                            (pts.Count == 0)
                            || (pts.Count == 1)
                            );
                        if (pts.Count == 0)
                        {
                            return ti;
                        }
                        else // if (pts.Count == 1)
                        {
                            ti.Add(pts[0]);
                            return ti;
                        }
                    }
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    {
                        int i = 0;
                        seg2d cur = segs[i];
                        ti.Add(cur.a);
                        ti.Add(cur.b);
                        segs.Remove(cur);
                        xy lastpt = cur.b;
                        while (segs.Count > 0)
                        {
                            xy nextpt;
                            find_seg(segs, lastpt, out nextpt, out cur);
                            ti.Add(nextpt);
                            segs.Remove(cur);
                            lastpt = nextpt;
                        }

                        break;
                    }
            }

            return ti;
        }
    }
}
