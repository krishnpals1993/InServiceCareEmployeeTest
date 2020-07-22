using Microsoft.Extensions.Options;
using EmployeeTest.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace EmployeeTest.Utility
{
    public class TestUtility
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        public TestUtility(IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
        }

        public List<TestQuestionViewModel> GetUserQuestions(int userId,int TestId)
        {
            List<TestQuestionViewModel> questionList = new List<TestQuestionViewModel>();
            var sNo = 0;
            questionList = (from question in _dbContext.tbl_Questions
                            where question.TestId == TestId
                            select new TestQuestionViewModel
                            {
                                QuestionId = question.Id,
                                Question = question.Question,
                                Choice1 = question.Choice1,
                                Choice2 = question.Choice2,
                                Choice3 = question.Choice3,
                                Choice4 = question.Choice4,
                                Choice5 = question.Choice5,
                                Answer = question.Answer,

                            }).ToList();

            var answeres = (from user_test in _dbContext.tbl_UserTests
                            join user_test_answer in _dbContext.tbl_UserTestAnswers
                            on user_test.Id equals user_test_answer.UserTestId
                            where user_test.UserId == userId && user_test.IsReset == false
                            select new TestQuestionViewModel
                            {
                                QuestionId = user_test_answer.QuestionId,
                                UserAnswer = user_test_answer.UserSelected
                            }).ToList();

            foreach (var item in questionList)
            {
                var checkAnswer = answeres.Where(w => w.QuestionId == item.QuestionId).FirstOrDefault();
                if (checkAnswer != null)
                {
                    item.UserAnswer = checkAnswer.UserAnswer;
                }
            }


            foreach (var item in questionList)
            {
                item.SeqNo = ++sNo;
            }
            return questionList;
        }


        public List<TestQuestionViewModel> GetUserQuestionsWithAnswer(int userTestId)
        {
            List<TestQuestionViewModel> questionList = new List<TestQuestionViewModel>();
            var userDetail = (from user_test in _dbContext.tbl_UserTests
                              join careGiver in _dbContext.tbl_Attendants
                             on user_test.UserId equals careGiver.UserId
                              where user_test.Id == userTestId
                              select new TestQuestionViewModel
                              {
                                  CareGiverName = careGiver.FirstName + " " + (Convert.ToString(careGiver.MiddleName) == "" ? "" : Convert.ToString(careGiver.MiddleName) + " ") + careGiver.LastName,
                                  CareGiverEmail = careGiver.Email,
                                  CorrectCount = user_test.QuestionsRight ?? 0,
                                  TestId = user_test.TestId


                              }).FirstOrDefault();


            questionList = (from question in _dbContext.tbl_Questions
                            where question.TestId == userDetail.TestId
                            select new TestQuestionViewModel
                            {
                                QuestionId = question.Id,
                                Question = question.Question,
                                Choice1 = question.Choice1,
                                Choice2 = question.Choice2,
                                Choice3 = question.Choice3,
                                Choice4 = question.Choice4,
                                Choice5 = question.Choice5,
                                Answer = question.Answer,
                                CareGiverName = userDetail.CareGiverName,
                                CareGiverEmail = userDetail.CareGiverEmail,
                                QuestionCount = userDetail.QuestionCount,
                                CorrectCount = userDetail.CorrectCount

                            }).ToList();

            var answeres = (from user_test in _dbContext.tbl_UserTests
                            join user_test_answer in _dbContext.tbl_UserTestAnswers
                            on user_test.Id equals user_test_answer.UserTestId

                            where user_test.Id == userTestId && user_test.IsReset == false
                            select new TestQuestionViewModel
                            {
                                QuestionId = user_test_answer.QuestionId,
                                UserAnswer = user_test_answer.UserSelected,
                                QuestionCount = questionList.Count(),
                                CorrectCount = user_test.QuestionsRight ?? 0
                            }).ToList();

            foreach (var item in questionList)
            {
                var checkAnswer = answeres.Where(w => w.QuestionId == item.QuestionId).FirstOrDefault();
                item.QuestionCount = questionList.Count();
                if (checkAnswer != null)
                {
                    item.UserAnswer = checkAnswer.UserAnswer;
                }
            }



            return questionList;
        }

    }
}



