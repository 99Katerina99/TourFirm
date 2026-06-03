using System.Text.Json;
using TravelAgency.Client.WinForms.Network;

namespace TravelAgency.Client.WinForms.Forms;

public partial class AddTourForm : Form
{
    private readonly string _token;
    private readonly TcpClientService _tcpService;

    // Поля тура
    private TextBox txtName;
    private ComboBox cmbTourType;
    private TextBox txtDestination;
    private DateTimePicker dtpStart;
    private DateTimePicker dtpEnd;
    private NumericUpDown numPrice;
    private NumericUpDown numMaxSeats;
    private TextBox txtDescription;

    // Поля клиента
    private ComboBox cmbExistingClient;
    private Button btnNewClient;
    private TextBox txtClientLastName;
    private TextBox txtClientFirstName;
    private TextBox txtClientPhone;
    private Panel panelClient;

    // Поля поставщика и платежа
    private ComboBox cmbSupplier;
    private ComboBox cmbPaymentMethod;
    private NumericUpDown numAmount;

    private Button btnSave;
    private Button btnCancel;
    private Label lblStatus;

    public AddTourForm(string token, TcpClientService tcpService)
    {
        _token = token;
        _tcpService = tcpService;
        InitializeControls();
    }

    private void InitializeControls()
    {
        Text = "Создание тура с регистрацией клиента";
        Size = new Size(550, 820);
        StartPosition = FormStartPosition.CenterParent;

        int y = 10;
        int lblW = 140;
        int inpW = 340;
        int step = 35;

        // ========== СЕКЦИЯ ТУРА ==========
        Controls.Add(new Label { Text = "Данные тура:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        y += 25;

        Controls.Add(new Label { Text = "Название:", Location = new Point(20, y), Width = lblW });
        txtName = new TextBox { Location = new Point(170, y), Width = inpW };
        Controls.Add(txtName);
        y += step;

        Controls.Add(new Label { Text = "Тип тура:", Location = new Point(20, y), Width = lblW });
        cmbTourType = new ComboBox { Location = new Point(170, y), Width = inpW, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTourType.Items.AddRange(new[] { "Круизный", "Курортный", "Туристический", "Бизнес", "Эксклюзивный" });
        Controls.Add(cmbTourType);
        y += step;

        Controls.Add(new Label { Text = "Направление Маршрута:", Location = new Point(20, y), Width = lblW });
        txtDestination = new TextBox { Location = new Point(170, y), Width = inpW, PlaceholderText = "Франция, Париж" };
        Controls.Add(txtDestination);
        y += step;

        Controls.Add(new Label { Text = "Дата начала:", Location = new Point(20, y), Width = lblW });
        dtpStart = new DateTimePicker { Location = new Point(170, y), Width = inpW, Format = DateTimePickerFormat.Short };
        Controls.Add(dtpStart);
        y += step;

        Controls.Add(new Label { Text = "Дата окончания:", Location = new Point(20, y), Width = lblW });
        dtpEnd = new DateTimePicker { Location = new Point(170, y), Width = inpW, Format = DateTimePickerFormat.Short };
        Controls.Add(dtpEnd);
        y += step;

        Controls.Add(new Label { Text = "Цена:", Location = new Point(20, y), Width = lblW });
        numPrice = new NumericUpDown { Location = new Point(170, y), Width = inpW, Maximum = 1000000, DecimalPlaces = 2, Value = 1000 };
        Controls.Add(numPrice);
        y += step;

        Controls.Add(new Label { Text = "Макс. мест:", Location = new Point(20, y), Width = lblW });
        numMaxSeats = new NumericUpDown { Location = new Point(170, y), Width = inpW, Maximum = 1000, Value = 10 };
        Controls.Add(numMaxSeats);
        y += step;

        Controls.Add(new Label { Text = "Описание:", Location = new Point(20, y), Width = lblW });
        txtDescription = new TextBox { Location = new Point(170, y), Width = inpW, Multiline = true, Height = 50 };
        Controls.Add(txtDescription);
        y += 70;

        // ========== СЕКЦИЯ КЛИЕНТА ==========
        Controls.Add(new Label { Text = "Клиент:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        y += 25;

        Controls.Add(new Label { Text = "Существующий:", Location = new Point(20, y), Width = lblW });
        cmbExistingClient = new ComboBox { Location = new Point(170, y), Width = inpW, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbExistingClient.Items.Add("(новый клиент)");
        cmbExistingClient.SelectedIndex = 0;
        cmbExistingClient.SelectedIndexChanged += cmbExistingClient_SelectedIndexChanged;
        Controls.Add(cmbExistingClient);
        y += step;

        // Панель для нового клиента
        panelClient = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(490, 110),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(240, 240, 240)
        };

        var lblLastName = new Label { Text = "Фамилия:", Location = new Point(10, 10), Width = 90 };
        txtClientLastName = new TextBox { Location = new Point(110, 10), Width = 200 };

        var lblFirstName = new Label { Text = "Имя:", Location = new Point(10, 45), Width = 90 };
        txtClientFirstName = new TextBox { Location = new Point(110, 45), Width = 200 };

        var lblPhone = new Label { Text = "Телефон:", Location = new Point(10, 80), Width = 90 };
        txtClientPhone = new TextBox { Location = new Point(110, 80), Width = 200 };

        panelClient.Controls.AddRange(new Control[] {
        lblLastName, txtClientLastName,
        lblFirstName, txtClientFirstName,
    lblPhone, txtClientPhone
});

        Controls.Add(panelClient);
        y += 120; // Отступ после панели

        // ========== СЕКЦИЯ ПОСТАВЩИКА И ПЛАТЕЖА ==========
        Controls.Add(new Label { Text = "Поставщик и оплата:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        y += 25;

        Controls.Add(new Label { Text = "Поставщик:", Location = new Point(20, y), Width = lblW });
        cmbSupplier = new ComboBox { Location = new Point(170, y), Width = inpW, DropDownStyle = ComboBoxStyle.DropDownList };
        Controls.Add(cmbSupplier);
        y += step;

        Controls.Add(new Label { Text = "Способ оплаты:", Location = new Point(20, y), Width = lblW });
        cmbPaymentMethod = new ComboBox { Location = new Point(170, y), Width = inpW, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbPaymentMethod.Items.AddRange(new[] { "Наличные", "Банковская карта", "Банковский перевод" });
        cmbPaymentMethod.SelectedIndex = 0;
        Controls.Add(cmbPaymentMethod);
        y += step;

        Controls.Add(new Label { Text = "Сумма оплаты:", Location = new Point(20, y), Width = lblW });
        numAmount = new NumericUpDown { Location = new Point(170, y), Width = inpW, Maximum = 1000000, DecimalPlaces = 2 };
        Controls.Add(numAmount);
        y += step;

        // ========== СТАТУС И КНОПКИ ==========
        lblStatus = new Label { Text = "", Location = new Point(20, y), Width = 490, ForeColor = Color.Red };
        Controls.Add(lblStatus);
        y += 30;

        btnSave = new Button { Text = "Сохранить всё", Location = new Point(170, y), Width = 150, Height = 40, BackColor = Color.LightGreen };
        btnSave.Click += btnSave_Click;
        Controls.Add(btnSave);

        btnCancel = new Button { Text = "Отмена", Location = new Point(340, y), Width = 150, Height = 40 };
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(btnCancel);

        // Загрузка справочников
        LoadSuppliersAsync();
        LoadClientsAsync();
    }

    private void cmbExistingClient_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Если выбран существующий клиент — скрываем поля нового, если "новый" — показываем
        if (cmbExistingClient.SelectedIndex <= 0)
            panelClient.Visible = true;
        else
            panelClient.Visible = false;
    }

    private async void LoadSuppliersAsync()
    {
        try
        {
            var response = await _tcpService.SendCommandAsync($"GET_SUPPLIERS|{_token}");
            if (response.StartsWith("OK|"))
            {
                var suppliers = JsonSerializer.Deserialize<List<SupplierDto>>(response.Substring(3)) ?? new();
                cmbSupplier.Items.Clear();
                foreach (var s in suppliers)
                    cmbSupplier.Items.Add($"{s.Id} - {s.Name} ({s.Type})");
                if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
            }
        }
        catch { /* игнорируем, пользователь введёт вручную если нужно */ }
    }

    private async void LoadClientsAsync()
    {
        try
        {
            var response = await _tcpService.SendCommandAsync($"GET_CLIENTS|{_token}");
            if (response.StartsWith("OK|"))
            {
                var clients = JsonSerializer.Deserialize<List<ClientDto>>(response.Substring(3)) ?? new();
                cmbExistingClient.Items.Clear();
                cmbExistingClient.Items.Add("(новый клиент)");
                foreach (var c in clients)
                    cmbExistingClient.Items.Add(c);
                cmbExistingClient.SelectedIndex = 0;
            }
        }
        catch { /* игнорируем */ }
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            lblStatus.Text = "❌ Введите название тура"; return;
        }
        if (cmbTourType.SelectedIndex == -1)
        {
            lblStatus.Text = "❌ Выберите тип тура"; return;
        }
        if (string.IsNullOrWhiteSpace(txtDestination.Text))
        {
            lblStatus.Text = "❌ Введите направление"; return;
        }
        if (dtpEnd.Value <= dtpStart.Value)
        {
            lblStatus.Text = "❌ Дата окончания должна быть позже даты начала"; return;
        }
        if (cmbSupplier.SelectedIndex == -1)
        {
            lblStatus.Text = "❌ Выберите поставщика"; return;
        }
        if (cmbExistingClient.SelectedIndex <= 0 &&
            (string.IsNullOrWhiteSpace(txtClientLastName.Text) || string.IsNullOrWhiteSpace(txtClientFirstName.Text)))
        {
            lblStatus.Text = "❌ Заполните данные клиента"; return;
        }

        // Определяем данные клиента
        string clientLastName, clientFirstName, clientPhone;
        if (cmbExistingClient.SelectedIndex > 0)
        {
            var selected = (ClientDto)cmbExistingClient.SelectedItem;
            clientLastName = selected.Name.Split(' ')[0];
            clientFirstName = selected.Name.Split(' ').Length > 1 ? selected.Name.Split(' ')[1] : "";
            clientPhone = selected.Phone;
        }
        else
        {
            clientLastName = txtClientLastName.Text;
            clientFirstName = txtClientFirstName.Text;
            clientPhone = txtClientPhone.Text;
        }

        // Извлекаем ID поставщика из строки "1 - Отель Марриотт (Отель)"
        var supplierText = cmbSupplier.SelectedItem?.ToString() ?? "";
        var supplierId = supplierText.Split(' ')[0];

        // CREATE_TOUR_FULL|token|Name|TourType|Destination|RouteId|StartDate|EndDate|Price|MaxSeats|Description
        //                  |ClientLastName|ClientFirstName|ClientPhone|SupplierId|PaymentMethod|Amount
        var command = $"CREATE_TOUR_FULL|{_token}" +
                      $"|{txtName.Text}|{cmbTourType.Text}|{txtDestination.Text}" +
                      $"|1|{dtpStart.Value:yyyy-MM-dd}|{dtpEnd.Value:yyyy-MM-dd}" +
                      $"|{numPrice.Value}|{numMaxSeats.Value}|{txtDescription.Text}" +
                      $"|{clientLastName}|{clientFirstName}|{clientPhone}" +
                      $"|{supplierId}|{cmbPaymentMethod.Text}|{numAmount.Value}";

        try
        {
            lblStatus.Text = "⏳ Создание...";
            lblStatus.ForeColor = Color.Blue;
            var response = await _tcpService.SendCommandAsync(command);

            if (response.StartsWith("OK|"))
            {
                lblStatus.Text = $"✅ {response.Substring(3)}";
                lblStatus.ForeColor = Color.Green;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = $"❌ {response}";
                lblStatus.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ Ошибка: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
        }
    }

    // DTO для справочников
    private class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public override string ToString() => $"{Id} - {Name} ({Type})";
    }

    private class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public override string ToString() => Name;
    }
}