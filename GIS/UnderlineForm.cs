using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIS
{
    public partial class UnderlineForm : Form
    {
        public event Action LayersChanged;

        private Dictionary<MapObject, bool> _objects;
        public Dictionary<MapObject, bool> Objects => _objects;

        public UnderlineForm(Dictionary<MapObject, bool> objects)
        {
            InitializeComponent();
            _objects = objects;

            foreach (var obj in objects)
            {
                int index = checkedListBox1.Items.Add(obj.Key.GetType().ToString());
                checkedListBox1.SetItemChecked(index, obj.Value);
            }

            checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
        }

        //public Dictionary<MapObject, bool> GetCheckedObjects()
        //{
        //    return _objects;
        //}

        private void UnderlineForm_Load(object sender, EventArgs e)
        {

        }

        public void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            KeyValuePair< MapObject, bool> obj = _objects.ElementAt(e.Index);

            if (obj.Key != null && _objects.ContainsKey(obj.Key))
            {
                _objects[obj.Key] = (e.NewValue == CheckState.Checked);
            }

            LayersChanged?.Invoke();
        }
    }
}
