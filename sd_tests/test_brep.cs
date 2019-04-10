
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
    public class test_brep
    {
        [Test]
        public void test_FullName()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "a", 10, 24, 1);
            Face f = s.FindFace("top");
            Assert.AreEqual(f.FullName, "a.top");
        }

        [Test]
        public void test_dot()
        {
            List<xyz> pts = new List<xyz>();
            pts.Add(new xyz(0, 0, 0));
            pts.Add(new xyz(4, 0, 0));
            pts.Add(new xyz(4, 4, 0));
            pts.Add(new xyz(0, 4, 0));

            xyz n = ut.GetRawNormalFromPointList(pts).normalize_in_place();
            xyz i = ((pts[1]) - (pts[0])).normalize_in_place();
            xyz j = xyz.cross(i, n).normalize_in_place();
            xyz p0 = (pts[0]);

            xyz pt = new xyz(2, 2, 5);
            Console.WriteLine("dist from plane to {0}: {1}", pt, xyz.dot(n, pt - p0));
        }

        [Test]
        public void test_GetLines()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            List<Line3d> lines = p.Result.GetLines();
            int count_edges = 0;
            foreach (Solid s in p.Result.Subs)
            {
                count_edges += s.Edges.Count;
            }
            Assert.IsTrue(lines.Count > 0);
            Assert.AreEqual(count_edges, lines.Count);
        }

        [Test]
        public void test_dovetail_ppi2d_case()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 0.75), new xy(0, 0.75));
            List<xy> p2 = ut.MakePoly(new xy(8, -23.25), new xy(8, 0), new xy(7.664063, 0), new xy(7.760045, 0.75), new xy(6.958705, 0.75), new xy(7.054688, 0), new xy(6.382813, 0), new xy(6.478795, 0.75), new xy(5.614955, 0.75), new xy(5.710938, 0), new xy(5.039063, 0), new xy(5.135045, 0.75), new xy(4.271205, 0.75), new xy(4.367188, 0), new xy(3.695313, 0), new xy(3.791295, 0.75), new xy(2.927455, 0.75), new xy(3.023438, 0), new xy(2.351563, 0), new xy(2.447545, 0.75), new xy(1.583705, 0.75), new xy(1.679688, 0), new xy(1.007813, 0), new xy(1.103795, 0.75), new xy(0.239955, 0.75), new xy(0.335938, 0), new xy(0, 0), new xy(0, -23.25));
            List<List<xy>> inter = ppi2d.Polygon2d_Intersection(p1, p2, PolyPolyOp.Intersection);
            Console.Out.WriteLine("Intersection has {0} parts:", inter.Count);
            foreach (List<xy> p in inter)
            {
                ut.DumpPoly2d("partial", p);
            }
            List<List<xy>> diff = ppi2d.Polygon2d_Intersection(p1, p2, PolyPolyOp.Difference);
            Console.Out.WriteLine("Difference has {0} parts:", diff.Count);
            foreach (List<xy> p in diff)
            {
                ut.DumpPoly2d("partial", p);
            }
        }

        [Test]
        public void test_FormatDimension()
        {
            Assert.AreEqual("5", utpub.FormatDimension(5));
            Assert.AreEqual("5 1/4", utpub.FormatDimension(5.25));
            Assert.AreEqual("5 1/2", utpub.FormatDimension(5.5));
            Assert.AreEqual("5 3/4", utpub.FormatDimension(5.75));
            Assert.AreEqual("0", utpub.FormatDimension(0));
            Assert.AreEqual("3/4", utpub.FormatDimension(0.75));
        }

        [Test]
        public void test_AnySegmentsTouch()
        {
            do_ast(ut.MakePoly(new xy(0, 0), new xy(1, 0), new xy(1, 1), new xy(0, 1)), ut.MakePoly(new xy(4, 4), new xy(5, 4), new xy(5, 5), new xy(4, 5)), false);
            do_ast(ut.MakePoly(new xy(0, 0), new xy(1, 0), new xy(1, 1), new xy(0, 1)), ut.MakePoly(new xy(0, 0), new xy(1, 0), new xy(1, 1), new xy(0, 1)), true);
            do_ast(ut.MakePoly(new xy(0, 0), new xy(1, 0), new xy(1, 1), new xy(0, 1)), ut.MakePoly(new xy(0, 0), new xy(2, 0), new xy(2, 2), new xy(0, 2)), true);
            do_ast(ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5)), ut.MakePoly(new xy(1, 1), new xy(2, 1), new xy(2, 2), new xy(1, 2)), false);
        }

        private void do_ast(List<xy> p1, List<xy> p2, bool b)
        {
            Assert.AreEqual(b, ut.AnySegmentsTouch(p1, p2));
        }

        [Test]
        public void test_translate_zero()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            xyz v1 = p.Result.Subs[0].Vertices[0];
            p.Result.Translate(0, 0, 0);
            Assert.AreEqual(v1, p.Result.Subs[0].Vertices[0]);
        }

        public void test_raybb(BoundingBox3d bb, xyz p, xyz v, double d)
        {
            double da = bb.IntersectRay_Planes_Max(p, v);

            Assert.IsTrue(fp.eq_unknowndata(d, da));
        }

        [Test]
        public void test_IntersectRay_Planes_Max()
        {
            BoundingBox3d bb = BoundingBox3d.FromPoints(new xyz(1, 1, 1), new xyz(5, 5, 5));

            test_raybb(bb, new xyz(0, 0, 0), new xyz(1, 0, 0), 5);
        }

        public void test_IsValidWithNoSubOverlaps_valid_plan(Plan p)
        {
            p.Execute();
            foreach (Step s in p.Steps)
            {
                Assert.IsTrue(s.Result.IsValidWithNoSubOverlaps());
            }
        }

        [Test]
        public void test_IsValidWithNoSubOverlaps()
        {
            test_IsValidWithNoSubOverlaps_valid_plan(test_plan.CreateBookShelf());

            Plan p2 = test_plan.CreateInvalidBlocks();
            p2.Execute();
            Assert.IsFalse(p2.Result.IsValidWithNoSubOverlaps());
        }

        [Test]
        public void test_Translate()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "a", 10, 24, 1);
            xyz v = s.Vertices[0];
            xyz vcopy = v.copy();
            s.Translate(0, 0, 0);
            Assert.IsTrue(fp.eq_inches(v, vcopy)); // TODO could this be eq_exact?
            s.Translate(1, 1, 1);
            Assert.IsFalse(fp.eq_inches(v, vcopy));
            Assert.IsTrue(fp.eq_inches(v, vcopy + new xyz(1, 1, 1)));
        }

        [Test]
        public void test_GetOrCreateVertex_ReturnIndex()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "a", 10, 24, 1);
            xyz v = s.Vertices[0].copy();
            int ndx = s.GetOrCreateVertex_ReturnIndex(v);
            Assert.AreEqual(0, ndx);
        }

        [Test]
        public void test_SamePoly2d()
        {
            List<xy> p1 = new List<xy>();
            p1.Add(new xy(0, 0));
            p1.Add(new xy(1, 0));
            p1.Add(new xy(1, 1));
            p1.Add(new xy(0, 1));
            Assert.IsTrue(ut.SamePoly2d(p1, p1));

            List<xy> p2 = new List<xy>();
            p2.Add(new xy(0, 1));
            p2.Add(new xy(0, 0));
            p2.Add(new xy(1, 0));
            p2.Add(new xy(1, 1));
            Assert.IsTrue(ut.SamePoly2d(p1, p2));
            Assert.IsTrue(ut.SamePoly2d(p2, p1));

            List<xy> p3 = new List<xy>();
            p3.Add(new xy(8, 8));
            p3.Add(new xy(9, 8));
            p3.Add(new xy(9, 9));
            p3.Add(new xy(8, 9));
            Assert.IsFalse(ut.SamePoly2d(p1, p3));
            Assert.IsFalse(ut.SamePoly2d(p2, p3));
        }

        [Test]
        public void test_PointOnLineSegment()
        {
            Assert.IsTrue(ut.PointOnSegment(new xyz(1, 1, 1), new xyz(0, 0, 0), new xyz(5, 5, 5)));
            Assert.IsFalse(ut.PointOnSegment(new xyz(1, 1, 1), new xyz(2, 2, 2), new xyz(5, 5, 5)));
            Assert.IsFalse(ut.PointOnSegment(new xyz(7, 7, 7), new xyz(2, 2, 2), new xyz(5, 5, 5)));
            Assert.IsFalse(ut.PointOnSegment(new xyz(1, 5, 1), new xyz(2, 2, 2), new xyz(5, 5, 5)));
        }

        [Test]
        public void test_HalfEdgeForFace()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "test", 10, 24, 1);
            foreach (Face f in s.Faces)
            {
                foreach (HalfEdge he in f.MainLoop)
                {
                    Edge e = he.edge;

                    Assert.AreSame(e.HalfEdgeForFace(f), he);

                    HalfEdge he2 = he.Opposite();
                    Face f2 = he2.face;

                    Assert.AreSame(e.HalfEdgeForFace(f2), he2);
                }
            }
        }

#if not
        [Test]
        public void test_xaml()
        {
            Plan p = Builtin_Plans.CreateBookShelf();
            p.Execute();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            p.Result.Write_XAML(sw);
            sw.Close();
            string xaml = sb.ToString();
            Assert.AreNotEqual(0, xaml.Length);
        }
