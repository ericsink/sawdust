#undef PP_DEBUG

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal enum PolyPolyOp
    {
        Intersection,
        Difference,
        Union,
    }

    internal enum SegmentInfo
    {
        Outside,
        Inside,
        Same,
        Opposite
    }

    internal class seginfo2d
    {
        public seg2d seg;
        public SegmentInfo info;

        public override string ToString()
        {
            return string.Format("{0}: {1}", seg, info);
        }

        public seginfo2d(seg2d s, SegmentInfo si)
        {
            seg = s;
            info = si;
        }

        public seginfo2d(seg2d s, Poly2dWithExtraStuff p)
        {
            seg = s;
            info = p.GetInfo(s);
        }
    }

    internal class Poly2dWithExtraStuff
    {
        public List<List<xy>> loops;
        public BoundingBox2d bb;
        public List<seg2d> segs;
        public List<seginfo2d> info;

        public Poly2dWithExtraStuff(List<List<xy>> oops)
        {
            loops = oops;
            bb = BoundingBox2d.FromArrayOfPoints(loops[0]);
            segs = seg2d.Convert(loops);
        }

        public Poly2dWithExtraStuff(List<xy> m)
        {
            loops = new List<List<xy>>();
            loops.Add(m);
            bb = BoundingBox2d.FromArrayOfPoints(loops[0]);
            segs = seg2d.Convert(loops);
        }

        internal SegmentInfo GetInfo(seg2d s)
        {
            // since we have split every segment such that it 
            // intersects with none others, we can now simply test the midpoint
            if (PointInside((s.a + s.b) / 2))
            {
                return SegmentInfo.Inside;
            }
            else
            {
                return SegmentInfo.Outside;
            }
        }

        internal bool PointInside(xy p)
        {
            if (bb.PointOutside(p))
            {
                return false;
            }

            if (!ut.PointInsidePoly(loops[0], p))
            {
                return false;
            }

            for (int i = 1; i < loops.Count; i++)
            {
                if (ut.PointInsidePoly(loops[i], p))
                {
                    return false;
                }
            }

            return true;
        }

        internal void GetSegments(List<seg2d> segs, SegmentInfo match)
        {
            foreach (seginfo2d si in info)
            {
                if (si.info == match)
                {
                    segs.Add(si.seg);
                }
            }
        }

        internal void GetSegmentsReversed(List<seg2d> segs, SegmentInfo match)
        {
            foreach (seginfo2d si in info)
            {
                if (si.info == match)
                {
                    segs.Add(si.seg.reverse());
                }
            }
        }
    }

    internal class ppi2d
    {
        public Poly2dWithExtraStuff p1;
        public Poly2dWithExtraStuff p2;

        public static void SplitSegment(List<seginfo2d> result, seg2d s1, Poly2dWithExtraStuff p2)
        {
            foreach (seg2d s2 in p2.segs)
            {
                if (fp.eq_inches(s1.a, s2.a) && fp.eq_inches(s1.b, s2.b))
                {
                    // same segment.
                    result.Add(new seginfo2d(s1, SegmentInfo.Same));
                    // nothing else can happen.  stop now.
                    return;
                }
                else if (fp.eq_inches(s1.a, s2.b) && fp.eq_inches(s1.b, s2.a))
                {
                    // same segment, but reversed
                    result.Add(new seginfo2d(s1, SegmentInfo.Opposite));
                    // nothing else can happen.  stop now.
                    return;
                }
                else
                {
                    xy q1;
                    xy q2;
                    SegIntersection si = ut.GetSegIntersection(s1, s2, out q1, out q2);
                    if (si == SegIntersection.Point)
                    {
                        if (
                            fp.eq_inches(q1, s1.a)
                            || fp.eq_inches(q1, s1.b)
                            )
                        {
                            // an endpoint.  this doesn't count as a hit.
                            // ignore this
                        }
                        else
                        {
                            SplitSegment(result, new seg2d(s1.a, q1), p2);
                            SplitSegment(result, new seg2d(q1, s1.b), p2);
                            return;
                        }
                    }
                    else if (si == SegIntersection.Overlap)
                    {
                        SegmentInfo dir;
                        xy v1 = (s1.b - s1.a).normalize_in_place();
                        xy v2 = (s2.b - s2.a).normalize_in_place();
                        if (fp.eq_unitvec(v1, v2))
                        {
                            dir = SegmentInfo.Same;
                        }
                        else
                        {
                            dir = SegmentInfo.Opposite;
                        }
                        if (
                            fp.eq_inches(s1.a, q1)
                            && fp.eq_inches(s1.b, q2)
                            )
                        {
                            result.Add(new seginfo2d(s1, dir));
                        }
                        else if (
                            fp.eq_inches(s1.a, q2)
                            && fp.eq_inches(s1.b, q1)
                            )
                        {
                            result.Add(new seginfo2d(s1, dir));
                        }
                        else if (fp.eq_inches(s1.a, q1))
                        {
                            result.Add(new seginfo2d(new seg2d(s1.a, q2), dir));
                            SplitSegment(result, new seg2d(q2, s1.b), p2);
                        }
                        else if (fp.eq_inches(s1.a, q2))
                        {
                            result.Add(new seginfo2d(new seg2d(s1.a, q1), dir));
                            SplitSegment(result, new seg2d(q1, s1.b), p2);
                        }
                        else if (fp.eq_inches(s1.b, q1))
                        {
                            SplitSegment(result, new seg2d(s1.a, q2), p2);
                            result.Add(new seginfo2d(new seg2d(q2, s1.b), dir));
                        }
                        else if (fp.eq_inches(s1.b, q2))
                        {
                            SplitSegment(result, new seg2d(s1.a, q1), p2);
                            result.Add(new seginfo2d(new seg2d(q1, s1.b), dir));
                        }
                        else
                        {
                            // both q1 and q2 are somewhere inside s1
                            xy z1 = q1 - s1.a;
                            xy z2 = q2 - s1.a;
                            if (z1.magnitude_squared() < z2.magnitude_squared())
                            {
                                // q1 is closer
                                SplitSegment(result, new seg2d(s1.a, q1), p2);
                                result.Add(new seginfo2d(new seg2d(q1, q2), dir));
                                SplitSegment(result, new seg2d(q2, s1.b), p2);
                            }
                            else
                            {
                                // q2 is closer
                                SplitSegment(result, new seg2d(s1.a, q2), p2);
                                result.Add(new seginfo2d(new seg2d(q2, q1), dir));
                                SplitSegment(result, new seg2d(q1, s1.b), p2);
                            }
                        }
                        return;
                    }
                }
            }
            result.Add(new seginfo2d(s1, p2));
        }

        public static void SplitAll(Poly2dWithExtraStuff p1, Poly2dWithExtraStuff p2)
        {
            p1.info = new List<seginfo2d>();

            if (!BoundingBox2d.intersect(p1.bb, p2.bb))
            {
                foreach (seg2d s1 in p1.segs)
                {
                    p1.info.Add(new seginfo2d(s1, SegmentInfo.Outside));
                }
            }
            else
            {
                foreach (seg2d s1 in p1.segs)
                {
                    SplitSegment(p1.info, s1, p2);
                }
            }
        }

        private void calc()
        {
            SplitAll(p1, p2);
            SplitAll(p2, p1);
        }

        public ppi2d(List<List<xy>> loops1, List<List<xy>> loops2)
        {
            p1 = new Poly2dWithExtraStuff(loops1);
            p2 = new Poly2dWithExtraStuff(loops2);
            calc();
        }

        public ppi2d(List<xy> m1, List<xy> m2)
        {
            p1 = new Poly2dWithExtraStuff(m1);
            p2 = new Poly2dWithExtraStuff(m2);
            calc();
        }

        public ppi2d(List<List<xy>> loops1, List<xy> m2)
        {
            p1 = new Poly2dWithExtraStuff(loops1);
            p2 = new Poly2dWithExtraStuff(m2);
            calc();
        }

        public ppi2d(List<xy> m1, List<List<xy>> loops2)
        {
            p1 = new Poly2dWithExtraStuff(m1);
            p2 = new Poly2dWithExtraStuff(loops2);
            calc();
        }

        public List<seg2d> GetIntersection()
        {
            List<seg2d> segs = new List<seg2d>();
            p1.GetSegments(segs, SegmentInfo.Inside);
            p2.GetSegments(segs, SegmentInfo.Inside);
            p1.GetSegments(segs, SegmentInfo.Same);
            return segs;
        }

        public List<seg2d> GetUnion()
        {
            List<seg2d> segs = new List<seg2d>();
            p1.GetSegments(segs, SegmentInfo.Outside);
            p2.GetSegments(segs, SegmentInfo.Outside);
            p1.GetSegments(segs, SegmentInfo.Same);
            return segs;
        }

        public List<seg2d> GetDifference1()
        {
            List<seg2d> segs = new List<seg2d>();
            p1.GetSegments(segs, SegmentInfo.Outside);
            p1.GetSegments(segs, SegmentInfo.Opposite);
            p2.GetSegmentsReversed(segs, SegmentInfo.Inside);
            return segs;
        }

        public List<seg2d> GetDifference2()
        {
            List<seg2d> segs = new List<seg2d>();
            p2.GetSegments(segs, SegmentInfo.Outside);
            p2.GetSegments(segs, SegmentInfo.Opposite);
            p1.GetSegmentsReversed(segs, SegmentInfo.Inside);
            return segs;
        }

#if false
        // TODO consider getting rid of this loop_starting_point stuff.
        // it was carried over from the 3d version of this code, but
        // coverage says it never gets hit.
        private static bool need_to_find_loop_starting_point(List<seg2d> segs)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                for (int j = i + 1; j < segs.Count; j++)
                {
                    if (fp.eq_inches(segs[i].a, segs[j].a))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*
         * It is possible for the bool3d code to return a segment
         * list which contains two loops that meet at a point.
         * This means one point will be the "from" portion of
         * two segments.  When we FindTheLoops on this segment
         * pile, we want to make sure we find two loops, not one
         * loop which is a non-simple polygon.  The way to do this
         * is to make sure we begin the search for a loop at the
         * troublesome point.
         * */
        private static seg2d find_loop_starting_point(List<seg2d> segs)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                for (int j = i + 1; j < segs.Count; j++)
                {
                    if (fp.eq_inches(segs[i].a, segs[j].a))
                    {
                        return segs[i];
                    }
                }
            }
            return segs[0];
        }
