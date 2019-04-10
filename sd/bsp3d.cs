
using System;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    internal enum WhichSideOfPlane
    {
        Coplanar,
        Inside,
        Outside,
        Split
    }

    internal enum PointInPoly
    {
        Inside,
        Outside,
        Coincident
    }

    internal class bsp3d
    {
        internal class node
        {
            public Triangle3d tri;

            public node inside;
            public node outside;
            public List<Triangle3d> coincident;

            public node(Triangle3d t)
            {
                tri = t;
            }

            public PointInPoly PointInPolyhedron(xyz p)
            {
                WhichSideOfPlane sid = tri.Classify(p);
                Debug.Assert(
                    (sid == WhichSideOfPlane.Inside)
                    || (sid == WhichSideOfPlane.Outside)
                    || (sid == WhichSideOfPlane.Coplanar)
                    );
                if (sid == WhichSideOfPlane.Inside)
                {
                    if (inside != null)
                    {
                        return inside.PointInPolyhedron(p);
                    }
                    else
                    {
                        return PointInPoly.Inside;
                    }
                }
                else if (sid == WhichSideOfPlane.Outside)
                {
                    if (outside != null)
                    {
                        return outside.PointInPolyhedron(p);
                    }
                    else
                    {
                        return PointInPoly.Outside;
                    }
                }
                else
                {
                    // coplanar
                    if (tri.PointInside(p))
                    {
                        return PointInPoly.Coincident;
                    }

                    if (coincident != null)
                    {
                        foreach (Triangle3d tc in coincident)
                        {
                            if (tc.PointInside(p))
                            {
                                return PointInPoly.Coincident;
                            }
                        }
                    }

                    if (inside != null)
                    {
                        return inside.PointInPolyhedron(p);
                    }
                    else // if (outside != null)
                    {
                        Debug.Assert(outside != null);
                        return outside.PointInPolyhedron(p);
                    }
#if not
                    else
                    {
                        return PointInPoly.Coincident;
                    }
#endif
                }
            }
        }

        public Solid solid;
        public node top;

        public bsp3d(Solid s)
        {
            solid = s;

            List<Triangle3d> tris = solid.GetTriangles();
            top = ConstructTree(tris);
        }

        public int CountSplits(List<Triangle3d> tris, int which)
        {
            int count = 0;
            Triangle3d t1 = tris[which];
            for (int i = 0; i < tris.Count; i++)
            {
                if (i == which)
                {
                    continue;
                }
                Triangle3d t2 = tris[i];
                WhichSideOfPlane wsop = t1.Classify(t2);
                if (wsop == WhichSideOfPlane.Split)
                {
                    count++;
                }
            }
            return count;
        }

        public Triangle3d ChooseSplitter(List<Triangle3d> tris)
        {
            int cur_ndx = -1;
            int cur_splits = tris.Count * 2;

            for (int i = 0; i < tris.Count; i++)
            {
                int count = CountSplits(tris, i);
                if (count == 0)
                {
                    cur_ndx = i;
                    cur_splits = count;
                    break;
                }
                if (cur_ndx < 0)
                {
                    cur_ndx = i;
                    cur_splits = count;
                }
                else
                {
                    if (count < cur_splits)
                    {
                        cur_ndx = i;
                        cur_splits = count;
                    }
                }
            }

            Triangle3d t1 = tris[cur_ndx];
            tris.Remove(t1);
            return t1;
        }

        public node ConstructTree(List<Triangle3d> tris)
        {
            Triangle3d t1 = ChooseSplitter(tris);

            node n = new node(t1);

            List<Triangle3d> inList = new List<Triangle3d>();
            List<Triangle3d> outList = new List<Triangle3d>();
            foreach (Triangle3d t in tris)
            {
                WhichSideOfPlane wsop = t1.Classify(t);
#if false
                Debug.Assert(
                    (wsop == WhichSideOfPlane.Inside)
                    || (wsop == WhichSideOfPlane.Outside)
                    || (wsop == WhichSideOfPlane.Coplanar)
                    );
#else
                if (wsop == WhichSideOfPlane.Split)
                {
                    List<Triangle3d> inList2 = new List<Triangle3d>();
                    List<Triangle3d> outList2 = new List<Triangle3d>();
                    t1.Split(t, inList2, outList2);
                    inList.AddRange(inList2);
                    outList.AddRange(outList2);
                    break;
                }
                else
#endif
                if (wsop == WhichSideOfPlane.Inside)
                {
                    inList.Add(t);
                }
                else if (wsop == WhichSideOfPlane.Outside)
                {
                    outList.Add(t);
                }
                else
                {
                    if (n.coincident == null)
                    {
                        n.coincident = new List<Triangle3d>();
                    }
                    n.coincident.Add(t);
                }
            }

            if (inList.Count > 0)
            {
                n.inside = ConstructTree(inList);
            }
            if (outList.Count > 0)
            {
                n.outside = ConstructTree(outList);
            }

            return n;
        }

        public PointInPoly PointInPolyhedron(xyz p)
        {
            return top.PointInPolyhedron(p);
        }

#if false
        public bool AnyVertexInside(Solid s2)
        {
            foreach (xyz vert in s2.Vertices)
            {
                if (this.PointInPolyhedron(vert) == PointInPoly.Inside)
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
}

