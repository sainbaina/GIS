namespace GIS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        { 
            //Layer layer0 = new Layer(0);
            //layer0.Add(new Point(10, 10));
            //layer0.Add(new Line(
            //    new Point(0, 0.4f), 
            //    new Point(123.67f, -444)
            //));
            //layer0.Add(new Polygon([
            //    new Point(0, 0.4f), 
            //    new Point(123.67f, -444),
            //    new Point(7000, 33.3f),
            //]));
            //layer0.Add(new Multiline(
            //    new List<Point[]>
            //    {
            //        new Point[] 
            //        {
            //            new Point(0,0),
            //            new Point(1,0),
            //            new Point(2,0),
            //            new Point(3,1),
            //        },
            //        new Point[]
            //        {
            //            new Point(1,0),
            //            new Point(1,1),
            //            new Point(2,1),
            //            new Point(3,2),
            //        }
            //    }
            //));
            //layer0.Add(new Text("Нью-Йоркск", new Point(12,12)));

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}