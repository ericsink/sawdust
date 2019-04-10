
#if DEBUG

using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace sd
{
    [TestFixture]
    public class test_fp
    {
        [Test]
        public void test_point_distancetoline()
        {
            xy v1 = new xy(0, 0);
            xy v2 = new xy(8, 0);
            xy p = new xy(3, -0.001);
            xy diff = (v2 - v1).normalize();
            xy other = p - v1;
            double d = xy.kross(other, diff);
            Console.WriteLine(d);
        }
    }
}

#endif
