#define BOOKSHELF_DOVETAIL

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace sd
{
#if DEBUG
    public partial class Builtin_Plans
    {
        public static Plan CreateSpecifiedDovetail()
        {
            Plan p = new Plan("dt1");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "16",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "16",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "a.end1.top",
                "path2", "b.end1.top",
                "numtails", "3",
                "tailwidth", "3",
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dt"
                ));
            return p;
        }

        public static Plan CreateSimpleDovetail()
        {
            Plan p = new Plan("dt1");
            p.DefineVariable(new VariableDefinition("tailcount", "", 2, 10, 4, 1));
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "6",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "6",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "a.end1.top",
                "path2", "b.end1.top",
                "numtails", "tailcount",
                "tailwidth", "0",
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dt"
                ));
            return p;
        }

        public static Plan CreateDoubleDovetail()
        {
            Plan p = new Plan("dt1");
            p.DefineVariable(new VariableDefinition("tailcount", "", 2, 10, 4, 1));
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "6",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "6",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "6",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "a.end1.top",
                "path2", "b.end1.top",
                "numtails", "tailcount",
                "tailwidth", "0",
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "c.end1.top",
                "path2", "b.end2.top",
                "numtails", "tailcount",
                "tailwidth", "0",
                "id", "qt"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "qt"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "qt"
                ));
            return p;
        }

        public static Plan CreateThreeMortisesInACube()
        {
            Plan p = new Plan("cube3m");
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
            p.AddStep(Action.MORTISE, "Cut mortise 2", Step.CreateParms(
             "path", "c.right.top",
             "id", "m2",
             "x", "1",
             "y", "1",
             "xsize", "3",
             "ysize", "3",
             "depth", "6"
             ));
            p.AddStep(Action.MORTISE, "Cut mortise 2", Step.CreateParms(
             "path", "c.end1.top",
             "id", "m3",
             "x", "1",
             "y", "1",
             "xsize", "3",
             "ysize", "3",
             "depth", "6"
             ));
            return p;
        }

        public static Plan CreateTable()
        {
            Plan p = new Plan("Table");

            // TODO chamfer the legs

            // TODO variable for leg thickness.  however, all the aprons and mortises will have to be adjusted

            p.DefineVariable(new VariableDefinition("height", "This variable specifies the length of the table leg.  The actual height of the table will be this number plus the the thickness of the top.", 24, 48, 30, 4));
            p.DefineVariable(new VariableDefinition("length", "TODO help", 36, 84, 60, 4));
            p.DefineVariable(new VariableDefinition("depth", "TODO help", 16, 48, 30, 4));

            Step intro = p.AddStep(Action.INTRO, null, null);

            intro.annotations_FF.Add(new Annotation_FaceToFace("top.end1.chamfer3_2", "top.end2.chamfer4_2", 6, 2));
            intro.annotations_FF.Add(new Annotation_FaceToFace("top.left.end1", "top.right.end1", 6, 2));

            p.AddStep(Action.NEW_BOARD, "Create leg1", Step.CreateParms(
                "material", "solid.oak",
                "newname", "leg1",
                "width", "3",
                "length", "height",
                "thickness", "3"
                ));

            p.AddStep(Action.NEW_BOARD, "Create leg3", Step.CreateParms(
"material", "solid.oak",
"newname", "leg3",
"width", "3",
"length", "height",
"thickness", "3"
));
            p.AddStep(Action.NEW_BOARD, "Create leg2", Step.CreateParms(
"material", "solid.oak",
"newname", "leg2",
"width", "3",
"length", "height",
"thickness", "3"
));
            p.AddStep(Action.NEW_BOARD, "Create leg4", Step.CreateParms(
"material", "solid.oak",
"newname", "leg4",
"width", "3",
"length", "height",
"thickness", "3"
));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 1 for long apron", Step.CreateParms(
                "path", "leg1.top.right",
                "id", "m1",
                "x", "0.5",
                "y", "1",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 1 for side apron", Step.CreateParms(
                "path", "leg1.right.bottom",
                "id", "m1a",
                "x", "0.5",
                "y", "1",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 2 for long apron", Step.CreateParms(
"path", "leg2.top.right",
                "id", "m2",
"x", "0.5",
"y", "1",
"xsize", "2.5",
"ysize", "1",
"depth", "2"
));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 2 for side apron", Step.CreateParms(
"path", "leg2.left.top",
                "id", "m2a",
