using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.Json;
using System.Windows.Forms;


namespace csbattleship
{
    public partial class Form1 : Form
    {
        // Настройки цвета
        private readonly Color _borderColor = Color.FromArgb(14, 40, 120);
        private readonly Color _seaColor = Color.FromArgb(15, 45, 135);
        private readonly Color _shipColor = Color.FromArgb(20, 20, 20);
        private readonly Color _missColor = Color.FromArgb(40, 60, 130);
        private readonly Color _hitColor = Color.FromArgb(105, 15, 0);

        private bool _isClient = true;
        private int _gameStatus = 0;

        public string host = "127.0.0.1";
        public int port = 7070;

        // Ячейки кораблей
        private const int totalParts = 20;
        private int currentParts = 0;
        private int aliveParts = totalParts;
        private bool vertical;
        private bool horizontal;

        private bool myTurn;
        private bool endOfGame = false;

        private string coordsToSend = "";
        public string messageToSend = "";
        private string commandToSend = "";

        private bool _imReadyForBattle = false;
        private bool _enemyReadyForBattle = false;

        private Dictionary<string, Button> _leftCellField = new();
        private Dictionary<string, Button> _rigthCellField = new();
        private Stack<Button> _shipCoords = new(totalParts);


        public Form1()
        {
            Program.f = this;   // для использования в NetworkClass

            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            tableLayoutPanelLeft.BackColor = _borderColor;
            tableLayoutPanelRigth.BackColor = _borderColor;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Button leftCell = new();
                    Button rigthCell = new();

                    leftCell.Click += new EventHandler(LeftCellField_Click);
                    rigthCell.Click += new EventHandler(RigthCellField_Click);

                    leftCell.FlatAppearance.BorderSize = 0;
                    rigthCell.FlatAppearance.BorderSize = 0;

                    Font font = new("Perpetua", 1);
                    leftCell.Font = font;
                    rigthCell.Font = font;

                    leftCell.Name = $"L{i}{j}";
                    rigthCell.Name = $"R{i}{j}";

                    leftCell.Dock = DockStyle.Fill;
                    rigthCell.Dock = DockStyle.Fill;

                    leftCell.FlatStyle = FlatStyle.Flat;
                    rigthCell.FlatStyle = FlatStyle.Flat;

                    SetCellStatus(leftCell, 0);
                    SetCellStatus(rigthCell, 0);

                    _leftCellField.Add($"{i}{j}", leftCell);
                    _rigthCellField.Add($"{i}{j}", rigthCell);

                    tableLayoutPanelLeft.Controls.Add(leftCell, i, j);
                    tableLayoutPanelRigth.Controls.Add(rigthCell, i, j);
                }
            }

