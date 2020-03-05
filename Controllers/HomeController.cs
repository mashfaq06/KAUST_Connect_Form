using KAUSTConnectForm.HelperClasses;
using KAUSTConnectForm.Models;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Web.Mvc;

namespace KAUSTConnectForm.Controllers
{
	public class HomeController : Controller
	{
		//Classes represent helper classes that include Scripts for Active Directory, Mail and Encryption/Decryption
		Classes Classes = new Classes();
		//KC represents the databse table
		KCEntities KC = new KCEntities();
		//HttpLink is the default link specified in web config that is used to send email
		static string HttpLink = ConfigurationManager.ConnectionStrings["Link"].ToString();

		//Default Page
		//Includes options to be directed to all other pages
		public ActionResult HomePage()
		{
			return View();
		}

		//Manager Approval page
		[HttpGet]
		public ActionResult ManagerApproval(string id, string mng)
		{
			string DecryptedManager = Classes.Decrypt(mng);
			string teamMember = System.Web.HttpContext.Current.User.Identity.Name;
			var user = teamMember.Remove(0, 6);
			var approver = Classes.SearchActiveDirectoryForViewStatusApprovers3(user);
			if (approver != null || teamMember.Contains(DecryptedManager))
			{
				string ID = Classes.Decrypt(id);
				string ManagerUsername = Classes.Decrypt(mng);
				ViewBag.Manager = ManagerUsername;
				int DecryptedID = Convert.ToInt32(ID);
				TempData["ID"] = DecryptedID;
				Table_NewRequest table = KC.Table_NewRequest.Find(DecryptedID);
				TempData["FullName"] = table.FirstName + " " + table.LastName;
				TempData["ManagerEmail"] = table.ManagerEmail;
				TempData["Requester"] = table.Requester;
				TempData["RequesterEmail"] = table.RequesterEmail;
				TempData["Type"] = table.NewOrRenew;
				if (table == null)
				{
					return View();
				}
				var Action = table.StatusOfRequest;
				if (Action != "Pending Manager Approval")
				{
					ViewBag.Hide = true;
				}
				return View(table);
			}
			else
			{
				return RedirectToAction("AccessDenied");
			}
		}

		[HttpPost]
		public ActionResult ManagerApproval( Table_NewRequest table_NewRequest, string Submit)
		{
			if (Submit == "Reject")
			{
				table_NewRequest.ApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
				table_NewRequest.ApprovedOn = DateTime.Now.ToString();
				table_NewRequest.StatusOfRequest = "Rejected by Manager";
				table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
				KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
				KC.SaveChanges();
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string FullName = Convert.ToString(TempData["FullName"]);
				string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
				string Requester = Convert.ToString(TempData["Requester"]);
				string Type = Convert.ToString(TempData["Type"]);
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by your manager. The reason for rejection is <br /> <font color='red'>" + table_NewRequest.Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);
				return RedirectToAction("ManagerRejected");
			}
			else 
			{
				table_NewRequest.ApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
				table_NewRequest.ApprovedOn = DateTime.Now.ToString();
				table_NewRequest.StatusOfRequest = "Pending Messaging Team Approval";
				table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
				KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
				KC.SaveChanges();
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string FullName = Convert.ToString(TempData["FullName"]);
				string Type = Convert.ToString(TempData["Type"]);
				string ManagerEmail = Convert.ToString(TempData["ManagerEmail"]);
				string Requester = Convert.ToString(TempData["Requester"]);
				string ViewLink = String.Format("<a href=\"{0}Home/TeamApproval?id={1}\">Click here</a>", HttpLink, EncryptedID);
				string message = "Dear Messaging Team, <br /><br /> KAUST Connect Account request has been submitted by <u>" + Requester + " </u> for <strong>" + FullName + "</strong> please review and approve the request by visit URL below: <br /> <br /> " + ViewLink + "<br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail("messagingservices@kaust.edu.sa", message, FullName, Type);
				return RedirectToAction("ManagerApproved");

			}
		}

		public ActionResult ManagerApproved()
		{
			return View();
		}

