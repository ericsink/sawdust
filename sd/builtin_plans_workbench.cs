
#undef WORKBENCH_DOG_HOLES_ROUND

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;

// TODO http://www.woodworking.org/WC/GArchive99/12_11bobhambench.html
//      long text description of a bench project

// TODO http://woodworkstuff.net/ETSVS.html
//      tips for veritas twin screw vise

// TODO dog holes in the sides for the front vise?

// TODO dog holes on the top for the front vise?

// TODO in the top, arrange the grain one way for hand planing

// TODO Rolf Safferthal bench
//      http://www.sawmillcreek.org/showthread.php?t=48755&highlight=workbench

// TODO Sam Maloof bench removable tray

// TODO BLO for the finish?

// TODO Chris Del bench
//      http://www.sawmillcreek.org/showpost.php?p=393311&postcount=14

// TODO Tony Sade bench
//      http://www.sawmillcreek.org/showthread.php?t=27812


namespace sd
{
    public partial class Builtin_Plans
    {
        public static Plan CreateWorkbench()
        {
            Plan p = new Plan("Workbench");

            p.document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph xml:space=\"preserve\">This workbench plan features a removable center tool tray, two vises, four rows of dog holes, and some dovetail joinery, mainly for aesthetics.  The supporting structure is red oak and the top is hard maple.</Paragraph><Paragraph>Several variables allow you to customize the dimensions of the bench to your liking.</Paragraph><Paragraph>For maximum flexibility, I have included an apron on the front but no apron on the back.</Paragraph></FlowDocument>";

            p.DefineVariable(new VariableDefinition("Height", "The total height of the bench, from the floor to the top.", 30, 40, 35, 4));
            p.DefineVariable(new VariableDefinition("Top_Slab_Length", "The length of the two top slabs.  The total length of the bench will be slightly longer because of the addition of the end caps and vise.", 60, 96, 78, 4));
            p.DefineVariable(new VariableDefinition("Top_Slab_Width", "The width of each of the two top slabs.", 12, 18, 14, 1));
            p.DefineVariable(new VariableDefinition("Tray_Width", "The width of the center tray between the two slabs of the top", 4, 12, 8, 1));
            p.DefineVariable(new VariableDefinition("Top_Thickness", "The thickness of the two slabs of the top", 1.5, 5, 4, 2));

            p.DefineVariable(new VariableDefinition("Front_Overhang", "The overhang on the front (and back) of the bench.", 0, 12, 4, 1));
            p.DefineVariable(new VariableDefinition("Right_Overhang", "The overhang on the right end of the bench.  This probably needs to be much larger than the left overhang to make room for the vise hardware underneath.", 0, 18, 16, 1));
            p.DefineVariable(new VariableDefinition("Left_Overhang", "The overhang on the left end of the bench.", 0, 18, 4, 1));

            p.DefineVariable(new VariableDefinition("Leg_Thickness", "Thickness of the legs, feet and bearers", 3, 5, 4, 4));

#if false
            p.DefineVariable(new VariableDefinition("Dog_Hole_Spacing", "Distance between dog holes, center to center", 4, 20, 6, 1));
            p.DefineVariable(new VariableDefinition("First_Dog_Hole", "Distance from the end of the top slab to the center of the first dog hole in the row", 2, 12, 3, 1));
            p.DefineVariable(new VariableDefinition("Dog_Holes_Inset", "Distance from the side of the slab to the row of dog holes", 2, 6, 2, 1));
#else
            p.DefineVariable(new VariableDefinition("Dog_Hole_Spacing", "6"));
            p.DefineVariable(new VariableDefinition("First_Dog_Hole", "3"));
            p.DefineVariable(new VariableDefinition("Dog_Holes_Inset", "2"));
#endif

            p.DefineVariable(new VariableDefinition("frontviselength", "21"));
            p.DefineVariable(new VariableDefinition("stretcher_width", "4"));
            p.DefineVariable(new VariableDefinition("traythickness", "0.5"));
            p.DefineVariable(new VariableDefinition("pad_thickness", "1"));
            p.DefineVariable(new VariableDefinition("stretcher_thickness", "1"));
            p.DefineVariable(new VariableDefinition("leglength", "Height - Top_Thickness - Leg_Thickness - pad_thickness - Leg_Thickness + Leg_Thickness/2 + Leg_Thickness/2"));
            p.DefineVariable(new VariableDefinition("footlength", "Top_Slab_Width*2+Tray_Width - Front_Overhang*2"));

            Step intro = p.AddStep(Action.INTRO, null, null);

            intro.annotations_FF.Add(new Annotation_FaceToFace("frontapron.top.right", "backtop.left.top", 6, 2));
            intro.annotations_FF.Add(new Annotation_FaceToFace("backtop.top.left", "foot1_pad2.bottom.end1", 6, 2));
            intro.annotations_FF.Add(new Annotation_FaceToFace("endvise.top.right", "leftendcap.top.right", 12, 2));

            p.AddStep(Action.NEW_BOARD, "Create foot 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot1",
                "width", "Leg_Thickness",
                "length", "footlength",
                "thickness", "Leg_Thickness"
                ));

