using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using NodeEditor;

namespace Sound_Processor
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            nodeEditor.nodesControl.Context = new SoundContext();
            nodeEditor.nodesControl.OnNodeContextSelected += NodesControlOnOnNodeContextSelected;
            try
            {
                byte[] template = File.ReadAllBytes("template.snd");
                nodeEditor.nodesControl.Deserialize(template);
            }
            catch
            {
            }
        }

        private void NodesControlOnOnNodeContextSelected(object obj)
        {
            nodeEditor.propertyGrid.SelectedObject = obj;

            NodesGraph graph = nodeEditor.nodesControl.graph;
            if (graph.Nodes.Exists(x => x.IsSelected))
            {
                foreach (NodeVisual n in graph.Nodes.Where(x => x.IsSelected))
                {
                    if (n.Name != "Process")
                        nodeEditor.nodesControl.Execute(n);

                    //dynamic context = obj;
                    dynamic context = n.GetNodeContext();
                    try
                    {
                        Samples samples = context.samplesOut;

                        Series series = new Series();
                        float yMin = 0;
                        float yMax = 0;
                        for (int i = 0; i < samples.Data.Length; i++)
                        {
                            series.Points.AddXY(i / 44100f, samples.Data[i]);

                            if (samples.Data[i] > yMax)
                                yMax = samples.Data[i];
                            if (samples.Data[i] < yMin)
                                yMin = samples.Data[i];
                        }

                        series.ChartType = SeriesChartType.Line;
                        chart.Series.Clear();
                        chart.ChartAreas[0].AxisY.IsStartedFromZero = false;
                        chart.Series.Add(series);

                        if (yMax != yMin)
                        {
                            chart.ChartAreas[0].AxisY.Maximum = yMax;
                            chart.ChartAreas[0].AxisY.Minimum = yMin;
                        }
                        else
                        {
                            chart.ChartAreas[0].AxisY.Maximum = yMax + 1;
                            chart.ChartAreas[0].AxisY.Minimum = yMin - 1;
                        }
                    }
                    catch
                    {
                    }
                    try
                    {
                        Signal signal = context.signal;

                        Series series = new Series();
                        float yMin = 0;
                        float yMax = 0;
                        for (int i = 0; i < signal.Length; i++)
                        {
                            series.Points.AddXY(i / 44100f, signal[i]);

                            if (signal[i] > yMax)
                                yMax = signal[i];
                            if (signal[i] < yMin)
                                yMin = signal[i];
                        }

                        series.ChartType = SeriesChartType.Line;
                        chart.Series.Clear();
                        chart.ChartAreas[0].AxisY.IsStartedFromZero = false;
                        chart.Series.Add(series);

                        if (yMax != yMin)
                        {
                            chart.ChartAreas[0].AxisY.Maximum = yMax;
                            chart.ChartAreas[0].AxisY.Minimum = yMin;
                        }
                        else
                        {
                            chart.ChartAreas[0].AxisY.Maximum = yMax + 1;
                            chart.ChartAreas[0].AxisY.Minimum = yMin - 1;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            
        }

        private void nodeEditor_Load(object sender, EventArgs e)
        {

        }

        private void nodeEditor_Load_1(object sender, EventArgs e)
        {

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgSaveFile.ShowDialog() == DialogResult.OK)
            {
                byte[] data = nodeEditor.nodesControl.Serialize();
                File.WriteAllBytes(dlgSaveFile.FileName, data);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(dlgOpenFile.FileName);
                nodeEditor.nodesControl.Deserialize(data);
            }
        }
    }
}