		//public void ManagerApprove(string Job)
		//{
		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string EncryptedID = Classes.Encrypt(ID.ToString());
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	KC.sp_MngApproval(ID, "Pending Messaging Team Approval", Job, ActionBy, DateAndTime);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string ManagerEmail = Convert.ToString(TempData["ManagerEmail"]);
		//	string Requester = Convert.ToString(TempData["Requester"]);
		//	string ViewLink = String.Format("<a href=\"{0}Home/TeamApproval?id={1}\">Click here</a>", HttpLink, EncryptedID);
		//	string message = "Dear Messaging Team, <br /><br /> KAUST Connect Account request has been submitted by <u>" + Requester + " </u> for <strong>" + FullName + "</strong> please review and approve the request by visit URL below: <br /> <br /> " + ViewLink + "<br /> <br /> Regards, <br /> Messaging Team";
		//	Classes.SendMail("messagingservices@kaust.edu.sa", message, FullName, Type);
		//}

		//public void ManagerRejection(string Comments)
		//{

		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	KC.sp_MngRejection(ID, "Rejected by Manager", ActionBy, DateAndTime, Comments);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
		//	string Requester = Convert.ToString(TempData["Requester"]);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by your manager. The reason for rejection is <br /> <font color='red'>" + Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
		//	Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);

		//}

		public ActionResult ManagerRejected()
		{
			return View();
		}


		[HttpGet]
		public ActionResult TeamApproval(string id)
		{
			string teamMember = System.Web.HttpContext.Current.User.Identity.Name;
			var user = teamMember.Remove(0, 6);
			var approver = Classes.SearchActiveDirectoryForKAUSTApprovers(user);
			if (approver != null)
			{

				string ID = Classes.Decrypt(id);
				int DecryptedID = Convert.ToInt32(ID);
				TempData["ID"] = DecryptedID;
				Table_NewRequest table = KC.Table_NewRequest.Find(DecryptedID);
				TempData["FullName"] = table.FirstName + " " + table.LastName;
				ViewBag.ManagerEmail = table.ManagerEmail;
				TempData["Email"] = table.RequesterEmail;
				TempData["Requester"] = table.Requester;
				TempData["RequesterEmail"] = table.RequesterEmail;
				TempData["Type"] = table.NewOrRenew;
				if (table == null)
				{
					return View();
				}
				var Action = table.StatusOfRequest;
				if (Action != "Pending Messaging Team Approval" && Action != "Pending Manager Approval" && Action != "Pending Reporting Manager Approval")
				{
					ViewBag.Hide = true;
				}
				return View(table);
			}
			else
			{
				return RedirectToAction("AccessDenied");
			}
		}


		[HttpPost]
		public ActionResult TeamApproval(Table_NewRequest table_NewRequest, string Submit)
		{
			if (Submit == "Reject")
			{
				table_NewRequest.MsgTeamApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
				table_NewRequest.MsgTeamApprovedOn = DateTime.Now.ToString();
				table_NewRequest.StatusOfRequest = "Rejected by Messaging Team";
				table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
				KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
				KC.SaveChanges();
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string FullName = Convert.ToString(TempData["FullName"]);
				string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
				string Requester = Convert.ToString(TempData["Requester"]);
				string Type = Convert.ToString(TempData["Type"]);
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by Messaging Team. The reason for rejection is <br /> <font color='red'>" + table_NewRequest.Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);
				return RedirectToAction("TeamRejected");
			}
			else
			{
				string Type = Convert.ToString(TempData["Type"]);
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string Requester = Convert.ToString(TempData["Requester"]);
				string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
				string ViewLink = String.Format("<a href=\"{0}Home/ViewForm?id={1}\">Click here</a>", HttpLink, EncryptedID);
				string StatusLink = String.Format("<a href=\"{0}Home/ViewStatus\">Click here</a>", HttpLink);
				string FullName = Convert.ToString(TempData["FullName"]);
				if (Type == "New")
				{
					table_NewRequest.MsgTeamApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
					table_NewRequest.MsgTeamApprovedOn = DateTime.Now.ToString();
					table_NewRequest.StatusOfRequest = "Approved by Messaging Team";
					table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
					KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
					KC.SaveChanges();
					string body = "Dear IThelpDesk, <br /><br /> Please create a ticket on behalf of <strong><font color='darkblue'>" + Requester + "</font></strong> to process the account request for <strong><font color='darkblue'>" + FullName + " (" + table_NewRequest.ExistingUsername + ") </font> </strong>. <br /><br /> Click on the link below to view the details <br /> " + ViewLink + "<br /><br /> To check the status for all submitted accounts, kindly " + StatusLink + "<br /><br />Regards, <br /> Messaging Team.";
					Classes.SendMail("messagingservices@kaust.edu.sa", body, FullName, Type);
					Classes.SendMail("ithelpdesk@kaust.edu.sa", body, FullName, Type);
				}
				else
				{
					table_NewRequest.MsgTeamApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
					table_NewRequest.MsgTeamApprovedOn = DateTime.Now.ToString();
					table_NewRequest.StatusOfRequest = "Approved and Completed";
					table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
					KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
					KC.SaveChanges();
					string body = "Dear " + Requester + ", <br /><br />The account request for <strong><font color='darkblue'>" + FullName + "</font></strong> is <u>completed</u> and new expiry date is <b>" + table_NewRequest.ExpiryDate + "</b> (yyyy-mm-dd) <br /><br />For any clarification please send an email to ithelpdesk@kaust.edu.sa <br /><br />Regards, <br /> Messaging Team.";
					Classes.ITMail("messagingservices@kaust.edu.sa", body, FullName, Type);
					Classes.ITMail(RequesterEmail, body, FullName, Type);

				}
				return RedirectToAction("TeamApproved");

			}
		}

