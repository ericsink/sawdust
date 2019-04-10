
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
        public static Plan CreateNewFamilyRoomShelf()
        {
            // TODO guid?
            Plan p = new Plan("Family Room Shelf");

            p.DefineVariable(new VariableDefinition("plythick", "0.75"));
            p.DefineVariable(new VariableDefinition("height", "69.25"));
            p.DefineVariable(new VariableDefinition("width", "136"));
            p.DefineVariable(new VariableDefinition("depth", "usable depth of the shelves", 6, 18, 13, 2));
            p.DefineVariable(new VariableDefinition("tv_space_height", "TODO help", 10, 200, 39.75, 1));
            p.DefineVariable(new VariableDefinition("tv_space_width", "TODO help", 10, 200, 56, 1));
            p.DefineVariable(new VariableDefinition("shelf1_height", "TODO help", 8, 20, 16, 4));
            p.DefineVariable(new VariableDefinition("shelf1_sides_width", "Width of the sides of the shelf just above the TV", 6, 16, 10, 1));
            p.DefineVariable(new VariableDefinition("shelf3_height", "TODO help", 6, 20, 10, 4));
            p.DefineVariable(new VariableDefinition("shelf4_height", "TODO help", 6, 20, 8.25, 4));
            p.DefineVariable(new VariableDefinition("shelf5_height", "TODO help", 6, 20, 14, 4));
            p.DefineVariable(new VariableDefinition("shelf6_height", "TODO help", 6, 20, 10, 4));
            p.DefineVariable(new VariableDefinition("shelf7_height", "TODO help", 6, 20, 9.75, 4));
            p.DefineVariable(new VariableDefinition("shelf8_height", "height - shelf3_height - shelf4_height - shelf5_height - shelf6_height - shelf7_height - 11 * plythick"));

            p.DefineVariable(new VariableDefinition("shelf2_height", "height - tv_space_height - shelf1_height - 6*plythick"));
            p.DefineVariable(new VariableDefinition("right_width", "width - tv_space_width - plythick*5"));

            Step intro = p.AddStep(Action.INTRO, null, null);

#if not
            intro.annotations_FF.Add(new Annotation_FaceToFace("tv_left.top.right", "tv_right.top.right", 6, 2));
            intro.annotations_FF.Add(new Annotation_FaceToFace("shelf2_a.bottom.right", "shelf1_c.top.right", 6, 2));
#endif

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "far_left_vertical",
                "width", "depth + plythick",
                "length", "height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "base_tv",
                "width", "tv_space_height",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "tv_left",
                "width", "depth",
                "length", "tv_space_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "tv_right",
                "width", "depth",
                "length", "tv_space_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "tv_left to tv_base", Step.CreateParms(
                "path2", "tv_left.left.bottom",
                "path1", "base_tv.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "tv_right to tv_base", Step.CreateParms(
                "path2", "tv_right.left.bottom",
                "path1", "base_tv.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "far_left_vertical.bottom.left",
                "path2", "base_tv.end1.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_a",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install bottom of shelf1 over tv", Step.CreateParms(
                "path1", "base_tv.left.bottom",
                "path2", "shelf1_a.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_b",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install middle piece of shelf over tv", Step.CreateParms(
                "path1", "shelf1_a.top.right",
                "path2", "shelf1_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_base",
                "width", "shelf1_height",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_left",
                "width", "depth",
                "length", "shelf1_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_right",
                "width", "depth",
                "length", "shelf1_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_c",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install middle piece of shelf over tv", Step.CreateParms(
                "path1", "shelf1_b.top.right",
                "path2", "shelf1_c.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "left to tv_base", Step.CreateParms(
                "path2", "shelf1_left.left.bottom",
                "path1", "shelf1_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to tv_base", Step.CreateParms(
                "path2", "shelf1_right.left.bottom",
                "path1", "shelf1_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf1_c.top.left",
                "path2", "shelf1_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_vert1",
                "width", "depth",
                "length", "shelf1_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf1_vert2",
                "width", "depth",
                "length", "shelf1_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf1_vert1", Step.CreateParms(
                "path1", "shelf1_c.top.right",
                "path2", "shelf1_vert1.end1.right",
                 "align", "right",
                "offset1", "shelf1_sides_width", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf1_vert2", Step.CreateParms(
                "path1", "shelf1_c.top.right",
                "path2", "shelf1_vert2.end1.right",
                 "align", "left",
                "offset1", "shelf1_sides_width", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_a",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install bottom half of shelf2", Step.CreateParms(
                "path1", "shelf1_base.left.bottom",
                "path2", "shelf2_a.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_b",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install other half of shelf2", Step.CreateParms(
                "path1", "shelf2_a.top.right",
                "path2", "shelf2_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_base",
                "width", "shelf2_height",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_left",
                "width", "depth",
                "length", "shelf2_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_right",
                "width", "depth",
                "length", "shelf2_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf2_left.left.bottom",
                "path1", "shelf2_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf2_right.left.bottom",
                "path1", "shelf2_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf2_b.top.left",
                "path2", "shelf2_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf2_vert",
                "width", "depth",
                "length", "shelf2_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf2_vert", Step.CreateParms(
                "path1", "shelf2_b.top.right",
                "path2", "shelf2_vert.end1.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelftopleft",
                "width", "depth + plythick",
                "length", "tv_space_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install shelftopleft", Step.CreateParms(
                "path1", "shelf2_base.left.bottom",
                "path2", "shelftopleft.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "middle_vertical",
                "width", "depth + plythick",
                "length", "height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install middle_vertical", Step.CreateParms(
                "path1", "tv_right.bottom.end1",
                "path2", "middle_vertical.bottom.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf3_left",
                "width", "depth",
                "length", "shelf3_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf3_right",
                "width", "depth",
                "length", "shelf3_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf3_base",
                "width", "shelf3_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf3_left.left.bottom",
                "path1", "shelf3_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf3_right.left.bottom",
                "path1", "shelf3_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf3_left", Step.CreateParms(
                "path1", "middle_vertical.top.end1",
                "path2", "shelf3_left.bottom.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_a",
                "width", "depth + plythick",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "far_right_vertical",
                "width", "depth + plythick",
                "length", "height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install shelf3_right", Step.CreateParms(
                "path1", "far_right_vertical.top.end1",
                "path2", "shelf3_right.bottom.end1",
                 "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install bottom of shelf1 over tv", Step.CreateParms(
                "path1", "shelf3_base.left.bottom",
                "path2", "shelf4_a.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf3_vert1",
                "width", "depth",
                "length", "shelf3_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf3_vert2",
                "width", "depth",
                "length", "shelf3_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf3_vert1", Step.CreateParms(
                "path1", "shelf4_a.bottom.right",
                "path2", "shelf3_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf3_vert2", Step.CreateParms(
                "path1", "shelf4_a.bottom.right",
                "path2", "shelf3_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_b",
                "width", "depth + plythick",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "Install other half of shelf4", Step.CreateParms(
                "path1", "shelf4_a.top.right",
                "path2", "shelf4_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_left",
                "width", "depth",
                "length", "shelf4_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_right",
                "width", "depth",
                "length", "shelf4_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_base",
                "width", "shelf4_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf4_left.left.bottom",
                "path1", "shelf4_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf4_right.left.bottom",
                "path1", "shelf4_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf4_b.top.left",
                "path2", "shelf4_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_vert1",
                "width", "depth",
                "length", "shelf4_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_vert2",
                "width", "depth",
                "length", "shelf4_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf4_vert3",
                "width", "depth",
                "length", "shelf4_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert1", Step.CreateParms(
                "path1", "shelf4_b.top.right",
                "path2", "shelf4_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert2", Step.CreateParms(
                "path1", "shelf4_b.top.right",
                "path2", "shelf4_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert3", Step.CreateParms(
                "path1", "shelf4_b.top.right",
                "path2", "shelf4_vert3.end1.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));


            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelf5_a",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install bottom of shelf5", Step.CreateParms(
                "path1", "shelf4_base.left.bottom",
                "path2", "shelf5_a.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelf5_b",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install other half of shelf5", Step.CreateParms(
                "path1", "shelf5_a.top.right",
                "path2", "shelf5_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));


            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf5_left",
                "width", "depth",
                "length", "shelf5_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf5_right",
                "width", "depth",
                "length", "shelf5_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf5_base",
                "width", "shelf5_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf5_left.left.bottom",
                "path1", "shelf5_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf5_right.left.bottom",
                "path1", "shelf5_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf5_b.top.left",
                "path2", "shelf5_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf5_vert1",
                "width", "depth",
                "length", "shelf5_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf5_vert2",
                "width", "depth",
                "length", "shelf5_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert1", Step.CreateParms(
                "path1", "shelf5_b.top.right",
                "path2", "shelf5_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert2", Step.CreateParms(
                "path1", "shelf5_b.top.right",
                "path2", "shelf5_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelf6_a",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install bottom of shelf1 over tv", Step.CreateParms(
               "path1", "shelf5_base.left.bottom",
               "path2", "shelf6_a.bottom.left",
                "align", "left",
               "offset1", "0", "offset2", "0"
               ));



            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
    "material", "plywood.oak",
    "newname", "shelf6_b",
    "width", "depth + plythick",
    "length", "right_width",
    "thickness", "plythick"
    ));

            p.AddStep(Action.JOIN, "Install other half of shelf6", Step.CreateParms(
                "path1", "shelf6_a.top.right",
                "path2", "shelf6_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_left",
                "width", "depth",
                "length", "shelf6_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_right",
                "width", "depth",
                "length", "shelf6_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_base",
                "width", "shelf6_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf6_left.left.bottom",
                "path1", "shelf6_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf6_right.left.bottom",
                "path1", "shelf6_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf6_b.top.left",
                "path2", "shelf6_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_vert1",
                "width", "depth",
                "length", "shelf6_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_vert2",
                "width", "depth",
                "length", "shelf6_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf6_vert3",
                "width", "depth",
                "length", "shelf6_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf6_vert1", Step.CreateParms(
                "path1", "shelf6_b.top.right",
                "path2", "shelf6_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf6_vert2", Step.CreateParms(
                "path1", "shelf6_b.top.right",
                "path2", "shelf6_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf6_vert3", Step.CreateParms(
                "path1", "shelf6_b.top.right",
                "path2", "shelf6_vert3.end1.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
              "material", "plywood.oak",
              "newname", "shelf7_a",
              "width", "depth + plythick",
              "length", "right_width",
              "thickness", "plythick"
              ));

            p.AddStep(Action.JOIN, "Install bottom of shelf7", Step.CreateParms(
                "path1", "shelf6_base.left.bottom",
                "path2", "shelf7_a.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelf7_b",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install other half of shelf7", Step.CreateParms(
                "path1", "shelf7_a.top.right",
                "path2", "shelf7_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));


            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf7_left",
                "width", "depth",
                "length", "shelf7_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf7_right",
                "width", "depth",
                "length", "shelf7_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf7_base",
                "width", "shelf7_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf7_left.left.bottom",
                "path1", "shelf7_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf7_right.left.bottom",
                "path1", "shelf7_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf7_b.top.left",
                "path2", "shelf7_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf7_vert1",
                "width", "depth",
                "length", "shelf7_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf7_vert2",
                "width", "depth",
                "length", "shelf7_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert1", Step.CreateParms(
                "path1", "shelf7_b.top.right",
                "path2", "shelf7_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf4_vert2", Step.CreateParms(
                "path1", "shelf7_b.top.right",
                "path2", "shelf7_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/3", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelf8_a",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install bottom of shelf1 over tv", Step.CreateParms(
               "path1", "shelf7_base.left.bottom",
               "path2", "shelf8_a.bottom.left",
                "align", "left",
               "offset1", "0", "offset2", "0"
               ));



            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
    "material", "plywood.oak",
    "newname", "shelf8_b",
    "width", "depth + plythick",
    "length", "right_width",
    "thickness", "plythick"
    ));

            p.AddStep(Action.JOIN, "Install other half of shelf8", Step.CreateParms(
                "path1", "shelf8_a.top.right",
                "path2", "shelf8_b.bottom.right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_left",
                "width", "depth",
                "length", "shelf8_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_right",
                "width", "depth",
                "length", "shelf8_height",
                "thickness", "plythick"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_base",
                "width", "shelf8_height",
                "length", "right_width",
                "thickness", "plythick"
                ));

            p.AddStep(Action.JOIN, "left to base", Step.CreateParms(
                "path2", "shelf8_left.left.bottom",
                "path1", "shelf8_base.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "right to base", Step.CreateParms(
                "path2", "shelf8_right.left.bottom",
                "path1", "shelf8_base.top.end2",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Back to left side", Step.CreateParms(
                "path1", "shelf8_b.top.left",
                "path2", "shelf8_base.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_vert1",
                "width", "depth",
                "length", "shelf8_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_vert2",
                "width", "depth",
                "length", "shelf8_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf8_vert3",
                "width", "depth",
                "length", "shelf8_height",
                "thickness", "plythick*2"
                ));

            p.AddStep(Action.JOIN, "Install shelf8_vert1", Step.CreateParms(
                "path1", "shelf8_b.top.right",
                "path2", "shelf8_vert1.end1.right",
                 "align", "right",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf8_vert2", Step.CreateParms(
                "path1", "shelf8_b.top.right",
                "path2", "shelf8_vert2.end1.right",
                 "align", "left",
                "offset1", "right_width/4", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Install shelf8_vert3", Step.CreateParms(
                "path1", "shelf8_b.top.right",
                "path2", "shelf8_vert3.end1.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, null, Step.CreateParms(
               "material", "plywood.oak",
               "newname", "shelftopright",
               "width", "depth + plythick",
               "length", "right_width",
               "thickness", "plythick"
               ));

            p.AddStep(Action.JOIN, "Install shelftopleft", Step.CreateParms(
                "path1", "shelf8_base.left.bottom",
                "path2", "shelftopright.bottom.left",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            return p;
        }

    }
#endif
}