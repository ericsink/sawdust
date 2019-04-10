
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using EricSinkMultiCoreLib;

namespace sd
{
    public enum FaceQuality
    {
        Good,
        EndGrain,
        Ugly,
        Unknown
    }

    internal class EdgeLoop
    {
        private List<HalfEdge> halfedges;
        private List<xyz> vertices;

        internal EdgeLoop()
        {
        }

        internal void Remove(HalfEdge he, xyz deadvert)
        {
            vertices.Remove(deadvert);
            halfedges.Remove(he);
        }

        internal int[] GetVertexIndices_xyz(Dictionary<xyz, int> vertassoc)
        {
            int[] result = new int[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                HalfEdge e = this[i];
                xyz v = e.to;
                result[i] = vertassoc[v];
            }
            return result;
        }

        public int[] GetVertexIndices(Solid s)
        {
            int[] result = new int[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                HalfEdge e = this[i];
                xyz v = e.to;
                int ndx = s.Vertices.IndexOf(v);
                result[i] = ndx;
            }
            return result;
        }

        public IEnumerator<HalfEdge> GetEnumerator()
        {
            return halfedges.GetEnumerator();
        }

        public void Kill()
        {
            for (int i = 0; i < this.Count; i++)
            {
                HalfEdge h = this[i];
                if (h.edge.a2b == h)
                {
                    h.edge.a2b = null;
                }
                else
                {
                    h.edge.b2a = null;
                }
            }
        }

        public int Count
        {
            get
            {
                return halfedges.Count;
            }
        }

        public int IndexOf(HalfEdge he)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == he)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FixIndex(int i)
        {
            while (i < 0)
            {
                i += halfedges.Count;
            }
            i = i % halfedges.Count;
            return i;
        }

        public HalfEdge this[int i]
        {
            get
            {
                return halfedges[i % halfedges.Count];
            }
        }

#if DEBUG
        private void AssertVerticesValid()
        {
            Debug.Assert(halfedges.Count == vertices.Count);
            for (int i = 0; i < halfedges.Count; i++)
            {
                Debug.Assert(object.ReferenceEquals(halfedges[i].to, vertices[i]));
            }
        }
#endif

        public void DoGeomChecks()
        {
#if DEBUG
            AssertVerticesValid();
#endif

            Debug.Assert(ut.PolygonIsSimple3d(Vertices));
            //throw new GeomCheckException("Edge loop must be a simple polygon.");
        }

        public List<xyz> Vertices
        {
            get
            {
                return vertices;
            }
        }

        // NOTE that the collection returned from this call must not be modified
        public List<xyz> CollectAllVertices()
        {
            return Vertices;
        }

        public void CollectAllVertices(List<xyz> a)
        {
            a.AddRange(Vertices);
        }

        public void CollectAllSegments(List<seg3d> a)
        {
            foreach (HalfEdge cur in this)
            {
                seg3d s = new seg3d(cur.from, cur.to, cur);
                a.Add(s);
            }
        }