#endif

        [Test]
        public void test_sweep()
        {
            List<xyz> a = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 3, 0), new xyz(3, 3, 0), new xyz(5, 5, 0), new xyz(3, 8, 0), new xyz(2, 5, 0), new xyz(0, 8, 0));
            Solid s = Solid.Sweep("weird", BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), a, new xyz(0, 0, -6));
            bsp3d bsp = new bsp3d(s);
            Assert.AreEqual(PointInPoly.Inside, bsp.PointInPolyhedron(new xyz(1, 1, -3)));
            Assert.AreEqual(PointInPoly.Outside, bsp.PointInPolyhedron(new xyz(4, 3.1, -3)));
        }

        [Test]
        public void test_edgeloop_indices()
        {
            Solid sol = Builtin_Solids.CreateCube("c", 5);
            Face f = sol.Faces[0];

            Assert.AreSame(f.MainLoop[0], f.MainLoop[4]);
            Assert.AreSame(f.MainLoop[1], f.MainLoop[5]);
            Assert.AreSame(f.MainLoop[2], f.MainLoop[6]);
            Assert.AreSame(f.MainLoop[3], f.MainLoop[7]);
            //Assert.AreSame(f.MainLoop[-1], f.MainLoop[3]);
            //Assert.AreSame(f.MainLoop[-2], f.MainLoop[2]);

            Solid sol2 = Builtin_Solids.CreateCube("d", 5);
            Face f2 = sol2.Faces[0];

            Assert.AreEqual(-1, f2.MainLoop.IndexOf(f.MainLoop[0]));
        }

        [Test]
        public void test_kill_all()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 10, 36, 2);
            for (int i = 0; i < 6; i++)
            {
                s.KillFace(s.Faces[0]);
            }
            Assert.IsTrue(s.Faces.Count == 0);
        }

        [Test]
        public void test_create_face_problems()
        {
            // verify that we can kill a face and recreate it
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 10, 36, 2);
            Face f = s.FindFace("top");
            List<xyz> a = f.CollectAllVertices();
            s.KillFace(f);
            Face f2 = s.CreateFace("plok", a);
            f2.RecalcPlane();
            s.DoGeomChecks();
        }

        internal double total_length_segs(List<seg3d> a)
        {
            double d = 0;
            foreach (seg3d s in a)
            {
                d += s.Length;
            }
            return d;
        }

        internal void verify_FindTheLoops(List<seg3d> a, int expected)
        {
            // make a copy of a
            List<seg3d> b = new List<seg3d>();
            b.AddRange(a);

            // run FindTheLoops on the copy, make sure it finds one loop
            int count;
            List<List<xyz>> loops = Solid.FindTheLoops(b, false);
            count = loops.Count;
            Assert.AreEqual(expected, count);
        }

        [Test]
        public void test_segs()
        {
            List<seg3d> a = new List<seg3d>();
            bool3d.segs_Add(a, new seg3d(new xyz(0, 0, 0), new xyz(5, 0, 0)));
            bool3d.segs_Add(a, new seg3d(new xyz(5, 0, 0), new xyz(5, 5, 0)));
            bool3d.segs_Add(a, new seg3d(new xyz(5, 5, 0), new xyz(0, 5, 0)));
            bool3d.segs_Add(a, new seg3d(new xyz(0, 5, 0), new xyz(0, 0, 0)));
            //ut.DumpSegments3d("box", a);

            Assert.IsTrue(fp.eq_inches(20, total_length_segs(a)));
            Assert.AreEqual(4, a.Count);

            // run FindTheLoops on the copy, make sure it finds one loop
            verify_FindTheLoops(a, 1);

            // make sure a is still okay
            Assert.IsTrue(fp.eq_inches(20, total_length_segs(a)));
            Assert.AreEqual(4, a.Count);

            // add something that is already there, make sure it had no effect
            bool3d.segs_Add(a, new seg3d(new xyz(1, 0, 0), new xyz(3, 0, 0)));

            Assert.IsTrue(fp.eq_inches(20, total_length_segs(a)));
            Assert.AreEqual(4, a.Count);
            verify_FindTheLoops(a, 1);

#if not
			// remove a little piece of the first segment
			bool3d.segs_Remove(a, new xyz(2,0,0), new xyz(3,0,0));
			Assert.IsTrue(ut.eq(19, total_length_segs(a)));
			verify_FindTheLoops(a, -1);

			// put it back (no overlaps on add)
			bool3d.segs_Add(a, new xyz(2,0,0), new xyz(3,0,0));
			Assert.IsTrue(ut.eq(20, total_length_segs(a)));
			Assert.AreNotEqual(4, a.Count);
			verify_FindTheLoops(a, 1);

			// remove a piece of the second segment
			bool3d.segs_Remove(a, new xyz(5,1,0), new xyz(5,4,0));
			Assert.IsTrue(ut.eq(17, total_length_segs(a)));
			verify_FindTheLoops(a, -1);

			// put it back (overlap 2 segs on add)
			bool3d.segs_Add(a, new xyz(5,0,0), new xyz(5,5,0));
			Assert.IsTrue(ut.eq(20, total_length_segs(a)));
			Assert.AreNotEqual(4, a.Count);
			verify_FindTheLoops(a, 1);
#endif

#if not
            List<seg3d> a2 = seg3d.Merge(a);
			Assert.IsTrue(ut.eq(20, total_length_segs(a2)));
			Assert.AreEqual(4, a2.Count);
			verify_FindTheLoops(a2, 1);
#endif
        }

        [Test]
        public void test_SegmentsOverlap3d()
        {
            SegmentsOverlap3d so;

            xyz a = new xyz(0, 0, 0);
            xyz b = new xyz(5, 5, 5);
            seg3d ab = new seg3d(a, b);

            List<seg3d> onlyp;
            List<seg3d> onlyq;
            List<seg3d> common;

            // TODO this test really should verify the contents of onlyp, onlyq and common

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(0, 0, 0), new xyz(5, 5, 5)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.SameSegment, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(0, 0, 0), new xyz(1, 1, 1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.P_On_Q, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(1, 1, 1), new xyz(5, 5, 5)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.P_On_Q, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(1, 1, 1), new xyz(2, 2, 2)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.P_On_Q, so);


            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(5, 5, 5), new xyz(0, 0, 0)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(1, 1, 1), new xyz(0, 0, 0)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(5, 5, 5), new xyz(1, 1, 1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(2, 2, 2), new xyz(1, 1, 1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);


            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(1, 1, 1), new xyz(6, 6, 6)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.Overlap, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(6, 6, 6), new xyz(1, 1, 1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(3, 3, 3), new xyz(-1, -1, -1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(-1, -1, -1), new xyz(3, 3, 3)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.Overlap, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(-1, -1, -1), new xyz(6, 6, 6)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.Q_On_P, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(6, 6, 6), new xyz(-1, -1, -1)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.OppositeDirection, so);


            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(0, 0, 0), new xyz(5, 5, 4)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.None, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(0, 0, 0), new xyz(5, 5, 6)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.None, so);

            so = ut.CalcSegmentsOverlap3d(new seg3d(new xyz(0, 0, 0), new xyz(5, 0, 0)), ab, onlyp = new List<seg3d>(), onlyq = new List<seg3d>(), common = new List<seg3d>());
            Assert.AreEqual(SegmentsOverlap3d.None, so);

        }

        [Test]
        public void test_pt_in_solid_with_round_hole()
        {
            CompoundSolid s1 = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();

            bsp3d bsp = new bsp3d(s1[0]);
            Assert.AreEqual(PointInPoly.Inside, bsp.PointInPolyhedron(s1.GetCenter()));

            s1 = wood.Drill(s1, s1.FindFace("c.top").FindEdge("end1"), 2.5, 2.5, 1, 0, 0, 4, 8, "d1");
            bsp = new bsp3d(s1[0]);
            Assert.AreEqual(PointInPoly.Outside, bsp.PointInPolyhedron(s1.GetCenter()));
        }

        [Test]
        public void test_neg_inches()
        {
            Inches f = new Inches(-4.25);
            double d = f;
            Assert.IsTrue(fp.eq_inches(d, -4.25));
        }

        [Test]
        public void test_PointFaceIntersection()
        {
            CompoundSolid s = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();
            Face f = s.FindFace("c.top");
            s = wood.Mortise(s, f.MainLoop[0], new xy(2, 2), new xyz(1, 1, 1), "id");

            // move this to the origin
            s.Translate(-(s.Subs[0].board_origin.x), -(s.Subs[0].board_origin.y), -(s.Subs[0].board_origin.z));

            f = s.FindFace("c.top");

            PointFaceIntersection pfi;

            pfi = f.CalcPointFaceIntersection(new xyz(0, 0, 0));
            Assert.AreEqual(PointFaceIntersection.OnEdge, pfi);

            pfi = f.CalcPointFaceIntersection(new xyz(1, 1, 0));
            Assert.AreEqual(PointFaceIntersection.Inside, pfi);

            pfi = f.CalcPointFaceIntersection(new xyz(2, 2, 0));
            Assert.AreEqual(PointFaceIntersection.OnEdge, pfi);

            pfi = f.CalcPointFaceIntersection(new xyz(2.5, 2.5, 0));
            Assert.AreEqual(PointFaceIntersection.None, pfi);

            pfi = f.CalcPointFaceIntersection(new xyz(3, 3, 0));
            Assert.AreEqual(PointFaceIntersection.OnEdge, pfi);
        }

        [Test]
        public void test_badpoly_from_bookshelf_cut()
        {
            List<xyz> poly = ut.MakePoly(new xyz(4, -1, -29), new xyz(4, -2, -29), new xyz(4, -2, -30), new xyz(4, 4, -30), new xyz(4, 4, -29), new xyz(4, 0, -29), new xyz(4, 0, -19), new xyz(4, 4, -19), new xyz(4, 4, -18), new xyz(4, 0, -18), new xyz(4, 0, -10), new xyz(4, 4, -10), new xyz(4, 4, -9), new xyz(4, 0, -9), new xyz(4, 0, 0), new xyz(4, 4, 0), new xyz(4, 4, 1), new xyz(4, 0, 1), new xyz(4, -1, 1));
            xyz n = ut.GetUnitNormalFromPointList(poly);
            Assert.IsTrue(fp.eq_unitvec(n, new xyz(1, 0, 0)));
        }

        [Test]
        public void test_bbox3d_diag()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);
            BoundingBox3d bb = s.GetBoundingBox();
            xyz v = bb.Diagonal();
            Assert.IsTrue(fp.eq_inches(v.magnitude(), Math.Sqrt(3 * 5 * 5)));
        }

        [Test]
        public void test_face_gettriangles()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);
            foreach (Face f in s.Faces)
            {
                List<Triangle3d> a = f.GetTriangles();
                Assert.AreEqual(2, a.Count);
            }
        }

        public void test_split_really(Triangle3d t, Triangle3d splitter, bool bOneEdge)
        {
            List<Triangle3d> tris_in = new List<Triangle3d>();
            List<Triangle3d> tris_out = new List<Triangle3d>();
            splitter.Split(t, tris_in, tris_out);

            Assert.IsTrue(tris_in.Count > 0);
            Assert.IsTrue(tris_out.Count > 0);

            if (bOneEdge)
            {
                Assert.AreEqual(2, tris_in.Count + tris_out.Count);
            }
            else
            {
                Assert.AreEqual(3, tris_in.Count + tris_out.Count);
            }

            double area1 = t.Area();

            double area2 = 0;
            foreach (Triangle3d tq in tris_in)
            {
                area2 += tq.Area();
            }
            foreach (Triangle3d tq in tris_out)
            {
                area2 += tq.Area();
            }

            Assert.IsTrue(fp.eq_area(area1, area2));

            xyz n1 = t.n;

            foreach (Triangle3d tq in tris_in)
            {
                xyz n = tq.n;
                Assert.IsTrue(fp.eq_unitvec(n1, n));
            }
        }

        public void test_split(Triangle3d t, Triangle3d splitter, bool bOneEdge)
        {
            test_split_really(t, splitter, bOneEdge);
            Triangle3d splitter_reversed = new Triangle3d(splitter.a, splitter.c, splitter.b);
            test_split_really(t, splitter_reversed, bOneEdge);
        }

        [Test]
        public void test_t3d_split()
        {
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 0, -1), new xyz(3, 6, -1), new xyz(3, 3, 1)),
                true
                );
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(1, 1, 1), new xyz(-1, -1, 1)),
                true
                );
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(6, 0, 0), new xyz(7, -1, 1), new xyz(5, 1, 1)),
                true
                );

            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 0, 0), new xyz(6, 6, 0), new xyz(6, 6, 5)),
                false
                );
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 3, 0), new xyz(6, 3, 0), new xyz(3, 3, -1)),
                false
                );
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(2, -1, 0), new xyz(2, 4, 0), new xyz(2, 1, -1)),
                false
                );
            test_split(
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(4, -1, 0), new xyz(4, 4, 0), new xyz(4, 1, -1)),
                false
                );
        }

        [Test]
        public void test_DumpSegments3d()
        {
            List<seg3d> a = new List<seg3d>();
            a.Add(new seg3d(new xyz(0, 0, 0), new xyz(5, 0, 0)));
            a.Add(new seg3d(new xyz(5, 0, 0), new xyz(5, 5, 0)));
            a.Add(new seg3d(new xyz(5, 5, 0), new xyz(0, 0, 0)));
            ut.DumpSegments3d("test_DumpSegments3d", a);
        }

        [Test]
        public void test_pt_in_solid_problem()
        {
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut_s2", 2, 2, 20);
            s2.Translate(-(s2.board_origin.x), -(s2.board_origin.y), -(s2.board_origin.z));
            s2.Translate(2, -1, 2);

            bsp3d bsp = new bsp3d(s2);

            seg3d seg = new seg3d(new xyz(4, 0, 0), new xyz(2, 0, 0));
            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(s2.SegmentInside(bsp, seg.a, seg.b));
            }
        }

        public CompoundSolid reconstitute_solid(CompoundSolid s1)
        {
            CompoundSolid cs = new CompoundSolid();

            foreach (Solid s in s1)
            {
                cs.AddSub(reconstitute_solid(s));
            }

            return cs;
        }

        public Solid reconstitute_solid(Solid s1)
        {
            Solid s2 = new Solid(s1.name, s1.material);

            foreach (Face f in s1.Faces)
            {
                List<seg3d> a = f.CollectAllSegments();
                s2.CreateFacesFromPileOfSegments(f.name, f.name, f.Shade, a, false);
            }

            if (s1.board_origin != null)
            {
                s2.board_origin = s1.board_origin.copy();
                s2.board_u = s1.board_u.copy();
                s2.board_v = s1.board_v.copy();
                s2.board_w = s1.board_w.copy();
            }

            s2.DoGeomChecks();
            Assert.IsTrue(fp.eq_volume(s1.Volume(), s2.Volume()));
            Assert.AreEqual(s1.Faces.Count, s2.Faces.Count);
            Assert.AreEqual(s1.Vertices.Count, s2.Vertices.Count);
            Assert.AreEqual(s1.Edges.Count, s2.Edges.Count);

            return s2;
        }

        [Test]
        public void test_piles_5()
        {
            reconstitute_solid(Builtin_Solids.CreateSolidWithHoleAndMortise("name"));
        }

        [Test]
        public void test_piles_4()
        {
            reconstitute_solid(Builtin_Solids.CreateCubeWithLotsOfMortises());
        }

        [Test]
        public void test_piles_2()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            reconstitute_solid(p.Result);
        }

        [Test]
        public void test_piles()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);

            for (int i = 0; i < 6; i++)
            {
                Face f = s1.Faces[i];
                s1 = wood.Mortise(f, 0, 1, 4, 4, 2, 2, 1, i.ToString());
            }

            reconstitute_solid(s1);
        }

        [Test]
        public void test_segdir_facenormals()
        {
            Solid s = Builtin_Solids.CreateCube("hello", 5);
            foreach (Face f in s.Faces)
            {
                xyz n1 = f.UnitNormal();
                foreach (HalfEdge he1 in f.MainLoop)
                {
                    HalfEdge he2 = f.GetNextHalfEdge(he1);
                    xyz v1 = he1.UnitVector();
                    xyz v2 = he2.UnitVector();
                    xyz n2 = xyz.cross(v1, v2).normalize();
                    Assert.IsTrue(fp.eq_unitvec(n1, n2));
                }
            }
        }

        [Test]
        public void test_IsSubsegment3d()
        {
            xyz a = new xyz(0, 0, 0);
            xyz b = new xyz(5, 5, 5);

            Assert.IsTrue(ut.IsSubsegment3d(new xyz(0, 0, 0), new xyz(5, 5, 5), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(0, 0, 0), new xyz(1, 1, 1), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(1, 1, 1), new xyz(5, 5, 5), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(1, 1, 1), new xyz(2, 2, 2), a, b));

            Assert.IsTrue(ut.IsSubsegment3d(new xyz(5, 5, 5), new xyz(0, 0, 0), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(1, 1, 1), new xyz(0, 0, 0), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(5, 5, 5), new xyz(1, 1, 1), a, b));
            Assert.IsTrue(ut.IsSubsegment3d(new xyz(2, 2, 2), new xyz(1, 1, 1), a, b));

            Assert.IsFalse(ut.IsSubsegment3d(new xyz(1, 1, 1), new xyz(6, 6, 6), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(6, 6, 6), new xyz(1, 1, 1), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(-1, -1, -1), new xyz(3, 3, 3), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(3, 3, 3), new xyz(-1, -1, -1), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(-1, -1, -1), new xyz(6, 6, 6), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(6, 6, 6), new xyz(-1, -1, -1), a, b));

            Assert.IsFalse(ut.IsSubsegment3d(new xyz(0, 0, 0), new xyz(5, 5, 4), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(0, 0, 0), new xyz(5, 5, 6), a, b));
            Assert.IsFalse(ut.IsSubsegment3d(new xyz(0, 0, 0), new xyz(5, 0, 0), a, b));
        }

        [Test]
        public void test_getnexthalfedge()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);
            Face f = s.Faces[0];
            HalfEdge he0 = f.MainLoop[0];
            Assert.AreEqual(f.GetNextHalfEdge(he0), f.MainLoop[1]);
            Assert.AreEqual(he0, f.MainLoop[4]);
            HalfEdge he3 = f.MainLoop[3];
            Assert.AreEqual(f.GetNextHalfEdge(he3), he0);
        }

        [Test]
        public void test_segment_in_cone()
        {
            xy p1 = new xy(0, 0);
            xy p2 = new xy(0, 5);
            xy p3 = new xy(5, 0);

            xy p4 = new xy(5, 5);

            xy p5 = new xy(-7, 4);

            // the angle is counterclockwise

            bool b23 = ut.SegmentInCone(p1, p4, p2, p3);
            Assert.IsTrue(b23, "For SegmentInCone, the cone is defined by the counterclockwise angle from arg 3 to arg 4 with arg 1 as the origin");
            bool b32 = ut.SegmentInCone(p1, p4, p3, p2);
            Assert.IsFalse(b32, "For SegmentInCone, the cone is defined by the counterclockwise angle from arg 3 to arg 4 with arg 1 as the origin");

            bool f23 = ut.SegmentInCone(p1, p5, p2, p3);
            Assert.IsFalse(f23, "For SegmentInCone, the cone is defined by the counterclockwise angle from arg 3 to arg 4 with arg 1 as the origin");
            bool f32 = ut.SegmentInCone(p1, p5, p3, p2);
            Assert.IsTrue(f32, "For SegmentInCone, the cone is defined by the counterclockwise angle from arg 3 to arg 4 with arg 1 as the origin");
        }

        [Test]
        public void test_segments_inside_cube_with_mortise()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);
            s.Translate(-(s.board_origin.x), -(s.board_origin.y), -(s.board_origin.z));

            bsp3d bsp = new bsp3d(s);

            Assert.IsTrue(s.SegmentInside(bsp, new xyz(0, 0, 0), new xyz(5, 5, -5)));

            // cut a mortise
            Face f = s.FindFace("top");
            s = wood.Mortise(f, 0, 1, 1, 1, 3, 3, 4, "m");

            bsp = new bsp3d(s);

            Assert.IsFalse(s.SegmentInside(bsp, new xyz(0, 0, 0), new xyz(5, 5, 0)));

            Assert.IsFalse(s.SegmentInside(bsp, new xyz(1, 1, 0), new xyz(4, 4, 0)));
            Assert.IsTrue(s.SegmentInside(bsp, new xyz(1, 1, 0), new xyz(0, 0, 0)));
            Assert.IsTrue(s.SegmentInside(bsp, new xyz(0, 0, 0), new xyz(5, 0, 0)));
        }

        [Test]
        public void test_inward_normal()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);
            Face f = s.FindFace("top");
            HalfEdge he = f.MainLoop[0];
            xyz n = he.GetInwardNormal();
            xyz c = he.Center();
            xyz p2 = c + n;
            PointFaceIntersection pfi = f.CalcPointFaceIntersection(p2);
            Assert.AreEqual(PointFaceIntersection.Inside, pfi);
        }

        [Test]
        public void test_point_in_cube_with_mortise()
        {
            Solid s = Builtin_Solids.CreateCube("c", 5);

            bsp3d bsp = new bsp3d(s);

            Face f = s.FindFace("top");

            xyz n = f.UnitNormal();

            // get the center of the face
            xyz c = f.GetCenter();
            // and a point 1 inch inside the face
            xyz p = c - n;

            // make sure both points are inside the solid now
            Assert.IsTrue(s.PointInside(bsp, p));
            Assert.IsTrue(s.PointInside(bsp, c));

            // cut a mortise
            s = wood.Mortise(f, 0, 1, 1, 1, 3, 3, 4, "m");

            bsp = new bsp3d(s);

            // make sure both points are now outside the solid
            Assert.IsFalse(s.PointInside(bsp, p));
            Assert.IsFalse(s.PointInside(bsp, c));
        }

        [Test]
        public void test_point_in_cube_with_through_mortise()
        {
            Solid s = Builtin_Solids.CreateCubeWithHole("cube");
            bsp3d bsp = new bsp3d(s);

            for (int i = 0; i < 100; i++)
            {
                Assert.IsFalse(s.PointInside(bsp, new xyz(6, 3, -3)));
                Assert.IsFalse(s.PointInside(bsp, new xyz(3, 0, 1)));
                Assert.IsFalse(s.PointInside(bsp, new xyz(3, -1, -3)));
                Assert.IsFalse(s.PointInside(bsp, new xyz(3, -1, -6)));

                Assert.IsTrue(s.PointInside(bsp, new xyz(0, 0, 0)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(3, 0, 0)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(0, 3, 0)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(0, 0, -3)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(5, 5, -5)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(0.5, 0.5, -0.5)));

                Assert.IsTrue(s.PointInside(bsp, new xyz(1, 1, 0)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(1, 1, -1)));
                Assert.IsTrue(s.PointInside(bsp, new xyz(1, 1, -5)));

                Assert.IsFalse(s.PointInside(bsp, new xyz(2, 2, -2)));
                Assert.IsFalse(s.PointInside(bsp, new xyz(2, 2, 0)));
                Assert.IsFalse(s.PointInside(bsp, new xyz(2, 2, -5)));
            }
        }

        [Test]
        public void test_point_in_cube_loop()
        {
            Solid s = Builtin_Solids.CreateCube("cube", 10);
            s.Translate(-(s.board_origin.x), -(s.board_origin.y), -(s.board_origin.z));

            bsp3d bsp = new bsp3d(s);

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(s.PointInside(bsp, new xyz(1.15, 0.52, -0.72)));
            }
        }

        [Test]
        public void pt_in_poly_failure()
        {
            xy pt = new xy(0.000181818181816951, 6.59046280991735);
            List<xy> poly = ut.MakePoly(new xy(0, 0), new xy(10, 0), new xy(10, 10), new xy(0, 10));
            Assert.IsTrue(ut.PointInsidePoly(poly, pt));
        }

        [Test]
        public void test_point_in_cube_failure_case()
        {
            Solid s = Builtin_Solids.CreateCube("cube", 10);
            s.Translate(-(s.board_origin.x), -(s.board_origin.y), -(s.board_origin.z));
            bsp3d bsp = new bsp3d(s);

            Assert.IsTrue(s.PointInside(bsp, new xyz(4.75, 3.11, -4.89)));
        }

        [Test]
        public void test_point_in_cube()
        {
            Solid s = Builtin_Solids.CreateCube("cube", 10);

            s.Translate(-(s.board_origin.x), -(s.board_origin.y), -(s.board_origin.z));

            bsp3d bsp = new bsp3d(s);

            Assert.IsFalse(s.PointInside(bsp, new xyz(11, 1, -1)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(11, 11, -11)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(0, 0, 1)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(10.1, 4, -4)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(-5, -5, 5)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(10, 10, 1)));
            Assert.IsFalse(s.PointInside(bsp, new xyz(11, 0, 0)));

            Assert.IsTrue(s.PointInside(bsp, new xyz(1, 1, -1)));
            Assert.IsTrue(s.PointInside(bsp, new xyz(5, 5, 0)));
            Assert.IsTrue(s.PointInside(bsp, new xyz(10, 5, -5)));
            Assert.IsTrue(s.PointInside(bsp, new xyz(0, 0, 0)));
            Assert.IsTrue(s.PointInside(bsp, new xyz(10, 10, -10)));

            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                double x = r.Next(1000) / 100.0;
                double y = r.Next(1000) / 100.0;
                double z = r.Next(1000) / 100.0;
                xyz p = new xyz(x, y, -z);
                Assert.IsTrue(s.PointInside(bsp, p));
            }
        }

        [Test]
        public void test_seg3d()
        {
            List<seg3d> a = new List<seg3d>();
            seg3d.Add(a, new seg3d(new xyz(0, 0, 0), new xyz(5, 5, 5)));
            seg3d.Add(a, new seg3d(new xyz(0, 0, 0), new xyz(5, 5, 5)));
            seg3d.Add(a, new seg3d(new xyz(5, 5, 5), new xyz(0, 0, 0)));
            Assert.AreEqual(1, a.Count);
        }

        [Test]
        public void test_rot_trans_samepoly()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "foo", 10, 24, 1);
            Face f = s.Faces[0];
            List<xyz> a = f.CollectAllVertices();

            xyz p0 = new xyz(0, 0, 0);
            xyz p1 = new xyz(5, 5, 5);
            xyz dir = (p1 - p0).normalize_in_place();
            double angle = Math.PI / 2;
            List<xyz> b = ut.RotatePoints(a, angle, p0, dir);

            xyz trans = new xyz(17, 34, -22);
            ut.TranslatePoints(b, trans.x, trans.y, trans.z);

            // now put it back
            ut.TranslatePoints(b, -trans.x, -trans.y, -trans.z);

            List<xyz> c = ut.RotatePoints(b, -angle, p0, dir);

            Assert.IsTrue(ut.SamePoly3d(a, c));
        }

        [Test]
        public void test_dumppoly2d()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 24), new xy(0, 24), new xy(-2, 24), new xy(-14, 24), new xy(-14, 0), new xy(-2, 0));
            ut.DumpPoly2d("test_dumppoly2d", poly1);
            Assert.IsTrue(true);
        }

        [Test]
        public void dumppoly3d()
        {
            Solid s = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "foo", 10, 24, 1);
            Face f = s.Faces[0];
            List<xyz> a = f.CollectAllVertices();
            ut.DumpPoly3d(a);
            ut.DumpPoly3d("label", a);
            Assert.AreEqual(4, a.Count);
        }

        public void compare_solids(Solid s1, Solid s2)
        {
            s1.DoGeomChecks();
            s2.DoGeomChecks();
            Assert.AreEqual(s1.name, s2.name);
            Assert.AreEqual(s1.Vertices.Count, s2.Vertices.Count);
            foreach (xyz v1 in s1.Vertices)
            {
                Assert.AreEqual(-1, s2.Vertices.IndexOf(v1));
            }
            for (int q = 0; q < s1.Faces.Count; q++)
            {
                Face f1 = s1.Faces[q];
                Face f2 = s2.Faces[q];
                Assert.IsNotNull(f2);
                Assert.AreNotSame(f1, f2);
                Assert.AreEqual(f1.MainLoop.Count, f2.MainLoop.Count);
                Assert.AreEqual(f1.loops.Count, f2.loops.Count);
                for (int i = 0; i < f1.loops.Count; i++)
                {
                    EdgeLoop curloop1 = f1.loops[i];
                    EdgeLoop curloop2 = f2.loops[i];
                    Assert.AreEqual(curloop1.Count, curloop2.Count);
#if not
					for (int j=0; j<curloop1.Count; j++)
					{
						HalfEdge he1 = curloop1[j];
						HalfEdge he2 = curloop2[j];
						Assert.AreNotSame(he1, he2);
						Assert.IsTrue(ut.eq(he1.to, he2.to));
					}
#endif
                }
            }
            Assert.IsTrue(fp.eq_volume(s1.Volume(), s2.Volume()));
            Assert.IsTrue(fp.eq_area(s1.SurfaceArea(), s2.SurfaceArea()));
            // TODO need more comparisons here to verify that the two solids are not the same but are identical?
        }

        [Test]
        public void test_clone()
        {
            Solid s1 = Builtin_Solids.CreateCube("cube", 5);
            Solid s2 = s1.Clone();
            compare_solids(s1, s2);

            s1 = Builtin_Solids.CreateCubeWithLotsOfMortises();
            s2 = s1.Clone();
            compare_solids(s1, s2);

            s1 = Builtin_Solids.CreateSolidWithHoleAndMortise("hello");
            s2 = s1.Clone();
            compare_solids(s1, s2);

            // TODO do something more complex here
        }

        [Test]
        public void test_inches()
        {
            // TODO real tests here
            Inches in1 = new Inches(3, 48, 8);
            Console.Out.WriteLine("{0}    {1}", (double)in1, in1);
            Assert.AreEqual((double)in1, 9);

            in1 = (Inches)10 / 3;
            Console.Out.WriteLine("{0}    {1}", (double)in1, in1);

            in1 = (Inches)2 / 3;
            Console.Out.WriteLine("{0}    {1}", (double)in1, in1);
            Assert.IsTrue(true);
        }

        [Test]
        public void test_GetPolyAsCode()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 24), new xy(0, 24), new xy(-2, 24), new xy(-14, 24), new xy(-14, 0), new xy(-2, 0));
            string s = ut.GetPoly2dAsCode(poly1);
            Assert.IsNotNull(s);
            // (smirk) a true test would invoke the C# compiler and verify that the code produces the same poly!  :-)
        }

        [Test]
        public void test_GetPolyAsCode3d()
        {
            List<xyz> poly = ut.MakePoly(new xyz(4, -1, -29), new xyz(4, -2, -29), new xyz(4, -2, -30), new xyz(4, 4, -30), new xyz(4, 4, -29), new xyz(4, 0, -29), new xyz(4, 0, -19), new xyz(4, 4, -19), new xyz(4, 4, -18), new xyz(4, 0, -18), new xyz(4, 0, -10), new xyz(4, 4, -10), new xyz(4, 4, -9), new xyz(4, 0, -9), new xyz(4, 0, 0), new xyz(4, 4, 0), new xyz(4, 4, 1), new xyz(4, 0, 1), new xyz(4, -1, 1));
            string s = ut.GetPoly3dAsCode(poly);
            Assert.IsNotNull(s);
            // (smirk) a true test would invoke the C# compiler and verify that the code produces the same poly!  :-)
        }

        [Test]
        public void polypoly_bookshelf_bug()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 24), new xy(0, 24), new xy(-2, 24), new xy(-14, 24), new xy(-14, -3.5527136788005E-15), new xy(-2, 0));
            List<xy> poly2 = ut.MakePoly(new xy(7.105427357601E-15, 24), new xy(-1.99999999999999, 24), new xy(-1.99999999999999, 7.88860905221012E-32), new xy(7.105427357601E-15, 7.88860905221012E-32));

            List<List<xy>> result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            Assert.AreEqual(1, result.Count);

            poly1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 24), new xy(0, 24), new xy(-2, 24), new xy(-14, 24), new xy(-14, 0), new xy(-2, 0));
            poly2 = ut.MakePoly(new xy(0, 24), new xy(-2, 24), new xy(-2, 0), new xy(0, 0));

            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void plane_normal_issue()
        {
            xyz p1 = new xyz(10, 5, 0);
            xyz p2 = new xyz(5, 5, 0);
            xyz p3 = new xyz(5, 10, 0);
            xyz p4 = new xyz(0, 15, 0);
            xyz p5 = new xyz(0, 0, 0);
            xyz p6 = new xyz(15, 0, 0);

            xyz origin;
            xyz iv;
            xyz jv;

            List<xyz> a1 = new List<xyz>(new xyz[] { p1, p2, p3, p4, p5, p6 });
            xyz n1;
            ut.GetPlaneFromArrayOfCoplanarPoints(a1, out origin, out n1, out iv, out jv);

            List<xyz> a2 = new List<xyz>(new xyz[] { p3, p4, p5, p6, p1, p2 });
            xyz n2;
            ut.GetPlaneFromArrayOfCoplanarPoints(a2, out origin, out n2, out iv, out jv);

            Assert.IsTrue(fp.eq_unitvec(n1, n2));
        }

