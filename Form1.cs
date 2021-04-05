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
        private readonly Color sea_color = Color.FromArgb(0, 0, 128);
        private readonly Color ship_color = Color.FromArgb(19, 19, 19);
        string host = "";
        string port = "";

        int gameStatus = 0;  // 0: построение, 1: бой

        // Ячейки кораблей
        int currentParts = 0;
        readonly int totalParts = 20;

        //public Button[,] leftCellMassive = new Button[10, 10];
        //public Button[,] rigthCellMassive = new Button[10, 10];

        public MainForm()
        {
            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            MapCleaning();
        }

        void StartGame(object sender, EventArgs e)
        {
            if (Validvalidation())
            {
                if (this.host != textBoxHost.Text || this.port != textBoxPort.Text)
                {
                    this.host = textBoxHost.Text;
                    this.port = textBoxPort.Text;

                    SendMessage($"Подключение к: {this.host}:{this.port}");

                    this.gameStatus = 1;
                }
            }
            else
            {
                SendMessage("Не верный IP адресс!");
            }
        }

        bool Validvalidation()
        {
            IPAddress ip;
            IPAddress.TryParse(textBoxHost.Text, out ip);

            if (textBoxPort.Text == "" || ip == null)
                return false;

            return true;
        }

        void MapCleaning()
        {
            tableLayoutPanelLeft.BackColor = this.sea_color;
            tableLayoutPanelRigth.BackColor = this.sea_color;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Button leftCell = new();
                    Button rigthCell = new();

                    leftCell.Click += new EventHandler(LeftCellMassive_Click);
                    rigthCell.Click += new EventHandler(RigthCellMassive_Click);

                    Font font = new("Perpetua", 1);
                    leftCell.Font = font;
                    rigthCell.Font = font;

                    leftCell.Name = $"L{i}{j}";
                    rigthCell.Name = $"R{i}{j}";

                    leftCell.Dock = DockStyle.Fill;
                    rigthCell.Dock = DockStyle.Fill;

                    leftCell.FlatStyle = FlatStyle.Flat;
                    rigthCell.FlatStyle = FlatStyle.Flat;

                    ChangeCellStatus(leftCell, 0);
                    ChangeCellStatus(rigthCell, 0);

                    //leftCellMassive[i, j] = leftCell;
                    //rigthCellMassive[i, j] = rigthCell;

                    tableLayoutPanelLeft.Controls.Add(leftCell, i, j);
                    tableLayoutPanelRigth.Controls.Add(rigthCell, i, j);
                }
            }
        }

        void LeftCellMassive_Click(object sender, EventArgs e)
        {
            //Button pressed_cell = new();
            //for (int i = 0; i < 10; i++)
            //    for (int j = 0; j < 10; j++)
            //        if ((Button)sender == leftCellMassive[i, j])
            //            pressed_cell = leftCellMassive[i, j];

            Button cell = (Button)sender;

            string name = cell.Name;

            if (this.gameStatus == 0)
            {
                if (cell.Text == "1")
                {
                    ChangeCellStatus(cell, 0);
                    this.currentParts -= 1;
                    SendMessage($"Слева убран {name}");
                }
                else if (cell.Text == "0" && this.currentParts <= this.totalParts)
                {
                    ChangeCellStatus(cell, 1);
                    this.currentParts += 1;
                    SendMessage($"Слева выбран {name}");
                }
            }

        }

        void RigthCellMassive_Click(object sender, EventArgs e)
        {
            string name = ((Button)sender).Name;

            SendMessage(name);
        }

        void ChangeCellStatus(Button cell, int status)
        {
            if (status == -1)
            {
                cell.BackColor = cell.ForeColor = Color.FromArgb(0, 0, 0);
                cell.Text = "-1";
            }    
            else if (status == 0)
            {
                cell.BackColor = this.sea_color;
                cell.Text = "0";
            }
            else if (status == 1)
            {
                cell.BackColor = cell.ForeColor = this.ship_color;
                cell.Text = "1";
            }
        }

        void ButtonSend_Click(object sender, EventArgs e)
        {
            string text = textBoxInput.Text;
            if (text != "")
            {
                textBoxInput.Clear();
                SendMessage(text, "player");
            }

        }

        void SendMessage(string text, string from = "sys")
        {
            if (text != "" && from == "sys" || from == "player")
            {
                listBoxChat.Items.Add($"{from}: {text}");
            }
        }

        void ButtonClear_Click(object sender, EventArgs e)
        {
            if (this.gameStatus == 0)
            {
                foreach (Button cell in tableLayoutPanelLeft.Controls)
                {
                    ChangeCellStatus(cell, 0);
                }

                this.currentParts = 0;
            }
        }
    }
}