"x", "0.5",
"y", "1",
"xsize", "2.5",
"ysize", "1",
"depth", "2"
));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 3 for long apron", Step.CreateParms(
                "path", "leg3.top.right",
                "id", "m3",
                "x", "0.5",
                "y", "1",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));

            p.AddStep(Action.MORTISE, "Cut mortise in leg 3 for side apron", Step.CreateParms(
                "path", "leg3.right.bottom",
                "id", "m3a",
                "x", "0.5",
                "y", "1",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 4 for long apron", Step.CreateParms(
"path", "leg4.top.right",
                "id", "m4",
"x", "0.5",
"y", "1",
"xsize", "2.5",
"ysize", "1",
"depth", "2"
));
            p.AddStep(Action.MORTISE, "Cut mortise in leg 4 for side apron", Step.CreateParms(
"path", "leg4.left.top",
                "id", "m4a",
"x", "0.5",
"y", "1",
"xsize", "2.5",
"ysize", "1",
"depth", "2"
));
            p.AddStep(Action.NEW_BOARD, "Create front apron", Step.CreateParms(
                "material", "solid.oak",
                "newname", "apron_front",
                "width", "4",
                "length", "length",
                "thickness", "1.5"
                ));
            p.AddStep(Action.TENON, "Cut tenon for front apron into leg 1", Step.CreateParms(
                "path", "apron_front.end1.top",
                "id", "t1",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_front.t1_right.end1",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.TENON, "Cut tenon for front apron into leg 2", Step.CreateParms(
                "path", "apron_front.end2.bottom",
                "id", "t2",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_front.t2_left.end2",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.JOIN, "Attach apron_front into leg 1", Step.CreateParms(
                 "path1", "leg1.m1_back.top",
                 "path2", "apron_front.t1_front.t1_frontshoulder", // uses new tenon code instead of the one-cut version
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach aprong front into leg 2", Step.CreateParms(
"path1", "leg2.m2_front.top",
"path2", "apron_front.t2_back.t2_backshoulder",  // uses new tenon code instead of the one-cut version
 "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, "Create back apron", Step.CreateParms(
                "material", "solid.oak",
                "newname", "apron_back",
                "width", "4",
                "length", "length",
                "thickness", "1.5"
                ));
            p.AddStep(Action.TENON, "Tenon for back apron into leg 3", Step.CreateParms(
                "path", "apron_back.end1.top",
                "id", "t1",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_back.t1_right.end1",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.TENON, "Tenon for back apron into leg 4", Step.CreateParms(
                "path", "apron_back.end2.bottom",
                "id", "t2",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_back.t2_left.end2",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.JOIN, "Attach back apron into leg 3", Step.CreateParms(
                 "path1", "leg3.m3_back.top",
                 "path2", "apron_back.t1_front.t1_frontshoulder",  // uses new tenon code instead of the one-cut version
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach back apron into leg 4", Step.CreateParms(
"path1", "apron_back.t2_back.t2_backshoulder",  // uses new tenon code instead of the one-cut version
"path2", "leg4.m4_front.top",
 "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, "Create left apron", Step.CreateParms(
    "material", "solid.oak",
    "newname", "apron_left",
    "width", "4",
    "length", "depth",
    "thickness", "1.5"
    ));
            p.AddStep(Action.TENON, "Tenon for left apron into leg 1", Step.CreateParms(
                "path", "apron_left.end1.top",
                "id", "t1",
                "x", "1",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_left.t1_right.end1",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.TENON, "Tenon for left apron into leg 3", Step.CreateParms(
                "path", "apron_left.end2.bottom",
                "id", "t2",
                "x", "1",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));

            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_left.t2_left.end2",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create right apron", Step.CreateParms(
    "material", "solid.oak",
    "newname", "apron_right",
    "width", "4",
    "length", "depth",
    "thickness", "1.5"
    ));
            p.AddStep(Action.TENON, "Tenon for right apron into leg 1", Step.CreateParms(
                "path", "apron_right.end1.top",
                "id", "t1",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));
            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_right.t1_right.end1",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.TENON, "Tenon for right apron into leg 3", Step.CreateParms(
                "path", "apron_right.end2.bottom",
                "id", "t2",
                "x", "0.5",
                "y", "0.25",
                "xsize", "2.5",
                "ysize", "1",
                "depth", "2"
                ));

            p.AddStep(Action.CROSSCUT, "Miter the tenon", Step.CreateParms(
                "path", "apron_right.t2_left.end2",
                "dist", "0",
                "miter", "45",
                "tilt", "0"
                ));
            p.AddStep(Action.JOIN, "Attach left apron to leg 1", Step.CreateParms(
"path1", "leg1.m1a_front.right",
"path2", "apron_left.t1_front.t1_frontshoulder",  // uses new tenon code instead of the one-cut version
"align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach right apron to leg 2", Step.CreateParms(
"path1", "leg2.m2a_back.left",
"path2", "apron_right.t1_front.t1_frontshoulder",  // uses new tenon code instead of the one-cut version
"align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach the two assemblies together", Step.CreateParms(
"path2", "apron_left.t2_back.t2_backshoulder",  // uses new tenon code instead of the one-cut version
"path1", "leg4.m4a_back.left",
"align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, "Create top", Step.CreateParms(
"material", "solid.oak",
"newname", "top",
"width", "depth+4",
"length", "length+4",
"thickness", "1.25"
));

            p.AddStep(Action.JOIN, "Attach the top", Step.CreateParms(
"path1", "apron_left.left.top",
"path2", "top.bottom.end1",
"align", "center",
"offset1", "0",
"offset2", "1.5" // TODO I don't think this offset is quite right yet
));

            p.AddStep(Action.CHAMFER, "Route the top front", Step.CreateParms(
"path", "top.top.left",
"id", "chamfer1",
"inset", "0.4"
));
            p.AddStep(Action.CHAMFER, "Route the top front", Step.CreateParms(
"path", "top.top.right",
"id", "chamfer2",
"inset", "0.4"
));
            p.AddStep(Action.CHAMFER, "Route the top front", Step.CreateParms(
"path", "top.top.end1",
"id", "chamfer3",
"inset", "0.4"
));
            p.AddStep(Action.CHAMFER, "Route the top front", Step.CreateParms(
"path", "top.top.end2",
"id", "chamfer4",
"inset", "0.4"
));

            return p;
        }

        public static Plan CreateFamilyRoomShelf()
        {
            // TODO guid?
            Plan p = new Plan("Family Room Shelf");

            p.DefineVariable(new VariableDefinition("plythick", "TODO help", 0.25, 2, 0.75, 4));
            p.DefineVariable(new VariableDefinition("depth", "TODO help", 6, 24, 12, 2));
            p.DefineVariable(new VariableDefinition("height", "TODO help", 36, 100, 69, 1));
            p.DefineVariable(new VariableDefinition("width", "TODO help", 36, 200, 135, 1));
            p.DefineVariable(new VariableDefinition("tv_space_height", "TODO help", 10, 200, 39, 1));
            p.DefineVariable(new VariableDefinition("tv_space_width", "TODO help", 10, 200, 53, 1));
            p.DefineVariable(new VariableDefinition("shelf_above_tv_height", "TODO help", 10, 200, 15.5, 4));
            p.DefineVariable(new VariableDefinition("rshelf1_height", "TODO help", 6, 200, 12, 4));
            p.DefineVariable(new VariableDefinition("rshelf2_height", "TODO help", 6, 200, 12, 4));
            p.DefineVariable(new VariableDefinition("rshelf3_height", "TODO help", 6, 200, 16, 4));
            p.DefineVariable(new VariableDefinition("rshelf4_height", "TODO help", 6, 200, 10, 4));

            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "far_left_vertical",
                "width", "depth",
                "length", "height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "left_back",
"width", "tv_space_height + plythick*2",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
"path1", "far_left_vertical.bottom.left",
"path2", "left_back.end1.bottom",
 "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "left_shelf_1",
"width", "depth - plythick",
"length", "tv_space_height",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
"path1", "far_left_vertical.bottom.end1",
"path2", "left_shelf_1.bottom.end1",
 "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "above_tv_1",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "above_tv_2",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Laminate that shelf", Step.CreateParms(
"path1", "above_tv_1.top.left",
"path2", "above_tv_2.bottom.left",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.JOIN, "Install the shelf", Step.CreateParms(
"path1", "left_shelf_1.end2.bottom",
"path2", "above_tv_2.top.end2",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_2",
"width", "depth - plythick",
"length", "tv_space_height",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Put in the other support", Step.CreateParms(
"path1", "above_tv_2.top.end1",
"path2", "shelf_support_2.end1.bottom",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "left_back_top",
"width", "height - (tv_space_height + plythick*2)",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Top back attach", Step.CreateParms(
"path1", "left_back.left.bottom",
"path2", "left_back_top.left.bottom",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "right_back_low",
"width", "rshelf1_height + rshelf2_height + rshelf3_height + plythick*4 + plythick",
"length", "width - tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Attach lower right back", Step.CreateParms(
"path1", "left_back.end2.right",
"path2", "right_back_low.end1.left",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "right_back_hi",
"width", "height - (rshelf1_height + rshelf2_height + rshelf3_height + plythick*4 + plythick)",
"length", "width - tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Top back attach", Step.CreateParms(
"path1", "right_back_low.right.bottom",
"path2", "right_back_hi.left.bottom",
       "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "far_right_vertical",
"width", "depth",
"length", "height",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Attach right vertical", Step.CreateParms(
   "path1", "right_back_hi.end2.top",
 "path2", "far_right_vertical.top.left",
   "align", "right",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "above_tv_3",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Laminate the shelf again", Step.CreateParms(
   "path1", "above_tv_1.bottom.end1",
 "path2", "above_tv_3.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_3",
"width", "depth - plythick",
"length", "shelf_above_tv_height",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_4",
"width", "depth - plythick",
"length", "shelf_above_tv_height",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "above_tv_3.top.left",
 "path2", "shelf_support_3.end1.left",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "above_tv_3.top.left",
 "path2", "shelf_support_4.end1.left",
   "align", "right",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_4",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Install shelf_4", Step.CreateParms(
   "path1", "shelf_support_4.end2.top",
 "path2", "shelf_4.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_5",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Install shelf_5", Step.CreateParms(
   "path1", "shelf_4.top.left",
 "path2", "shelf_5.bottom.left",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_5",
"width", "depth - plythick",
"length", "height - tv_space_height - 6*plythick - shelf_above_tv_height",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_6",
"width", "depth - plythick",
"length", "height - tv_space_height - 6*plythick - shelf_above_tv_height",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "shelf_5.top.left",
 "path2", "shelf_support_5.end1.left",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "shelf_5.top.left",
 "path2", "shelf_support_6.end1.left",
   "align", "right",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_6",
"width", "depth - plythick",
"length", "tv_space_width",
"thickness", "plythick"
));

            p.AddStep(Action.JOIN, "Install shelf_6", Step.CreateParms(
   "path1", "shelf_support_6.end2.top",
 "path2", "shelf_6.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_7",
"width", "depth - plythick",
"length", "rshelf1_height",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_8",
"width", "depth - plythick",
"length", "rshelf1_height",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "shelf_support_2.bottom.left",
 "path2", "shelf_support_7.top.left",
   "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "far_right_vertical.top.right",
 "path2", "shelf_support_8.top.left",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "rshelf_1",
"width", "depth - plythick",
"length", "width - tv_space_width",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install rshelf_1", Step.CreateParms(
   "path1", "shelf_support_8.end2.top",
 "path2", "rshelf_1.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_9",
"width", "depth - plythick",
"length", "rshelf2_height",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_10",
"width", "depth - plythick",
"length", "rshelf2_height",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_1.top.end2",
 "path2", "shelf_support_9.end1.top",
   "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_1.top.end1",
 "path2", "shelf_support_10.end1.top",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "rshelf_2",
"width", "depth - plythick",
"length", "width - tv_space_width",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install rshelf_2", Step.CreateParms(
   "path1", "shelf_support_10.end2.top",
 "path2", "rshelf_2.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_11",
"width", "depth - plythick",
"length", "rshelf3_height",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_12",
"width", "depth - plythick",
"length", "rshelf3_height",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_2.top.end2",
 "path2", "shelf_support_11.end1.top",
   "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_2.top.end1",
 "path2", "shelf_support_12.end1.top",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "rshelf_3",
"width", "depth - plythick",
"length", "width - tv_space_width",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install rshelf_3", Step.CreateParms(
   "path1", "shelf_support_12.end2.top",
 "path2", "rshelf_3.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_13",
"width", "depth - plythick",
"length", "10",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_14",
"width", "depth - plythick",
"length", "10",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_3.top.end2",
 "path2", "shelf_support_13.end1.top",
   "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_3.top.end1",
 "path2", "shelf_support_14.end1.top",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "rshelf_4",
"width", "depth - plythick",
"length", "width - tv_space_width",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install rshelf_4", Step.CreateParms(
   "path1", "shelf_support_14.end2.top",
 "path2", "rshelf_4.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_15",
"width", "depth - plythick",
"length", "height - (rshelf1_height + rshelf2_height + rshelf3_height + rshelf4_height + plythick*2*4) - plythick",
"thickness", "plythick"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "shelf_support_16",
"width", "depth - plythick",
"length", "height - (rshelf1_height + rshelf2_height + rshelf3_height + rshelf4_height + plythick*2*4) - plythick",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_4.top.end2",
 "path2", "shelf_support_15.end1.top",
   "align", "right",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.JOIN, "Attach it", Step.CreateParms(
   "path1", "rshelf_4.top.end1",
 "path2", "shelf_support_16.end1.top",
   "align", "left",
"offset1", "0", "offset2", "0"
));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "rshelf_5",
"width", "depth - plythick",
"length", "width - tv_space_width",
"thickness", "plythick"
));
            p.AddStep(Action.JOIN, "Install rshelf_5", Step.CreateParms(
   "path1", "shelf_support_16.end2.top",
 "path2", "rshelf_5.bottom.end1",
   "align", "left",
"offset1", "0", "offset2", "0"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert1_1",
"width", "depth - plythick",
"length", "rshelf1_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert1_1", Step.CreateParms(
   "path1", "rshelf_1.bottom.left",
 "path2", "vert1_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert1_2",
"width", "depth - plythick",
"length", "rshelf1_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert1_2", Step.CreateParms(
   "path1", "rshelf_1.bottom.left",
 "path2", "vert1_2.end1.left",
   "align", "right",
"offset2", "0", "offset1", "2*(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert2_1",
"width", "depth - plythick",
"length", "rshelf2_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert2_1", Step.CreateParms(
   "path1", "rshelf_2.bottom.left",
 "path2", "vert2_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert2_2",
"width", "depth - plythick",
"length", "rshelf2_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert2_2", Step.CreateParms(
   "path1", "rshelf_2.bottom.left",
 "path2", "vert2_2.end1.left",
   "align", "right",
"offset2", "0", "offset1", "2*(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert2_3",
"width", "depth - plythick",
"length", "rshelf2_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert2_3", Step.CreateParms(
   "path1", "rshelf_2.bottom.left",
 "path2", "vert2_3.end1.left",
   "align", "right",
"offset2", "0", "offset1", "3*(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert3_1",
"width", "depth - plythick",
"length", "rshelf3_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert3_1", Step.CreateParms(
   "path1", "rshelf_3.bottom.left",
 "path2", "vert3_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert3_2",
"width", "depth - plythick",
"length", "rshelf3_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert3_2", Step.CreateParms(
   "path1", "rshelf_3.bottom.left",
 "path2", "vert3_2.end1.left",
   "align", "right",
"offset2", "0", "offset1", "2*(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert4_1",
"width", "depth - plythick",
"length", "rshelf4_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert4_1", Step.CreateParms(
   "path1", "rshelf_4.bottom.left",
 "path2", "vert4_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert4_2",
"width", "depth - plythick",
"length", "rshelf4_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert4_2", Step.CreateParms(
   "path1", "rshelf_4.bottom.left",
 "path2", "vert4_2.end1.left",
   "align", "right",
"offset2", "0", "offset1", "2*(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert4_3",
"width", "depth - plythick",
"length", "rshelf4_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert4_3", Step.CreateParms(
   "path1", "rshelf_4.bottom.left",
 "path2", "vert4_3.end1.left",
   "align", "right",
"offset2", "0", "offset1", "3*(width-tv_space_width-2*plythick - 3*(2*plythick))/4"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert5_1",
"width", "depth - plythick",
"length", "height - (rshelf1_height + rshelf2_height + rshelf3_height + rshelf4_height + plythick*2*4) - plythick",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert5_1", Step.CreateParms(
   "path1", "rshelf_5.bottom.left",
 "path2", "vert5_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert5_2",
"width", "depth - plythick",
"length", "height - (rshelf1_height + rshelf2_height + rshelf3_height + rshelf4_height + plythick*2*4) - plythick",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert5_2", Step.CreateParms(
   "path1", "rshelf_5.bottom.left",
 "path2", "vert5_2.end1.left",
   "align", "right",
"offset2", "0", "offset1", "2*(width-tv_space_width-2*plythick - 2*(2*plythick))/3"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
"newname", "vert_tv_1",
"width", "depth - plythick",
"length", "shelf_above_tv_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert_tv_1", Step.CreateParms(
   "path1", "shelf_4.bottom.left",
 "path2", "vert_tv_1.end1.left",
   "align", "right",
"offset2", "0", "offset1", "9"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert_tv_2",
"width", "depth - plythick",
"length", "shelf_above_tv_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert_tv_2", Step.CreateParms(
   "path1", "shelf_4.bottom.left",
 "path2", "vert_tv_2.end1.left",
   "align", "left",
"offset2", "0", "offset1", "9"
));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
"material", "plywood.oak",
"newname", "vert_top_left",
"width", "depth - plythick",
"length", "height - tv_space_height - 6*plythick - shelf_above_tv_height",
"thickness", "plythick*2"
));
            p.AddStep(Action.JOIN, "Install vert_top_left", Step.CreateParms(
   "path1", "shelf_6.bottom.left",
 "path2", "vert_top_left.end1.left",
   "align", "center",
"offset1", "0", "offset2", "0"
));

            return p;
        }

        public static Plan CreateTaperedBoard()
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
                "path", "a.top.left",
                "dist", "2",
                "taper", "2",
                "tilt", "0"
                ));

            return p;

        }

        public static Plan CreateMiteredBoard()
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
                "path", "a.top.end1",
                "dist", "4",
                "miter", "-30",
                "tilt", "-45"
                ));

            return p;

        }

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

        public static Plan CreateTestBlocks()
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

            p.AddStep(Action.DADO, "cut dado in a.top", Step.CreateParms(
                "path", "a.top.end1",
                "id", "d1",
                "dist", "2",
                "width", "1",
                "depth", "1"
                ));

            p.AddStep(Action.DRILL, "drill hole in a.left", Step.CreateParms(
                "path", "a.left.bottom",
                "x", "1",
                "y", "1",
                "diam", "0.5",
                "depth", "6",
                "id", "d1"
                ));

            p.AddStep(Action.CHAMFER, "chamfer edge between a.end1 and a.left", Step.CreateParms(
                "path", "a.end1.left",
                "id", "ch1",
                "inset", "0.5"
                ));

            p.AddStep(Action.ROUNDOVER, "round edge between a.end1 and a.right", Step.CreateParms(
                "path", "a.end1.right",
                "id", "r1",
                "radius", "0.5"
                ));

            p.AddStep(Action.RABBET, "Rabbet edge between a.end2 and a.right", Step.CreateParms(
                "path", "a.end2.right",
                "id", "rab1",
                "inset", "0.5",
                "depth", "0.5"
                ));

            p.AddStep(Action.MORTISE, "Mortise a.end2", Step.CreateParms(
                "path", "a.end2.left",
                "id", "m1",
                "x", "2",
                "y", "2",
                "xsize", "1",
                "ysize", "1",
                "depth", "2"
                ));

            p.AddStep(Action.NEW_BOARD, "b", Step.CreateParms(
                "material", "solid.oak",
                "newname", "b",
                "width", "5",
                "length", "5",
                "thickness", "5"
                ));

            p.AddStep(Action.TENON, "Tenon b.top", Step.CreateParms(
                "path", "b.top.end1",
                "id", "t1",
                "x", "2",
                "y", "2",
                "xsize", "1",
                "ysize", "1",
                "depth", "2"
                ));

            p.AddStep(Action.JOIN, "Join a and b", Step.CreateParms(
                "path1", "a.end2.bottom",
                "path2", "b.end2.right",
                "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "c",
                "width", "16",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "v",
                "width", "16",
                "length", "24",
                "thickness", "1"
                ));
            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "c.end1.top",
                "path2", "v.end1.top",
                "numtails", "3",
                "tailwidth", "3",
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dt"
                ));
            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dt"
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


        public static Plan CreateOneBoard()
        {
            Plan p = new Plan("dt1");
            p.AddStep(Action.INTRO, null, null);

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "solid.oak",
                "newname", "a",
                "width", "8",
                "length", "20",
                "thickness", "2"
                ));
            return p;
        }
    }
#endif
}