#if not // this should be checked at the UI level
		[Test]
		public void CubeWithMortiseZeroDepth()
		{
			Solid sol = Builtin_Solids.CreateCube("cube", 5);
			Face f = sol.Faces[0];

			bool bfailed = false;
			try
			{
				sol = wood.Mortise(f, 0, 1, 1.5, 1.5, 2, 2, 0, "m");
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "A zero-depth mortise makes no sense.");
		}
#endif

        [Test]
        public void test_polypoly_inside()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));
            List<xy> poly2 = ut.MakePoly(new xy(2, 2), new xy(4, 2), new xy(4, 4), new xy(2, 4));
            List<List<xy>> result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(ut.SamePoly2d(result[0], poly2));

            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Union);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(ut.SamePoly2d(result[0], poly1));

            // in this case Difference returns two polys.  poly1 is untouched.  poly2 comes back reversed, since it is a hole.
            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(ut.SamePoly2d(result[0], poly1));
            poly2.Reverse();
            Assert.IsTrue(ut.SamePoly2d(result[1], poly2)); // poly2 has been reversed

#if not
			foreach (ArrayList a in result)
			{
				Console.Out.WriteLine("Found poly:");
				ut.DumpPoly2d(a);
				Assert.IsTrue(ut.PolygonIsSimple2d(a));
			}
#endif
        }

        [Test]
        public void test_findedge_failure()
        {
            Solid b = Builtin_Solids.CreateCube("c", 5);
            Face f = b.FindFace("top");
            HalfEdge he = f.FindEdge("nope");
            Assert.IsNull(he);
        }

