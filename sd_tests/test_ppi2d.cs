
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
    public class test_ppi2d
    {
        [Test]
        public void test_with_holes()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            List<xy> h1 = ut.MakePoly(new xy(1, 1), new xy(1, 3), new xy(3, 3), new xy(3, 1));
            List<xy> p2 = ut.MakePoly(new xy(0, 2), new xy(4, 2), new xy(4, 4), new xy(0, 4));
            List<List<xy>> both1 = new List<List<xy>>();
            both1.Add(p1);
            both1.Add(h1);
            ppi2d pi1 = new ppi2d(both1, p2);
            Assert.AreEqual(2 * 4 - 2 * 1, ut.PolygonArea2d(ppi2d.FindTheLoops(pi1.GetIntersection())));

            ppi2d pi2 = new ppi2d(p2, both1);
            Assert.AreEqual(2 * 4 - 2 * 1, ut.PolygonArea2d(ppi2d.FindTheLoops(pi2.GetIntersection())));

            foreach (seginfo2d si in pi2.p1.info)
            {
                Assert.IsNotNull(si);
                Assert.IsNotNull(si.seg);
                string q = si.ToString();
                Assert.IsNotNull(q);
            }
        }

        [Test]
        public void simple()
        {
            // two 4x4 squares, overlap area is 2x2
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            List<xy> p2 = ut.MakePoly(new xy(2, 2), new xy(6, 2), new xy(6, 6), new xy(2, 6));

            ppi2d pi = new ppi2d(p1, p2);

            Assert.AreEqual(2 * 2, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetIntersection())));
            Assert.AreEqual(4 * 4 + 4 * 4 - 2 * 2, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetUnion())));
            Assert.AreEqual(4 * 4 - 2 * 2, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference1())));
            Assert.AreEqual(4 * 4 - 2 * 2, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference2())));
        }

        [Test]
        public void triangle_inside_square()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            List<xy> p2 = ut.MakePoly(new xy(1, 1), new xy(3, 1), new xy(2, 3));

            ppi2d pi = new ppi2d(p1, p2);

            Assert.AreEqual(ut.PolygonArea2d(p2), ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetIntersection())));
            Assert.AreEqual(4 * 4, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetUnion())));
            Assert.AreEqual(4 * 4 - ut.PolygonArea2d(p2), ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference1())));
            Assert.AreEqual(0, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference2())));
        }

        [Test]
        public void triangle_touching_square_at_vertex()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            List<xy> p2 = ut.MakePoly(new xy(3, 3), new xy(5, 5), new xy(3, 7));

            ppi2d pi = new ppi2d(p1, p2);

            Assert.AreEqual(0.5, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetIntersection())));
            Assert.AreEqual(4 * 4 + 2 * 4 / 2 - 0.5, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetUnion())));
            Assert.AreEqual(4 * 4 - 0.5, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference1())));
            Assert.AreEqual(2 * 4 / 2 - 0.5, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference2())));
        }

        [Test]
        public void nasty_case()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(10, 0), new xy(10, 1), new xy(0, 1), new xy(0, 2), new xy(5, 2), new xy(5, 3), new xy(0, 3), new xy(0, 4), new xy(10, 4), new xy(10, 5), new xy(0, 5), new xy(0, 6), new xy(5, 6), new xy(5, 7), new xy(0, 7), new xy(0, 8), new xy(10, 8), new xy(10, 9), new xy(0, 9), new xy(0, 10), new xy(10, 10), new xy(10, 11), new xy(-1, 11), new xy(-1, 0));
            List<xy> p2 = ut.MakePoly(new xy(5, 0), new xy(6, 0), new xy(6, 11), new xy(5, 11));

            ppi2d pi = new ppi2d(p1, p2);

            Assert.AreEqual(11, ut.PolygonArea2d(p2));
            Assert.AreEqual(11 + 1 + 6 + 1 + 11 + 1 + 6 + 1 + 11 + 1 + 11, ut.PolygonArea2d(p1));

            Assert.AreEqual(4, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetIntersection())));
            Assert.AreEqual(ut.PolygonArea2d(p1) + 3 + 3 + 1, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetUnion())));
            Assert.AreEqual(ut.PolygonArea2d(p1) - 4, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference1())));
            Assert.AreEqual(7, ut.PolygonArea2d(ppi2d.FindTheLoops(pi.GetDifference2())));
        }

    }
}

#endif

