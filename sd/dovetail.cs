
using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using EricSinkMultiCoreLib;

namespace sd
{
    internal class Dovetail
    {
        public string path1;
        public string path2;
        public string id;

        CompoundSolid orig_cs1;
        CompoundSolid orig_cs2;

        int numtails;

        Inches tailwidth_bottom;
        Inches gapwidth_bottom;
        Inches boardwidth;
        double gapwidth_top;
        double tailwidth_top;

        int slope = 8;

        public Dovetail(string _id, string _path1, string _path2, int _numtails, Inches _tailwidth_specified, CompoundSolid _cs1, Solid _s1, Face _f1, HalfEdge _he1, CompoundSolid _cs2, Solid _s2, Face _f2, HalfEdge _he2)
        {
            id = _id;
            path1 = _path1;
            path2 = _path2;

            orig_cs1 = _cs1;

            orig_cs2 = _cs2;

            numtails = _numtails;

            boardwidth = _he1.Length();

            if (fp.eq_inches(_tailwidth_specified, 0))
            {
                tailwidth_bottom = boardwidth / (numtails * 2);
                gapwidth_bottom = tailwidth_bottom;
            }
            else
            {
                tailwidth_bottom = _tailwidth_specified;
                gapwidth_bottom = (boardwidth - numtails * tailwidth_bottom) / (numtails);
            }

            gapwidth_top = gapwidth_bottom - 2 * (tailwidth_bottom / slope);
            tailwidth_top = tailwidth_bottom + 2 * (tailwidth_bottom / slope);
        }

        public CompoundSolid Tails(CompoundSolid cs2)
        {
            Solid s1;
            Face f1;
            HalfEdge he1;
            orig_cs1.Lookup(path1, out s1, out f1, out he1);

            Solid s2;
            Face f2;
            HalfEdge he2;
            cs2.Lookup(path2, out s2, out f2, out he2);

            // TODO search for the actual edge

            // TODO assert the two edges have the same length


            // The dimensions above are at the base of the tail.
            // we need the dimensions at the top of the tail, flush with s2

            // the edges specify the two edges on the outside of the joint.  
            // the two edges will overlap each other when we're done.

            // cs1 is main.  cs2 will be added into it for the join.
            // cs2 is the tails.

            // length of the tails is distance from f2 to the base of
            // the tails, which is the thickness of s1.

            double s1_thickness_tail_length = f1.GetNextHalfEdge(he1).Length(); // TODO this assumes end of board is a simple rectangle with 4 edges
            double s2_thickness = f2.GetNextHalfEdge(he2).Length(); // TODO this assumes end of board is a simple rectangle with 4 edges

            // TODO assert gapwidth_top > 0 and > some reasonable number

            /*
             * Position the boards for the join.
             * Create a cutter to cut the tails
             * Cut them in cs2
             * Cut.
             * Join (already in position)
             * */

            xyz u2 = he2.UnitVector();
            xyz in2 = he2.Opposite().GetInwardNormal();

            // one end
            cs2 = bool3d.Subtract(cs2, Solid.Sweep(id + "_dt_cutleft", BoardMaterial.Find(BoardMaterial.NONE),
                (ut.MakePoly(
                he2.from,
                he2.from + in2 * s1_thickness_tail_length,
                he2.from + in2 * s1_thickness_tail_length + u2 * gapwidth_bottom / 2,
                he2.from + u2 * gapwidth_top / 2
                )),
                he2.GetInwardNormal() * s2_thickness));

            // other end
            cs2 = bool3d.Subtract(cs2, Solid.Sweep(id + "_dt_cutright", BoardMaterial.Find(BoardMaterial.NONE),
                (ut.MakePoly(
                he2.to,
                he2.to - u2 * gapwidth_top / 2,
                he2.to + in2 * s1_thickness_tail_length - u2 * gapwidth_bottom / 2,
                he2.to + in2 * s1_thickness_tail_length
                )),
                he2.GetInwardNormal() * s2_thickness));

            xyz top_start = he2.from + u2 * gapwidth_top / 2;
            xyz bot_start = he2.from + in2 * s1_thickness_tail_length + u2 * gapwidth_bottom / 2;

            for (int i = 1; i <= (numtails - 1); i++)
            {
                cs2 = bool3d.Subtract(cs2, Solid.Sweep(string.Format("{0}_dt_cut{1}", id, i), BoardMaterial.Find(BoardMaterial.NONE),
                    (ut.MakePoly(
                    top_start + u2 * ((i * tailwidth_top) + ((i - 1) * gapwidth_top)),
                    bot_start + u2 * ((i * tailwidth_bottom) + ((i - 1) * gapwidth_bottom)),
                    bot_start + u2 * ((i * tailwidth_bottom) + ((i) * gapwidth_bottom)),
                    top_start + u2 * ((i * tailwidth_top) + (i * gapwidth_top))
                    )),
                    he2.GetInwardNormal() * s2_thickness));
            }

            return cs2;
        }

        public CompoundSolid Pins(CompoundSolid cs1)
        {
            /*
             * This code is absurd.  The way we cut pins is we clone the
             * original cs2, orient it properly, re-cut the tails in the clone,
             * and use the clone as a cutter for the pins.  What we should do
             * is simply clone the current cs2 with the tails already cut,
             * then orient the clone as use it to cut the pins in cs1.  But
             * we don't know how to do that orient step.
             * */

            Solid s1;
            Face f1;
            HalfEdge he1;
            cs1.Lookup(path1, out s1, out f1, out he1);

            CompoundSolid cs2 = orig_cs2.Clone();

            Solid s2;
            Face f2;
            HalfEdge he2;
            cs2.Lookup(path2, out s2, out f2, out he2);

            orient.Edges(cs2, f1, he2.Opposite().face, he1, he2.Opposite(), EdgeAlignment.Center, 0, 0, true);

            cs2 = Tails(cs2);

            CompoundSolid new_cs1 = bool3d.Subtract(cs1, cs2);

            return new_cs1;
        }

        public void Join(CompoundSolid cs1, CompoundSolid cs2)
        {
            /*
             * This code is absurd.  We know that one side of
             * the far left tail is going to be called {id}_dt_cutleft_3,
             * and that this will get left as an imprint name on
             * the corresponding face of the pins when the tails are
             * used to cut those pins.  If the name of that face
             * ever changes, everything will break.
             * 
             * Unfortunately, we don't know what the name of the
             * endgrain pin faces will be.  The one we want (for
             * finding the edges to align for orienting the solid)
             * is either the first one or the last one, but we don't
             * know which.  So we try both.  Absurd.
             * */

            string s_s1, s_f1, s_e1;
            string s_s2, s_f2, s_e2;

            ut.ParsePath(path1, out s_s1, out s_f1, out s_e1);
            ut.ParsePath(path2, out s_s2, out s_f2, out s_e2);

            Face myf1 = cs1.FindFace(string.Format("{0}.{1}_{2}_dt_cutleft_3", s_s1, s_s2, id));
            Face myf2 = cs2.FindFace(string.Format("{0}.{1}_dt_cutleft_3", s_s2, id));
            HalfEdge myhe1 = myf1.FindEdge(string.Format("{0}_1", s_f1));
            if (myhe1 == null)
            {
                myhe1 = myf1.FindEdge(string.Format("{0}_{1}", s_f1, numtails + 1));
            }
            HalfEdge myhe2 = myf2.FindEdge(s_e2);
            orient.Edges(cs2, myf1, myf2, myhe1, myhe2, EdgeAlignment.Center, 0, 0, false);

            // TODO why don't we have to clone here?

            cs1.AddSub(cs2);
        }
    }
}

