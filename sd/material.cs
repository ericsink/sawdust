using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace sd
{
    public class RGB
    {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public RGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class BoardMaterial
    {
        private static Dictionary<string, BoardMaterial> materials;

        private static object mylock = new object();

        private static void init()
        {
            lock (mylock)
            {
                if (materials == null)
                {
                    materials = new Dictionary<string, BoardMaterial>();
                    materials[SOLID_OAK_RED] = new BoardMaterial("solid.oak.red", 41.0 / (12 * 12 * 12), new RGB(183, 113, 28));
                    materials[SOLID_OAK_WHITE] = new BoardMaterial("solid.oak.white", 46.0 / (12 * 12 * 12), new RGB(189, 139, 54));
                    materials[SOLID_MAPLE_HARD] = new BoardMaterial("solid.maple.hard", 43.0 / (12 * 12 * 12), new RGB(233, 217, 191));
                    materials[SOLID_MAPLE_SOFT] = new BoardMaterial("solid.maple.soft", 33.0 / (12 * 12 * 12), new RGB(233, 217, 191));
                    materials[SOLID_IPE] = new BoardMaterial("solid.ipe", 70.0 / (12 * 12 * 12), new RGB(135, 57, 44));
                    materials[PLYWOOD_OAK] = new BoardMaterial("plywood.oak", 75.0 / (48 * 96 * .75), new RGB(183, 113, 28));
                    materials[NONE] = new BoardMaterial("none.none", 1, new RGB(0, 0, 0));
                }
            }
        }

        static public string SOLID_OAK_RED = "solid.oak.red";
        static public string SOLID_OAK_WHITE = "solid.oak.white";
        static public string SOLID_MAPLE_HARD = "solid.maple.hard";
        static public string SOLID_MAPLE_SOFT = "solid.maple.soft";
        static public string SOLID_IPE = "solid.ipe";
        static public string PLYWOOD_OAK = "plywood.oak";
        static public string NONE = "none.none";

        private string type;
        private string species;
        private string kind;
        private double weight;
        private RGB clr;

        public static List<BoardMaterial> GetAll()
        {
            List<BoardMaterial> result = new List<BoardMaterial>();
            result.AddRange(materials.Values);
            return result;
        }

        private BoardMaterial(string bm, double w, RGB c)
        {
            string[] parts = bm.Split('.');
            type = parts[0].Trim().ToLower();
            species = parts[1].Trim().ToLower();
            if (parts.Length > 2)
            {
                kind = parts[2].Trim().ToLower();
            }
            weight = w;
            clr = c;
        }

        public static BoardMaterial Find(string bm)
        {
            init();

            if (materials.ContainsKey(bm))
            {
                return materials[bm];
            }
            // TODO find the best match.  for now, return oak.
            return materials[BoardMaterial.SOLID_OAK_RED];
        }

        public double GetPoundsPerCubicInch()
        {
            return weight;
        }

        public bool IsSolid()
        {
            return (type == "solid");
        }

        public string GetProse()
        {
            if (IsSolid())
            {
                return string.Format("solid {0}", species);
            }
            else
            {
                return string.Format("{0} {1}", species, type);
            }
        }

        public RGB GetColor()
        {
            return clr;
        }
    }
}
