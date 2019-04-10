
#if DEBUG

using System;
using System.Drawing;

namespace sd
{
    public class Builtin_Solids
    {
        public static Solid CreatePyramid(string solname, double side, double height)
        {
            Solid s = new Solid(solname, BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

            s.AddVertex(0, 0, 0);
            s.AddVertex(side, 0, 0);
            s.AddVertex(side, side, 0);
            s.AddVertex(0, side, 0);
            s.AddVertex(side / 2, side / 2, -height);

            s.CreateFaceCW("pyr_1", 0, 3, 2, 1);
            s.CreateFaceCW("pyr_2", 0, 1, 4);
            s.CreateFaceCW("pyr_3", 1, 2, 4);
            s.CreateFaceCW("pyr_4", 2, 3, 4);
            s.CreateFaceCW("pyr_5", 3, 0, 4);

            s.RecalcFacePlanes();

            return s.DoGeomChecks();
        }

        public static Solid CreateTetrahedron(string solname, double d)
        {
            Solid s = new Solid(solname, BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

            s.AddVertex(d, d, d);
            s.AddVertex(-d, -d, d);
            s.AddVertex(d, -d, -d);
            s.AddVertex(-d, d, -d);

            s.CreateFace("tet_1", 0, 1, 2);
            s.CreateFace("tet_2", 1, 3, 2);
            s.CreateFace("tet_3", 0, 2, 3);
            s.CreateFace("tet_4", 0, 3, 1);

            s.RecalcFacePlanes();

            return s.DoGeomChecks();
        }

        public static Solid CreateCubeWithLotsOfMortises()
        {
            CompoundSolid sol = CreateCube("cube", 50).ToCompoundSolid();
            // This is a particularly ugly case for the subtract code, which creates new solid each time.
            for (int f = 0; f < 6; f++)
            {
                for (int i = 10; i <= 40; i += 10)
                {
                    Face face = sol[0].Faces[f];
                    sol = wood.Mortise(sol, face.MainLoop[0], new xy(i, i), new xyz(3, 3, 3), string.Format("{0}_{1}", face.name, i));
                }
            }
            sol.DoGeomChecks();
            return sol[0];
        }

        public static Solid CreateCube(string solname, double d)
        {
            return wood.CreateBoard(BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED), solname, d, d, d);
        }

        public static Solid CreateCubeWithHole(string solname)
        {
            Solid s = new Solid(solname, BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

            double d = 5;

            s.AddVertex(0, 0, 0);   // 0
            s.AddVertex(d, 0, 0);   // 1
            s.AddVertex(d, d, 0);   // 2
            s.AddVertex(0, d, 0);   // 3
            s.AddVertex(0, 0, -d);  // 4
            s.AddVertex(d, 0, -d);  // 5
            s.AddVertex(d, d, -d);  // 6
            s.AddVertex(0, d, -d);  // 7

            Face cut = s.CreateFace("top", 0, 1, 2, 3);
            s.CreateFace("right", 1, 5, 6, 2);
            s.CreateFace("left", 0, 3, 7, 4);
            Face cut2 = s.CreateFace("bottom", 4, 7, 6, 5);
            s.CreateFace("end1", 0, 4, 5, 1);
            s.CreateFace("end2", 3, 2, 6, 7);

            // another mortise, all the way through

            // TODO make this into a routine that can be called

            s.AddVertex(1, 1, 0);
            s.AddVertex(1, 4, 0);
            s.AddVertex(4, 4, 0);
            s.AddVertex(4, 1, 0);

            //cw
            cut.AddHole(8, 9, 10, 11);

            s.AddVertex(1, 1, -5);
            s.AddVertex(1, 4, -5);
            s.AddVertex(4, 4, -5);
            s.AddVertex(4, 1, -5);

            cut2.AddHole(12, 15, 14, 13);

            //ccw
            s.CreateFace("cube_hole_1", 8, 12, 13, 9);
            s.CreateFace("cube_hole_2", 13, 14, 10, 9);
            s.CreateFace("cube_hole_3", 10, 14, 15, 11);
            s.CreateFace("cube_hole_4", 11, 15, 12, 8);

            s.RecalcFacePlanes();

            return s.DoGeomChecks();
        }

        public static Solid CreateSolidWithHoleAndMortise(string solname)
        {
            Solid s = new Solid(solname, BoardMaterial.Find(BoardMaterial.SOLID_OAK_RED));

            s.AddVertex(0, 0, 0);   // 0
            s.AddVertex(5, 0, 0);   // 1
            s.AddVertex(5, 5, 0);   // 2
            s.AddVertex(0, 5, 0);   // 3

            s.AddVertex(0, 0, -30); // 4
            s.AddVertex(5, 0, -30); // 5
            s.AddVertex(5, 5, -30); // 6
            s.AddVertex(0, 5, -30); // 7

            //ccw
            s.CreateFace("f1", 0, 1, 2, 3);             // end near origin
            s.CreateFace("f2", 1, 5, 6, 2);
            Face cut = s.CreateFace("f3", 3, 2, 6, 7);  // face with hole
            s.CreateFace("f4", 0, 3, 7, 4);         // left long face
            s.CreateFace("f5", 6, 5, 4, 7);         // other end
            Face cut2 = s.CreateFace("f6", 0, 4, 5, 1);

            s.RecalcFacePlanes();

            CompoundSolid cs = s.ToCompoundSolid();

            cs = wood.Mortise(cs, cut.MainLoop[1], new xy(8, 1.5), new xyz(16, 2, 2), "m1");

            cut = s.FindFace("f3");
            cs = wood.Mortise(cs, cut.MainLoop[1], new xy(1.5, 1.5), new xyz(4, 2, 5), "m2");

            cs.DoGeomChecks();

            return cs[0];
        }
    }
}

#endif
