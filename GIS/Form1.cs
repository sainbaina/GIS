using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GIS
{
    public partial class Form1 : Form
    {
        Map map = null;
        public Dictionary<int, Layer> layersDictionary = new();
        public int currLayer = 0;

        public Form1()
        {
            InitializeComponent();
            //InitializeButtons(); // Call to initialize buttons
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Additional initialization logic can go here
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "GeoJSON Files (*.geojson)|*.geojson";
                openFileDialog.Title = "Выберите GeoJSON файл";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }

            if (string.IsNullOrEmpty(filePath)) return;

            using (ChoseLayerType layerTypeForm = new ChoseLayerType())
            {
                if (layerTypeForm.ShowDialog() == DialogResult.OK)
                {
                    Enums.LayerType selectedLayerType = layerTypeForm.SelectedLayerType;

                    List<MapObject> objects = GEOJsonToPrimitives.Parse(filePath);
                    Layer layer = new Layer(0, selectedLayerType);
                    layer.Name = Path.GetFileNameWithoutExtension(filePath);

                    bool isDuplicate = layersDictionary.Values.Any(existingLayer => existingLayer.Name == layer.Name);
                    if (isDuplicate)
                    {
                        MessageBox.Show("Unable to add the same layer.");
                        return;
                    }

                    foreach (var obj in objects)
                    {
                        obj.Layer = layer;
                        layer.Add(obj);
                    }

                    map.AddLayer(layer);

                    layerCheckList.Items.Insert(0, layer.Name + $" [{0}]");
                    layerCheckList.SetItemChecked(0, true);

                    Dictionary<int, Layer> newDict = new();
                    foreach (KeyValuePair<int, Layer> item in layersDictionary)
                    {
                        layerCheckList.Items[item.Key + 1] = $"{item.Value.Name} [{item.Key + 1}]";
                        newDict[item.Key + 1] = item.Value;
                        item.Value.LayerNumber++;
                    }

                    newDict[0] = layer;
                    layersDictionary = newDict;

                    layerCheckList.ItemCheck -= LayerCheckList_ItemCheck;
                    layerCheckList.ItemCheck += LayerCheckList_ItemCheck;
                    layerCheckList.MouseUp -= LayerCheckList_MouseUp;
                    layerCheckList.MouseUp += LayerCheckList_MouseUp;

                    currLayer++;
                }
            }
        }

        private void LayerCheckList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (layersDictionary.TryGetValue(e.Index, out Layer layer))
            {
                layer.Visible = e.NewValue == CheckState.Checked;
                map.Invalidate();
            }
        }

        private void BtnDeleteLayer_Click(object sender, EventArgs e)
        {
            int selectedIndex = layerCheckList.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select a layer to delete.");
                return;
            }

            if (layersDictionary.TryGetValue(selectedIndex, out Layer selectedLayer))
            {
                map.RemoveLayer(selectedLayer);
                layersDictionary.Remove(selectedIndex);

                Dictionary<int, Layer> updatedDictionary = new();
                int newIndex = 0;
                foreach (var entry in layersDictionary.OrderBy(kvp => kvp.Key))
                {
                    updatedDictionary[newIndex] = entry.Value;
                    entry.Value.LayerNumber = newIndex;
                    newIndex++;
                }

                layersDictionary = updatedDictionary;

                UpdateCheckList();
                map.Invalidate();
            }
        }

        private void BtnChangeColor_Click(object sender, EventArgs e)
        {
            int selectedIndex = layerCheckList.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select a layer to change its color.");
                return;
            }

            if (layersDictionary.TryGetValue(selectedIndex, out Layer selectedLayer))
            {
                using (ChoseLayerType chooseLayerTypeDialog = new ChoseLayerType())
                {
                    if (chooseLayerTypeDialog.ShowDialog() == DialogResult.OK)
                    {
                        Enums.LayerType selectedLayerType = chooseLayerTypeDialog.SelectedLayerType;
                        Color selectedColor = Color.FromArgb((int)selectedLayerType);

                        selectedLayer.Color = selectedColor;

                        map.Invalidate();
                    }
                }
            }
        }

        private void LayerCheckList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            int index = layerCheckList.IndexFromPoint(e.Location);

            if (index == ListBox.NoMatches)
                return;

            if (!layersDictionary.TryGetValue(index, out Layer selectedLayer))
                return;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Выбери желаемый слой\n" +
                $"Текущий слой: {selectedLayer.LayerNumber}",
                "Выбор слоя");

            if (int.TryParse(input, out int newIndex) && newIndex >= 0)
            {
                ReorderLayers(index, newIndex);
                UpdateCheckList();
            }
        }

        private void UpdateCheckList()
        {
            layerCheckList.Items.Clear();

            foreach (var entry in layersDictionary.OrderBy(kvp => kvp.Key))
            {
                layerCheckList.Items.Add($"{entry.Value.Name} [{entry.Value.LayerNumber}]", entry.Value.Visible);
            }
        }

        private void ReorderLayers(int currInd, int aimInd)
        {
            if (aimInd < 0) return;
            else if (aimInd == currInd) return;
            else if (aimInd >= layersDictionary.Count)
            {
                aimInd = layersDictionary.Count - 1;
            }

            int min = Math.Min(currInd, aimInd);
            int max = Math.Max(currInd, aimInd);
            int d = 0;
            if (currInd < aimInd)
            {
                d = -1;
            }
            else if (currInd > aimInd)
            {
                d = 1;
            }

            Dictionary<int, Layer> updatedDictionary = new();
            foreach (var entry in layersDictionary)
            {
                if (entry.Key == currInd)
                {
                    updatedDictionary[aimInd] = entry.Value;
                    entry.Value.LayerNumber = aimInd;
                    continue;
                }
                if (entry.Key >= min && entry.Key <= max)
                {
                    updatedDictionary[entry.Key + d] = entry.Value;
                    entry.Value.LayerNumber = entry.Key + d;
                    continue;
                }
                updatedDictionary[entry.Key] = entry.Value;
            }

            layersDictionary = updatedDictionary;
        }

        private void map1_Load(object sender, EventArgs e)
        {
            map = map1;
            map.ShowRects = checkBox1.Checked;
            Controls.Add(map);

            map.ScaleChanged += (s, e) =>
            {
                Label scaleLabel = (Label)this.Controls.Find("scaleLabel", true).FirstOrDefault();
                scaleLabel.Text = map.currLayerInd.ToString() + " " + map.ScaleFactor.ToString();
            };
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            map.ShowRects = checkBox1.Checked;
            map.Refresh();
        }

        private void ChangeColor_Click(object sender, EventArgs e)
        {
            // Ensure a layer is selected
            int selectedIndex = layerCheckList.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select a layer to change its color.");
                return;
            }

            // Retrieve the selected layer from the dictionary
            if (layersDictionary.TryGetValue(selectedIndex, out Layer selectedLayer))
            {
                // Open the ChoseLayerType dialog to let the user pick a layer type
                using (ChoseLayerType chooseLayerTypeDialog = new ChoseLayerType())
                {
                    if (chooseLayerTypeDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the selected layer type and its associated color
                        Enums.LayerType selectedLayerType = chooseLayerTypeDialog.SelectedLayerType;
                        Color selectedColor = Color.FromArgb((int)selectedLayerType);

                        // Update the layer's color with the selected color
                        selectedLayer.Color = selectedColor;

                        // Refresh the map to reflect the color change
                        map.Invalidate();
                    }
                }
            }
        }

        private void LayerDelete_Click(object sender, EventArgs e)
        {
            int selectedIndex = layerCheckList.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select a layer to delete.");
                return;
            }

            if (layersDictionary.TryGetValue(selectedIndex, out Layer selectedLayer))
            {
                map.RemoveLayer(selectedLayer);
                layersDictionary.Remove(selectedIndex);

                Dictionary<int, Layer> updatedDictionary = new();
                int newIndex = 0;
                foreach (var entry in layersDictionary.OrderBy(kvp => kvp.Key))
                {
                    updatedDictionary[newIndex] = entry.Value;
                    entry.Value.LayerNumber = newIndex;
                    newIndex++;
                }

                layersDictionary = updatedDictionary;

                UpdateCheckList();
                map.Invalidate();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        private void label2_Click_1(object sender, EventArgs e)
        {
            
        }
    }
}