using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Configuration;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace EmployeeAccounting
{
    internal class DbContext : IDbContext
    {
        private readonly DataTable _employeeTable;
        private SqlDataAdapter _adapter;    // SqlConnection открывается и закрывается адаптером при вызове методов Fill и Update
        private SqlConnection _connection;
        private List<DataRow> _rowsForSearch;
        private bool _disposed = false;

        public DbContext()
        {
            _connection = new SqlConnection(GetConnectionString());
            string request = "SELECT * FROM Employees";
            _adapter = new SqlDataAdapter(request, _connection);
            _employeeTable = new DataTable();
            _adapter.Fill(_employeeTable);
            _employeeTable.PrimaryKey = new DataColumn[] { _employeeTable.Columns[0] };
            FillRowsForSearch(_employeeTable.Rows.Count);
        }

        public DataTable GetAllEmployees()
        {
            return _employeeTable;
        }



        public async void AddEmployee(List<string> employeeInfo)
        {

            DataRow newRow = _employeeTable.NewRow();
            FillRow(newRow, employeeInfo);
            await InsertToDataBase(newRow);
            _employeeTable.Rows.Add(newRow);
            DataRow rowForSearch = _employeeTable.NewRow();
            rowForSearch.ItemArray = newRow.ItemArray;
            _rowsForSearch.Add(rowForSearch);

        }

        public async void UpdateEmployee(int id, List<string> newEmployeeInfo)
        {
            var row = _employeeTable.Rows.Find(id);
            var searchRow = _rowsForSearch.Find(r => (int)r["Id"] == id);
            FillRow(row, newEmployeeInfo);
            FillRow(searchRow, newEmployeeInfo);
            using (SqlCommand updateCommand = new SqlCommand("UPDATE Employees SET LastName = @p1, Name = @p2, Patronymic = @p3, Position = @p4, Salary = @p5, EmploymentDate = @p6, DismissalDate = @p7 WHERE Id = @p8", _connection))
            {
                updateCommand.Parameters.AddWithValue("@p8", row["Id"]);
                for (int i = 1; i < row.ItemArray.Length; i++)
                {
                    updateCommand.Parameters.AddWithValue("@p" + i, row.ItemArray[i]);
                }
                await _connection.OpenAsync();
                await updateCommand.ExecuteNonQueryAsync();
                await _connection.CloseAsync();
            }

        }


        public async void DeleteEmployee(int id)
        {
            var searchRow = _rowsForSearch.Find(r => (int)r["Id"] == id);
            _rowsForSearch.Remove(searchRow);
            using (SqlCommand command = new SqlCommand($"DELETE FROM Employees WHERE Id = {id}", _connection))
            {
                await _connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await _connection.CloseAsync();
            }
        }

   

        public void FindEmployees(string searchLine, int selectedComboBoxIndex)
        {
            _employeeTable.Rows.Clear();
            if (string.IsNullOrWhiteSpace(searchLine))
            {
                AddRowsToTable(_rowsForSearch);
                return;
            }
            var searchedRows = _rowsForSearch.Where(r => r[selectedComboBoxIndex + 1].ToString().StartsWith(searchLine, StringComparison.CurrentCultureIgnoreCase));
            AddRowsToTable(searchedRows);
        }


        public void GetStatistics(out int employeeCount, out decimal avgSalary, out int dismissedCount)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Employees", connection))
                {
                    employeeCount = (int)command.ExecuteScalar();
                    command.CommandText = "SELECT AVG(Salary) FROM Employees";
                    avgSalary = (decimal)command.ExecuteScalar();
                    command.CommandText = "SELECT COUNT(*) FROM Employees WHERE NOT DismissalDate IS NULL";
                    dismissedCount = (int)command.ExecuteScalar();
                }
                connection.Close();
            }


        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _adapter.Dispose();
                _connection.Dispose();
                _employeeTable.Dispose();
                _disposed = true;
            }
        }

        private void AddRowsToTable(IEnumerable<DataRow> rows)
        {
            foreach (var row in rows)
            {
                _employeeTable.Rows.Add(row.ItemArray);
            }
        }


        private void FillRowsForSearch(int employeeCount)
        {
            DataRow newRow;
            _rowsForSearch = new List<DataRow>(employeeCount);
            for (int i = 0; i < _employeeTable.Rows.Count; i++)
            {
                newRow = _employeeTable.NewRow();
                newRow.ItemArray = _employeeTable.Rows[i].ItemArray;
                _rowsForSearch.Add(newRow);
            }

        }


        private string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            if (!connectionString.Contains("%CONTENTROOTPATH%"))
                return connectionString;

            string solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            return connectionString.Replace("%CONTENTROOTPATH%", solutionPath);


        }

        private void FillRow(DataRow row, List<string> employeeInfo)
        {
            int counter = 1;
            foreach (var value in employeeInfo)
            {

                row[counter++] = value;

            }
        }


        private async Task InsertToDataBase(DataRow row)
        {
            using (SqlCommandBuilder commandBuilder = new SqlCommandBuilder(_adapter))
            {
                _adapter.InsertCommand = commandBuilder.GetInsertCommand();
                _adapter.InsertCommand.CommandText += ";SELECT SCOPE_IDENTITY()";
                _adapter.InsertCommand.Parameters.Clear();
                for (int i = 1; i < row.ItemArray.Length; i++)
                {
                    _adapter.InsertCommand.Parameters.AddWithValue("@p" + i, row.ItemArray[i]);
                }
                await _connection.OpenAsync();
                var id = await _adapter.InsertCommand.ExecuteScalarAsync();
                await _connection.CloseAsync();
                row[0] = id;
            }

        }

    }
}
