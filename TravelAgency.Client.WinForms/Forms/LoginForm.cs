using System.Windows.Forms;
using TravelAgency.Client.WinForms.Network;

namespace TravelAgency.Client.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly TcpClientService _tcpService;

    public LoginForm()
    {
        InitializeComponent();
        _tcpService = new TcpClientService();
    }

    // === Обработчик кнопки "Войти" ===
    private async void btnLogin_Click(object sender, EventArgs e)
    {
        // Валидация полей
        if (string.IsNullOrWhiteSpace(txtUsername.Text))
        {
            ShowStatus("⚠ Введите логин", false);
            txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            ShowStatus("⚠ Введите пароль", false);
            txtPassword.Focus();
            return;
        }

        string host = txtHost.Text.Trim();
        int port = int.TryParse(txtPort.Text, out var p) ? p : 8888;
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        lblStatus.Text = "Подключение...";
        lblStatus.ForeColor = Color.Blue;
        btnLogin.Enabled = false;

        try
        {
            // 1. Подключение к серверу
            if (!await _tcpService.ConnectAsync(host, port))
            {
                ShowStatus("❌ Не удалось подключиться к серверу");
                btnLogin.Enabled = true;
                return;
            }

            // 2. Авторизация
            if (await _tcpService.LoginAsync(username, password))
            {
                ShowStatus("✅ Успешный вход!", false);

                // 3. Открываем главное окно
                var mainForm = new MainForm(_tcpService, username);
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                ShowStatus("❌ Неверный логин или пароль");
                _tcpService.Disconnect();
                btnLogin.Enabled = true;
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Ошибка: {ex.Message}");
            _tcpService.Disconnect();
            btnLogin.Enabled = true;
        }
    }

    // === Обработчик закрытия формы ===
    private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _tcpService?.Dispose();
    }

    // === Вспомогательный метод для статуса ===
    private void ShowStatus(string message, bool isError = true)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isError ? Color.Red : Color.Green;

        // Автоочистка через 3 секунды для успешных сообщений
        if (!isError)
        {
            Task.Delay(3000).ContinueWith(_ =>
                this.Invoke(() => { if (lblStatus.Text == message) lblStatus.Text = ""; }),
                TaskScheduler.Default);
        }
    }

    // === Обработка Enter в поле пароля ===
    private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            btnLogin.PerformClick();
            e.Handled = true;
        }
    }
}