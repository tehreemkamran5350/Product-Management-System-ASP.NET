using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPrac.Security;

namespace WebPrac.Controllers
{
    public class ProductController : Controller
    {
        private ActionResult GetUrlToRedirect()
        {
            if (SessionManager.IsValidUser)
            {
                if (SessionManager.User.IsAdmin == false)
                {
                    TempData["Message"] = "Unauthorized Access";
                    return Redirect("~/Home/NormalUser");
                }
            }
            else
            {
                TempData["Message"] = "Unauthorized Access";
                return Redirect("~/User/Login");
            }

            return null;
        }
        public ActionResult ShowAll()
        {
            if (SessionManager.IsValidUser == false)
            {
                return Redirect("~/User/Login");
            }

            var products = PMS.BAL.ProductBO.GetAllProducts(true);

            return View(products);
        }

        public ActionResult New()
        {
              var redVal = GetUrlToRedirect();
              if (redVal == null)
              {
                  var dto = new ProductDTO();
                  redVal =  View(dto);
              }

              return redVal;
            //var dto = new ProductDTO();
            //return View("New",dto);
        }

        public ActionResult Edit(int id)
        {

            var redVal = GetUrlToRedirect();
            if (redVal == null)
            {
                var prod = PMS.BAL.ProductBO.GetProductById(id);
                redVal= View("New", prod);
            }

            return redVal;
            
        }
        public ActionResult Edit2(int id)
        {
            var prod = PMS.BAL.ProductBO.GetProductById(id);
            return View("New", prod);
        }
        public ActionResult Delete(int id)
        {

            var dto = PMS.DAL.ProductDAO.GetProductById(id);
            if (SessionManager.IsValidUser)
            {
                if(SessionManager.User.IsAdmin == true)
                {
                    PMS.BAL.ProductBO.DeleteProduct(id);
                    TempData["Msg"] = "Record is deleted!";
                    return RedirectToAction("ShowAll");
                }
                if (dto.CreatedBy != WebPrac.Security.SessionManager.User.UserID)
                {
                    TempData["Message"] = "Unauthorized Access";
                    return Redirect("~/Home/NormalUser");
                }
            }
            else
            {
                return Redirect("~/User/Login");
            }

            PMS.BAL.ProductBO.DeleteProduct(id);
            TempData["Msg"] = "Record is deleted!";
            return RedirectToAction("ShowAll");
        }
        [HttpPost]
        public ActionResult Save(ProductDTO dto)
        {
            //var dto = PMS.DAL.ProductDAO.GetProductById(p.ProductID);
            if (SessionManager.IsValidUser)
            {

                if (SessionManager.User.IsAdmin == true)
                {
                    var uniqueName = "";

                    if (Request.Files["Image"] != null)
                    {
                        var file = Request.Files["Image"];
                        if (file.FileName != "")
                        {
                            var ext = System.IO.Path.GetExtension(file.FileName);

                            //Generate a unique name using Guid
                            uniqueName = Guid.NewGuid().ToString() + ext;

                            //Get physical path of our folder where we want to save images
                            var rootPath = Server.MapPath("~/UploadedFiles");

                            var fileSavePath = System.IO.Path.Combine(rootPath, uniqueName);

                            // Save the uploaded file to "UploadedFiles" folder
                            file.SaveAs(fileSavePath);

                            dto.PictureName = uniqueName;
                        }
                    }



                    if (dto.ProductID > 0)
                    {
                        dto.ModifiedOn = DateTime.Now;
                        dto.ModifiedBy = WebPrac.Security.SessionManager.User.UserID;
                    }
                    else
                    {
                        dto.CreatedOn = DateTime.Now;
                        dto.CreatedBy = WebPrac.Security.SessionManager.User.UserID;
                    }

                    PMS.BAL.ProductBO.Save(dto);

                    TempData["Msg"] = "Record is saved!";

                    return RedirectToAction("ShowAll");
                }
                var p = PMS.DAL.ProductDAO.GetProductById(dto.ProductID);
                p.Name = dto.Name;
                p.Price = dto.Price;
                dto = p;
                if (dto.CreatedBy != WebPrac.Security.SessionManager.User.UserID)
                {
                    TempData["Message"] = "Unauthorized Access";
                    return Redirect("~/Home/NormalUser");
                }
            }
            else
            {
                return Redirect("~/User/Login");
            }


            var Name = "";

            if (Request.Files["Image"] != null)
            {
                var file = Request.Files["Image"];
                if (file.FileName != "")
                {
                    var ext = System.IO.Path.GetExtension(file.FileName);

                    //Generate a unique name using Guid
                    Name = Guid.NewGuid().ToString() + ext;

                    //Get physical path of our folder where we want to save images
                    var rootPath = Server.MapPath("~/UploadedFiles");

                    var fileSavePath = System.IO.Path.Combine(rootPath, Name);

                    // Save the uploaded file to "UploadedFiles" folder
                    file.SaveAs(fileSavePath);

                    dto.PictureName = Name;
                }
            }



            if (dto.ProductID > 0)
            {
                dto.ModifiedOn = DateTime.Now;
                dto.ModifiedBy = WebPrac.Security.SessionManager.User.UserID;
            }
            else
            {
                dto.CreatedOn = DateTime.Now;
                dto.CreatedBy = WebPrac.Security.SessionManager.User.UserID;
            }

            PMS.BAL.ProductBO.Save(dto);

            TempData["Msg"] = "Record is saved!";

            return RedirectToAction("ShowAll");
        }

        [HttpPost]
        public ActionResult SaveComment(int product, string textComment)
        {

            PMS.Entities.CommentDTO cmnts = new PMS.Entities.CommentDTO();
            cmnts.CommentOn = DateTime.Now;
            cmnts.CommentText = textComment;
            cmnts.ProductID = product;
            cmnts.UserID = @WebPrac.Security.SessionManager.User.UserID;
            cmnts.UserName = @WebPrac.Security.SessionManager.User.Name;
            cmnts.PictureName = @WebPrac.Security.SessionManager.User.PictureName;
            PMS.Entities.ProductDTO p = PMS.DAL.ProductDAO.GetProductById(product);
           
            if((PMS.DAL.CommentDAO.Save(cmnts))>0)
            {
                Response.Write("<script>alert('comment Added')</script>");
            }
            else
                Response.Write("<script>alert('comment not added')</script>");
            return RedirectToAction("ShowAll");
        }
    }
}