		public ActionResult TeamApproved()
		{
			return View();
		}

		//public void TeamApprove(string Job, string FirstName, string MiddleName, string LastName, string VendorName, string KAUSTID, string ExistingUsername, string ExpiryDate, string NewEmail)
		//{
		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string EncryptedID = Classes.Encrypt(ID.ToString());
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	int KID = Convert.ToInt32(KAUSTID);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string Requester = Convert.ToString(TempData["Requester"]);
		//	string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
		//	string ViewLink = String.Format("<a href=\"{0}Home/ViewForm?id={1}\">Click here</a>", HttpLink, EncryptedID);
		//	string StatusLink = String.Format("<a href=\"{0}Home/ViewStatus\">Click here</a>", HttpLink);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	if (Type == "New")
		//	{
		//		KC.sp_CreateExistingID(ID, ExistingUsername, FirstName, MiddleName, LastName, KID, Job, VendorName, NewEmail, ExpiryDate);
		//		KC.sp_MsgApproval(ID, ActionBy, DateAndTime, "Approved by Messaging Team");
		//		string body = "Dear IThelpDesk, <br /><br /> Please create a ticket on behalf of <strong><font color='darkblue'>" + Requester + "</font></strong> to process the account request for <strong><font color='darkblue'>" + FullName + " (" + ExistingUsername + ") </font> </strong>. <br /><br /> Click on the link below to view the details <br /> " + ViewLink + "<br /><br /> To check the status for all submitted accounts, kindly " + StatusLink + "<br /><br />Regards, <br /> Messaging Team.";
		//		Classes.SendMail("messagingservices@kaust.edu.sa", body, FullName, Type);
		//		//Classes.SendMail("ithelpdesk@kaust.edu.sa", body, FullName, Type);
		//	}
		//	else
		//	{
		//		KC.sp_CreateExistingID(ID, ExistingUsername, FirstName, MiddleName, LastName, KID, Job, VendorName, NewEmail, ExpiryDate);
		//		KC.sp_MsgApproval(ID, ActionBy, DateAndTime, "Approved and Completed");
		//		string body = "Dear " + Requester + ", <br /><br />The account request for <strong><font color='darkblue'>" + FullName + "</font></strong> is <u>completed</u> and new expiry date is <b>" + ExpiryDate + "</b> (yyyy-mm-dd) <br /><br />For any clarification please send an email to ithelpdesk@kaust.edu.sa <br /><br />Regards, <br /> Messaging Team.";
		//		Classes.ITMail("messagingservices@kaust.edu.sa", body, FullName, Type);
		//		Classes.ITMail(RequesterEmail, body, FullName, Type);

