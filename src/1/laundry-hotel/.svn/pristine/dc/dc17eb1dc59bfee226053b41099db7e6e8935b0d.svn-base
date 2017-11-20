using ETexsys.Common.Rabbit;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Customer.Models;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class BillMaintainController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }
        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }
        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }
        [Dependency]
        public IRepository<invoicelog> i_invoicelog { get; set; }
        [Dependency]
        public IRepository<invoicelogdetail> i_invoicelogdetail { get; set; }
        [Dependency]
        public IRepository<size> i_size { get; set; }
        [Dependency]
        public IRepository<sys_customer> i_sys_customer { get; set; }

        public ActionResult Index()
        {
            List<region> list = i_region.Entities.Where(q => q.RegionType == 1).ToList();
            ViewData["regionlist"] = list;
            return View();
        }

        public ActionResult GetMaintainList(MaintainParamModel model)
        {
            var query = from i in i_invoice.Entities
                        join h in i_region.Entities on i.HotelID equals h.ID into j_hotel
                        from h in j_hotel.DefaultIfEmpty()
                        join r in i_region.Entities on i.RegionID equals r.ID into j_region
                        from r in j_region.DefaultIfEmpty()
                        select new
                        {
                            InvNo = i.InvNo,
                            HotelName = h.RegionName,
                            Floorparam = r.RegionName,
                            CreateUserName = i.CreateUserName,
                            CreateTime = i.CreateTime,
                            Count = i.Quantity,
                            InvType = i.InvType,
                        };
            if (model.InvType != 0)
            {
                query = query.Where(p => p.InvType == model.InvType);
            }
            if (model.HotelId != 0)
            {
                query = query.Where(p => p.HotelName == model.Hotel);
            }
            if (model.Floor != null)
            {
                query = query.Where(p => p.Floorparam == model.Floor);
            }
            if (model.Time != DateTime.MinValue)
            {
                DateTime date = model.Time.AddDays(1);
                query = query.Where(p => p.CreateTime > model.Time && p.CreateTime < date);
            }
            if (model.InvNo != null)
            {
                query = query.Where(p => p.InvNo == model.InvNo);
            }
            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.InvNo).Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
            var JsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(JsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetDetailFloor(int HotelId, int FloorId)
        {
            List<region> FoolrList = i_region.Entities.Where(p => p.ParentID == HotelId).ToList();
            var query = from r in i_region.Entities
                        where r.ParentID == HotelId
                        select new
                        {
                            ID = r.ID,
                            RegionName = r.RegionName,
                            Val = FloorId
                        };
            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDetail(string InvNo)
        {
            MsgModel msg = new MsgModel();
            var list = from v in i_invoice.Entities
                       join h in i_region.Entities on v.HotelID equals h.ID into j_hotel
                       from h in j_hotel.DefaultIfEmpty()
                       join r in i_region.Entities on v.RegionID equals r.ID into j_floor
                       from r in j_floor.DefaultIfEmpty()
                       where v.InvNo == InvNo
                       select new
                       {
                           InvNo = v.InvNo,
                           CreatTime = v.CreateTime.ToString(),
                           HotelName = h.RegionName,
                           FloorName = r.RegionName,
                           CreateUser = v.CreateUserName,
                           HotelId = h.ID,
                           FloorId = v.RegionID
                       };
            msg.OtherResult = list.FirstOrDefault();
            msg.Result = list.ToList();
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDetailTable(string InvNo)
        {
            var list = from v in i_invoicedetail.Entities
                       join r in i_region.Entities on v.RegionID equals r.ID into j_region
                       from r in j_region.DefaultIfEmpty()
                       join d in i_invoice.Entities on v.InvNo equals d.InvNo into j_detail
                       from d in j_detail.DefaultIfEmpty()
                       where v.InvNo == InvNo
                       group v by new { t0 = v.SizeName, t1 = v.ClassName } into m
                       orderby m.Key.t1, m.Key.t0
                       select new
                       {
                           SizeName = m.Key.t0,
                           ClassName = m.Key.t1,
                           count = m.Sum(p => p.TextileCount)
                       };
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFloor(int HotelId)
        {
            var query = from r in i_region.Entities
                        where r.ParentID == HotelId
                        select new
                        {
                            ID = r.ID,
                            RegionName = r.RegionName,
                        };
            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(string InvNo, int HotelId, int FloorId)
        {
            try
            {
                invoice invoice = i_invoice.Entities.Where(p => p.InvNo == InvNo).FirstOrDefault();
                List<invoicedetail> idetail = i_invoicedetail.Entities.Where(p => p.InvNo == InvNo).OrderBy(p => p.ClassName).ThenBy(p => p.SizeName).ToList();
                List<invoicelog> loglist = new List<invoicelog>();
                List<invoicelogdetail> logdetaillist = new List<invoicelogdetail>();
                invoicelog log = new invoicelog();

                invoicelogdetail logdetail = new invoicelogdetail();
                int length = int.Parse(Request["length"]);
                for (int i = 0; i < length; i++)
                {
                    if (Regex.IsMatch(Request["Arr[" + i + "][count]"], @"^-?\d+$"))
                    {
                        if (int.Parse(Request["Arr[" + i + "][count]"]) < 0)
                        {
                            if (-int.Parse(Request["Arr[" + i + "][count]"]) > idetail[i].TextileCount)
                            {
                                return Json("false1", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json("false", JsonRequestBehavior.AllowGet);
                    }
                }
                if (FloorId == 0)
                {
                    return Json("false3", JsonRequestBehavior.AllowGet);
                }
                #region 酒店改变
                if (invoice.HotelID != HotelId)
                {
                    #region 酒店
                    int oldeHotelId = invoice.HotelID;
                    invoice.HotelID = HotelId;
                    int invoiceUpdateHotel = i_invoice.Update(invoice);
                    if (invoiceUpdateHotel != 0)
                    {
                        log = new invoicelog();
                        log.LogID = Guid.NewGuid().ToString();
                        log.TableName = "invoice";
                        log.BussinessName = "单据修改";
                        log.CreateUserName = LoginUserManage.GetInstance().GetLoginUser().LoginName;
                        log.CreateType = "update";
                        log.CreateTime = DateTime.Now;
                        loglist.Add(log);

                        logdetail = new invoicelogdetail();
                        logdetail.LogID = log.LogID;
                        logdetail.ColumnName = "HotelID";
                        logdetail.ColumnText = "酒店";
                        logdetail.PrimaryKeyColName = "ID";
                        logdetail.PrimaryKeyValue = invoice.ID;
                        logdetail.OldValue = oldeHotelId.ToString();
                        logdetail.NewValue = invoice.HotelID.ToString();
                        logdetail.CreateType = "update";
                        logdetaillist.Add(logdetail);
                    }
                    foreach (var item in idetail)
                    {
                        int olderidetailHotelID = item.HotelID;
                        item.HotelID = HotelId;
                        int idetailupdate1Hotel = i_invoicedetail.Update(item);
                    }
                    #endregion
                    #region 楼层
                    int oldeFloorId = invoice.RegionID;
                    invoice.RegionID = FloorId;
                    int invoiceupdateRegion = i_invoice.Update(invoice);
                    if (invoiceupdateRegion != 0)
                    {
                        logdetail = new invoicelogdetail();
                        logdetail.LogID = log.LogID;
                        logdetail.ColumnName = "RegionID";
                        logdetail.ColumnText = "楼层";
                        logdetail.PrimaryKeyColName = "ID";
                        logdetail.PrimaryKeyValue = invoice.ID;
                        logdetail.OldValue = oldeFloorId.ToString();
                        logdetail.NewValue = invoice.RegionID.ToString(); ;
                        logdetail.CreateType = "update";
                        logdetaillist.Add(logdetail);
                    }
                    foreach (var item in idetail)
                    {
                        int olderidetailRegionID = item.RegionID;
                        item.RegionID = FloorId;
                        int idetailupdate1Region = i_invoicedetail.Update(item);
                    }
                    #endregion
                    #region 异常数
                    int sum = 0;
                    int oldQuantity = invoice.Quantity;
                    //bool val = false;
                    //for (int i = 0; i < length; i++)
                    //{
                    //    if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                    //    {
                    //        val = true;
                    //    }
                    //}
                    //if (val)
                    //{
                    //    log = new invoicelog();
                    //    log.LogID = Guid.NewGuid().ToString();
                    //    log.TableName = "invoice";
                    //    log.BussinessName = "单据修改";
                    //    log.CreateUserName = LoginUserManage.GetInstance().GetLoginUser().LoginName;
                    //    log.CreateType = "update";
                    //    log.CreateTime = DateTime.Now;
                    //    loglist.Add(log);
                    //}
                    for (int i = 0; i < length; i++)
                    {
                        if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                        {
                            int oldeCount = idetail[i].TextileCount;
                            idetail[i].TextileCount = idetail[i].TextileCount + int.Parse(Request["Arr[" + i + "][count]"]);
                            int ditailupdateCount = i_invoicedetail.Update(idetail[i]);
                            if (ditailupdateCount != 0)
                            {
                                sum += int.Parse(Request["Arr[" + i + "][count]"]);
                                logdetail = new invoicelogdetail();
                                logdetail.LogID = log.LogID;
                                logdetail.ColumnName = "TextileCount";
                                logdetail.ColumnText = "数量";
                                logdetail.PrimaryKeyColName = "ID";
                                logdetail.PrimaryKeyValue = idetail[i].ID.ToString();
                                logdetail.OldValue = oldeCount.ToString();
                                logdetail.NewValue = idetail[i].TextileCount.ToString();
                                logdetail.CreateType = "updatd";
                                logdetaillist.Add(logdetail);
                            }
                        }
                    }
                    if (sum != 0)
                    {
                        invoice.Quantity = invoice.Quantity + sum;
                        int invoiceupdateCount = i_invoice.Update(invoice);
                        if (invoiceupdateCount > 0)
                        {
                            logdetail = new invoicelogdetail();
                            logdetail.LogID = log.LogID;
                            logdetail.ColumnName = "Quantity";
                            logdetail.ColumnText = "总数";
                            logdetail.PrimaryKeyColName = "ID";
                            logdetail.PrimaryKeyValue = invoice.ID.ToString();
                            logdetail.OldValue = oldQuantity.ToString();
                            logdetail.NewValue = (invoice.Quantity + sum).ToString();
                            logdetail.CreateType = "update";
                            logdetaillist.Add(logdetail);
                        }
                    }
                    #endregion
                }
                #endregion

                #region 楼层改变
                else if (invoice.RegionID != FloorId)
                {
                    #region 楼层
                    int oldeFloorId = invoice.RegionID;
                    invoice.RegionID = FloorId;
                    int invoiceupdateRegion = i_invoice.Update(invoice);
                    if (invoiceupdateRegion != 0)
                    {
                        log = new invoicelog();
                        log.LogID = Guid.NewGuid().ToString();
                        log.TableName = "invoice";
                        log.BussinessName = "单据修改";
                        log.CreateUserName = LoginUserManage.GetInstance().GetLoginUser().LoginName;
                        log.CreateType = "update";
                        log.CreateTime = DateTime.Now;
                        loglist.Add(log);

                        logdetail = new invoicelogdetail();
                        logdetail.LogID = log.LogID;
                        logdetail.ColumnName = "RegionID";
                        logdetail.ColumnText = "楼层";
                        logdetail.PrimaryKeyColName = "ID";
                        logdetail.PrimaryKeyValue = invoice.ID;
                        logdetail.OldValue = oldeFloorId.ToString();
                        logdetail.NewValue = invoice.RegionID.ToString(); ;
                        logdetail.CreateType = "update";
                        logdetaillist.Add(logdetail);
                    }
                    foreach (var item in idetail)
                    {
                        int olderidetailRegionID = item.RegionID;
                        item.RegionID = FloorId;
                        int idetailupdate1Region = i_invoicedetail.Update(item);
                    }
                    #endregion
                    #region 异常数
                    int sum = 0;
                    int oldQuantity = invoice.Quantity;
                    //bool val = false;
                    //for (int i = 0; i < length; i++)
                    //{
                    //    if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                    //    {
                    //        val = true;
                    //    }
                    //}
                    //if (val)
                    //{
                    //    log = new invoicelog();
                    //    log.LogID = Guid.NewGuid().ToString();
                    //    log.TableName = "invoice";
                    //    log.BussinessName = "单据修改";
                    //    log.CreateUserName = LoginUserManage.GetInstance().GetLoginUser().LoginName;
                    //    log.CreateType = "update";
                    //    log.CreateTime = DateTime.Now;
                    //    loglist.Add(log);
                    //}
                    for (int i = 0; i < length; i++)
                    {
                        if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                        {
                            int oldeCount = idetail[i].TextileCount;
                            idetail[i].TextileCount = idetail[i].TextileCount + int.Parse(Request["Arr[" + i + "][count]"]);
                            int ditailupdateCount = i_invoicedetail.Update(idetail[i]);
                            if (ditailupdateCount != 0)
                            {
                                sum += int.Parse(Request["Arr[" + i + "][count]"]);
                                logdetail = new invoicelogdetail();
                                logdetail.LogID = log.LogID;
                                logdetail.ColumnName = "TextileCount";
                                logdetail.ColumnText = "数量";
                                logdetail.PrimaryKeyColName = "ID";
                                logdetail.PrimaryKeyValue = idetail[i].ID.ToString();
                                logdetail.OldValue = oldeCount.ToString();
                                logdetail.NewValue = idetail[i].TextileCount.ToString();
                                logdetail.CreateType = "updatd";
                                logdetaillist.Add(logdetail);
                            }
                        }
                    }
                    if (sum != 0)
                    {
                        invoice.Quantity = invoice.Quantity + sum;
                        int invoiceupdateCount = i_invoice.Update(invoice);
                        if (invoiceupdateCount > 0)
                        {
                            logdetail = new invoicelogdetail();
                            logdetail.LogID = log.LogID;
                            logdetail.ColumnName = "Quantity";
                            logdetail.ColumnText = "总数";
                            logdetail.PrimaryKeyColName = "ID";
                            logdetail.PrimaryKeyValue = invoice.ID.ToString();
                            logdetail.OldValue = oldQuantity.ToString();
                            logdetail.NewValue = (invoice.Quantity + sum).ToString();
                            logdetail.CreateType = "update";
                            logdetaillist.Add(logdetail);
                        }
                    }
                    #endregion
                }
                #endregion

                #region 异常数
                else
                {
                    #region 异常数
                    int sum = 0;
                    int oldQuantity = invoice.Quantity;
                    bool val = false;
                    for (int i = 0; i < length; i++)
                    {
                        if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                        {
                            val = true;
                        }
                    }
                    if (val)
                    {
                        log = new invoicelog();
                        log.LogID = Guid.NewGuid().ToString();
                        log.TableName = "invoice";
                        log.BussinessName = "单据修改";
                        log.CreateUserName = LoginUserManage.GetInstance().GetLoginUser().LoginName;
                        log.CreateType = "update";
                        log.CreateTime = DateTime.Now;
                        loglist.Add(log);
                    }
                    for (int i = 0; i < length; i++)
                    {
                        if (int.Parse(Request["Arr[" + i + "][count]"]) != 0)
                        {
                            int oldeCount = idetail[i].TextileCount;
                            idetail[i].TextileCount = idetail[i].TextileCount + int.Parse(Request["Arr[" + i + "][count]"]);
                            int ditailupdateCount = i_invoicedetail.Update(idetail[i]);
                            if (ditailupdateCount != 0)
                            {
                                sum += int.Parse(Request["Arr[" + i + "][count]"]);
                                logdetail = new invoicelogdetail();
                                logdetail.LogID = log.LogID;
                                logdetail.ColumnName = "TextileCount";
                                logdetail.ColumnText = "数量";
                                logdetail.PrimaryKeyColName = "ID";
                                logdetail.PrimaryKeyValue = idetail[i].ID.ToString();
                                logdetail.OldValue = oldeCount.ToString();
                                logdetail.NewValue = idetail[i].TextileCount.ToString();
                                logdetail.CreateType = "updatd";
                                logdetaillist.Add(logdetail);
                            }
                        }
                    }
                    if (sum != 0)
                    {
                        invoice.Quantity = invoice.Quantity + sum;
                        int invoiceupdateCount = i_invoice.Update(invoice);
                        if (invoiceupdateCount > 0)
                        {
                            logdetail = new invoicelogdetail();
                            logdetail.LogID = log.LogID;
                            logdetail.ColumnName = "Quantity";
                            logdetail.ColumnText = "总数";
                            logdetail.PrimaryKeyColName = "ID";
                            logdetail.PrimaryKeyValue = invoice.ID.ToString();
                            logdetail.OldValue = oldQuantity.ToString();
                            logdetail.NewValue = (invoice.Quantity + sum).ToString();
                            logdetail.CreateType = "update";
                            logdetaillist.Add(logdetail);
                        }
                    }
                    #endregion
                }
                #endregion

                i_invoicelog.Insert(loglist);
                i_invoicelogdetail.Insert(logdetaillist);
                sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
                string code = sc != null ? sc.Code : "A";

                RabbitMQModel mqModel = new RabbitMQModel();
                mqModel.Type = "InvoiceModify";
                mqModel.Value = loglist[0].LogID;
                mqModel.Code = code;
                RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

            }
            catch (Exception)
            {
                return Json("fales3", JsonRequestBehavior.AllowGet);
            }

            return Json("true", JsonRequestBehavior.AllowGet);
        }
    }
}