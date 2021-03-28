using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csbattleship
{
    public partial class MainForm : Form
    {
        public Color sea_color = Color.FromArgb(0, 0, 128);
        public Color ship_color = Color.FromArgb(19, 19, 19);
        public string host = "";
        public string port = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tableLayoutPanelLeft.BackColor = sea_color;
            tableLayoutPanelRigth.BackColor = sea_color;

            for (int i = 0; i < 10; i++)
            {

                for (int j = 0; j < 10; j++)
                {
                    Button leftbutton = new();
                    Button rigthButton = new();

                    leftbutton.Name = rigthButton.Name = $"leftButton_{i}{j}";

                    leftbutton.Dock = DockStyle.Fill;
                    rigthButton.Dock = DockStyle.Fill;

                    leftbutton.FlatStyle = FlatStyle.Flat;
                    rigthButton.FlatStyle = FlatStyle.Flat;

                    leftbutton.BackColor = ship_color;
                    rigthButton.BackColor = ship_color;

                    leftbutton.ForeColor = ship_color;
                    rigthButton.ForeColor = ship_color;

                    tableLayoutPanelLeft.Controls.Add(leftbutton, i, j);
                    tableLayoutPanelRigth.Controls.Add(rigthButton, i, j);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                tableLayoutPanelLeft.ColumnStyles[i].SizeType = SizeType.Percent;
                tableLayoutPanelRigth.ColumnStyles[i].SizeType = SizeType.Percent;

                tableLayoutPanelLeft.RowStyles[i].SizeType = SizeType.Percent;
                tableLayoutPanelRigth.RowStyles[i].SizeType = SizeType.Percent;

                tableLayoutPanelLeft.ColumnStyles[i].Width = 10;
                tableLayoutPanelRigth.ColumnStyles[i].Width = 10;

                tableLayoutPanelLeft.RowStyles[i].Height = 10;
                tableLayoutPanelRigth.RowStyles[i].Height = 10;
            }
        }

    }
}
