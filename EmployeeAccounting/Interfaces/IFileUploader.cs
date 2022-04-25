using System.Windows.Forms;

namespace EmployeeAccounting
{
    public interface IFileUploader
    {
        void UploadEmployees(string fileName, DataGridViewRowCollection rows);
    }
}
