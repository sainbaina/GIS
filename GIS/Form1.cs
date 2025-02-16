using System.Windows.Forms;

namespace GIS
{
    public partial class Form1 : Form
    {
        Map map = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //map = new Map();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "GeoJSON Files (*.geojson)|*.geojson";
                openFileDialog.Title = "Âûבונטעו GeoJSON פאיכ";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }

            if (filePath != "")
            {
                List<MapObject> objects = GEOJsonToPrimitives.Parse(filePath);
                Layer layer = new Layer(1);
                foreach (var obj in objects)
                {
                    layer.Add(obj);
                }
                map.AddLayer(layer);
            }
        }

        private void map1_Load(object sender, EventArgs e)
        {
            map = map1;
            Controls.Add(map);
        }

        private void ShowScale()
        {
            Control scaleLabel = this.Controls.Find("scaleLabel", true).FirstOrDefault();
            scaleLabel.Text = map.ScaleFactor.ToString();
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            map.ScaleFactor *= 1.2f;
            ShowScale();
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            map.ScaleFactor /= 1.2f;
            ShowScale();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
