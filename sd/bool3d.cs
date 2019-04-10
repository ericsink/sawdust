
#undef BOOL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;

using EricSinkMultiCoreLib;

namespace sd
{
    internal class bool3d
    {
        private static void FixSegmentDirection(seg3d s1, Face f1, Face f2, bool reverse)
        {
            xyz n1 = f1.UnitNormal();
            xyz n2 = f2.UnitNormal();
            xyz k = xyz.cross(n2, n1).normalize_in_place();
            if (reverse)
            {
                k.negate_in_place();
            }

            xyz vab = (s1.b - s1.a).normalize_in_place();
            if (!fp.eq_unitvec(vab, k))
            {
                xyz t = s1.a;
                s1.a = s1.b;
                s1.b = t;
            }
        }

        public static void segs_Remove(List<seg3d> segs, seg3d p)
        {
            List<seg3d> rm = new List<seg3d>();
            List<seg3d> add = new List<seg3d>();
            foreach (seg3d q in segs)
            {
                //List<seg3d> onlyp = new List<seg3d>();
                List<seg3d> onlyq = new List<seg3d>();
                //List<seg3d> common = new List<seg3d>();
                SegmentsOverlap3d so = ut.CalcSegmentsOverlap3d(p, q, null, onlyq, null);
                Debug.Assert(
                    (so == SegmentsOverlap3d.SameSegment)
                    || (so == SegmentsOverlap3d.None)
                    || (so == SegmentsOverlap3d.OppositeDirection)
                    || (so == SegmentsOverlap3d.P_On_Q)
                    || (so == SegmentsOverlap3d.Q_On_P)
                    || (so == SegmentsOverlap3d.Overlap)
                    );
                switch (so)
                {
                    case SegmentsOverlap3d.SameSegment:
                        {
                            rm.Add(q);
                            break;
                        }
                    case SegmentsOverlap3d.None:
                        {
                            break;
                        }
                    case SegmentsOverlap3d.OppositeDirection:
                        {
                            break;
                        }
                    case SegmentsOverlap3d.P_On_Q:
                    case SegmentsOverlap3d.Q_On_P:
                    case SegmentsOverlap3d.Overlap:
                        {
                            rm.Add(q);
                            foreach (seg3d ns in onlyq)
                            {
                                segs_Add(add, ns);
                            }
                            break;
                        }
                }
            }
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
            foreach (seg3d s in add)
            {
                segs.Add(s);
            }
        }

        public static void segs_Add(List<seg3d> segs, seg3d s)
        {
            segs_Add(segs, s, 0);
        }

        public static void segs_Add(List<seg3d> segs, seg3d p, int level)
        {
            List<seg3d> add = new List<seg3d>();

            // start with the whole seg.  we'll remove pieces of it wherever overlaps occur
            add.Add(p);
            foreach (seg3d q in segs)
            {
                //List<seg3d> onlyp = new List<seg3d>();
                //List<seg3d> onlyq = new List<seg3d>();
                List<seg3d> common = new List<seg3d>();
                SegmentsOverlap3d so = ut.CalcSegmentsOverlap3d(p, q, null, null, common);
                Debug.Assert(
                    (so == SegmentsOverlap3d.SameSegment)
                    || (so == SegmentsOverlap3d.None)
                    || (so == SegmentsOverlap3d.OppositeDirection)
                    || (so == SegmentsOverlap3d.P_On_Q)
                    || (so == SegmentsOverlap3d.Q_On_P)
                    || (so == SegmentsOverlap3d.Overlap)
                    );
                switch (so)
                {
                    case SegmentsOverlap3d.SameSegment:
                        {
                            segs_Remove(add, p);
                            break;
                        }
                    case SegmentsOverlap3d.None:
                        {
                            break;
                        }
                    case SegmentsOverlap3d.OppositeDirection:
                        {
                            break;
                        }
                    case SegmentsOverlap3d.P_On_Q:
                    case SegmentsOverlap3d.Q_On_P:
                    case SegmentsOverlap3d.Overlap:
                        {
                            foreach (seg3d ns in common)
                            {
                                segs_Remove(add, ns);
                            }
                            break;
                        }
                }

                if (add.Count == 0)
                {
                    // nothing left.  we must be done.
                    return;
                }
            }

            foreach (seg3d s in add)
            {
                segs.Add(s);
            }
        }

