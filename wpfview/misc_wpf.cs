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
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Windows.Markup;
using System.Printing;
using System.IO;
using System.ComponentModel;

using sd;

namespace sdwpf
{
    public class wpfmisc
    {
        public const double TRANSPARENT_OPACITY_VALUE = 0.3;

        private static bool TooBig(Viewport3D vp)
        {
            Rect r = wpfmisc.Get2DBoundingBox(vp);

            if (r.Left < 0)
            {
                return true;
            }
            if (r.Right > vp.ActualWidth)
            {
                return true;
            }
            if (r.Top < 0)
            {
                return true;
            }
            if (r.Bottom > vp.ActualHeight)
            {
                return true;
            }
            return false;
        }

        public static void AutoZoom(Viewport3D vp, StepViewTransform tran)
        {
            if (TooBig(vp))
            {
                while (TooBig(vp) && (tran.Zoom >= 0.1))
                {
                    tran.AddToZoom(-0.1);
                }
                while (!TooBig(vp))
                {
                    tran.AddToZoom(0.01);
                }
                if (tran.Zoom >= 0.01)
                {
                    tran.AddToZoom(-0.01);
                }
            }
            else
            {
                while (!TooBig(vp))
                {
                    tran.AddToZoom(0.1);
                }
                while (TooBig(vp))
                {
                    tran.AddToZoom(-0.01);
                }
            }
        }

        public static Rect Get2DBoundingBox(Viewport3D vp)
        {
            bool bOK;

            Viewport3DVisual vpv =
                VisualTreeHelper.GetParent(vp.Children[0]) as Viewport3DVisual;

            Matrix3D m =
                MathUtils.TryWorldToViewportTransform(vpv, out bOK);

            bool bFirst = true;
            Rect r = new Rect();

            foreach (Visual3D v3d in vp.Children)
            {
                if (v3d is ModelVisual3D)
                {
                    ModelVisual3D mv3d = (ModelVisual3D)v3d;

                    if (mv3d.Content is GeometryModel3D)
                    {
                        GeometryModel3D gm3d =
                            (GeometryModel3D)mv3d.Content;

                        if (gm3d.Geometry is MeshGeometry3D)
                        {
                            MeshGeometry3D mg3d =
                                (MeshGeometry3D)gm3d.Geometry;

                            foreach (Point3D p3d in mg3d.Positions)
                            {
                                Point3D pb = m.Transform(p3d);
                                Point p2d = new Point(pb.X, pb.Y);
                                if (bFirst)
                                {
                                    r = new Rect(p2d, new Size(1, 1));
                                    bFirst = false;
                                }
                                else
                                {
                                    r.Union(p2d);
                                }
                            }
                        }
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Creates a ModelVisual3D containing a text label.
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="textColor">The color of the text.</param>
        /// <param name="bDoubleSided">Visible from both sides?</param>
        /// <param name="height">Height of the characters</param>
        /// <param name="center">The center of the label</param>
        /// <param name="over">Horizontal direction of the label</param>
        /// <param name="up">Vertical direction of the label</param>
        /// <returns>Suitable for adding to your Viewport3D</returns>
        public static ModelVisual3D CreateTextLabel3D(
            string text,
            Brush textColor,
            bool bDoubleSided,
            double height,
            Point3D center,
            Vector3D over,
            Vector3D up)
        {
            // First we need a textblock containing the text of our label
            TextBlock tb = new TextBlock(new Run(text));
            tb.Foreground = textColor;
            tb.FontFamily = new FontFamily("Arial");

            // Now use that TextBlock as the brush for a material
            DiffuseMaterial mat = new DiffuseMaterial();
            mat.Brush = new VisualBrush(tb);

            // We just assume the characters are square
            double width = text.Length * height;

            // Since the parameter coming in was the center of the label,
            // we need to find the four corners
            // p0 is the lower left corner
            // p1 is the upper left
            // p2 is the lower right
            // p3 is the upper right
            Point3D p0 = center - width / 2 * over - height / 2 * up;
            Point3D p1 = p0 + up * 1 * height;
            Point3D p2 = p0 + over * width;
            Point3D p3 = p0 + up * 1 * height + over * width;

            // Now build the geometry for the sign.  It's just a
            // rectangle made of two triangles, on each side.

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);    // 0
            mg.Positions.Add(p1);    // 1
            mg.Positions.Add(p2);    // 2
            mg.Positions.Add(p3);    // 3

            if (bDoubleSided)
            {
                mg.Positions.Add(p0);    // 4
                mg.Positions.Add(p1);    // 5
                mg.Positions.Add(p2);    // 6
                mg.Positions.Add(p3);    // 7
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (bDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            // These texture coordinates basically stretch the
            // TextBox brush to cover the full side of the label.

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            if (bDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(1, 1));
                mg.TextureCoordinates.Add(new Point(1, 0));
                mg.TextureCoordinates.Add(new Point(0, 1));
                mg.TextureCoordinates.Add(new Point(0, 0));
            }

            // And that's all.  Return the result.

            ModelVisual3D mv3d = new ModelVisual3D();
            mv3d.Content = new GeometryModel3D(mg, mat); ;
            return mv3d;
        }

        public static Point3D fixPoint(xyz p)
        {
            return new Point3D(p.x, p.y, p.z);
        }

        public static Vector3D fixVector(xyz v)
        {
            return new Vector3D(v.x, v.y, v.z);
        }

#if not
        public static void SaveStepPictureAsPNG(Step st, string file, ViewMaker vo)
        {
            // TODO clone tran and autozoom

            BitmapSource bit = MakeBitmap(st, vo);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bit));
            File.Delete(file);
            using (Stream stm = File.Create(file))
            {
                png.Save(stm);
            }
        }
#endif

        public static void FixLines(Viewport3D vp)
        {
            foreach (ModelVisual3D mv3d in vp.Children)
            {
                if (mv3d is ScreenSpaceLines3D)
                {
                    ScreenSpaceLines3D ss = mv3d as ScreenSpaceLines3D;
                    ss.Rescale();
                }
            }
        }
    }
}

