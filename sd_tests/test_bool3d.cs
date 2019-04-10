
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
    public class test_bool3d
    {
        [Test]
        public void test_overlapping_mortises()
        {
            Solid s1 = Builtin_Solids.CreateCube("c1", 8);

            Solid s2a = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "s2a", 2, 2, 12);
            s2a.Translate(1, 1, 0);

            Solid s3 = bool3d.Subtract(s1, s2a);
            Assert.IsTrue(fp.eq_volume(s3.Volume(), 8 * 8 * 8 - 2 * 2 * 8));

            Solid s2b = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "s2b", 2, 2, 12);
            s2b.Translate(2, 2, 1);
            Solid s4 = bool3d.Subtract(s3, s2b);

            Assert.IsTrue(fp.eq_volume(s4.Volume(), 8 * 8 * 8 - 2 * 2 * 8 - 2 * 2 * 8 + 1 * 1 * 8));
        }

        private void check_share(Solid s1, Solid s2, bool b)
        {
            Assert.AreEqual(bool3d.CheckIfTwoSolidsShareAnySpace(s1, s2), b);
            Assert.AreEqual(bool3d.CheckIfTwoSolidsShareAnySpace(s2, s1), b);
        }

        [Test]
        public void test_CheckIfTwoSolidsShareAnySpace()
        {
            Solid s1;
            Solid s2;

            // two identical cubes
            s1 = Builtin_Solids.CreateCube("c1", 5);
            s1.Translate(-(s1.board_origin.x), -(s1.board_origin.y), -(s1.board_origin.z));

            s2 = Builtin_Solids.CreateCube("c2", 5);
            check_share(s1, s2, true);

            // offset and overlapping
            s2.Translate(1, 1, 1);
            check_share(s1, s2, true);

            // too far apart
            s2.Translate(9, 9, 9);
            check_share(s1, s2, false);

            // sharing a face
            s2 = Builtin_Solids.CreateCube("c2", 5);
            s2.Translate(5, 0, 0);
            check_share(s1, s2, false);

            // sharing an edge
            s2 = Builtin_Solids.CreateCube("c2", 5);
            s2.Translate(5, 5, 0);
            check_share(s1, s2, false);

            // sharing one vertex
            s2 = Builtin_Solids.CreateCube("c2", 5);
            s2.Translate(5, 5, 5);
            check_share(s1, s2, false);

            // one smaller, inside the other, sharing faces
            s2 = Builtin_Solids.CreateCube("c2", 3);
            s2.Translate(-(s2.board_origin.x), -(s2.board_origin.y), -(s2.board_origin.z));
            check_share(s1, s2, true);

            // inside the other, not touching
            s2.Translate(1, 1, 1);
            check_share(s1, s2, true);

            s2 = Builtin_Solids.CreatePyramid("c2", 3, 5);
            check_share(s1, s2, true);
            s2.Translate(1, 1, 0);
            check_share(s1, s2, true);
            s2.Translate(0, 0, -5);
            check_share(s1, s2, false);
            s2.Translate(0, 0, 0.25);
            check_share(s1, s2, true);

            s2 = Builtin_Solids.CreatePyramid("c2", 3, 6);
            check_share(s1, s2, true);
            s2.Translate(1, 1, -0.5);
            check_share(s1, s2, true);

            // long stick and a cube
            s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "c2", 2, 2, 12);
            check_share(s1, s2, true);
            s2.Translate(0, 0, -1);
            check_share(s1, s2, true);
            s2.Translate(1, 1, 0);
            check_share(s1, s2, true);

            Plan p = test_plan.CreateCubeIntoHole();
            p.Execute();
            s1 = p.Result.FindSub("c");
            s2 = p.Result.FindSub("b");
            check_share(s1, s2, false);
        }

        [Test]
        public void test_SplitsVEP()
        {
            xyz fn2a = new xyz(0, 0, 1);
            xyz fn2b = new xyz(1, 0, 0);
            xyz n2a = new xyz(1, 0, 0);
            xyz n2b = new xyz(0, 0, 1);

            xyz v1 = new xyz(1, 0, 1);

            bool b = bool3d.SplitsVEP(v1, n2a, n2b, fn2a, fn2b);
            Assert.IsFalse(b);

            b = bool3d.SplitsVEP(-v1, n2a, n2b, fn2a, fn2b);
            Assert.IsTrue(b);

            fn2a = -fn2a;
            fn2b = -fn2b;

            b = bool3d.SplitsVEP(v1, n2a, n2b, fn2a, fn2b);
            Assert.IsTrue(b);

            b = bool3d.SplitsVEP(-v1, n2a, n2b, fn2a, fn2b);
            Assert.IsFalse(b);
        }

        [Test]
        public void test_subtract_nothing()
        {
            Solid sol = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 12, 24, 2);
            double vol = sol.Volume();
            Solid cut = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b2", 12, 36, 2);
            cut.Translate(0, 6, 2);
            sol = bool3d.Subtract(sol, cut);
            Assert.IsTrue(fp.eq_volume(sol.Volume(), vol));
        }

        [Test]
        public void test_rabbet()
        {
            CompoundSolid sol = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 12, 24, 2).ToCompoundSolid();
            Face f = sol.FindFace("b1.top");
            HalfEdge he = f.FindEdge("right");
            sol = wood.DoRabbet(sol, he, 0.5, 0.5, "r1");
            f = sol.FindFace("b1.top");
            he = f.FindEdge("end1");
            sol = wood.DoRabbet(sol, he, 0.5, 0.5, "r2");
            sol.DoGeomChecks();
        }

        [Test]
        public void test_chamfer()
        {
            CompoundSolid sol = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 12, 24, 2).ToCompoundSolid();
            Face f = sol.FindFace("b1.top");
            HalfEdge he = f.FindEdge("right");
            sol = wood.DoChamfer(sol, he, 0.5, "c1");
            f = sol.FindFace("b1.top");
            he = f.FindEdge("end1");
            sol = wood.DoChamfer(sol, he, 0.5, "c2");
            sol.DoGeomChecks();
        }