#endif

        public static void FindTheLoops(List<List<xy>> loops, List<seg2d> segs)
        {
#if not
            bool bTricky = need_to_find_loop_starting_point(segs);
#endif
            while (segs.Count > 0)
            {
                List<xy> pts = new List<xy>();
                seg2d cur;
#if not
                if (bTricky)
                {
                    cur = find_loop_starting_point(segs);
                }
                else
#endif
                {
                    cur = segs[0];
                }
                pts.Add(cur.a);
                pts.Add(cur.b);
                segs.Remove(cur);
                xy p0 = cur.a;
                xy plast = cur.b;
                while (true)
                {
                    xy next;
                    cur = seg2d.find_seg_a(segs, plast, out next);
                    Debug.Assert(cur != null);
                    segs.Remove(cur);
                    if (fp.eq_inches(next, p0))
                    {
                        // loop closed
                        break;
                    }
                    pts.Add(next);
                    plast = next;
                }
                loops.Add(pts);
            }
        }

        public static List<List<xy>> FindTheLoops(List<seg2d> segs)
        {
            List<List<xy>> loops = new List<List<xy>>();
            FindTheLoops(loops, segs);
            return loops;
        }

#if DEBUG
        /*
         * This method is only used by the unit tests.  It exists to provide
         * a wrapper for the new ppi2d code to present the API from the old
         * ppi2d code.  Without this wrapper, I would need to rewrite all
         * the ppi2d unit tests.
         * */
        public static List<List<xy>> Polygon2d_Intersection(List<xy> poly1, List<xy> poly2, PolyPolyOp op)
        {
            List<List<xy>> result = new List<List<xy>>();
            Debug.Assert(
                (op == PolyPolyOp.Intersection)
                || (op == PolyPolyOp.Union)
                || (op == PolyPolyOp.Difference)
                );
            ppi2d pi = new ppi2d(poly1, poly2);
            switch (op)
            {
                case PolyPolyOp.Intersection:
                    {
                        ppi2d.FindTheLoops(result, pi.GetIntersection());
                        break;
                    }
                case PolyPolyOp.Union:
                    {
                        ppi2d.FindTheLoops(result, pi.GetUnion());
                        break;
                    }
                case PolyPolyOp.Difference:
                    {
                        ppi2d.FindTheLoops(result, pi.GetDifference1());
                        break;
                    }
            }
            if (
                (result == null)
                || (result.Count == 0)
                )
            {
                return null;
            }
            else
            {
                return result;
            }
        }
#endif
    }
}
