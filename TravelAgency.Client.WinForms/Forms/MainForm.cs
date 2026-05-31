using System.Data;
using System.Text.Json;
using System.Windows.Forms;
using TravelAgency.Client.WinForms.Network;
using TravelAgency.Core.Entities;

namespace TravelAgency.Client.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly TcpClientService _tcpService;
    private readonly string _username;
    private Label lblUser;
    private Button btnRefresh;
    private Button btnAdd;
    private Button btnSearch;
    private TextBox txtFilterCountry;
    private DataGridView dgvTours;
    private Button btnLogout;
    private Label lblStatus;

    public MainForm(TcpClientService tcpService, string username)
    {
        InitializeComponent();
        _tcpService = tcpService;
        _username = username;
        SetupDataGridView();
        lblUser.Text = $"Пользователь: {username}";
    }

    private void SetupDataGridView()
    {
        dgvTours.AutoGenerateColumns = false;
        dgvTours.Columns.Clear();

        // Создаём колонки, явно привязанные к свойствам Tour
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.Id),
            HeaderText = "ID",
            Width = 40,
            ReadOnly = true
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.Name),
            HeaderText = "Название",
            Width = 150
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.Country),
            HeaderText = "Страна",
            Width = 100
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.City),
            HeaderText = "Город",
            Width = 100
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.Price),
            HeaderText = "Цена",
            Width = 80
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.StartDate),
            HeaderText = "Дата",
            Width = 100
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.Tour.AvailableSeats),
            HeaderText = "Места",
            Width = 60
        });
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        await LoadToursAsync();
    }

    private async Task LoadToursAsync()
    {
        if (_tcpService.CurrentToken == null) return;

        try
        {
            var response = await _tcpService.SendCommandAsync($"GET_TOURS_ORM|{_tcpService.CurrentToken}");

            if (response.StartsWith("OK|"))
            {
                var json = response.Substring(3);
                var tours = JsonSerializer.Deserialize<List<Core.Entities.Tour>>(json) ?? new();

                // ✅ Прямая привязка списка к DataGridView
                dgvTours.DataSource = tours;

                lblStatus.Text = $"✅ Загружено туров: {tours.Count}";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = $"❌ Ошибка сервера: {response}";
                lblStatus.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ Ошибка: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await LoadToursAsync();
    }

    private async void btnAdd_Click(object sender, EventArgs e)
    {
        var addForm = new AddTourForm(_tcpService.CurrentToken!, _tcpService);

        if (addForm.ShowDialog() == DialogResult.OK)
        {
            await LoadToursAsync(); // Обновляем таблицу после успешного добавления
            lblStatus.Text = "..."; 
            lblStatus.ForeColor = Color.Green;
        }
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        string country = txtFilterCountry.Text.Trim();

        // Если фильтр пустой — загружаем все туры
        if (string.IsNullOrEmpty(country))
        {
            await LoadToursAsync();
            return;
        }

        try
        {
            // 1. Получаем все туры с сервера
            var response = await _tcpService.SendCommandAsync($"GET_TOURS_ORM|{_tcpService.CurrentToken}");

            if (response.StartsWith("OK|"))
            {
                var json = response.Substring(3);
                var allTours = JsonSerializer.Deserialize<List<Core.Entities.Tour>>(json) ?? new();

                // 2. Фильтруем на клиенте
                var filtered = allTours
                    .Where(t => t.Country.Contains(country, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // 3. Привязываем отфильтрованный список напрямую к DataGridView
                dgvTours.DataSource = null; // Сброс для корректного обновления
                dgvTours.DataSource = filtered;

                lblStatus.Text = $"🔍 Найдено туров по стране '{country}': {filtered.Count}";
                lblStatus.ForeColor = filtered.Count > 0 ? Color.Green : Color.Orange;
            }
            else
            {
                lblStatus.Text = $"❌ Ошибка: {response}";
                lblStatus.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ Ошибка поиска: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }
    

    private void InitializeComponent()
    {
        lblUser = new Label();
        btnRefresh = new Button();
        btnAdd = new Button();
        btnSearch = new Button();
        txtFilterCountry = new TextBox();
        dgvTours = new DataGridView();
        btnLogout = new Button();
        lblStatus = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvTours).BeginInit();
        SuspendLayout();
        // 
        // lblUser
        // 
        lblUser.AutoSize = true;
        lblUser.Location = new Point(654, 52);
        lblUser.Name = "lblUser";
        lblUser.Size = new Size(139, 20);
        lblUser.TabIndex = 0;
        lblUser.Text = "Имя пользователя";
        lblUser.Click += label1_Click;
        // 
        // btnRefresh
        // 
        btnRefresh.Location = new Point(68, 12);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(147, 60);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "Обновить список туров";
        btnRefresh.UseVisualStyleBackColor = true;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(68, 107);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(147, 60);
        btnAdd.TabIndex = 2;
        btnAdd.Text = "Добавить тур";
        btnAdd.UseVisualStyleBackColor = true;
        // 
        // btnSearch
        // 
        btnSearch.Location = new Point(68, 205);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(147, 60);
        btnSearch.TabIndex = 3;
        btnSearch.Text = "Поиск по стране";
        btnSearch.UseVisualStyleBackColor = true;
        // 
        // txtFilterCountry
        // 
        txtFilterCountry.Location = new Point(68, 320);
        txtFilterCountry.Name = "txtFilterCountry";
        txtFilterCountry.Size = new Size(125, 27);
        txtFilterCountry.TabIndex = 4;
        // 
        // dgvTours
        // 
        dgvTours.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvTours.Location = new Point(525, 178);
        dgvTours.Name = "dgvTours";
        dgvTours.RowHeadersWidth = 51;
        dgvTours.Size = new Size(300, 188);
        dgvTours.TabIndex = 5;
        // 
        // btnLogout
        // 
        btnLogout.Location = new Point(68, 408);
        btnLogout.Name = "btnLogout";
        btnLogout.Size = new Size(147, 60);
        btnLogout.TabIndex = 6;
        btnLogout.Text = "Выйти из системы";
        btnLogout.UseVisualStyleBackColor = true;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(872, 52);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(164, 20);
        lblStatus.TabIndex = 7;
        lblStatus.Text = "Статусные сообщения";
        // 
        // MainForm
        // 
        ClientSize = new Size(1062, 516);
        Controls.Add(lblStatus);
        Controls.Add(btnLogout);
        Controls.Add(dgvTours);
        Controls.Add(txtFilterCountry);
        Controls.Add(btnSearch);
        Controls.Add(btnAdd);
        Controls.Add(btnRefresh);
        Controls.Add(lblUser);
        Name = "MainForm";
        ((System.ComponentModel.ISupportInitialize)dgvTours).EndInit();
        ResumeLayout(false);
        PerformLayout();
        // === Улучшенная компоновка элементов ===

        // lblUser - в правом верхнем углу
        lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblUser.Location = new Point(850, 20);
        lblUser.TextAlign = ContentAlignment.MiddleRight;
        lblUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        // lblStatus - под lblUser
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblStatus.Location = new Point(850, 45);
        lblStatus.MaximumSize = new Size(200, 0);
        lblStatus.ForeColor = Color.Red;

        // Панель кнопок слева
        var panelButtons = new Panel
        {
            Dock = DockStyle.Left,
            Width = 200,
            Padding = new Padding(10)
        };
        panelButtons.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnSearch, txtFilterCountry, btnLogout });

        // DataGridView - занимает остальное место
        dgvTours.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvTours.Location = new Point(220, 10);
        dgvTours.Size = new Size(820, 480);
        dgvTours.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTours.RowHeadersVisible = false;
        dgvTours.AllowUserToAddRows = false;
        dgvTours.AllowUserToDeleteRows = false;
        dgvTours.ReadOnly = true;

        // Расположение кнопок в панели
        btnRefresh.Dock = DockStyle.Top;
        btnRefresh.Height = 50;
        btnAdd.Dock = DockStyle.Top;
        btnAdd.Height = 50;
        btnSearch.Dock = DockStyle.Top;
        btnSearch.Height = 50;
        txtFilterCountry.Dock = DockStyle.Top;
        txtFilterCountry.Height = 30;
        txtFilterCountry.PlaceholderText = "Страна...";
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Height = 50;
        btnLogout.BackColor = Color.FromArgb(220, 50, 50);
        btnLogout.ForeColor = Color.White;
        btnLogout.FlatStyle = FlatStyle.Flat;

        // Добавляем панель на форму
        Controls.Add(panelButtons);
        Controls.SetChildIndex(panelButtons, 0); // Панель поверх других

        // Обработчики событий кнопок
        btnRefresh.Click += btnRefresh_Click;
        btnAdd.Click += btnAdd_Click;
        btnSearch.Click += btnSearch_Click;
        btnLogout.Click += btnLogout_Click;
    }

    private void btnLogout_Click(object sender, EventArgs e)
    {
        _tcpService.Disconnect();
        this.Close();
        var loginForm = new LoginForm();
        loginForm.Show();
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
}