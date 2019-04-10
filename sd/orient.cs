
using System;
using System.Drawing;
using System.Diagnostics;

namespace sd
{
    internal enum EdgeAlignment
    {
        Center,
        Right,
        Left
    }

    internal interface IOrient
    {
        void Translate(double x, double y, double z);
        void Rotate(double costheta, double sintheta, xyz p1, xyz u);
    }

    internal class orient
    {
        private static void RotateSoFacesAreParallelAndSameDir(IOrient s2, Face f1, Face f2, xyz rotpt)
        {
            // rotate s2 so the faces are facing in the same direction

            xyz n1 = f1.UnitNormal();
            xyz n2 = f2.UnitNormal();

            // TODO I'm not sure the first two cases are actually correct.
            if (fp.eq_unitvec(n1, n2))
            {
                // we need to flip s2 180 degrees with respect to the plane of f2
                xyz rotv = (f2.MainLoop[1].to - f2.MainLoop[0].to).normalize_in_place();
                // cos(pi) is -1  and sin(pi) is 0
                s2.Rotate(-1, 0, rotpt, rotv);
            }
            else if (fp.eq_unitvec(n1, -n2))
            {
                // do nothing.  this is perfect.
            }
            else
            {
                double dot = xyz.dot(n1, n2);
                xyz kp = xyz.cross(n2, n1).normalize_in_place();
                s2.Rotate(dot, Math.Sqrt(1 - dot * dot), rotpt, kp);
            }
        }

        private static void RotateSoFacesAreParallelAndOpposite(IOrient s2, Face f1, Face f2, xyz rotpt)
        {
            // rotate s2 so the faces are facing in opposite directions

            xyz n1 = f1.UnitNormal();
            xyz n2 = f2.UnitNormal();
            if (fp.eq_unitvec(n1, n2))
            {
                // we need to flip s2 180 degrees with respect to the plane of f2
                xyz rotv = (f2.MainLoop[1].to - f2.MainLoop[0].to).normalize_in_place();
                // cos(pi) is -1  and sin(pi) is 0
                s2.Rotate(-1, 0, rotpt, rotv);
            }
            else if (fp.eq_unitvec(n1, -n2))
            {
                // do nothing.  this is perfect.
            }
            else
            {
                double dot = xyz.dot(n1, n2);
                xyz kp = xyz.cross(n2, n1).normalize_in_place();
                s2.Rotate(-dot, -Math.Sqrt(1 - dot * dot), rotpt, kp);
            }
        }

        public static void Edges(IOrient s2, Face f1, Face f2, HalfEdge he1, HalfEdge he2, EdgeAlignment align, double offset1, double offset2)
        {
            Edges(s2, f1, f2, he1, he2, align, offset1, offset2, false);
        }

        public static void Edges(IOrient s2, Face f1, Face f2, HalfEdge he1, HalfEdge he2, EdgeAlignment align, double offset1, double offset2, bool reversed)
        {
            xyz v1;
            xyz v2;

            xyz uv1 = (he1.to - he1.from).normalize_in_place();

            Debug.Assert(
                (align == EdgeAlignment.Center)
                || (align == EdgeAlignment.Right)
                || (align == EdgeAlignment.Left)
                );

            // move the solid to match up two points
            if (align == EdgeAlignment.Center)
            {
                v1 = he1.Center();
                if (!fp.eq_inches(offset1, 0))
                {
                    v1 += (offset1 * uv1);
                }
                v2 = he2.Center();
            }
            else if (align == EdgeAlignment.Right)
            {
                v1 = he1.to;
                if (!fp.eq_inches(offset1, 0))
                {
                    v1 -= (offset1 * uv1);
                }
                v2 = he2.from;
            }
            else // left
            {
                v1 = he1.from;
                if (!fp.eq_inches(offset1, 0))
                {
                    v1 += (offset1 * uv1);
                }
                v2 = he2.to;
            }

            if (!fp.eq_inches(offset2, 0))
            {
                xyz uv1perp = -(he1.GetInwardNormal());
                v1 += (offset2 * uv1perp);
            }

            xyz tv = v1 - v2;
            s2.Translate(tv.x, tv.y, tv.z);

            if (reversed)
            {
                RotateSoFacesAreParallelAndSameDir(s2, f1, f2, v1);
            }
            else
            {
                RotateSoFacesAreParallelAndOpposite(s2, f1, f2, v1);
            }

            // now rotate around origin = f1.MainLoop[0].to and vector = n1 to align the other points
            xyz q1 = he1.UnitVector();
            xyz q2 = he2.UnitVector();
            if (reversed)
            {
                q2 = -q2;
            }
            if (fp.eq_unitvec(q1, q2))
            {
                s2.Rotate(-1, 0, v1, f1.UnitNormal());
            }
            else if (fp.eq_unitvec(q1, -q2))
            {
                // do nothing.  this is perfect.
            }
            else
            {
                double dot = xyz.dot(q1, q2);
                xyz kp = xyz.cross(q2, q1).normalize_in_place();
                s2.Rotate(-dot, -Math.Sqrt(1 - dot * dot), v1, kp);
            }
        }
    }
}
