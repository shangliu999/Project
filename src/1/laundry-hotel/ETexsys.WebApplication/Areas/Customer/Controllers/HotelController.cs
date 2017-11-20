﻿using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Customer.Models;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    [Authorize(Roles = "Sys_User")]

    public class HotelController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }
        
        // GET: Customer/Hotel
        [AddressUrl]
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<brandtype> brandList = i_brandtype.Entities.Where(v => v.IsDelete == false).ToList();

            ViewData["BrandList"] = brandList;
            ViewData["RightID"] = rightId;

            sys_user sys_user = LoginUserManage.GetInstance().GetLoginUser();
            //StoreType 1租赁仓库 2周转仓库 3采购仓库
            int storeType = 3;
            int storeID = 0;
            if (sys_user != null && sys_user.StoreType != null)
            {
                storeType = (int)sys_user.StoreType;
                storeID = (int)sys_user.StoreID;
            }
            List<region> regionList = null;
            if (storeType == 3)
            {
                regionList = i_region.Entities.Where(c => c.RegionType == 3 && c.RegionMode == 1).ToList();
            }
            else
            {
                regionList = i_region.Entities.Where(c => c.ID == storeID).ToList();
            }
            ViewData["RegionList"] = regionList;

            return View();
        }

        [AddressUrl]
        public JsonResult GetHotelList()
        {
            MsgModel msgModel = new MsgModel();

            var query = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false);

            sys_user sys_user = LoginUserManage.GetInstance().GetLoginUser();
            //StoreType 1租赁仓库 2周转仓库 3采购仓库
            int storeType = 3;
            int storeID = 0;
            if (sys_user != null && sys_user.StoreType != null)
            {
                storeType = (int)sys_user.StoreType;
                storeID = (int)sys_user.StoreID;
            }

            if (storeType != 3)
            {
                query = query.Where(c => c.StoreID == storeID);
            }

            List<region> hotelList = query.OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();

            msgModel.Result = hotelList;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddHotel(region requestParam)
        {
            MsgModel msgModel = new MsgModel();

            try
            {
                if (Request.Files.Count > 0)
                {
                    #region Logo

                    string savepath = @"upload\logo";
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
                        requestParam.LogoUrl = savepath + filename;
                    }
                    #endregion
                }
                requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                requestParam.CreateTime = DateTime.Now;
                requestParam.IsDelete = false;
                requestParam.RegionType = 1;
                int rtn = i_region.Insert(requestParam);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }
            catch
            {
                msgModel.ResultCode = 1;
            }


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateHotel(region requestParam)
        {
            MsgModel msgModel = new MsgModel();
            region model = i_region.GetByKey(requestParam.ID);
            if (Request.Files.Count > 0)
            {
                #region Logo

                string savepath = @"upload\logo";
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
                    model.LogoUrl = savepath + filename;
                }
                #endregion
            }

            model.StoreID = requestParam.StoreID;
            model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            model.UpdateTime = DateTime.Now;
            model.RegionName = requestParam.RegionName;
            model.FullName = requestParam.FullName;
            model.BrandID = requestParam.BrandID;
            model.RegionMode = requestParam.RegionMode;
            model.LinkMan = requestParam.LinkMan;
            model.Tel = requestParam.Tel;
            model.Address = requestParam.Address;
            model.Lng = requestParam.Lng;
            model.Lat = requestParam.Lat;
            model.HandConfirm = requestParam.HandConfirm;
            int rtn = i_region.Update(model);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [AddressUrl]
        public ActionResult GetHotel(int hotelId)
        {
            MsgModel msgModel = new MsgModel();

            region model = i_region.GetByKey(hotelId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        [AddressUrl]
        public JsonResult CheckHotelName(string HotelName, int HotelID)
        {
            //id为0表示新增
            bool result = false;
            if (HotelID > 0)
            {
                int count = i_region.LoadEntities(v => v.ID != HotelID && v.RegionName == HotelName && v.IsDelete == false && v.RegionType == 1).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_region.LoadEntities(v => v.RegionName == HotelName && v.IsDelete == false && v.RegionType == 1).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AddressUrl]
        public ActionResult BasicData()
        {
            int cusId = 0;
            int.TryParse(Request.Params["CusID"], out cusId);

            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            int userId = LoginUserManage.GetInstance().GetLoginUserId();
            List<sys_right> rightList = LoginUserManage.GetInstance().GetCurrentRight(userId).Where(v => v.RightParentID == rightId).ToList();
            ViewData["RightList"] = rightList;

            List<brandtype> brandList = i_brandtype.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["BrandList"] = brandList;

            sys_user sys_user = LoginUserManage.GetInstance().GetLoginUser();
            //StoreType 1租赁仓库 2周转仓库 3采购仓库
            int storeType = 3;
            int storeID = 0;
            if (sys_user != null && sys_user.StoreType != null)
            {
                storeType = (int)sys_user.StoreType;
                storeID = (int)sys_user.StoreID;
            }
            List<region> regionList = null;
            if (storeType == 3)
            {
                regionList = i_region.Entities.Where(c => c.RegionType == 3 && c.RegionMode == 1).ToList();
            }
            else
            {
                regionList = i_region.Entities.Where(c => c.ID == storeID).ToList();
            }
            ViewData["RegionList"] = regionList;

            region model = i_region.GetByKey(cusId);

            return View(model);
        }

        [AddressUrl]
        public ActionResult Settings()
        {
            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false).OrderBy(v => v.DeliveryTime).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();

            return View(hotelList);
        }

        [AddressUrl]
        public ActionResult UpdateSettings(RegionModel requestParam)
        {
            MsgModel msgModel = new MsgModel();

            int id = 0;
            int rtn = 0;
            for (int i = 0; i < requestParam.Data.Count; i++)
            {
                id = requestParam.Data[i].ID;
                region reg = i_region.Entities.SingleOrDefault(v => v.ID == id);
                reg.DeliveryTime = requestParam.Data[i].DeliveryTime;
                if (i >= requestParam.Data.Count - 1)
                {
                    rtn += i_region.Update(reg);
                }
                else
                {
                    rtn += i_region.Update(reg, false);
                }
            }

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}