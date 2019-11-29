using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dlid.MiHome.ExampleUI
{
    public partial class Form1 : Form
    {

        BackgroundWorker bgWorker;

        public Dictionary<string, string> templates = new Dictionary<string, string>
        {
            { "General - miIO.info", JsonConvert.SerializeObject(new { method = "miIO.info" }, Formatting.Indented) },
            { "Vacuum - app_start", JsonConvert.SerializeObject(new { method = "app_start" }, Formatting.Indented) },
            { "Vacuum - app_stop", JsonConvert.SerializeObject(new { method = "app_stop" }, Formatting.Indented) },
            { "Vacuum - app_charge", JsonConvert.SerializeObject(new { method = "app_charge" }, Formatting.Indented) },
            { "Vacuum - get_consumable", JsonConvert.SerializeObject(new { method = "get_consumable" }, Formatting.Indented) },
            { "Vacuum - get_clean_summary", JsonConvert.SerializeObject(new { method = "get_clean_summary" }, Formatting.Indented) },
            { "Vacuum - get_status", JsonConvert.SerializeObject(new { method = "get_status" }, Formatting.Indented) },
            { "Vacuum - get_timer", JsonConvert.SerializeObject(new { method = "get_timer" }, Formatting.Indented) },
            { "Vacuum - app_zoned_clean", JsonConvert.SerializeObject(new { method = "app_zoned_clean", @params = new int[] {11, 33, 44, 55, 1 }, _comment = "[x1,y1,x2,y2,times]" }, Formatting.Indented) },
        };

        public Form1()
        {
            InitializeComponent();

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;

            comboBox1.Items.Clear();
            templates.Keys.ToList().ForEach(text =>
            {
                comboBox1.Items.Add(text);
            });
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null && e.Result is Exception)
            {
                var msg = (e.Result as Exception).Message;
                textBox4.Text = "ERROR: " + msg;
            } else {
                textBox4.Text = e.Result as string;
            }
            button1.Enabled = true;
            textBox3.Enabled = true;
            textBox2.Enabled = true;
            comboBox1.Enabled = true;
            textBoxIpAddress.Enabled = true;
            Cursor = Cursors.Default;

        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var device = new MiDevice(textBoxIpAddress.Text, textBox2.Text);
                var response = device.Send(JsonConvert.DeserializeObject(textBox3.Text));
                e.Result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(response.ResponseText), Formatting.Indented);
            } catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void textBoxIpAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbx = sender as ComboBox;
            if (cbx.SelectedIndex != -1)
            {
                var template = templates[cbx.SelectedItem.ToString()];
                textBox3.Text = template;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            textBox3.Enabled = false;
            textBox2.Enabled = false;
            comboBox1.Enabled = false;
            textBoxIpAddress.Enabled = false;
            Cursor = Cursors.WaitCursor;
            bgWorker.RunWorkerAsync(JsonConvert.DeserializeObject(textBox3.Text));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }


}