        public static void PartitionFace_RemoveByDirection(List<seg3d> segs, Face f1, Solid s2, bool reverse)
        {
            List<seg3d> rm = new List<seg3d>();
            foreach (seg3d s in segs)
            {
                foreach (Face f2 in s2.Faces)
                {
                    xyz n = f2.myPlane.n;
                    xyz p0 = f2.myPlane.pts[0];

                    double dist_a = xyz.dotsub(n, s.a, p0);
                    double dist_b = xyz.dotsub(n, s.b, p0);

                    int sign_a = fp.getsign_dot_distancetoplane(dist_a);
                    int sign_b = fp.getsign_dot_distancetoplane(dist_b);

                    if (
                        (sign_b == 0)
                        && (sign_a != 0)
                        && (PointFaceIntersection.Inside == f2.CalcPFI(s.b))
                        )
                    {
                        // this segment is moving into a face.  find out which way the face is pointing
                        if (xyz.dotsub(f2.UnitNormal(), s.b, s.a) < 0)
                        {
                            // the segment is outside s2
                            if (reverse)
                            {
                                rm.Add(s);
                            }
                        }
                        else
                        {
                            // the segment is inside s2
                            if (!reverse)
                            {
                                rm.Add(s);
                            }
                        }
                        break;
                    }
                    else if (
                        (sign_b != 0)
                        && (sign_a == 0)
                        && (PointFaceIntersection.Inside == f2.CalcPFI(s.a))
                        )
                    {
                        // this segment is moving away from a face.  find out which way the face is pointing
                        if (xyz.dotsub(f2.UnitNormal(), s.a, s.b) < 0)
                        {
                            // the segment is outside s2
                            if (reverse)
                            {
                                rm.Add(s);
                            }
                        }
                        else
                        {
                            // the segment is inside s2
                            if (!reverse)
                            {
                                rm.Add(s);
                            }
                        }
                        break;
                    }
                }
            }
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
        }

        public static void PartitionFace_RemoveObvious(List<seg3d> segs, Face f1, Solid s2, bsp3d bsp2, bool reverse)
        {
            List<seg3d> rm = new List<seg3d>();
            foreach (seg3d s in segs)
            {
                PointInPoly pip_a = bsp2.PointInPolyhedron(s.a);
                if (pip_a == PointInPoly.Coincident)
                {
                    break;
                }
                PointInPoly pip_b = bsp2.PointInPolyhedron(s.b);
                if (pip_b == PointInPoly.Coincident)
                {
                    break;
                }

                if (pip_a != pip_b)
                {
                    break;
                }

#if false // coverage says this never gets hit
                bool bNone = true;
                foreach (Face f2 in s2.Faces)
				{
					if (f2.SegmentTouches(s))
					{
                        // coverage says this line is never hit
                        // which may mean this entire check is
                        // unneeded.
                        bNone = false;
                        break;
					}
				}
				if (bNone)
#endif
                {
                    bool bKeep = false;
                    if (s2.PointInside(bsp2, s.b))
                    {
                        bKeep = false;
                    }
                    else
                    {
                        bKeep = true;
                    }
                    if (reverse)
                    {
                        bKeep = !bKeep;
                    }
                    if (!bKeep)
                    {
                        rm.Add(s);
                    }
                }
            }
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
        }

