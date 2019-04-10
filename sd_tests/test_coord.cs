
#if DEBUG

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using NUnit.Framework;

namespace sd
{
    [TestFixture]
    public class test_coord
    {
        // TODO test some cases of converting to 2d

        // TODO test the raw normal from array of points stuff

        // test some basic cases of normals being calculated properly, including some NOT parallel to the Z=0 plane

        [Test]
        public void test_coord_system()
        {
            xyz normalShouldBe = new xyz(0, 0, 1);

            // note that these points are counter-clockwise
            xyz pt1 = new xyz(0, 0, 0);
            xyz pt2 = new xyz(1, 0, 0);
            xyz pt3 = new xyz(0, 1, 0);
            xyz n = xyz.normal(pt1, pt2, pt3);
            Assert.IsTrue(fp.eq_unitvec(n, normalShouldBe));

            // now convert to 2d and see if the point is on the correct side of the line
            List<xyz> a3d = new List<xyz>();
            a3d.Add(pt1);
            a3d.Add(pt2);
            a3d.Add(pt3);
            List<xy> a2d = ut.Convert3dPointsTo2d(a3d);
            Assert.AreEqual(-1, ut.PointSideOfLine(a2d[2], a2d[0], a2d[1]));

            // now see what GetPlaneFromArrayOfPoints says
            xyz origin;
            xyz norm;
            xyz iv;
            xyz jv;
            ut.GetPlaneFromArrayOfCoplanarPoints(a3d, out origin, out norm, out iv, out jv);
            Assert.IsTrue(fp.eq_unitvec(norm, normalShouldBe));

            // now see what GetNormalFromPointList says
            xyz n3 = ut.GetUnitNormalFromPointList(a3d);
            Assert.IsTrue(fp.eq_unitvec(n3, normalShouldBe));
        }

        [Test]
        public void test_normals()
        {
            List<xyz> p = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(7, 0, 0), new xyz(7, 5, 0), new xyz(9, 5, 0), new xyz(9, 8, 0), new xyz(5, 12, 0), new xyz(0, 13, 0), new xyz(0, 9, 0), new xyz(-4, 9, 0), new xyz(-4, 5, 0), new xyz(-2, 0, 0));
            xyz n = ut.GetUnitNormalFromPointList(p);
            Assert.IsTrue(fp.eq_unitvec(new xyz(0, 0, 1), n));
        }

        [Test]
        public void test_convert_to_2d()
        {
            Triangle3d t = new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0));
            Triangle2d t2 = t.ConvertTo2d();
            Assert.IsTrue(fp.eq_inches(new xy(0, 0), t2.a));
            Assert.IsTrue(fp.eq_inches(new xy(5, 0), t2.b));
            Assert.IsTrue(fp.eq_inches(new xy(5, 5), t2.c));
        }

        [Test]
        public void test_cross()
        {
            Assert.IsTrue(fp.eq_unitvec(new xyz(0, 0, 1), xyz.cross(new xyz(1, 0, 0), new xyz(0, 1, 0))));
        }

        [Test]
        public void test_tri_normals()
        {
            Assert.IsTrue(fp.eq_unitvec(new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0)).n, new xyz(0, 0, 1)));
            Assert.IsTrue(fp.eq_unitvec(new Triangle3d(new xyz(0, 0, 0), new xyz(0, 0, 1), new xyz(0, 1, 0)).n, new xyz(-1, 0, 0)));
            Assert.IsTrue(fp.eq_unitvec(new Triangle3d(new xyz(0, 0, 0), new xyz(1, 0, 0), new xyz(0, 0, 1)).n, new xyz(0, -1, 0)));
        }

        [Test]
        public void test_triangle3d_whichside()
        {
            Triangle3d t = new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0));

            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(0, 0, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(5, 0, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(5, 5, 0)));

            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(3, 0, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(5, 3, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(3, 3, 0)));

            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(4, 1, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(2, 1, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(3, 2, 0)));

            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(7, 7, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(5, 6, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(0, 9, 0)));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(new xyz(-1, -1, 0)));

            Assert.AreEqual(WhichSideOfPlane.Outside, t.Classify(new xyz(4, 1, 1)));
            Assert.AreEqual(WhichSideOfPlane.Outside, t.Classify(new xyz(0, 0, 1)));
            Assert.AreEqual(WhichSideOfPlane.Outside, t.Classify(new xyz(9, 9, 1)));

            Assert.AreEqual(WhichSideOfPlane.Inside, t.Classify(new xyz(3, 1, -1)));
        }

        [Test]
        public void test_triangle3d_whichside_tri()
        {
            Triangle3d t = new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0));
            Assert.AreEqual(WhichSideOfPlane.Coplanar, t.Classify(t));

            Assert.AreEqual(WhichSideOfPlane.Inside, t.Classify(new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 0, -2))));

            Assert.AreEqual(WhichSideOfPlane.Outside, t.Classify(new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 0, 2))));

            Assert.AreEqual(WhichSideOfPlane.Split, t.Classify(new Triangle3d(new xyz(0, 0, -1), new xyz(5, 0, -1), new xyz(5, 0, 2))));
        }

        [Test]
        public void test_cube()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 5);
            bsp3d bsp = new bsp3d(s1);
            BoundingBox3d bb = BoundingBox3d.FromArrayOfPoints(s1.Vertices);
            foreach (xyz v in s1.Vertices)
            {
                Assert.AreEqual(PointInPoly.Coincident, bsp.PointInPolyhedron(v));
                Assert.IsTrue(bb.PointInside(v));
            }
            foreach (Face f in s1.Faces)
            {
                xyz v = f.GetCenter();
                xyz n = f.UnitNormal();
                Assert.AreEqual(PointInPoly.Coincident, bsp.PointInPolyhedron(v));
                Assert.AreEqual(PointInPoly.Outside, bsp.PointInPolyhedron(v + n));
                Assert.AreEqual(PointInPoly.Inside, bsp.PointInPolyhedron(v - n));
                Assert.IsTrue(bb.PointInside(v));
                Assert.IsFalse(bb.PointInside(v + n));
                Assert.IsTrue(bb.PointInside(v - n));
            }
            foreach (Face f in s1.Faces)
            {
                foreach (HalfEdge h in f.MainLoop)
                {
                    xyz v = h.Center();
                    xyz n = h.GetInwardNormal();
                    Assert.IsTrue(bb.PointInside(v));
                    Assert.IsTrue(bb.PointInside(v + n));
                    Assert.IsFalse(bb.PointInside(v - n));
                    Assert.AreEqual(PointInPoly.Coincident, bsp.PointInPolyhedron(v));
                    Assert.AreEqual(PointInPoly.Coincident, bsp.PointInPolyhedron(v + n));
                    Assert.AreEqual(PointInPoly.Outside, bsp.PointInPolyhedron(v - n));
                }
            }
            Assert.AreEqual(PointInPoly.Inside, bsp.PointInPolyhedron(s1.GetCenter()));
            Assert.IsTrue(bb.PointInside(s1.GetCenter()));
        }
    }
}

#endif

