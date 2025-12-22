namespace EmailServiceApplication.Models
{
    public class DepartmentGroup
    {
        
            public int Id { get; set; }
            public string Name { get; set; } // HR, IT, Finance

            public List<User> Users { get; set; }
        
    }
}
