
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
    public class test_plan
    {
        public static Plan CreateInvalidBlocks()
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

            p.AddStep(Action.MORTISE, "Mortise a.end2", Step.CreateParms(
                "path", "a.end2.left",
                "id", "m1",
                "x", "1",
                "y", "1",
                "xsize", "3",
                "ysize", "3",
                "depth", "1"
                ));

            p.AddStep(Action.NEW_BOARD, "b", Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));

            p.AddStep(Action.JOIN, "Join a and b", Step.CreateParms(
                "path1", "a.m1_bottom.m1_left",
                "path2", "b.end2.bottom",
                                                "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            return p;
        }

        public static Plan CreateBookShelf()
        {
            // TODO do we want to give this stock plan a predefined guid?

            Plan p = new Plan("Bookshelf");

            p.DefineVariable(new VariableDefinition("depth", "Depth of the shelf", 4, 16, 6, 4));
            p.DefineVariable(new VariableDefinition("height", "Height of the shelf", 16, 60, 30, 4));
            p.DefineVariable(new VariableDefinition("width", "Width of the shelf", 16, 64, 24, 4));
            p.DefineVariable(new VariableDefinition("thickness", "Thickness of the boards being used", 0.5, 4, 1, 32));

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "bottom", Step.CreateParms(
"material", "solid.oak",
"newname", "bottom",
"width", "depth",
#if BOOKSHELF_DOVETAIL
 "length", "width + thickness*2",
#else
 "length", "width",
#endif
 "thickness", "thickness"
));
            p.AddStep(Action.NEW_BOARD, "left_vertical", Step.CreateParms(
"material", "solid.oak",
"newname", "left_vertical",
"width", "depth",
"length", "height",
"thickness", "thickness"
));
            p.AddStep(Action.NEW_BOARD, "right_vertical", Step.CreateParms(
"material", "solid.oak",
"newname", "right_vertical",
"width", "depth",
"length", "height",
"thickness", "thickness"
));
            p.AddStep(Action.NEW_BOARD, "back", Step.CreateParms(
"material", "plywood.oak",
"newname", "back",
"width", "width",
"length", "height - thickness",
"thickness", "0.5"
));
            p.AddStep(Action.NEW_BOARD, "top", Step.CreateParms(
"material", "solid.oak",
"newname", "top",
"width", "depth + 1",
"length", "width + thickness*2 + 2",
"thickness", "thickness"
));
            p.AddStep(Action.NEW_BOARD, "lowershelf", Step.CreateParms(
"material", "solid.oak",
"newname", "lowershelf",
"width", "depth - 0.5",
"length", "width + thickness",
"thickness", "thickness"
));
            p.AddStep(Action.NEW_BOARD, "uppershelf", Step.CreateParms(
"material", "solid.oak",
"newname", "uppershelf",
"width", "depth - 0.5",
"length", "width + thickness",
"thickness", "thickness"
));

            p.AddStep(Action.DADO, "Cut left dado for lower shelf", Step.CreateParms(
"path", "left_vertical.top.end1",
"id", "d1",
"dist", "height/3",
"width", "thickness",
"depth", "thickness/2"
));
            p.AddStep(Action.DADO, "Cut left dado for upper shelf", Step.CreateParms(
"path", "left_vertical.top_2.end2",
"id", "d2",
"dist", "height/3",
"width", "thickness",
"depth", "thickness/2"
));
            p.AddStep(Action.DADO, "Cut right dado for lower shelf", Step.CreateParms(
"path", "right_vertical.top.end1",
"id", "d3",
"dist", "height/3",
"width", "thickness",
"depth", "thickness/2"
));
            p.AddStep(Action.DADO, "Cut right dado for upper shelf", Step.CreateParms(
"path", "right_vertical.top_2.end2",
"id", "d4",
"dist", "height/3",
"width", "thickness",
"depth", "thickness/2"
));

#if BOOKSHELF_DOVETAIL
            p.AddStep(Action.DOVETAIL_TAILS, "Join left side to bottom", Step.CreateParms(
"path1", "bottom.end2.bottom",
"path2", "left_vertical.end1.bottom",
"numtails", "4",
"tailwidth", "0",
"id", "dtleft"
));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dtleft"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dtleft"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, "Join right side to bottom", Step.CreateParms(
"path1", "bottom.end1.bottom",
"path2", "right_vertical.end1.bottom",
"numtails", "4",
"tailwidth", "0",
"id", "dtright"
));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dtright"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dtright"
                ));
            p.AddStep(Action.JOIN, "Join back to the assembly", Step.CreateParms(
"path1", "bottom.top.right",
"path2", "back.end1.top",
 "align", "left",
"offset1", "thickness", "offset2", "0"
));
#else
            p.AddStep(Action.JOIN, "Join left side to bottom", Step.CreateParms(
"path1", "bottom.end2.bottom",
"path2", "left_vertical.top_1.end1",
 "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Join right side to bottom", Step.CreateParms(
"path1", "bottom.end1.bottom",
"path2", "right_vertical.top_1.end1",
 "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Join back to the assembly", Step.CreateParms(
"path1", "bottom.top.right",
"path2", "back.end1.top",
 "align", "right",
"offset1", "0", "offset2", "0"
));
#endif

            // TODO this should be Join_Dado, a special case -- it should be simpler to join into a dado since we know about it

            p.AddStep(Action.JOIN, "Insert lower shelf", Step.CreateParms(
"path1", "right_vertical.d3_bottom.d3_side_below",
"path2", "lowershelf.end1.top",
 "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.JOIN, "Insert upper shelf", Step.CreateParms(
"path1", "right_vertical.d4_bottom.d4_side_above",
"path2", "uppershelf.end1.top",
 "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.CHAMFER, "Route the top front", Step.CreateParms(
"path", "top.top.left",
"id", "chamfer1",
"inset", "thickness/2"
));
            p.AddStep(Action.CHAMFER, "Route the top right", Step.CreateParms(
"path", "top.top.end1",
"id", "chamfer2",
"inset", "thickness/2"
));
            p.AddStep(Action.CHAMFER, "Route the top left", Step.CreateParms(
"path", "top.top.end2",
"id", "chamfer3",
"inset", "thickness/2"
));

            p.AddStep(Action.JOIN, "Join top to the assembly", Step.CreateParms(
"path1", "back.end2.top",
"path2", "top.bottom.right",
 "align", "center",
"offset1", "0", "offset2", "0"
));

            return p;
        }

        [Test]
        public void test_halfedge_vectors()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            Solid s = p.Result.Subs[0];
            Face f = s.Faces[0];
            HalfEdge he = f.MainLoop[0];
            xyz v = he.Vector();
            xyz vn = he.UnitVector();
            Assert.IsNotNull(v);
            Assert.IsNotNull(vn);
            v.normalize_in_place();
            Assert.IsTrue(fp.eq_unitvec(v, vn));
        }


        [Test]
        public void test_face_tostring()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Face f in p.Result.Subs[0].Faces)
            {
                string s = f.ToString();
                Assert.IsNotNull(s);
            }
        }

        [Test]
        public void test_execute_with_without_threads()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute(true);
            double v1 = p.Result.Volume();
            p.Execute(false);
            double v2 = p.Result.Volume();
            Assert.AreEqual(v1, v2);
        }

        [Test]
        public void test_step_notes()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                string s = st.Notes;
                Assert.IsNotNull(s);
            }
        }

        [Test]
        public void test_clone_step()
        {
            Plan p = test_plan.CreateBookShelf();
            foreach (Step st in p.Steps)
            {
                Step s2 = st.Clone();
                Assert.IsNotNull(s2);
            }
        }

        private void check_action_strings(Plan p)
        {
            foreach (Step st in p.Steps)
            {
                string s = Step.GetActionString(st.action);
                Assert.IsNotNull(s);
            }
        }

        [Test]
        public void test_GetActionString()
        {
            check_action_strings(test_plan.CreateBookShelf());
        }

        [Test]
        public void test_Views()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                double h = st.ViewRotY;
                double v = st.ViewRotX;
                double z = st.ViewZoom;

                st.ViewRotY = 0;
                st.ViewRotX = 0;
                st.ViewZoom = 1;
            }
        }

