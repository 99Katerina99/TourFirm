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

    // Боковая панель
    private Panel sidePanel;
    private Button btnTours;
    private Button btnAddTour;
    private Button btnDeleteTour;
    private Button btnSupplierContracts;
    private Button btnLogout;

    // Основная область
    private Panel topPanel;
    private TextBox txtFilter;
    private Button btnSearch;
    private DataGridView dgvTours;
    private Label lblStatus;
    private Label lblUser;

    public MainForm(TcpClientService tcpService, string username)
    {
        InitializeComponent();
        _tcpService = tcpService;
        _username = username;
        SetupDataGridView();
        lblUser.Text = $" {username}";
    }

    private void SetupDataGridView()
    {
        dgvTours.AutoGenerateColumns = false;
        dgvTours.Columns.Clear();

        // Основные данные тура
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.TourId),
            HeaderText = "ID",
            Width = 40,
            ReadOnly = true
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.TourName),
            HeaderText = "Название",
            Width = 150
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.TourType),
            HeaderText = "Тип",
            Width = 100
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.Destination),
            HeaderText = "Направление Маршрута",
            Width = 120
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.StartDate),
            HeaderText = "Начало",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.EndDate),
            HeaderText = "Окончание",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.Price),
            HeaderText = "Цена",
            Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.MaxSeats),
            HeaderText = "Мест",
            Width = 50
        });

        // Клиент
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.ClientName),
            HeaderText = "Клиент",
            Width = 120
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.ClientPhone),
            HeaderText = "Телефон",
            Width = 100
        });

        // Договор с клиентом
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.ClientContractNumber),
            HeaderText = "Договор",
            Width = 110
        });

        // Поставщик
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.SupplierName),
            HeaderText = "Поставщик",
            Width = 120
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.SupplierContractNumber),
            HeaderText = "Дог. поставщика",
            Width = 120
        });

        // Бронирование
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.BookingNumber),
            HeaderText = "Бронь",
            Width = 110
        });

        // Платёж
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.PaymentMethod),
            HeaderText = "Оплата",
            Width = 100
        });
        dgvTours.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Core.Entities.TourExtendedDto.PaymentStatus),
            HeaderText = "Статус",
            Width = 100
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
            var response = await _tcpService.SendCommandAsync($"GET_TOURS_EXTENDED|{_tcpService.CurrentToken}");

            if (response.StartsWith("OK|"))
            {
                var json = response.Substring(3);
                var tours = JsonSerializer.Deserialize<List<Core.Entities.TourExtendedDto>>(json) ?? new();

                dgvTours.DataSource = tours;

                lblStatus.Text = $" Загружено туров: {tours.Count}";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = $" Ошибка: {response}";
                lblStatus.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $" Ошибка: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }

    private async void btnAddTour_Click(object sender, EventArgs e)
    {
        var addForm = new AddTourForm(_tcpService.CurrentToken!, _tcpService);

        if (addForm.ShowDialog() == DialogResult.OK)
        {
            await LoadToursAsync();
            lblStatus.Text = " Тур добавлен";
            lblStatus.ForeColor = Color.Green;
        }
    }

    private async void btnDeleteTour_Click(object sender, EventArgs e)
    {
        if (dgvTours.CurrentRow == null)
        {
            lblStatus.Text = " Выберите тур";
            lblStatus.ForeColor = Color.Red;
            return;
        }

        var selectedTour = dgvTours.CurrentRow.DataBoundItem as Core.Entities.TourExtendedDto;
        if (selectedTour == null)
        {
            lblStatus.Text = " Ошибка выбора";
            lblStatus.ForeColor = Color.Red;
            return;
        }

        var confirm = MessageBox.Show(
            $"Удалить тур \"{selectedTour.TourName}\" и все связанные данные?",
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            var response = await _tcpService.SendCommandAsync(
                $"DELETE_TOUR|{_tcpService.CurrentToken}|{selectedTour.TourId}");

            if (response.StartsWith("OK|"))
            {
                lblStatus.Text = $"Удалено";
                lblStatus.ForeColor = Color.Green;
                await LoadToursAsync();
            }
            else
            {
                lblStatus.Text = $" {response}";
                lblStatus.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Ошибка: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        string searchText = txtFilter.Text.Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            await LoadToursAsync();
            return;
        }

        try
        {
            var response = await _tcpService.SendCommandAsync($"GET_TOURS_EXTENDED|{_tcpService.CurrentToken}");

            if (response.StartsWith("OK|"))
            {
                var json = response.Substring(3);
                var allTours = JsonSerializer.Deserialize<List<Core.Entities.TourExtendedDto>>(json) ?? new();

                var filtered = allTours
                    .Where(t =>
                        (t.TourName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.TourType?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Destination?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.ClientName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.SupplierName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

                dgvTours.DataSource = null;
                dgvTours.DataSource = filtered;

                lblStatus.Text = $"Найдено: {filtered.Count}";
                lblStatus.ForeColor = filtered.Count > 0 ? Color.Green : Color.Orange;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Ошибка: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }

    private void btnSupplierContracts_Click(object sender, EventArgs e)
    {
        var form = new SupplierContractsForm(_tcpService.CurrentToken!, _tcpService);
        form.ShowDialog();
    }

    private void btnLogout_Click(object sender, EventArgs e)
    {
        _tcpService.Disconnect();
        this.Close();
        var loginForm = new LoginForm();
        loginForm.Show();
    }

    private void InitializeComponent()
    {
        // Создаём элементы
        sidePanel = new Panel();
        topPanel = new Panel();
        lblUser = new Label();
        lblStatus = new Label();
        txtFilter = new TextBox();
        btnSearch = new Button();
        dgvTours = new DataGridView();

        btnTours = new Button();
        btnAddTour = new Button();
        btnDeleteTour = new Button();
        btnSupplierContracts = new Button();
        btnLogout = new Button();

        SuspendLayout();

        // ========== БОКОВАЯ ПАНЕЛЬ ==========
        sidePanel.Dock = DockStyle.Left;
        sidePanel.Width = 180;
        sidePanel.BackColor = Color.FromArgb(45, 45, 48); // Тёмный фон
        sidePanel.Padding = new Padding(10);

        // Заголовок в боковой панели
        var lblTitle = new Label
        {
            Text = "Меню",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 0, 10)
        };
        sidePanel.Controls.Add(lblTitle);

        // Кнопки боковой панели
        btnTours = CreateSideButton("Обновить Туры", Color.FromArgb(158, 158, 158));
        btnTours.Click += (s, e) => LoadToursAsync();

        btnAddTour = CreateSideButton("Добавить тур", Color.FromArgb(158, 158, 158));
        btnAddTour.Click += btnAddTour_Click;

        btnDeleteTour = CreateSideButton("X Удалить тур X", Color.FromArgb(244, 67, 54));
        btnDeleteTour.Click += btnDeleteTour_Click;

        btnSupplierContracts = CreateSideButton("Просмотреть Договоры", Color.FromArgb(158, 158, 158));
        btnSupplierContracts.Click += btnSupplierContracts_Click;

        btnLogout = CreateSideButton("Выйти :(", Color.FromArgb(158, 158, 158));
        btnLogout.Click += btnLogout_Click;
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Margin = new Padding(0, 10, 0, 0);

        sidePanel.Controls.Add(btnTours);
        sidePanel.Controls.Add(btnAddTour);
        sidePanel.Controls.Add(btnDeleteTour);
        sidePanel.Controls.Add(btnSupplierContracts);
        sidePanel.Controls.Add(btnLogout);

        // ========== ВЕРХНЯЯ ПАНЕЛЬ (поиск) ==========
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 60;
        topPanel.BackColor = Color.FromArgb(245, 245, 245);
        topPanel.Padding = new Padding(10);

        txtFilter = new TextBox
        {
            PlaceholderText = " Поиск по названию, типу, направлению, клиенту...",
            Width = 400,
            Height = 30,
            Font = new Font("Segoe UI", 10F)
        };
        txtFilter.Location = new Point(10, 15);
        txtFilter.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnSearch_Click(s, e); };

        btnSearch = new Button
        {
            Text = "Найти",
            Width = 80,
            Height = 30,
            Location = new Point(420, 15),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnSearch.Click += btnSearch_Click;

        lblUser = new Label
        {
            Text = "Пользователь",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(520, 20),
            AutoSize = true
        };

        lblStatus = new Label
        {
            Text = "Готов к работе",
            ForeColor = Color.Green,
            Location = new Point(520, 40),
            AutoSize = true,
            Font = new Font("Segoe UI", 8F)
        };

        topPanel.Controls.Add(txtFilter);
        topPanel.Controls.Add(btnSearch);
        topPanel.Controls.Add(lblUser);
        topPanel.Controls.Add(lblStatus);

        // ========== ТАБЛИЦА ==========
        dgvTours.Dock = DockStyle.Fill;
        dgvTours.AllowUserToAddRows = false;
        dgvTours.AllowUserToDeleteRows = false;
        dgvTours.ReadOnly = true;
        dgvTours.RowHeadersVisible = false;
        dgvTours.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTours.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvTours.BackgroundColor = Color.White;
        dgvTours.GridColor = Color.FromArgb(220, 220, 220);
        dgvTours.EnableHeadersVisualStyles = false;
        dgvTours.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
        dgvTours.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvTours.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvTours.ColumnHeadersHeight = 45;
        dgvTours.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

        // ========== ФОРМА ==========
        ClientSize = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Travel Agency - Управление турами";
        BackColor = Color.White;

        Controls.Add(dgvTours);
        Controls.Add(topPanel);
        Controls.Add(sidePanel);

        ResumeLayout(false);
        PerformLayout();
    }

    // Вспомогательный метод для создания кнопок боковой панели
    private Button CreateSideButton(string text, Color color)
    {
        var btn = new Button
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 5, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 67, 54, 50);
        return btn;
    }
}