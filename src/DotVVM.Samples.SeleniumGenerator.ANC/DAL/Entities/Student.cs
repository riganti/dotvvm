using System;

namespace DotVVM.Samples.SeleniumGenerator.ANC.DAL.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string About { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