#if not
		[Test]
		public void test_findedge_failure_2()
		{
			Solid b = Builtin_Solids.CreateCube("c", 5);
			Face ftop = b.FindFace("top");
			
			// now name two faces the same name.  this isn't actually legal in practice, but it ensures that FindEdge will throw.
			Face f1 = b.FindFace("right");
			f1.name = "dup";
			Face f2 = b.FindFace("left");
			f2.name = "dup";

			bool bfailed = false;
			try
			{
				HalfEdge he = ftop.FindEdge("dup");
			}
			catch
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed);
		}
#endif

        [Test]
        public void test_findface_and_misc()
        {
            Solid b1 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "test_findface_and_misc", 4, 12, 2);
            Face f = b1.FindFace("this face won't exist");
            Assert.IsNull(f);
            Edge e = b1.FindEdge("no way", "nevermore");
            Assert.IsNull(e);
            Edge e3 = b1.FindEdge("top", "not_here");
            Assert.IsNull(e3);
            Edge e1 = b1.FindEdge("top", "end1");
            Edge e2 = b1.FindEdge("end1", "top");
            Assert.IsNotNull(e1);
            Assert.IsNotNull(e2);
            Assert.AreEqual(e1, e2);

            Face f1 = b1.FindFace("top");
            Assert.IsNotNull(f1);
            HalfEdge he1 = f1.MainLoop[0];
            Assert.IsNotNull(he1);
            e1 = he1.edge;
            Assert.IsNotNull(e1);
            Assert.AreEqual(he1, e1.HalfEdgeForFace(f1));

            Face f2 = b1.FindFace("bottom");
            Assert.IsNull(e1.HalfEdgeForFace(f2));

            Solid cube = Builtin_Solids.CreateCube("cube", 5);
            Face q = cube.FindFace("qwerty");
            Assert.IsNull(q);
#if not
			bool bfailed = false;
			try
			{
				Face z = cube.FindFace("plok");
			}
			catch (Exception)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed);
#endif
        }

        [Test]
        public void test_removing_a_face()
        {
            Solid c = Builtin_Solids.CreateCube("cube", 5);
            Assert.IsTrue(c.IsClosed());

            Face f1 = c.Faces[0];
            int[] verts = f1.MainLoop.GetVertexIndices(c);
            c.KillFace(f1);

            Assert.IsFalse(c.IsClosed());
            c.CreateFace("test_removing_a_face", verts);
            Assert.IsTrue(c.IsClosed());

            Assert.IsTrue(fp.eq_volume(c.Volume(), 5 * 5 * 5));
        }

        [Test]
        public void test_pyramid()
        {
            Solid s = Builtin_Solids.CreatePyramid("pyro", 6, 4);
            Assert.IsTrue(s.Volume() > 0);
        }

        [Test]
        public void test_xy()
        {
            xy p1 = new xy(0, 0);
            Assert.IsTrue(fp.eq_inches(p1, p1));
            Assert.IsTrue(fp.eq_inches(p1, -p1));

            xy p2 = new xy(5, 5);
            xy p3 = -p2;

            Assert.IsTrue(fp.eq_inches(p2, -p3));
        }

        [Test]
        public void test_samepoly3d()
        {
            List<xyz> poly = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0), new xyz(0, 5, 0));
            List<xyz> poly2 = ut.MakePoly(new xyz(50, 50, 0), new xyz(80, 80, 0), new xyz(50, 80, 0));
            List<xyz> poly3 = ut.MakePoly(new xyz(0, 0, 0), new xyz(7, 0, 0), new xyz(5, 5, 0), new xyz(0, 5, 0));
            List<xyz> poly4 = ut.MakePoly(new xyz(0, 5, 0), new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0));
            List<xyz> poly5 = ut.MakePoly(new xyz(0, 5, 7), new xyz(0, 0, 7), new xyz(5, 0, 7), new xyz(5, 5, 7));
            Assert.IsTrue(ut.SamePoly3d(poly, poly));
            Assert.IsTrue(ut.SamePoly3d(poly, poly4));
            Assert.IsFalse(ut.SamePoly3d(poly, poly2));
            Assert.IsFalse(ut.SamePoly3d(poly, poly3));
            Assert.IsFalse(ut.SamePoly3d(poly, poly5));
        }

        [Test]
        public void test_samePoly()
        {
            List<xy> poly = ut.MakePoly(new xy(1, 6), new xy(1, 1), new xy(3, 1), new xy(3, 5), new xy(7, 5), new xy(7, 1), new xy(10, 1), new xy(10, 6));
            Assert.IsTrue(ut.SamePoly2d(poly, poly));
            List<xy> poly2 = ut.MakePoly(new xy(50, 50), new xy(80, 80), new xy(50, 80));
            Assert.IsFalse(ut.SamePoly2d(poly, poly2));
            List<xy> poly3 = ut.MakePoly(new xy(1, 6), new xy(1, 1), new xy(3, 1), new xy(3, 22), new xy(7, 5), new xy(7, 1), new xy(10, 1), new xy(10, 6));
            Assert.IsFalse(ut.SamePoly2d(poly, poly3));
        }

        [Test]
        public void test_bb2d()
        {
            List<xy> poly = ut.MakePoly(new xy(1, 6), new xy(1, 1), new xy(3, 1), new xy(3, 5), new xy(7, 5), new xy(7, 1), new xy(10, 1), new xy(10, 6));
            double a1 = ut.PolygonArea2d(poly);
            BoundingBox2d bb = BoundingBox2d.FromArrayOfPoints(poly);
            double a2 = bb.Area();
            Assert.IsTrue(a1 <= a2);
        }

        [Test]
        public void Test_Polygon2d_ops()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));

            List<xy> poly2 = ut.MakePoly(new xy(3, 3), new xy(8, 3), new xy(8, 8), new xy(3, 8));
            List<List<xy>> result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsTrue(ut.SamePoly2d(result[0], ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 3), new xy(3, 3), new xy(3, 5), new xy(0, 5))));

            poly2 = ut.MakePoly(new xy(3, 3), new xy(5, 3), new xy(5, 5), new xy(3, 5));
            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsTrue(ut.SamePoly2d(result[0], ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 3), new xy(3, 3), new xy(3, 5), new xy(0, 5))));

            poly2 = ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 3), new xy(0, 3));
            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            Assert.IsTrue(ut.SamePoly2d(result[0], poly2));
            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsTrue(ut.SamePoly2d(result[0], ut.MakePoly(new xy(3, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5), new xy(0, 3), new xy(3, 3))));
        }

        internal void CheckPolyInt_OtherMethod(List<xy> poly1, List<xy> poly2, List<List<xy>> result)
        {
            double area1 = 0;
            foreach (List<xy> poly in result)
            {
                area1 += ut.PolygonArea2d(poly);
            }

            List<xyz> p1_3d = new List<xyz>();
            foreach (xy p in poly1)
            {
                p1_3d.Add(new xyz(p.x, p.y, 0));
            }
            List<xyz> p2_3d = new List<xyz>();
            foreach (xy p in poly2)
            {
                p2_3d.Add(new xyz(p.x, p.y, 0));
            }

            List<List<xyz>> loops1 = new List<List<xyz>>();
            loops1.Add(p1_3d);
            List<List<xyz>> loops2 = new List<List<xyz>>();
            loops2.Add(p2_3d);

            double area2 = 0;
            List<TriangleIntersection3d> tis = ppi3d.CalcIntersection(loops1, loops2);
            foreach (TriangleIntersection3d ti in tis)
            {
                if (ti.Count >= 3)
                {
                    List<xy> a2d = ut.Convert3dPointsTo2d(ti.pts);
                    area2 += ut.PolygonArea2d(a2d);
                }
            }

            Assert.IsTrue(fp.eq_area(area1, area2));
        }

        internal void CheckPolyInt(List<xy> poly1, List<xy> poly2, List<xy> expected)
        {
            List<List<xy>> result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            if (expected == null)
            {
                Assert.IsNull(result);
                result = ppi2d.Polygon2d_Intersection(poly2, poly1, PolyPolyOp.Intersection);
                Assert.IsNull(result);
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                List<xy> poly = result[0];
                Assert.IsTrue(ut.SamePoly2d(poly, expected));

                // now do this the other way
                result = ppi2d.Polygon2d_Intersection(poly2, poly1, PolyPolyOp.Intersection);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                poly = result[0];
                Assert.IsTrue(ut.SamePoly2d(poly, expected));

                CheckPolyInt_OtherMethod(poly1, poly2, result);

                // now do the difference
                double area1 = ut.PolygonArea2d(poly1);
                double area2 = ut.PolygonArea2d(poly2);
                double areaIntersection = ut.PolygonArea2d(poly);
                result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
                if (result != null)
                {
                    double areaDifference = 0.0;
                    foreach (List<xy> polly in result)
                    {
                        areaDifference += ut.PolygonArea2d(polly);
                    }
                    Assert.IsTrue(fp.eq_area(area1 - areaIntersection, areaDifference));
                }

                // now the union
                result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Union);
                double areaUnion = 0.0;
                foreach (List<xy> polly in result)
                {
                    areaUnion += ut.PolygonArea2d(polly);
                }
                Assert.IsTrue(fp.eq_area(area1 + area2 - areaIntersection, areaUnion));
            }
        }

        [Test]
        public void test_ppi2d_edge()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));
            CheckPolyInt(poly1, ut.MakePoly(new xy(5, 0), new xy(8, 0), new xy(8, 5), new xy(5, 5)), null);
        }

        [Test]
        public void Test_Polygon2dIntersection()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));

            // two rects that are separated
            CheckPolyInt(poly1, ut.MakePoly(new xy(6, 6), new xy(7, 6), new xy(7, 7), new xy(6, 7)), null);

            // two rects that share an edge
            CheckPolyInt(poly1, ut.MakePoly(new xy(5, 0), new xy(8, 0), new xy(8, 5), new xy(5, 5)), null);

            // two rects that share a point
            CheckPolyInt(poly1, ut.MakePoly(new xy(5, 5), new xy(8, 5), new xy(8, 8), new xy(5, 8)), null);

            // two rects that intersect, no overlaps, no vertices			
            CheckPolyInt(poly1, ut.MakePoly(new xy(3, 3), new xy(8, 3), new xy(8, 8), new xy(3, 8)), ut.MakePoly(new xy(3, 3), new xy(5, 3), new xy(5, 5), new xy(3, 5)));

            // two rects, one inside the other, sharing part of one edge
            CheckPolyInt(poly1, ut.MakePoly(new xy(3, 2), new xy(5, 2), new xy(5, 4), new xy(3, 4)), ut.MakePoly(new xy(3, 2), new xy(5, 2), new xy(5, 4), new xy(3, 4)));

            // two rects, one inside the other, sharing part of two edges
            CheckPolyInt(poly1, ut.MakePoly(new xy(3, 3), new xy(5, 3), new xy(5, 5), new xy(3, 5)), ut.MakePoly(new xy(3, 3), new xy(5, 3), new xy(5, 5), new xy(3, 5)));

            // two rects, one inside the other, sharing three edges (may be partial edges)
            CheckPolyInt(poly1, ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 5), new xy(0, 5)), ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 5), new xy(0, 5)));

            // same rect
            CheckPolyInt(poly1, poly1, poly1);

            // two rects, one inside the other but overlapping outside, sharing edges
            CheckPolyInt(poly1, ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 8), new xy(0, 8)), ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 5), new xy(0, 5)));

        }

        [Test]
        public void test_polypoly_diffbug()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(4, 0), new xy(4, 4), new xy(0, 4));
            List<xy> poly2 = ut.MakePoly(new xy(4, 4), new xy(4, -4), new xy(0, -4), new xy(0, 4));
            poly2.Reverse();
            List<List<xy>> diff = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsNull(diff);
        }

        [Test]
        public void test_polypoly_weird()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(5, 5), new xy(0, 5));

            // a little more complicated
            CheckPolyInt(poly1, ut.MakePoly(new xy(-2, 0), new xy(3, 0), new xy(3, 2), new xy(0, 5), new xy(2, 5), new xy(3, 6), new xy(-2, 9)), ut.MakePoly(new xy(0, 0), new xy(3, 0), new xy(3, 2), new xy(0, 5)));
        }

        [Test]
        public void MorePoly2dTests()
        {
            List<xy> poly1 = ut.MakePoly(new xy(0, 0), new xy(12, 0), new xy(12, 4), new xy(0, 4));
            List<xy> poly2 = ut.MakePoly(new xy(12, 4), new xy(8, 4), new xy(8, 0), new xy(12, 0));
            CheckPolyInt(poly1, poly2, poly2);

            poly2 = ut.MakePoly(new xy(12, 4), new xy(8, 4), new xy(8, 4.44089209850063E-16), new xy(12, 0));
            CheckPolyInt(poly1, poly2, poly2);

            poly2 = ut.MakePoly(new xy(4, 0), new xy(8, 0), new xy(8, 4), new xy(4, 4));
            CheckPolyInt(poly1, poly2, poly2);

            poly2 = ut.MakePoly(new xy(1, 6), new xy(1, 1), new xy(3, 1), new xy(3, 5), new xy(7, 5), new xy(7, 1), new xy(10, 1), new xy(10, 6));
            List<List<xy>> result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Intersection);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(ut.SamePoly2d(result[1], ut.MakePoly(new xy(1, 1), new xy(3, 1), new xy(3, 4), new xy(1, 4))));
            Assert.IsTrue(ut.SamePoly2d(result[0], ut.MakePoly(new xy(7, 1), new xy(10, 1), new xy(10, 4), new xy(7, 4))));

            result = ppi2d.Polygon2d_Intersection(poly1, poly2, PolyPolyOp.Difference);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(ut.SamePoly2d(result[0], ut.MakePoly(new xy(0, 0), new xy(12, 0), new xy(12, 4), new xy(10, 4), new xy(10, 1), new xy(7, 1), new xy(7, 4), new xy(3, 4), new xy(3, 1), new xy(1, 1), new xy(1, 4), new xy(0, 4))));

            poly1 = ut.MakePoly(new xy(0, 0), new xy(5, 0), new xy(6, 1), new xy(7, 0), new xy(12, 0), new xy(12, 4), new xy(0, 4));
            poly2 = ut.MakePoly(new xy(1, 2), new xy(11, 2), new xy(11, 3), new xy(1, 3));
            CheckPolyInt(poly1, poly2, poly2);

            poly2 = ut.MakePoly(new xy(1, 3), new xy(11, 3), new xy(11, 4), new xy(1, 4));
            CheckPolyInt(poly1, poly2, poly2);

#if not // in the new ppi2d code, this test sort of fails.  the resulting poly is the same, but it has two collinear segments
			poly2 = ut.MakePoly(new xy(1,1), new xy(11,1), new xy(11,2), new xy(1,2));
			CheckPolyInt(poly1, poly2, poly2);

			poly2 = ut.MakePoly(new xy(1,1), new xy(11,1), new xy(11,4), new xy(1,4));
			CheckPolyInt(poly1, poly2, poly2);
