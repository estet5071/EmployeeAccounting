using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EmployeeAccounting
{
    public partial class EditForm : Form
    {
        private readonly IDbContext _service;
        private bool _isUpdate;
        private int _idForUpdate;
        public EditForm(IDbContext service)
        {
            _service = service;
            InitializeComponent();
        }

        public event Action UpdateStatistics;

        public void Show(bool isUpdate, params string[] employeeInfo)
        {
            _isUpdate = isUpdate;
            if (isUpdate)
            {
                _idForUpdate = int.Parse(employeeInfo[0]);
                lastNameTextBox.Text = employeeInfo[1];
                nameTextBox.Text = employeeInfo[2];
                patronymicTextBox.Text = employeeInfo[3];
                positionTextBox.Text = employeeInfo[4];
                salaryTextBox.Text = employeeInfo[5];
                employmentDate.Value = DateTime.Parse(employeeInfo[6]);
            }

            base.Show();
        }


        private void ApplyButton_Click(object sender, EventArgs e)
        {

            try
            {
                if (_isUpdate)
                    _service.UpdateEmployee(_idForUpdate, GetValues());
                else
                    _service.AddEmployee(GetValues());
            }
            catch (Exception)
            {
                MessageBox.Show("Неверные данные", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            UpdateStatistics?.Invoke();
            this.Close();
        }


        private List<string> GetValues()
        {
            return new List<string> { lastNameTextBox.Text, nameTextBox.Text, patronymicTextBox.Text, positionTextBox.Text, salaryTextBox.Text, employmentDate.Value.ToShortDateString() };
        }

      
        private void DismissButton_Click(object sender, EventArgs e)
        {
            List<string> values = GetValues();
            values.Add(DateTime.Now.ToShortDateString());
            _service.UpdateEmployee(_idForUpdate, values);
            UpdateStatistics?.Invoke();
            this.Close();
        }

        private void LettersTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void SalaryTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8 && e.KeyChar != 44)
                e.Handled = true;


            if (e.KeyChar == 44 && salaryTextBox.Text.Contains(','))
                e.Handled = true;

        }
    }
}