#if false // TODO fails due to precision problems?
		[Test]
		public void test_tenon_tetrahedron()
		{
			CompoundSolid sol = (CompoundSolid) Builtin_Solids.CreateTetrahedron("t", 10);
			Face f = sol[0].Faces[0];
			HalfEdge he = f.MainLoop[0];
			sol = wood.Tenon_OneCutMethod(sol, he, new xy(3,3), new xyz(1, 1, 1), "t1");
			sol.DoGeomChecks();
		}
#endif

#if false // this test would generate something which is not a manifold
		[Test]
		public void test_weird_case_3()
		{
			List<xyz> a = ut.MakePoly3d(new xyz(0,0,0), new xyz(5,0,0), new xyz(5,3,0), new xyz(3,3,0), new xyz(5,5,0), new xyz(3,8,0), new xyz(2,5,0), new xyz(0,8,0));
			Solid sol = Solid.Sweep("weird", a, new xyz(0,0,12));
			sol = wood.Mortise(sol.Faces[0], (sol.Faces[0]).MainLoop[0], new xy(1,1), new xyz(2,2,20));
			sol.DoGeomChecks();
		}
#endif

        [Test]
        public void test_weird_case_2()
        {
            List<xyz> a = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 3, 0), new xyz(3, 3, 0), new xyz(5, 5, 0), new xyz(3, 8, 0), new xyz(2, 5, 0), new xyz(0, 8, 0));
            CompoundSolid sol = Solid.Sweep("weird", BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), a, new xyz(2, 2, 12)).ToCompoundSolid();
            sol = wood.Mortise(sol, (sol[0].Faces[0]).MainLoop[0], new xy(1, 1), new xyz(3, 3, 20), "m");
            sol.DoGeomChecks();
        }

        [Test]
        public void test_weird_case_1()
        {
            List<xyz> a = ut.MakePoly(new xyz(0, 0, 0), new xyz(5, 0, 0), new xyz(5, 3, 0), new xyz(3, 3, 0), new xyz(5, 5, 0), new xyz(3, 8, 0), new xyz(2, 5, 0), new xyz(0, 8, 0));
            CompoundSolid sol = Solid.Sweep("weird", BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), a, new xyz(0, 0, 12)).ToCompoundSolid();
            sol = wood.Mortise(sol, (sol[0].Faces[0]).MainLoop[0], new xy(1, 1), new xyz(3, 3, 20), "m");
            sol.DoGeomChecks();
        }

        [Test]
        public void test_tenon_methods()
        {
            CompoundSolid sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();
            CompoundSolid t1 = wood.Tenon(sol, sol.FindFace("c.top").MainLoop[0], new xy(1, 2), new xyz(3, 1, 1), "t1");
            Assert.IsTrue(fp.eq_volume(t1.Volume(), 125 - 25 + 3));
            t1.DoGeomChecks();
        }

        [Test]
        public void test_tenon_formerly_1cut()
        {
            CompoundSolid sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();
            sol = wood.Tenon(sol, sol.FindFace("c.top").MainLoop[0], new xy(1, 2), new xyz(3, 1, 1), "t1");
            Assert.IsTrue(fp.eq_volume(sol.Volume(), 125 - 25 + 3));

            sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();
            Face f = sol[0].Faces[3];
            sol = wood.Tenon(sol, sol.FindFace("c.top").MainLoop[0], new xy(1, 2), new xyz(3, 1, 1), "t2");
            Assert.IsTrue(fp.eq_volume(sol.Volume(), 125 - 25 + 3));

            sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();
            Solid cut = Builtin_Solids.CreateCube("d", 5);
            cut.Translate(3, 3, -1);
            sol = bool3d.Subtract(sol, cut);

            sol = wood.Tenon(sol, sol.FindFace("c.top").MainLoop[0], new xy(1, 1), new xyz(1, 1, 1), "t3");
            sol.DoGeomChecks();
        }

        [Test]
        public void test_drill_and_mortise_intersect()
        {
            CompoundSolid sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();

            sol = wood.Drill(sol, sol.FindFace("c.top").FindEdge("end1"), 2.5, 2.5, 1, 0, 0, 4, 8, "d1");
            sol = wood.Mortise(sol, sol.FindFace("c.end1").FindEdge("top"), new xy(1, 1), new xyz(3, 3, 8), "m");

            sol.DoGeomChecks();
        }

        [Test]
        public void test_three_mortises_in_a_cube()
        {
            CompoundSolid sol = Builtin_Solids.CreateCube("c", 5).ToCompoundSolid();

            sol = wood.Mortise(sol, sol.FindFace("c.top").FindEdge("end1"), new xy(1, 1), new xyz(3, 3, 8), "m1");
            sol = wood.Mortise(sol, sol.FindFace("c.right").FindEdge("top"), new xy(1, 1), new xyz(3, 3, 8), "m2");
            sol = wood.Mortise(sol, sol.FindFace("c.end1").FindEdge("top"), new xy(1, 1), new xyz(3, 3, 8), "m3");

            sol.DoGeomChecks();
        }

        [Test]
        public void test_cut_board_in_half()
        {
            // TODO fix this to create two solids?

            Solid b1 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 10, 24, 2);
            Solid b2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 24, 2, 5);
            b2.Translate(-5, 10, -1);
            Solid b3 = bool3d.Subtract(b1, b2);
            b3.DoGeomChecks();
        }

        [Test]
        public void test_subtract_self()
        {
            Solid c1 = Builtin_Solids.CreateCube("c1", 5);
            Solid nothing = bool3d.Subtract(c1, c1);
            Assert.AreEqual(0, nothing.Faces.Count);
            Assert.AreEqual(0, nothing.Vertices.Count);
            Assert.AreEqual(0, nothing.Edges.Count);
        }

        [Test]
        public void test_cuts_naming()
        {
            for (int i = 0; i < 100; i++)
            {
                Solid s1 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "b1", 10, 24, 2);
                Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.PLYWOOD_OAK), "cut", 14, 2, 2);
                s2.Translate(-1, 8, 1);
                Solid s3 = bool3d.Subtract(s1, s2);

                Face f = s3.FindFace("btop");
                Assert.IsNull(f);

                Face f1 = s3.FindFace("top_1");
                Face f2 = s3.FindFace("top_2");

                Assert.IsNotNull(f1);
                Assert.IsNotNull(f2);

                //ut.DumpPoly3d("f1", f1.MainLoop.CollectAllVertices());
                //ut.DumpPoly3d("f2", f2.MainLoop.CollectAllVertices());

                PointFaceIntersection pfi1 = f1.CalcPointFaceIntersection(new xyz(0, 0, 0));
                Assert.AreEqual(PointFaceIntersection.OnEdge, pfi1);

                PointFaceIntersection pfi2 = f2.CalcPointFaceIntersection(new xyz(0, 0, 0));
                Assert.AreEqual(PointFaceIntersection.None, pfi2);
            }
        }

        public void test_rot_cube(double rot)
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 8);

            s2.Rotate_Deg(rot, new xyz(0, 0, 0), new xyz(0, 0, 1));
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void rotated_cubes()
        {
            test_rot_cube(2);
            test_rot_cube(10);
            test_rot_cube(20);
            test_rot_cube(30);
            test_rot_cube(45);
            test_rot_cube(50);
            test_rot_cube(80);
            test_rot_cube(89);
            test_rot_cube(90);
            test_rot_cube(91);
        }

        [Test]
        public void rotated_cube_30_indent()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 8);
            s2.Rotate_Deg(30, new xyz(0, 0, 0), new xyz(0, 0, 1));
            s2.Translate(0, 0, 1);
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void rotated_cube_30_outdent()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 8);
            s2.Rotate_Deg(30, new xyz(0, 0, 0), new xyz(0, 0, 1));
            s2.Translate(0, 0, -1);
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void cylinder_2()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateCylinder("cyl", 2, 20, 30);
            s2.Translate(4, 4, -3);

            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void cylinder_1()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateCylinder("cyl", 4, 20, 30);
            s2.Translate(0, -1, -3);

            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void mortise_through_entire_bookshelf()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            CompoundSolid s1 = p.Result;

            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 2, 2, 50);

            s2.Translate(2, 10, -40);
            s2.RecalcFacePlanes();
            CompoundSolid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void test_subtract_nothing_1()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 10);
            s2.Translate(10, 0, 0);

            const double vol = 10 * 10 * 10;
            Assert.IsTrue(fp.eq_volume(s1.Volume(), vol));
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
            Assert.IsTrue(fp.eq_volume(s3.Volume(), vol));
        }

        [Test]
        public void test_subtract_nothing_2()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 5);
            s2.Translate(10, 0, 0);

            const double vol = 10 * 10 * 10;
            Assert.IsTrue(fp.eq_volume(s1.Volume(), vol));
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
            Assert.IsTrue(fp.eq_volume(s3.Volume(), vol));
        }

        [Test]
        public void test_subtract_nothing_3()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 5);
            s2.Translate(10, 2, 2);

            const double vol = 10 * 10 * 10;
            Assert.IsTrue(fp.eq_volume(s1.Volume(), vol));
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
            Assert.IsTrue(fp.eq_volume(s3.Volume(), vol));
        }

        [Test]
        public void test_subtract_nothing_4()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 10);
            s2.Translate(50, 0, 0);

            const double vol = 10 * 10 * 10;
            Assert.IsTrue(fp.eq_volume(s1.Volume(), vol));
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
            Assert.IsTrue(fp.eq_volume(s3.Volume(), vol));
        }

        [Test]
        public void test_seginside_thing()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut2", 6, 6, 6);

            s2.Translate(2, 2, -1);
            Solid s3 = bool3d.Subtract(s1, s2);

            bsp3d bsp = new bsp3d(s3);

            bool b = s3.SegmentInside(bsp, new xyz(5, 2, 2), new xyz(8, 2, 2));
        }

        [Test]
        public void test_two_mortises_connect_harder()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut2", 6, 6, 6);

            s2.Translate(2, 2, -1);
            Solid sj = bool3d.Subtract(s1, s2);

            Solid s3 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut3", 6, 6, 6);

            s3.Translate(5, 2, 2);
            Solid sk = bool3d.Subtract(sj, s3);
            sk.DoGeomChecks();
        }

        [Test]
        public void test_cut_with_overlaps()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 8);

            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void test_lop_off_half_a_cube()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 20);

            s2.Translate(7, -2, -2);
            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
        }

        [Test]
        public void test_dado_and_through_mortise_angled()
        {
            for (int i = 0; i < 10; i++)
            {
                Solid s1 = Builtin_Solids.CreateCube("s1", 10);
                Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 2, 2, 20);

                s2.Translate(2, -1, -2);
                s2.Rotate_Deg(10, new xyz(0, 0, 0), new xyz(0, 1, 0));
                Solid s3 = bool3d.Subtract(s1, s2);

                Solid s4 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut2", 2, 2, 20);
                s4.Translate(7, 7, -2);

                Solid s5 = bool3d.Subtract(s3, s4);
                s5.DoGeomChecks();
            }
        }

        [Test]
        public void test_dado_and_through_mortise()
        {
            for (int i = 0; i < 10; i++)
            {
                Solid s1 = Builtin_Solids.CreateCube("s1", 10);
                Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut_s2", 2, 2, 20);
                s2.Translate(2, -1, -2);

                Solid s3 = bool3d.Subtract(s1, s2);

                Solid s4 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut_s4", 2, 2, 20);
                s4.Translate(7, 7, -2);

                Solid s5 = bool3d.Subtract(s3, s4);
                s5.DoGeomChecks();
            }
        }

        [Test]
        public void test_two_through_mortises_same_face()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 2, 2, 20);

            s2.Translate(2, 2, -2);
            Solid s3 = bool3d.Subtract(s1, s2);

            Solid s4 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut2", 2, 2, 20);

            s4.Translate(7, 7, -2);
            Solid s5 = bool3d.Subtract(s3, s4);
            s5.DoGeomChecks();
        }

        [Test]
        public void test_two_mortises_that_connect()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 6, 6, 6);
            s2.Translate(2, 2, -1);

            Solid s3 = bool3d.Subtract(s1, s2);

            Solid s4 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut2", 6, 6, 6);
            s4.Translate(5, 3, 2);

            Solid s5 = bool3d.Subtract(s3, s4);
            s5.DoGeomChecks();
        }

        [Test]
        public void test_subtract_mortise()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = Builtin_Solids.CreateCube("s2", 4);

            s2.Translate(2, 2, -2);

            double volume1 = s1.Volume();
            Solid s3 = bool3d.Subtract(s1, s2);
            double volume2 = s3.Volume();
            s3.DoGeomChecks();
            Assert.Less(volume2, volume1);
        }

        [Test]
        public void test_subtract_through_mortise()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            Solid s2 = wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), "cut", 2, 2, 20);

            s2.Translate(2, 2, -2);

            double volume1 = s1.Volume();
            Solid s3 = bool3d.Subtract(s1, s2);
            double volume2 = s3.Volume();
            s3.DoGeomChecks();
            Assert.Less(volume2, volume1);
        }

        [Test]
        public void test_subtract()
        {
            Solid s1 = Builtin_Solids.CreateCube("s1", 10);
            double volume1 = s1.Volume();
            Solid s2 = Builtin_Solids.CreateCube("s2", 8);
            s2.Translate(4, 4, -4);

            Solid s3 = bool3d.Subtract(s1, s2);
            s3.DoGeomChecks();
            double volume2 = s3.Volume();
            Assert.IsTrue(fp.eq_volume(volume2, 784));
            Assert.AreEqual(9, s3.Faces.Count);
        }
    }
}

#endif