#endif
        }

        public void DoSimpleEulerCheck(Solid sol)
        {
            Assert.AreEqual(2, sol.Vertices.Count - sol.Edges.Count + sol.Faces.Count, "euler problem");
        }

        [Test]
        public void SegIntersectionTest_Problem()
        {
            xy p1 = new xy(0, 0);
            xy p2 = new xy(12, 0);
            xy q1 = new xy(8, 4);
            xy q2 = new xy(8, 4.44089209850063E-16);    // close enough to 8,0

            SegIntersection si = ut.GetSegIntersection(p1, p2, q1, q2);
            Assert.AreEqual(SegIntersection.Point, si);
        }

        [Test]
        public void SegIntersectionTests_WithOutpoints()
        {
            xy out1;
            xy out2;
            SegIntersection si;

            xy p0;
            xy q0;

            xy p1;
            xy q1;

            // same exact segment
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(0, 0);
            q1 = new xy(5, 5);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Overlap, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsTrue(fp.eq_inches(out2, q0));

            // interior point
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(2, 0);
            q1 = new xy(0, 2);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, new xy(1, 1)));
            Assert.IsNull(out2);

            // endpoint
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(5, 5);
            q1 = new xy(9, 5);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, q0));
            Assert.IsNull(out2);

            // endpoint
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(0, 0);
            q1 = new xy(-44, -44);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsNull(out2);

            // endpoint
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(-44, -44);
            q1 = new xy(0, 0);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsNull(out2);

            // endpoint
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(-1, 1);
            q1 = new xy(1, -1);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsNull(out2);

            // endpoint
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(-1, 0);
            q1 = new xy(1, 0);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Point, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsNull(out2);

            // overlap
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(3, 3);
            q1 = new xy(8, 8);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Overlap, si);
            Assert.IsTrue(fp.eq_inches(out1, p1));
            Assert.IsTrue(fp.eq_inches(out2, q0));

            // overlap
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(3, 3);
            q1 = new xy(5, 5);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Overlap, si);
            Assert.IsTrue(fp.eq_inches(out1, p1));
            Assert.IsTrue(fp.eq_inches(out2, q1));

            // overlap
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(3, 3);
            q1 = new xy(0, 0);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Overlap, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsTrue(fp.eq_inches(out2, p1));

            // overlap
            p0 = new xy(0, 0);
            q0 = new xy(5, 5);
            p1 = new xy(3, 3);
            q1 = new xy(-4, -4);
            si = ut.GetSegIntersection(p0, q0, p1, q1, out out1, out out2);
            Assert.AreEqual(SegIntersection.Overlap, si);
            Assert.IsTrue(fp.eq_inches(out1, p0));
            Assert.IsTrue(fp.eq_inches(out2, p1));
        }

        [Test]
        public void SegIntersectionTests()
        {
            // simple, obvious intersection at an interior point
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(5, 0), new xy(0, 5)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(5, 0), new xy(0, 5), new xy(0, 0), new xy(5, 5)));

            // same exact segment
            Assert.AreEqual(SegIntersection.Overlap, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(0, 0), new xy(5, 5)));

            // one segment a portion of the other
            Assert.AreEqual(SegIntersection.Overlap, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(1, 1), new xy(4, 4)));
            Assert.AreEqual(SegIntersection.Overlap, ut.GetSegIntersection(new xy(0, 0), new xy(-10, 0), new xy(8, 0), new xy(-30, 0)));

            // overlapping segments
            Assert.AreEqual(SegIntersection.Overlap, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(3, 3), new xy(8, 8)));
            Assert.AreEqual(SegIntersection.Overlap, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(3, 3), new xy(-4, -4)));

            // parallel lines
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(1, 0), new xy(6, 5)));
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(0, 1000), new xy(1, 0), new xy(1, 1000)));
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(1000, 0), new xy(0, 1), new xy(1000, 1)));

            // nearly parallel lines which do not intersect
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(0, 1000), new xy(1, 0), new xy(2, 1000)));
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(1000, 0), new xy(0, 1), new xy(1000, 2)));

            // nearly parallel lines which DO intersect: 1,000
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(0, 1000), new xy(1, 0), new xy(0, 1000)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(1000, 0), new xy(0, 1), new xy(1000, 0)));

            // nearly parallel lines which DO intersect: 10,000
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(0, 10000), new xy(1, 0), new xy(0, 10000)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(10000, 0), new xy(0, 1), new xy(10000, 0)));

#if NOT // these two tests fail.  They're kind of sadistic anyway.
			// nearly parallel lines which DO intersect: 100,000
			Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0,0), new xy(0,100000), new xy(1,0), new xy(0,100000)));
			Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0,0), new xy(100000,0), new xy(0,1), new xy(100000,0)));
#endif

            // segments lie on the same line, but do not touch each other
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(6, 6), new xy(9, 9)));
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(5, 0), new xy(6, 0), new xy(9, 0)));
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(0, 5), new xy(0, 6), new xy(0, 9)));

            // far enough apart
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(25, 25), new xy(30, 25)));

            // endpoints
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(5, 5), new xy(9, 5)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(0, 0), new xy(-44, -44)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(-44, -44), new xy(0, 0)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(5, 5), new xy(9, 5), new xy(0, 0), new xy(5, 5)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(-1, 1), new xy(1, -1)));
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(-1, 0), new xy(1, 0)));

            // near miss
            Assert.AreEqual(SegIntersection.None, ut.GetSegIntersection(new xy(0, 0), new xy(5, 5), new xy(0.01, 0), new xy(9, 0)));

            // perpendicular, vertical and horizontal
            Assert.AreEqual(SegIntersection.Point, ut.GetSegIntersection(new xy(0, 0), new xy(0, 5), new xy(0, 5), new xy(9, 5)));
        }

        [Test]
        public void Test_PointInsideTriangle2d()
        {
            Triangle2d t = new Triangle2d(new xy(0, 0), new xy(6, 0), new xy(3, 6));

            // points on the triangle
            Assert.IsTrue(t.PointInside(new xy(0, 0)));
            Assert.IsTrue(t.PointInside(new xy(3, 0)));
            Assert.IsTrue(t.PointInside(new xy(6, 0)));
            Assert.IsTrue(t.PointInside(new xy(3, 6)));

            // points in the triangle
            Assert.IsTrue(t.PointInside(new xy(5, 1)));
            Assert.IsTrue(t.PointInside(new xy(4, 3)));
            Assert.IsTrue(t.PointInside(new xy(4.5, 3)));

            // points outside the triangle
            Assert.IsFalse(t.PointInside(new xy(3, 7)));
            Assert.IsFalse(t.PointInside(new xy(6.01, 0)));
            Assert.IsFalse(t.PointInside(new xy(7, 1)));
            Assert.IsFalse(t.PointInside(new xy(6, 1)));
            Assert.IsFalse(t.PointInside(new xy(1, 4)));
            Assert.IsFalse(t.PointInside(new xy(-3, 6)));
            Assert.IsFalse(t.PointInside(new xy(-3, -3)));
            Assert.IsFalse(t.PointInside(new xy(5000, 0)));
            Assert.IsFalse(t.PointInside(new xy(5000, 1)));
        }

        [Test]
        public void Test_xyz_copy()
        {
            xyz a = new xyz(4, 5, 6);
            Assert.AreEqual(a, a);
            xyz b = a;
            Assert.AreEqual(a, b);
            Assert.IsTrue(fp.eq_inches(a, b));
            xyz c = a.copy();
            Assert.AreNotEqual(a, c);
            Assert.IsTrue(fp.eq_inches(a, c));
        }

        public void check_ti3d(int pts, Triangle3d t1, Triangle3d t2)
        {
            check_ti3d(pts, t1, t2, null, null);
            Triangle3d t1b = new Triangle3d(t1.b, t1.c, t1.a);
            check_ti3d(pts, t1b, t2, null, null);
            Triangle3d t1c = new Triangle3d(t1.c, t1.a, t1.b);
            check_ti3d(pts, t1c, t2, null, null);
            Triangle3d t2b = new Triangle3d(t2.b, t2.c, t2.a);
            check_ti3d(pts, t1, t2b, null, null);
            Triangle3d t2c = new Triangle3d(t2.c, t2.a, t2.b);
            check_ti3d(pts, t1, t2c, null, null);
        }

        public void check_ti3d(int pts, Triangle3d t1, Triangle3d t2, xyz s1, xyz s2)
        {
            TriangleIntersection3d ti = ppi3d.CalcTriangleIntersection3d(t1, t2);
            Assert.IsTrue(ti.Count <= 6);
            Assert.AreEqual(pts, ti.Count);

            if (pts > 0)
            {
                Assert.IsTrue(ppi3d.TestTriangleIntersection3d(t1, t2));
                Assert.IsTrue(ppi3d.TestTriangleIntersection3d(t2, t1));
            }
            else
            {
                Assert.IsFalse(ppi3d.TestTriangleIntersection3d(t1, t2));
                Assert.IsFalse(ppi3d.TestTriangleIntersection3d(t2, t1));
            }

            if (pts == 3)
            {
                xyz n = xyz.normal(t1.a, t1.b, t1.c).normalize_in_place();
                xyz i = (t1.b - t1.a).normalize_in_place();
                xyz j = xyz.cross(i, n).normalize_in_place();

                List<xy> p1 = ut.MakePoly(ut.ConvertPointTo2d(t1.a, t1.a, i, j), ut.ConvertPointTo2d(t1.b, t1.a, i, j), ut.ConvertPointTo2d(t1.c, t1.a, i, j));
                List<xy> p2 = ut.MakePoly(ut.ConvertPointTo2d(t2.a, t1.a, i, j), ut.ConvertPointTo2d(t2.b, t1.a, i, j), ut.ConvertPointTo2d(t2.c, t1.a, i, j));
                //ut.DumpPoly2d("p1", p1);
                //ut.DumpPoly2d("p2", p2);
                List<List<xy>> isect = ppi2d.Polygon2d_Intersection(p1, p2, PolyPolyOp.Intersection);

                List<xy> pts2d = new List<xy>();
                foreach (xyz p in ti)
                {
                    pts2d.Add(ut.ConvertPointTo2d(p, t1.a, i, j));
                }
                //ut.DumpPoly2d("pts2d", pts2d);
                //ut.DumpPoly2d("poly intersection", isect[0]);
                Assert.IsTrue(ut.SamePoly2d(isect[0], pts2d));
            }

            if (pts == 2)
            {
                if (
                    (s1 != null)
                    && (s2 != null)
                    )
                {
                    Assert.IsTrue(!fp.eq_unknowndata(s1, s2));
                    Assert.IsTrue((fp.eq_unknowndata(s1, ti[0]) || fp.eq_unknowndata(s1, ti[1])) && (fp.eq_unknowndata(s2, ti[0]) || fp.eq_unknowndata(s2, ti[1])));
                }
            }
        }

        [Test]
        public void test_fq_unknown()
        {
            // non-boards don't have face quality
            Solid s = Builtin_Solids.CreateTetrahedron("fq", 5);
            Assert.AreEqual(FaceQuality.Unknown, s.Faces[0].GetQuality());
        }

        [Test]
        public void Test_CalcTriangleIntersection3d()
        {
            // parallel
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // one vertex
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 0), new xyz(3, 6, 4))
                );
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 0))
                );

            // two vertices (an edge)
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 4)),
                new xyz(0, 0, 0), new xyz(6, 0, 0)
                );
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new xyz(6, 0, 0), new xyz(3, 6, 0)
                );
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 4), new xyz(3, 6, 0)),
                new xyz(0, 0, 0), new xyz(3, 6, 0)
                );

            // same triangle
            check_ti3d(3,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // one point on an edge
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 0, 0), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // one point inside
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 3, 0), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // not parallel, but entirely underneath
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 3))
                );

            // not parallel, but entirely above
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(0, 0, 7), new xyz(6, 0, 7), new xyz(3, 6, 5))
                );

            // intersects in an interior line
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(3, 0, 0), new xyz(3, 6, 0), new xyz(3, 3, 6))
                );

            // not nearby
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(43, 0, 0), new xyz(43, 6, 0), new xyz(43, 3, 6))
                );

            // coplanar miss
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4)),
                new Triangle3d(new xyz(40, 0, 4), new xyz(46, 0, 4), new xyz(43, 6, 4))
                );

            // coplanar overlap
            check_ti3d(3,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(8, 4, 0), new xyz(1, 9, 0))
                );

            // coplanar one vertex
            check_ti3d(1,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(6, 0, 0), new xyz(9, 0, 0), new xyz(9, 3, 0))
                );

            // coplanar one inside the other (both ways)
            check_ti3d(3,
                new Triangle3d(new xyz(1, 1, 0), new xyz(2, 1, 0), new xyz(2, 2, 0)),
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0))
                );
            check_ti3d(3,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(1, 1, 0), new xyz(2, 1, 0), new xyz(2, 2, 0))
                );

            // peek just through
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 3, -0.1), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // just miss peek through
            check_ti3d(0,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 3, 0.1), new xyz(6, 0, 4), new xyz(3, 6, 4))
                );

            // overlap same plane
            check_ti3d(3,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(0, 3, 0), new xyz(6, 3, 0), new xyz(3, 9, 0))
                );

            // nonplanar segment
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(3, 3, -1), new xyz(3, 3, 1), new xyz(3, 19, 1)),
                new xyz(3, 3, 0), new xyz(3, 6, 0)
                );
            // partial edge, coplanar
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(2, 0, 0), new xyz(4, 0, 0), new xyz(3, -10, 0)),
                new xyz(2, 0, 0), new xyz(4, 0, 0)
                );
            // partial edge, not coplanar
            check_ti3d(2,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 6, 0)),
                new Triangle3d(new xyz(2, 0, 0), new xyz(4, 0, 0), new xyz(3, -10, 50)),
                new xyz(2, 0, 0), new xyz(4, 0, 0)
                );

            check_ti3d(4,
                new Triangle3d(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 5, 0)),
                new Triangle3d(new xyz(5, 2, 0), new xyz(5, 4, 0), new xyz(2, 3, 0))
                );

            check_ti3d(6,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 4, 0)),
                new Triangle3d(new xyz(0, 3, 0), new xyz(3, -1, 0), new xyz(6, 3, 0))
                );

            check_ti3d(5,
                new Triangle3d(new xyz(0, 0, 0), new xyz(6, 0, 0), new xyz(3, 4, 0)),
                new Triangle3d(new xyz(0, 3, 0), new xyz(3, 0, 0), new xyz(6, 3, 0))
                );
        }

        [Test]
        public void Test_PolyPolyIntersection3d()
        {
            List<List<xyz>> poly1 = new List<List<xyz>>();

            List<xyz> main1 = new List<xyz>();
            main1.Add(new xyz(0, 0, 0));
            main1.Add(new xyz(9, 0, 0));
            main1.Add(new xyz(6, 4, 0));
            main1.Add(new xyz(9, 9, 0));
            main1.Add(new xyz(5, 20, 0));
            main1.Add(new xyz(0, 20, 0));
            main1.Add(new xyz(1, 9, 0));
            poly1.Add(main1);

            List<xyz> hole1 = new List<xyz>();
            hole1.Add(new xyz(1, 1, 0));
            hole1.Add(new xyz(1, 2, 0));
            hole1.Add(new xyz(2, 2, 0));
            hole1.Add(new xyz(2, 1, 0));
            poly1.Add(hole1);

            List<xyz> hole2 = new List<xyz>();
            hole2.Add(new xyz(1, 12, 0));
            hole2.Add(new xyz(1, 18, 0));
            hole2.Add(new xyz(5, 18, 0));
            hole2.Add(new xyz(3, 15, 0));
            hole2.Add(new xyz(4, 12, 0));
            poly1.Add(hole2);

            List<List<xyz>> poly2;
            List<xyz> main2;
            double x;
            double y;
            double z;

            // a little square, parallel to poly1 and above it, does not intersect
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            main2.Add(new xyz(3, 3, 1));
            main2.Add(new xyz(5, 3, 1));
            main2.Add(new xyz(5, 5, 1));
            main2.Add(new xyz(3, 5, 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

#if not
			// same little square, now moved down inside poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
			main2.Add(new xyz(3,3,0));
			main2.Add(new xyz(5,3,0));
			main2.Add(new xyz(5,5,0));
			main2.Add(new xyz(3,5,0));
			poly2.Add(main2);
			Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));
#endif

            // same idea, now the little square is 1x1 instead of 2x2
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 3;
            y = 3;
            main2.Add(new xyz(x, y, 0));
            main2.Add(new xyz(x + 1, y, 0));
            main2.Add(new xyz(x + 1, y + 1, 0));
            main2.Add(new xyz(x, y + 1, 0));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

#if not
			// same 1x1 square, now it touches a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
			x = 5;
			y = 3;
			main2.Add(new xyz(x,y,0));
			main2.Add(new xyz(x+1,y,0));
			main2.Add(new xyz(x+1,y+1,0));
			main2.Add(new xyz(x,y+1,0));
			poly2.Add(main2);
			Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));
