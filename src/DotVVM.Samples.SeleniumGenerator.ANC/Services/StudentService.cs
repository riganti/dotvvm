using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Samples.SeleniumGenerator.ANC.DAL;
using DotVVM.Samples.SeleniumGenerator.ANC.DAL.Entities;
using DotVVM.Samples.SeleniumGenerator.ANC.Models;
using Microsoft.EntityFrameworkCore;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Services
{
    public class StudentService
    {
        private readonly StudentDbContext studentDbContext;

        public StudentService(StudentDbContext studentDbContext)
        {
            this.studentDbContext = studentDbContext;
        }

        public async Task<List<StudentListModel>> GetAllStudentsAsync()
        {

            return await studentDbContext.Students.Select(
                s => new StudentListModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName
                }
            ).ToListAsync();
        }


        public async Task<StudentDetailModel> GetStudentByIdAsync(int studentId)
        {
            return await studentDbContext.Students.Select(
                    s => new StudentDetailModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        About = s.About,
                        EnrollmentDate = s.EnrollmentDate
                    })
                .FirstOrDefaultAsync(s => s.Id == studentId);

        }

        public async Task UpdateStudentAsync(StudentDetailModel student)
        {
            var entity = await studentDbContext.Students.FirstOrDefaultAsync(s => s.Id == student.Id);

            entity.FirstName = student.FirstName;
            entity.LastName = student.LastName;
            entity.About = student.About;
            entity.EnrollmentDate = student.EnrollmentDate;

            await studentDbContext.SaveChangesAsync();
        }

        public async Task InsertStudentAsync(StudentDetailModel student)
        {
            var entity = new Student()
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                About = student.About,
                EnrollmentDate = student.EnrollmentDate
            };

            studentDbContext.Students.Add(entity);
            await studentDbContext.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var entity = new Student()
            {
                Id = studentId
            };
            studentDbContext.Students.Attach(entity);
            studentDbContext.Students.Remove(entity);
            await studentDbContext.SaveChangesAsync();
        }


    }
}