#if not
        [Test]
        public void test_CreateNonManifold()
        {
            Plan p = CreateNonManifold();
            p.Execute();
            Assert.AreNotEqual(0, p.ErrorCount);
        }
#endif

        public static Plan CreateNonManifold()
        {
            Plan p = new Plan("c");
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "4",
                "length", "4",
                "thickness", "4"
                ));
            p.AddStep(Action.MORTISE, "m1", Step.CreateParms(
                "path", "c.top.end1",
                "id", "m1",
                "x", "1",
                "y", "1",
                "xsize", "1",
                "ysize", "1",
                "depth", "5"
                ));
            p.AddStep(Action.MORTISE, "m2", Step.CreateParms(
                "path", "c.top.end1",
                "id", "m2",
                "x", "2",
                "y", "2",
                "xsize", "1",
                "ysize", "1",
                "depth", "5"
                ));
            return p;
        }

        public static Plan CreateCubeWithSmallHole()
        {
            Plan p = new Plan("cube16th");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "4",
                "length", "4",
                "thickness", "4"
                ));
            p.AddStep(Action.DRILL, "drill hole in top", Step.CreateParms(
                "path", "c.top.end1",
                "x", "2",
                "y", "2",
                "diam", "1/16",
                "depth", "6",
                "id", "d1"
                ));
            return p;
        }

        [Test]
        public void test_smallhole()
        {
            Plan p = CreateCubeWithSmallHole();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

        [Test]
        public void test_tenon_in_face_with_hole()
        {
            Plan p = CreateTenonInFaceWithHole();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
            Assert.AreEqual(1000, p.Steps[1].Result.Volume());
            Assert.AreEqual(1000 - 5 * 1 * 1, p.Steps[2].Result.Volume());

            double vol = 10 * 10 * 10; // starting cube
            vol -= 2 * 10 * 10; // the full slice for the tenon
            vol += 3 * 3 * 2; // add the tenon back.  this would be the volume if not for the mortise
            vol -= 3 * 1 * 1; // remove the portion of the mortise that still remains
            Assert.AreEqual(vol, p.Steps[3].Result.Volume());
        }

        [Test]
        public void test_CreateTenonInFaceWithHoleBool3dBug()
        {
            Plan p = CreateTenonInFaceWithHoleBool3dBug();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

        [Test]
        public void test_CreateTenonBug()
        {
            Plan p = CreateTenonBug();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

        public static Plan CreateTenonInFaceWithHole()
        {
            Plan p = new Plan("CreateTenonInFaceWithHole");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "a",
                "width", "10",
                "length", "10",
                "thickness", "10"
                ));
            p.AddStep(Action.MORTISE, "mortise", Step.CreateParms(
                "path", "a.top.end1",
                "id", "m1",
                "x", "1",
                "y", "1",
                "xsize", "1",
                "ysize", "1",
                "depth", "5"
                ));
            p.AddStep(Action.TENON, "tenon", Step.CreateParms(
                "path", "a.top.end1",
                "id", "t1",
                "x", "3",
                "y", "3",
                "xsize", "3",
                "ysize", "3",
                "depth", "2"
                ));

            return p;
        }

        public static Plan CreateTenonInFaceWithHoleBool3dBug()
        {
            Plan p = new Plan("CreateTenonInFaceWithHole");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "a",
                "width", "10",
                "length", "10",
                "thickness", "10"
                ));
            p.AddStep(Action.MORTISE, "mortise", Step.CreateParms(
                "path", "a.top.end1",
                "id", "m1",
                "x", "1",
                "y", "1",
                "xsize", "1",
                "ysize", "1",
                "depth", "5"
                ));
            p.AddStep(Action.TENON, "tenon", Step.CreateParms(
                "path", "a.top.end1",
                "id", "t1",
                "x", "1.5",
                "y", "1",
                "xsize", "3",
                "ysize", "3",
                "depth", "2"
                ));

            return p;
        }

        public static Plan CreateTenonBug()
        {
            Plan p = new Plan("tenonbug");

            p.DefineVariable(new VariableDefinition("Height", "The total height of the bench, from the floor to the top.", 30, 40, 35, 4));
            p.DefineVariable(new VariableDefinition("Top_Slab_Length", "The length of the two top slabs.  The total length of the bench will be slightly longer because of the addition of the end caps and vise.", 60, 96, 78, 4));
            p.DefineVariable(new VariableDefinition("Top_Slab_Width", "The width of each of the two top slabs.", 12, 18, 14, 1));
            p.DefineVariable(new VariableDefinition("Tray_Width", "The width of the center tray between the two slabs of the top", 4, 12, 8, 1));
            p.DefineVariable(new VariableDefinition("Top_Thickness", "The thickness of the two slabs of the top", 1.5, 5, 4, 2));

            p.DefineVariable(new VariableDefinition("Leg_Thickness", "Thickness of the legs, feet and bearers", 2, 5, 4, 4));

            p.DefineVariable(new VariableDefinition("stretcher_width", "4"));
            p.DefineVariable(new VariableDefinition("traythickness", "0.5"));
            p.DefineVariable(new VariableDefinition("pad_thickness", "1"));
            p.DefineVariable(new VariableDefinition("stretcher_thickness", "1"));
            p.DefineVariable(new VariableDefinition("leglength", "Height - Top_Thickness - Leg_Thickness - pad_thickness - Leg_Thickness + Leg_Thickness/2 + Leg_Thickness/2"));
            p.DefineVariable(new VariableDefinition("footlength", "Top_Slab_Width*2+Tray_Width - Front_Overhang*2"));

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "Create leg 11", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "leg11",
                "width", "Leg_Thickness",
                "length", "leglength",
                "thickness", "Leg_Thickness"
                ));

            p.AddStep(Action.MORTISE, "Cut stretcher mortise in leg11", Step.CreateParms(
                "path", "leg11.right.end1",
                "id", "leg11_mortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for bottom of leg11", Step.CreateParms(
                "path", "leg11.end1.top",
                "id", "leg11_tenon1",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "5"  // was Leg_Thickness/2.  Leave it at Leg_Thickness and this crashes in the bool3d code.
                ));

            return p;
        }

        [Test]
        public void test_glue_stuff()
        {
            Plan p = CreateGlueTestPlan();
            p.Execute();
            GlueJointScore sc = p.Steps[3].GetGlueJointScore();
            Assert.IsTrue(sc.faces[0].Grains == GrainGrain.Long_Long);
            sc = p.Steps[5].GetGlueJointScore();
            Assert.IsTrue(sc.faces[0].Grains == GrainGrain.Cross);
            sc = p.Steps[7].GetGlueJointScore();
            Assert.IsTrue(sc.faces[0].Grains == GrainGrain.End_End);
        }

        public static Plan CreateGlueTestPlan()
        {
            Plan p = new Plan("gluetest");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "board", Step.CreateParms(
        "material", "solid.maple.hard",
        "newname", "a",
        "width", "8",
        "length", "8",
        "thickness", "2"
        ));

            p.AddStep(Action.NEW_BOARD, "board", Step.CreateParms(
        "material", "solid.maple.hard",
        "newname", "b",
        "width", "8",
        "length", "8",
        "thickness", "2"
        ));

            p.AddStep(Action.JOIN, "good join", Step.CreateParms(
                 "path1", "a.top.right",
                 "path2", "b.bottom.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "board", Step.CreateParms(
        "material", "solid.maple.hard",
        "newname", "c",
        "width", "8",
        "length", "8",
        "thickness", "2"
        ));

            p.AddStep(Action.JOIN, "crossgrain", Step.CreateParms(
                 "path1", "b.top.right",
                 "path2", "c.bottom.end1",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "board", Step.CreateParms(
        "material", "solid.maple.hard",
        "newname", "d",
        "width", "8",
        "length", "8",
        "thickness", "2"
        ));

            p.AddStep(Action.JOIN, "endgrain", Step.CreateParms(
                 "path1", "c.end1.top",
                 "path2", "d.end1.top",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "board", Step.CreateParms(
        "material", "solid.maple.hard",
        "newname", "e",
        "width", "8",
        "length", "8",
        "thickness", "2"
        ));

            p.AddStep(Action.JOIN, "end/long", Step.CreateParms(
                 "path1", "d.top.end2",
                 "path2", "e.end1.top",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            return p;
        }

        [Test]
        public void Test_WBProblemwithsolidoverlaps()
        {
            Plan p = CreateWBTopProblem();
            p.Execute();
            Assert.IsFalse(p.Result.IsValidWithNoSubOverlaps());
        }

        public static Plan CreateWBTopProblem()
        {
            Plan p = new Plan("Workbench");

            p.DefineVariable(new VariableDefinition("height", "The total height of the bench, from the floor to the top.", 30, 40, 35, 4));
            p.DefineVariable(new VariableDefinition("length", "TODO help", 60, 96, 80, 4));
            p.DefineVariable(new VariableDefinition("topdepth", "TODO help", 12, 18, 14, 1));
            p.DefineVariable(new VariableDefinition("top_thickness", "TODO help", 1.5, 5, 4, 2));
            p.DefineVariable(new VariableDefinition("leg_thickness", "TODO help", 2, 5, 4, 4));
            p.DefineVariable(new VariableDefinition("stretcher_width", "TODO help", 2, 4, 4, 1));
            p.DefineVariable(new VariableDefinition("overhangfrontback", "TODO help", 0, 12, 4, 1));
            p.DefineVariable(new VariableDefinition("overhangright", "TODO help", 0, 18, 16, 1)); // need 16 inch overhang for the vise
            p.DefineVariable(new VariableDefinition("overhangleft", "TODO help", 0, 18, 6, 1));
            p.DefineVariable(new VariableDefinition("traywidth", "TODO help", 6, 12, 8, 1));
            p.DefineVariable(new VariableDefinition("dogholeinset", "TODO help", 2, 6, 2, 1));
            p.DefineVariable(new VariableDefinition("dogholespacing", "TODO help", 4, 20, 6, 1));
            p.DefineVariable(new VariableDefinition("firstdoghole", "TODO help", 2, 12, 3, 1));
            p.DefineVariable(new VariableDefinition("frontviselength", "TODO help", 16, 24, 21, 1));

            p.DefineVariable(new VariableDefinition("traythickness", "0.5"));
            p.DefineVariable(new VariableDefinition("pad_thickness", "1"));
            p.DefineVariable(new VariableDefinition("stretcher_thickness", "1"));
            p.DefineVariable(new VariableDefinition("leglength", "height - top_thickness - leg_thickness - pad_thickness - leg_thickness + leg_thickness/2 + leg_thickness/2"));
            p.DefineVariable(new VariableDefinition("shortstretcherheight", "leglength / 2 - (stretcher_width-2)/2"));
            p.DefineVariable(new VariableDefinition("footlength", "topdepth*2+traywidth - overhangfrontback*2"));

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "Create the front half of the top", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "fronttop",
                "width", "topdepth",
                "length", "length",
                "thickness", "top_thickness"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the front tray support", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "fronttraysupport",
                "width", "0.5",
                "length", "length",
                "thickness", "0.5"
                ));

            p.AddStep(Action.JOIN, "Attach front tray support", Step.CreateParms(
                 "path1", "fronttop.right.bottom",
                 "path2", "fronttraysupport.bottom.left",
                 "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create bearer1", Step.CreateParms(
    "material", "solid.oak.red",
    "newname", "bearer1",
    "width", "leg_thickness",
    "length", "topdepth*2+traywidth - overhangfrontback*2",
    "thickness", "leg_thickness"
    ));

            p.AddStep(Action.NEW_BOARD, "Create traybearer1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "traybearer1",
                "width", "leg_thickness",
                "length", "traywidth",
                "thickness", "0.5"
                ));

            p.AddStep(Action.JOIN, "Attach traybearer1", Step.CreateParms(
                 "path1", "traybearer1.bottom.right",
                 "path2", "bearer1.bottom.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach front top", Step.CreateParms(
     "path1", "fronttop.bottom.left",
     "path2", "bearer1.bottom.end1",
     "align", "right",
    "offset1", "overhangleft", "offset2", "-overhangfrontback"
    ));

            return p;
        }

#if false
        [Test]
        public void test_VerifyVariable()
        {
            Plan p = test_plan.CreateBookShelf();
            List<double> failed = p.VerifyVariable("depth");
            Assert.AreEqual(0, failed.Count);
        }

        public static Plan CreateCubeIntoHole_Impossible()
        {
            Plan p = new Plan("cube");
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise 1", Step.CreateParms(
             "path", "c.top.end1",
             "id", "m1",
             "x", "1",
             "y", "1",
             "dx", "3",
             "dy", "3",
             "depth", "3"
             ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "d",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.JOIN, "Attach d onto c, covering the mortise", Step.CreateParms(
                 "path1", "c.top.end1",
                 "path2", "d.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "3",
                "length", "3",
                "thickness", "3"
                ));
            p.AddStep(Action.JOIN, "Attach b into c", Step.CreateParms(
                 "path1", "c.m1_front.top",
                 "path2", "b.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            return p;
        }
#endif

#if not
        [Test]
        public void test_absurd_join_animation()
        {
            Plan p = CreateCubeIntoHole_Impossible();
            p.Execute();
            List<TriBag> bags;
            foreach (Step st in p.Steps)
            {
                if (st.action == Action.JOIN)
                {
                    bags = st.AnimateJoin(true);
                }
                else
                {
                    bags = st.Result.CreateExplodedViewAnimation(true);
                }
            }
            // TODO test to see if there is a bag with a zero vector
            // TODO later, test to see if there is a warning that the join is impossible
        }
#endif

        public void test_highlights(Step st)
        {
            List<Face> highlights = st.GetHighlightedFacesForThisStep();
            // TODO put in a better test for the dado thing
            // TODO and the tenon thing
            switch (st.action)
            {
                case Action.INTRO:
                case Action.NEW_BOARD:
                case Action.JOIN:
                case Action.DOVETAIL_JOIN:
                    {
                        Assert.IsNull(highlights);
                        break;
                    }
                default:
                    {
                        Assert.Greater(highlights.Count, 0);
                        break;
                    }
            }
        }

        [Test]
        public void test_GlueJointScore()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                if (st.IsJoin())
                {
                    GlueJointScore sc = st.GetGlueJointScore();
                    Assert.IsTrue(sc != null);
                }
            }
        }

        [Test]
        public void test_CreateBags()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                BunchOfTriBags bunch = st.GetBunch(true);
                Assert.IsTrue(bunch != null);
                Assert.IsTrue(bunch.notmoving.Count > 0);
            }
        }

        [Test]
        public void test_GetTriangles()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                List<Triangle3d> tris = st.Result.GetTriangles();
                Assert.IsTrue(tris != null);
                Assert.IsTrue(tris.Count > 0);
            }
        }

        [Test]
        public void test_GetHighlightedFacesForThisStep()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            foreach (Step st in p.Steps)
            {
                test_highlights(st);
            }
        }

