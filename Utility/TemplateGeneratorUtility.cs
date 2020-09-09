using EmployeeTest.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTest.Utility
{
    public class TemplateGeneratorUtility
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        public TemplateGeneratorUtility(IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
        }
        public string GetHTMLString(EmployeeViewModel model)
        {
            var sb = new StringBuilder();
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);

            sb.AppendFormat(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h4>Standard Precautions HIPPA <br/> Emergency Preparedness Home Safety</h4>
                                <h4>CareGiver: {0} {1} {2}</h4>
                                </div>
                               ", model.FirstName, model.MiddleName ?? "", model.LastName);

            foreach (var item in model.TestList)
            {
                var userAnswer = _testUtility.GetUserQuestionsWithAnswer(item.UserTestId);
                sb.AppendFormat(@" 
<div class='testheader'>Test: {0}</div>
<table align='center'>
                                    <tr>
                                <th>   Status  </th>
                                <th>  Question   </th>
                                <th> Choice1 </th>
                                <th> Choice2 </th>
                                <th>  Choice3 </th>
                                <th>Choice4</th>
                                <th>Choice5</th>
                                <th>Correct Answer</th>
                                <th>User Given</th>
                                    </tr>", item.Name);
                foreach (var row in userAnswer)
                {
                    sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                     
                                  </tr>",
                                      (row.Answer == row.UserAnswer ? "Right" : "Wrong"),
                                      row.Question,
                                      row.Choice1,
                                      row.Choice2,
                                      row.Choice3,
                                      row.Choice4,
                                      row.Choice5,
                                      (row.Answer == 1 ? row.Choice1 : (row.Answer == 2 ? row.Choice2 : (row.Answer == 3 ? row.Choice3 : (row.Answer == 4 ? row.Choice4 : row.Choice5)))),
                                      (row.UserAnswer == 1 ? row.Choice1 : (row.UserAnswer == 2 ? row.Choice2 : (row.UserAnswer == 3 ? row.Choice3 : (row.UserAnswer == 4 ? row.Choice4 : row.Choice5))))
                                      );
                }

                sb.Append(@"</table>");

            }


            sb.Append(@"</body>
                        </html>");

            return sb.ToString();

        }

        public string GetHTMLStringForAll(List<PrintPDFViewModel> list)
        {
            var sb = new StringBuilder();
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var userIdList = list.Select(s => s.UserId).Distinct().ToList();
            //< div class='header'><h4>Standard Precautions HIPPA<br/> Emergency Preparedness Home Safety</h4>
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body style='margin-bottom:15px'>
                               ");



            foreach (var userId in userIdList)
            {
                var userTests = list.Where(w => w.UserId == userId).ToList();
                var userDetail = userTests.FirstOrDefault();

               

                var userTestsIdList = list.Where(w => w.UserId == userId).Select(s => s.TestId).Distinct().ToList();

                foreach (var testId in userTestsIdList)
                {
                    var userTestList = list.Where(w => w.UserId == userId && w.TestId == testId).ToList();
                    var testDetail = userTestList.FirstOrDefault();
                    sb.AppendFormat(@" 

<div class='testheader'>CareGiver: <span class='cname'>{0} {1} {2}</span> Test: <span class='tname'>{3}</span><br/>  Complete Date:<span class='tname'> {4} </span> Status: <span class='tname'>{5} </span>
<table align='center'>
                                    <tr>
                                <th>   Status  </th>
                                <th>  Question   </th>
                                <th> Choice1 </th>
                                <th> Choice2 </th>
                                <th>  Choice3 </th>
                                <th>Choice4</th>
                                <th>Choice5</th>
                                <th>Correct Answer</th>
                                <th>User Given</th>
                                    </tr>", userDetail.FirstName, userDetail.MiddleName ?? "", userDetail.LastName, testDetail.TestName, testDetail.EndDate.ToString("MM/dd/yyyy"),
                                  testDetail.Pass == 1 ? "Passed" : "Failed");

                    foreach (var row in userTestList)
                    {
                        sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                     
                                  </tr>",
                                          (row.Answer == row.UserAnswer ? "Right" : "Wrong"),
                                          row.Question,
                                          row.Choice1,
                                          row.Choice2,
                                          row.Choice3,
                                          row.Choice4,
                                          row.Choice5,
                                          (row.Answer == 1 ? row.Choice1 : (row.Answer == 2 ? row.Choice2 : (row.Answer == 3 ? row.Choice3 : (row.Answer == 4 ? row.Choice4 : row.Choice5)))),
                                          (row.UserAnswer == 1 ? row.Choice1 : (row.UserAnswer == 2 ? row.Choice2 : (row.UserAnswer == 3 ? row.Choice3 : (row.UserAnswer == 4 ? row.Choice4 : row.Choice5))))
                                          );
                    }


                    sb.Append(@"</table></div>");
                }





              

            }


            sb.Append(@"</body>
                        </html>");

            return sb.ToString();

        }
    }
}
