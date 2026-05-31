using System.ComponentModel;  // ← Для Container, ISupportInitialize
using System.Drawing;         // ← Для Color, Font, Point, Size
using System.Windows.Forms;
namespace TravelAgency.Client.WinForms.Forms;

partial class LoginForm : Form
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtHost;
    private TextBox txtPort;
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnLogin;
    private Label lblStatus;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;

    private void InitializeComponent()
    {
        this.txtHost = new TextBox();
        this.txtPort = new TextBox();
        this.txtUsername = new TextBox();
        this.txtPassword = new TextBox();
        this.btnLogin = new Button();
        this.lblStatus = new Label();
        this.label1 = new Label();
        this.label2 = new Label();
        this.label3 = new Label();
        this.label4 = new Label();
        this.SuspendLayout();

        // label1: "Сервер:"
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(20, 20);
        this.label1.Text = "Сервер:";

        // txtHost
        this.txtHost.Location = new System.Drawing.Point(100, 17);
        this.txtHost.Size = new System.Drawing.Size(150, 23);
        this.txtHost.Text = "127.0.0.1";

        // label2: "Порт:"
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(20, 50);
        this.label2.Text = "Порт:";

        // txtPort
        this.txtPort.Location = new System.Drawing.Point(100, 47);
        this.txtPort.Size = new System.Drawing.Size(50, 23);
        this.txtPort.Text = "8888";

        // label3: "Логин:"
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(20, 80);
        this.label3.Text = "Логин:";

        // txtUsername
        this.txtUsername.Location = new System.Drawing.Point(100, 77);
        this.txtUsername.Size = new System.Drawing.Size(150, 23);

        // label4: "Пароль:"
        this.label4.AutoSize = true;
        this.label4.Location = new System.Drawing.Point(20, 110);
        this.label4.Text = "Пароль:";

        // txtPassword
        this.txtPassword.Location = new System.Drawing.Point(100, 107);
        this.txtPassword.Size = new System.Drawing.Size(150, 23);
        this.txtPassword.PasswordChar = '*';

        // btnLogin
        this.btnLogin.Location = new System.Drawing.Point(100, 140);
        this.btnLogin.Size = new System.Drawing.Size(100, 30);
        this.btnLogin.Text = "Войти";
        this.btnLogin.Click += btnLogin_Click;

        // lblStatus
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new System.Drawing.Point(20, 180);
        this.lblStatus.ForeColor = System.Drawing.Color.Red;

        // LoginForm
        this.ClientSize = new System.Drawing.Size(284, 221);
        this.Text = "Авторизация — Travel Agency";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosing += LoginForm_FormClosing;
        this.Controls.AddRange(new Control[] {
            label1, txtHost, label2, txtPort, label3, txtUsername,
            label4, txtPassword, btnLogin, lblStatus });
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}