using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TravelAgency.Client.WinForms.Network;

namespace TravelAgency.Client.WinForms.Forms;

public partial class AddTourForm : Form
{
    private readonly string _token;
    private readonly TcpClientService _tcpService;

    // ⚠️ Важно: принимаем токен И сервис подключения, чтобы использовать существующую сессию
    public AddTourForm(string token, TcpClientService tcpService)
    {
        InitializeComponent();
        _token = token;
        _tcpService = tcpService;

        // Устанавливаем дату по умолчанию (через месяц)
        dtpStart.Value = DateTime.Today.AddDays(30);
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(txtName.Text)) { ShowStatus("⚠ Введите название тура", false); return; }
        if (string.IsNullOrWhiteSpace(txtCountry.Text)) { ShowStatus("⚠ Введите страну", false); return; }
        if (numPrice.Value <= 0) { ShowStatus(" Цена должна быть больше 0", false); return; }
        if (numSeats.Value <= 0) { ShowStatus("⚠ Количество мест должно быть > 0", false); return; }

        btnSave.Enabled = false;
        ShowStatus("📤 Отправка данных на сервер...", true);

        try
        {
            // Формат команды: ADD_TOUR_SQL|token|Name|Desc|Country|City|Days|Price|StartDate|Seats
            string command = $"ADD_TOUR_SQL|{_token}|{txtName.Text}|{txtDesc.Text}|{txtCountry.Text}|{txtCity.Text}|{(int)numDays.Value}|{numPrice.Value}|{dtpStart.Value:yyyy-MM-dd}|{(int)numSeats.Value}";

            string response = await _tcpService.SendCommandAsync(command);

            if (response.StartsWith("OK|"))
            {
                ShowStatus("✅ Тур успешно добавлен!", false);
                await Task.Delay(800);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                string errorMsg = response.StartsWith("ERROR|") ? response.Substring(6) : response;
                ShowStatus($"❌ Ошибка: {errorMsg}", false);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Ошибка соединения: {ex.Message}", false);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void ShowStatus(string message, bool isLoading)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isLoading ? System.Drawing.Color.Gray :
                              message.Contains("✅") ? System.Drawing.Color.Green : System.Drawing.Color.Red;
    }
}