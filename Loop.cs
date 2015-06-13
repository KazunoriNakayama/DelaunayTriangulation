using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;

namespace _5.Classes
{
    class Loop : Rhino_Processing
    {
        Delaunay delaunay = new Delaunay(300, 10);

        public override void Setup() // this runs once in the beginning.
        {
            delaunay.GeneratePoint();
            delaunay.BaseIncremental();
            delaunay.Incremental();
        }

        public override void Draw()
        {
            delaunay.Display(doc);
        }
    }
}