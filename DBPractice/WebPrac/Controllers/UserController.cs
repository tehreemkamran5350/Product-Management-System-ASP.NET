using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPrac.Security;
using PMS.DAL;
namespace WebPrac.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult UserScreen(string pic)
        {
           
        if (WebPrac.Security.SessionManager.User == null)
        {
            return Redirect("~/User/Login");
        }
 
            PMS.Entities.UserDTO u = PMS.DAL.UserDAO.GetUserByPicture(pic);
            return View(u);
        }
        [HttpGet]
        public ActionResult SignUp()
        {
            return View("signUp");
        }
        public ActionResult EditProfile()
        {
            if (WebPrac.Security.SessionManager.User == null)
            {
                return Redirect("~/User/Login");
            }
            var id = WebPrac.Security.SessionManager.User.UserID;
            var user = PMS.DAL.UserDAO.GetUserById(id);
            TempData["name"] = user.Name;
            TempData["login"] = user.Login;
            TempData["pic"] = user.PictureName;
            TempData["email"] = user.Email;
            TempData["uId"] = id;
            return View("EditProfile");
        }

        [HttpPost]
        public ActionResult Login(String login, String password)
        {
            var obj = PMS.BAL.UserBO.ValidateUser(login, password);
            if (obj != null)
            {
                Session["user"] = obj;
                if (obj.IsAdmin == true)
                    return Redirect("~/Home/Admin");
                else
                    return Redirect("~/Home/NormalUser");
            }

            Response.Write("<script>alert('invalid Login/Password')</script>");
            ViewBag.Login = login;

            return View();
        }

        [HttpPost]
        public ActionResult Register(UserDTO dto)
        {
            var file = Request.Files["PictureName"];
            var name = "";
            if (file != null)
            {
                if (file.FileName != "")
                {
                    var ext = System.IO.Path.GetExtension(file.FileName);
                    name = Guid.NewGuid().ToString() + ext;
                    var rootPath = Server.MapPath("~/UploadedFiles");
                    var fileSavePath = System.IO.Path.Combine(rootPath, name);
                    file.SaveAs(fileSavePath);
                    dto.PictureName = name;
                }
            }
            dto.IsAdmin = false;
            dto.IsActive = false;
            if (PMS.DAL.UserDAO.Save(dto) > 0) 
            {
                Response.Write("<script>alert('signUp successfully')</script>");
            }
            return View("Login");
        }

        [HttpPost]
        public ActionResult Save(UserDTO dto)
        {
            //User Save Logic
            var file = Request.Files["PictureName"];
            var name = "";
            if (file != null)
            {
                if (file.FileName != "")
                {
                    var ext = System.IO.Path.GetExtension(file.FileName);
                    name = Guid.NewGuid().ToString() + ext;
                    var rootPath = Server.MapPath("~/UploadedFiles");
                    var fileSavePath = System.IO.Path.Combine(rootPath, name);
                    file.SaveAs(fileSavePath);
                    dto.PictureName = name;
                }
            }
            if (PMS.DAL.UserDAO.Save(dto) > 0)
            {
                Response.Write("<script>alert('profile updated')</script>");
            }
            
                Session["user"] = dto;
                if (dto.IsAdmin == true)
                    return Redirect("~/Home/Admin");
                else
                    return Redirect("~/Home/NormalUser");
            
        }
        [HttpPost]
        public ActionResult SendEmail(String Email)
        {
            if (Email == "")
            {
                Response.Write("<script>alert('please the field!')</script>");
                return View("ExistingUser");
            }
            Random r = new Random();
            int c = r.Next(100, 1000);
            String code = c.ToString();
            if (PMS.BAL.UserBO.sendEmail(Email, "Recovery Code", "Your recovery code is " + code))
            {
                Session["code"] = code;
                Session["updateEmail"] = Email;
                return View("ResetCode");
            }
            else
            {
                Response.Write("<script>alert('some problem has occured!')</script>");
                return View("ForgotPassword");
            }
        }
   
        public ActionResult ForgotPassword()
        {
           
                return View("ForgotPassword");
        }
        [HttpPost]
        public ActionResult UpdatePassword(String code)
        {
            var c = Session["code"];
            if (c.Equals(code))
            {
                return View("updatePassword");
            }
            else
            {
                Response.Write("<script>alert('code does not match!')</script>");
                return View("ResetCode");
            }
        }
        [HttpPost]
        public ActionResult Update(String password)
        {
            var e = (String)Session["updateEmail"];
            PMS.Entities.UserDTO u=null;
            if(e!=null)
            {
                u = PMS.DAL.UserDAO.GetUserByEmail(e);
            }
            else
                u = PMS.DAL.UserDAO.GetUserById(@WebPrac.Security.SessionManager.User.UserID);
            u.Password = password;
    
            if (PMS.DAL.UserDAO.UpdatePassword(u)>0)
            {
                if (u.IsAdmin == true)
                {
                    Session["updateEmail"] = null;
                    return Redirect("~/Home/Admin");
                }
                else
                    return Redirect("~/Home/NormalUser");
            }
            else
            {
                Response.Write("<script>alert('password is not reset!')</script>");
                return View("UpdatePassword");
            }
        }
        [HttpGet]
        public ActionResult Logout()
        {
            SessionManager.ClearSession();
            return RedirectToAction("Login");
        }
        public ActionResult ChangePassword()
        {
            
            return View();
        }
        [HttpPost]
        public ActionResult Change(string oldPswd,int uid)
        {
            if (PMS.DAL.UserDAO.GetUserById(uid)!=null)
            {
                return View("UpdatePassword");
            }
            else
                return View("ChangePassword");
        }

        [HttpGet]
        public ActionResult Login2()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ValidateUser(String login, String password)
        {

            Object data = null;

            try
            {
                var url = "";
                var flag = false;

                var obj = PMS.BAL.UserBO.ValidateUser(login, password);
                if (obj != null)
                {
                    flag = true;
                    SessionManager.User = obj;

                    if (obj.IsAdmin == true)
                        url = Url.Content("~/Home/Admin");
                    else
                        url = Url.Content("~/Home/NormalUser");
                }

                data = new
                {
                    valid = flag,
                    urlToRedirect = url
                };
            }
            catch (Exception)
            {
                data = new
                {
                    valid = false,
                    urlToRedirect = ""
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
	}
}