using System;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;

namespace EmployeeAccounting
{
    internal class XmlFileUploader : IFileUploader
    {
        public void UploadEmployees(string fileName, DataGridViewRowCollection rows)
        {
            XDocument document = new XDocument();
            XElement employees = new XElement("employees");

            foreach (DataGridViewRow row in rows)
            {
                XElement person = new XElement("person");
                XAttribute id = new XAttribute("id", row.Cells["Id"].Value);
                XElement lastName = new XElement("lastName", row.Cells["LastName"].Value);
                XElement name = new XElement("name", row.Cells["Name"].Value);
                XElement patronymic = new XElement("patronymic", row.Cells["Patronymic"].Value);
                XElement position = new XElement("position", row.Cells["Position"].Value);
                XElement salary = new XElement("salary", row.Cells["Salary"].Value);
                XElement employmentDate = new XElement("employmentDate", ((DateTime)row.Cells["EmploymentDate"].Value).ToShortDateString());
                string DismissalDate = row.Cells["DismissalDate"].Value == DBNull.Value ? string.Empty : ((DateTime)row.Cells["DismissalDate"].Value).ToShortDateString();
                XElement dismissalDate = new XElement("dismissalDate", DismissalDate);
                person.Add(id, lastName, name, patronymic, position, salary, employmentDate, dismissalDate);
                employees.Add(person);
            }

            document.Add(employees);
            document.Save(Path.Combine(fileName, "employees.xml"));
        }
    }
}
