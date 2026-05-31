using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TravelAgency.Client.WinForms.Forms;

partial class AddTourForm : Form
{
    private IContainer components = null;
    private TextBox txtName, txtDesc, txtCountry, txtCity;
    private NumericUpDown numDays, numPrice, numSeats;
    private DateTimePicker dtpStart;
    private Button btnSave, btnCancel;
    private Label lblStatus, lblName, lblDesc, lblCountry, lblCity, lblDays, lblPrice, lblSeats, lblDate;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        txtName = new TextBox(); txtDesc = new TextBox(); txtCountry = new TextBox(); txtCity = new TextBox();
        numDays = new NumericUpDown(); numPrice = new NumericUpDown(); numSeats = new NumericUpDown();
        dtpStart = new DateTimePicker(); btnSave = new Button(); btnCancel = new Button(); lblStatus = new Label();
        lblName = new Label(); lblDesc = new Label(); lblCountry = new Label(); lblCity = new Label();
        lblDays = new Label(); lblPrice = new Label(); lblSeats = new Label(); lblDate = new Label();

        ((ISupportInitialize)numDays).BeginInit();
        ((ISupportInitialize)numPrice).BeginInit();
        ((ISupportInitialize)numSeats).BeginInit();
        SuspendLayout();

        // === Подписи ===
        lblName.Text = "Название:"; lblName.Location = new Point(20, 20); lblName.AutoSize = true;
        lblDesc.Text = "Описание:"; lblDesc.Location = new Point(20, 50); lblDesc.AutoSize = true;
        lblCountry.Text = "Страна:"; lblCountry.Location = new Point(20, 80); lblCountry.AutoSize = true;
        lblCity.Text = "Город:"; lblCity.Location = new Point(20, 110); lblCity.AutoSize = true;
        lblDays.Text = "Дней:"; lblDays.Location = new Point(20, 140); lblDays.AutoSize = true;
        lblPrice.Text = "Цена:"; lblPrice.Location = new Point(20, 170); lblPrice.AutoSize = true;
        lblDate.Text = "Дата старта:"; lblDate.Location = new Point(20, 200); lblDate.AutoSize = true;
        lblSeats.Text = "Мест:"; lblSeats.Location = new Point(20, 230); lblSeats.AutoSize = true;

        // === Поля ввода ===
        txtName.Location = new Point(120, 17); txtName.Size = new Size(260, 23);
        txtDesc.Location = new Point(120, 47); txtDesc.Size = new Size(260, 23);
        txtCountry.Location = new Point(120, 77); txtCountry.Size = new Size(120, 23);
        txtCity.Location = new Point(120, 107); txtCity.Size = new Size(120, 23);

        numDays.Location = new Point(120, 137); numDays.Size = new Size(70, 23); numDays.Minimum = 1; numDays.Maximum = 365; numDays.Value = 7;
        numPrice.Location = new Point(120, 167); numPrice.Size = new Size(100, 23); numPrice.Minimum = 0; numPrice.Increment = 100; numPrice.DecimalPlaces = 2;
        numSeats.Location = new Point(120, 227); numSeats.Size = new Size(70, 23); numSeats.Minimum = 1; numSeats.Maximum = 1000; numSeats.Value = 10;

        dtpStart.Location = new Point(120, 197); dtpStart.Size = new Size(130, 23); dtpStart.Format = DateTimePickerFormat.Short;

        // === Кнопки ===
        btnSave.Location = new Point(120, 265); btnSave.Size = new Size(120, 32); btnSave.Text = "💾 Сохранить"; btnSave.Click += btnSave_Click;
        btnCancel.Location = new Point(250, 265); btnCancel.Size = new Size(120, 32); btnCancel.Text = "❌ Отмена"; btnCancel.Click += btnCancel_Click;

        // === Статус ===
        lblStatus.Location = new Point(20, 310); lblStatus.Size = new Size(360, 20); lblStatus.ForeColor = Color.Red; lblStatus.Text = "";

        // === Форма ===
        ClientSize = new Size(400, 350);
        Text = " Добавить новый тур";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Controls.AddRange(new Control[] {
            lblStatus, btnCancel, btnSave, dtpStart, numSeats, numPrice, numDays,
            txtCity, txtCountry, txtDesc, txtName,
            lblDate, lblSeats, lblPrice, lblDays, lblCity, lblCountry, lblDesc, lblName
        });

        ((ISupportInitialize)numDays).EndInit();
        ((ISupportInitialize)numPrice).EndInit();
        ((ISupportInitialize)numSeats).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}