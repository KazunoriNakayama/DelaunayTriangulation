using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Rhino;
using Rhino.DocObjects;


class Rhino_Processing
{

    protected RhinoDoc doc;
    public static int frame_no;
    protected bool is_Running;

    protected Window window;

    public Rhino_Processing()
    {
        frame_no = 0;
        is_Running = true;
        RhinoApp.EscapeKeyPressed += Abort;
    }

    public void Run(RhinoDoc _doc, bool refresh = true)
    {

        doc = _doc;


        //Rhino.Display.DisplayPipeline.DrawForeground += 

        // make dialog to stop iteration
        window = new Window
        {
            Title = "stop loop",
            Width = 100,
            Height = 50
        };

        StackPanel stack_panel = new StackPanel();

        Button stop_button = new Button();
        stop_button.Content = "stop";
        stop_button.Click += Abort;

        stack_panel.Children.Add(stop_button);
        window.Content = stack_panel;

        new System.Windows.Interop.WindowInteropHelper(window).Owner = Rhino.RhinoApp.MainWindowHandle();
        window.Show();

        // iteration starts here
        DateTime time_start = DateTime.Now;

        Setup(); // frame_no

        while (is_Running)
        {

            if (refresh) Refresh();

            frame_no++;
            Draw();

            doc.Views.Redraw();
            RhinoApp.Wait();
        }

        Finish();

        TimeSpan duration = DateTime.Now - time_start;

        double frames_per_second = Math.Truncate(frame_no / duration.TotalSeconds * 100.0) / 100.0;
        RhinoApp.WriteLine("fps: " + frames_per_second);

        frame_no = 0;
    }

    public virtual void Setup()
    {

        RhinoApp.WriteLine("-- setup --");

    }

    public virtual void Draw()
    {


    }

    public virtual void Finish()
    {

    }

    public void Refresh()
    {

        List<RhinoObject> objs = new List<RhinoObject>(doc.Objects);

        for (int i = 0; i < objs.Count; i++)
        {
            doc.Objects.Delete(objs[i], true);
        }

    }

    public void Abort(object sender, EventArgs e)
    {

        is_Running = false;
        window.Close();
    }
}