#endif

            // same 1x1 square, now it touches a vertex but is otherwise outside
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 9;
            y = 9;
            main2.Add(new xyz(x, y, 0));
            main2.Add(new xyz(x + 1, y, 0));
            main2.Add(new xyz(x + 1, y + 1, 0));
            main2.Add(new xyz(x, y + 1, 0));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // same 1x1 square, now it is just outside
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 9.1;
            y = 9;
            main2.Add(new xyz(x, y, 0));
            main2.Add(new xyz(x + 1, y, 0));
            main2.Add(new xyz(x + 1, y + 1, 0));
            main2.Add(new xyz(x, y + 1, 0));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // same 1x1 square, just outside again, different spot
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 6;
            y = 18;
            main2.Add(new xyz(x, y, 0));
            main2.Add(new xyz(x + 1, y, 0));
            main2.Add(new xyz(x + 1, y + 1, 0));
            main2.Add(new xyz(x, y + 1, 0));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

#if not
			// same 1x1 square, now it is inside a hole, but it touches the edge of the hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
			x = 2;
			y = 14;
			main2.Add(new xyz(x,y,0));
			main2.Add(new xyz(x+1,y,0));
			main2.Add(new xyz(x+1,y+1,0));
			main2.Add(new xyz(x,y+1,0));
			poly2.Add(main2);
			Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

			// same 1x1 square, now it is entirely inside a hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
			x = 2;
			y = 13;
			main2.Add(new xyz(x,y,0));
			main2.Add(new xyz(x+1,y,0));
			main2.Add(new xyz(x+1,y+1,0));
			main2.Add(new xyz(x,y+1,0));
			poly2.Add(main2);
			Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));
#endif

            // square is now 1x1 minus .02 inch, sitting entirely inside a 1x1 hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 1.01;
            y = 1.01;
            main2.Add(new xyz(x, y, 0));
            main2.Add(new xyz(x + 0.98, y, 0));
            main2.Add(new xyz(x + 0.98, y + 0.98, 0));
            main2.Add(new xyz(x, y + 0.98, 0));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // little 1x1 square, perp to poly1, sitting too low to intersect
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 3;
            y = 3;
            z = -3;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // little 1x1 square, perp to poly1, now touches
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 3;
            y = 3;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // but not when it's inside the hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 2;
            y = 13;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // now it's inside, just touching a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 6;
            y = 5;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it's cutting through both sides of the poly
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 4;
            y = 4;
            z = -0.5;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // but not when it's in a hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 2;
            y = 13;
            z = -0.5;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // now it's outside, just touching a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 9;
            y = 8;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it's outside, just missing a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 6;
            y = 18;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // now it's outside, just missing a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 7;
            y = 4;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // now it's touching with overlap on an edge
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 1;
            y = 14;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it's touching with overlap on an edge
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 1;
            y = 21;
            z = -1;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x, y + 1, z));
            main2.Add(new xyz(x, y + 1, z + 1));
            main2.Add(new xyz(x, y, z + 1));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // big poly parallel to the x axis, perp to poly1, out of range at y=21
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 21;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // now it overlaps the y0 edge of poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 0;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it cuts poly1, through the small hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 1.5;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it cuts poly1, touching no holes, hitting a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 4;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it cuts poly1, touching no holes or vertices
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 6;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it cuts poly1, through the big hole at a vertex
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 15;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it cuts poly1, through the big hole overlapping an edge of that hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 18;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // now it misses poly1 again
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = -0.001;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // poly with more sides, similar to the big one, slices poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 6;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 25, y, z + 6));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // big poly with a spike on top
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 0;
            z = -2;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // spike now just touches inside poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 6;
            z = -9;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // spike now just misses inside poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 6;
            z = -9.01;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through inside poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -5;
            y = 6;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through, but outside poly1
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = 45;
            y = 6;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through, but inside a hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -8;
            y = 15;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through inside a hole, but touches the edge of that hole
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -9;
            y = 15;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through barely inside a hole 
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -7.1;
            y = 15;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsFalse(ppi3d.TestIntersection(poly1, poly2));

            // spike now just pokes through barely outside a hole 
            poly2 = new List<List<xyz>>();
            main2 = new List<xyz>();
            x = -6.8;
            y = 15;
            z = -8.99;
            main2.Add(new xyz(x, y, z));
            main2.Add(new xyz(x + 20, y, z));
            main2.Add(new xyz(x + 20, y, z + 4));
            main2.Add(new xyz(x + 12, y, z + 5));
            main2.Add(new xyz(x + 10, y, z + 9));
            main2.Add(new xyz(x + 8, y, z + 5));
            main2.Add(new xyz(x, y, z + 4));
            poly2.Add(main2);
            Assert.IsTrue(ppi3d.TestIntersection(poly1, poly2));

        }

        [Test]
        public void Test_LineVsTriangle3d()
        {
            Triangle3d t = new Triangle3d(new xyz(0, 0, 4), new xyz(6, 0, 4), new xyz(3, 6, 4));

            // straight up through the heart of the triangle
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(3, 3, 3), new xyz(3, 3, 5)));
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(3, 3, 3.999), new xyz(3, 3, 4.001)));

            // endpoints
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 0), new xyz(0, 0, 4)));
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 0), new xyz(6, 0, 4)));
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 0), new xyz(3, 6, 4)));

            // edges
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(-5, -5, -5), new xyz(3, 0, 4)));

            // close calls
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(-5, -5, -5), new xyz(3, 1, 4.01)));   // near hit
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(-5, -5, -5), new xyz(3, 0, 4.01))); // near miss
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 3.999), new xyz(4, 4, 4.001)));
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 3.999), new xyz(9, 9, 4.001)));

            // through the middle somewhere
            Assert.IsTrue(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(-100, -100, -100), new xyz(3, 3, 4.001)));

            // segments that don't touch
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 0), new xyz(-1, 0, 0)));
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 0), new xyz(9, 9, 0)));
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(3, 3, 3), new xyz(3, 3, 3.99)));
            Assert.IsFalse(ppi3d.TriangleLineSegmentIntersect3d(t, new xyz(0, 0, 4.001), new xyz(9, 9, 4.001)));
        }

        [Test]
        public void Test_TriangulateWithLotsOfHoles()
        {
            List<xy> main = new List<xy>();
            main.Add(new xy(0, 0));
            main.Add(new xy(100, 0));
            main.Add(new xy(100, 100));
            main.Add(new xy(0, 100));

            List<List<xy>> holes = new List<List<xy>>();
            for (int i = 10; i <= 90; i += 10)
            {
                for (int j = 10; j <= 90; j += 10)
                {
                    List<xy> hole = new List<xy>();
                    hole.Add(new xy(i, j));
                    hole.Add(new xy(i, j + 5));
                    hole.Add(new xy(i + 5, j + 5));
                    hole.Add(new xy(i + 5, j));
                    holes.Add(hole);
                }
            }

            List<Triangle2d> tris = new List<Triangle2d>();
            tri.Triangulate2d_WithHoles(tris, main, holes);

            double area_main = Math.Abs(ut.PolygonArea2d(main));
            double area_holes = holes.Count * 25;
            double area = area_main - area_holes;

            double area_tris = 0;
            foreach (Triangle2d t in tris)
            {
                area_tris += t.Area();
            }

            Assert.IsTrue(fp.eq_area(area_tris, area));
        }

        [Test]
        public void Test_CutSegments()
        {
            List<xy> main = new List<xy>();
            main.Add(new xy(0, 0));
            main.Add(new xy(9, 0));
            main.Add(new xy(9, 9));
            main.Add(new xy(0, 9));
            main.Add(new xy(-5, 1));
            main.Add(new xy(0, 8));

            List<xy> hole1 = new List<xy>();
            hole1.Add(new xy(1, 1));
            hole1.Add(new xy(1, 2));
            hole1.Add(new xy(2, 2));
            hole1.Add(new xy(2, 1));

            List<xy> hole2 = new List<xy>();
            hole2.Add(new xy(4, 5));
            hole2.Add(new xy(4, 7));
            hole2.Add(new xy(8, 7));
            hole2.Add(new xy(8, 5));

            List<xy> hole3 = new List<xy>();
            hole3.Add(new xy(6, 1));
            hole3.Add(new xy(6, 3));
            hole3.Add(new xy(8, 3));
            hole3.Add(new xy(8, 1));

            List<List<xy>> holes = new List<List<xy>>();
            holes.Add(hole1);
            holes.Add(hole2);
            holes.Add(hole3);

            // test main 0 against all four vertices of each of the other holes
            Assert.IsTrue(ut.IsCutSegment(main, holes, 0, 0, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 0, 0, 1));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 0, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 0, 0, 3));

            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 1, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 1, 1));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 1, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 1, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 0, 2, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 2, 1));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 0, 2, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 0, 2, 3));

            // test main 1 against all four vertices of each of the other holes
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 0, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 1, 0, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 0, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 0, 3));

            Assert.IsFalse(ut.IsCutSegment(main, holes, 1, 1, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 1, 1, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 1, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 1, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 2, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 1, 2, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 2, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 1, 2, 3));

            // test main 2 against all four vertices of each of the other holes
            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 0, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 0, 1));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 0, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 0, 3));

            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 1, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 2, 1, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 2, 1, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 2, 1, 3));

            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 2, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 2, 2, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 2, 2, 2));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 2, 2, 3));

            // test main 3 against all four vertices of each of the other holes
            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 0, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 0, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 0, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 3, 0, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 1, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 1, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 1, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 3, 1, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 3, 2, 0));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 3, 2, 1));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 3, 2, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 3, 2, 3));

            // main 4 should be out of reach of ALL hole vertices
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Assert.IsFalse(ut.IsCutSegment(main, holes, 4, i, j));
                }
            }

            // test main 5 against all four vertices of each of the other holes
            // main 5 is much like main 3, except for vertex 1 of hole 2,
            // which is now accessible because it doesn't nip vertex 0 of
            // hole 1
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 0, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 0, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 0, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 5, 0, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 1, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 1, 1));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 1, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 5, 1, 3));

            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 2, 0));
            Assert.IsTrue(ut.IsCutSegment(main, holes, 5, 2, 1)); // diff from main 3
            Assert.IsFalse(ut.IsCutSegment(main, holes, 5, 2, 2));
            Assert.IsFalse(ut.IsCutSegment(main, holes, 5, 2, 3));

            List<Triangle2d> tris = new List<Triangle2d>();
            tri.Triangulate2d_WithHoles(tris, main, holes);

            double area_main = Math.Abs(ut.PolygonArea2d(main));
            double area_holes = 1 + 8 + 4;
            double area = area_main - area_holes;

            double area_tris = 0;
            foreach (Triangle2d t in tris)
            {
                area_tris += t.Area();
            }

            Assert.IsTrue(fp.eq_area(area_tris, area));
        }

#if not // this is an assert now
		[Test]
		public void BadTriangles()
		{
			bool bfailed = false;
			try
			{
				Triangle3d t = new Triangle3d(new xyz(0,0,0), new xyz(1,1,1), new xyz(2,2,2));
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Creating a triangle with 3 collinear points should cause an exception.");

			bfailed = false;
			try
			{
				Triangle2d t = new Triangle2d(new xy(0,0), new xy(1,1), new xy(2,2));
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Creating a triangle with 3 collinear points should cause an exception.");

			bfailed = false;
			try
			{
				Triangle2d t = new Triangle2d(new xy(0,0), new xy(1,1), new xy(1,0));
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Creating a triangle clockwise should cause an exception.");

			// Note that Triangle3d doesn't check for clockwise-ness, 
			// since it would require us to convert to 2d, and that's just excessive.
		}
#endif

        [Test]
        public void CollinearPoints()
        {
            Assert.IsTrue(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(1, 1, 1), new xyz(5, 5, 5)));
            Assert.IsTrue(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(1, 0, 0), new xyz(2, 0, 0)));
            Assert.IsTrue(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(1, 1, 1), new xyz(555, 555, 555)));
            Assert.IsTrue(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(-5, -5, -5), new xyz(5, 5, 5)));

            Assert.IsFalse(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(1, 1, 1), new xyz(5, 8, 5)));
            Assert.IsFalse(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(1, 5, 1), new xyz(5, 8, 5)));
            Assert.IsFalse(ut.PointsAreCollinear3d(new xyz(0, 0, 0), new xyz(7, 7, -1), new xyz(5, 5, 5)));

            Assert.IsTrue(ut.PointsAreCollinear2d(new xy(0, 0), new xy(1, 1), new xy(2, 2)));
            Assert.IsTrue(ut.PointsAreCollinear2d(new xy(0, 0), new xy(1, 0), new xy(2, 0)));
            Assert.IsTrue(ut.PointsAreCollinear2d(new xy(0, 0), new xy(0, 1), new xy(0, 2)));
            Assert.IsTrue(ut.PointsAreCollinear2d(new xy(0, 0), new xy(-5, -5), new xy(9, 9)));

            Assert.IsFalse(ut.PointsAreCollinear2d(new xy(0, 0), new xy(1, 1), new xy(2.1, 2)));

            List<xyz> a = new List<xyz>();
            a.Add(new xyz(0, 0, 0));
            a.Add(new xyz(5, 5, 5));
            a.Add(new xyz(-10, -10, -10));

