using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeTest.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
           : base(options) { }

        public DbSet<User> tbl_Users { get; set; }
        public DbSet<Role> tbl_Roles { get; set; }
        public DbSet<Attendant> tbl_Attendants { get; set; }
        public DbSet<Import_attendant> tbl_Import_attendants { get; set; }
        public DbSet<Questions> tbl_Questions { get; set; }
        public DbSet<UserTest> tbl_UserTests { get; set; }
        public DbSet<UserTestAnswer> tbl_UserTestAnswers { get; set; }
        public DbSet<Document> tbl_Documents { get; set; }
        public DbSet<TestPassingPercentage> tbl_TestPassingPercentages { get; set; }
        public DbSet<Testvideo> tbl_Testvideos { get; set; }
        public DbSet<AttendentTestVideo> tbl_AttendentTestVideos { get; set; }
        public DbSet<Season> tbl_Seasons { get; set; }
        public DbSet<Test> tbl_Tests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }

    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
       
    }

    [Table("roles")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        public string Rolename { get; set; }

        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    [Table("attendants")]
    public class Attendant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeNo { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UserId { get; set; }
        public decimal? Video1 { get; set; }
        public decimal? Video2 { get; set; }
        public decimal? Video3 { get; set; }

    }

    [Table("import_attendants")]
    public class Import_attendant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeNo { get; set; }
     
    }

    [Table("questions")]
    public class Questions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Question { get; set; }
        public string Choice1 { get; set; }
        public string Choice2 { get; set; }
        public string Choice3 { get; set; }
        public string Choice4 { get; set; }
        public string Choice5 { get; set; }
        public int Answer { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TestId { get; set; }
    }

    [Table("user_test")]
    public class UserTest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TestId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? QuestionsAttend { get; set; }
        public int? QuestionsRight { get; set; }
        public string Signature { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsReset { get; set; }
      
    }

    [Table("user_test_answers")]
    public class UserTestAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserTestId { get; set; }
        public int QuestionId { get; set; }
        public int UserSelected { get; set; }
        public bool IsRight { get; set; }

    }

    [Table("Documents")]
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? SeasonId { get; set; }
    }

    [Table("test_passing_percentage")]
    public class TestPassingPercentage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal Value { get; set; }
        public int TestId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    [Table("test_videos")]
    public class Testvideo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }
        public int TestId { get; set; }
        public decimal Duration { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    [Table("attendent_test_videos")]
    public class AttendentTestVideo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int VideoId { get; set; }
        public decimal Duration { get; set; }
        public int UserId { get; set; }


    }

    [Table("seasons")]
    public class Season
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }

    [Table("tests")]
    public class Test
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeasonId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        

    }


}