using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Printing;
using System.IO;
using System.ComponentModel;

using sd;

namespace sdwpf
{
    public class PartialModel
    {
        public TriBag bag;
        public Color clr;
        public bool bTransparent;
        public GeometryModel3D gm3d;
        public MeshGeometry3D mesh;
        public List<Triangle3d> tris;
        int ndx;
        public Dictionary<xyz, int> shades = new Dictionary<xyz, int>();

        public PartialModel(TriBag _bag, Color _clr, TranslateTransform3D _tt, bool _transparent)
        {
            bag = _bag;
            clr = _clr;
            bTransparent = _transparent;
            gm3d = new GeometryModel3D();
            if (_tt != null)
            {
                gm3d.Transform = _tt;
            }
            Brush br = new SolidColorBrush(clr);
            if (bTransparent)
            {
                br.Opacity = wpfmisc.TRANSPARENT_OPACITY_VALUE;
            }
            gm3d.Material = new DiffuseMaterial(br);

            mesh = new MeshGeometry3D();
            gm3d.Geometry = mesh;
            mesh.Positions = new Point3DCollection();
            mesh.TriangleIndices = new Int32Collection();

            ndx = 0;
            tris = new List<Triangle3d>();
        }

        private void verify_shaded(xyz p)
        {
            if (!shades.ContainsKey(p))
            {
                mesh.Positions.Add(new Point3D(p.x, p.y, p.z));
                shades[p] = ndx++;
            }
        }

        public void Add(Triangle3d t)
        {
            tris.Add(t);

            if (t.face.Shade)
            {
                verify_shaded(t.a);
                verify_shaded(t.b);
                verify_shaded(t.c);

                mesh.TriangleIndices.Add(shades[t.a]);
                mesh.TriangleIndices.Add(shades[t.b]);
                mesh.TriangleIndices.Add(shades[t.c]);
            }
            else
            {
                mesh.Positions.Add(new Point3D(t.a.x, t.a.y, t.a.z));
                mesh.Positions.Add(new Point3D(t.b.x, t.b.y, t.b.z));
                mesh.Positions.Add(new Point3D(t.c.x, t.c.y, t.c.z));

                mesh.TriangleIndices.Add(ndx++);
                mesh.TriangleIndices.Add(ndx++);
                mesh.TriangleIndices.Add(ndx++);
            }
        }
    }
}
