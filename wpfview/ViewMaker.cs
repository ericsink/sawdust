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
    public class ViewMaker
    {
        public Viewport3D vp;

        // the following are reset on every call to MakeView
        public Model3DGroup m3dgrp;
        public List<PartialModel> models;
        public CompoundSolid cs;
        public List<VectorTranslateTransform3D> movers;
        public BunchOfTriBags bunch;
        public StepViewTransform tran;

        // if the width and height are >0, the view will be arranged and updated
        public int width;
        public int height;

        public bool bShowAnnotations = true;
        public bool bShowFaceLabels = true;
        public bool bShowEndGrain = true;
        public bool bShowHighlights = true;
        public bool bShowLines = true;
        public bool bStaticLines = false;
        public bool bAnimate = false;
        public bool bShowAxes = false;
        public bool bCloneTran = false;
        public bool bAutoZoom = false;

        public List<string> transparencies = new List<string>();
        public double LineThickness = 1;

        public ViewMaker CloneSettings()
        {
            ViewMaker vo = new ViewMaker();

            vo.width = this.width;
            vo.height = this.height;

            vo.bShowAnnotations = this.bShowAnnotations;
            vo.bShowFaceLabels = this.bShowFaceLabels;
            vo.bShowEndGrain = this.bShowEndGrain;
            vo.bShowHighlights = this.bShowHighlights;
            vo.bShowLines = this.bShowLines;
            vo.bStaticLines = this.bStaticLines;
            vo.bAnimate = this.bAnimate;
            vo.bShowAxes = this.bShowAxes;
            vo.bCloneTran = this.bCloneTran;
            vo.bAutoZoom = this.bAutoZoom;

            vo.transparencies = this.transparencies;
            vo.LineThickness = this.LineThickness;

            return vo;
        }

        private static Color GetColorByFaceQuality(Face f)
        {
            FaceQuality fq = f.GetQuality();
            switch (fq)
            {
                case FaceQuality.EndGrain:
                    {
                        return Colors.Yellow;
                    }
                case FaceQuality.Ugly:
                    {
                        return Colors.Red;
                    }
                default:
                    {
                        var c = f.solid.material.GetColor();
                        return Color.FromRgb(c.R, c.G, c.B);
                    }
            }
        }

        private static Model3DGroup SetupView(Viewport3D vp, StepViewTransform vt)
        {
            Model3DGroup grp = new Model3DGroup();

            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 0, 200);
            myPCamera.LookDirection = new Vector3D(0, 0, -1);
            myPCamera.UpDirection = new Vector3D(0, 1, 0);
            myPCamera.FieldOfView = 60;
            vp.Camera = myPCamera;
            vp.Camera.Transform = vt.tran;

            grp.Children.Add(new AmbientLight(Color.FromArgb(0xff, 0x80, 0x80, 0x80)));

            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Color.FromArgb(0xff, 0x80, 0x80, 0x80);
            myDirectionalLight.Direction = new Vector3D(0, 0, -1);
            myDirectionalLight.Transform = vt.tran;
            grp.Children.Add(myDirectionalLight);

            return grp;
        }

        private void MakeModels(TriBag bag, TranslateTransform3D tt, List<Face> highlights)
        {
            bool bTransparent = (transparencies != null) && (transparencies.Contains(bag.solid.name));

            Dictionary<Color, PartialModel> curmodels = new Dictionary<Color, PartialModel>();

            foreach (Triangle3d t in bag.tris)
            {
                Color clr;
                if (
                    bShowHighlights
                    && (highlights != null)
                    && highlights.Contains(t.face)
                    )
                {
                    clr = Colors.Cyan;
                }
                else
                {
                    if (bShowEndGrain)
                    {
                        clr = GetColorByFaceQuality(t.face);
                    }
                    else
                    {
                        var c = t.face.solid.material.GetColor();
                        clr = Color.FromRgb(c.R, c.G, c.B);
                    }
                }
                PartialModel pm;
                if (!curmodels.ContainsKey(clr))
                {
                    pm = new PartialModel(bag, clr, tt, bTransparent);
                    curmodels[clr] = pm;
                }
                else
                {
                    pm = curmodels[clr];
                }

                pm.Add(t);
            }

            if (bTransparent)
            {
                this.models.AddRange(curmodels.Values);
            }
            else
            {
                this.models.InsertRange(0, curmodels.Values);
            }
        }

        public BitmapSource MakeBitmap(Step st)
        {
            MakeView(st);

            int dpi = 96;

            RenderTargetBitmap bit = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);

            Rectangle visual = new Rectangle();
            visual.Width = 96 * (((double)width) / dpi);
            visual.Height = 96 * (((double)height) / dpi);
            visual.Fill = Brushes.White;
            visual.Arrange(new Rect(0, 0, visual.Width, visual.Height));
            bit.Render(visual);

            bit.Render(vp);

            return bit;
        }

        private void CreateAnnotations(List<Annotation_PointToPoint> list)
        {
            foreach (Annotation_PointToPoint a in list)
            {
                double length = (a.pt2 - a.pt1).magnitude();

                string label = new Inches(length).GetStringWithoutUnits();

                double extra = length - label.Length * a.size - 1;
                if (extra < 0)
                {
                    extra = 0;
                }

                Point3D p1 = new Point3D(a.pt1.x, a.pt1.y, a.pt1.z);
                Point3D p2 = new Point3D(a.pt2.x, a.pt2.y, a.pt2.z);

                Vector3D over = p2 - p1;
                over.Normalize();

                Vector3D up = new Vector3D(a.dir.x, a.dir.y, a.dir.z);

                ScreenSpaceLines3D ss = new ScreenSpaceLines3D();
                ss.Color = Colors.Black;
                ss.Thickness = LineThickness;
                ss.Points.Add(p1 + up / 2);
                ss.Points.Add(p1 + up * (a.dist + a.size));
                ss.Points.Add(p2 + up / 2);
                ss.Points.Add(p2 + up * (a.dist + a.size));
                if (extra > 0)
                {
                    ss.Points.Add(p1 + up * (a.dist + a.size / 2));
                    ss.Points.Add(p1 + up * (a.dist + a.size / 2) + over * (extra / 2));
                    ss.Points.Add(p2 + up * (a.dist + a.size / 2));
                    ss.Points.Add(p2 + up * (a.dist + a.size / 2) - over * (extra / 2));
                }
                vp.Children.Add(ss);

                ModelVisual3D mv3d = wpfmisc.CreateTextLabel3D(label, Brushes.Black, true, a.size,
                    p1 + over * length / 2 + up * (a.dist + a.size / 2),
                    over, up);

                vp.Children.Add(mv3d);
            }
        }

        private void CreateAxes()
        {
            const double LENGTH = 400;

            ScreenSpaceLines3D ss = new ScreenSpaceLines3D();
            ss.Color = Colors.White;
            ss.Thickness = LineThickness;

            ss.Points.Add(new Point3D(-LENGTH, 0, 0));
            ss.Points.Add(new Point3D(LENGTH, 0, 0));

            ss.Points.Add(new Point3D(0, -LENGTH, 0));
            ss.Points.Add(new Point3D(0, LENGTH, 0));

            ss.Points.Add(new Point3D(0, 0, -LENGTH));
            ss.Points.Add(new Point3D(0, 0, LENGTH));

            ss.Transform = new TranslateTransform3D(tran.center.OffsetX, tran.center.OffsetY, tran.center.OffsetZ);

            vp.Children.Add(ss);
        }

        private static void CreateFaceLabel(Viewport3D myVP, Face f, TranslateTransform3D ttmove)
        {
            HalfEdge he = f.FindLongestEdge();
            xyz p1 = he.Center();
            xyz p2 = f.Measure(he, p1);
            xyz v = p2 - p1;
            double dist = v.magnitude();
            xyz c = p1 + v / 2;
            c += f.UnitNormal() * .001;
            Point3D center = wpfmisc.fixPoint(c);
            Vector3D over = wpfmisc.fixVector(he.UnitVector());
            Vector3D up = wpfmisc.fixVector(he.GetInwardNormal());

            double height;
            if (dist <= 1)
            {
                height = dist / 3;
            }
            else if (dist < 10)
            {
                height = 1;
            }
            else
            {
                height = 2;
            }

            // TODO the following isn't a very pretty hack
            if (f.name.Length * height > he.Length())
            {
                height = he.Length() / f.name.Length;
            }

            ModelVisual3D mv3d = wpfmisc.CreateTextLabel3D(f.name, Brushes.Black, false, height, center, over, up);
            if (ttmove != null)
            {
                mv3d.Transform = ttmove;
            }
            myVP.Children.Add(mv3d);
        }

        public void MakeLineVisual(List<Line3d> lines, TranslateTransform3D tt)
        {
            ScreenSpaceLines3D ss = new ScreenSpaceLines3D();
            ss.Color = Colors.Black;
            ss.Thickness = LineThickness;
            if (tt != null)
            {
                ss.Transform = tt;
            }
            foreach (Line3d ln in lines)
            {
                ss.Points.Add(new Point3D(ln.p1.x, ln.p1.y, ln.p1.z));
                ss.Points.Add(new Point3D(ln.p2.x, ln.p2.y, ln.p2.z));
            }
            vp.Children.Add(ss);
        }

        public void MakeView(Step st)
        {
            if (this.vp == null)
            {
                this.vp = new Viewport3D();
            }
            else
            {
                this.vp.Children.Clear();
            }

            if (this.bCloneTran)
            {
                this.tran = new StepViewTransform(st, false);
            }
            else
            {
                this.tran = StepViewTransform.GetViewTransformFor(st);
            }

            this.m3dgrp = SetupView(this.vp, this.tran);
            this.cs = st.Result;

            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            myModelVisual3D.Content = this.m3dgrp;
            this.vp.Children.Add(myModelVisual3D);

            if (this.bAnimate)
            {
                this.movers = new List<VectorTranslateTransform3D>();
            }
            else
            {
                this.movers = null;
            }

            this.bunch = st.GetBunch(this.bAnimate);

            this.models = new List<PartialModel>();

            List<Face> highlights;
            if (this.bShowHighlights)
            {
                highlights = st.GetHighlightedFacesForThisStep();
            }
            else
            {
                highlights = null;
            }

            if (this.bunch.notmoving != null)
            {
                foreach (TriBag bag in this.bunch.notmoving)
                {
                    MakeModels(bag, null, highlights);

                    if (this.bShowLines)
                    {
                        MakeLineVisual(bag.lines, null);
                    }
                    if (this.bShowFaceLabels)
                    {
                        foreach (Face f in st.facesToBeLabeled)
                        {
                            if (bag.solid == f.solid)
                            {
                                CreateFaceLabel(this.vp, f, null);
                            }
                        }
                    }
                }
            }

            if (this.bunch.moving != null)
            {
                TranslateTransform3D ttmove = new TranslateTransform3D(0, 0, 0);
                VectorTranslateTransform3D vtt = new VectorTranslateTransform3D(ttmove, this.bunch.vec, 0);
                this.movers.Add(vtt);

                foreach (TriBag bag in this.bunch.moving)
                {
                    MakeModels(bag, ttmove, highlights);

                    if (this.bShowLines)
                    {
                        MakeLineVisual(bag.lines, ttmove);
                    }

                    if (this.bShowFaceLabels)
                    {
                        foreach (Face f in st.facesToBeLabeled)
                        {
                            if (bag.solid == f.solid)
                            {
                                CreateFaceLabel(this.vp, f, ttmove);
                            }
                        }
                    }
                }
            }

            if (this.bShowAnnotations)
            {
                CreateAnnotations(st.annotations_PP);
            }

            if (this.bShowAxes)
            {
                CreateAxes();
            }

            foreach (PartialModel pm in this.models)
            {
                this.m3dgrp.Children.Add(pm.gm3d);
            }

            if (
                (this.width > 0)
                && (this.height > 0)
                )
            {
                this.vp.Width = this.width;
                this.vp.Height = this.height;
                this.vp.Measure(new Size(this.width, this.height));
                this.vp.Arrange(new Rect(0, 0, this.width, this.height));
                this.vp.UpdateLayout();

                wpfmisc.FixLines(this.vp);

                if (this.bAutoZoom)
                {
                    wpfmisc.AutoZoom(this.vp, this.tran);
                    wpfmisc.FixLines(this.vp);
                }

                if (this.bStaticLines)
                {
                    List<ScreenSpaceLines3D> remove = new List<ScreenSpaceLines3D>();
                    List<ModelVisual3D> add = new List<ModelVisual3D>();
                    foreach (ModelVisual3D mv3d in this.vp.Children)
                    {
                        if (mv3d is ScreenSpaceLines3D)
                        {
                            ScreenSpaceLines3D ss = mv3d as ScreenSpaceLines3D;
                            ModelVisual3D plain = new ModelVisual3D();
                            plain.Content = ss.Content;
                            add.Add(plain);
                            remove.Add(ss);
                        }
                    }
                    foreach (ModelVisual3D mv3d in add)
                    {
                        this.vp.Children.Add(mv3d);
                    }
                    foreach (ScreenSpaceLines3D ss in remove)
                    {
                        this.vp.Children.Remove(ss);
                    }
                }
            }
        }
    }

}
