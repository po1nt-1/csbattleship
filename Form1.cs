using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace csbattleship
{
    public partial class MainForm : Form
    {
        public Color sea_color = Color.FromArgb(0, 0, 128);
        public Color ship_color = Color.FromArgb(19, 19, 19);
        public string host = "";
        public string port = "";

        public Button[,] leftCellMassive = new Button[10, 10];
        public Button[,] rigthCellMassive = new Button[10, 10];

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MapCleaning();
        }

        private void StartGame(object sender, EventArgs e)
        {
            if (Validvalidation())
            {
                if (host != textBoxHost.Text || port != textBoxPort.Text)
                {
                    host = textBoxHost.Text;
                    port = textBoxPort.Text;

                    listBoxChat.Items.Add($"Подключение к: {host}:{port}");
                }
            }
            else
            {
                listBoxChat.Items.Add("Не верный IP адресс!");
            }
        }

        private bool Validvalidation()
        {
            IPAddress ip;
            IPAddress.TryParse(textBoxHost.Text, out ip);

            if (textBoxPort.Text == "" || ip == null)
                return false;

            return true;
        }

        private void MapCleaning()
        {
            tableLayoutPanelLeft.BackColor = sea_color;
            tableLayoutPanelRigth.BackColor = sea_color;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                    {
                        Button leftCell = new();
                        Button rigthCell = new();
                        
                        Font font = new("Perpetua", 1);
                        leftCell.Font = font;
                        rigthCell.Font = font;
                        
                        leftCell.Name = $"lefCell_{i}{j}";
                        rigthCell.Name = $"rigthCell_{i}{j}";
                        
                        leftCell.Dock = DockStyle.Fill;
                        rigthCell.Dock = DockStyle.Fill;
                        
                        leftCell.FlatStyle = FlatStyle.Flat;
                        rigthCell.FlatStyle = FlatStyle.Flat;
                        
                        ChangeCellStatus(leftCell, 0);
                        ChangeCellStatus(rigthCell, 0);
                        
                        leftCellMassive[i, j] = leftCell;
                        rigthCellMassive[i, j] = rigthCell;
                        
                        tableLayoutPanelLeft.Controls.Add(leftCell, i, j);
                        tableLayoutPanelRigth.Controls.Add(rigthCell, i, j);
                    }
            }

        }

        private void ChangeCellStatus(Button cell, int status)
        {
            if (status == -1)
            {
                cell.BackColor = cell.ForeColor = Color.FromArgb(0, 0, 0);
                cell.Text = "-1";
            }    
            else if (status == 0)
            {
                cell.BackColor = sea_color;
                cell.Text = "0";
            }
            else if (status == 1)
            {
                cell.BackColor = cell.ForeColor = ship_color;
                cell.Text = "1";
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string text = textBoxInput.Text;
            if (text != "")
            {
                listBoxChat.Items.Add(text);
                textBoxInput.Clear();
            }

        }
    }
}