		//	}
		//}

		//public void TeamRejection(string Comments)
		//{

		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	KC.sp_MsgRejection(ID, "Rejected by Messaging Team ", ActionBy, DateAndTime, Comments);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
		//	string Requester = Convert.ToString(TempData["Requester"]);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by Messaging Team. The reason for rejection is <br /> <font color='red'>" + Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
		//	Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);

		//}

		public ActionResult TeamRejected()
		{
			return View();
		}

		public ActionResult ViewForm(string id)
		{
			string ID = Classes.Decrypt(id);
			int EncryptedID = Convert.ToInt32(ID);
			Table_NewRequest table = KC.Table_NewRequest.Find(EncryptedID);
			if (table == null)
			{
				return View();
			}
			return View(table);
		}

		[HttpGet]
		public ActionResult ViewStatus(int? page)
		{
			var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
			var user = currentUser.Remove(0, 6);
			var approver1 = Classes.SearchActiveDirectoryForViewStatusApprovers1(user);
			var approver2 = Classes.SearchActiveDirectoryForViewStatusApprovers2(user);
			var approver3 = Classes.SearchActiveDirectoryForKAUSTApprovers(user);
			var approver4 = Classes.SearchActiveDirectoryForViewStatusApprovers4(user);
			if (approver1 != null || approver2 != null || approver3 != null || approver4 != null)
			{
				Session["ViewStatusApprover"] = "Valid";
				var model = KC.Table_NewRequest.Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				int pageSize = 10;
				int pageNumber = (page ?? 1);
				return View(model.ToPagedList(pageNumber, pageSize));
			}
			else
			{
				return RedirectToAction("AccessDenied");
			}

		}

