using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication16.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult CompassReport()
        {
            List<string> dataSql = new List<string>();
            ViewBag.List = dataSql;

            SqlConnection con = new SqlConnection("data source=dewdfctwsql0009,51433;initial catalog=CTW_COMPASS;integrated security=True;MultipleActiveResultSets=true;App=EntityFramework&quot;");
            con.Open();
            SqlCommand com = new SqlCommand("Select Task.RequestId, Task.Name, ProjectRequest.ProjectManagerId, ProjectRequest.ProjectSponsorId, GETDATE() AS Data_Date" +
                " FROM Task INNER JOIN ProjectRequest ON ProjectRequest.Id = Task.RequestId" +
                " WHERE(Task.IsDeleted = 0) AND(Task.SequenceNumber = 0) AND(Task.AssignationDate IS NULL) AND(Task.OwnershipStatus = 0)" +
                " AND (Task.AssignedToUserId IS NULL) AND (Task.Name NOT LIKE 'Fixed%') AND (Task.Name NOT LIKE '1Hello%')" +
                " AND (Task.Name NOT LIKE 'FM Test%') AND (Task.Name NOT LIKE 'MHE%') AND (Task.Name NOT LIKE 'FM PRD%')" +
                " AND (Task.Name NOT LIKE '---DEMO%') AND (Task.Name NOT LIKE 'Test Project') AND (Task.Name NOT LIKE 'testmhe')" +
                " AND (ProjectRequest.ProjectPhase = 2 OR ProjectRequest.ProjectPhase = 5) ORDER BY Task.RequestId", con);
            com.CommandType = CommandType.Text;
            SqlDataReader dr = com.ExecuteReader();

            string pmId, psId, projectId, title, issueScore;
            Tuple<string, string> staffed, pmUpdateStatus, riskScore, milestone;
            List<string> alreadyDisplayed = new List<string>();

            ViewBag.Message += "<table cellpadding=\"7\" id=\"projectDataTable\" border=\"1\"><tr id=\"tableHeaders\">" +
                "<th>Project Name:<br/><input type=\"text\" id=\"searchName\" oninput=\"searchProjectName()\" placeholder=\"Search table\" /></th>" +
                "<th>Project Sponsor:<br/><input type=\"text\" id=\"searchSponsor\" oninput=\"searchProjectSponsor()\" placeholder=\"Search table\" /></th>" +
                "<th>Project Manager:<br/><input type=\"text\" id=\"searchManager\" oninput=\"searchProjectManager()\" placeholder=\"Search table\" /></th>" +
                "<th>PM Update Status</th>" +
                "<th>Milestone Status</th>" +
                "<th>Staffed</th>" +
                "<th>Risk Score</th>" +
                "<th>Issue Score</th>" +
                "</tr><tbody>";

            while (dr.Read())
            {
                projectId = dr["RequestId"].ToString();
                title = dr["Name"].ToString();
                pmId = dr["ProjectManagerId"].ToString();
                psId = dr["ProjectSponsorId"].ToString();
                milestone = getMilestone(projectId, con);
                pmUpdateStatus = getLastPMUpdate(projectId, con);
                staffed = getStaffedDataLvl1(projectId, con);
                riskScore = getRiskScore(projectId, con);
                issueScore = GetIssues(projectId, con).ToString();

                ViewBag.Message += "<tr>";
                ViewBag.Message += "<td>";
                ViewBag.Message += title;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += getUserName(psId);
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += getUserName(pmId);
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td title=\"" + pmUpdateStatus.Item2 + "\">";
                ViewBag.Message += pmUpdateStatus.Item1;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td title=\"" + milestone.Item2 + "\">";
                ViewBag.Message += milestone.Item1;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td title=\"" + staffed.Item2 + "\">";
                ViewBag.Message += staffed.Item1;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td title=\"" + riskScore.Item2 + "\">";
                ViewBag.Message += riskScore.Item1;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td title=\"Issue Score: " + issueScore + "\">";
                ViewBag.Message += getIcon(issueScore, 0, 3);
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += projectId;
                ViewBag.Message += "</td>";
                ViewBag.Message += "</tr>";
            }

            ViewBag.Message += "</tbody></table>";

            dr.Close();
            con.Close();
            return View();
        }

        public string getUserName(string id)
        {
            string name = "N/A";
            SqlConnection con = new SqlConnection("data source=dewdfctwsql0009,51433;initial catalog=CTW_COMPASS;integrated security=True;MultipleActiveResultSets=true;App=EntityFramework&quot;");
            con.Open();
            SqlCommand com = new SqlCommand("Select PersonId, DisplayName FROM CTW_COMPASS.dbo.[User]", con);
            com.CommandType = CommandType.Text;
            SqlDataReader dr = com.ExecuteReader();
            while (dr.Read())
            {
                if(id == dr["PersonId"].ToString())
                {
                    name = dr["DisplayName"].ToString();
                }
            }
            dr.Close();
            con.Close();

            return name;
        }

        public ActionResult About()
        {
           /* ViewBag.Message = "";
            List<string> dataSql = new List<string>();
            ViewBag.List = dataSql;
            // Connection to SQL Database //
            SqlConnection con = new SqlConnection("data source=dewdfctwsql0009,51433;initial catalog=CTW_Compass_CSI;integrated security=True;MultipleActiveResultSets=true;App=EntityFramework&quot;");
            con.Open();
            SqlCommand com = new SqlCommand("Select ProjectId, Date, ProjectName, ProjectManager, ProjectSponsor, MilestoneStatus, MilestoneStatusText from CompassWeeklyStatus", con);
            
            
            com.CommandType = CommandType.Text;

           

            SqlDataReader dr = com.ExecuteReader();
            
            //Create variable for all table information
            string projectId, projectName, 
                projectSponsor, projectManager, 
                issueScore;
            List<string> alreadyDisplayed = new List<string>();
            Tuple<string,string> staffed,pmUpdateStatus,riskScore,milestone;
            //string[] yesterdayData; 

            /////////////////////
            /// Table creation - hard coded columns
            /////////////////////

            ViewBag.Message += "<table cellpadding=\"7\" id=\"projectDataTable\" border=\"1\"><tr id=\"tableHeaders\">" +
                "<th>Project Name:<br/><input type=\"text\" id=\"searchName\" onkeydown=\"searchProjectName()\" placeholder=\"Search table\" /></th>" +
                "<th>Project Sponsor:<br/><input type=\"text\" id=\"searchSponsor\" onkeydown=\"searchProjectSponsor()\" placeholder=\"Search table\" /></th>" +
                "<th>Project Manager:<br/><input type=\"text\" id=\"searchManager\" onkeydown=\"searchProjectManager()\" placeholder=\"Search table\" /></th>" +
                "<th>PM Update Status</th>" +
                "<th>Milestone Status</th>" +
                "<th>Staffed</th>" +
                "<th>Risk Score</th>" +
                "<th>Issue Score</th>" +
                "</tr><tbody>";
            while (dr.Read())
            {
                
                //Retrieves all data for each project
                //dataSql.Add(dr["ProjectName"].ToString());
                projectId = dr["ProjectId"].ToString();
                if (!alreadyDisplayed.Contains(projectId))
                {
                    alreadyDisplayed.Add(projectId);
                    projectName = dr["ProjectName"].ToString();
                    projectManager = dr["ProjectManager"].ToString();
                    projectSponsor = dr["ProjectSponsor"].ToString();
                    milestone = getMilestone(projectId,con);
                    pmUpdateStatus = getLastPMUpdate(projectId,con);
                    staffed = getStaffedDataLvl1(projectId, con);
                    riskScore = getRiskScore(projectId, con);
                    issueScore = GetIssues(projectId, con, "TestIssueTable2").ToString();
                    //yesterdayData = GetOldData(projectId);
                    //write milestone status function and risk level
                    
                    

                    //creates the projects row using data
                    ViewBag.Message += "<tr>";
                    ViewBag.Message += "<td>";
                    ViewBag.Message += projectName;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td>";
                    ViewBag.Message += projectSponsor;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td>";
                    ViewBag.Message += projectManager;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td title=\"" + pmUpdateStatus.Item2 + "\">";
                    ViewBag.Message += pmUpdateStatus.Item1;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td title=\"" + milestone.Item2 + "\">";
                    ViewBag.Message += milestone.Item1;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td title=\""+staffed.Item2+"\">";
                    ViewBag.Message += staffed.Item1;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td title=\"" + riskScore.Item2 + "\">";
                    ViewBag.Message += riskScore.Item1;
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "<td title=\"Issue Score: " + issueScore + "\">";
                    ViewBag.Message += getIcon(issueScore,0,3);
                    ViewBag.Message += "</td>";
                    ViewBag.Message += "</tr>";
                }
            }

            ViewBag.Message += "</tbody></table>";
            dr.Close();
            //ViewBag.List = dataSql;
            con.Close();
        */
            return View();
        }

        public Tuple<string, string> getMilestone(string projectId, SqlConnection con)
        {
            SqlCommand com = new SqlCommand("Select Count(RequestId) As CountNum, RequestId from Task" +
                " WHERE (IsDeleted = 0) AND (IsSubMilestone = 1) AND (RequestedEndDate < GETDATE()) AND (PercentComplete < 100) AND (Completed = 0)" +
                " GROUP BY RequestId", con);
            com.CommandType = CommandType.Text;
            SqlDataReader dr = com.ExecuteReader();
            string icon = "n/a";
            string tooltip = "n/a";
            
            int mMissed=0;
            string[] mText = new string[100];

            while (dr.Read())
            {
                if(projectId == dr["RequestId"].ToString())
                {
                    mMissed = Convert.ToInt32(dr["CountNum"].ToString());
                }
            }


            if (mMissed == 0)
            {
                icon = "<img align=\"middle\" src =\"../Img/Traffic_Green.png\">";
                tooltip = "Project is missing no milestones.";
            }
            else if (mMissed == 1)
            {
                icon = "<img align=\"middle\" src =\"../Img/Traffic_Yellow.png\">";
                tooltip = "Project is missing one milestone.";
            }
            else
            {
                icon = "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
                
                tooltip = "Project is missing " + mMissed + " milestones.";
               
                
            }


            return Tuple.Create(icon, tooltip);
        }

        public Tuple<string,string> getRiskScore(string projectId,SqlConnection con)
        {
            //variable declaration
            string icon = "<img  align=\"middle\" src =\"../Img/Traffic_Green.png\">";
            string tooltip = "RiskScore: n/a";

            //Makes a new connection to the CompassStaffingLevel1 database
            SqlCommand staffCon = new SqlCommand("Select MAX(Severity) AS MaxSeverity, ProjectRequestId FROM RaidRisk" +
                " WHERE (IsDeleted = 0) AND (Status = 0) GROUP BY ProjectRequestId", con);
            staffCon.CommandType = CommandType.Text;
            SqlDataReader dr = staffCon.ExecuteReader();
            while (dr.Read())
            {
                if (projectId == dr["ProjectRequestId"].ToString())
                {
                    icon = getIcon(dr["MaxSeverity"].ToString(), 4, 16);
                    tooltip = "Risk Score: " + dr["MaxSeverity"].ToString();
                }
            }
            return Tuple.Create(icon, tooltip);
        }

        public Tuple<string, string> getLastPMUpdate(string projectId, SqlConnection con)
        {
            //variable declaration
            int timeDifference = 0;
            string icon = "n/a";
            string tooltip = "n/a";

            //Makes a new connection to the CompassStaffingLevel1 database
            SqlCommand staffCon = new SqlCommand("Select MAX(ReportPeriodEndingDate) AS Last_PM_Update_Date, ProjectRequestId from ProjectReport WHERE (Status=2) GROUP BY ProjectRequestId", con);
            staffCon.CommandType = CommandType.Text;
            SqlDataReader dr = staffCon.ExecuteReader();

            //Reads through every row in the database. If it finds a row with a matching
            //project id, it then checks if the date field is empty. If it is not empty
            //it calculated how long the project is staffed for. Then if the staffed
            //time frame is greater then the previous it replace the previous max with
            //the current time frame it just calculated
            while (dr.Read())
            {
                if (projectId == dr["ProjectRequestId"].ToString())
                {
                    if (dr["Last_PM_Update_Date"].ToString() != "")
                    {
                        DateTime update = Convert.ToDateTime(dr["Last_PM_Update_Date"]);
                        string datetext = update.ToString("yyyy-MM-dd");
                        
                        string color;
                        DateTime today = DateTime.Now.ToUniversalTime();
                        timeDifference = GetBusinessDays(update,today);
                        if (timeDifference <= 7)
                        {
                            color = "green";
                            icon= "<img  align=\"middle\" src =\"../Img/Traffic_Green.png\">";
                        }
                        else if (timeDifference >= 15)
                        {
                            color = "red";
                            icon = "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
                        }
                        else
                        {
                            color = "yellow";
                            icon = "<img align=\"middle\" src =\"../Img/Traffic_Yellow.png\">";
                        }
                        tooltip = "It has been " + timeDifference.ToString() +
                            " business days since PM made an update the project therefore it is showing as " + color +
                            ". Last PM Update date was " + datetext;
                                                  


                    }
                }
            }

            return Tuple.Create(icon, tooltip);
        }

        //Figures out the how much longer a project is staffed for and returns the tooltip text and the icon
        public Tuple<string,string> getStaffedDataLvl1(string projectId, SqlConnection staffData)
        {
            //variable declaration
            double tempTimeDifference=0;
            int maxTimeDifference=0;
            bool foundData = false;
            string icon = "n/a";
            string tooltip;

            //Makes a new connection to the CompassStaffingLevel1 database
            SqlCommand staffCon = new SqlCommand("Select Id, RequestedEndDate, RequestId from Task" +
                " WHERE (IsDeleted = 0) AND (IsMilestone = 0) AND (IsSubMilestone = 0) AND (IsApprovalTask = 0) AND (IsPrimaryTask = 0) AND (IsTaskList = 0) AND (Status <> 4) AND (AssignedToUserId IS NULL)", staffData);
            staffCon.CommandType = CommandType.Text;
            SqlDataReader dr = staffCon.ExecuteReader();

            //Reads through every row in the database. If it finds a row with a matching
            //project id, it then checks if the date field is empty. If it is not empty
            //it calculated how long the project is staffed for. Then if the staffed
            //time frame is greater then the previous it replace the previous max with
            //the current time frame it just calculated
            while (dr.Read())
            {
                if (projectId == dr["RequestId"].ToString())
                {
                    
                    if (dr["RequestedEndDate"].ToString()!="")
                    {
                        foundData = true;
                        DateTime staff = dr.GetDateTime(1);
                        DateTime today = DateTime.Now.ToUniversalTime();
                        tempTimeDifference = GetBusinessDays(today, staff);
                        if (tempTimeDifference > maxTimeDifference)
                        {
                            maxTimeDifference = (int)tempTimeDifference;
                        }
                    }
                }
            }
            //If data was found it sets the icon to the appropriate color and adjusts
            //the tooltip text
            if (foundData)
            {
                icon = getIcon(maxTimeDifference, 56, 21);
                tooltip = "Project is staffed for the next " + maxTimeDifference + " days.";
            }
            //If data was not found it calls the getStaffedDataLvl2 function to check the second
            //database for staffing
            else
            {
                Tuple<string, string, bool> staffedLevel2 = getStaffedDataLvl2(projectId,staffData);
                //If the getStaffedDataLvl2 function finds data
                //then it copies the tooltip and icon the function returned to icon and tooltip
                if (staffedLevel2.Item3)
                {
                    icon = staffedLevel2.Item1;
                    tooltip = staffedLevel2.Item2;
                }
                //If nothing was found for a project tooltip and icon gets set to the below t
                else
                {
                    tooltip = "Project is not assigned to any user.";
                    icon = "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
                }
                
            }
            dr.Close();

            //returnts a tuple of the icon text and tooltip text
            return Tuple.Create(icon, tooltip);
        }

        //Used to check if a project is in the CompassStaffingLevel2 database. Returns a tuple
        //of the icon text, tooltip text, and whether it found any data. Requires a project id
        //and an established connection to the database
        public Tuple<string,string,bool> getStaffedDataLvl2(string projectId, SqlConnection con)
        {
            //variable declaration
            string icon = "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
            bool found = false;
            string tooltip = "Project is not assigned to any user.";

            //Makes a new connection to the CompassStaffingLevel2 database
            SqlCommand staffCon = new SqlCommand("Select Id, RequestId from Task" +
                " WHERE (IsDeleted = 0) AND (IsMilestone = 0) AND (IsSubMilestone = 0) AND (IsApprovalTask = 0) AND (IsPrimaryTask = 0) AND (IsTaskList = 0) AND (Status <> 4) AND (AssignedToUserId IS NOT NULL)", con);
            staffCon.CommandType = CommandType.Text;
            SqlDataReader dr = staffCon.ExecuteReader();

            //Reads through every row in the database. If it finds the project id 
            //it sets found to true, icon to green, and the tooltip text.
            while(dr.Read())
            {
                if (projectId == dr["RequestId"].ToString())
                {
                    found = true;
                    icon = "<img align=\"middle\" src =\"../Img/Traffic_Green.png\">";
                    tooltip = "Project is not assingned to an user.";
                }
            }

            //returns a tuple of the icon text, tooltip text, and whether it found
            //the data or not
            return Tuple.Create(icon, tooltip, found);
        }

        //used to get the icon image string. Requires a string containing a number,
        //the value the string must be less than to be green, and the value the string
        //must be greater than to be red.
        public string getIcon(string score, int green, int red)
        {
            int numScore = Convert.ToInt32(score);
            if (numScore <= green)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Green.png\">";
            }
            else if(numScore > green && numScore < red)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Yellow.png\">";
            }
            else if (numScore >= red)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
            }
            else
            {
                return "n/a";
            }

        }

        //used to get the icon image string. Requires an int containing a number,
        //the value the string must be greater than to be green, and the value the string
        //must be less than to be red.
        public string getIcon(int score, int green, int red)
        {
            int numScore = score;
            if (numScore >= green)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Green.png\">";
            }
            else if (numScore < green && numScore > red)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Yellow.png\">";
            }
            else if (numScore <= red)
            {
                return "<img align=\"middle\" src =\"../Img/Traffic_Red.png\">";
            }
            else
            {
                return "n/a";
            }

        }

        //This function will be used to get the old data for last week data.
        //Currently not in use. Please ignore.
        public string[] GetOldData(string projectid)
        {
            string[] oldData = new string[5];
            SqlConnection oldDataCon = new SqlConnection("data source=dewdfctwsql0009,51433;initial catalog=CTW_Compass_CSI;integrated security=True;MultipleActiveResultSets=true;App=EntityFramework&quot;");
            oldDataCon.Open();
            SqlCommand com = new SqlCommand("Select * from CompassWeeklyStatus", oldDataCon);
            com.CommandType = CommandType.Text;
            SqlDataReader dr = com.ExecuteReader();
            while (dr.Read())
            {
                if (projectid == dr["ProjectId"].ToString())
                {
                    oldData[1] = dr["MilestoneStatus"].ToString();
                    oldData[0] = dr["PMUpdateStatus"].ToString();
                    oldData[2] = dr["ProjectStaffed"].ToString();
                    oldData[3] = dr["RiskScore"].ToString();
                    //oldData[4] = GetIssues(projectid, oldDataCon, "TestIssueTable2").ToString();
                }
            }


            return oldData;
        }

        //Way to access issues database while having a read open for the main database
        //For each database you want to access at the same time you need to create another function and
        //open the database from there.
               
        public int GetIssues(string projectid, SqlConnection con)
        {
            //variable declaration
            int tempScore=0;
            int maxScore=0;

            //connection to issues database
            SqlCommand issueConnection = new SqlCommand("Select Id, Impact, Priority, ProjectRequestId from RaidIssue" +
                " WHERE (Status = 0) and (IsDeleted = 0)", con);
            issueConnection.CommandType = CommandType.Text;
            SqlDataReader issueData = issueConnection.ExecuteReader();

            //Reads lines from database. If it finds a line with that matches the project
            //id then it multiples that rows impact and priority scores to get the issue score
            //if that issue score is higher than the current max then it replaces the max
            //score with the current score
            while (issueData.Read())
            {
            if(projectid == issueData["ProjectRequestId"].ToString())
                {
                    tempScore = (Convert.ToInt32(issueData["Impact"].ToString()) * Convert.ToInt32(issueData["Priority"].ToString()));
                    if (tempScore > maxScore)
                    {
                        maxScore = tempScore;
                    }
                }
            }
            issueData.Close();

            //returns the maxscore
            return maxScore;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ProjectByStatus()
        {
            ViewBag.Message = "<tr id =\"tableHeaders\">" +
                  "<th>Project Name</th>" +
                  "<th>Phase</th>" +
                  "<th>Days Running</th>" +
                  "</tr><tbody>";

            string projectId, projectName, projectPhase;
            DateTime createdDate;
            DateTime today = DateTime.Now.ToUniversalTime();


            List<string> dataSql = new List<string>();
            ViewBag.List = dataSql;
            // Connection to SQL Database //
            SqlConnection con = new SqlConnection("data source=dewdfctwsql0009,51433;initial catalog=CTW_COMPASS;integrated security=True;MultipleActiveResultSets=true;App=EntityFramework&quot;");
            con.Open();
            SqlCommand com = new SqlCommand("Select Task.RequestId, Task.Name, Task.CreatedDate, ProjectRequest.ProjectPhase from Task, ProjectRequest where ProjectRequest.id = Task.RequestId and Task.IsDeleted = 0 and Task.IsPrimaryTask = 1" +
                "and (ProjectPhase = -1 or ProjectPhase = 1 or ProjectPhase = 2 or ProjectPhase = 3 or ProjectPhase = 4 or ProjectPhase = 5) and Task.RequestId <> 49", con);
            com.CommandType = CommandType.Text;
            SqlDataReader dr = com.ExecuteReader();
            while (dr.Read())
            {
                projectId = dr["RequestId"].ToString();
                projectPhase = dr["ProjectPhase"].ToString();
                createdDate = dr.GetDateTime(2);
                projectName = dr["Name"].ToString();
                ViewBag.Message += "<tr>";
                ViewBag.Message += "<td>";
                ViewBag.Message += projectName;
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += getPhaseText(projectPhase);
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += GetBusinessDays(createdDate,today);
                ViewBag.Message += "</td>";
                ViewBag.Message += "<td>";
                ViewBag.Message += projectId;
                ViewBag.Message += "</td>";
                ViewBag.Message += "</tr>";
            }

            ViewBag.Message += "</tbody>";
            dr.Close();
            con.Close();

            return View();
        }

        
        public string getPhaseText(string phaseNum)
        {
            string phase = "";

            if(phaseNum == "-1")
            {
                phase = "Pipeline";
            }
            else if (phaseNum == "1")
            {
                phase = "Initiation";
            }
            else if (phaseNum == "2" || phaseNum == "5")
            {
                phase = "Management";
            }
            else if (phaseNum == "3")
            {
                phase = "Closeout";
            }
            else if (phaseNum == "4")
            {
                phase = "Archived";
            }

            return phase;
        }

        public static int GetBusinessDays(DateTime firstDay, DateTime lastDay)//, params DateTime[] bankHolidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                //throw new ArgumentException("Incorrect last day " + lastDay);
                return 0;

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            //foreach (DateTime bankHoliday in bankHolidays)
            //{
               // DateTime bh = bankHoliday.Date;
               // if (firstDay <= bh && bh <= lastDay)
                   // --businessDays;
            //}

            return businessDays;
        }

    }
}