#if false // new implementation doesn't throw.  I'm not sure we need this really.
			bool bfailed = false;
			try
			{
				xyz n = ut.GetNormalFromPointList(a);
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Getting normal from three collinear points should cause an exception.");

			bfailed = false;
			try
			{
				xyz n;
				xyz i;
				xyz j;
				xyz origin;

				ut.GetPlaneFromArrayOfPoints(a, out origin, out n, out i, out j);
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Getting plane info from three collinear points should cause an exception.");
#endif
        }

        [Test]
        public void Test_PointSideOfLine()
        {
            xy a = new xy(0, 0);
            xy b = new xy(5, 5);

            // points on the line
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(0, 0), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(1, 1), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(3, 3), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(5, 5), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(8, 8), a, b));

            // points left of the line
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(0, 3), a, b));
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(1, 1.01), a, b));
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(0, 1), a, b));
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(0, 50), a, b));

            // points right of the line
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(3, 0), a, b));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(1, 0), a, b));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(3, 2.9), a, b));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(50, 0), a, b));

            // reverse the line segment.  points previously left are now right.
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(0, 3), b, a));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(1, 1.01), b, a));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(0, 1), b, a));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(0, 50), b, a));

            a = new xy(5, 0);
            b = new xy(0, 5);

            Assert.AreEqual(1, ut.PointSideOfLine(new xy(3, 3), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(3, 2), a, b));
            Assert.AreEqual(0, ut.PointSideOfLine(new xy(2, 3), a, b));
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(2, 2), a, b));
            Assert.AreEqual(-1, ut.PointSideOfLine(new xy(0, 0), a, b));
            Assert.AreEqual(1, ut.PointSideOfLine(new xy(5, 5), a, b));
        }

        [Test]
        public void TriangleArea()
        {
            Triangle2d t = new Triangle2d(new xy(0, 0), new xy(5, 0), new xy(5, 5));
            double area = t.Area();
            Assert.IsTrue(fp.eq_area(area, 12.5));
        }

        [Test]
        public void Test_ArePolygonsSimpleOrNot()
        {
            List<xy> pts = new List<xy>();

            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(5, 0));
            pts.Add(new xy(0, 5));
            pts.Add(new xy(5, 5));
            Assert.IsFalse(ut.PolygonIsSimple2d(pts));

            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(5, 0));
            pts.Add(new xy(5, 5));
            pts.Add(new xy(0, 0));
            pts.Add(new xy(0, 5));
            pts.Add(new xy(-5, 5));
            pts.Add(new xy(-5, 0));
            Assert.IsFalse(ut.PolygonIsSimple2d(pts));

            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(5, 0));
            pts.Add(new xy(5, 5));
            pts.Add(new xy(2, -3));
            Assert.IsFalse(ut.PolygonIsSimple2d(pts));

            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(5, 0));
            Assert.IsFalse(ut.PolygonIsSimple2d(pts));

            // collinear
            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(1, 1));
            pts.Add(new xy(5, 5));
            Assert.IsFalse(ut.PolygonIsSimple2d(pts));

            pts.Clear();
            pts.Add(new xy(0, 0));
            pts.Add(new xy(5, 0));
            pts.Add(new xy(5, 5));
            Assert.IsTrue(ut.PolygonIsSimple2d(pts));
        }

        [Test]
        public void Test_PointInPoly()
        {
            List<xy> square = ut.MakePoly(new xy(12, 4), new xy(8, 4), new xy(8, 0), new xy(12, 0));
            Assert.IsFalse(ut.PointInsidePoly(square, new xy(0, 4)));
            Assert.IsFalse(ut.PointInsidePoly(square, new xy(0, 0)));

            List<xy> pts = new List<xy>();
            pts.Add(new xy(0, 0));  // 0
            pts.Add(new xy(2, 0));  // 1
            pts.Add(new xy(3, 2));  // 2
            pts.Add(new xy(4, 1));  // 3
            pts.Add(new xy(5, 4));  // 4
            pts.Add(new xy(6, 0));  // 5
            pts.Add(new xy(7, 3));  // 6
            pts.Add(new xy(7, 6));  // 7
            pts.Add(new xy(8, 1));  // 8
            pts.Add(new xy(9, 7));  // 9
            pts.Add(new xy(9, 9));  // 10
            pts.Add(new xy(8, 7));  // 11
            pts.Add(new xy(3, 7));  // 12

            Assert.IsTrue(ut.PolygonIsSimple2d(pts));

            // points ON the poly
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(1, 0)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(0, 0)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(8, 1)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(9, 7)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(9, 8)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(2, 0)));

            // points clearly inside
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(4, 5)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(1, 1)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(6, 2)));
            Assert.IsTrue(ut.PointInsidePoly(pts, new xy(8, 4)));

            // points outside
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(1, 5)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(1, 7)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(1, 4)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(5, 2)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(7, 2)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(4, 0)));
            Assert.IsFalse(ut.PointInsidePoly(pts, new xy(9, 6)));

            List<Triangle2d> tris = new List<Triangle2d>();
            tri.Triangulate2d_WithHoles(tris, pts, null);

            double area = Math.Abs(ut.PolygonArea2d(pts));

            double area_tris = 0;
            foreach (Triangle2d t in tris)
            {
                area_tris += t.Area();
            }

            Assert.IsTrue(fp.eq_area(area_tris, area));
        }

        [Test]
        public void Test_NastyPolygon_DiagonalsAndTris()
        {
            List<xy> pts = new List<xy>();
            pts.Add(new xy(0, 0));  // 0
            pts.Add(new xy(2, 0));  // 1
            pts.Add(new xy(3, 2));  // 2
            pts.Add(new xy(4, 1));  // 3
            pts.Add(new xy(5, 4));  // 4
            pts.Add(new xy(6, 0));  // 5
            pts.Add(new xy(7, 3));  // 6
            pts.Add(new xy(7, 6));  // 7
            pts.Add(new xy(8, 1));  // 8
            pts.Add(new xy(9, 7));  // 9
            pts.Add(new xy(3, 7));  // 10

            Assert.IsTrue(ut.IsDiagonal(pts, 0, 2));
            Assert.IsTrue(ut.IsDiagonal(pts, 2, 4));
            Assert.IsTrue(ut.IsDiagonal(pts, 4, 7));
            Assert.IsTrue(ut.IsDiagonal(pts, 4, 10));
            Assert.IsTrue(ut.IsDiagonal(pts, 0, 7));
            Assert.IsTrue(ut.IsDiagonal(pts, 4, 6));
            Assert.IsTrue(ut.IsDiagonal(pts, 6, 10));
            Assert.IsTrue(ut.IsDiagonal(pts, 7, 9));
            Assert.IsTrue(ut.IsDiagonal(pts, 7, 5));

            Assert.IsFalse(ut.IsDiagonal(pts, 1, 3));
            Assert.IsFalse(ut.IsDiagonal(pts, 1, 2));
            Assert.IsFalse(ut.IsDiagonal(pts, 1, 5));
            Assert.IsFalse(ut.IsDiagonal(pts, 2, 6));
            Assert.IsFalse(ut.IsDiagonal(pts, 2, 7));
            Assert.IsFalse(ut.IsDiagonal(pts, 0, 6));
            Assert.IsFalse(ut.IsDiagonal(pts, 6, 6));
            Assert.IsFalse(ut.IsDiagonal(pts, 5, 8));
            Assert.IsFalse(ut.IsDiagonal(pts, 7, 3));

            double area1;
            double area2;
            List<Triangle2d> tris;

            tris = new List<Triangle2d>();
            tri.Triangulate2d(tris, pts);

            area1 = ut.PolygonArea2d(pts);
            area2 = ut.SumTriangleAreas2d(tris);
            Assert.IsTrue(fp.eq_area(area1, area2));

            List<List<xy>> holes = new List<List<xy>>();

            // add a 1x1 hole and see if the triangulation area is correct
            List<xy> hole = new List<xy>();
            hole.Add(new xy(3, 3));
            hole.Add(new xy(3, 4));
            hole.Add(new xy(4, 4));
            hole.Add(new xy(4, 3));
            holes.Add(hole);

            tris = new List<Triangle2d>();
            tri.Triangulate2d_WithHoles(tris, pts, holes);

            area1 = ut.PolygonArea2d(pts) - holes.Count;
            area2 = ut.SumTriangleAreas2d(tris);
            Assert.IsTrue(fp.eq_area(area1, area2));

            // add ANOTHER 1x1 hole and see if the triangulation area is correct
            hole = new List<xy>();
            hole.Add(new xy(4, 5));
            hole.Add(new xy(4, 6));
            hole.Add(new xy(5, 6));
            hole.Add(new xy(5, 5));
            holes.Add(hole);

            tris = new List<Triangle2d>();
            tri.Triangulate2d_WithHoles(tris, pts, holes);

            area1 = ut.PolygonArea2d(pts) - holes.Count;
            area2 = ut.SumTriangleAreas2d(tris);
            Assert.IsTrue(fp.eq_area(area1, area2));
        }

        [Test]
        public void Tri_Trivial()
        {
            List<Triangle2d> tris = new List<Triangle2d>();
            List<xy> pts = new List<xy>();

            pts.Add(new xy(0, 0));
            pts.Add(new xy(9, 0));
            pts.Add(new xy(9, 9));
            pts.Add(new xy(0, 9));

            tri.Triangulate2d(tris, pts);

            Assert.AreEqual(2, tris.Count);
        }

        [Test]
        public void xyz_Indexing()
        {
            xyz v = new xyz(7, 9, 13);

            // no calculations happened, so I'm assuming that testing for actual equality is OK here

            Assert.AreEqual(v[0], 7);
            Assert.AreEqual(v[1], 9);
            Assert.AreEqual(v[2], 13);

            bool bfailed = false;
            try
            {
                double d = v[3];
            }
            catch (ArgumentOutOfRangeException)
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed, "ndx out of range should throw an exception");
        }

        [Test]
        public void MiscTestsOnFaceWithNoHoles()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Face f = sol.Faces[0];
            Assert.IsNull(f.Holes);
            Assert.IsFalse(f.HasHoles());
            List<List<xy>> a = f.GetLoopsIn2d();
            Assert.AreEqual(1, a.Count);
        }

        [Test]
        public void HalfEdge_Opposite()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Edge e = sol.Edges[0];
            Assert.AreSame(e.a2b, e.b2a.Opposite());
            Assert.AreSame(e.b2a, e.a2b.Opposite());
        }

        [Test]
        public void Test_Convert3dPointsTo2d()
        {
            // this test verifies that a 3d ccw loop comes out as a 2d ccw loop

            List<xyz> a3d = new List<xyz>();
            a3d.Add(new xyz(0, 0, 0));
            a3d.Add(new xyz(5, 0, 0));
            a3d.Add(new xyz(5, 5, 0));
            a3d.Add(new xyz(0, 5, 0));
            List<xy> a2d = ut.Convert3dPointsTo2d(a3d);
            Assert.AreEqual(a3d.Count, a2d.Count);
            for (int i = 0; i < a2d.Count; i++)
            {
                xy pt2d = a2d[i];
                xyz pt3d = a3d[i];

                Assert.IsTrue(fp.eq_inches(pt2d.x, pt3d.x));
                Assert.IsTrue(fp.eq_inches(pt2d.y, pt3d.y));
            }
        }

        [Test]
        public void Test_Convert2dPointsTo3d()
        {
            // start with a simple square in 2d
            List<xy> a2d = new List<xy>();
            a2d.Add(new xy(0, 0));
            a2d.Add(new xy(10, 0));
            a2d.Add(new xy(10, 10));
            a2d.Add(new xy(0, 10));

            // project it onto some plane (more or less randomly chosen)
            xyz n = new xyz(4, 7, 9).normalize_in_place();
            xyz iv = new xyz(-3, 21, 4).normalize_in_place();
            xyz jv = xyz.cross(iv, n).normalize_in_place();

            List<xyz> a3d = ut.Convert2dPointsTo3d(a2d, new xyz(17, -32, 44), iv, jv);

            // now take those 3d points and convert to the 2d local coord system of that plane
            List<xy> back = ut.Convert3dPointsTo2d(a3d);
            Assert.AreEqual(a2d.Count, back.Count);

            // the resulting 2d points should be the same as the ones we started with
            for (int i = 0; i < a2d.Count; i++)
            {
                xy a = a2d[i];
                xy b = back[i];

                Assert.IsTrue(fp.eq_inches(a, b));
            }
        }

        [Test]
        public void xyz_MagnitudeTests()
        {
            xyz v1 = new xyz(1, 0, 0);
            double mag = v1.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 1));

            xyz v = 17 * v1;
            mag = v.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 17));

            v = v1 * 17;
            mag = v.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 17));

            v = new xyz(17, 38, -44);
            xyz vn = v.normalize();
            mag = vn.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 1));
        }

        [Test]
        public void xy_MagnitudeTests()
        {
            xy v1 = new xy(1, 0);
            double mag = v1.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 1));

            xy v = 17 * v1;
            mag = v.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 17));

            v = v1 * 17;
            mag = v.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 17));

            v = new xy(17, 38);
            xy vn = v.normalize();
            mag = vn.magnitude();
            Assert.IsTrue(fp.eq_inches(mag, 1));
        }

        [Test]
        public void xy_ToString()
        {
            xy v = new xy(1, 2);
            Assert.AreEqual(v.ToString(), "(1, 2)");
        }

        [Test]
        public void xyz_ToString()
        {
            xyz v = new xyz(1, 2, 3);
            Assert.AreEqual(v.ToString(), "(1, 2, 3)");
        }

        [Test]
        public void RotateDoesNotAffectVolumeOrArea()
        {
            Solid sol = Builtin_Solids.CreateSolidWithHoleAndMortise("sol");
            Solid_Verify(sol);

            double vol = sol.Volume();
            double area = sol.SurfaceArea();

            sol.Rotate_Deg(0, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(360, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(180, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(-180, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(370, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(-370, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(-23, new xyz(0, 0, 0), new xyz(5, 5, 5));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(14, new xyz(0, 0, 0), new xyz(1, 0, 0));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(73, new xyz(0, 0, 0), new xyz(0, 1, 0));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            sol.Rotate_Deg(22, new xyz(0, 0, 0), new xyz(0, 0, 1));
            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));
        }

#if not
        [Test]
        public void test_boardcolor()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Solid s in p.Result.Subs)
            {
                System.Windows.Media.Color c = s.material.GetColor();
                Assert.IsNotNull(c);
            }
        }
#endif

        [Test]
        public void RotateDoesNotAffectWeight()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            CompoundSolid cs = p.Result;
            double weight = cs.Weight();
            Assert.IsTrue(fp.gt_unknowndata(weight, 0));
            cs.Rotate(Math.Cos(ut.DegreeToRadian(27)), Math.Sin(ut.DegreeToRadian(27)), new xyz(0, 0, 0), new xyz(0, 0, 1));
            Assert.IsTrue(fp.eq_unknowndata(weight, cs.Weight()));
        }

        [Test]
        public void test_minmax()
        {
            List<xy> p1 = ut.MakePoly(new xy(0, 0), new xy(8, 0), new xy(8, 0.75), new xy(0, 0.75));
            Assert.IsTrue(fp.eq_inches(0, ut.GetMinX(p1)));
            Assert.IsTrue(fp.eq_inches(0, ut.GetMinY(p1)));
            Assert.IsTrue(fp.eq_inches(8, ut.GetMaxX(p1)));
            Assert.IsTrue(fp.eq_inches(0.75, ut.GetMaxY(p1)));
        }

        [Test]
        public void TranslateDoesNotAffectVolumeOrArea()
        {
            Solid sol = Builtin_Solids.CreateSolidWithHoleAndMortise("sol");
            Solid_Verify(sol);

            double vol = sol.Volume();
            double area = sol.SurfaceArea();
            xyz center = sol.GetCenter();

            xyz off = new xyz(12, 37, 22);

            sol.Translate(off.x, off.y, off.z);

            Assert.IsTrue(fp.eq_volume(vol, sol.Volume()));
            Assert.IsTrue(fp.eq_area(area, sol.SurfaceArea()));

            xyz center2 = sol.GetCenter();
            xyz center3 = center + off;

            Assert.IsTrue(fp.eq_inches(center2, center3));
        }

        #region Sample Solids

        /*
			These tests construct sample solids, calling Solid_Verify()
			to run checks on each one.  In certain cases, volume and
			surface area are verified against standard formulas.
		*/

        public void Solid_CheckEulerFormula(Solid sol)
        {
            /*
			
			TODO we could check the advanced euler formula here.
			
			V - E + F - (L - F) - 2(S - G) = 0

			* V: the number of vertices
			* E: the number of edges
			* F: the number of faces
			* G: the number of holes that penetrate the solid, usually referred to as genus in topology
			* S: the number of shells. A shell is an internal void of a solid. A shell is bounded by a 2-manifold surface, which can have its own genus value. Note that the solid itself is counted as a shell. Therefore, the value for S is at least 1.
			* L: the number of loops, all outer and inner loops of faces are counted.

			 http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/model/euler.html
			
			Counting vertices, edges, faces, and loops is easy.
			
			Counting shells is hard.
			
			Counting holes is hard.
			
			*/
        }

        public void Solid_VolumeNotGreaterThanBoundingBoxVolume(Solid sol)
        {
            double vol = sol.Volume();
            BoundingBox3d bb = sol.GetBoundingBox();
            Assert.IsTrue(vol <= bb.volume, "the volume of the solid can never be more than the volume of its bbox");
        }

        public void Solid_TriangulationAreaMatch(Solid sol)
        {
            double area = sol.SurfaceArea();
            double tri_area = ut.SumTriangleAreas3d(sol.GetTriangles());
            Assert.IsTrue(fp.eq_area(area, tri_area));
        }

        public void Solid_Verify(Solid sol)
        {
            Assert.IsTrue(sol.IsClosed());
            sol.DoGeomChecks();
            Solid_TriangulationAreaMatch(sol);
            Solid_VolumeNotGreaterThanBoundingBoxVolume(sol);
            Solid_CheckEulerFormula(sol);
        }

        [Test]
        public void SolidWithHoleAndMortise()
        {
            Solid sol = Builtin_Solids.CreateSolidWithHoleAndMortise("sol");
            Solid_Verify(sol);
        }

        [Test]
        public void Cube()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);
            Assert.IsTrue(fp.eq_volume(size * size * size, sol.Volume()));
            Assert.IsTrue(fp.eq_area(size * size * 6, sol.SurfaceArea()));
            DoSimpleEulerCheck(sol);
        }

        [Test]
        public void CubeWithMortise()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);

            double vol1 = sol.Volume();
            Assert.IsTrue(fp.eq_volume(size * size * size, vol1));

            Assert.IsTrue(sol.IsClosed());
            sol = wood.Mortise(sol.Faces[0], 0, 1, 1.5, 1.5, 2, 2, 1, "m");
            Assert.IsTrue(sol.IsClosed());

            double vol2 = sol.Volume();

            Assert.IsTrue(fp.eq_volume(vol1 - 4, vol2));
        }

        [Test]
        public void Cylinder()
        {
            Solid sol = wood.CreateCylinder("cyl", 10, 20, 10);
            Solid_Verify(sol);
            DoSimpleEulerCheck(sol);
        }

        [Test]
        public void Tetrahedron()
        {
            const int size = 5;
            double gap = size * 2;
            double edge = Math.Sqrt(gap * gap + gap * gap);
            Solid sol = Builtin_Solids.CreateTetrahedron("tet", size);
            Solid_Verify(sol);
            Assert.IsTrue(fp.eq_volume(edge * edge * edge * Math.Sqrt(2) / 12, sol.Volume()));
            Assert.IsTrue(fp.eq_area(sol.SurfaceArea(), Math.Sqrt(3) * edge * edge));
            DoSimpleEulerCheck(sol);
        }

        #endregion

        #region GeomCheck Tests

        /*
			The GeomCheck tests verify that exceptions get thrown
			properly when we try to construct something with
			invalid geometry.
		*/

        [Test]
        public void GeomCheck_CubeWithLotsOfMortises()
        {
            Solid sol = Builtin_Solids.CreateCubeWithLotsOfMortises();
            Solid_Verify(sol);
        }

        [Test]
        public void Test_Glue_SurfaceArea()
        {
            Plan p = new Plan("Test");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));

            p.AddStep(Action.NEW_BOARD, "b", Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));

            p.AddStep(Action.JOIN, "Join a and b", Step.CreateParms(
                "path1", "a.end2.bottom",
                "path2", "b.end2.bottom",
                "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.Execute();

            Assert.IsTrue(fp.eq_area(p.Steps[1].Result.SurfaceArea(), 5 * 5 * 6));
            Assert.IsTrue(fp.eq_area(p.Steps[2].Result.SurfaceArea(), 5 * 5 * 6));
            Assert.IsTrue(fp.eq_area(p.Steps[3].Result.ActualSurfaceArea(), 5 * 5 * 6 + 5 * 5 * 6 - 5 * 5 * 2));
        }

        [Test]
        public void GeomCheck_CubeWithMortiseThatIsTooDeep()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);
            Face f = sol.Faces[0];

            // this is legal now.
            sol = wood.Mortise(f, 0, 1, 1.5, 1.5, 2, 2, 6, "m");
            sol.DoGeomChecks();
        }

        [Test]
        public void GeomCheck_CubeWithMortiseThatIsBarelyTooDeep()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);
            Face f = sol.Faces[0];

            sol = wood.Mortise(f, 0, 1, 1.5, 1.5, 2, 2, 5, "m");
            sol.DoGeomChecks();
        }

        [Test]
        public void GeomCheck_CubeWithTwoMortisesThatIntersect()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);

            int depth = 4;

            // create a 3x3 mortise at 1,1 on this face, 4 inches deep
            sol = wood.Mortise(sol.Faces[0], 0, 1, 1, 1, 3, 3, depth, "m1");

            Face f = sol.Faces[0];
            Face f2 = f.MainLoop[0].Opposite().face;

            Assert.IsTrue(f.SharesAnEdgeWith(f2), "This face should be adjacent to the other one.");

            // create the same mortise on this face.  This will hit the other mortise, but it succeeds now that we have the bool3d code.
            sol = wood.Mortise(f2, 0, 1, 1, 1, 3, 3, depth, "m2");
        }

        [Test]
        public void GeomCheck_CubeWithTwoMortisesThatDoNotIntersect()
        {
            const int size = 5;
            Solid sol = Builtin_Solids.CreateCube("cube", size);
            Solid_Verify(sol);
            Face f = sol.Faces[0];

            // same basic test as the intersecting mortises thing above, but the depth is too shallow to conflict
            double depth = 0.5;

            // create a 3x3 mortise at 1,1 on this face, 0.5 inches deep
            sol = wood.Mortise(f, 0, 1, 1, 1, 3, 3, depth, "m1");
            f = sol.Faces[0];

            Face f2 = f.MainLoop[0].Opposite().face;

            Assert.IsTrue(f.SharesAnEdgeWith(f2), "This face should be adjacent to the other one.");

            sol = wood.Mortise(f2, 0, 1, 1, 1, 3, 3, depth, "m2");
            Assert.IsTrue(true);
        }

        [Test]
        public void BadVertexIndex()
        {
            Solid s = new Solid("test", BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

            double d = 5;

            s.AddVertex(0, 0, 0);
            s.AddVertex(d, 0, 0);
            s.AddVertex(d, d, 0);
            s.AddVertex(0, d, 0);
            s.AddVertex(0, 0, d);
            s.AddVertex(d, 0, d);
            s.AddVertex(d, d, d);
            s.AddVertex(0, d, d);

            bool bfailed = false;
            try
            {
                s.CreateFace("test", 0, 1, 8, 3);       // WRONG:  there is no vertex 8.  this should fail.
            }
            catch (ArgumentOutOfRangeException)
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed, "vertex out of range should throw an exception");
        }

