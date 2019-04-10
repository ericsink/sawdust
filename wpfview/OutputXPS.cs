using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Xml;
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
    public class OutputXPS
    {
        private static FixedPage CreateFixedPageForStep(Step st, PrintStuff ps)
        {
            FixedPage page = new FixedPage();
            page.Background = Brushes.White;
            page.Width = ps.dpi * ps.page_width;
            page.Height = ps.dpi * ps.page_height;

            double picWidth = 4;
            double picHeight = 4;

            ViewMaker vo = new ViewMaker();
            vo.bAnimate = false;
            vo.bShowAnnotations = true;
            vo.bShowAxes = false;
            vo.bShowEndGrain = true;
            vo.bShowFaceLabels = true;
            vo.bShowHighlights = true;
            vo.bShowLines = true;
            vo.bStaticLines = true;
            vo.height = (int)(ps.dpi * picHeight);
            vo.width = (int)(ps.dpi * picWidth);
            vo.bCloneTran = true;
            vo.bAutoZoom = true;
            vo.transparencies = null;

            vo.MakeView(st);

            // Move the picture to the center of the area
            Rect r = wpfmisc.Get2DBoundingBox(vo.vp);
            vo.vp.RenderTransform = new TranslateTransform((vo.vp.Width - r.Width) / 2 - r.Left, (vo.vp.Height - r.Height) / 2 - r.Top);

            Border b = new Border();
            //b.Background = Brushes.Yellow;
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Black;
            b.Child = vo.vp;

            FixedPage.SetLeft(b, ps.dpi * ps.margin_left);
            FixedPage.SetTop(b, ps.dpi * (ps.margin_left * 2));

            page.Children.Add((UIElement)b);

            TextBlock tbTitle = new TextBlock();
            tbTitle.Text = st.Description;
            tbTitle.FontSize = 24;
            tbTitle.FontFamily = new FontFamily("Arial");
            FixedPage.SetLeft(tbTitle, ps.dpi * ps.margin_left);
            FixedPage.SetTop(tbTitle, ps.dpi * ps.margin_top);
            page.Children.Add((UIElement)tbTitle);

            TextBlock tbProse = new TextBlock();
            tbProse.TextWrapping = TextWrapping.Wrap;
            tbProse.MaxWidth = vo.width;
            tbProse.Text = st.prose;
            tbProse.FontSize = 12;
            tbProse.FontFamily = new FontFamily("serif");
            FixedPage.SetLeft(tbProse, ps.dpi * ps.margin_left);
            FixedPage.SetTop(tbProse, ps.dpi * (ps.margin_top + 1));
            page.Children.Add((UIElement)tbProse);

            FlowDocumentScrollViewer fdsv_notes = new FlowDocumentScrollViewer();
            fdsv_notes.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            fdsv_notes.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            fdsv_notes.Width = ps.dpi * 3;
            fdsv_notes.Height = ps.dpi * 3;
            if (st.Notes != null)
            {
                // TODO fdsv_notes.Document = XamlReader.Load(new XmlTextReader(new StringReader(st.Notes))) as FlowDocument;
            }
            FixedPage.SetLeft(fdsv_notes, ps.dpi * ps.margin_left);
            FixedPage.SetTop(fdsv_notes, ps.dpi * 6);
            page.Children.Add((UIElement)fdsv_notes);

            TextBlock tbCredit = new TextBlock(new Run("www.sawdust.com"));
            tbCredit.FontSize = 10;
            tbCredit.FontFamily = new FontFamily("Arial");

            FixedPage.SetRight(tbCredit, ps.dpi * ps.margin_right);
            FixedPage.SetBottom(tbCredit, ps.dpi * ps.margin_bottom);
            page.Children.Add(tbCredit);

            Size sz = new Size(ps.dpi * ps.page_width, ps.dpi * ps.page_height);
            page.Measure(sz);
            page.Arrange(new Rect(new Point(), sz));
            page.UpdateLayout();

            return page;
        }

        public class PrintStuff
        {
            public double margin_left = 0.75;
            public double margin_right = 0.75;
            public double margin_top = 0.75;
            public double margin_bottom = 0.75;
            public double dpi = 96;
            public double page_width = 8.5;
            public double page_height = 11;
        }

        public static FixedDocument CreateFixedDocument(Step st)
        {
            PrintStuff ps = new PrintStuff();

            FixedDocument doc = new FixedDocument();
            doc.DocumentPaginator.PageSize = new Size(ps.dpi * ps.page_width, ps.dpi * ps.page_height);

            PageContent page = new PageContent();
            FixedPage fixedPage = CreateFixedPageForStep(st, ps);
            ((IAddChild)page).AddChild(fixedPage);

            doc.Pages.Add(page);

            return doc;
        }

        public static FixedDocument CreateFixedDocument(Plan p)
        {
            PrintStuff ps = new PrintStuff();

            FixedDocument doc = new FixedDocument();
            doc.DocumentPaginator.PageSize = new Size(ps.dpi * ps.page_width, ps.dpi * ps.page_height);

            foreach (Step st in p.Steps)
            {
                PageContent page = new PageContent();
                FixedPage fixedPage = CreateFixedPageForStep(st, ps);
                ((IAddChild)page).AddChild(fixedPage);

                doc.Pages.Add(page);

                break; // TODO
            }

            return doc;
        }

    }
}
