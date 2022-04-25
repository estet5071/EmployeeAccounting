using System;
using System.Collections.Generic;
using System.Data;


namespace EmployeeAccounting
{
    public interface IDbContext : IDisposable
    {
        DataTable GetAllEmployees();

        void AddEmployee(List<string> employeeInfo);

        void UpdateEmployee(int id, List<string> newEmployeeInfo);

        void DeleteEmployee(int id);

        void FindEmployees(string searchLine, int SelectedComboBoxIndex);

        void GetStatistics(out int employeeCount, out decimal avgSalary, out int dismissedCount);




    }
}