		[HttpPost]
		public ActionResult ViewStatus(int? page, string SearchString, string currentFilter, string Status)
		{
			string approver = Session["ViewStatusApprover"].ToString();
			int pageSize = 10;
			int pageNumber = (page ?? 1);
			if (approver == "Valid")
			{

				if (SearchString != null)
				{
					page = 1;
				}
				else
				{
					SearchString = currentFilter;
				}
				ViewBag.CurrentFilter = SearchString;
				ViewBag.Status = Status;
				if (!String.IsNullOrEmpty(Status) && (Status != "--Select Status--" && Status != "All"))
				{
					var model = KC.Table_NewRequest.Where(s => s.StatusOfRequest.Contains(Status)).Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));

				}
				else if (!String.IsNullOrEmpty(SearchString))
				{
					var model = KC.Table_NewRequest.Where(s => s.ExistingUsername.Contains(SearchString) || s.ID.ToString().Contains(SearchString) || s.KAUSTID.ToString().Contains(SearchString) || s.ManagerUsername.Contains(SearchString) || s.ManagerName.Contains(SearchString) || s.FirstName.Contains(SearchString) || s.LastName.Contains(SearchString)).Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));
				}

				else
				{
					var model = KC.Table_NewRequest.Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));
				}
			}
			else
			{
				return RedirectToAction("AccessDenied");
			}
		}

		[HttpGet]
		public ActionResult MySubmissions(int? page)
		{
			string user = System.Web.HttpContext.Current.User.Identity.Name;
			string userid = user.Remove(0, 6);
			SearchResult value = Classes.SearchActiveDirectoryByUserName(userid);
			string RequesterEmail = value.Properties.Contains("mail") ? Convert.ToString(value.Properties["mail"][0]) : "no-email";
			Session["RequesterEmail"] = RequesterEmail;
			var model = KC.Table_NewRequest.Where(s => s.RequesterEmail.Contains(RequesterEmail)).Select(s => new Data
			{
				ID = s.ID,
				Username = s.ExistingUsername,
				FirstName = s.FirstName,
				LastName = s.LastName,
				Department = s.ManagerDept,
				ManagerName = s.ManagerName,
				Status = s.StatusOfRequest
			}).OrderByDescending(s => s.ID).ToList();
			int pageSize = 10;
			int pageNumber = (page ?? 1);
			return View(model.ToPagedList(pageNumber, pageSize));
		}


		[HttpPost]
		public ActionResult MySubmissions(int? page, string SearchString, string currentFilter, string Status)
		{
			string RequesterEmail = Session["RequesterEmail"].ToString();
			int pageSize = 10;
			int pageNumber = (page ?? 1);

			if (SearchString != null)
			{
				page = 1;
			}
			else
			{
				SearchString = currentFilter;
			}
			ViewBag.CurrentFilter = SearchString;
			ViewBag.Status = Status;
			if (!String.IsNullOrEmpty(Status) && (Status != "--Select Status--" && Status != "All"))
			{
				var model = KC.Table_NewRequest.Where(s => s.StatusOfRequest.Contains(Status) && s.RequesterEmail.Contains(RequesterEmail)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));

			}
			else if (!String.IsNullOrEmpty(SearchString))
			{
				var model = KC.Table_NewRequest.Where(s => (s.ExistingUsername.Contains(SearchString) || s.ID.ToString().Contains(SearchString) || s.KAUSTID.ToString().Contains(SearchString) || s.ManagerUsername.Contains(SearchString) || s.ManagerName.Contains(SearchString) || s.FirstName.Contains(SearchString) || s.LastName.Contains(SearchString)) && s.RequesterEmail.Contains(RequesterEmail)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}

			else
			{
				var model = KC.Table_NewRequest.Where(s => s.RequesterEmail.Contains(RequesterEmail)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}
		}


		[HttpGet]
		public ActionResult MyApprovals(int? page)
		{
			var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
			var user = currentUser.Remove(0, 6);
			Session["user"] = user;
			var model = KC.Table_NewRequest.Where(s => s.ManagerUsername.Contains(user)).Select(s => new Data
			{
				ID = s.ID,
				Username = s.ExistingUsername,
				FirstName = s.FirstName,
				LastName = s.LastName,
				Department = s.ManagerDept,
				ManagerName = s.ManagerName,
				ManagerUsername = s.ManagerUsername,
				Status = s.StatusOfRequest
			}).OrderByDescending(s => s.ID).ToList();
			int pageSize = 10;
			int pageNumber = (page ?? 1);
			return View(model.ToPagedList(pageNumber, pageSize));
		}


		[HttpPost]
		public ActionResult MyApprovals(int? page, string SearchString, string currentFilter, string Status)
		{
			string user = Session["user"].ToString();
			int pageSize = 10;
			int pageNumber = (page ?? 1);

			if (SearchString != null)
			{
				page = 1;
			}
			else
			{
				SearchString = currentFilter;
			}
			ViewBag.CurrentFilter = SearchString;
			ViewBag.Status = Status;
			if (!String.IsNullOrEmpty(Status) && (Status != "--Select Status--" && Status != "All"))
			{
				var model = KC.Table_NewRequest.Where(s => s.StatusOfRequest.Contains(Status) && s.ManagerUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));

			}
			else if (!String.IsNullOrEmpty(SearchString))
			{
				var model = KC.Table_NewRequest.Where(s => (s.ExistingUsername.Contains(SearchString) || s.ID.ToString().Contains(SearchString) || s.KAUSTID.ToString().Contains(SearchString) || s.ManagerUsername.Contains(SearchString) || s.ManagerName.Contains(SearchString) || s.FirstName.Contains(SearchString) || s.LastName.Contains(SearchString)) && s.ManagerUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}

			else
			{
				var model = KC.Table_NewRequest.Where(s => s.ManagerUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}
		}

		[HttpGet]
		public ActionResult ReportingManagerApprovals(int? page)
		{
			var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
			var user = currentUser.Remove(0, 6);
			Session["user"] = user;
			var model = KC.Table_NewRequest.Where(s => s.ReportingMngUsername.Contains(user)).Select(s => new Data		
			{
				ID = s.ID,
				Username = s.ExistingUsername,
				FirstName = s.FirstName,
				LastName = s.LastName,
				Department = s.ManagerDept,
				ManagerName = s.ManagerName,
				ManagerUsername = s.ManagerUsername,
				Status = s.StatusOfRequest
			}).OrderByDescending(s => s.ID).ToList();
			int pageSize = 10;
			int pageNumber = (page ?? 1);
			return View(model.ToPagedList(pageNumber, pageSize));
		}

		[HttpPost]
		public ActionResult ReportingManagerApprovals(int? page, string SearchString, string currentFilter, string Status)
		{
			string user = Session["user"].ToString();
			int pageSize = 10;
			int pageNumber = (page ?? 1);

			if (SearchString != null)
			{
				page = 1;
			}
			else
			{
				SearchString = currentFilter;
			}
			ViewBag.CurrentFilter = SearchString;
			ViewBag.Status = Status;
			if (!String.IsNullOrEmpty(Status) && (Status != "--Select Status--" && Status != "All"))
			{
				var model = KC.Table_NewRequest.Where(s => s.StatusOfRequest.Contains(Status) && s.ReportingMngUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));

			}
			else if (!String.IsNullOrEmpty(SearchString))
			{
				var model = KC.Table_NewRequest.Where(s => (s.ExistingUsername.Contains(SearchString) || s.KAUSTID.ToString().Contains(SearchString) || s.ManagerUsername.Contains(SearchString) || s.ManagerName.Contains(SearchString) || s.FirstName.Contains(SearchString) || s.LastName.Contains(SearchString) || s.ID.ToString().Contains(SearchString)) && s.ReportingMngUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}

			else
			{
				var model = KC.Table_NewRequest.Where(s => s.ReportingMngUsername.Contains(user)).Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					ManagerUsername = s.ManagerUsername,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				return View(model.ToPagedList(pageNumber, pageSize));
			}
		}
		[HttpGet]
		public ActionResult Index(int? page)
		{
			var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
			var user = currentUser.Remove(0, 6);
			var approver = Classes.SearchActiveDirectoryForKAUSTApprovers(user);
			if (approver != null)
			{
				Session["Approver"] = "Valid";
				var model = KC.Table_NewRequest.Select(s => new Data
				{
					ID = s.ID,
					Username = s.ExistingUsername,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Department = s.ManagerDept,
					ManagerName = s.ManagerName,
					Status = s.StatusOfRequest
				}).OrderByDescending(s => s.ID).ToList();
				int pageSize = 10;
				int pageNumber = (page ?? 1);
				return View(model.ToPagedList(pageNumber, pageSize));

			}
			else
			{
				return RedirectToAction("AccessDenied");
			}


		}

		[HttpPost]
		public ActionResult Index(int? page, string SearchString, string currentFilter, string Status)
		{
			string approver = Session["Approver"].ToString();
			int pageSize = 10;
			int pageNumber = (page ?? 1);
			if (approver == "Valid")
			{

				if (SearchString != null)
				{
					page = 1;
				}
				else
				{
					SearchString = currentFilter;
				}
				ViewBag.CurrentFilter = SearchString;
				ViewBag.Status = Status;
				if (!String.IsNullOrEmpty(Status) && (Status != "--Select Status--" && Status != "All"))
				{
					var model = KC.Table_NewRequest.Where(s => s.StatusOfRequest.Contains(Status)).Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));

				}
				else if (!String.IsNullOrEmpty(SearchString))
				{
					var model = KC.Table_NewRequest.Where(s => s.ExistingUsername.Contains(SearchString) || s.ManagerUsername.Contains(SearchString) || s.ManagerName.Contains(SearchString) || s.FirstName.Contains(SearchString) || s.LastName.Contains(SearchString) || s.ID.ToString().Contains(SearchString)).Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));
				}

				else
				{
					var model = KC.Table_NewRequest.Select(s => new Data
					{
						ID = s.ID,
						Username = s.ExistingUsername,
						FirstName = s.FirstName,
						LastName = s.LastName,
						Department = s.ManagerDept,
						ManagerName = s.ManagerName,
						Status = s.StatusOfRequest
					}).OrderByDescending(s => s.ID).ToList();
					return View(model.ToPagedList(pageNumber, pageSize));
				}
			}
			else
			{
				return RedirectToAction("AccessDenied");
			}
		}

		public void ExportToExcel()
		{
			var dataList = KC.Table_NewRequest.ToList();

			ExcelPackage pck = new ExcelPackage();
			ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Requests");
			ws.Cells["A1"].Value = "Type";
			ws.Cells["B1"].Value = "SamAccountName";
			ws.Cells["C1"].Value = "FirstName";
			ws.Cells["D1"].Value = "MiddleName";
			ws.Cells["E1"].Value = "LastName";
			ws.Cells["F1"].Value = "PasswordRecoveryEmail";
			ws.Cells["G1"].Value = "NewEmail/ExistingEmail";
			ws.Cells["H1"].Value = "EmployeeNo";
			ws.Cells["I1"].Value = "Validity";
			ws.Cells["J1"].Value = "Vendor";
			ws.Cells["K1"].Value = "JobTitle";
			ws.Cells["L1"].Value = "DepartmentName";
			ws.Cells["M1"].Value = "ManagerName";
			ws.Cells["N1"].Value = "RequesterEmail";
			ws.Cells["O1"].Value = "MobileNumber";
			ws.Cells["P1"].Value = "MessasingApprovalTime";

			int rowStart = 2;
			foreach (var item in dataList)
			{
				ws.Cells[string.Format("A{0}", rowStart)].Value = item.NewOrRenew;
				ws.Cells[string.Format("B{0}", rowStart)].Value = item.ExistingUsername;
				ws.Cells[string.Format("C{0}", rowStart)].Value = item.FirstName;
				ws.Cells[string.Format("D{0}", rowStart)].Value = item.MiddleName;
				ws.Cells[string.Format("E{0}", rowStart)].Value = item.LastName;
				ws.Cells[string.Format("F{0}", rowStart)].Value = item.Email;
				ws.Cells[string.Format("G{0}", rowStart)].Value = item.NewEmail;
				ws.Cells[string.Format("H{0}", rowStart)].Value = item.KAUSTID;
				ws.Cells[string.Format("I{0}", rowStart)].Value = item.ExpiryDate;
				ws.Cells[string.Format("J{0}", rowStart)].Value = item.VendorName;
				ws.Cells[string.Format("K{0}", rowStart)].Value = item.JobTitle;
				ws.Cells[string.Format("L{0}", rowStart)].Value = item.ManagerDept;
				ws.Cells[string.Format("M{0}", rowStart)].Value = item.ManagerUsername;
				ws.Cells[string.Format("N{0}", rowStart)].Value = item.LoginDetailsSentTo;
				ws.Cells[string.Format("O{0}", rowStart)].Value = item.PhoneNumber;
				ws.Cells[string.Format("P{0}", rowStart)].Value = item.MsgTeamApprovedOn;
				rowStart++;
			}

			ws.Cells["A:AZ"].AutoFitColumns();
			Response.Clear();
			Response.AddHeader("content-disposition", "attachment;filename=Requests.xlsx");
			Response.ContentType = "application/vnd.ms-excel";
			Response.BinaryWrite(pck.GetAsByteArray());
			Response.End();
		}

		public ActionResult AccessDenied()
		{
			return View();
		}

		public ActionResult ErrorOccured()
		{
			return View();
		}

		public ActionResult SearchDirectory(string username)
		{
			if (username != "")
			{
				SearchResult value;

				if (username.Contains("@"))
				{
					value = Classes.SearchActiveDirectoryBymail(username);
					return FillData(value);

				}
				var result = int.TryParse(username, out int KID);
				if (result == true)
				{
					value = Classes.SearchActiveDirectoryByIDno(username);
					return FillData(value);
				}
				else
				{
					value = Classes.SearchActiveDirectoryByUserName(username);
					return FillData(value);
				}
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}
		}

		private JsonResult FillData(SearchResult value)
		{
			if (value != null)
			{
				var details = new Details
				{
					user = value.Properties.Contains("samaccountname") ? Convert.ToString(value.Properties["samaccountname"][0]) : string.Empty,
					FirstName = value.Properties.Contains("givenName") ? Convert.ToString(value.Properties["givenName"][0]) : string.Empty,
					MiddleInitial = value.Properties.Contains("initials") ? Convert.ToString(value.Properties["initials"][0]) : string.Empty,
					LastName = value.Properties.Contains("sn") ? Convert.ToString(value.Properties["sn"][0]) : string.Empty,
					Job = value.Properties.Contains("title") ? Convert.ToString(value.Properties["title"][0]) : string.Empty,
					Vendor = value.Properties.Contains("company") ? Convert.ToString(value.Properties["company"][0]) : string.Empty,
					KAUSTID = value.Properties.Contains("extensionAttribute15") ? Convert.ToString(value.Properties["extensionAttribute15"][0]) : string.Empty,
					Email = value.Properties.Contains("mail") ? Convert.ToString(value.Properties["mail"][0]).ToLower() : string.Empty
				};
				return Json(details);
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}
		}

		public ActionResult SearchManager(string username)
		{

			if (username != "")
			{
				SearchResult value;

				if (username.Contains("@"))
				{
					value = Classes.SearchActiveDirectoryforManagerMail(username);
					return FillManager(value);

				}
				var result = int.TryParse(username, out int KID);
				if (result == true)
				{
					value = Classes.SearchActiveDirectoryforManagerKID(username);
					return FillManager(value);
				}
				else
				{
					value = Classes.SearchActiveDirectoryforManager(username);
					return FillManager(value);
				}
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}

		}

		public ActionResult SearchReportingManager(string username)
		{

			if (username != "")
			{
				SearchResult value;

				if (username.Contains("@"))
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartnersMail(username);
					return FillManager(value);

				}
				var result = int.TryParse(username, out int KID);
				if (result == true)
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartnersId(username);
					return FillManager(value);
				}
				else
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartners(username);
					return FillManager(value);
				}
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}

		}

		public ActionResult SearchReportingKAUSTManager(string username)
		{

			if (username != "")
			{
				SearchResult value;

				if (username.Contains("@"))
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartnersApproverMail(username);
					return FillManager(value);

				}
				var result = int.TryParse(username, out int KID);
				if (result == true)
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartnersApproverId(username);
					return FillManager(value);
				}
				else
				{
					value = Classes.SearchActiveDirectoryForKAUSTPartnerApprovers(username);
					return FillManager(value);
				}
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}

		}

		private JsonResult FillManager(SearchResult value)
		{
			if (value != null)
			{
				var details = new Details

				{
					user = value.Properties.Contains("samaccountname") ? Convert.ToString(value.Properties["samaccountname"][0]) : string.Empty,
					ManagerName = value.Properties.Contains("displayName") ? Convert.ToString(value.Properties["displayName"][0]) : string.Empty,
					ManagerDept = value.Properties.Contains("department") ? Convert.ToString(value.Properties["department"][0]) : string.Empty,
					ManagerEmail = value.Properties.Contains("mail") ? Convert.ToString(value.Properties["mail"][0]).ToLower() : string.Empty,
					ManagerID = value.Properties.Contains("extensionAttribute15") ? Convert.ToString(value.Properties["extensionAttribute15"][0]) : string.Empty,
					ManagerTitle = value.Properties.Contains("title") ? Convert.ToString(value.Properties["title"][0]) : string.Empty,


				};
				return Json(details);
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}
		}

		public ActionResult SearchExistingMail(string username)
		{
			if (username != "")
			{
				SearchResult value;
				value = Classes.SearchActiveDirectoryBymail(username);
				return FillData(value);
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}
		}

		public ActionResult SearchExistingUsername(string username)
		{
			if (username != "")
			{
				SearchResult value;
				value = Classes.SearchActiveDirectoryByUserName(username);
				return FillData(value);
			}
			else
			{
				var details = new Details
				{
					user = "null"
				};
				return Json(details);
			}
		}


	}

	public class Details
	{
		public string user { get; set; }
		public string FirstName { get; set; }
		public string MiddleInitial { get; set; }
		public string LastName { get; set; }
		public string Job { get; set; }
		public string Vendor { get; set; }
		public string Email { get; set; }
		public string KAUSTID { get; set; }
		public string ManagerUsername { get; set; }
		public string ManagerName { get; set; }
		public string ManagerDept { get; set; }
		public string ManagerEmail { get; set; }
		public string ManagerID { get; set; }
		public string ManagerTitle { get; set; }

	}

	public class Data
	{
		public int ID { get; set; }
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Department { get; set; }
		public string ManagerName { get; set; }
		public string ManagerUsername { get; set; }
		public string Status { get; set; }

	}
}