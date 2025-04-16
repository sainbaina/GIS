using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIS
{
    public partial class ChoseLayerType : Form
    {
        public Enums.LayerType SelectedLayerType { get; private set; }

        public ChoseLayerType()
        {
            InitializeComponent();

            foreach (var layerType in Enum.GetValues(typeof(Enums.LayerType)))
            {
                layerTypeComboBox.Items.Add(layerType);
            }

            if (layerTypeComboBox.Items.Count > 0)
            {
                layerTypeComboBox.SelectedIndex = 0;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (layerTypeComboBox.SelectedItem != null)
            {
                SelectedLayerType = (Enums.LayerType)layerTypeComboBox.SelectedItem;
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите тип слоя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
