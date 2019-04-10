
#if DEBUG

using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

using NUnit.Framework;

namespace sd
{
    [TestFixture]
    public class test_misc
    {
        [Test]
        public void test_measure()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "a", 10, 24, 1);
            Face f = s.FindFace("top");
            HalfEdge he = f.FindLongestEdge();
            Assert.IsTrue(fp.eq_inches(24, he.Length()));
            xyz p1 = he.Center();
            xyz p2 = f.Measure(he, p1);
            xyz q = p2 - p1;
            Assert.IsTrue(fp.eq_inches(10, q.magnitude()));

            CompoundSolid cs = wood.Mortise(s.ToCompoundSolid(), he, new xy(3, 3), new xyz(18, 4, 2), "m1");

            f = cs.FindFace("a.top");
            Assert.AreEqual(2, f.loops.Count);
            he = f.FindLongestEdge();
            Assert.IsTrue(fp.eq_inches(24, he.Length()));
            p1 = he.Center();
            p2 = f.Measure(he, p1);
            q = p2 - p1;
            Assert.IsTrue(fp.eq_inches(3, q.magnitude()));
        }

        [Test]
        public void test_TouchesAnySegment()
        {
            List<xy> p = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            Assert.IsTrue(ut.TouchesAnySegment(p, new xy(0, 0), new xy(-4, 0)));
            Assert.IsTrue(ut.TouchesAnySegment(p, new xy(-2, 2), new xy(2, -2)));
            Assert.IsFalse(ut.TouchesAnySegment(p, new xy(1, 1), new xy(2, 2)));
        }

        [Test]
        public void test_PointOnAnySegment()
        {
            List<xyz> pts = ut.MakePoly(new xyz(3, 3, 3), new xyz(4, 4, 4), new xyz(1, 1, 1), new xyz(6, 6, 6), new xyz(5, 5, 5), new xyz(0, 0, 0), new xyz(7, 7, 7), new xyz(2, 2, 2));
            Assert.IsTrue(ut.PointOnAnySegment(new xyz(3, 3, 3), pts));
            pts = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0), new xyz(0, 5, 0));
            Assert.IsFalse(ut.PointOnAnySegment(new xyz(3, 3, 3), pts));
            Assert.IsTrue(ut.PointOnAnySegment(new xyz(0, 0, 0), pts));
            Assert.IsTrue(ut.PointOnAnySegment(new xyz(3, 0, 0), pts));
            Assert.IsTrue(ut.PointOnAnySegment(new xyz(5, 0, 0), pts));
            Assert.IsTrue(ut.PointOnAnySegment(new xyz(0, 4, 0), pts));
        }

        [Test]
        public void test_xy_copy()
        {
            xy p1 = new xy(5, 5);
            xy p2 = p1.copy();
            Assert.AreEqual(5, p2.x);
            Assert.IsTrue(fp.eq_unitvec(p1, p2));
            Assert.IsFalse(p1.GetHashCode() == p2.GetHashCode());
            p1.x = 9;
            Assert.IsFalse(fp.eq_unitvec(p1, p2));
        }

        [Test]
        public void test_bb3d()
        {
            BoundingBox3d bb = BoundingBox3d.FromArrayOfPoints(ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 5, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(3, 3, 3)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 0, 0)));
            Assert.IsTrue(bb.PointInside(new xyz(5, 5, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(5, 0, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 0, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 4, 0)));
            Assert.IsFalse(bb.PointInside(new xyz(0, 6, 0)));
            Assert.IsFalse(bb.PointInside(new xyz(0, 0, -1)));
            Assert.IsFalse(bb.PointInside(new xyz(7, 6, 0)));
            Assert.IsFalse(bb.PointInside(new xyz(7, 7, 7)));

            BoundingBox3d bb2 = BoundingBox3d.FromArrayOfPoints(ut.MakePoly(new xyz(8, 8, 8), new xyz(9, 9, 9)));

            bb = bb + bb2;
            Assert.IsTrue(bb.PointInside(new xyz(3, 3, 3)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 0, 0)));
            Assert.IsTrue(bb.PointInside(new xyz(5, 5, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(5, 0, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 0, 5)));
            Assert.IsTrue(bb.PointInside(new xyz(0, 4, 0)));

            Assert.IsTrue(bb.PointInside(new xyz(8, 8, 8)));
            Assert.IsTrue(bb.PointInside(new xyz(7, 7, 7)));
        }

        [Test]
        public void test_seg2d_convert()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));
            List<seg2d> segs = seg2d.Convert(poly1);
            Assert.AreEqual(4, segs.Count);
            double d = 0;
            foreach (seg2d s in segs)
            {
                d += s.Length;
                string tos = s.ToString();
                Assert.IsNotNull(tos);
                Assert.IsTrue(tos.Length > 0);
            }
            Assert.IsTrue(fp.eq_inches(20, d));
        }

        [Test]
        public void test_parseBool()
        {
            Assert.IsTrue(ut.ParseBool("yes", false));
            Assert.IsTrue(ut.ParseBool("true", false));
            Assert.IsTrue(ut.ParseBool("on", false));
            Assert.IsTrue(ut.ParseBool("TRUE", false));
            Assert.IsTrue(ut.ParseBool("tRue", false));
            Assert.IsTrue(ut.ParseBool("  true", false));
            Assert.IsTrue(ut.ParseBool("true  ", false));
            Assert.IsTrue(ut.ParseBool("  trUE  ", false));

            Assert.IsFalse(ut.ParseBool("false", true));
            Assert.IsFalse(ut.ParseBool("nope", true));
            Assert.IsFalse(ut.ParseBool("forget it", true));
            Assert.IsFalse(ut.ParseBool("tr ue", true));
            Assert.IsFalse(ut.ParseBool("f", true));
            Assert.IsFalse(ut.ParseBool("trew", true));
        }

        void calc_rot(xyz look, xyz me)
        {
            xyz look_proj = new xyz(look.x, 0, look.z);
            look_proj = look_proj.normalize();
            xyz me_proj = new xyz(me.x, 0, me.z);
            me_proj = me_proj.normalize();
            double rad = ut.GetAngleBetweenTwoNormalizedVectorsInRadians(look_proj, me_proj);
            double deg = ut.RadianToDegree(rad);

            xyz result = ut.RotateUnitVector(me.normalize(), deg, new xyz(0, 1, 0));
            Console.WriteLine("{0}: {1}  {2}", me, deg, result);
        }

        [Test]
        public void test_rots()
        {
            calc_rot(new xyz(0, 0, 1), new xyz(0, 0, 1));
            calc_rot(new xyz(0, 0, 1), new xyz(0, 0, -1));
            calc_rot(new xyz(0, 0, 1), new xyz(1, 0, -1));
            calc_rot(new xyz(0, 0, 1), new xyz(1, 0, 1));
        }

        [Test]
        public void test_rot_loop()
        {
            xyz b = new xyz(0, 0, 1);
            xyz axis = new xyz(0, 1, 0);
            for (int i = 0; i <= 360; i += 5)
            {
                xyz rot = ut.RotateUnitVector(b, i, axis);
                double deg = ut.HowManyDegreesToRotateN2ToEqualBaseVec(b, rot, axis);
                xyz b2 = ut.RotateUnitVector(rot, deg, axis);
                Assert.IsTrue(fp.eq_unitvec(b2, b));
                Assert.IsTrue(deg >= -180);
                Assert.IsTrue(deg <= 180);
            }
        }

        [Test]
        public void test_getallmaterials()
        {
            List<BoardMaterial> mats = BoardMaterial.GetAll();
            Assert.IsTrue(mats.Count > 0);
        }

        [Test]
        public void test_SortPointList()
        {
            List<xyz> pts;

            pts = ut.MakePoly(new xyz(3, 3, 3), new xyz(4, 4, 4), new xyz(1, 1, 1), new xyz(6, 6, 6), new xyz(5, 5, 5), new xyz(0, 0, 0), new xyz(7, 7, 7), new xyz(2, 2, 2));
            ut.SortPointList(pts);
            for (int i = 0; i <= 7; i++)
            {
                Assert.IsTrue(fp.eq_inches(pts[i], new xyz(i, i, i)));
            }
            ut.SortPointList(pts);
            for (int i = 0; i <= 7; i++)
            {
                Assert.IsTrue(fp.eq_inches(pts[i], new xyz(i, i, i)));
            }

            pts = ut.MakePoly(new xyz(3, 0, 0), new xyz(4, 0, 0), new xyz(1, 0, 0), new xyz(6, 0, 0), new xyz(5, 0, 0), new xyz(0, 0, 0), new xyz(7, 0, 0), new xyz(2, 0, 0));
            ut.SortPointList(pts);
            for (int i = 0; i <= 7; i++)
            {
                Assert.IsTrue(fp.eq_inches(pts[i], new xyz(i, 0, 0)));
            }
            ut.SortPointList(pts);
            for (int i = 0; i <= 7; i++)
            {
                Assert.IsTrue(fp.eq_inches(pts[i], new xyz(i, 0, 0)));
            }
        }
    }
}

#endif

