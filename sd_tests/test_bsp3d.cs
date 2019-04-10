
#if DEBUG

using System;
using System.Drawing;
using System.Text;
using System.Xml;
using System.IO;

using NUnit.Framework;

namespace sd
{
    [TestFixture]
    public class test_bsp3d
    {
        [Test]
        public void test1()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 5);
            bsp3d bsp = new bsp3d(s1);
            Assert.AreEqual(PointInPoly.Inside, bsp.PointInPolyhedron(new xyz(1, 1, -1)));
            Assert.AreEqual(PointInPoly.Outside, bsp.PointInPolyhedron(new xyz(6, 6, -6)));
        }
    }
}

#endif

