using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal class TriangleIntersection3d
    {
        public List<xyz> pts;

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

        public IEnumerator<xyz> GetEnumerator()
        {
            return pts.GetEnumerator();
        }

        public void Add(xyz p)
        {
            if (p == null)
            {
                return;
            }

            if (pts == null)
            {
                pts = new List<xyz>();
            }

            foreach (xyz q in pts)
            {
                if (fp.eq_inches(p, q))
                {
                    return;
                }
            }

            pts.Add(p);
        }

        public xyz this[int i]
        {
            get
            {
                return pts[i];
            }
        }
    }

    internal class ppi3d
    {
        public static TriangleIntersection3d CalcTriangleIntersection3d_NotCoplanar(Triangle3d t1, Triangle3d t2)
        {
            int sign_t2_a = t1.GetSide(t2.a);
            int sign_t2_b = t1.GetSide(t2.b);
            int sign_t2_c = t1.GetSide(t2.c);

            int sign_t1_a = t2.GetSide(t1.a);
            int sign_t1_b = t2.GetSide(t1.b);
            int sign_t1_c = t2.GetSide(t1.c);

            TriangleIntersection3d ti = new TriangleIntersection3d();

            if (!((sign_t2_a == sign_t2_b) && (sign_t2_b == sign_t2_c)))
            {
                xyz pt1;
                xyz pt2;
                if ((sign_t2_a != sign_t2_b) && TriangleLineSegmentIntersect3d(t1, t2.a, t2.b, out pt1, out pt2, sign_t2_a, sign_t2_b))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
                if ((sign_t2_b != sign_t2_c) && TriangleLineSegmentIntersect3d(t1, t2.b, t2.c, out pt1, out pt2, sign_t2_b, sign_t2_c))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
                if ((sign_t2_c != sign_t2_a) && TriangleLineSegmentIntersect3d(t1, t2.c, t2.a, out pt1, out pt2, sign_t2_c, sign_t2_a))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
                if ((sign_t1_a != sign_t1_b) && TriangleLineSegmentIntersect3d(t2, t1.a, t1.b, out pt1, out pt2, sign_t1_a, sign_t1_b))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
                if ((sign_t1_b != sign_t1_c) && TriangleLineSegmentIntersect3d(t2, t1.b, t1.c, out pt1, out pt2, sign_t1_b, sign_t1_c))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
                if ((sign_t1_c != sign_t1_a) && TriangleLineSegmentIntersect3d(t2, t1.c, t1.a, out pt1, out pt2, sign_t1_c, sign_t1_a))
                {
                    ti.Add(pt1);
                    ti.Add(pt2);
                }
            }
            return ti;
        }

        public static TriangleIntersection3d CalcTriangleIntersection3d(Triangle3d t1, Triangle3d t2)
        {
            if (ut.AllVerticesPlanar(t1, t2))
            {
                TriangleIntersection3d ti = new TriangleIntersection3d();

                xyz n = t1.n;
                xyz i = t1.iv;
                xyz j = t1.jv;

                xy a1 = ut.ConvertPointTo2d(t1.a, t1.a, i, j);
                xy b1 = ut.ConvertPointTo2d(t1.b, t1.a, i, j);
                xy c1 = ut.ConvertPointTo2d(t1.c, t1.a, i, j);

                xy a2 = ut.ConvertPointTo2d(t2.a, t1.a, i, j);
                xy b2 = ut.ConvertPointTo2d(t2.b, t1.a, i, j);
                xy c2 = ut.ConvertPointTo2d(t2.c, t1.a, i, j);

                TriangleIntersection2d ti2d = TriangleIntersection2d.CalcTriangleIntersection2d(a1, b1, c1, a2, b2, c2);
                if (ti2d.Count > 0)
                {
                    foreach (xy p in ti2d)
                    {
                        xyz q = ut.Convert2dPointTo3d(p, t1.a, i, j);
                        ti.Add(q);
                    }
                }

                return ti;
            }
            else
            {
                return CalcTriangleIntersection3d_NotCoplanar(t1, t2);
            }
        }

        // TODO the following method is only used in unit tests
        public static bool TestTriangleIntersection3d(Triangle3d t1, Triangle3d t2)
        {
            // TODO there is a faster way to do this.  See Schneider.
            if (TriangleLineSegmentIntersect3d(t1, t2.a, t2.b))
            {
                return true;
            }
            if (TriangleLineSegmentIntersect3d(t1, t2.b, t2.c))
            {
                return true;
            }
            if (TriangleLineSegmentIntersect3d(t1, t2.a, t2.c))
            {
                return true;
            }
            if (TriangleLineSegmentIntersect3d(t2, t1.a, t1.b))
            {
                return true;
            }
            if (TriangleLineSegmentIntersect3d(t2, t1.b, t1.c))
            {
                return true;
            }
#if false // this one can't be true if none of the others were.  so we remove it and assert it.
			if (TriangleLineSegmentIntersect3d(t2, t1.a, t1.c))
			{
				return true;
			}
#endif
            Debug.Assert(!TriangleLineSegmentIntersect3d(t2, t1.a, t1.c));

            return false;
        }

        public static bool TriangleLineSegmentIntersect3d(Triangle3d t, xyz d, xyz e)
        {
            xyz pt1, pt2;
            bool b = TriangleLineSegmentIntersect3d(t, d, e, out pt1, out pt2);
            return b;
        }

        public static bool TriangleLineSegmentIntersect3d(Triangle3d t, xyz d, xyz e, out xyz pt1, out xyz pt2, int sign_d, int sign_e)
        {
            if (
                (sign_d > 0)
                && (sign_e > 0)
                )
            {
                pt1 = null;
                pt2 = null;
                return false;
            }

            if (
                (sign_d < 0)
                && (sign_e < 0)
                )
            {
                pt1 = null;
                pt2 = null;
                return false;
            }

            if (
                (sign_d == 0)
                && (sign_e == 0)
                )
            {
                // the segment is in the same plane as the triangle
                // convert everything to 2d

                Triangle2d t2d = t.ConvertTo2d();
                xy d2 = ut.ConvertPointTo2d(d, t.a, t.iv, t.jv);
                xy e2 = ut.ConvertPointTo2d(e, t.a, t.iv, t.jv);

                bool bd = t2d.PointInside(d2);
                bool be = t2d.PointInside(e2);

                if (bd && be)
                {
                    pt1 = d;
                    pt2 = e;
                    return true;
                }
                else
                {
                    xy w1;
                    xy w2;
                    bool bresult = t2d.SegmentIntersection(d2, e2, out w1, out w2);
                    if (bresult)
                    {
                        pt1 = ut.Convert2dPointTo3d(w1, t.a, t.iv, t.jv);
                        if (w2 != null)
                        {
                            pt2 = ut.Convert2dPointTo3d(w2, t.a, t.iv, t.jv);
                        }
                        else
                        {
                            pt2 = null;
                        }
                    }
                    else
                    {
                        pt1 = null;
                        pt2 = null;
                    }
                    return bresult;
                }
            }
            else
            {
                // the segment intersects the plane in only one point, so pt2 will be null
                pt2 = null;

                xyz p;
                if (sign_d == 0)
                {
                    p = d;
                }
                else if (sign_e == 0)
                {
                    p = e;
                }
                else
                {
                    // find the point where the pp_segment intersects the plane
                    double u = xyz.dotsub(t.n, t.a, d) / xyz.dotsub(t.n, e, d);
                    p = (e - d).multiply_in_place(u).add_in_place(d);
                }

                Triangle2d t2d = t.ConvertTo2d();
                xy ipt = ut.ConvertPointTo2d(p, t.a, t.iv, t.jv);

                bool b = t2d.PointInside(ipt);
                if (b)
                {
                    pt1 = p.copy();
                }
                else
                {
                    pt1 = null;
                }
                return b;
            }
        }

        public static bool TriangleLineSegmentIntersect3d(Triangle3d t, xyz d, xyz e, out xyz pt1, out xyz pt2)
        {
            // get the signed distance from each point to the plane of the triangle
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the triangle.

            double dist_d = xyz.dotsub(t.n, d, t.a);
            double dist_e = xyz.dotsub(t.n, e, t.a);

            int sign_d = fp.getsign_dot_distancetoplane(dist_d);
            int sign_e = fp.getsign_dot_distancetoplane(dist_e);

            return TriangleLineSegmentIntersect3d(t, d, e, out pt1, out pt2, sign_d, sign_e);
        }

#if not // never used, not sure we need
        public static List<TriangleIntersection3d> CalcIntersection_NotCoplanar(List<Triangle3d> tris1, List<List<xyz>> loops2)
        {
            List<Triangle3d> tris2 = new List<Triangle3d>();

            tri.Triangulate3d_WithHoles(tris2, loops2);

            List<TriangleIntersection3d> result = new List<TriangleIntersection3d>();

            foreach (Triangle3d t1 in tris1)
            {
                foreach (Triangle3d t2 in tris2)
                {
                    TriangleIntersection3d ti = CalcTriangleIntersection3d_NotCoplanar(t1, t2);
                    if (ti.Count > 0)
                    {
                        result.Add(ti);
                    }
                }
            }

            return result;
        }

        public static List<TriangleIntersection3d> CalcIntersection_NotCoplanar(List<List<xyz>> loops1, List<List<xyz>> loops2)
		{
            if (!MightIntersect_BB(loops1[0], loops2[0]))
            {
                return null;
            }

            List<Triangle3d> tris1 = new List<Triangle3d>();

			tri.Triangulate3d_WithHoles(tris1, loops1);

            return CalcIntersection_NotCoplanar(tris1, loops2);
		}
#endif

        public static bool MightIntersect_BB(List<xyz> main1, List<xyz> main2)
        {
            BoundingBox3d bb1 = BoundingBox3d.FromArrayOfPoints(main1);
            BoundingBox3d bb2 = BoundingBox3d.FromArrayOfPoints(main2);

            return BoundingBox3d.intersect(bb1, bb2);
        }

        public static List<TriangleIntersection3d> CalcIntersection(List<List<xyz>> loops1, List<List<xyz>> loops2)
        {
            List<xyz> main1 = loops1[0];
            List<xyz> main2 = loops2[0];

            if (!MightIntersect_BB(main1, main2))
            {
                return null;
            }

            List<Triangle3d> tris1 = new List<Triangle3d>();
            List<Triangle3d> tris2 = new List<Triangle3d>();

            tri.Triangulate3d_WithHoles(tris1, loops1);
            tri.Triangulate3d_WithHoles(tris2, loops2);

            List<TriangleIntersection3d> result = new List<TriangleIntersection3d>();

            foreach (Triangle3d t1 in tris1)
            {
                foreach (Triangle3d t2 in tris2)
                {
                    TriangleIntersection3d ti = CalcTriangleIntersection3d(t1, t2);
                    if (ti.Count > 0)
                    {
                        result.Add(ti);
                    }
                }
            }

            return result;
        }

        public static bool TestIntersection(List<List<xyz>> loops1, List<List<xyz>> loops2)
        {
            List<TriangleIntersection3d> isect = ppi3d.CalcIntersection(loops1, loops2);
            return (isect != null) && (isect.Count > 0);
        }

        public static List<seg3d> CalcIntersection_NotCoplanar(Face f1, Face f2)
        {
            List<seg3d> segs_from_f1 = f1.IntersectWithPlaneOfOtherFace(f2);
            List<seg3d> segs_from_f2 = f2.IntersectWithPlaneOfOtherFace(f1);

            if (
                (segs_from_f1 == null)
                || (segs_from_f2 == null)
                || (segs_from_f1.Count == 0)
                || (segs_from_f2.Count == 0)
                )
            {
                return null;
            }

            List<seg3d> result = new List<seg3d>();
            // TODO these segments are sorted, so we should not need to run through the f2 list for every seg in the f1 list
            foreach (seg3d s1 in segs_from_f1)
            {
                int count = 0;
                foreach (seg3d s2 in segs_from_f2)
                {
                    seg3d s3 = ut.CalcUndirectedSegmentsOverlap3d(s1, s2);
                    if (s3 != null)
                    {
                        result.Add(s3);
                    }
                    else
                    {
                        if (count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