        public static void PartitionFace_SplitEdges(List<seg3d> segs, Solid s2, BoundingBox3d bb2, bool reverse)
        {
            List<seg3d> rm = new List<seg3d>();
            List<seg3d> news = new List<seg3d>();

            for (int i = 0; i < segs.Count; i++)
            {
                seg3d s = segs[i];
                if (bb2.SegmentCannotIntersect(s.a, s.b))
                {
                    continue;
                }

                for (int j = 0; j < s2.Faces.Count; j++)
                {
                    Face f2 = s2.Faces[j];

                    xyz hit = f2.CalcSegmentFaceIntersection_HitOnly(s);
                    if (hit != null)
                    {
                        rm.Add(s);

                        seg3d to_the_face = new seg3d(s.a, hit, s.origin);
                        seg3d away_from_the_face = new seg3d(hit, s.b, s.origin);

                        news.Add(to_the_face);
                        news.Add(away_from_the_face);

                        break;
                    }
                }
            }

            if (news.Count > 0)
            {
                PartitionFace_SplitEdges(news, s2, bb2, reverse);

                foreach (seg3d s in rm)
                {
                    segs.Remove(s);
                }
                foreach (seg3d s in news)
                {
                    segs.Add(s);
                }
            }
        }

        public static bool SplitsVEP(xyz n1, xyz n2a, xyz n2b, xyz fn2a, xyz fn2b)
        {
            if (fp.eq_unitvec(n2a, -n2b))
            {
                if (fp.eq_unitvec(n1, fn2a))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            xyz n = xyz.cross(n2a, n2b);
            xyz iv = n2a;
            xyz jv = xyz.cross(n, iv).normalize_in_place();

            xy p2a = ut.ConvertPointTo2d(n2a, iv, jv);
            xy p2b = ut.ConvertPointTo2d(n2b, iv, jv);
            xy p = ut.ConvertPointTo2d(n1, iv, jv);

            double d = xyz.dot(fn2a, n2b);

            Debug.Assert(!fp.eq_dot_unit(d, 0), "If d were zero, the case above should have caught it.");

            if (d > 0)
            {
                bool b = ut.SegmentInCone(new xy(0, 0), p, p2a, p2b);
#if BOOL_DEBUG
			Console.Out.WriteLine("SplitsVEP: n1={0}  n2a={1}  n2b={2}  p={3}  p2a={4}  p2b={5}  result={6}", n1, n2a, n2b, p, p2a, p2b, b);
#endif
                return b;
            }
            else
            {
                bool b = ut.SegmentInCone(new xy(0, 0), p, p2b, p2a);
#if BOOL_DEBUG
			Console.Out.WriteLine("SplitsVEP: n1={0}  n2a={1}  n2b={2}  p={3}  p2a={4}  p2b={5}  result={6}", n1, n2a, n2b, p, p2a, p2b, b);
#endif
                return b;
            }
        }

        public static void PartitionFace_HandleCoplanarStuff(List<seg3d> segs, Face f1, Solid s2, bool reverse)
        {
            List<seg3d> rm = new List<seg3d>();
#if false // I think we *should* need this, but coverage says it never gets hit, so I am removing it for now.
            foreach (seg3d s in segs)
            {
                PointFaceIntersection pfi_a = f1.CalcPFI(s.a);
                PointFaceIntersection pfi_b = f1.CalcPFI(s.b);

                if (
                    (
                    (pfi_a == PointFaceIntersection.None)
                    && (pfi_b == PointFaceIntersection.OnEdge)
                    )
                    ||
                    (
                    (pfi_a == PointFaceIntersection.OnEdge)
                    && (pfi_b == PointFaceIntersection.None)
                    )
                    )
                {
                    rm.Add(s);
                }
            }
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
#endif

            if (segs.Count > 0)
            {
                rm.Clear();
                xyz n1 = f1.UnitNormal();
                for (int ndx_f2 = 0; ndx_f2 < s2.Faces.Count; ndx_f2++)
                {
                    Face f2 = s2.Faces[ndx_f2];
                    if (
                        fp.eq_unitvec(f2.UnitNormal(), n1)
                        && f1.IsCoPlanarWith(f2)
                        )
                    {
                        foreach (seg3d s in segs)
                        {
                            PointFaceIntersection pfi_a = f2.CalcPFI(s.a);
                            PointFaceIntersection pfi_b = f2.CalcPFI(s.b);

                            if (
                                (
                                (pfi_a == PointFaceIntersection.Inside)
                                && (pfi_b == PointFaceIntersection.OnEdge)
                                )
                                ||
                                (
                                (pfi_a == PointFaceIntersection.OnEdge)
                                && (pfi_b == PointFaceIntersection.Inside)
                                )
                                )
                            {
                                rm.Add(s);
                            }
                        }
                    }
                }
                foreach (seg3d s in rm)
                {
                    segs.Remove(s);
                }
            }
        }

        public static void PartitionFace_HandleEdges(List<seg3d> segs, Face f1, Solid s2, bsp3d bsp2, bool reverse)
        {
            List<seg3d> rm = new List<seg3d>();
            segs.ForEach(delegate (seg3d s)
            {
                HalfEdge he1a;
                if (f1.IsAnEdge(s, out he1a))
                {
                    HalfEdge he1b = he1a.Opposite();
                    for (int ndx_f2 = 0; ndx_f2 < s2.Faces.Count; ndx_f2++)
                    {
                        Face f2 = s2.Faces[ndx_f2];
                        HalfEdge he2a = null;
                        if (f2.IsAnEdge(s, out he2a))
                        {
                            HalfEdge he2b = he2a.Opposite();

                            bool bkeep;

                            xyz n1 = he1a.GetInwardNormal();
                            xyz n2a = he2a.GetInwardNormal();
                            xyz n2b = he2b.GetInwardNormal();

                            bkeep = bool3d.SplitsVEP(n1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal());

                            if (reverse)
                            {
                            }
                            else
                            {
                                bkeep = !bkeep;
                            }

                            if (!bkeep)
                            {
                                rm.Add(s);
                            }
                            break;
                        }

                        PointFaceIntersection pfi_a;
                        PointFaceIntersection pfi_b;
                        if (f2.CalcSegmentFaceIntersection_SamePlane(s, out pfi_a, out pfi_b))
                        {
                            bool bProceed = false;

                            if (
                                (
                                (pfi_a == PointFaceIntersection.Inside)
                                && (pfi_b == PointFaceIntersection.Inside)
                                )
                                ||
                                (
                                (pfi_a == PointFaceIntersection.Inside)
                                && (pfi_b == PointFaceIntersection.OnEdge)
                                )
                                ||
                                (
                                (pfi_a == PointFaceIntersection.OnEdge)
                                && (pfi_b == PointFaceIntersection.Inside)
                                )
                                )
                            {
                                bProceed = true;
                            }
                            else if (
                                (
                                (pfi_a == PointFaceIntersection.OnEdge)
                                && (pfi_b == PointFaceIntersection.OnEdge)
                                )
                                )
                            {
                                if (f2.solid.SegmentOnSurface(bsp2, s.a, s.b))
                                {
                                    bProceed = true;
                                }
                            }

                            if (bProceed)
                            {
                                bool bkeep;

                                double dot = xyz.dot(he1a.GetInwardNormal(), f2.UnitNormal());

                                if (fp.eq_dot_unit(dot, 0))
                                {
                                    // this occurs on the outside of f2
                                    bkeep = true;
                                }
                                else if (dot < 0)
                                {
                                    // this intersect occurs on the inside of f2
                                    bkeep = false;
                                }
                                else
                                {
                                    // this occurs on the outside of f2
                                    bkeep = true;
                                }

                                if (reverse)
                                {
                                    bkeep = !bkeep;
                                }

                                if (!bkeep)
                                {
                                    rm.Add(s);
                                    if (rm.Count == segs.Count)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
#if false // so far, it looks like we don't need this code.  It was written when CreateTenonInFaceWithHoleBool3dBug turned up.  I rewrote tenon to use the 4-cut method again and the bug "went away".  :-)
                    foreach (Face f2 in s2.Faces)
                    {
                        HalfEdge he2a = null;
                        if (f2.IsAnEdge(s, out he2a))
                        {
                            HalfEdge he2b = he2a.Opposite();

                            bool bkeep;

                            xyz n1 = xyz.cross((s.b - s.a).normalize_in_place(), f1.UnitNormal()).normalize_in_place();

                            xyz n2a = he2a.GetInwardNormal();
                            xyz n2b = he2b.GetInwardNormal();

                            if (bool3d.SplitsVEP(n1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal()))
                            {
                                bkeep = true;
                            }
                            else if (bool3d.SplitsVEP(-n1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal()))
                            {
                                bkeep = true;
                            }
                            else
                            {
                                bkeep = false;
                            }

                            if (reverse)
                            {
                            }
                            else
                            {
                                bkeep = !bkeep;
                            }

                            if (!bkeep)
                            {
                                rm.Add(s);
                            }
                            break;
                        }
                    }
#endif
                }
            }
            );
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
        }

        public static void PartitionFace_HandleOverlaps(List<seg3d> segs, Face f1, Solid s2, bool reverse)
        {
            xyz n1 = f1.UnitNormal();

            List<Face> faces = new List<Face>();

            for (int ndx_f2 = 0; ndx_f2 < s2.Faces.Count; ndx_f2++)
            {
                Face f2 = s2.Faces[ndx_f2];
                xyz n2 = f2.UnitNormal();
                if (
                    (fp.eq_unitvec(n1, n2) && !reverse)
                    || (fp.eq_unitvec(n1, -n2) && reverse)
                    )
                {
                    xyz p1 = f1.myPlane.pts[0];
                    xyz p2 = null;
                    foreach (HalfEdge he2 in f2.MainLoop)
                    {
                        if (!fp.eq_inches(he2.to, p1))
                        {
                            p2 = he2.to;
                            break;
                        }
                    }
                    Debug.Assert(p2 != null);
                    double d = xyz.dotsub(n1, p2, p1);
                    if (fp.eq_dot_distancetoplane(d, 0))
                    {
                        // the faces are in the same plane

                        faces.Add(f2);
                    }
                }
            }

            if (faces.Count > 0)
            {
                foreach (Face f2 in faces)
                {
                    List<List<xy>> loops2d = f1.myPlane.Convert3dLoopsTo2d(f2.GetLoopsIn3d());

                    if (reverse)
                    {
                        foreach (List<xy> loop in loops2d)
                        {
                            loop.Reverse();
                        }
                    }

                    ppi2d pi = new ppi2d(f1.myPlane.loops2d, loops2d);

                    List<seg2d> segs_common1_2d = pi.GetIntersection();

                    if (segs_common1_2d.Count > 0)
                    {
                        foreach (seg2d s in segs_common1_2d)
                        {
                            xyz a = f1.myPlane.Convert2dPointTo3d(s.a);
                            xyz b = f1.myPlane.Convert2dPointTo3d(s.b);
                            segs_Remove(segs, new seg3d(a, b));
                        }

                        if (!reverse)
                        {
                            List<seg2d> segs_f2 = new List<seg2d>();
                            pi.p2.GetSegmentsReversed(segs_f2, SegmentInfo.Inside);
                            foreach (seg2d s in segs_f2)
                            {
                                xyz a = f1.myPlane.Convert2dPointTo3d(s.a);
                                xyz b = f1.myPlane.Convert2dPointTo3d(s.b);
                                segs_Add(segs, new seg3d(a, b));
                            }
                        }
                    }
                }
            }
        }

        public static void PartitionFace_Edges_StuffInOtherSolid(List<seg3d> segs, Solid s2, bsp3d bsp2, bool reverse)
        {
            // TODO this function feels like such a hack.

            List<seg3d> rm = new List<seg3d>();
            foreach (seg3d s in segs)
            {
                bool bkeep;

                if (reverse)
                {
                    bkeep = false;
                    if (s2.SegmentInside(bsp2, s.a, s.b))
                    {
                        bkeep = true;
                    }
                    else if (s2.SegmentOnSurface(bsp2, s.a, s.b))
                    {
                        bkeep = true;
                    }
                }
                else
                {
                    bkeep = true;
                    if (
                        s2.SegmentInside(bsp2, s.a, s.b)
                        && !(s2.SegmentOnSurface(bsp2, s.a, s.b))
                        )
                    {
                        bkeep = false;
                    }
                }

                if (!bkeep)
                {
                    rm.Add(s);
                }
            }
            foreach (seg3d s in rm)
            {
                segs.Remove(s);
            }
        }

        public static void PartitionFace_CalcFaceIntersections(List<seg3d> segs, Face f1, Solid s2, bool reverse)
        {
            xyz n1 = f1.UnitNormal();
            for (int ndx_f2 = 0; ndx_f2 < s2.Faces.Count; ndx_f2++)
            {
                Face f2 = s2.Faces[ndx_f2];
                xyz n2 = f2.UnitNormal();
                if (
                    fp.eq_unitvec(n1, n2)
                    || fp.eq_unitvec(n1, -n2)
                    )
                {
                    continue;
                }

                if (!f1.MightIntersect_BB(f2))
                {
                    continue;
                }

                List<seg3d> isegs = ppi3d.CalcIntersection_NotCoplanar(f1, f2);
                if (
                    (isegs == null)
                    || (isegs.Count == 0)
                    )
                {
                    continue;
                }

                foreach (seg3d s in isegs)
                {
                    FixSegmentDirection(s, f1, f2, reverse);
                }

                if (isegs.Count > 0)
                {
                    foreach (seg3d snew in isegs)
                    {
                        HalfEdge he1;
                        HalfEdge he2a;
                        bool bIsEdgeOnFace1 = f1.IsAnEdge(snew, out he1);
                        bool bIsEdgeOnFace2 = f2.IsAnEdge(snew, out he2a);

                        if (bIsEdgeOnFace1 && bIsEdgeOnFace2)
                        {
#if false // TODO This code seems like it should be right, but it causes problems.
                            //if (reverse)
                            {
                                HalfEdge he2b = he2a.Opposite();
                                xyz ne1 = he1.GetInwardNormal();
                                xyz n2a = he2a.GetInwardNormal();
                                xyz n2b = he2b.GetInwardNormal();

                                if (bool3d.SplitsVEP(ne1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal()))
                                {
                                    segs_Remove(segs, snew.a, snew.b);
                                }
                            }
#endif
                        }
                        else if (bIsEdgeOnFace1 && !bIsEdgeOnFace2)
                        {
                            // TODO we don't need this, right?
                        }
                        else if (!bIsEdgeOnFace1 && bIsEdgeOnFace2)
                        {
                            HalfEdge he2b = he2a.Opposite();
                            xyz norm1 = xyz.cross(n1, (snew.b - snew.a).normalize_in_place()).normalize_in_place();
                            xyz n2a = he2a.GetInwardNormal();
                            xyz n2b = he2b.GetInwardNormal();
                            if (bool3d.SplitsVEP(norm1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal()))
                            {
                                segs_Add(segs, snew);
                            }
                            else if (bool3d.SplitsVEP(-norm1, n2a, n2b, he2a.face.UnitNormal(), he2b.face.UnitNormal()))
                            {
                                segs_Add(segs, snew);
                            }
                            else
                            {
                                // TODO do anything here?
                            }
                        }
                        else
                        {
                            segs_Add(segs, snew);
                        }
                    }
                }
                else
                {
                    // these two faces intersect only in points.  ignore this for now, but Hoffman says we need to analyze further.
                }
            }
        }

        internal class segpile
        {
            public List<seg3d> segs;
            public string name;
            public string OriginalName;
            public bool Shade;
            public bool reverse;

            public segpile(List<seg3d> _segs, string _name, string _oname, bool _shade, bool _reverse)
            {
                segs = _segs;
                name = _name;
                reverse = _reverse;
                OriginalName = _oname;
                Shade = _shade;
            }
        }

        public static segpile PartitionFace(Solid s3, Face f1, Solid s2, BoundingBox3d bb1, BoundingBox3d bb2, bsp3d bsp1, bsp3d bsp2, bool reverse)
        {
#if BOOL_DEBUG
			Console.Out.WriteLine("PartitionFace: f1.name={0} s2={1} reverse={2}", f1.name, s2.name, reverse);
#endif

            List<seg3d> segs = f1.CollectAllSegments();
#if BOOL_DEBUG
			ut.DumpSegments3d("CollectAllSegments", segs);
#endif

            PartitionFace_CalcFaceIntersections(segs, f1, s2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("CalcFaceIntersections", segs);
#endif

            PartitionFace_HandleOverlaps(segs, f1, s2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("HandleOverlaps", segs);
#endif

            PartitionFace_SplitEdges(segs, s2, bb2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("SplitEdges", segs);
#endif

#if true
            // TODO:  If we remove this call, only one unit test fails.
            // that means we usually don't need it.  I'd love to find a
            // way to decide when we need it so we can save the call
            // most of the time.  The failing unit test is the two
            // mortises overlapping, a case which should never happen
            // in practice, AFAIK.

            if (!reverse) // TODO for now, only call this on the !reverse pass
            {
                PartitionFace_SplitEdges(segs, f1.solid, bb1, !reverse);
            }
#if BOOL_DEBUG
            ut.DumpSegments3d("SplitEdges2", segs);
#endif
#endif

            PartitionFace_RemoveObvious(segs, f1, s2, bsp2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("RemoveObvious", segs);
#endif

            PartitionFace_HandleCoplanarStuff(segs, f1, s2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("HandleCoplanarStuff", segs);
#endif

            PartitionFace_RemoveByDirection(segs, f1, s2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("RemoveByDirection", segs);
#endif

            PartitionFace_Edges_StuffInOtherSolid(segs, s2, bsp2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("StuffInOtherSolid", segs);
#endif

            PartitionFace_HandleEdges(segs, f1, s2, bsp2, reverse);
#if BOOL_DEBUG
			ut.DumpSegments3d("HandleEdges", segs);
#endif

            Debug.Assert((segs.Count == 0) || (segs.Count >= 3));

            string name;
            if (reverse)
            {
                name = string.Format("{0}_{1}", f1.solid.name, f1.name);
            }
            else
            {
                name = f1.name;
            }

            return new segpile(segs, name, f1.OriginalName, f1.Shade, reverse);
        }

        public static Solid Subtract(Solid s1, Solid s2)
        {
            bsp3d bsp1 = new bsp3d(s1);
            bsp3d bsp2 = new bsp3d(s2);

            BoundingBox3d bb1 = s1.GetBoundingBox();
            BoundingBox3d bb2 = s2.GetBoundingBox();

            Solid s3 = new Solid(s1.name, s1.material);
            if (s1.board_origin != null)
            {
                s3.board_origin = s1.board_origin.copy();
                s3.board_u = s1.board_u.copy();
                s3.board_v = s1.board_v.copy();
                s3.board_w = s1.board_w.copy();
            }

            List<segpile> piles = new List<segpile>();
            for (int ndx_f1 = 0; ndx_f1 < s1.Faces.Count; ndx_f1++)
            {
                Face f1 = s1.Faces[ndx_f1];
                piles.Add(PartitionFace(s3, f1, s2, bb1, bb2, bsp1, bsp2, false));
            }

            for (int ndx_f2 = 0; ndx_f2 < s2.Faces.Count; ndx_f2++)
            {
                Face f2 = s2.Faces[ndx_f2];
                piles.Add(PartitionFace(s3, f2, s1, bb1, bb2, bsp2, bsp1, true));
            }

            foreach (segpile sp in piles)
            {
                if (sp.segs.Count > 0)
                {
                    s3.CreateFacesFromPileOfSegments(sp.name, sp.OriginalName, sp.Shade, sp.segs, sp.reverse);
                }
            }

            s3.FixCollinearTrios();

            return s3;
        }

        public static CompoundSolid Subtract(CompoundSolid s1, Solid s2)
        {
            CompoundSolid result = new CompoundSolid();

            BoundingBox3d bb2 = s2.GetBoundingBox();

            bool bCheck = result.HasSubOverlaps;

            foreach (Solid s in s1.Subs)
            {
                BoundingBox3d bb1 = s.GetBoundingBox();
                if (!BoundingBox3d.intersect(bb1, bb2))
                {
                    result.AddSub(s.Clone(), bCheck);
                }
                else
                {
                    result.AddSub(Subtract(s, s2), bCheck);
                }
            }

#if DEBUG
            result.AssertNoNameClashes();
#endif

            return result;
        }

        // TODO don't use this much.  it's slow.
        public static CompoundSolid Subtract(CompoundSolid s1, CompoundSolid s2)
        {
            foreach (Solid s in s2.Subs)
            {
                s1 = Subtract(s1, s);
            }
            return s1;
        }

        public static bool CheckIfTwoSolidsShareAnySpace(Solid s1, Solid s2)
        {
            BoundingBox3d bb1 = s1.GetBoundingBox();
            BoundingBox3d bb2 = s2.GetBoundingBox();

            if (!BoundingBox3d.intersect(bb1, bb2))
            {
                return false;
            }

            BoundingBox3d bb3 = BoundingBox3d.CalcIntersection(bb1, bb2);

            if (fp.eq_tol(bb3.volume, 0, 0.0001))
            {
                return false;
            }

            bsp3d bsp1 = new bsp3d(s1);
            bsp3d bsp2 = new bsp3d(s2);

            xyz c1 = s1.GetCenter();
            if (
                (bsp1.PointInPolyhedron(c1) == PointInPoly.Inside)
                && (bsp2.PointInPolyhedron(c1) == PointInPoly.Inside)
                )
            {
                return true;
            }
            xyz c2 = s2.GetCenter();
            if (
                (bsp2.PointInPolyhedron(c2) == PointInPoly.Inside)
                && (bsp1.PointInPolyhedron(c2) == PointInPoly.Inside)
                )
            {
                return true;
            }

            foreach (Face f1 in s1.Faces)
            {
                xyz p = f1.GetCenter() - f1.UnitNormal() * 0.01;
                if (
                    (bsp2.PointInPolyhedron(p) == PointInPoly.Inside)
                    && (bsp1.PointInPolyhedron(p) == PointInPoly.Inside)
                    )
                {
                    return true;
                }
            }

            foreach (Face f2 in s2.Faces)
            {
                xyz p = f2.GetCenter() - f2.UnitNormal() * 0.01;
                if (
                    (bsp1.PointInPolyhedron(p) == PointInPoly.Inside)
                    && (bsp2.PointInPolyhedron(p) == PointInPoly.Inside)
                    )
                {
                    return true;
                }
            }

            if (s1.AnyEdgePiercesAnyFace(s2))
            {
                return true;
            }

            if (s2.AnyEdgePiercesAnyFace(s1))
            {
                return true;
            }

            // TODO we apparently need another testhere

            return false;
        }
    }
}
