using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ETexsys.WebApplication.Areas.SysAdmin.Controllers
{
    [Authorize(Roles = "Sys_Admin")]
    public class SysAdminController : Controller
    {
        #region 对象属性

        [Dependency]
        public IRepository<sys_right> sys_right_ibr { get; set; }

        [Dependency]
        public IRepository<sys_application> sys_app_ibr { get; set; }

        [Dependency]
        public IRepository<sys_right_button> sys_right_btn { get; set; }

        [Dependency]
        public IRepository<sys_customer> sys_cus { get; set; }

        [Dependency]
        public IRepository<sys_customer_right> sys_cus_right { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        #endregion

        #region Sys_Customer

        // GET: SysAdmin/SysAdmin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetSys_CustomerList(ParamModel model)
        {
            var query = sys_cus.Entities;

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.ID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSys_Customer(int ID)
        {
            MsgModel msgModel = new MsgModel();

            sys_customer model = sys_cus.GetByKey(ID);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSys_Customer(sys_customer requestParam)
        {
            MsgModel msgModel = new MsgModel();

            try
            {
                if (Request.Files.Count > 0)
                {
                    #region Logo

                    string savepath = @"upload\\logo";
                    if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + savepath))
                    {
                        System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + savepath);
                    }

                    HttpPostedFileBase file = Request.Files[0];
                    if (file.FileName != "")
                    {
                        Random seed = new Random();
                        int j = seed.Next(0, 9);
                        string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + j.ToString() + Path.GetExtension(file.FileName);
                        filename = @"\\" + filename;
                        //检查上传文件是否有相同文件名
                        if (System.IO.File.Exists(savepath + filename))
                        {
                            int i = 1;
                            while (System.IO.File.Exists(savepath + filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."))))
                            {
                                i++;
                            }

                            filename = filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."));
                        }

                        file.SaveAs(AppDomain.CurrentDomain.BaseDirectory + savepath + filename);
                        requestParam.Logo = savepath + filename;
                    }
                    #endregion
                }

                int rtn = sys_cus.Insert(requestParam);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }
            catch
            {
                msgModel.ResultCode = 1;
            }


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSys_Customer(sys_customer requestParam)
        {
            MsgModel msgModel = new MsgModel();
            if (Request.Files.Count > 0)
            {
                #region Logo

                string savepath = @"upload\\logo";
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + savepath))
                {
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + savepath);
                }

                HttpPostedFileBase file = Request.Files[0];
                if (file.FileName != "")
                {
                    Random seed = new Random();
                    int j = seed.Next(0, 9);
                    string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + j.ToString() + Path.GetExtension(file.FileName);
                    filename = @"\\" + filename;
                    //检查上传文件是否有相同文件名
                    if (System.IO.File.Exists(savepath + filename))
                    {
                        int i = 1;
                        while (System.IO.File.Exists(savepath + filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."))))
                        {
                            i++;
                        }

                        filename = filename.Substring(0, filename.LastIndexOf(".")) + "(" + i + ")" + filename.Substring(filename.LastIndexOf("."));
                    }

                    file.SaveAs(AppDomain.CurrentDomain.BaseDirectory + savepath + filename);
                    requestParam.Logo = savepath + filename;
                }
                #endregion
            }
            int rtn = sys_cus.Update(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult GetCustomerSysRightList(int cusId)
        {
            var query = sys_right_ibr.Entities;

            var orderingQuery = query.OrderBy(q => q.ApplicationID).ThenBy(q => q.RightParentID).ThenBy(q => q.RightCode);

            var cusright = sys_cus_right.Entities.Where(v => v.CusID == cusId);

            var data = from A in orderingQuery
                       join cr in cusright on A.RightID equals cr.RightID into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {

                           A.sys_application.ApplicationName,
                           A.ApplicationID,
                           A.RightCode,
                           A.RightIcon,
                           A.RightID,
                           A.RightName,
                           A.RightParentID,
                           A.RightSort,
                           A.RightType,
                           A.RightUrl,
                           A.ShowInMainMenu,
                           IsCheck = t == null ? false : true
                       };
            var d = data.ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetSysCusRight(int cusId, string rightIds)
        {
            MsgModel msgModel = new MsgModel();

            sys_cus_right.Delete(p => p.CusID == cusId, false);

            List<sys_customer_right> list = new List<sys_customer_right>();
            sys_customer_right model = null;

            string[] str = rightIds.Split('|');
            foreach (var s in str)
            {
                int id = 0;
                int.TryParse(s, out id);
                if (id > 0)
                {
                    model = new sys_customer_right();
                    model.CusID = cusId;
                    model.RightID = id;
                    list.Add(model);
                }
            }

            sys_cus_right.Insert(list);

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddUser(sys_user requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.IsAdmin = true;
            requestParam.IsLevel = false;
            requestParam.UserType = 1;
            int rtn = i_sys_user.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckUserName(string loginName)
        {
            //id为0表示新增
            bool result = false;

            int count = i_sys_user.LoadEntities(v => v.LoginName == loginName).Count();
            result = count > 0 ? false : true;

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Sys_Right

        public ActionResult Sys_Right()
        {
            List<sys_application> appList = sys_app_ibr.Entities.ToList();
            ViewData["AppList"] = appList;
            return View();
        }

        public ActionResult GetSys_RightList(ParamModel model)
        {
            var query = sys_right_ibr.Entities;

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(q => q.ApplicationID).ThenBy(q => q.RightCode).Skip(model.iDisplayStart).Take(model.iDisplayLength);
            var data = from A in orderingQuery
                       select new
                       {
                           A.sys_application.ApplicationName,
                           A.ApplicationID,
                           A.RightCode,
                           A.RightIcon,
                           A.RightID,
                           A.RightName,
                           A.RightParentID,
                           A.RightSort,
                           A.RightType,
                           A.RightUrl,
                           A.ShowInMainMenu
                       };
            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = data
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSys_Right(int RightID)
        {
            MsgModel msgModel = new MsgModel();

            sys_right model = sys_right_ibr.GetByKey(RightID);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSys_Right(sys_right requestParam)
        {
            MsgModel msgModel = new MsgModel();
            if (requestParam.RightParentID == null)
            {
                string code = sys_right_ibr.LoadEntities(v => v.RightParentID == null).Max(v => v.RightCode);
                int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                requestParam.RightCode = (count + 1).ToString().PadLeft(2, '0');
            }
            else
            {
                string code = sys_right_ibr.LoadEntities(v => v.RightParentID == requestParam.RightParentID).Max(v => v.RightCode);
                int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                sys_right model = sys_right_ibr.LoadEntities(v => v.RightID == requestParam.RightParentID).FirstOrDefault();
                requestParam.RightCode = model.RightCode + (count + 1).ToString().PadLeft(2, '0');
            }
            int rtn = sys_right_ibr.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSys_Right(sys_right requestParam)
        {
            MsgModel msgModel = new MsgModel();

            sys_right rightModel = sys_right_ibr.Entities.Where(v => v.RightID == requestParam.RightID).FirstOrDefault();

            if (rightModel != null)
            {
                if (rightModel.RightParentID != requestParam.RightParentID)
                {
                    if (requestParam.RightParentID == null)
                    {
                        string code = sys_right_ibr.LoadEntities(v => v.RightParentID == null).Max(v => v.RightCode);
                        int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                        rightModel.RightCode = (count + 1).ToString().PadLeft(2, '0');
                    }
                    else
                    {
                        string code = sys_right_ibr.LoadEntities(v => v.RightParentID == requestParam.RightParentID).Max(v => v.RightCode);
                        int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                        sys_right model = sys_right_ibr.LoadEntities(v => v.RightID == requestParam.RightParentID).FirstOrDefault();
                        rightModel.RightCode = model.RightCode + (count + 1).ToString().PadLeft(2, '0');
                    }
                }
            }

            rightModel.ApplicationID = requestParam.ApplicationID;
            rightModel.RightIcon = requestParam.RightIcon;
            rightModel.RightName = requestParam.RightName;
            rightModel.RightParentID = requestParam.RightParentID;
            rightModel.RightSort = requestParam.RightSort;
            rightModel.RightType = requestParam.RightType;
            rightModel.RightUrl = requestParam.RightUrl;
            rightModel.ShowInMainMenu = requestParam.ShowInMainMenu;

            int rtn = sys_right_ibr.Update(rightModel);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelSys_Right(int RightID)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = sys_right_ibr.Delete(RightID);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult SettingRightBtn(int RightID, bool Add, bool Update, bool Del, bool Setting)
        {
            MsgModel msgModel = new MsgModel();

            sys_right_btn.Delete(p => p.RightID == RightID, false);

            List<sys_right_button> list = new List<sys_right_button>();
            sys_right_button btnModel = null;

            if (Add)
            {
                btnModel = new sys_right_button();
                btnModel.RightID = RightID;
                btnModel.BtnClass = "btn btn-primary";
                btnModel.BtnCode = "Add";
                btnModel.BtnIcon = "fa fa-plus";
                btnModel.BtnName = "添加";
                btnModel.BtnScript = "AddModule";
                list.Add(btnModel);
            }
            if (Update)
            {
                btnModel = new sys_right_button();
                btnModel.RightID = RightID;
                btnModel.BtnClass = "btn btn-warning";
                btnModel.BtnCode = "Update";
                btnModel.BtnIcon = "fa fa-pencil";
                btnModel.BtnName = "修改";
                btnModel.BtnScript = "UpdateModule";
                list.Add(btnModel);
            }
            if (Del)
            {
                btnModel = new sys_right_button();
                btnModel.RightID = RightID;
                btnModel.BtnClass = "btn btn-danger";
                btnModel.BtnCode = "Delete";
                btnModel.BtnIcon = "fa fa-trash-o";
                btnModel.BtnName = "删除";
                btnModel.BtnScript = "DeleteModule";
                list.Add(btnModel);
            }
            if (Setting)
            {
                btnModel = new sys_right_button();
                btnModel.RightID = RightID;
                btnModel.BtnClass = "btn btn-success";
                btnModel.BtnCode = "Setting";
                btnModel.BtnIcon = "fa fa-gear";
                btnModel.BtnName = "设置";
                btnModel.BtnScript = "SettingModule";
                list.Add(btnModel);
            }

            sys_right_btn.Insert(list);

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult GetSettingRightList(int rightID)
        {
            MsgModel msgModel = new MsgModel();

            List<sys_right_button> list = sys_right_btn.LoadEntities(v => v.RightID == rightID).ToList();

            msgModel.ResultCode = 0;
            msgModel.Result = list;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;

        }

        public JsonResult CheckRightName(string rightName, int rightId)
        {
            //id为0表示新增
            bool result = false;
            if (rightId > 0)
            {
                int count = sys_right_ibr.LoadEntities(v => v.RightID != rightId && v.RightName == rightName).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = sys_right_ibr.LoadEntities(v => v.RightName == rightName).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Application

        public ActionResult Sys_Application()
        {
            return View();
        }

        public ActionResult GetApplicationList(ParamModel model)
        {
            var query = sys_app_ibr.Entities;

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(q => q.ApplicationID).Skip(model.iDisplayStart).Take(model.iDisplayLength);
            var data = from A in orderingQuery
                       select new
                       {
                           A.ApplicationID,
                           A.ApplicationCode,
                           A.ApplicationDesc,
                           A.ApplicationName,
                           A.ShowInMenu
                       };
            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = data
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetApplication(int ApplicationID)
        {
            MsgModel msgModel = new MsgModel();

            sys_application model = sys_app_ibr.GetByKey(ApplicationID);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSys_Application(sys_application requestParam)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = sys_app_ibr.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSys_Application(sys_application requestParam)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = sys_app_ibr.Update(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelSys_Application(int ApplicationID)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = sys_app_ibr.Delete(ApplicationID);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckApplicationName(string appName, int appId)
        {
            //id为0表示新增
            bool result = false;
            if (appId > 0)
            {
                int count = sys_app_ibr.LoadEntities(v => v.ApplicationID != appId && v.ApplicationName == appName).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = sys_app_ibr.LoadEntities(v => v.ApplicationName == appName).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult LogoutLogin()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("sysadmin", "login", new { area = "" });
        }
    }
}