            buttonClear.Enabled = false;
            tableLayoutPanelLeft.Enabled = false;
            tableLayoutPanelRigth.Enabled = false;
            tableLayoutPanelMessage.Enabled = false;
        }

        void StartGame(object sender, EventArgs e)
        {
            if (_gameStatus == 0 && CheckConnect())
            {
                textBoxHost.Enabled = textBoxPort.Enabled = false;
                radioButtonClient.Enabled = radioButtonServer.Enabled = false;
                buttonStart.Enabled = false;

                if (_isClient)
                {
                    myTurn = false;
                    ChangeTurnStatus();
                    NetworkClass.Client();
                }
                else
                {
                    myTurn = true;
                    ChangeTurnStatus();
                    NetworkClass.Server();
                }
            }
            else if (_gameStatus == 1 && CheckBattle())
            {
                _imReadyForBattle = true;
                buttonStart.Enabled = false;
                if (_enemyReadyForBattle == false)
                    SendMessage("Ожидание соперника.");

                if (_isClient)
                    commandToSend = "client ready";
                else
                    commandToSend = "server ready";
            }
        }

        bool CheckConnect()
        {
            if (textBoxHost.Text == "")
                textBoxHost.Text = textBoxHost.PlaceholderText;
            if (textBoxPort.Text == "")
                textBoxPort.Text = textBoxPort.PlaceholderText;

            IPAddress ip;
            IPAddress.TryParse(textBoxHost.Text, out ip);

            bool fail = false;
            if (ip is null)
            {
                SendMessage("Не верно указан IP адресс.");
                fail = true;
            }
            else if (int.TryParse(textBoxPort.Text, out _) == false)
            {
                SendMessage("Не верно указан порт.");
                fail = true;
            }
            else if (Convert.ToInt32(textBoxPort.Text) < 1000 || Convert.ToInt32(textBoxPort.Text) > Math.Pow(2, 16))
            {
                SendMessage("Не верно указан порт.");
                fail = true;
            }

            if (fail)
            {
                if (textBoxHost.Text == textBoxHost.PlaceholderText)
                    textBoxHost.Text = "";
                if (textBoxPort.Text == textBoxPort.PlaceholderText)
                    textBoxPort.Text = "";

                return false;
            }

            host = textBoxHost.Text;
            port = Convert.ToInt32(textBoxPort.Text);
            return true;
        }

        bool CheckBattle()
        {
            if (currentParts != totalParts)
            {
                SendMessage("Не все корабли расставлены.");
                return false;
            }

            return true;
        }

        public string GetNetDataToSend()
        {
            if (endOfGame)
            {
                commandToSend = "end of game";
                endOfGame = false;
            }

            List<string> comboData = new() { coordsToSend, messageToSend, commandToSend };
            coordsToSend = commandToSend = "";

            return JsonSerializer.Serialize(comboData);
        }

        public void ViewReceivedNetData(string data)
        {
            try
            {
                Action action = () =>
                {
                    List<string> comboData = JsonSerializer.Deserialize<List<string>>(data);

                    if (comboData[0] != "")
                    {
                        processEnemyAction(comboData[0]);
                    }

                    if (comboData[1] != "")
                    {
                        SendMessage(comboData[1], "enemy");
                    }

                    if (comboData[2] != "")
                    {
                        if (comboData[2] == "end of game")
                        {
                            SendMessage("Победа");
                            SetGameStatus(1);
                            return;
                        }
                        else if (comboData[2] == "client ready" || comboData[2] == "server ready")
                        {
                            _enemyReadyForBattle = true;
                            SendMessage("Соперник готов к бою.");
                        }
                        else
                        {
                            if (comboData[2][2..] == "hit")
                            {
                                Button cell = _rigthCellField[comboData[2].Substring(0, 2)];
                                SetCellStatus(cell, 3);
                            }
                            else if (comboData[2][2..] == "miss")
                            {
                                myTurn = !myTurn;
                                ChangeTurnStatus();
                                Button cell = _rigthCellField[comboData[2].Substring(0, 2)];
                                SetCellStatus(cell, 2);
                            }
                        }
                    }

                    if (_imReadyForBattle && _enemyReadyForBattle)
                    {
                        commandToSend = "special code for start battle";
                        SetGameStatus(2);

                        _imReadyForBattle = _enemyReadyForBattle = false;
                    }
                };

                if (InvokeRequired)
                    Invoke(action);
                else
                    action();
            }
            catch (JsonException)
            {
                SendMessage("Ошибка: слишком много сообщений");
            }
        }

        void processEnemyAction(string coords)
        {
            Button cell = _leftCellField[coords];
            if (cell.Text == "1")
            {
                SetCellStatus(cell, 3);
                commandToSend = $"{coords}hit";
                aliveParts -= 1;

                if (aliveParts == 0)
                {
                    SendMessage("Поражение");
                    SetGameStatus(1);
                    endOfGame = true;
                }
            }
            else if (cell.Text == "0")
            {
                SetCellStatus(cell, 2);
                commandToSend = $"{coords}miss";
                myTurn = !myTurn;
                ChangeTurnStatus();
            }
        }

        public void SetGameStatus(int newGameStatus)
        {
            Action action = () =>
            {
                if (newGameStatus == 0)
                {
                    ClearMap();

                    _gameStatus = 0;
                    if (textBoxHost.Text == textBoxHost.PlaceholderText)
                        textBoxHost.Text = "";
                    if (textBoxPort.Text == textBoxPort.PlaceholderText)
                        textBoxPort.Text = "";
                    textBoxHost.Enabled = textBoxPort.Enabled = true;
                    radioButtonClient.Enabled = radioButtonServer.Enabled = true;
                    buttonClear.Enabled = false;
                    tableLayoutPanelLeft.Enabled = false;
                    tableLayoutPanelRigth.Enabled = false;
                    tableLayoutPanelMessage.Enabled = false;
                    turnStatus.Visible = false;
                    buttonSurrender.Visible = false;
                    buttonStart.Text = "Подключиться";
                    buttonStart.Enabled = true;
                    _imReadyForBattle = false;
                    _enemyReadyForBattle = false;
                }
                else if (newGameStatus == 1)
                {
                    ClearMap();

                    _gameStatus = 1;
                    buttonClear.Enabled = true;
                    tableLayoutPanelLeft.Enabled = true;
                    tableLayoutPanelRigth.Enabled = false;
                    tableLayoutPanelMessage.Enabled = true;
                    turnStatus.Visible = true;
                    buttonSurrender.Visible = true;
                    buttonStart.Text = "Готов";
                    _imReadyForBattle = false;
                    _enemyReadyForBattle = false;
                    buttonStart.Enabled = true;
                    aliveParts = 20;
                    SendMessage("Подготовка к бою.");
                }
                else if (newGameStatus == 2)
                {
                    _gameStatus = 2;
                    tableLayoutPanelLeft.Enabled = buttonClear.Enabled = buttonStart.Enabled = false;
                    tableLayoutPanelRigth.Enabled = true;
                    turnStatus.Visible = true;
                    buttonSurrender.Visible = true;
                    endOfGame = false;
                    aliveParts = 20;
                    SendMessage("Бой начинается.");
                }
            };

            if (InvokeRequired)
                Invoke(action);
            else
                action();

        }

        void LeftCellField_Click(object sender, EventArgs e)
        {
            if (_gameStatus == 1)
            {
                Button cell = (Button)sender;

                if (cell.Text == "1" && cell == _shipCoords.Peek())
                {
                    _shipCoords.Pop();
                    SetCellStatus(cell, 0);
                    currentParts -= 1;
                }
                else if (cell.Text == "0" && currentParts < totalParts)
                {
                    if (CanSetShip(cell))
                    {
                        SetCellStatus(cell, 1);
                        currentParts += 1;
                    }
                }
            }
        }

        void RigthCellField_Click(object sender, EventArgs e)
        {
            Button cell = (Button)sender;
            if (_gameStatus == 2 && cell.Text == "0" && myTurn)
            {
                coordsToSend = $"{cell.Name[1..]}";
            }
        }

        bool CanSetShip(Button cell)
        {
            if (IsCellClear(cell, -1, -1) && IsCellClear(cell, -1, +1) && IsCellClear(cell, +1, -1) && IsCellClear(cell, +1, +1))
            {
                if ((currentParts == 5 || currentParts == 6 || currentParts == 8 || currentParts == 9
                     || currentParts == 11 || currentParts == 13 || currentParts == 15) && IsNearLastCoords(cell) == false)
                {
                    return false;
                }

                if ((currentParts == 0 || currentParts == 4 || currentParts == 7 || currentParts == 10 ||
                    currentParts == 12 || currentParts == 14 || currentParts >= 16) &&
                    (IsCellClear(cell, -1, 0) && IsCellClear(cell, 0, -1) && IsCellClear(cell, 0, +1) && IsCellClear(cell, +1, 0)))
                {
                    horizontal = false;
                    vertical = false;

                    _shipCoords.Push(cell);
                    return true;
                }

                else if (currentParts == 1 || currentParts == 5 || currentParts == 8)
                {
                    if (IsCellClear(cell, -1, 0) == false || IsCellClear(cell, +1, 0) == false)
                    {
                        horizontal = true;
                        vertical = false;

                        _shipCoords.Push(cell);
                        return true;
                    }
                    else if (IsCellClear(cell, 0, -1) == false || IsCellClear(cell, 0, +1) == false)
                    {
                        vertical = true;
                        horizontal = false;

                        _shipCoords.Push(cell);
                        return true;
                    }
                }

                else if ((currentParts == 2 || currentParts == 3 || currentParts == 6 || currentParts == 9) &&
                    ((horizontal && (IsCellClear(cell, -1, 0) == false || IsCellClear(cell, +1, 0) == false)) ||
                        (vertical && (IsCellClear(cell, 0, +1) == false || IsCellClear(cell, 0, -1) == false))))
                {
                    _shipCoords.Push(cell);
                    return true;
                }

                else if ((currentParts == 11 || currentParts == 13 || currentParts == 15) &&
                    (IsCellClear(cell, -1, 0) == false || IsCellClear(cell, +1, 0) == false || IsCellClear(cell, 0, -1) == false || IsCellClear(cell, 0, +1) == false))
                {
                    _shipCoords.Push(cell);
                    return true;
                }
            }

            return false;
        }

        bool IsNearLastCoords(Button cell)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    try
                    {
                        string iter_coords = $"{Convert.ToInt32(cell.Name[1].ToString()) + i}{Convert.ToInt32(cell.Name[2].ToString()) + j}";

                        if (_leftCellField[iter_coords].Text == "1" && _shipCoords.Peek().Name[1..] == iter_coords)
                        {
                            return true;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }

            return false;
        }

        void SetCellStatus(Button cell, int status)
        {
            // Левое поле
            if (status == 0)    // пусто
            {
                cell.BackColor = _seaColor;
                cell.Text = "0";
            }
            else if (status == 1)   // целый
            {
                cell.BackColor = _shipColor;
                cell.Text = "1";
            }
            // Правое поле
            else if (status == 2)   // промах
            {
                cell.BackColor = _missColor;
                cell.Text = "2";
            }
            else if (status == 3)   // попадание
            {
                cell.BackColor = _hitColor;
                cell.Text = "3";
            }
        }

        bool IsCellClear(Button cell, int dx, int dy)
        {
            try
            {
                if (_leftCellField[$"{Convert.ToInt32(cell.Name[1].ToString()) + dx}{Convert.ToInt32(cell.Name[2].ToString()) + dy}"].Text == "0")
                {
                    return true;
                }
                return false;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
        }

        void ButtonSend_Click(object sender, EventArgs e)
        {
            string text = textBoxInput.Text.Trim();
            if (text != "")
            {
                textBoxInput.Clear();
                messageToSend = text;
            }

        }

        public void SendMessage(string text, string from = "#")
        {
            Action action = () =>
            {
                text = text.Trim();
                if (text != "")
                {
                    listBoxChat.Items.Add($"{from}: {text}");
                    listBoxChat.TopIndex = listBoxChat.Items.Count - 1;
                }
            };

            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        void ButtonClear_Click(object sender, EventArgs e)
        {
            if (_gameStatus == 1)
            {
                ClearMap();
            }
        }

        void ClearMap()
        {
            foreach (Button cell in tableLayoutPanelLeft.Controls)
            {
                SetCellStatus(cell, 0);
            }

            foreach (Button cell in tableLayoutPanelRigth.Controls)
            {
                SetCellStatus(cell, 0);
            }

            currentParts = 0;
            _shipCoords.Clear();
        }

        void radioButtonClient_CheckedChanged(object sender, EventArgs e)
        {
            _isClient = !_isClient;
            if (_isClient)
                textBoxHost.PlaceholderText = "127.0.0.1";
            else
                textBoxHost.PlaceholderText = "0.0.0.0";
        }

        void ChangeTurnStatus()
        {
            if (myTurn)
            {
                turnStatus.Image = Properties.Resources.green;
            }
            else
            {
                turnStatus.Image = Properties.Resources.red;
            }

        }

        private void buttonSurrender_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
            "Вы действительно хотите сдаться?",
            "Сообщение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.Yes)
            {
                SendMessage("Поражение");
                SetGameStatus(1);
                endOfGame = true;
            }

        }
    }

}
