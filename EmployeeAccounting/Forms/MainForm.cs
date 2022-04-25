using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace EmployeeAccounting
{
    public partial class MainForm : Form
    {
        private readonly IDbContext _service;
        private readonly IFileUploader _uploader;
        public MainForm(IDbContext service, IFileUploader fileUploader)
        {
            _service = service;
            _uploader = fileUploader;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            grid.DataSource = _service.GetAllEmployees();
            UpdateStatistics();
            ChangeColumnNames();
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

        }


        private void ChangeColumnNames()
        {
            grid.Columns[1].HeaderText = "Фамилия";
            grid.Columns[2].HeaderText = "Имя";
            grid.Columns[3].HeaderText = "Отчество";
            grid.Columns[4].HeaderText = "Должность";
            grid.Columns[5].HeaderText = "Оклад";
            grid.Columns[6].HeaderText = "Дата приема на работу";
            grid.Columns[7].HeaderText = "Дата увольнения";
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _service.FindEmployees(searchTextBox.Text, searchComboBox.SelectedIndex);
        }

        private void AddEmployee_Click(object sender, EventArgs e)
        {
            EditForm editForm = new EditForm(_service);
            editForm.UpdateStatistics += UpdateStatistics;
            editForm.Show(false);
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            EditForm editForm = new EditForm(_service);
            editForm.UpdateStatistics += UpdateStatistics;
            try
            {
                editForm.Show(true, GetValues());
            }
            catch (Exception)
            {
                MessageBox.Show("Выберите строку", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Grid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            _service.DeleteEmployee((int)grid.SelectedRows[0].Cells[0].Value);
            UpdateStatistics();
        }


        private void UpdateStatistics()
        {
            _service.GetStatistics(out int EmpCount, out decimal avgSalary, out int dismissedCount);
            countLabel.Text = EmpCount.ToString();
            avgLabel.Text = avgSalary.ToString();
            dismissedNumberLabel.Text = dismissedCount.ToString();

        }

        private string[] GetValues()
        {
            var cells = grid.SelectedRows[0].Cells;
            List<string> values = new List<string>();
            for (int i = 0; i < cells.Count; i++)
            {
                values.Add(cells[i].Value.ToString());
            }
            return values.ToArray();
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _uploader.UploadEmployees(dialog.SelectedPath, grid.Rows);
                }
                else
                {
                    MessageBox.Show("Не удалось выбрать путь", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }


    }
}
