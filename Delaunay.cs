using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;

namespace _5.Classes
{
    class Delaunay
    {

        //////////////
        //properties//
        //////////////

        private int max_size; //点をランダムに生成する範囲の最大値
        private int number_points; //生成する点の個数
        private Point3d a, b, c; //すべての点を包含する三角形の各頂点

        private List<Point3d> vertex = new List<Point3d>();

        private List<Triangle> triangle = new List<Triangle>();

        ////////////////
        //constructors//
        ////////////////

        public Delaunay
            (
            int _max_size,
            int _number_points
            )
        {
            max_size = _max_size;
            number_points = _number_points;
        }

        ///////////
        //methods//
        ///////////

        //点をランダムに生成
        public void GeneratePoint()
        {
            int seed = Environment.TickCount;

            for (int i = 0; i < number_points; i++)
            {
                Random rnd = new Random(seed++);
                vertex.Add(new Point3d(rnd.Next(0, max_size), rnd.Next(0, max_size), 0)); //0からmax_sizeまでの範囲内に点を生成
            }
        }

        //すべての点を包含する三角形
        public void BaseIncremental()
        {
            double cx = max_size / 2;
            double cy = max_size / 2;
            double r = Math.Pow((Math.Pow(max_size, 2) + Math.Pow(max_size, 2)), 0.5) / 2; //GeneratePointで生成する範囲を内接する円
            //その円を内接するように三角形の各頂点を決める。
            a = new Point3d(cx - Math.Sqrt(3) * r, cy + r, 0);
            b = new Point3d(cx, cy - 2 * r, 0);
            c = new Point3d(cx + Math.Sqrt(3) * r, cy + r, 0);
            triangle.Add(new Triangle(a, b, c));
        }

        //三角形分割を行う
        public void Incremental()
        {
            //生成した点を順番に判定
            for (int i = 0; i < vertex.Count; i++)
            {
                List<Triangle>temporary_triangle = new List<Triangle>(0); //分割を行った三角形を一時的に追加するリスト
                temporary_triangle.Clear();

                //三角形を順番に判定
                for (int j = 0; j < triangle.Count; j++) 
                {
                    //三角形の中心と点との距離が、三角形の外接円の半径よりも小さいかつ、その点が三角形の頂点でない場合処理を行う
                    if (triangle[j].Center().DistanceTo(vertex[i]) < triangle[j].Radius() && triangle[j].A() != vertex[i] && triangle[j].B() != vertex[i] && triangle[j].C() != vertex[i])
                    {
                        //分割してできた三角形を一時的に追加
                        temporary_triangle.Add(new Triangle(triangle[j].A(), triangle[j].B(), vertex[i]));
                        temporary_triangle.Add(new Triangle(triangle[j].B(), triangle[j].C(), vertex[i]));
                        temporary_triangle.Add(new Triangle(triangle[j].C(), triangle[j].A(), vertex[i]));
                        triangle.RemoveAt(j); //元の三角形を消去
                        j--; //消去して順番が繰り下がるため合わせる
                    }
                }

                //一時リストの三角形を重複判定
                for (int k = 0; k < i + 1; k++)
                {
                    for (int l = 0; l < temporary_triangle.Count; l++)
                    {
                        //生成した三角形の外接円の中に点を包含していないか判定
                        if (temporary_triangle[l].Center().DistanceTo(vertex[k]) <= temporary_triangle[l].Radius() && temporary_triangle[l].A() != vertex[k] && temporary_triangle[l].B() != vertex[k] && temporary_triangle[l].C() != vertex[k])
                         {
                             temporary_triangle.RemoveAt(l); //包含していた三角形を消去
                             l--;
                         }
                    }
                }
                    triangle.AddRange(temporary_triangle); //一時リストの三角形を追加
            }

            //すべての点を包含する三角形の各頂点をもつものを消去
            for (int i = 0; i < triangle.Count; i++)
            {
                if (triangle[i].A() == a || triangle[i].A() == b || triangle[i].A() == c || triangle[i].B() == a || triangle[i].B() == b || triangle[i].B() == c || triangle[i].C() == a || triangle[i].C() == b || triangle[i].C() == c)
                {
                    triangle.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < triangle.Count; i++)
            {
                for (int j = 0; j < triangle.Count; j++)
                {
                    if (i != j && triangle[i].A() + triangle[i].B() + triangle[i].C() == triangle[j].A() + triangle[j].B() + triangle[j].C())
                    {
                        triangle.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        //三角形を描画
        public void Display(RhinoDoc _doc)
        {
            for (int i = 0; i < triangle.Count; i++)
            {
                _doc.Objects.AddCurve(triangle[i].WriteTriangle());
            }
        }
    }

    //三角形の各頂点や外接円の半径や中心を返したいのでクラスを作成
    class Triangle
    {
        //////////////
        //properties//
        //////////////

        private Point3d point_a;
        private Point3d point_b;
        private Point3d point_c;

        private double x1, y1, x2, y2, x3, y3;

        ////////////////
        //constructors//
        ////////////////

        public Triangle
            (
                Point3d _point_a,
                Point3d _point_b,
                Point3d _point_c
            )
        {
            point_a = _point_a;
            point_b = _point_b;
            point_c = _point_c;

            x1 = point_a.X;
            y1 = point_a.Y;
            x2 = point_b.X;
            y2 = point_b.Y;
            x3 = point_c.X;
            y3 = point_c.Y;
        }

        ///////////
        //methods//
        ///////////

        public Curve WriteTriangle()　//与えられた３つの点から三角形を作る
        {
            List<Point3d> vertex_list = new List<Point3d>();
            vertex_list.Clear();
            vertex_list.Add(point_a);
            vertex_list.Add(point_b);
            vertex_list.Add(point_c);
            vertex_list.Add(point_a);
            Curve triangle = Curve.CreateInterpolatedCurve(vertex_list, 1);

            return triangle;
        }

        public Point3d Center()　//外接円の中心を返す
        {
            double cn = 2 * ((x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1));
            double x = ((y3 - y1) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1) + (y1 - y2) * (x3 * x3 - x1 * x1 + y3 * y3 - y1 * y1)) / cn;
            double y = ((x1 - x3) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1) + (x2 - x1) * (x3 * x3 - x1 * x1 + y3 * y3 - y1 * y1)) / cn;

            Point3d center = new Point3d(x, y, 0);
            return center;
        }

        public double Radius()　//外接円の半径を返す
        {
            Point3d center = Center();

            double radius = center.DistanceTo(point_a);　//外接円の中心と任意の頂点との距離が半径
            return radius;
        }

        public Point3d A()  //頂点Aを返す
        {
            return point_a;
        }

        public Point3d B()　//頂点Bを返す
        {
            return point_b;
        }

        public Point3d C()　//頂点Cを返す
        {
            return point_c;
        }
    }
}