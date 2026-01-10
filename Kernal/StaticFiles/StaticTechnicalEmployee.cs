namespace EventPro.Kernal.StaticFiles
{
    public static class StaticTechnicalEmployee
    {
        public static List<EmployeeEMail> EmployeeList = new List<EmployeeEMail>() {
                new EmployeeEMail
                {
                    Name = "Ahmed",
                    Email = "developer1@eventpro.example.com"
                },
                 new EmployeeEMail
                {
                    Name = "Mohammed",
                    Email = "cs@EventPro.org"
                },
                  new EmployeeEMail
                {
                    Name = "Sherif",
                    Email = "developer2@eventpro.example.com"
                },
            };

    }

    public class EmployeeEMail
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