#if true
        public static Plan CreateTwoDrilledHolesInACube()
        {
            Plan p = new Plan("cube3m");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "4",
                "length", "4",
                "thickness", "4"
                ));
            p.AddStep(Action.DRILL, "drill hole in top", Step.CreateParms(
                "path", "c.top.end1",
                "x", "2",
                "y", "2",
                "diam", "2",
                "depth", "6",
                "id", "d1"
                ));
            p.AddStep(Action.DRILL, "drill hole in end1", Step.CreateParms(
                "path", "c.end1.top",
                "x", "2",
                "y", "2",
                "diam", "2",
                "depth", "6",
                "id", "d2"
                ));
#if false // THREE intersecting drills causes a bunch of other problems
            p.AddStep(Action.DRILL, "drill hole in right", Step.CreateParms(
                "path", "c.right.top",
                "x", "2",
                "y", "2",
                "diam", "2",
                "depth", "6",
                "id", "d3"
                ));
#endif
            return p;
        }

        [Test]
        public void test_CreateTwoDrilledHolesInACube()
        {
            Plan p = CreateTwoDrilledHolesInACube();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

#endif

        public static Plan create_two_mortises_overlapping()
        {
            Plan p = new Plan("cube");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise 1", Step.CreateParms(
             "path", "c.top.end1",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "6"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise 2", Step.CreateParms(
             "path", "c.top.end1",
             "id", "m2",
             "x", "1.5",
             "y", "1.5",
             "xsize", "1",
             "ysize", "1",
             "depth", "6"
             ));
            return p;
        }

        [Test]
        public void test_two_mortises_overlapping()
        {
            Plan p = create_two_mortises_overlapping();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

#if not
        [Test]
        public void test_pp_segment_ApproximatelyEqualTo()
        {
            ppi2d.pp_segment s1 = new ppi2d.pp_segment(new xy(0, 0), new xy(5, 5));
            ppi2d.pp_segment s2 = new ppi2d.pp_segment(new xy(5, 5), new xy(0, 0));
            ppi2d.pp_segment s3 = new ppi2d.pp_segment(new xy(0, 0), new xy(5, 5));
            ppi2d.pp_segment s4 = new ppi2d.pp_segment(new xy(5, 5), new xy(0, 0));

            Assert.IsTrue(s1.ApproximatelyEqualTo(s2));
            Assert.IsTrue(s1.ApproximatelyEqualTo(s3));
            Assert.IsTrue(s1.ApproximatelyEqualTo(s4));
        }
#endif

        [Test]
        public void test_variable_change_listener()
        {
            Plan p = new Plan("cube");
            p.DefineVariable(new VariableDefinition("x", "test_variable_change_listener", 0, 100, 1, 1));
            VariableDefinition vd = p.FindVariable("x");
            vd.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vd_PropertyChanged);

            b_hit_test_variable_change_listener = false;
            vd.Value = 33;
            Assert.IsTrue(b_hit_test_variable_change_listener);
            Assert.IsTrue(fp.eq_unknowndata(33, p.GetVariable("x")));

            b_hit_test_variable_change_listener = false;
            p.SetVariable("x", 44);
            Assert.IsTrue(b_hit_test_variable_change_listener);
            Assert.IsTrue(fp.eq_unknowndata(44, p.GetVariable("x")));
        }

        private bool b_hit_test_variable_change_listener;

        void vd_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            b_hit_test_variable_change_listener = true;
        }

        public static Plan CreateCubeIntoHole()
        {
            Plan p = new Plan("cube");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise 1", Step.CreateParms(
             "path", "c.top.end1",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "3",
             "ysize", "3",
             "depth", "6"
             ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "3",
                "length", "3",
                "thickness", "3"
                ));
            p.AddStep(Action.JOIN, "Attach b into c", Step.CreateParms(
                 "path1", "c.m1_front.top",
                 "path2", "b.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            return p;
        }

        [Test]
        public void test_CreateLotsOfCubesGlued()
        {
            Plan p = CreateLotsOfCubesGlued();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

        public static Plan CreateLotsOfCubesGlued()
        {
            Plan p = new Plan("SevenCubesGlued");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "middle",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c1",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c2",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c3",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c4",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c5",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c6",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "d1",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "d2",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.JOIN, "Attach c1 to middle", Step.CreateParms(
                 "path1", "middle.top.end1",
                 "path2", "c1.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach c2 to middle", Step.CreateParms(
                 "path1", "middle.bottom.end1",
                 "path2", "c2.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach c3 to middle", Step.CreateParms(
                 "path1", "middle.left.top",
                 "path2", "c3.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach c4 to middle", Step.CreateParms(
                 "path1", "middle.right.top",
                 "path2", "c4.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach c5 to middle", Step.CreateParms(
                 "path1", "middle.end1.top",
                 "path2", "c5.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach c6 to middle", Step.CreateParms(
                 "path1", "middle.end2.top",
                 "path2", "c6.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach d1 to a corner", Step.CreateParms(
                 "path1", "c6.right.bottom",
                 "path2", "d1.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            p.AddStep(Action.JOIN, "Attach d2 to a corner", Step.CreateParms(
                 "path1", "c3.right.bottom",
                 "path2", "d2.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));
            return p;
        }

        [Test]
        public void test_plans_no_errors()
        {
            Plan p;

            p = test_plan.CreateBookShelf();
            p.Execute();
            Assert.AreEqual(0, p.ErrorCount);
        }

        [Test]
        public void test_two_mortises()
        {
            Plan p = new Plan("2");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
        "material", "solid.oak",
        "newname", "a",
        "width", "10",
        "length", "24",
        "thickness", "2"
        ));

            p.AddStep(Action.MORTISE, "m1", Step.CreateParms(
                "path", "a.top.right",
                "id", "m1",
                "x", "5",
                "y", "4",
                "xsize", "1",
                "ysize", "1",
                "depth", "3"
                ));

            p.AddStep(Action.MORTISE, "m2", Step.CreateParms(
                "path", "a.top.right",
                "id", "m2",
                "x", "15",
                "y", "4",
                "xsize", "1",
                "ysize", "1",
                "depth", "3"
                ));

            p.Execute();
        }

        [Test]
        public void test_mortise_through_bookshelf()
        {
            Plan p = test_plan.CreateBookShelf();
            p.AddStep(Action.MORTISE, "absurd mortise through entire shelf", Step.CreateParms(
                "path", "top.top.right",
                "id", "m",
                "x", "12",
                "y", "3",
                "xsize", "1",
                "ysize", "1",
                "depth", "100"
                ));
            p.Execute();

            Assert.IsTrue(p.LastStep.Errors.Count == 0);
            Assert.IsTrue(p.LastStep.Warnings.Count > 0);
        }

        [Test]
        public void test_drill_through_bookshelf()
        {
            Plan p = test_plan.CreateBookShelf();
            p.AddStep(Action.DRILL, "absurd drill through entire shelf", Step.CreateParms(
                "path", "top.top.right",
                "x", "12",
                "y", "3",
                "diam", "1",
                "depth", "100",
                "id", "d1"
                ));
            p.Execute();

            Assert.IsTrue(p.LastStep.Errors.Count == 0);
            Assert.IsTrue(p.LastStep.Warnings.Count > 0);
        }

        [Test]
        public void test_crosscut_notreally()
        {
            Plan p = new Plan("Test");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
        "material", "solid.oak",
        "newname", "a",
        "width", "10",
        "length", "24",
        "thickness", "2"
        ));

            p.AddStep(Action.CROSSCUT, "miter a", Step.CreateParms(
                "path", "a.top.left",
                "dist", "4",
                "miter", "-30",
                "tilt", "-45"
                ));

            p.Execute();

            Assert.IsTrue(p.Steps[2].Warnings.Count > 0);
        }

        [Test]
        public void test_rip_notreally()
        {
            Plan p = new Plan("Test");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
        "material", "solid.oak",
        "newname", "a",
        "width", "10",
        "length", "60",
        "thickness", "2"
        ));

            p.AddStep(Action.RIP, "taper a", Step.CreateParms(
                "path", "a.top.end1",
                "dist", "2",
                "taper", "2",
                "tilt", "0"
                ));

            p.Execute();

            Assert.IsTrue(p.Steps[2].Warnings.Count > 0);
        }

        [Test]
        public void test_simple_step_error()
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

            p.AddStep(Action.CHAMFER, "chamfer edge between a.end1 and a.left", Step.CreateParms(
                "path", "a.end1.not",
                "id", "c1",
                "inset", "2"
                ));

            p.Execute();

            Assert.IsTrue(p.Steps[1].Errors.Count == 0);
            Assert.IsTrue(p.Steps[2].Errors.Count > 0);
        }

        [Test]
        public void test_VariableDefinition()
        {
            Plan p = new Plan("test_VariableDefinition");
            p.DefineVariable(new VariableDefinition("x", "test_VariableDefinition", 0, 22, 1, 64));
            p.DefineVariable(new VariableDefinition("y", "test_VariableDefinition", 0, 22, 1, 64));
            p.DefineVariable(new VariableDefinition("z", "test_VariableDefinition", 3, 7, 4, 64));
            VariableDefinition vd = p.FindVariable("x");

            vd.Value = 17;

            vd = p.FindVariable("y");

            p.SetVariable("z", 5);
            Assert.AreEqual(p.GetVariable("z"), 5);

            bool bthrow = false;
            try
            {
                p.SetVariable("z", 2);
            }
            catch
            {
                bthrow = true;
            }
            Assert.IsTrue(bthrow);

            bthrow = false;
            try
            {
                p.SetVariable("z", 7.1);
            }
            catch
            {
                bthrow = true;
            }
            Assert.IsTrue(bthrow);
        }

        [Test]
        public void test_FaceQuality_45()
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

            p.AddStep(Action.CHAMFER, "chamfer edge between a.end1 and a.left", Step.CreateParms(
                "path", "a.end1.left",
                "id", "ch1",
                "inset", "2"
                ));

            p.Execute();

            Face f = p.Result.FindFace("a.ch1_2");
            FaceQuality fq = f.GetQuality();
            // TODO currently the grain on a 45 degree cut is considered Good, but this could change
            Assert.AreEqual(fq, FaceQuality.Good);
        }

        [Test]
        public void test_FaceQuality()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            Face f = p.Result.FindFace("bottom.top");
            Assert.AreEqual(f.GetQuality(), FaceQuality.Good);
            f = p.Result.FindFace("top.end1");
            Assert.AreEqual(f.GetQuality(), FaceQuality.EndGrain);
            f = p.Result.FindFace("back.end1");
            Assert.AreEqual(f.GetQuality(), FaceQuality.Ugly);
            f = p.Result.FindFace("back.top");
            Assert.AreEqual(f.GetQuality(), FaceQuality.Good);
        }

        [Test]
        public void test_step_desc()
        {
            Plan p = test_plan.CreateBookShelf();
            foreach (Step st in p.Steps)
            {
                Assert.IsNotNull(st.Description);
            }
        }

        [Test]
        public void test_NextStep()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();

            Step st = p.Steps[4];
            Assert.AreSame(st.NextStep, p.Steps[5]);

            Assert.IsNull(p.Steps[p.Steps.Count - 1].NextStep);
        }

        public void test_Prose_helper(Plan p)
        {
            p.Execute();
            foreach (Step st in p.Steps)
            {
                string s = st.GetProse();
                Assert.IsNotNull(s);
            }
        }

        [Test]
        public void test_Prose()
        {
            test_Prose_helper(test_plan.CreateBookShelf());
        }

        public void lookup(CompoundSolid cs, string path, bool bShouldFail)
        {
            bool bfailed = false;
            try
            {
                Solid sol;
                Face f;
                HalfEdge he;

                cs.Lookup(path, out sol, out f, out he);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed == bShouldFail);
        }

        [Test]
        public void test_involves_simple()
        {
            Plan p = new Plan("c");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.Execute();
            Assert.IsTrue(p.LastStep.Involves("a"));
        }

        [Test]
        public void test_failed_lookups_2()
        {
            Plan p = new Plan("c");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.Execute();

            lookup(p.Result, "not", true);
            lookup(p.Result, "not.not", true);
            lookup(p.Result, "not.not.not", true);
            lookup(p.Result, "a.not", true);
            lookup(p.Result, "a.not.not", true);
            lookup(p.Result, "a.top.not", true);
            lookup(p.Result, "a.top.end1", false);
        }

        [Test]
        public void test_failed_lookups()
        {
            Plan p = new Plan("c");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise", Step.CreateParms(
             "path", "not",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "0.25"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise", Step.CreateParms(
             "path", "not.not",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "0.25"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise", Step.CreateParms(
             "path", "not.not.not",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "0.25"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise", Step.CreateParms(
             "path", "a.not.not",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "0.25"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise", Step.CreateParms(
             "path", "a.top.not",
             "id", "m1",
             "x", "1",
             "y", "1",
             "xsize", "1",
             "ysize", "1",
             "depth", "0.25"
             ));
            p.Execute();
            for (int i = 2; i < p.Steps.Count; i++)
            {
                Assert.IsTrue(p.Steps[i].Errors.Count > 0);
            }
        }

        [Test]
        public void test_field_definition()
        {
            Plan p = test_plan.CreateBookShelf();

            VariableDefinition vd = p.FindVariable("depth");
            Assert.AreEqual(vd.Name, "depth");
            Assert.IsTrue(fp.eq_unknowndata(vd.max, 16));
            Assert.IsTrue(fp.eq_unknowndata(vd.min, 4));

            p.SetVariable(vd, 12);

            p.Execute();

            FieldDefinition fd = p.Steps[1].FindField("width");
            Assert.AreEqual(fd.Name, "width");
            Assert.AreEqual(fd.type, "Dimension");
            Assert.AreEqual(fd.BaseType, "Dimension");
            // TODO test BaseType on something that is a complex type

            Assert.IsTrue(fp.eq_unknowndata(ut.ParseDouble(fd.EvaluatedValue, -1), 12));

            p.SetVariable(vd, 15);
            p.Execute();
            Assert.IsTrue(fp.eq_unknowndata(ut.ParseDouble(fd.EvaluatedValue, -1), 15));

            FieldDefinition fd2 = p.Steps[1].FindField("newname");
            Assert.AreEqual(fd2.EvaluatedValue, "bottom");

            FieldDefinition fd3 = p.Steps[1].FindField("why is the rum always gone?");
            Assert.IsNull(fd3);
        }

        [Test]
        public void test_is_solid()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            Solid s1 = p.Result.FindSub("back");
            Assert.IsFalse(s1.material.IsSolid());
            Solid s2 = p.Result.FindSub("top");
            Assert.IsTrue(s2.material.IsSolid());
        }

        [Test]
        public void test_rabbet()
        {
            Plan p = new Plan("table leg");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "solid.oak",
"newname", "b1",
"width", "4",
"length", "30",
"thickness", "4"
));
            p.AddStep(Action.RABBET, "rabbet", Step.CreateParms(
"path", "b1.top.right",
"id", "r1",
"inset", "1",
"depth", "1"
));
            p.AddStep(Action.RABBET, "rabbet", Step.CreateParms(
"path", "b1.top.left",
"id", "r2",
"inset", "1",
"depth", "1"
));
            p.AddStep(Action.RABBET, "rabbet", Step.CreateParms(
"path", "b1.bottom.right",
"id", "r3",
"inset", "1",
"depth", "1"
));
            p.AddStep(Action.RABBET, "rabbet", Step.CreateParms(
"path", "b1.bottom.left",
"id", "r4",
"inset", "1",
"depth", "1"
));
            p.Execute();
            CompoundSolid sol = p.Result;
            sol.DoGeomChecks();
        }

        [Test]
        public void test_roundover()
        {
            Plan p = new Plan("table leg");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "solid.oak",
"newname", "b1",
"width", "4",
"length", "30",
"thickness", "4"
));
            p.AddStep(Action.ROUNDOVER, "round", Step.CreateParms(
"path", "b1.top.right",
"id", "r1",
"radius", "1"
));
            p.AddStep(Action.ROUNDOVER, "round", Step.CreateParms(
"path", "b1.top.left",
"id", "r2",
"radius", "1"
));
            p.AddStep(Action.ROUNDOVER, "round", Step.CreateParms(
"path", "b1.bottom.right",
"id", "r3",
"radius", "1"
));
            p.AddStep(Action.ROUNDOVER, "round", Step.CreateParms(
"path", "b1.bottom.left",
"id", "r4",
"radius", "1"
));
            p.Execute();
            CompoundSolid sol = p.Result;
            sol.DoGeomChecks();
        }

        [Test]
        public void test_tenon()
        {
            Plan p = new Plan("test_drill_and_mortise");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "solid.oak",
"newname", "b1",
"width", "10",
"length", "24",
"thickness", "2"
));
            p.AddStep(Action.TENON, "cut a tenon", Step.CreateParms(
"path", "b1.end1.top",
"id", "t1",
"x", "1",
"y", "0.25",
"xsize", "8",
"ysize", "1.5",
"depth", "2"
));
            p.Execute();
            CompoundSolid sol = p.Result;
            sol.DoGeomChecks();

            test_a_plan(p);
        }

        [Test]
        public void test_plan_steps_stuff()
        {
            Plan p = new Plan("test_drill_and_mortise");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "solid.oak",
"newname", "b1",
"width", "10",
"length", "24",
"thickness", "2"
));
            p.AddStep(Action.DRILL, "Drill through that board", Step.CreateParms(
                "path", "b1.top.end1",
                "x", "5",
                "y", "5",
                "diam", "3",
                "depth", "3",
                "id", "d1"
                ));

            p.AddStep(Action.MORTISE, "Mortise the same board", Step.CreateParms(
"path", "b1.top.end1",
                "id", "m1",
"x", "5",
"y", "16",
"xsize", "2",
"ysize", "2",
"depth", "3"
));

            p.Execute();
            Step st = p.Steps[p.Steps.Count - 2];
            Assert.AreEqual(1, st.pieces.Count);

            CompoundSolid sol = p.Result;
            sol.DoGeomChecks();

            double vol = sol.Volume();

            Assert.IsTrue(fp.lt_volume(vol, 10 * 24 * 2 - 2 * 2 * 2));

            test_a_plan(p);

        }

        [Test]
        public void test_xml_file()
        {
            string f = Path.GetTempFileName();
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            p.WriteXML(f);
            Assert.IsTrue(File.Exists(f));
            FileInfo fi = new FileInfo(f);
            Assert.IsTrue(fi.Length > 0);
            File.Delete(f);
            Assert.IsFalse(File.Exists(f));
        }

        public void test_a_plan(Plan p)
        {
            StringBuilder sb = new StringBuilder();
            p.WriteXML(sb);
            string xml = sb.ToString();
            int len = xml.Length;
            Assert.Greater(len, 0);

            XmlTextReader xr = new XmlTextReader(new StringReader(xml));
            XmlDocument xd = new XmlDocument();
            xd.Load(xr);
            xr.Close();

            Plan p2 = Plan.FromXML(xd);

            Assert.AreEqual(p.Steps.Count, p2.Steps.Count);

            StringBuilder sb2 = new StringBuilder();
            p2.WriteXML(sb2);
            string xml2 = sb.ToString();

            Assert.AreEqual(xml, xml2);

            p.Execute();
            p2.Execute();

            double v1 = p.Result.Volume();
            double v2 = p2.Result.Volume();
            Assert.IsTrue(fp.eq_volume(v1, v2));
        }

        [Test]
        public void test_xml()
        {
            Plan p = test_plan.CreateBookShelf();
            p.Execute();
            test_a_plan(p);
        }

        [Test]
        public void test_bookshelf_multi()
        {
            Plan p = test_plan.CreateBookShelf();

            // weird case
            p.SetVariable("depth", 7.3);
            p.SetVariable("width", 16);
            p.SetVariable("thickness", 0.76352439482);
            p.SetVariable("height", 48);
            p.Execute();
            p.Result.DoGeomChecks();

            // more normal-looking numbers
            p.SetVariable("depth", 6);
            p.SetVariable("width", 24);
            p.SetVariable("thickness", 1.5);
            p.SetVariable("height", 30);
            p.Execute();
            p.Result.DoGeomChecks();

            p.SetVariable("depth", 12);
            p.Execute();
            p.Result.DoGeomChecks();

            p.SetVariable("thickness", 0.5);
            p.Execute();
            p.Result.DoGeomChecks();

            p.SetVariable("thickness", 3);
            p.Execute();
            p.Result.DoGeomChecks();

            p.SetVariable("width", 60);
            p.Execute();
            p.Result.DoGeomChecks();

        }

        [Test]
        public void test_inrange()
        {
            Plan p = new Plan("test");

            p.DefineVariable(new VariableDefinition("x", "", -10000, 10000, 20, 1));
            p.DefineVariable(new VariableDefinition("y", "", 5, 15, 10, 1));

            VariableDefinition vdx = p.FindVariable("x");
            Assert.IsTrue(vdx.InRange(0));
            Assert.IsTrue(vdx.InRange(-5));
            Assert.IsTrue(vdx.InRange(5));
            Assert.IsTrue(vdx.InRange(1000));

            VariableDefinition vdy = p.FindVariable("y");
            Assert.IsTrue(vdy.InRange(5));
            Assert.IsTrue(vdy.InRange(15));
            Assert.IsFalse(vdy.InRange(4));
            Assert.IsFalse(vdy.InRange(16));
        }

        [Test]
        public void test_plan_bookshelf()
        {
            // create the bookshelf plan

            Plan p = test_plan.CreateBookShelf();

            // init the variables

            p.SetVariable("depth", 6);
            p.SetVariable("width", 24);
            p.SetVariable("thickness", 1.5);
            p.SetVariable("height", 30);

            // run the steps

            p.Execute();

            // now grab the final result and do the geom checks on it

            CompoundSolid sol = p.Result;
            sol.DoGeomChecks();

            BoundingBox3d bb1 = sol.GetBoundingBox();

            sol.Translate(1000, 1000, 1000);

            BoundingBox3d bb2 = sol.GetBoundingBox();

            Assert.IsFalse(BoundingBox3d.intersect(bb1, bb2));
        }

        public static Plan CreateMortiseInPlywood()
        {
            Plan p = new Plan("Test");

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, "a", Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "8",
                "length", "16",
                "thickness", "1"
                ));

            p.AddStep(Action.MORTISE, "Mortise", Step.CreateParms(
                "path", "a.top.end1",
                "id", "m1",
                "x", "2",
                "y", "6",
                "xsize", "4",
                "ysize", "4",
                "depth", "1"
                ));

            return p;
        }
    }
}

#endif
