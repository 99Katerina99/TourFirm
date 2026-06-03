using System.Text.Json;
using TravelAgency.Client.WinForms.Network;

namespace TravelAgency.Client.WinForms.Forms;

public partial class SupplierContractsForm : Form
{
    private readonly string _token;
    private readonly TcpClientService _tcpService;
    private DataGridView dgvContracts;
    private Label lblStatus;

    public SupplierContractsForm(string token, TcpClientService tcpService)
    {
        _token = token;
        _tcpService = tcpService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Договоры с поставщиками";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;

        dgvContracts = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        // Колонки
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Id", HeaderText = "ID", Width = 40 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "ContractNumber", HeaderText = "№ Договора", Width = 120 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "TourName", HeaderText = "Тур", Width = 200 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "SupplierName", HeaderText = "Поставщик", Width = 150 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Service", HeaderText = "Услуга", Width = 150 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Cost",
            HeaderText = "Стоимость",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "ConfirmationStatus", HeaderText = "Статус", Width = 120 });
        dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "ContractDate", HeaderText = "Дата", Width = 100 });

        lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = "Загрузка...",
            ForeColor = Color.Blue
        };

        Controls.Add(dgvContracts);
        Controls.Add(lblStatus);

        Load += SupplierContractsForm_Load;
    }

    private async void SupplierContractsForm_Load(object sender, EventArgs e)
    {
        await LoadContractsAsync();
    }

    private async Task LoadContractsAsync()
    {
        try
        {
            // Запрос к серверу (нужно добавить команду GET_SUPPLIER_CONTRACTS)
            var response = await _tcpService.SendCommandAsync($"GET_SUPPLIER_CONTRACTS|{_token}");

            if (response.StartsWith("OK|"))
            {
                var json = response.Substring(3);
                var contracts = JsonSerializer.Deserialize<List<SupplierContractDto>>(json) ?? new();
                dgvContracts.DataSource = contracts;
                lblStatus.Text = $"✅ Загружено договоров: {contracts.Count}";
                lblStatus.ForeColor = Color.Green;
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

    private class SupplierContractDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string TourName { get; set; }
        public string SupplierName { get; set; }
        public string Service { get; set; }
        public decimal Cost { get; set; }
        public string ConfirmationStatus { get; set; }
        public DateTime ContractDate { get; set; }
    }
}