        public void CreateHalfEdges(Solid solid, Face f, params int[] verts)
        {
            halfedges = new List<HalfEdge>();
            vertices = new List<xyz>();
            for (int i = 0; i < verts.Length; i++)
            {
                int j = (i + 1) % verts.Length;
                xyz from = solid.GetVertex(verts[i]);
                xyz to = solid.GetVertex(verts[j]);
                HalfEdge he = solid.CreateHalfEdge(f, from, to);
                halfedges.Add(he);
                vertices.Add(he.to);
            }
#if DEBUG
            AssertVerticesValid();
#endif
        }
    }

    internal enum PointFaceIntersection
    {
        None,
        Inside,
        OnEdge,
    }

    internal class Plane
    {
        public xyz n;
        public xyz i;
        public xyz j;
        public List<xyz> pts;
        public List<List<xy>> loops2d;

        public xy ConvertPointTo2d(xyz p)
        {
            return ut.ConvertPointTo2d(p, pts[0], i, j);
        }

        public List<List<xy>> Convert3dLoopsTo2d(List<List<xyz>> loops3d)
        {
            return ut.Convert3dLoopsTo2d(loops3d, pts[0], i, j);
        }

        internal xyz Convert2dPointTo3d(xy p)
        {
            return ut.Convert2dPointTo3d(p, pts[0], i, j);
        }
    }

    public class Face
    {
        public string name;
        public string OriginalName;
        public Solid solid;
        internal List<EdgeLoop> loops;
        public bool Shade;

        public override string ToString()
        {
            return string.Format("{0}(OriginalName={1})", name, OriginalName);
        }

        public string FullName
        {
            get
            {
                return string.Format("{0}.{1}", solid.name, name);
            }
        }

        internal int GetSide(xyz p)
        {
            double ktest = xyz.dotsub(myPlane.n, p, MainLoop[0].to);
            return fp.getsign_dot_distancetoplane(ktest);
        }

        internal bool AnyEdgePierces(Solid s2)
        {
            foreach (Edge e in s2.Edges)
            {
                if (CalcSegmentFaceIntersection_AnyEdgePierces(e.a, e.b))
                {
                    return true;
                }
            }

            return false;
        }

        // TODO instead of modeling this like a cache, model it like myPlane and recalc it only when needed
        private BoundingBox3d bbCache;

        internal BoundingBox3d GetBoundingBox()
        {
            // TODO do we need to lock (this) ?
            {
                if (bbCache == null)
                {
                    bbCache = BoundingBox3d.FromArrayOfPoints(MainLoop.Vertices);
                }
            }
            return bbCache;
        }

        public xyz Measure(HalfEdge he, xyz p1)
        {
            Debug.Assert(ut.PointOnSegment(p1, he.from, he.to));

            xyz p2 = p1 + he.GetInwardNormal() * 1000;

            xy p1_2d = this.myPlane.ConvertPointTo2d(p1);
            xy p2_2d = this.myPlane.ConvertPointTo2d(p2);

            xy result = null;
            double dist = 0;
            List<List<xy>> loops2d = this.GetLoopsIn2d();
            foreach (List<xy> loop2d in loops2d)
            {
                for (int i = 0; i < loop2d.Count; i++)
                {
                    xy q1 = loop2d[i];
                    xy q2 = loop2d[(i + 1) % loop2d.Count];
                    xy intpt1;
                    xy intpt2;
                    SegIntersection si = ut.GetSegIntersection(new seg2d(p1_2d, p2_2d), new seg2d(q1, q2), out intpt1, out intpt2);
                    if (si != SegIntersection.None)
                    {
                        double d2 = (intpt1 - p1_2d).magnitude_squared();
                        if (fp.gt_inches(d2, 0))
                        {
                            if (
                                (result == null)
                                || (d2 < dist)
                                )
                            {
                                result = intpt1;
                                dist = d2;
                            }
                        }
                        if (si == SegIntersection.Overlap)
                        {
                            d2 = (intpt2 - p1_2d).magnitude_squared();
                            if (fp.gt_inches(d2, 0))
                            {
                                if (
                                    (result == null)
                                    || (d2 < dist)
                                    )
                                {
                                    result = intpt2;
                                    dist = d2;
                                }
                            }
                        }
                    }
                }
            }
            xyz phit = myPlane.Convert2dPointTo3d(result);
            return phit;
        }

        public HalfEdge FindLongestEdge()
        {
            HalfEdge result = null;
            double length = 0;
            foreach (HalfEdge he in MainLoop)
            {
                double len = he.Length();
                if (
                    (result == null)
                    || (len > length)
                    )
                {
                    result = he;
                    length = len;
                }
            }
            return result;
        }

        internal bool MightIntersect_BB(Face f2)
        {
            BoundingBox3d bb1 = GetBoundingBox();
            BoundingBox3d bb2 = f2.GetBoundingBox();

            return BoundingBox3d.intersect(bb1, bb2);
        }

        internal List<seg3d> IntersectWithPlaneOfOtherFace(Face f1)
        {
            List<xyz> pts = new List<xyz>();
            Dictionary<xyz, HalfEdge> origins = new Dictionary<xyz, HalfEdge>();
            foreach (EdgeLoop el in this.loops)
            {
                for (int i = 0; i < el.Count; i++)
                {
                    HalfEdge he = el[i];
                    xyz from = he.from;
                    xyz to = he.to;

                    int from_sign = f1.GetSide(from);
                    int to_sign = f1.GetSide(to);

                    if (
                        ((from_sign > 0) && (to_sign > 0))
                        || ((from_sign < 0) && (to_sign < 0))
                        )
                    {
                        // this segment does not touch the plane of f1
                        continue;
                    }
                    else if ((from_sign == 0) && (to_sign == 0))
                    {
                        // this segment is in the same plane as f1
                        pts.Add(from);
                        pts.Add(to);
                        origins[from] = he;
                        origins[to] = he;
                    }
                    else if (from_sign == 0)
                    {
                        // from is in the same plane as f1
                        // we ignore this case, catching transverse crossings in the to case
                        continue;
                    }
                    else if (to_sign == 0)
                    {
                        // to is in the same plane as f1
                        if (f1.GetSide(el[i + 1].to) != from_sign)
                        {
                            // transverse crossing
                            pts.Add(to);
                            origins[to] = he;
                        }
                    }
                    else
                    {
                        // this segment intersects the plane of f1
                        double u = xyz.dotsub(f1.UnitNormal(), f1.MainLoop[0].to, from) / xyz.dotsub(f1.UnitNormal(), to, from);
                        xyz pt = (to - from).multiply_in_place(u).add_in_place(from);
                        pts.Add(pt);
                        origins[pt] = he;
                    }
                }
            }
            if (pts.Count == 0)
            {
                return null;
            }
            Debug.Assert(pts.Count >= 2);

            ut.SortPointList(pts);

            ut.RemoveDuplicates(pts);

            List<seg3d> segs = new List<seg3d>();

            for (int i = 0; i < (pts.Count - 1); i++)
            {
                xyz p = pts[i];
                xyz q = pts[i + 1];

                // we assume that if the midpoint of this segment is inside the face, then the whole segment is.  :-)
                // we don't have to check the endpoints p and q, because they came from the edges of this face.
                if (CalcPointFaceIntersection((p + q).divide_in_place(2)) != PointFaceIntersection.None)
                {
                    HalfEdge hep = origins.ContainsKey(p) ? origins[p] : null;
                    HalfEdge heq = origins.ContainsKey(q) ? origins[q] : null;
                    if (
                        (hep != null)
                        && (heq != null)
                        )
                    {
                        if (hep == heq)
                        {
                            segs.Add(new seg3d(p, q, hep));
                        }
                        else if (ut.PointOnSegment(q, hep.from, hep.to))
                        {
                            segs.Add(new seg3d(p, q, hep));
                        }
                        else if (ut.PointOnSegment(p, heq.from, heq.to))
                        {
                            segs.Add(new seg3d(p, q, heq));
                        }
                        else
                        {
                            segs.Add(new seg3d(p, q, null));
                        }
                    }
                    else
                    {
                        segs.Add(new seg3d(p, q, null));
                    }
                }
            }

            return segs;
        }

        internal PointFaceIntersection CalcPointFaceIntersection(xyz pt_to_check)
        {
            // p is a point in the same plane as the face

            // convert everything to 2d and check Point in polygon
            xy pt2d = myPlane.ConvertPointTo2d(pt_to_check);

            if (ut.PointOnAnySegment(pt2d, myPlane.loops2d[0]))
            {
                return PointFaceIntersection.OnEdge;
            }

            if (ut.PointInsidePoly_NoEdgeCheck(myPlane.loops2d[0], pt2d))
            {
                // the point is inside the main loop.
                // now check the holes.
                if (myPlane.loops2d.Count == 1)
                {
                    return PointFaceIntersection.Inside;
                }

                for (int i = 1; i < myPlane.loops2d.Count; i++)
                {
                    List<xy> h2d = myPlane.loops2d[i];
                    if (ut.PointOnAnySegment(pt2d, h2d))
                    {
                        return PointFaceIntersection.OnEdge;
                    }
                    if (ut.PointInsidePoly_NoEdgeCheck(h2d, pt2d))
                    {
                        return PointFaceIntersection.None;
                    }
                }

                return PointFaceIntersection.Inside;
            }
            else
            {
                // the point is not inside the main loop.
                return PointFaceIntersection.None;
            }
        }

        public FaceQuality GetQuality()
        {
            if (solid.board_u == null)
            {
                return FaceQuality.Unknown;
            }

            xyz normal_face = this.UnitNormal();
            double deg_u = ut.RadianToDegree(ut.GetAngleBetweenTwoNormalizedVectorsInRadians(normal_face, solid.board_u.normalize()));
            double deg_v = ut.RadianToDegree(ut.GetAngleBetweenTwoNormalizedVectorsInRadians(normal_face, solid.board_v.normalize()));
            if (solid.material.IsSolid())
            {
                // TODO these angle ranges are quite arbitrary.  are they what we want?
                // TODO unit test these numbers with some cuts
                if (
                    (deg_v < 15)
                    || (deg_v > 165)
                    )
                {
                    return FaceQuality.EndGrain;
                }
                else if (
                    (deg_v > 75)
                    && (deg_v < 105)
                    )
                {
                    return FaceQuality.Good;
                }
                else
                {
                    // TODO deal with middle cases.  for now we call them good
                    return FaceQuality.Good;
                }
            }
            else
            {
                if (
                    fp.eq_degrees(deg_u, 90)
                    && fp.eq_degrees(deg_v, 90)
                    )
                {
                    // TODO for plywood, should also be able to determine if a face is parallel to a good face but internal to the board.
                    return FaceQuality.Good;
                }
                else
                {
                    return FaceQuality.Ugly;
                }
            }
        }

        internal bool IsAnEdge(seg3d s, out HalfEdge result)
        {
            if (s.origin != null)
            {
                if (s.origin.face == this)
                {
                    result = s.origin;
                    return true;
                }
                if (s.origin.Opposite().face == this)
                {
                    result = s.origin.Opposite();
                    return true;
                }
            }

            // first make sure both points are in the same plane as the face
            if (0 != fp.getsign_dot_distancetoplane(xyz.dotsub(myPlane.n, s.a, MainLoop[0].to)))
            {
                result = null;
                return false;
            }
            if (0 != fp.getsign_dot_distancetoplane(xyz.dotsub(myPlane.n, s.b, MainLoop[0].to)))
            {
                result = null;
                return false;
            }

            for (int ndx_el = 0; ndx_el < loops.Count; ndx_el++)
            {
                EdgeLoop el = loops[ndx_el];
                foreach (HalfEdge he in el)
                {
                    if (ut.IsSubsegment3d(s.a, s.b, he.from, he.to))
                    {
                        result = he;
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        private Dictionary<xyz, PointFaceIntersection> pfiCache = new Dictionary<xyz, PointFaceIntersection>();
        internal PointFaceIntersection CalcPFI(xyz p)
        {
            if (pfiCache.ContainsKey(p))
            {
                return pfiCache[p];
            }
            PointFaceIntersection pfi = CalcPointFaceIntersection(p);
            pfiCache[p] = pfi;
            return pfi;
        }

        internal bool CalcSegmentFaceIntersection_AnyEdgePierces(xyz pt1, xyz pt2)
        {
            xyz n = myPlane.n;
            xyz p0 = (myPlane.pts[0]);

            // get the signed distance from each point to the plane of the poly
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the poly.

            double dist_pt1 = xyz.dotsub(n, pt1, p0);
            double dist_pt2 = xyz.dotsub(n, pt2, p0);

            int sign_pt1 = fp.getsign_dot_distancetoplane(dist_pt1);
            int sign_pt2 = fp.getsign_dot_distancetoplane(dist_pt2);

            if (sign_pt1 == sign_pt2)
            {
                return false;
            }

            if (sign_pt1 == 0)
            {
                return false;
            }
            else if (sign_pt2 == 0)
            {
                return false;
            }
            else
            {
                double u = xyz.dotsub(n, p0, pt1) / xyz.dotsub(n, pt2, pt1);
                xyz pt_to_check = (pt2 - pt1).multiply_in_place(u).add_in_place(pt1);
                PointFaceIntersection pfi = this.CalcPFI(pt_to_check);
                return pfi == PointFaceIntersection.Inside;
            }
        }

        internal bool CalcSegmentFaceIntersection_SamePlane(seg3d s, out PointFaceIntersection pfi_a, out PointFaceIntersection pfi_b)
        {
            xyz n = myPlane.n;
            xyz p0 = (myPlane.pts[0]);

            // get the signed distance from each point to the plane of the poly
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the poly.

            double dist_pt1 = xyz.dotsub(n, s.a, p0);
            double dist_pt2 = xyz.dotsub(n, s.b, p0);

            if (fp.eq_dot_distancetoplane(dist_pt1, 0) && fp.eq_dot_distancetoplane(dist_pt2, 0))
            {
                // the segment is in the same plane as the poly
                pfi_a = this.CalcPFI(s.a);
                pfi_b = this.CalcPFI(s.b);

                return true;
            }

            pfi_a = PointFaceIntersection.None;
            pfi_b = PointFaceIntersection.None;
            return false;
        }

        internal xyz CalcSegmentFaceIntersection_HitOnly(seg3d s)
        {
            xyz pt1 = s.a;
            xyz pt2 = s.b;

            xyz n = myPlane.n;
            xyz p0 = myPlane.pts[0];

            // get the signed distance from each point to the plane of the poly
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the poly.

            double dist_pt1 = xyz.dotsub(n, pt1, p0);
            double dist_pt2 = xyz.dotsub(n, pt2, p0);

            if (fp.eq_dot_distancetoplane(dist_pt1, 0) || fp.eq_dot_distancetoplane(dist_pt2, 0))
            {
                return null;
            }

            if (
                (dist_pt1 > 0)
                && (dist_pt2 > 0)
                )
            {
                return null;
            }

            if (
                (dist_pt1 < 0)
                && (dist_pt2 < 0)
                )
            {
                return null;
            }


            double u = xyz.dotsub(n, p0, pt1) / xyz.dotsub(n, pt2, pt1);
            //oldcode: xyz pt_to_check = pt1 + u * (pt2 - pt1);
            xyz pt_to_check = (pt2 - pt1).multiply_in_place(u).add_in_place(pt1);
            PointFaceIntersection pfi = this.CalcPFI(pt_to_check);
            if (pfi != PointFaceIntersection.None)
            {
                return pt_to_check;
            }
            else
            {
                return null;
            }
        }

        internal void Clone(Solid ns, Dictionary<xyz, int> vertassoc)
        {
            int[] main = this.MainLoop.GetVertexIndices_xyz(vertassoc);
            Face nf = ns.CreateFace(this.name, main);
            nf.OriginalName = this.OriginalName;
            nf.Shade = this.Shade;
            if (this.HasHoles())
            {
                foreach (EdgeLoop el in this.Holes)
                {
                    int[] vertindices = el.GetVertexIndices_xyz(vertassoc);
                    nf.AddHole(vertindices);
                }
            }
            nf.RecalcPlane();
        }

        internal Face(string sname, Solid s, int[] verts)
        {
            Debug.Assert(verts.Length >= 3);
            Debug.Assert(sname != null);
            Debug.Assert(sname.Length > 0);

            name = sname;
            OriginalName = name;
            solid = s;
            loops = new List<EdgeLoop>();
            EdgeLoop el = new EdgeLoop();
            el.CreateHalfEdges(s, this, verts);
            loops.Add(el);

            DoGeomChecks();
        }

        internal EdgeLoop GetEdgeLoopFor(HalfEdge he)
        {
            Debug.Assert(he != null);
            EdgeLoop result = null;
            foreach (EdgeLoop el in loops)
            {
                for (int i = 0; i < el.Count; i++)
                {
                    if (el[i] == he)
                    {
                        result = el;
                        break;
                    }
                }
            }
            Debug.Assert(result != null);
            return result;
        }

        internal HalfEdge GetNextHalfEdge(HalfEdge he)
        {
            Debug.Assert(he != null);
            HalfEdge result = null;
            foreach (EdgeLoop el in loops)
            {
                for (int i = 0; i < el.Count; i++)
                {
                    if (el[i] == he)
                    {
                        result = el[i + 1];
                        break;
                    }
                }
            }
            Debug.Assert(result != null);
            return result;
        }

        public xyz GetCenter()
        {
            // TODO this might not be the right way to find the center

            xyz c = new xyz(0, 0, 0);
            List<xyz> a = this.MainLoop.CollectAllVertices();
            foreach (xyz p in a)
            {
                c += p;
            }
            c = c / a.Count;
            return c;
        }

        internal bool IsCoPlanarWith(Face f2)
        {
            xyz n1 = this.UnitNormal();
            xyz n2 = f2.UnitNormal();
            if (
                (fp.eq_unitvec(n1, n2))
                || (fp.eq_unitvec(n1, -n2))
                )
            {
                xyz p1 = MainLoop[0].to;
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
                    return true;
                }
            }
            return false;
        }

        internal bool IntersectsWith_SamePlane(Face other)
        {
            // TODO perf this is silly to calc the area and then throw it away
            double area;
            return IntersectsWith_SamePlane(other, out area);
        }

        internal bool IntersectsWith_SamePlane(Face other, out double area)
        {
            // TODO this code now handles holes.  should it?

            List<List<xy>> loops2 = myPlane.Convert3dLoopsTo2d(other.GetLoopsIn3d());
            if (fp.eq_unitvec(this.UnitNormal(), -(other.UnitNormal())))
            {
                foreach (List<xy> loop in loops2)
                {
                    loop.Reverse();
                }
            }

            ppi2d pi = new ppi2d(myPlane.loops2d, loops2);
            List<seg2d> segs = pi.GetIntersection();

            if ((segs != null) && (segs.Count > 0))
            {
                List<List<xy>> inter = ppi2d.FindTheLoops(segs);
                area = ut.PolygonArea2d(inter);
                return true;
            }
            else
            {
                area = 0;
                return false;
            }
        }

        internal bool IntersectsWith(Face other)
        {
            List<List<xyz>> myLoops = this.GetLoopsIn3d();
            List<List<xyz>> otherLoops = other.GetLoopsIn3d();
            return ppi3d.TestIntersection(myLoops, otherLoops);
        }

        internal HalfEdge FindEdge(string otherFaceName)
        {
            foreach (EdgeLoop el in loops)
            {
                foreach (HalfEdge he in el)
                {
                    if (he.Opposite().face.name == otherFaceName)
                    {
                        return he;
                    }
                }
            }

            return null;
        }

        internal bool SharesAnEdgeWith(Face other)
        {
            for (int iloop = 0; iloop < loops.Count; iloop++)
            {
                EdgeLoop curloop = loops[iloop];
                for (int i = 0; i < curloop.Count; i++)
                {
                    HalfEdge he = curloop[i];
                    if (he.Opposite().face == other)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal int SharesVerticesWith(Face other)
        {
            int count = 0;
            List<xyz> myVerts = this.CollectAllVertices();
            List<xyz> otherVerts = other.CollectAllVertices();

            foreach (xyz myV in myVerts)
            {
                if (otherVerts.Contains(myV))
                {
                    count++;
                }
            }
            return count;
        }

        internal bool SharesAVertexWith(Face other)
        {
            return 0 != SharesVerticesWith(other);
        }

#if DEBUG
        private static void MakeSureLoopsDontTouch(List<List<xy>> loops)
        {
            for (int icurloop = 0; icurloop < loops.Count; icurloop++)
            {
                List<xy> curloop = loops[icurloop];
                for (int iloop2 = 0; iloop2 < loops.Count; iloop2++)
                {
                    List<xy> loop2 = loops[iloop2];
                    if (icurloop != iloop2)
                    {
                        Debug.Assert(!ut.AnySegmentsTouch(curloop, loop2), "Holes cannot touch each other or the main face loop.");
                        if (icurloop != 0)
                        {
                            Debug.Assert(!ut.PointInsidePoly(curloop, loop2[0]), "A hole cannot be inside another hole.");
                        }
                    }
                }
            }
        }
#endif

        internal void DoGeomChecks()
        {
            Debug.Assert(this.name != null);
            Debug.Assert(this.name.Length > 0);
            Debug.Assert(ut.AllVerticesPlanar(CollectAllVertices()));

            for (int i = 0; i < loops.Count; i++)
            {
                EdgeLoop cur = loops[i];

                cur.DoGeomChecks();
            }

#if DEBUG
            if (HasHoles())
            {
                List<List<xy>> loops2d = this.GetLoopsIn2d();
                MakeSureLoopsDontTouch(loops2d);
                List<xy> main2d = loops2d[0];

                for (int i = 1; i < loops.Count; i++)
                {
                    List<xy> h = loops2d[i];
                    xy pt = h[0];
                    Debug.Assert(ut.PointInsidePoly(main2d, pt), "Hole must be inside face");
                }
            }
#endif
        }

        internal bool HasHoles()
        {
            return loops.Count > 1;
        }

        internal EdgeLoop MainLoop
        {
            get
            {
                return loops[0];
            }
        }

        internal List<EdgeLoop> Holes
        {
            get
            {
                if (loops.Count <= 1)
                {
                    return null;
                }
                return loops.GetRange(1, loops.Count - 1);
            }
        }

        // TODO here again, it would be nice to not need this function.
        internal List<List<xyz>> GetLoopsIn3d()
        {
            List<List<xyz>> result = new List<List<xyz>>();

            result.Add(MainLoop.CollectAllVertices());

            if (HasHoles())
            {
                foreach (EdgeLoop cur in Holes)
                {
                    result.Add(cur.CollectAllVertices());
                }
            }

            return result;
        }

        internal List<seg3d> CollectAllSegments()
        {
            List<seg3d> result = new List<seg3d>();
            foreach (EdgeLoop el in loops)
            {
                el.CollectAllSegments(result);
            }
            return result;
        }

        internal List<List<xy>> GetLoopsIn2d()
        {
            return myPlane.loops2d;
        }

        internal List<Triangle3d> GetTriangles()
        {
            List<Triangle3d> a = new List<Triangle3d>();

            this.GetTriangles(a);

            return a;
        }

        internal void GetTriangles(List<Triangle3d> tris)
        {
            List<Triangle3d> mytris = new List<Triangle3d>();

            tri.Triangulate3d_WithHoles(mytris, this.GetLoopsIn3d());

            foreach (Triangle3d t in mytris)
            {
                t.face = this;
            }

            tris.AddRange(mytris);
        }

        internal List<xyz> CollectAllVertices()
        {
            List<xyz> a = new List<xyz>();
            CollectAllVertices(a);
            return a;
        }

        internal void CollectAllVertices(List<xyz> a)
        {
            foreach (EdgeLoop cur in loops)
            {
                cur.CollectAllVertices(a);
            }
        }

        internal void AddHole(params int[] verts)
        {
            EdgeLoop h = new EdgeLoop();
            h.CreateHalfEdges(solid, this, verts);
            loops.Add(h);

            //DoGeomChecks();
        }

        public xyz UnitNormal()
        {
            return myPlane.n;
        }

        internal Plane myPlane = null;

        internal void RecalcPlane()
        {
            myPlane = CalcPlane();
        }

        internal Plane CalcPlane()
        {
            Plane p = new Plane();
            p.n = ut.GetUnitNormalFromPointList(MainLoop.Vertices);
            p.pts = MainLoop.CollectAllVertices();
            p.i = ((p.pts[1]) - (p.pts[0])).normalize_in_place();
            p.j = xyz.cross(p.n, p.i).normalize_in_place();
            p.loops2d = new List<List<xy>>();
            p.loops2d.Add(ut.Convert3dPointsTo2d(p.pts, p.pts[0], p.i, p.j));
            if (HasHoles())
            {
                foreach (EdgeLoop elh in Holes)
                {
                    p.loops2d.Add(ut.Convert3dPointsTo2d(elh.CollectAllVertices(), p.pts[0], p.i, p.j));
                }
            }
            return p;
        }

        internal double Area()
        {
            // TODO fix this to use EdgeLoop.Area() ?

            xyz normal = UnitNormal();

            xyz run = new xyz(0, 0, 0);
            foreach (EdgeLoop curloop in loops)
            {
                for (int i = 0; i < curloop.Count; i++)
                {
                    run += xyz.cross(curloop[i].to, curloop[i + 1].to);
                }
            }
            return xyz.dot(normal, run) / 2;
        }

        internal double FaceVolume()
        {
            // Note that we're using 'point 0' as the "origin".
            // It doesn't seem to matter much what we use.
            double d = 0.0;

            foreach (EdgeLoop curloop in loops)
            {
                for (int i = 0; i < curloop.Count; i++)
                {
                    xyz c = xyz.cross(curloop[0].to, curloop[i].to);
                    d += xyz.dot(curloop[i + 1].to, c);
                }
            }
            return d;
        }

#if false // used in the bool3d code, but only from a routine which is currently #if-ed out
        internal bool SegmentTouches(seg3d s)
        {
            xyz n = myPlane.n;
            xyz p0 = (myPlane.pts[0]);

            // get the signed distance from each point to the plane of the poly
            // if the signs are the same, then the two points lie on the same
            // side of the plane, which means they don't intersect the plane,
            // which means they can't intersect the poly.

            double dist_pt1 = xyz.dotsub(n, s.a, p0);
            double dist_pt2 = xyz.dotsub(n, s.b, p0);

            int sign_pt1 = fp.getsign_dot_distancetoplane(dist_pt1);
            int sign_pt2 = fp.getsign_dot_distancetoplane(dist_pt2);

            if (
                (sign_pt1 > 0)
                && (sign_pt2 > 0)
                )
            {
                return false;
            }

            if (
                (sign_pt1 < 0)
                && (sign_pt2 < 0)
                )
            {
                return false;
            }

            if (
                (sign_pt1 == 0)
                && (sign_pt2 == 0)
                )
            {
                // the segment is in the same plane as the poly

                // convert everything to 2d and check for intersections
                xy pt1_2d = myPlane.ConvertPointTo2d(s.a);
                xy pt2_2d = myPlane.ConvertPointTo2d(s.b);

                foreach (List<xy> lp in myPlane.loops2d)
                {
                    if (ut.TouchesAnySegment(lp, pt1_2d, pt2_2d))
                    {
                        return true;
                    }
                }

                return CalcPFI(s.a) != PointFaceIntersection.None;
            }

            if (sign_pt1 == 0)
            {
                // pt1 is in the plane
                return CalcPFI(s.a) != PointFaceIntersection.None;
            }
            else if (sign_pt2 == 0)
            {
                return CalcPFI(s.b) != PointFaceIntersection.None;
            }
            else
            {
                double u = xyz.dotsub(n, p0, s.a) / xyz.dotsub(n, s.b, s.a);
                xyz pt_to_check = s.a + u * (s.b - s.a);
                PointFaceIntersection pfi = this.CalcPFI(pt_to_check);
                return (pfi != PointFaceIntersection.None);
            }
        }
#endif
    }

    internal class Edge
    {
        public xyz a;
        public xyz b;
        public HalfEdge a2b;
        public HalfEdge b2a;

        public Edge(xyz v1, xyz v2)
        {
            a = v1;
            b = v2;
        }

        public double Length()
        {
            return (b - a).magnitude();
        }

        public xyz GetOtherPoint(xyz p)
        {
            if (p == a)
            {
                return b;
            }
            else
            {
                Debug.Assert(p == b);
                return a;
            }
        }

        public HalfEdge HalfEdgeForFace(Face f)
        {
            if (a2b.face == f)
            {
                return a2b;
            }
            else if (b2a.face == f)
            {
                return b2a;
            }
            else
            {
                return null;
            }
        }

        public void DoValidityAssertions()
        {
            Debug.Assert(a != null);
            Debug.Assert(b != null);
            Debug.Assert(a != b);
            Debug.Assert(a2b != null);
            Debug.Assert(b2a != null);
            Debug.Assert(a2b != b2a);
            Debug.Assert(a2b.face != b2a.face);
            Debug.Assert(a2b.edge == this);
            Debug.Assert(b2a.edge == this);
        }

        public HalfEdge GetOpposite(HalfEdge q)
        {
            Debug.Assert((a2b == q) || (b2a == q));
            if (a2b == q)
            {
                return b2a;
            }
            else
            {
                return a2b;
            }
        }

        public bool MatchVertexPair(xyz v1, xyz v2)
        {
            if (
                ((v1 == a) && (v2 == b))
                || ((v1 == b) && (v2 == a))
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public HalfEdge CreateHalfEdge(Face f, xyz from, xyz to)
        {
            Debug.Assert(((from == a) && (to == b)) || ((from == b) && (to == a)));

            if (
                (from == a) && (to == b)
                )
            {
                if (a2b != null)
                {
                    throw new NonManifoldSolidException();
                }

                a2b = new HalfEdge(this, f, to);
                return a2b;
            }
            else
            {
                if (b2a != null)
                {
                    throw new NonManifoldSolidException();
                }

                b2a = new HalfEdge(this, f, to);
                return b2a;
            }
        }
    }

    public class HalfEdge
    {
        internal Edge edge;
        internal xyz to;
        public Face face;

        internal HalfEdge(Edge e, Face f, xyz _p_to)
        {
            edge = e;
            to = _p_to;
            face = f;
        }

        public xyz from
        {
            get
            {
                return edge.GetOtherPoint(to);
            }
        }

        public xyz GetInwardNormal()
        {
            return xyz.cross(face.UnitNormal(), UnitVector()).normalize_in_place();
        }

        public xyz UnitVector()
        {
            return (to - Opposite().to).normalize_in_place();
        }

        public xyz Center()
        {
            return (to + Opposite().to).divide_in_place(2);
        }

        public double Length()
        {
            return edge.Length();
        }

        public xyz Vector()
        {
            return to - Opposite().to;
        }

        public HalfEdge Opposite()
        {
            return edge.GetOpposite(this);
        }
    }

    public enum GrainGrain
    {
        Long_Long,
        End_Long,
        End_End,
        Cross,
        End_Ugly,
        Ugly_Ugly,
        Ugly_Long,
        Unknown
    }

    public class GlueJoint
    {
        public Face f1;
        public Face f2;
        public double Area;

        public GlueJoint(Face _f1, Face _f2, double a)
        {
            f1 = _f1;
            f2 = _f2;
            Area = a;
        }

        private static bool match_grains(FaceQuality q1, FaceQuality q2, FaceQuality m1, FaceQuality m2)
        {
            return (
                ((q1 == m1) && (q2 == m2))
                || ((q1 == m2) && (q2 == m1))
            );
        }

        public GrainGrain Grains
        {
            get
            {
                FaceQuality q1 = f1.GetQuality();
                FaceQuality q2 = f2.GetQuality();

                if (match_grains(q1, q2, FaceQuality.EndGrain, FaceQuality.EndGrain))
                {
                    return GrainGrain.End_End;
                }
                if (match_grains(q1, q2, FaceQuality.EndGrain, FaceQuality.Good))
                {
                    return GrainGrain.End_Long;
                }
                if (match_grains(q1, q2, FaceQuality.EndGrain, FaceQuality.Ugly))
                {
                    return GrainGrain.End_Ugly;
                }
                if (match_grains(q1, q2, FaceQuality.Ugly, FaceQuality.Ugly))
                {
                    return GrainGrain.Ugly_Ugly;
                }
                if (match_grains(q1, q2, FaceQuality.Ugly, FaceQuality.Good))
                {
                    return GrainGrain.Ugly_Long;
                }
                if (match_grains(q1, q2, FaceQuality.Good, FaceQuality.Good))
                {
                    xyz v1 = f1.solid.board_v;
                    xyz v2 = f2.solid.board_v;

                    double rad = ut.GetAngleBetweenTwoNormalizedVectorsInRadians(v1.normalize(), v2.normalize());
                    while (rad < 0)
                    {
                        rad += Math.PI;
                    }
                    while (rad > Math.PI)
                    {
                        rad -= Math.PI;
                    }

                    double deg = ut.RadianToDegree(rad);
                    // anything within 10 degrees of 90 is considered crossgrain
                    if (fp.eq_tol(deg, 90, 10))
                    {
                        return GrainGrain.Cross;
                    }
                    else
                    {
                        return GrainGrain.Long_Long;
                    }
                }

                // TODO should never happen
                return GrainGrain.Unknown;
            }
        }
    }

    internal class CompareSolidsByName : IComparer<Solid>
    {
        public int Compare(Solid a, Solid b)
        {
            return string.Compare(a.name, b.name);
        }
    }

    public class CompoundSolid : IOrient
    {
        public List<Solid> Subs = new List<Solid>();
        internal bool HasSubOverlaps = false;

        internal CompoundSolid()
        {
        }

        internal bool IsValidWithNoSubOverlaps()
        {
            return !HasSubOverlaps;
        }

        public Solid FindSub(string s)
        {
            foreach (Solid sol in Subs)
            {
                if (sol.name == s)
                {
                    return sol;
                }
            }
            return null;
        }

        internal BunchOfTriBags CreateBags()
        {
            BunchOfTriBags bunch = new BunchOfTriBags();
            bunch.notmoving = new List<TriBag>();

            Subs.ForEach(delegate (Solid s)
            {
                bunch.notmoving.Add(new TriBag(s));
            });

            return bunch;
        }

#if not // this just doesn't work well
        public List<TriBag> CreateExplodedViewAnimation(bool bRightHanded)
        {
            List<TriBag> bags = new List<TriBag>();
            if (Subs.Count == 1)
            {
                bags.Add(new TriBag(null, Subs[0].GetTriangles(bRightHanded), Subs[0].GetLines(bRightHanded)));
                return bags;
            }

            multicore.Map_Void(Subs, delegate(Solid s)
            {
                List<xyz> vectors = new List<xyz>();
                foreach (Face f1 in s.Faces)
                {
                    xyz n1 = f1.UnitNormal();
                    foreach (Solid s2 in Subs)
                    {
                        if (s == s2)
                        {
                            continue;
                        }

                        foreach (Face f2 in s2.Faces)
                        {
                            xyz n2 = f2.UnitNormal();

                            if (fp.eq_unitvec(n1, -n2))
                            {
                                if (f1.IsCoPlanarWith(f2))
                                {
                                    if (f1.IntersectsWith_SamePlane(f2))
                                    {
                                        vectors.Add(n1);
                                    }
                                }
                            }
                        }
                    }
                }

                xyz vec = null;
                if (vectors.Count == 1)
                {
                    vec = vectors[0];
                }
                else
                {
                    List<xyz> choices = new List<xyz>();
                    choices.Add(s.board_u.normalize());
                    choices.Add(s.board_v.normalize());
                    choices.Add(s.board_w.normalize());
                    choices.Add(-(s.board_u.normalize()));
                    choices.Add(-(s.board_v.normalize()));
                    choices.Add(-(s.board_w.normalize()));
                    vec = ut.SelectExplodeVector(choices, vectors);
                    if (vec == null)
                    {
                        vec = ut.CalculateExplodeVector(vectors);
                    }
                }

                Debug.Assert(vec != null);
                if (fp.eq_inches(vec.magnitude_squared(), 0))
                {
                    vec = null;
                }
                else
                {
                    vec = -(vec.normalize());
                    if (bRightHanded)
                    {
                        vec.SwapXY();
                    }
                }
                bags.Add(new TriBag(vec, s.GetTriangles(bRightHanded), s.GetLines(bRightHanded)));
            });

            return bags;
        }
#endif

#if not // no longer used
        public void Write_XAML(TextWriter tw)
        {
            List<Triangle3d> tris = this.GetTriangles(true);

            tw.WriteLine("<ModelVisual3D>");
            tw.WriteLine("<ModelVisual3D.Content>");
            tw.WriteLine("<Model3DGroup>");
            tw.WriteLine("<GeometryModel3D>");
            tw.WriteLine("<GeometryModel3D.Geometry>");
            tw.WriteLine("<MeshGeometry3D>");

            StringBuilder sb_positions = new StringBuilder();
            StringBuilder sb_indices = new StringBuilder();
            int ndx = 0;
            foreach (Triangle3d t in tris)
            {
                // note writing backwards.  problem?
                sb_positions.AppendFormat("{0},{1},{2}  {3},{4},{5}  {6},{7},{8}  ",
                    t.a.x, t.a.y, t.a.z,
                    t.b.x, t.b.y, t.b.z,
                    t.c.x, t.c.y, t.c.z
                );
                sb_indices.AppendFormat("{0},{1},{2}  {3},{4},{5}  {6},{7},{8}  ",
                    ndx++, ndx++, ndx++,
                    ndx++, ndx++, ndx++,
                    ndx++, ndx++, ndx++
                );
            }

            tw.WriteLine("<MeshGeometry3D.Positions>{0}</MeshGeometry3D.Positions>", sb_positions.ToString());
            tw.WriteLine("<MeshGeometry3D.TriangleIndices>{0}</MeshGeometry3D.TriangleIndices>", sb_indices.ToString());
            tw.WriteLine("</MeshGeometry3D>");
            tw.WriteLine("</GeometryModel3D.Geometry>");
            tw.WriteLine("<GeometryModel3D.Material>");
            tw.WriteLine("<DiffuseMaterial>");
            tw.WriteLine("<DiffuseMaterial.Brush>");
            tw.WriteLine("<SolidColorBrush Color=\"Tan\"/>"); // TODO fix this
            tw.WriteLine("</DiffuseMaterial.Brush>");
            tw.WriteLine("</DiffuseMaterial>");
            tw.WriteLine("</GeometryModel3D.Material>");
            tw.WriteLine("</GeometryModel3D>");
            tw.WriteLine("</Model3DGroup>");
            tw.WriteLine("</ModelVisual3D.Content>");
            tw.WriteLine("</ModelVisual3D>");
        }
#endif

#if DEBUG
        internal void AssertNoNameClashes()
        {
            for (int i = 0; i < Subs.Count; i++)
            {
                Subs[i].AssertNoNameClashes();

                for (int j = i + 1; j < Subs.Count; j++)
                {
                    Debug.Assert(Subs[i].name != Subs[j].name);
                }
            }
        }
#endif

        internal List<GlueJoint> FindGlueJoints()
        {
            List<GlueJoint> joints = new List<GlueJoint>();

            for (int i1 = 0; i1 < Subs.Count; i1++)
            {
                Solid s1 = Subs[i1];
                foreach (Face f in s1.Faces)
                {
                    xyz n1 = f.UnitNormal();
                    for (int i2 = i1 + 1; i2 < Subs.Count; i2++)
                    {
                        Solid s2 = Subs[i2];

                        foreach (Face f2 in s2.Faces)
                        {
                            xyz n2 = f2.UnitNormal();

                            if (fp.eq_unitvec(n1, -n2))
                            {
                                if (f.IsCoPlanarWith(f2))
                                {
                                    double area;

                                    if (f.IntersectsWith_SamePlane(f2, out area))
                                    {
                                        joints.Add(new GlueJoint(f, f2, area));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return joints;
        }

        internal void AddSub(Solid other)
        {
            AddSub(other, true);
        }

        internal void AddSub(Solid other, bool bCheck)
        {
            if (bCheck && !HasSubOverlaps)
            {
                foreach (Solid s in Subs)
                {
                    if (bool3d.CheckIfTwoSolidsShareAnySpace(s, other))
                    {
                        // Console.WriteLine("Offending solids:  {0} and {1}", s.name, other.name);
                        // TODO I suppose we could record the two offending solids for later use?
                        this.HasSubOverlaps = true;
                        break;
                    }
                }
            }

            Subs.Add(other);
        }

        internal Solid this[int i]
        {
            get
            {
                return (Solid)Subs[i];
            }
        }

        public IEnumerator<Solid> GetEnumerator()
        {
            return Subs.GetEnumerator();
        }

        internal void DoGeomChecks()
        {
            foreach (Solid s in Subs)
            {
                s.DoGeomChecks();
            }
        }

        internal void AddSub(CompoundSolid others)
        {
            foreach (Solid s in others.Subs)
            {
                this.AddSub(s);
            }
        }

        internal CompoundSolid ShallowClone()
        {
            CompoundSolid cs = new CompoundSolid();

            foreach (Solid s in Subs)
            {
                cs.AddSub(s, false);
            }

            cs.HasSubOverlaps = this.HasSubOverlaps;

            return cs;
        }

        internal CompoundSolid Clone()
        {
            CompoundSolid cs = new CompoundSolid();

            foreach (Solid s in Subs)
            {
                cs.AddSub(s.Clone(), false);
            }

            cs.HasSubOverlaps = this.HasSubOverlaps;

            return cs;
        }

        internal Face FindFace(string sol, string face)
        {
            foreach (Solid s in Subs)
            {
                if (s.name == sol)
                {
                    Face f = s.FindFace(face);
                    if (f != null)
                    {
                        return f;
                    }
                }
            }
            return null;
        }

        internal Face FindFace(string name)
        {
            string sol;
            string face;
            string edge;
            ut.ParsePath(name, out sol, out face, out edge);
            return FindFace(sol, face);
        }

        public List<Line3d> GetLines()
        {
            List<Line3d> a = new List<Line3d>();
            GetLines(a);
            return a;
        }

        public void GetLines(List<Line3d> a)
        {
            foreach (Solid s in Subs)
            {
                s.GetLines(a);
            }
        }

        public void GetTriangles(List<Triangle3d> a)
        {
            foreach (Solid s in Subs)
            {
                s.GetTriangles(a);
            }
        }

        public List<Triangle3d> GetTriangles()
        {
            List<Triangle3d> a = new List<Triangle3d>();
            this.GetTriangles(a);
            return a;
        }

        public void Rotate(double costheta, double sintheta, xyz p1, xyz u)
        {
            if (fp.eq_radians(1, costheta))
            {
                return;
            }

            foreach (Solid s in Subs)
            {
                s.Rotate(costheta, sintheta, p1, u);
            }
        }

        public void Translate(double x, double y, double z)
        {
            if (
                fp.eq_inches(0, x)
                && fp.eq_inches(0, y)
                && fp.eq_inches(0, z)
                )
            {
                return;
            }

            foreach (Solid s in Subs)
            {
                s.Translate(x, y, z);
            }
        }

        internal void SumAllVerts(out xyz sum, out int count)
        {
            xyz r = new xyz(0, 0, 0);
            int c = 0;

            foreach (Solid s in Subs)
            {
                xyz q;
                int c2;

                s.SumAllVerts(out q, out c2);

                r.add_in_place(q);
                c += c2;
            }

            sum = r;
            count = c;
        }

        public xyz GetCenter()
        {
            return this.GetBoundingBox().center;
        }

        public BoundingBox3d GetBoundingBox()
        {
            BoundingBox3d bb = new BoundingBox3d();

            foreach (Solid s in Subs)
            {
                bb.Add(s.GetBoundingBox());
            }

            return bb;
        }

        public double Weight()
        {
            double w = 0.0;

            foreach (Solid s in Subs)
            {
                w += s.Weight();
            }

            return w;
        }

        public double Volume()
        {
            double volume = 0.0;

            foreach (Solid s in Subs)
            {
                volume += s.Volume();
            }

            return volume;
        }

        public double GlueArea(List<GlueJoint> joints)
        {
            double area = 0;
            foreach (GlueJoint gj in joints)
            {
                area += gj.Area;
            }
            return area;
        }

        public double SurfaceArea()
        {
            double area = 0;
            foreach (Solid s in Subs)
            {
                area += s.SurfaceArea();
            }
            return area;
        }

        public double ActualSurfaceArea()
        {
            double area = 0;
            foreach (Solid s in Subs)
            {
                area += s.SurfaceArea();
            }
            area -= (2 * GlueArea(FindGlueJoints()));
            return area;
        }

        internal void Lookup(string path, out Solid sol, out Face f, out HalfEdge he)
        {
            CompoundSolid cs = this;
            sol = null;
            f = null;
            he = null;

            string[] parts = path.Split('.');
            string s_sol = null;
            string s_face = null;
            string s_edge = null;

            s_sol = parts[0];
            if (parts.Length > 1)
            {
                s_face = parts[1];
                if (parts.Length > 2)
                {
                    s_edge = parts[2];
                }
            }

            sol = this.FindSub(s_sol);
            if (sol == null)
            {
                throw new Exception(string.Format("Piece {0} could not be found", s_sol));
            }

            if (s_face != null)
            {
                f = sol.FindFace(s_face);
                if (f == null)
                {
                    throw new Exception(string.Format("Face {0} could not be found inside piece {1}", s_face, s_sol));
                }

                if (s_edge != null)
                {
                    he = f.FindEdge(s_edge);
                    if (he == null)
                    {
                        throw new Exception(string.Format("Edge {0} could not be found adjacent to face {1} inside piece {2}", s_edge, s_face, s_sol));
                    }
                }
            }
        }

#if ROUND
        internal void RoundAllVertices(int prec)
        {
            foreach (Solid s in Subs)
            {
                s.RoundAllVertices(prec);
            }
        }

        internal void RoundAllVertices()
        {
            foreach (Solid s in Subs)
            {
                s.RoundAllVertices();
            }
        }
#endif

        internal HalfEdge FindEdge(string s, string f, string e)
        {
            Solid sol = FindSub(s);
            if (sol == null)
            {
                return null;
            }
            Face face = sol.FindFace(f);
            if (face == null)
            {
                return null;
            }
            return face.FindEdge(e);
        }

        internal void FindPath(string path, out Solid s, out Face f, out HalfEdge he)
        {
            string s_s;
            string s_f;
            string s_he;

            ut.ParsePath(path, out s_s, out s_f, out s_he);

            s = FindSub(s_s);
            if (s == null)
            {
                f = null;
                he = null;
                return;
            }
            f = s.FindFace(s_f);
            if (f == null)
            {
                he = null;
                return;
            }
            he = f.FindEdge(s_he);
        }
    }

    public class Solid : IOrient
    {
        public string name;

        internal List<xyz> Vertices = new List<xyz>();
        internal List<Edge> Edges = new List<Edge>();
        internal List<Face> Faces = new List<Face>();

        // The following members are for support for 3d texture coordinates
        public xyz board_origin;
        public xyz board_u;
        public xyz board_v;
        public xyz board_w;

        public BoardMaterial material;

        internal Solid(string s, BoardMaterial bm)
        {
            Debug.Assert(s != null);
            Debug.Assert(s.Length > 0);
            Debug.Assert(bm != null);

            name = s;
            material = bm;
        }

        internal CompoundSolid ToCompoundSolid()
        {
            CompoundSolid cs = new CompoundSolid();
            cs.AddSub(this);
            return cs;
        }

        internal Solid Clone()
        {
            Solid ns = new Solid(this.name, this.material);

            if (this.board_origin != null)
            {
                ns.board_origin = this.board_origin.copy();
                ns.board_u = this.board_u.copy();
                ns.board_v = this.board_v.copy();
                ns.board_w = this.board_w.copy();
            }

            Dictionary<xyz, int> vertassoc = new Dictionary<xyz, int>();

            // first the verts
            foreach (xyz v in this.Vertices)
            {
                int ndx = ns.AddVertex(v.x, v.y, v.z);
                vertassoc[v] = ndx;
            }

            foreach (Face f in this.Faces)
            {
                f.Clone(ns, vertassoc);
            }

            return ns;
        }


        internal static void Sweep(Solid s, List<xyz> top, xyz vec, string prefix)
        {
            List<xyz> otherEnd = new List<xyz>();
            foreach (xyz p in top)
            {
                otherEnd.Add(p.copy());
            }
            ut.TranslatePoints(otherEnd, vec.x, vec.y, vec.z);

            s.CreateFace(prefix + "top", s.VerifyAllPointsExist(top));
            s.CreateFaceCW(prefix + "bottom", s.VerifyAllPointsExist(otherEnd));

            // TODO disallow sweeping in the same direction as the face normal?

            for (int i = 0; i < top.Count; i++)
            {
                List<xyz> pts = new List<xyz>();
                pts.Add(top[i]);
                pts.Add(otherEnd[i]);
                pts.Add(otherEnd[(i + 1) % top.Count]);
                pts.Add(top[(i + 1) % top.Count]);
                int[] indices = s.VerifyAllPointsExist(pts);
                s.CreateFace(prefix + (i + 1).ToString(), indices);
            }
        }

        internal static Solid Sweep(string name, BoardMaterial bm, List<xyz> top, xyz vec)
        {
            Solid s = new Solid(name, bm);
            Sweep(s, top, vec, "");
            s.RecalcFacePlanes();
            return s;
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
        private static seg3d find_loop_starting_point(List<seg3d> segs)
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

        private static bool need_to_find_loop_starting_point(List<seg3d> segs)
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

        internal static List<List<xyz>> FindTheLoops(List<seg3d> segs, bool reverse)
        {
            bool bTricky = need_to_find_loop_starting_point(segs);
            List<List<xyz>> loops = new List<List<xyz>>();
            while (segs.Count > 0)
            {
                List<xyz> pts = new List<xyz>();
                seg3d cur;
                if (bTricky)
                {
                    cur = find_loop_starting_point(segs);
                }
                else
                {
                    cur = segs[0];
                }
                pts.Add(cur.a);
                pts.Add(cur.b);
                segs.Remove(cur);
                xyz p0 = cur.a;
                xyz plast = cur.b;
                while (true)
                {
                    xyz next;
                    cur = seg3d.find_seg_a(segs, plast, out next);
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
                if (reverse)
                {
                    pts.Reverse();
                }
                loops.Add(pts);

#if false
                // TODO the following is an ugly hack
                if (segs.Count < 3)
                {
                    break;
                }
#endif
            }
            return loops;
        }

        internal void CreateFacesFromPileOfSegments(string fname, string OriginalName, bool Shade, List<seg3d> segs, bool reverse)
        {
            List<List<xyz>> loops = FindTheLoops(segs, reverse);
            CreateFacesFromPileOfLoops(fname, Shade, OriginalName, loops);
        }

        internal class MainLoopSorter : IComparer<List<xyz>>
        {
            public int Compare(List<xyz> ax, List<xyz> ay)
            {
                BoundingBox3d bbx = BoundingBox3d.FromArrayOfPoints(ax);
                BoundingBox3d bby = BoundingBox3d.FromArrayOfPoints(ay);

                xyz cx = bbx.center;
                xyz cy = bby.center;

                int which = -1;
                double diff = double.MinValue;

                for (int i = 0; i < 3; i++)
                {
                    double d = Math.Abs(cx[i] - cy[i]);
                    if (d > diff)
                    {
                        which = i;
                        diff = d;
                    }
                }

                if (cx[which] < cy[which])
                {
                    return -1;
                }
                else if (cx[which] > cy[which])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal List<Face> CreateFaces(string fname, string OriginalName, List<List<xyz>> mains)
        {
            List<Face> result = new List<Face>();
            mains.Sort(new MainLoopSorter());

            string prefix = null;
            if (mains.Count != 1)
            {
                prefix = fname + "_";
            }
            for (int i = 0; i < mains.Count; i++)
            {
                List<xyz> loop = mains[i];

                string thisname;
                if (mains.Count == 1)
                {
                    thisname = fname;
                }
                else
                {
                    thisname = prefix + (i + 1).ToString();
                }
                Face f = this.CreateFaceFromPoints(thisname, loop);
                f.OriginalName = OriginalName;
                result.Add(f);
            }
            return result;
        }

        internal class holeref
        {
            public int ndx;
            public List<xyz> hole;
            public holeref(int n, List<xyz> a)
            {
                ndx = n;
                hole = a;
            }
        }

        internal void FindMainsAndHoles(List<List<xyz>> loops3d, List<List<xyz>> mains, List<holeref> holes)
        {
            List<List<xy>> loops2d = ut.Convert3dLoopsTo2d(loops3d);

            int done = 0;
            for (int i = 0; i < loops2d.Count; i++)
            {
                List<xy> iloop = loops2d[i];
                bool i_is_hole = false;
                for (int j = 0; j < loops2d.Count; j++)
                {
                    if (i != j)
                    {
                        if (
                            ut.PointInsidePoly_NoEdgeCheck(loops2d[j], iloop[0])
                            && !ut.PointOnAnySegment(iloop[0], loops2d[j])
                            )
                        {
                            // loop i is inside loop j.  i is a hole.  j is a main.
                            int ndx = mains.IndexOf(loops3d[j]);
                            if (ndx < 0)
                            {
                                mains.Add(loops3d[j]);
                                ndx = mains.Count - 1;
                                done++;
                            }
                            holes.Add(new holeref(ndx, loops3d[i]));
                            done++;
                            i_is_hole = true;
                            break;
                        }
                    }
                }

                if (!i_is_hole)
                {

                    int ndx = mains.IndexOf(loops3d[i]);
                    if (ndx < 0)
                    {
                        mains.Add(loops3d[i]);
                        ndx = mains.Count - 1;
                        done++;
                    }
                }

                if (done == loops2d.Count)
                {
                    break;
                }
            }

            Debug.Assert(done == loops3d.Count);
        }

        internal void CreateFacesFromPileOfLoops(string fname, bool Shade, string OriginalName, List<List<xyz>> loops3d)
        {
            if (loops3d.Count > 1)
            {
                // all these loops have to be coplanar

                List<List<xyz>> mains = new List<List<xyz>>();
                List<holeref> holes = new List<holeref>();

                FindMainsAndHoles(loops3d, mains, holes);

                List<Face> newFaces = CreateFaces(fname, OriginalName, mains);

                foreach (holeref hr in holes)
                {
                    Face f = newFaces[hr.ndx];
                    int[] indices = this.VerifyAllPointsExist(hr.hole);
                    f.AddHole(indices);
                }

                foreach (Face f in newFaces)
                {
                    f.Shade = Shade;
                    f.RecalcPlane();
                }
            }
            else
            {
                List<xyz> loop = loops3d[0];
                Face f = this.CreateFaceFromPoints(fname, loop);
                f.OriginalName = OriginalName;
                f.Shade = Shade;
                f.RecalcPlane();
            }
        }

        internal bool SegmentOnSurface(bsp3d bsp, xyz p1, xyz p2)
        {
            if (!PointOnSurface(bsp, p1))
            {
                return false;
            }

            if (!PointOnSurface(bsp, p2))
            {
                return false;
            }

            // TODO this is absurd

            xyz v = p2 - p1;
            for (double t = 0.1; t <= 0.9; t += 0.1)
            {
                xyz p = (t * v).add_in_place(p1);
                if (!PointOnSurface(bsp, p))
                {
                    return false;
                }
            }
            return true;
        }

        // TODO this function could be called IsDiagonal, except then it wouldn't catch segments which are edges or are on a face
        internal bool SegmentInside(bsp3d bsp, xyz p1, xyz p2)
        {
            if (!PointInside(bsp, p1))
            {
                return false;
            }

            if (!PointInside(bsp, p2))
            {
                return false;
            }

            // TODO this is absurd

            xyz v = p2 - p1;
            for (double t = 0.1; t <= 0.9; t += 0.2)
            {
                xyz p = (t * v).add_in_place(p1);
                if (!PointInside(bsp, p))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool PointOnSurface(bsp3d bsp, xyz p)
        {
            PointInPoly pip = bsp.PointInPolyhedron(p);
            return pip == PointInPoly.Coincident;
        }

        internal bool PointInside(bsp3d bsp, xyz p)
        {

            PointInPoly pip = bsp.PointInPolyhedron(p);
            return pip != PointInPoly.Outside;
        }

        internal List<int> ConvertVerticesToVertexIndices(List<xyz> a)
        {
            List<int> b = new List<int>();
            foreach (xyz v in a)
            {
                int ndx = this.Vertices.IndexOf(v);
                Debug.Assert(ndx >= 0);
                b.Add(ndx);
            }
            return b;
        }

        internal int[] VerifyAllPointsExist(List<xyz> a)
        {
            int[] vs = new int[a.Count];
            for (int i = 0; i < vs.Length; i++)
            {
                xyz v2 = a[i];
                xyz v1 = GetVertex(v2);
                int ndx;
                if (v1 == null)
                {
                    ndx = AddVertex(v2.x, v2.y, v2.z);
                }
                else
                {
                    ndx = Vertices.IndexOf(v1);
                }
                vs[i] = ndx;
            }
            return vs;
        }

        // TODO note that this is no longer used anywhere except in the unit tests
        internal void KillFace(Face f)
        {
            foreach (EdgeLoop el in f.loops)
            {
                el.Kill();
            }
            this.Faces.Remove(f);
            f.loops = null;
            f.solid = null;
            f.name = null;
        }

        internal Edge FindEdge(string id1, string id2)
        {
            Face f1 = FindFace(id1);
            if (f1 == null)
            {
                return null;
            }
            HalfEdge he = f1.FindEdge(id2);
            if (he == null)
            {
                return null;
            }
            return he.edge;
        }

        internal Face FindFace(string name)
        {
            foreach (Face f in Faces)
            {
                if (f.name == name)
                {
                    return f;
                }
            }
            return null;
        }

        internal List<Face> FindFacesByOriginalName(string oname)
        {
            List<Face> result = new List<Face>();

            foreach (Face f in Faces)
            {
                if (f.OriginalName == oname)
                {
                    result.Add(f);
                }
            }

            return result;
        }

        private void DoGeomCheck_FaceIntersections()
        {
            for (int i = 0; i < Faces.Count; i++)
            {
                Face iface = Faces[i];
                for (int j = i + 1; j < Faces.Count; j++)
                {
                    Face jface = Faces[j];

                    if (iface.SharesAnEdgeWith(jface))
                    {
                        continue;
                    }

                    if (iface.IsCoPlanarWith(jface))
                    {
                        Debug.Assert(!iface.IntersectsWith_SamePlane(jface));
                    }

                    if (iface.SharesAVertexWith(jface))
                    {
                        // TODO explore this case more.  It is not safe to just ignore these two faces because they share a vertex.
                        continue;
                    }

                    Debug.Assert(!iface.IntersectsWith(jface));
                }
            }
        }

        internal Solid DoGeomChecks()
        {
            Debug.Assert(this.IsClosed());

            foreach (Edge e in this.Edges)
            {
                e.DoValidityAssertions();
            }

            foreach (Face f in this.Faces)
            {
                f.DoGeomChecks();
            }

            DoGeomCheck_FaceIntersections();

#if DEBUG
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                for (int j = i + 1; j < this.Vertices.Count; j++)
                {
                    Debug.Assert(!fp.eq_inches(this.Vertices[i], this.Vertices[j]));
                }
            }
#endif

#if not // TODO for now we do not support volume texture coordinates
			if (this.board_origin != null)
			{
				foreach (xyz v in this.Vertices)
				{
					xyz tc = this.GetVolumeTextureCoords(v);
					if (
						ut.lt(tc.x , 0)
						|| ut.gt(tc.x , 1)
						|| ut.lt(tc.y , 0)
						|| ut.gt(tc.y , 1)
						|| ut.lt(tc.z , 0)
						|| ut.gt(tc.z , 1)
						)
					{
						throw new ShouldNeverHappenException("texture coords out of range");
					}
				}
			}
#endif

#if DEBUG
            foreach (xyz v in this.Vertices)
            {
                List<HalfEdge> a1 = new List<HalfEdge>();
                foreach (Edge e in this.Edges)
                {
                    if (e.a2b.to == v)
                    {
                        Debug.Assert(e.b == v);
                        a1.Add(e.a2b);
                    }
                    else if (e.b2a.to == v)
                    {
                        Debug.Assert(e.a == v);
                        a1.Add(e.b2a);
                    }
                }

                List<HalfEdge> a2 = new List<HalfEdge>();
                foreach (Face f in this.Faces)
                {
                    foreach (EdgeLoop loop in f.loops)
                    {
                        for (int i = 0; i < loop.Count; i++)
                        {
                            HalfEdge he = loop[i];
                            Debug.Assert(he.face == f);
                            if (he.to == v)
                            {
                                a2.Add(he);
                            }
                        }
                    }
                }
            }

            List<xyz> myverts = new List<xyz>();
            foreach (Edge e in this.Edges)
            {
                if (!myverts.Contains(e.a))
                {
                    myverts.Add(e.a);
                }
                if (!myverts.Contains(e.b))
                {
                    myverts.Add(e.b);
                }
            }
            Debug.Assert(myverts.Count == this.Vertices.Count);

            myverts = new List<xyz>();
            foreach (Face f in this.Faces)
            {
                foreach (EdgeLoop loop in f.loops)
                {
                    for (int i = 0; i < loop.Count; i++)
                    {
                        HalfEdge he = loop[i];
                        if (!myverts.Contains(he.to))
                        {
                            myverts.Add(he.to);
                        }
                    }
                }
            }
            Debug.Assert(myverts.Count == this.Vertices.Count);
#endif

            return this;
        }

        internal bool IsClosed()
        {
            // closed iff every edge has exactly two faces
            foreach (Edge e in Edges)
            {
                if (
                    (e.a2b == null)
                    || (e.b2a == null)
                    )
                {
                    return false;
                }
            }
            return true;
        }

        public List<Line3d> GetLines()
        {
            List<Line3d> a = new List<Line3d>();
            GetLines(a);
            return a;
        }

        public void GetLines(List<Line3d> a)
        {
            foreach (Edge e in Edges)
            {
                if (
                    !e.a2b.face.Shade
                    && !e.b2a.face.Shade
                    )
                {
                    Line3d l = new Line3d(e.a, e.b);
                    a.Add(l);
                }
            }
        }

        public void GetTriangles(List<Triangle3d> a)
        {
            foreach (Face f in Faces)
            {
                f.GetTriangles(a);
            }
        }

        public List<Triangle3d> GetTriangles()
        {
            List<Triangle3d> a = new List<Triangle3d>();
            this.GetTriangles(a);
            return a;
        }

        internal Face CreateFaceCW(string name, params int[] verts)
        {
            int[] v2 = new int[verts.Length];
            for (int i = 0; i < v2.Length; i++)
            {
                v2[i] = verts[verts.Length - i - 1];
            }
            return CreateFace(name, v2);
        }

        internal Face CreateFaceFromPoints(string name, List<xyz> a)
        {
            List<xyz> b = this.ConvertPointsToVertices(a);
            return CreateFace(name, b);
        }

        internal Face CreateFace(string name, List<xyz> arraylist_of_vertices_that_already_exist)
        {
            List<int> alIndices = this.ConvertVerticesToVertexIndices(arraylist_of_vertices_that_already_exist);
            int[] indices = new int[alIndices.Count];
            alIndices.CopyTo(indices);

            return CreateFace(name, indices);
        }

#if DEBUG
        internal void AssertNoNameClashes()
        {
            for (int i = 0; i < Faces.Count; i++)
            {
                for (int j = i + 1; j < Faces.Count; j++)
                {
                    //Debug.Assert(Faces[i].name != Faces[j].name, string.Format("name clash: {0}", Faces[i].name));
                    Debug.Assert(Faces[i].name != Faces[j].name);
                }
            }
        }
#endif

        internal Face CreateFace(string name, params int[] verts)
        {
            Face f = new Face(name, this, verts);

            Faces.Add(f);

            return f;
        }

        private Dictionary<xyz, List<Edge>> edgefinder = new Dictionary<xyz, List<Edge>>();

        internal Edge GetEdge(xyz a, xyz b)
        {
            if (edgefinder.ContainsKey(a))
            {
                List<Edge> le = edgefinder[a];
                if (le != null)
                {
                    foreach (Edge e in le)
                    {
                        if (e.MatchVertexPair(a, b))
                        {
                            return e;
                        }
                    }
                }
            }
            return null;
        }

        internal Edge CreateEdge(xyz a, xyz b)
        {
            Edge e = new Edge(a, b);
            Edges.Add(e);
            edgefinder[a].Add(e);
            edgefinder[b].Add(e);
            return e;
        }

        internal HalfEdge CreateHalfEdge(Face f, xyz from, xyz to)
        {
            Edge edge = GetEdge(from, to);
            if (edge == null)
            {
                edge = CreateEdge(from, to);
            }
            HalfEdge he = edge.CreateHalfEdge(f, from, to);
            return he;
        }

        internal xyz GetVertex(int ndx)
        {
            return Vertices[ndx];
        }

        internal List<xyz> ConvertPointsToVertices(List<xyz> a)
        {
            List<xyz> b = new List<xyz>();
            foreach (xyz p in a)
            {
                b.Add(this.GetOrCreateVertex(p));
            }
            return b;
        }

        internal xyz GetOrCreateVertex(xyz pt)
        {
            xyz v = GetVertex(pt);
            if (v == null)
            {
                int ndx = AddVertex(pt.x, pt.y, pt.z);
                v = Vertices[ndx];
            }
            return v;
        }

        internal int GetOrCreateVertex_ReturnIndex(xyz pt)
        {
            int ndx = this.GetVertexIndex(pt);
            if (ndx < 0)
            {
                ndx = AddVertex(pt.x, pt.y, pt.z);
            }
            return ndx;
        }

        internal int GetVertexIndex(xyz pt)
        {
#if true
            int ndx = this.Vertices.IndexOf(pt);
            if (ndx >= 0)
            {
                return ndx;
            }
#endif

            for (int i = 0; i < this.Vertices.Count; i++)
            {
                xyz v = (xyz)Vertices[i];
                if (fp.eq_inches(pt, v))
                {
                    return i;
                }
            }
            return -1;
        }

        internal xyz GetVertex(double x, double y, double z)
        {
            foreach (xyz v in this.Vertices)
            {
                if (
                    fp.eq_inches(x, v.x)
                    && fp.eq_inches(y, v.y)
                    && fp.eq_inches(z, v.z)
                    )
                {
                    return v;
                }
            }
            return null;
        }

        internal xyz GetVertex(xyz pt)
        {
            foreach (xyz v in this.Vertices)
            {
                if (fp.eq_inches(pt, v))
                {
                    return v;
                }
            }
            return null;
        }

        internal void AddVertex(xyz v)
        {
            Vertices.Add(v);
            edgefinder[v] = new List<Edge>();
        }

        internal int AddVertex(double x, double y, double z)
        {
            Debug.Assert(GetVertex(x, y, z) == null);

            AddVertex(new xyz(x, y, z));

            return Vertices.Count - 1;
        }

#if DEBUG
        internal void Rotate_Deg(double degrees, xyz p1, xyz p2)
        {
            // It's not a good idea to use this, since it means we converted from radians to degrees and back to radians
            double r = ut.DegreeToRadian(degrees);

            Rotate(Math.Cos(r), Math.Sin(r), p1, (p2 - p1).normalize_in_place());
        }
#endif

#if not // TODO for now we do not support volume texture coordinates
		public xyz GetVolumeTextureCoords(xyz p)
		{
			xyz vp = p - board_origin;
			double u = xyz.dot(vp, board_u.normalize());
			double v = xyz.dot(vp, board_v.normalize());
			double w = xyz.dot(vp, board_w.normalize());

			u = u / board_u.magnitude();
			v = v / board_v.magnitude();
			w = w / board_w.magnitude();

			// TODO the following test is probably a bit too fussy
			if (
				ut.gt(u, 1)
				|| ut.gt(v,1)
				|| ut.gt(w,1)
				)
			{
				throw new ShouldNeverHappenException("texture coord out of range");
			}

			return new xyz(u,v,w);
		}
#endif

        public void Rotate(double costheta, double sintheta, xyz p1, xyz u)
        {
            if (fp.eq_radians(1, costheta))
            {
                return;
            }

            foreach (xyz v in Vertices)
            {
                ut.RotatePointAboutLine_InPlace(v, costheta, sintheta, p1, u);
            }

            if (board_origin != null)
            {
                board_u = ut.RotateVectorAboutLine(board_u, board_origin, costheta, sintheta, p1, u);
                board_v = ut.RotateVectorAboutLine(board_v, board_origin, costheta, sintheta, p1, u);
                board_w = ut.RotateVectorAboutLine(board_w, board_origin, costheta, sintheta, p1, u);

                board_origin = ut.RotatePointAboutLine(board_origin, costheta, sintheta, p1, u);
            }

            RecalcFacePlanes();
        }

        internal void RecalcFacePlanes()
        {
            foreach (Face f in this.Faces)
            {
                f.RecalcPlane();
            }
        }

        public void Translate(double x, double y, double z)
        {
            if (
                fp.eq_inches(0, x)
                && fp.eq_inches(0, y)
                && fp.eq_inches(0, z)
                )
            {
                return;
            }

            foreach (xyz p in this.Vertices)
            {
                p.Translate(x, y, z);
            }

            if (board_origin != null)
            {
                board_origin.Translate(x, y, z);
            }

            RecalcFacePlanes();

            // TODO instead of modeling this like a cache, model it like myPlane and recalc it only when needed
            bbCache = null;
        }

        internal void SumAllVerts(out xyz sum, out int count)
        {
            xyz r = new xyz(0, 0, 0);
            foreach (xyz p in this.Vertices)
            {
                r.add_in_place(p);
            }
            int c = this.Vertices.Count;

            sum = r;
            count = c;
        }

        public xyz GetCenter()
        {
            return this.GetBoundingBox().center;
        }

        // TODO instead of modeling this like a cache, model it like myPlane and recalc it only when needed
        private BoundingBox3d bbCache;

        public BoundingBox3d GetBoundingBox()
        {
            // TODO do we need to lock (this) ?
            {
                if (bbCache == null)
                {
                    bbCache = BoundingBox3d.FromArrayOfPoints(Vertices);
                }
            }
            return bbCache;
        }

        public double Weight()
        {
            double vol = Volume();

            return vol * material.GetPoundsPerCubicInch();
        }

        public double Volume()
        {
            double volume = 0.0;
            foreach (Face f in Faces)
            {
                volume += f.FaceVolume();
            }
            volume = volume / 6;

            return volume;
        }

        public double SurfaceArea()
        {
            double area = 0;
            foreach (Face f in Faces)
            {
                area += f.Area();
            }
            return area;
        }

        internal bool AnyEdgePiercesAnyFace(Solid s2)
        {
            foreach (Face f in this.Faces)
            {
                if (f.AnyEdgePierces(s2))
                {
                    return true;
                }
            }
            return false;
        }

        internal void FixCollinearTrios()
        {
            foreach (Face f in Faces)
            {
                foreach (EdgeLoop el in f.loops)
                {
                    int i = 0;
                    do
                    {
                        HalfEdge heCur = el[i];
                        HalfEdge heNext = el[i + 1];
                        if (
                            (heCur.Opposite().face == heNext.Opposite().face)
                            && ut.PointsAreCollinear3d(heCur.from, heCur.to, heNext.to)
                            )
                        {
                            // these two halfedges should be one.  merge them by removing heNext

                            // to remove heNext, we need to remove both halves of the edge from their respective edgeloops
                            HalfEdge heNextOpp = heNext.Opposite();

                            xyz deadvert = heCur.to;

                            // extend heCur to cover the full length of the edge
                            el.Remove(heNext, deadvert);
                            if (heCur.to == heCur.edge.a)
                            {
                                heCur.edge.a = heNext.to;
                            }
                            else
                            {
                                heCur.edge.b = heNext.to;
                            }
                            heCur.to = heNext.to;
                            EdgeLoop elOther = heNextOpp.face.GetEdgeLoopFor(heNextOpp);
                            int ndxOther = elOther.IndexOf(heNextOpp);
                            elOther.Remove(heNextOpp, deadvert);

                            this.Vertices.Remove(deadvert);

                            this.Edges.Remove(heNext.edge);

                            el.DoGeomChecks();
                            elOther.DoGeomChecks();

                            this.DoGeomChecks();
                        }
                        else
                        {
                            i++;
                        }
                    }
                    while (i < el.Count);
                }
            }
        }
    }
}
