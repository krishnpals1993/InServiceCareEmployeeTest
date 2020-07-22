﻿using JqueryDataTables.ServerSide.AspNetCoreWeb.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeTest.Models
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class LoginViewModel
    {

        [Required(ErrorMessage = "Please enter username")]
        [EmailAddress(ErrorMessage = "Please enter valid email")]
        public string Username { get; set; }


        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
    }

    public class QuestionViewModel
    {
        public QuestionViewModel()
        {
            TestList = new List<TestViewModel>();
        }
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Please enter question")]
        public string Question { get; set; }

        [Required(ErrorMessage = "Please enter choice 1")]
        public string Choice1 { get; set; }

        [Required(ErrorMessage = "Please enter choice 2")]
        public string Choice2 { get; set; }

        public string Choice3 { get; set; }

        public string Choice4 { get; set; }


        public string Choice5 { get; set; }

        [Required(ErrorMessage = "Please select answer")]
        public int Answer { get; set; }

        [Required(ErrorMessage = "Please select test")]
        public int TestId { get; set; }

        public List<TestViewModel> TestList { get; set; }
        public string TestName { get; set; }
        public string SeasonName { get; set; }


    }

    public class EmployeeViewModel
    {
        public EmployeeViewModel()
        {
            TestList = new List<CareGiverTestViewModel>();
        }
        public List<CareGiverTestViewModel> TestList { get; set; } 
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeNo { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public int TotalQuestion { get; set; }
        public bool IsLocked { get; set; }
        public bool sendEmail { get; set; }
        public int UserTestId { get; set; }
        public int QuestionsAttend { get; set; }
        public int QuestionsRight { get; set; }
        public decimal? Video1 { get; set; }
        public decimal? Video2 { get; set; }
        public decimal? Video3 { get; set; }
        public decimal? VideoDuration { get; set; }
        public int PassingPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public int Totaltest { get; set; }
        public int PassedTest { get; set; }



    }

    public class CareGiverTestViewModel {
        
        public int TestId { get; set; }
        public string Name { get; set; }
        public int TotalQuestion { get; set; }
        public bool IsLocked { get; set; }
        public int UserTestId { get; set; }
        public int QuestionsAttend { get; set; }
        public int QuestionsRight { get; set; }
        public decimal? VideoDuration { get; set; }
        public int PassingPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public int Score { get; set; }
    }

    public class ForgetPasswordViewModel
    {

        [Required(ErrorMessage = "Please enter an email")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

    }

    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Please enter an email")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [Display(Name = "Password")]
        [MaxLength(15, ErrorMessage = "Password should be less then or equal to 15 characters long")]
        [MinLength(4, ErrorMessage = "Password at least 4 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password")]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }

    public class ResetPasswordViewModel
    {

        [Required(ErrorMessage = "Please enter an email")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [Display(Name = "Password")]
        [MaxLength(15, ErrorMessage = "Password should be less then or equal to 15 characters long")]
        [MinLength(6, ErrorMessage = "Password at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password")]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }

    public class UserTestViewModel
    {

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

    public class UserTestAnswerViewModel
    {

        public int Id { get; set; }
        public int UserTestId { get; set; }
        public int QuestionId { get; set; }
        public int UserSelected { get; set; }
        public bool IsRight { get; set; }

    }

    public class TestQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int SequenceId { get; set; }
        public string Question { get; set; }
        public string Choice1 { get; set; }
        public string Choice2 { get; set; }
        public string Choice3 { get; set; }
        public string Choice4 { get; set; }
        public string Choice5 { get; set; }
        public int Answer { get; set; }
        public int UserAnswer { get; set; }
        public int SeqNo { get; set; }
        public int MaxSequenceId { get; set; }
        public int MinSequenceId { get; set; }
        public int QuestionCount { get; set; }
        public int CorrectCount { get; set; }
        public string CareGiverName { get; set; }
        public string CareGiverEmail { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; }
    }

    public class CareGiverVideoViewModel
    {
        public VideoViewModel Videos { get; set; }
        public List<DocumentViewModel> Documents { get; set; }

        public CareGiverVideoViewModel()
        {
            Videos = new VideoViewModel();
            Documents = new List<DocumentViewModel>();
        }

    }

    public class VideoViewModel
    {
        public decimal? Video1 { get; set; }
        public decimal? Video2 { get; set; }
        public decimal? Video3 { get; set; }
        public decimal? Video4 { get; set; }
        public decimal? Video5 { get; set; }
        public decimal? Video6 { get; set; }
        public decimal? Video7 { get; set; }
        public decimal? Video8 { get; set; }
        public decimal? Video9 { get; set; }
        public decimal? Video10 { get; set; }
        public decimal? Video11 { get; set; }
        public decimal? Video12 { get; set; }
        public decimal? Video13 { get; set; }
        public decimal? Video14 { get; set; }
        public decimal? Video15 { get; set; }
        public decimal? Video16 { get; set; }
        public decimal? Video1Duration { get; set; }
        public decimal? Video2Duration { get; set; }
        public decimal? Video3Duration { get; set; }
        public decimal? Video4Duration { get; set; }
        public decimal? Video5Duration { get; set; }
        public decimal? Video6Duration { get; set; }
        public decimal? Video7Duration { get; set; }
        public decimal? Video8Duration { get; set; }
        public decimal? Video9Duration { get; set; }
        public decimal? Video10Duration { get; set; }
        public decimal? Video11Duration { get; set; }
        public decimal? Video12Duration { get; set; }
        public decimal? Video13Duration { get; set; }
        public decimal? Video14Duration { get; set; }
        public decimal? Video15Duration { get; set; }
        public decimal? Video16Duration { get; set; }
    }

    public class AttendantViewModel
    {
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


    public class UserViewModel
    {

        public UserViewModel()
        {
            RoleList = new List<RoleViewModel>();
        }

        public List<RoleViewModel> RoleList { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter an email")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [Display(Name = "Password")]
        [MaxLength(15, ErrorMessage = "Password should be less then or equal to 15 characters long")]
        [MinLength(6, ErrorMessage = "Password at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password")]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select role")]
        [StringLength(50)]
        public string RoleId { get; set; }
        public int iRoleId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string RoleName { get; set; }
    }


    public class RoleViewModel
    {

        public int RoleId { get; set; }

        public string Rolename { get; set; }

        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class DocumentViewModel
    {
        public DocumentViewModel()
        {
            SeasonList = new List<SeasonViewModel>();
        }
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<SeasonViewModel> SeasonList { get; set; }
    }

    public class TestPassingPercentageViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Percentage")]
        [Required(ErrorMessage = "Please enter percentage")]
        [Range(1, 100, ErrorMessage = "Percentage should be  between 1 to 100")]
        public decimal Value { get; set; }
        public int TestId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class SeasonViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter season name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please select saeson start day")]
        public DateTime StartDay { get; set; }
        [Required(ErrorMessage = "Please select season end day")]
        public DateTime EndDay { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }

    public class TestViewModel
    {
        public TestViewModel()
        {
            SeasonList = new List<SeasonViewModel>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter test name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select season")]
        [Display(Name = "Season")]
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<SeasonViewModel> SeasonList { get; set; }
        public string Status { get;  set; }
    }

}