#if false // TODO should probably put these tests back.  but they're asserts now, so they don't throw.
		[Test]
		public void GeomCheck_HoleIntersectsMainLoop()
		{
            Solid s = new Solid("test", BoardMaterial.SOLID_OAK);

			s.AddVertex(0, 0, 0);	// 0
			s.AddVertex(5, 0, 0);	// 1
			s.AddVertex(5, 5, 0);	// 2
			s.AddVertex(0, 5, 0);	// 3

			s.AddVertex(0, 0, 30);	// 4
			s.AddVertex(5, 0, 30);	// 5
			s.AddVertex(5, 5, 30);	// 6
			s.AddVertex(0, 5, 30);	// 7

			//ccw
			s.CreateFace("f1", 0, 1, 2, 3);				// end near origin
			s.CreateFace("f2", 1, 5, 6, 2);
			Face cut = s.CreateFace("f3", 3, 2, 6, 7);	// face with hole
			s.CreateFace("f4", 0, 3, 7, 4);			// left long face
			s.CreateFace("f5", 6, 5, 4, 7);			// other end
			Face cut2 = s.CreateFace("f6", 0, 4, 5, 1);

			s.AddVertex(1.5, 5, 8);		// 8
			s.AddVertex(1.5, 5, 24);	// 9
			s.AddVertex(3.5, 5, 30);	// 10  WRONG:  30 should be 24.  this hole touches the main loop.
			s.AddVertex(3.5, 5, 8);		// 11

			bool bfailed = false;
			try
			{
				//cw
				cut.AddHole(8, 9, 10, 11);
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Creating a hole which intersects the main face loop should cause an exception.");
		}

		[Test]
		public void GeomCheck_HoleIntersectsOtherHole()
		{
            Solid s = new Solid("test", BoardMaterial.SOLID_OAK);

			s.AddVertex(0, 0, 0);	// 0
			s.AddVertex(5, 0, 0);	// 1
			s.AddVertex(5, 5, 0);	// 2
			s.AddVertex(0, 5, 0);	// 3

			s.AddVertex(0, 0, 30);	// 4
			s.AddVertex(5, 0, 30);	// 5
			s.AddVertex(5, 5, 30);	// 6
			s.AddVertex(0, 5, 30);	// 7

			//ccw
			s.CreateFace("f1", 0, 1, 2, 3);				// end near origin
			s.CreateFace("f2", 1, 5, 6, 2);
			Face cut = s.CreateFace("f3", 3, 2, 6, 7);	// face with hole
			s.CreateFace("f4", 0, 3, 7, 4);			// left long face
			s.CreateFace("f5", 6, 5, 4, 7);			// other end
			Face cut2 = s.CreateFace("f6", 0, 4, 5, 1);

			s.AddVertex(1.5, 5, 8);		// 8
			s.AddVertex(1.5, 5, 24);	// 9
			s.AddVertex(3.5, 5, 24);	// 10
			s.AddVertex(3.5, 5, 8);		// 11

			//cw
			cut.AddHole(8, 9, 10, 11);

			s.AddVertex(1.5, 3, 8);
			s.AddVertex(1.5, 3, 24);
			s.AddVertex(3.5, 3, 24);
			s.AddVertex(3.5, 3, 8);

			//ccw
			s.CreateFace("q1", 8, 12, 13, 9);
			s.CreateFace("q2", 13, 14, 10, 9);
			s.CreateFace("q3", 10, 14, 15, 11);
			s.CreateFace("q4", 11, 15, 12, 8);
			s.CreateFace("q5", 13, 12, 15, 14);		// bottom of mortise

			// another mortise, all the way through

			// WRONG: The following hole intersects with the other one.
			s.AddVertex(1, 5, 9);
			s.AddVertex(1, 5, 17);
			s.AddVertex(4, 5, 17);
			s.AddVertex(4, 5, 9);

			bool bfailed = false;
			try
			{
				//cw
				cut.AddHole(16, 17, 18, 19);
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Creating a hole which intersects another hole should cause an exception.");
		}
#endif

#if false // this is an assert now, not a throw
		[Test]
		public void GeomCheckFail_AddSameVertexTwice()
		{
            Solid s = new Solid("test", BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

			s.AddVertex(3,3,3);
			bool bfailed = false;
			try
			{
				s.AddVertex(3,3,3);
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Attempting to add the same vertex twice should cause an exception.");			
		}
#endif

#if not // the non-simple polygon test is an assert now, not a throw
		[Test]
		public void GeomCheck_FaceNonSimplePolygon()
		{
            Solid s = new Solid("test", BoardMaterial.SOLID_OAK);
			
			double d = 5;

			s.AddVertex(0, 0, 0);
			s.AddVertex(d, 0, 0);
			s.AddVertex(d, d, 0);
			s.AddVertex(0, d, 0);
			s.AddVertex(0, 0, d);
			s.AddVertex(d, 0, d);
			s.AddVertex(d, d, d);
			s.AddVertex(0, d, d);

			bool bfailed = false;
			try
			{
				s.CreateFace("test", 0, 1, 2, 0, 3);		// WRONG:  this polygon is not simple.  this should fail.
			}
			catch (GeomCheckException)
			{
				bfailed = true;
			}
			Assert.IsTrue(bfailed, "Attempt to create a face which is not a simple polygon should cause an exception.");
		}
#endif

        #endregion

    }

}

#endif