            p.AddStep(Action.MORTISE, "Cut foot 1 mortise 1", Step.CreateParms(
                "path", "foot1.top.end1",
                "id", "foot1_mortise1",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut foot 1 mortise 1", Step.CreateParms(
                "path", "foot1.top.end2",
                "id", "foot1_mortise2",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer one end of foot 1", Step.CreateParms(
                "path", "foot1.top.end1",
                "id", "foot1_chamfer1",
                "inset", "Leg_Thickness/2"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph xml:space=\"preserve\">This chamfer may be too big for a router.  If so, try a compound miter saw.</Paragraph></FlowDocument>";

            p.AddStep(Action.CHAMFER, "Chamfer the other end of foot 1", Step.CreateParms(
                "path", "foot1.top.end2",
                "id", "foot1_chamfer2",
                "inset", "Leg_Thickness/2"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>(See my comments on the previous step)</Paragraph></FlowDocument>";

            p.AddStep(Action.NEW_BOARD, "Create foot 1 pad 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot1_pad1",
                "width", "Leg_Thickness",
                "length", "Leg_Thickness*2",
                "thickness", "pad_thickness"
                ));

            p.AddStep(Action.NEW_BOARD, "Create foot 1 pad 2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot1_pad2",
                "width", "Leg_Thickness",
                "length", "Leg_Thickness*2",
                "thickness", "pad_thickness"
                ));

            p.AddStep(Action.JOIN, "Attach foot 1 pad 1", Step.CreateParms(
                 "path1", "foot1.bottom.end1",
                 "path2", "foot1_pad1.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach foot 1 pad 2", Step.CreateParms(
                 "path1", "foot1.bottom.end2",
                 "path2", "foot1_pad2.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

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

            p.AddStep(Action.MORTISE, "Cut top stretcher mortise in leg11", Step.CreateParms(
                "path", "leg11.right.end2",
                "id", "leg11_topmortise1",
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
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for top of leg11", Step.CreateParms(
                "path", "leg11.end2.top",
                "id", "leg11_tenon2",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create leg12", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "leg12",
                "width", "Leg_Thickness",
                "length", "leglength",
                "thickness", "Leg_Thickness"
                ));


            p.AddStep(Action.MORTISE, "Cut stretcher mortise in leg12", Step.CreateParms(
                "path", "leg12.left.end1",
                "id", "leg12_mortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut top stretcher mortise in leg12", Step.CreateParms(
                "path", "leg12.left.end2",
                "id", "leg12_topmortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for bottom of leg12", Step.CreateParms(
                "path", "leg12.end1.top",
                "id", "leg12_tenon1",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for top of leg12", Step.CreateParms(
                "path", "leg12.end2.top",
                "id", "leg12_tenon2",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.JOIN_MT, "Attach leg11 to foot1", Step.CreateParms(
                 "mortisepath", "leg11.end1.bottom",
                 "tenonpath", "foot1.foot1_mortise1_bottom.foot1_mortise1_back"
                ));

            p.AddStep(Action.JOIN, "Attach leg12 to foot1", Step.CreateParms(
                 "path1", "leg12.end1.bottom",
                 "path2", "foot1.foot1_mortise2_bottom.foot1_mortise2_back",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create long stretcher 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "longstretcher1",
                "width", "stretcher_width",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.TENON, "Cut tenon1 in longstretcher1", Step.CreateParms(
                "path", "longstretcher1.end1.top",
                "id", "t1",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut tenon2 in longstretcher1", Step.CreateParms(
                "path", "longstretcher1.end2.top",
                "id", "t2",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create long stretcher 2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "longstretcher2",
                "width", "stretcher_width",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.TENON, "Cut tenon1 in longstretcher2", Step.CreateParms(
                "path", "longstretcher2.end1.top",
                "id", "t1",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut tenon2 in longstretcher2", Step.CreateParms(
                "path", "longstretcher2.end2.top",
                "id", "t2",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create long stretcher 3", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "longstretcher3",
                "width", "stretcher_width",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.TENON, "Cut tenon1 in longstretcher3", Step.CreateParms(
                "path", "longstretcher3.end1.top",
                "id", "t1",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut tenon2 in longstretcher3", Step.CreateParms(
                "path", "longstretcher3.end2.top",
                "id", "t2",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create long stretcher 4", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "longstretcher4",
                "width", "stretcher_width",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.TENON, "Cut tenon1 in longstretcher4", Step.CreateParms(
                "path", "longstretcher4.end1.top",
                "id", "t1",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut tenon2 in longstretcher4", Step.CreateParms(
                "path", "longstretcher4.end2.top",
                "id", "t2",
                "x", "1",
                "y", "0",
                "xsize", "stretcher_width-2",
                "ysize", "stretcher_thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.JOIN, "Attach long stretcher 1", Step.CreateParms(
                 "path2", "longstretcher1.end1.bottom",
                 "path1", "leg11.leg11_mortise1_bottom.leg11_mortise1_right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach long stretcher 2", Step.CreateParms(
                 "path2", "longstretcher2.end1.bottom",
                 "path1", "leg12.leg12_mortise1_bottom.leg12_mortise1_right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach long stretcher 3", Step.CreateParms(
                 "path2", "longstretcher3.end1.bottom",
                 "path1", "leg11.leg11_topmortise1_bottom.leg11_topmortise1_right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach long stretcher 4", Step.CreateParms(
                 "path2", "longstretcher4.end1.bottom",
                 "path1", "leg12.leg12_topmortise1_bottom.leg12_topmortise1_right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create foot2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot2",
                "width", "Leg_Thickness",
                "length", "footlength",
                "thickness", "Leg_Thickness"
                ));

            p.AddStep(Action.MORTISE, "Cut foot2 mortise 1", Step.CreateParms(
                "path", "foot2.top.end1",
                "id", "foot2_mortise1",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut foot2 mortise 1", Step.CreateParms(
                "path", "foot2.top.end2",
                "id", "foot2_mortise2",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer one end of foot2", Step.CreateParms(
                "path", "foot2.top.end1",
                "id", "foot2_chamfer1",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer the other end of foot2", Step.CreateParms(
                "path", "foot2.top.end2",
                "id", "foot2_chamfer2",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create foot2 pad 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot2_pad1",
                "width", "Leg_Thickness",
                "length", "Leg_Thickness*2",
                "thickness", "pad_thickness"
                ));

            p.AddStep(Action.NEW_BOARD, "Create foot2 pad 2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "foot2_pad2",
                "width", "Leg_Thickness",
                "length", "Leg_Thickness*2",
                "thickness", "pad_thickness"
                ));

            p.AddStep(Action.JOIN, "Attach foot2 pad 1", Step.CreateParms(
                 "path1", "foot2.bottom.end1",
                 "path2", "foot2_pad1.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach foot2 pad 2", Step.CreateParms(
                 "path1", "foot2.bottom.end2",
                 "path2", "foot2_pad2.top.end1",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create leg21", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "leg21",
                "width", "Leg_Thickness",
                "length", "leglength",
                "thickness", "Leg_Thickness"
                ));


            p.AddStep(Action.MORTISE, "Cut stretcher mortise in leg21", Step.CreateParms(
                "path", "leg21.right.end1",
                "id", "leg21_mortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut top stretcher mortise in leg21", Step.CreateParms(
                "path", "leg21.right.end2",
                "id", "leg21_topmortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for bottom of leg21", Step.CreateParms(
                "path", "leg21.end1.top",
                "id", "leg21_tenon1",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for top of leg21", Step.CreateParms(
                "path", "leg21.end2.top",
                "id", "leg21_tenon2",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create leg22", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "leg22",
                "width", "Leg_Thickness",
                "length", "leglength",
                "thickness", "Leg_Thickness"
                ));


            p.AddStep(Action.MORTISE, "Cut stretcher mortise in leg22", Step.CreateParms(
                "path", "leg22.left.end1",
                "id", "leg22_mortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut top stretcher mortise in leg22", Step.CreateParms(
                "path", "leg22.left.end2",
                "id", "leg22_topmortise1",
                "x", "(Leg_Thickness - stretcher_thickness)/2",
                "y", "Leg_Thickness",
                "xsize", "stretcher_thickness",
                "ysize", "stretcher_width-2",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for bottom of leg22", Step.CreateParms(
                "path", "leg22.end1.top",
                "id", "leg22_tenon1",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.TENON, "Cut for top of leg22", Step.CreateParms(
                "path", "leg22.end2.top",
                "id", "leg22_tenon2",
                "x", "Leg_Thickness/4",
                "y", "0",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.JOIN, "Attach leg21 to foot2", Step.CreateParms(
                 "path1", "leg21.end1.bottom",
                 "path2", "foot2.foot2_mortise1_bottom.foot2_mortise1_back",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach leg22 to foot2", Step.CreateParms(
                 "path1", "leg22.end1.bottom",
                 "path2", "foot2.foot2_mortise2_bottom.foot2_mortise2_back",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach it all together", Step.CreateParms(
                 "path1", "longstretcher2.end2.top",
                 "path2", "leg21.leg21_mortise1_bottom.leg21_mortise1_right",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create bearer2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "bearer2",
                "width", "Leg_Thickness",
                "length", "Top_Slab_Width*2+Tray_Width - Front_Overhang*2",
                "thickness", "Leg_Thickness"
                ));

            p.AddStep(Action.MORTISE, "Cut bearer2 mortise 1", Step.CreateParms(
                "path", "bearer2.top.end1",
                "id", "bearer2_mortise1",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut bearer2 mortise 1", Step.CreateParms(
                "path", "bearer2.top.end2",
                "id", "bearer2_mortise2",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer one end of bearer2", Step.CreateParms(
                "path", "bearer2.top.end1",
                "id", "bearer2_chamfer1",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer the other end of bearer2", Step.CreateParms(
                "path", "bearer2.top.end2",
                "id", "bearer2_chamfer2",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.JOIN, "Attach bearer2", Step.CreateParms(
                 "path1", "leg21.end2.top",
                 "path2", "bearer2.bearer2_mortise1_bottom.bearer2_mortise1_front",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create bearer1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "bearer1",
                "width", "Leg_Thickness",
                "length", "Top_Slab_Width*2+Tray_Width - Front_Overhang*2",
                "thickness", "Leg_Thickness"
                ));

            p.AddStep(Action.MORTISE, "Cut bearer1 mortise 1", Step.CreateParms(
                "path", "bearer1.top.end1",
                "id", "bearer1_mortise1",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.MORTISE, "Cut bearer1 mortise 1", Step.CreateParms(
                "path", "bearer1.top.end2",
                "id", "bearer1_mortise2",
                "x", "Leg_Thickness/4",
                "y", "Leg_Thickness",
                "xsize", "Leg_Thickness/2",
                "ysize", "Leg_Thickness",
                "depth", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer one end of bearer1", Step.CreateParms(
                "path", "bearer1.top.end1",
                "id", "bearer1_chamfer1",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.CHAMFER, "Chamfer the other end of bearer1", Step.CreateParms(
                "path", "bearer1.top.end2",
                "id", "bearer1_chamfer2",
                "inset", "Leg_Thickness/2"
                ));

            p.AddStep(Action.JOIN, "Attach bearer1", Step.CreateParms(
                 "path1", "leg11.end2.top",
                 "path2", "bearer1.bearer1_mortise1_bottom.bearer1_mortise1_front",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the front half of the top", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "fronttop",
                "width", "Top_Slab_Width",
                "length", "Top_Slab_Length",
                "thickness", "Top_Thickness"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>Each half of the workbench top is shown here as a single board, but that is almost certainly not true.</Paragraph><Paragraph xml:space=\"preserve\">To get a board this large and this thick, you should build it by laminating pieces.  For example, to build a top which is 12 inches wide and 4 inches thick, first cut 12 pieces of wood which are 4 inches wide and 1 inch thick.  Then face glue them all together.</Paragraph></FlowDocument>";

            p.AddStep(Action.NEW_BOARD, "Create the back half of the top", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "backtop",
                "width", "Top_Slab_Width",
                "length", "Top_Slab_Length",
                "thickness", "Top_Thickness"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>(See my comments on the previous step)</Paragraph></FlowDocument>";

            p.AddStep(Action.NEW_BOARD, "Create the front tray support", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "fronttraysupport",
                "width", "0.5",
                "length", "Top_Slab_Length",
                "thickness", "0.5"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the back tray support", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "backtraysupport",
                "width", "0.5",
                "length", "Top_Slab_Length",
                "thickness", "0.5"
                ));


            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL, 
#else
Action.MORTISE,
#endif
 "cut holes for bench dogs", Step.CreateParms(
                "path", "fronttop.top.end1",
                "dx", "0",
                "dy", "Dog_Hole_Spacing",
                "count", "(Top_Slab_Length-First_Dog_Hole) / Dog_Hole_Spacing + 1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Dog_Holes_Inset",
                "y", "First_Dog_Hole",
                "diam", "0.75",
#else
 "x", "Dog_Holes_Inset - 0.75/2",
                "y", "First_Dog_Hole - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh1"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>All these dog holes can be either round or square.</Paragraph><Paragraph>Traditional holes for bench dogs are square with the dogs made of wood.</Paragraph><Paragraph xml:space=\"preserve\">Nowadays, round dog holes are common, each hole being drilled at 3/4 inch diameter.  Woodworking stores sell pre-made bench dogs made of brass.</Paragraph></FlowDocument>";

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut holes for bench dogs", Step.CreateParms(
                "path", "fronttop.top.end1",
                "dx", "0",
                "dy", "Dog_Hole_Spacing",
                "count", "(Top_Slab_Length-First_Dog_Hole) / Dog_Hole_Spacing + 1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Top_Slab_Width-Dog_Holes_Inset",
                "y", "First_Dog_Hole",
                "diam", "0.75",
#else
 "x", "Top_Slab_Width-Dog_Holes_Inset - 0.75/2",
                "y", "First_Dog_Hole - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh2"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut holes for bench dogs", Step.CreateParms(
                "path", "backtop.top.end2",
                "dx", "0",
                "dy", "Dog_Hole_Spacing",
                "count", "(Top_Slab_Length-First_Dog_Hole) / Dog_Hole_Spacing + 1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Dog_Holes_Inset",
                "y", "First_Dog_Hole",
                "diam", "0.75",
#else
 "x", "Dog_Holes_Inset - 0.75/2",
                "y", "First_Dog_Hole - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh1"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut holes for bench dogs", Step.CreateParms(
                "path", "backtop.top.end2",
                "dx", "0",
                "dy", "Dog_Hole_Spacing",
                "count", "(Top_Slab_Length-First_Dog_Hole) / Dog_Hole_Spacing + 1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Top_Slab_Width-Dog_Holes_Inset",
                "y", "First_Dog_Hole",
                "diam", "0.75",
#else
 "x", "Top_Slab_Width-Dog_Holes_Inset - 0.75/2",
                "y", "First_Dog_Hole - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh2"
                ));

            p.AddStep(Action.JOIN, "Attach front tray support", Step.CreateParms(
                 "path1", "fronttop.right.bottom",
                 "path2", "fronttraysupport.bottom.left",
                 "align", "right",
                "offset1", "0", "offset2", "0"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph xml:space=\"preserve\">Take extra care when you glue these tray supports.  You don't want the tray falling through because of a bad glue joint.</Paragraph><Paragraph>Still, if it does, the shelf below should catch things before they hit the floor.</Paragraph></FlowDocument>";

            p.AddStep(Action.JOIN, "Attach back tray support", Step.CreateParms(
                 "path1", "backtop.right.bottom",
                 "path2", "backtraysupport.bottom.left",
                 "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.JOIN, "Attach front top", Step.CreateParms(
                 "path1", "fronttop.bottom.left",
                 "path2", "bearer1.bottom.end1",
                 "align", "right",
                "offset1", "Left_Overhang", "offset2", "-Front_Overhang"
                ));

            p.AddStep(Action.JOIN, "Attach back top", Step.CreateParms(
                 "path1", "backtop.bottom.left",
                 "path2", "bearer1.bottom.end2",
                 "align", "left",
                "offset1", "Left_Overhang", "offset2", "-Front_Overhang"
                ));

            p.AddStep(Action.NEW_BOARD, "Create shelf support 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "shelfsupport1",
                "width", "stretcher_width - 1.5",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness*2",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.JOIN, "Attach shelf support 1", Step.CreateParms(
                 "path2", "shelfsupport1.top.right",
                 "path1", "longstretcher1.bottom.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create shelf support 1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "shelfsupport2",
                "width", "stretcher_width - 1.5",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness*2",
                "thickness", "stretcher_thickness"
                ));

            p.AddStep(Action.JOIN, "Attach shelf support 2", Step.CreateParms(
                 "path2", "shelfsupport2.top.right",
                 "path1", "longstretcher2.top.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the lower shelf", Step.CreateParms(
                "material", "plywood.oak",
                "newname", "shelf",
                "width", "Top_Slab_Width*2+Tray_Width - Leg_Thickness*3 - Front_Overhang*2 - stretcher_thickness",
                "length", "Top_Slab_Length - Left_Overhang - Right_Overhang - Leg_Thickness*2",
                "thickness", "1.5"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph xml:space=\"preserve\">I'm specifying this shelf as plywood, a inch and a half thick.  That's probably overkill, but I like to overbuild things.  Anyway, I assume it's actually made of two 3/4 inch pieces laminated together.</Paragraph></FlowDocument>";

            p.AddStep(Action.JOIN, "Attach lower shelf", Step.CreateParms(
                 "path2", "shelf.top.left",
                 "path1", "shelfsupport1.left.top",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the front apron", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "frontapron",
                "width", "Top_Thickness+ 1.5 + 2.75",
                "length", "Top_Slab_Length+2+2",
                "thickness", "2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create left endcap", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "leftendcap",
                "width", "Top_Thickness + 1.5 + 2.75",
                "length", "Top_Slab_Width*2+Tray_Width+2",
                "thickness", "2"
                ));

            p.AddStep(Action.NEW_BOARD, "Create right endcap", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "rightendcap",
                "width", "Top_Thickness + 1.5 + 2.75",
                "length", "Top_Slab_Width*2+Tray_Width+2",
                "thickness", "2"
                ));

            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "frontapron.end1.top",
                "path2", "rightendcap.end1.top",
                "numtails", "2",
                "tailwidth", "0",
                "id", "dtfrontright"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dtfrontright"
                ));

            p.AddStep(Action.DOVETAIL_TAILS, null, Step.CreateParms(
                "path1", "frontapron.end2.top",
                "path2", "leftendcap.end1.top",
                "numtails", "2",
                "tailwidth", "0",
                "id", "dtfrontleft"
                ));
            p.AddStep(Action.DOVETAIL_PINS, null, Step.CreateParms(
                "id", "dtfrontleft"
                ));

            p.AddStep(Action.JOIN, "Attach front border", Step.CreateParms(
                 "path1", "fronttop.left.top",
                 "path2", "frontapron.bottom.right",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dtfrontright"
                ));

            p.AddStep(Action.DOVETAIL_JOIN, null, Step.CreateParms(
                "id", "dtfrontleft"
                ));

            p.AddStep(Action.NEW_BOARD, "Create endvise face", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "endvise",
                "width", "Top_Thickness + 1.5 + 2.75",
                "length", "Top_Slab_Width*2+Tray_Width+2",
                "thickness", "2"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL, 
#else
Action.MORTISE,
#endif
 "cut endvise holes for bench dogs", Step.CreateParms(
                "path", "endvise.right.bottom",
                "dx", "0",
                "dy", "0",
                "count", "1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Dog_Holes_Inset",
                "y", "1",
                "diam", "0.75",
#else
 "x", "Dog_Holes_Inset - 0.75/2",
                "y", "1 - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh1"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut endvise holes for bench dogs", Step.CreateParms(
                "path", "endvise.right.bottom",
                "dx", "0",
                "dy", "0",
                "count", "1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Top_Slab_Width-Dog_Holes_Inset",
                "y", "1",
                "diam", "0.75",
#else
 "x", "Top_Slab_Width-Dog_Holes_Inset - 0.75/2",
                "y", "1 - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh2"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut endvise holes for bench dogs", Step.CreateParms(
                "path", "endvise.right.bottom",
                "dx", "0",
                "dy", "0",
                "count", "1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Top_Slab_Width+Tray_Width+Dog_Holes_Inset",
                "y", "1",
                "diam", "0.75",
#else
 "x", "Top_Slab_Width+Tray_Width+Dog_Holes_Inset - 0.75/2",
                "y", "1 - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh3"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut endvise holes for bench dogs", Step.CreateParms(
                "path", "endvise.right.bottom",
                "dx", "0",
                "dy", "0",
                "count", "1",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "Top_Slab_Width+Tray_Width+Top_Slab_Width-Dog_Holes_Inset",
                "y", "1",
                "diam", "0.75",
#else
 "x", "Top_Slab_Width+Tray_Width+Top_Slab_Width-Dog_Holes_Inset - 0.75/2",
                "y", "1 - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh4"
                ));

            p.AddStep(Action.JOIN, "Attach endvise", Step.CreateParms(
                 "path2", "endvise.bottom.end1",
                 "path1", "rightendcap.top.end2",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create shelf endcap1", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "shelfendcap1",
                "width", "1.5",
                "length", "Top_Slab_Width*2+Tray_Width - Leg_Thickness*4 - Front_Overhang*2",
                "thickness", "1"
                ));

            p.AddStep(Action.JOIN, "Attach shelf endcap1", Step.CreateParms(
                 "path2", "shelfendcap1.bottom.right",
                 "path1", "shelf.end1.top",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(Action.NEW_BOARD, "Create shelf endcap2", Step.CreateParms(
                "material", "solid.oak.red",
                "newname", "shelfendcap2",
                "width", "1.5",
                "length", "Top_Slab_Width*2+Tray_Width - Leg_Thickness*4 - Front_Overhang*2",
                "thickness", "1"
                ));

            p.AddStep(Action.JOIN, "Attach shelf endcap2", Step.CreateParms(
                 "path2", "shelfendcap2.bottom.right",
                 "path1", "shelf.end2.top",
                 "align", "center",
                "offset1", "0", "offset2", "0"
                ));


            p.AddStep(Action.NEW_BOARD, "Create the tray part 1", Step.CreateParms(
                "material", "plywood.oak",
                "newname", "tray1",
                "width", "Tray_Width",
                "length", "Top_Slab_Length/2",
                "thickness", "traythickness"
                ));

            p.AddStep(Action.NEW_BOARD, "Create the tray part 2", Step.CreateParms(
                "material", "plywood.oak",
                "newname", "tray2",
                "width", "Tray_Width",
                "length", "Top_Slab_Length/2",
                "thickness", "traythickness"
                ));

            p.AddStep(Action.JOIN, "Attach tray", Step.CreateParms(
                 "path2", "tray1.bottom.left",
                 "path1", "fronttraysupport.right.bottom",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph xml:space=\"preserve\">Don't actually attach the tray.  It's supposed to simply sit there on its supports.  It's handy to be able to remove it for situations where you want to clamp something in the middle.</Paragraph><Paragraph>You may want to drill a hole in the tray to make it easier to grab onto it when you want to remove it.</Paragraph></FlowDocument>";

            p.AddStep(Action.JOIN, "Attach tray", Step.CreateParms(
                 "path1", "tray1.end1.top",
                 "path2", "tray2.end1.top",
                 "align", "left",
                "offset1", "0", "offset2", "0"
                )).document = "<FlowDocument AllowDrop=\"True\" PagePadding=\"5,0,5,0\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>(See my comments on the previous step)</Paragraph></FlowDocument>";

            p.AddStep(Action.NEW_BOARD, "Front vise front jaw", Step.CreateParms(
                "material", "solid.maple.hard",
                "newname", "frontvisefrontjaw",
                "width", "Top_Thickness + 1.5 + 2.75",
                "length", "frontviselength",
                "thickness", "2"
                ));

            p.AddStep(Action.JOIN, "Attach front vise front jaw", Step.CreateParms(
                 "path1", "frontapron.top.right",
                 "path2", "frontvisefrontjaw.bottom.left",
                 "align", "right",
                "offset1", "0", "offset2", "0"
                ));

            p.AddStep(
#if WORKBENCH_DOG_HOLES_ROUND
                Action.DRILL,
#else
Action.MORTISE,
#endif
 "cut holes for bench dogs", Step.CreateParms(
                "path", "frontvisefrontjaw.left.bottom",
                "dx", "Dog_Hole_Spacing",
                "dy", "0",
                "count", "frontviselength / Dog_Hole_Spacing",
#if WORKBENCH_DOG_HOLES_ROUND
                "x", "First_Dog_Hole + 10 * Dog_Hole_Spacing - Top_Slab_Length + frontviselength - 2",
                "y", "1",
                "diam", "0.75",
#else
 "x", "First_Dog_Hole + 10 * Dog_Hole_Spacing - Top_Slab_Length + frontviselength - 2 - 0.75/2",
                "y", "1 - 0.75/2",
                "xsize", "0.75",
                "ysize", "0.75",
#endif
 "depth", "Top_Thickness",
                "id", "dh1"
                ));

            return p;
        }

    }
}
