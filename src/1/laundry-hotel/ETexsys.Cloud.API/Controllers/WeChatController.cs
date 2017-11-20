﻿using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.Common.Log;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    //[SupportFilter]
    public class WeChatController : ApiController
    {
        private static readonly object invLcker = new object();

        private static readonly string querysql = "SELECT t.ID,t.ClassID,tc.ClassName,tc.Sort AS ClassSort,t.SizeID,s.SizeName,s.Sort AS SizeSort,t.BrandID,bt.BrandName,bt.Sort AS BrandSort,t.TextileState,t.TagNo,t.RegisterTime,t.Washtimes,t.UpdateTime,t.HotelID,h.RegionName AS HotelName,t.RegionID,t.LastReceiveRegionID,rt.RFIDWashtime,tc.ClassLeft AS ClassLeft,rt.CostTime AS RFIDCostTime,tc.PackCount,t.LastReceiveInvID FROM textile AS t INNER JOIN rfidtag as rt ON t.tagno=RT.rfidtagno LEFT JOIN textileclass AS tc ON t.ClassID=tc.ID LEFT JOIN size AS s ON t.SizeID= s.ID LEFT JOIN brandtype AS bt on t.BrandID= BT.ID LEFT JOIN region as h on t.HotelID= h.ID WHERE RT.RFIDState=1 AND t.LogoutType= 0 AND t.IsFlag=1 ";

        [Dependency]
        public IRepository<inventory> i_inventory { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<v_textile_tag> i_v_textle_tag { get; set; }

        [Dependency]
        public IRepository<task> i_task { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<region_setting> i_region_setting { get; set; }

        [Dependency]
        public IRepository<pricedetail> i_pricedetail { get; set; }

        [Dependency]
        public IRepository<integraldetail> i_integraldetail { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<complain> i_complain { get; set; }

        [Dependency]
        public IRepository<complainattach> i_complainattach { get; set; }

        [Dependency]
        public IRepository<goodscate> i_goodscate { get; set; }

        [Dependency]
        public IRepository<goods> i_goods { get; set; }

        [Dependency]
        public IRepository<fc_invoice> i_fc_invoice { get; set; }

        [Dependency]
        public IRepository<goodsorder> i_goodsorder { get; set; }

        [Dependency]
        public IRepository<goodsorderdetail> i_goodsorderdetail { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_customer { get; set; }

        [Dependency]
        public IRepository<hotel_detail> i_hotel_detail { get; set; }

        public MsgModel IndexData([FromBody]WeChatParamModel request)
        {
            MsgModel msg = new MsgModel();

            ResponseWeChatIndexModel indexModel = new ResponseWeChatIndexModel();

            List<ResponseGoodsModel> list = new List<ResponseGoodsModel>();
            ResponseGoodsModel model = null;

            #region 租赁总数

            var query = from t in i_inventory.Entities
                        where t.RegionID == request.hotelId
                        group t by new { t.ClassID, t.SizeID } into grp
                        let max_date = grp.Max(v => v.CreateTime)
                        from row in grp
                        where row.CreateTime == max_date
                        select row;

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();
            List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).ToList();

            query.ToList().ForEach(q =>
            {
                string name = classList.Where(v => v.ID == q.ClassID).FirstOrDefault().ClassName;
                if (q.SizeID != null && q.SizeID != 0)
                {
                    name = name + "(" + sizeList.Where(v => v.ID == q.SizeID).FirstOrDefault().SizeName + ")";
                }
                model = list.Where(v => v.name == name).FirstOrDefault();
                if (model == null)
                {
                    model = new ResponseGoodsModel();
                    model.name = name;
                    model.rentdata = q.TextileTotal.ToString("N0");
                    model.usingdata = "";
                    model.abdata = "";
                    list.Add(model);
                }
                else
                {
                    model.rentdata = q.TextileTotal.ToString("N0");
                }
            });


            #endregion

            #region 正常使用

            DateTime time = DateTime.Now.AddDays(-30);

            var query1 = i_v_textle_tag.SQLQuery(querysql, "");

            var uq = from t in query1
                     where (t.TextileState == 2 || t.TextileState == 9) && t.HotelID == request.hotelId && t.UpdateTime >= time
                     group t by new { t.ClassName, t.ClassSort, t.SizeName } into m
                     orderby m.Key.ClassSort, m.Key.ClassName, m.Key.SizeName
                     select new { m.Key.ClassName, m.Key.SizeName, Count = m.Count() };

            uq.ToList().ForEach(q =>
            {
                string name = "";
                if (q.SizeName != null && q.SizeName != "")
                {
                    name = q.ClassName + "(" + q.SizeName + ")";
                }
                else
                {
                    name = q.ClassName;
                }
                model = list.Where(v => v.name == name).FirstOrDefault();
                if (model == null)
                {
                    model = new ResponseGoodsModel();
                    model.name = name;
                    model.usingdata = q.Count.ToString("N0");
                    model.usingdata = "";
                    model.abdata = "";
                    list.Add(model);
                }
                else
                {
                    model.usingdata = q.Count.ToString("N0");
                }
            });

            #endregion

            #region 异常使用

            var query2 = i_v_textle_tag.SQLQuery(querysql, "");

            var aq = from t in query2
                     where (t.TextileState == 2 || t.TextileState == 9) && t.HotelID == request.hotelId && t.UpdateTime < time
                     group t by new { t.ClassName, t.ClassSort, t.SizeName } into m
                     orderby m.Key.ClassSort, m.Key.ClassName, m.Key.SizeName
                     select new { m.Key.ClassName, m.Key.SizeName, Count = m.Count() };

            List<ResponseGoodsModel> AbList = new List<ResponseGoodsModel>();
            aq.ToList().ForEach(q =>
             {
                 string name = "";
                 if (q.SizeName != null && q.SizeName != "")
                 {
                     name = q.ClassName + "(" + q.SizeName + ")";
                 }
                 else
                 {
                     name = q.ClassName;
                 }
                 model = list.Where(v => v.name == name).FirstOrDefault();
                 if (model == null)
                 {
                     model = new ResponseGoodsModel();
                     model.name = name;
                     model.abdata = q.Count.ToString("N0");
                     model.usingdata = "";
                     model.abdata = "";
                     list.Add(model);
                 }
                 else
                 {
                     model.abdata = q.Count.ToString("N0");
                 }
             });


            #endregion

            indexModel.TextileDistribution = list;

            msg.Result = indexModel;

            return msg;
        }

        public MsgModel HotelIntegral([FromBody]WeChatParamModel requst)
        {
            MsgModel msg = new MsgModel();

            region_setting model = i_region_setting.Entities.Where(v => v.RegionID == requst.hotelId).FirstOrDefault();

            if (model != null)
            {
                msg.Result = model.Integral;
                msg.OtherCode = model.Deposit.HasValue ? model.Deposit.Value.ToString("N0") : "";
                msg.ResultCode = 0;
            }

            return msg;
        }

        public MsgModel HotelIntegralRecords([FromBody]WeChatParamModel requst)
        {
            MsgModel msg = new MsgModel();

            DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var query = from t in i_integraldetail.Entities
                        where t.HotelID == requst.hotelId && t.CreateTime >= time
                        orderby t.CreateTime descending
                        select new
                        {
                            t.CreateTime,
                            t.SubType,
                            t.InvNo,
                            t.Points
                        };

            List<ResponseIntegralRecords> list = new List<ResponseIntegralRecords>();
            ResponseIntegralRecords model = null;

            query.ToList().ForEach(q =>
            {
                model = new ResponseIntegralRecords();
                model.date = q.CreateTime.ToString("MM月dd日");
                model.time = q.CreateTime.ToString("HH：mm");
                model.Type = q.SubType;
                model.data = q.SubType == 1 ? "+" + q.Points.ToString("N2") : "-" + q.Points.ToString("N2");
                model.Remark = q.SubType == 1 ? "酒店充值" : (q.SubType == 2 ? "洗涤费:" + q.InvNo : (q.SubType == 3 ? "租赁费:" + q.InvNo : "商城消费:" + q.InvNo));
                model.invNo = q.InvNo;
                list.Add(model);
            });

            msg.Result = list;

            return msg;
        }

        public MsgModel HotelPrice([FromBody]WeChatParamModel requst)
        {
            MsgModel msg = new MsgModel();

            region_setting model = i_region_setting.GetByKey(requst.hotelId);

            if (model != null)
            {
                var washingQuery = from t in i_pricedetail.Entities
                                   join c in i_textileclass.Entities on t.ClassID equals c.ID
                                   join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                   from s in s_join.DefaultIfEmpty()
                                   where t.TemplateID == model.WashID.Value
                                   select new
                                   {
                                       t.ClassID,
                                       t.SizeID,
                                       c.ClassName,
                                       SizeName = s == null ? "" : s.SizeName,
                                       t.Price
                                   };
                var rentQuery = from t in i_pricedetail.Entities
                                join c in i_textileclass.Entities on t.ClassID equals c.ID
                                join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                from s in s_join.DefaultIfEmpty()
                                where t.TemplateID == model.RentID.Value
                                select new
                                {
                                    t.ClassID,
                                    t.SizeID,
                                    c.ClassName,
                                    SizeName = s == null ? "" : s.SizeName,
                                    t.Price
                                };

                List<ResponsePriceModel> list = new List<ResponsePriceModel>();
                ResponsePriceModel priceModel = null;
                washingQuery.ToList().ForEach(q =>
                {
                    priceModel = new ResponsePriceModel();
                    priceModel.ClassID = q.ClassID;
                    priceModel.SizeID = q.SizeID;
                    if (q.SizeID == 0)
                    {
                        priceModel.ClassName = q.ClassName;
                    }
                    else
                    {
                        priceModel.ClassName = q.ClassName + "[" + q.SizeName + "]";
                    }
                    priceModel.WashingData = q.Price.ToString("N2");
                    priceModel.RentData = "0";
                    list.Add(priceModel);
                });

                rentQuery.ToList().ForEach(q =>
                {
                    priceModel = list.Where(v => v.ClassID == q.ClassID && v.SizeID == q.SizeID).FirstOrDefault();
                    if (priceModel == null)
                    {
                        priceModel = new ResponsePriceModel();
                        priceModel.ClassID = q.ClassID;
                        priceModel.SizeID = q.SizeID;
                        if (q.SizeID == 0)
                        {
                            priceModel.ClassName = q.ClassName;
                        }
                        else
                        {
                            priceModel.ClassName = q.ClassName + "[" + q.SizeName + "]";
                        }
                        priceModel.WashingData = "0";
                        priceModel.RentData = q.Price.ToString("N2");
                        list.Add(priceModel);
                    }
                    else
                    {
                        priceModel.RentData = q.Price.ToString("N2");
                    }
                });

                msg.Result = list;
            }

            return msg;
        }

        public MsgModel HotelDaySummary([FromBody] WeChatInvoiceParamModel requst)
        {
            MsgModel msg = new MsgModel();

            if (requst.subType == 1)
            {
                #region 日表

                DateTime startTime = Convert.ToDateTime(requst.time);
                DateTime endTime = startTime.AddDays(1);

                var query = from t in i_invoicedetail.Entities
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where t.InvType == requst.invType && t.HotelID == requst.hotelId && t.InvCreateTime >= startTime && t.InvCreateTime < endTime
                            group t by new { t.InvSubType, ClassSort = c.Sort, t.ClassName, s.Sort, t.SizeName } into m
                            orderby m.Key.ClassSort, m.Key.ClassName, m.Key.Sort, m.Key.SizeName
                            select new
                            {
                                m.Key.InvSubType,
                                m.Key.ClassName,
                                m.Key.SizeName,
                                Count = m.Sum(v => v.TextileCount)
                            };
                List<ResponseDaySummaryModel> list = new List<ResponseDaySummaryModel>();
                ResponseDaySummaryModel model = null;
                int total = 0;
                int otherTotal = 0;
                if (requst.invType == 1)
                {
                    query.Where(v => v.InvSubType == 1).ToList().ForEach(q =>
                    {
                        model = new ResponseDaySummaryModel();
                        model.ClassName = q.ClassName;
                        model.SizeName = q.SizeName;
                        model.Count = q.Count;
                        model.OtherCount = "";
                        total += q.Count;
                        list.Add(model);
                    });

                    query.Where(v => v.InvSubType == 2).ToList().ForEach(q =>
                    {
                        model = list.Where(v => v.ClassName == q.ClassName && v.SizeName == q.SizeName).FirstOrDefault();
                        if (model == null)
                        {
                            model = new ResponseDaySummaryModel();
                            model.ClassName = q.ClassName;
                            model.SizeName = q.SizeName;
                            model.Count = 0;
                            model.OtherCount = q.Count.ToString();
                            list.Add(model);
                            otherTotal += q.Count;
                        }
                        else
                        {
                            model.OtherCount = q.Count.ToString();
                        }
                    });

                    model = new ResponseDaySummaryModel();
                    model.ClassName = "合计";
                    model.SizeName = "";
                    model.Count = total;
                    model.OtherCount = otherTotal.ToString();
                    list.Add(model);
                }
                else
                {
                    query.ToList().ForEach(q =>
                    {
                        model = new ResponseDaySummaryModel();
                        model.ClassName = q.ClassName;
                        model.SizeName = q.SizeName;
                        model.Count = q.Count;
                        model.OtherCount = "";
                        total += q.Count;
                        list.Add(model);
                    });

                    model = new ResponseDaySummaryModel();
                    model.ClassName = "合计";
                    model.SizeName = "";
                    model.Count = total;
                    model.OtherCount = otherTotal.ToString();
                    list.Add(model);
                }

                msg.Result = list;

                #endregion
            }
            else
            {
                DateTime startTime = Convert.ToDateTime(requst.time + "-1");
                DateTime endTime = startTime.AddMonths(1);

                if (endTime > DateTime.Now)
                {
                    endTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1);
                }


                List<ResponseMonthSummaryModel> list = new List<ResponseMonthSummaryModel>();
                ResponseMonthSummaryModel model = null;

                int total = 0;
                int othertotal = 0;
                //收货、特殊处理返洗
                if (requst.invType == 1)
                {
                    var query = from t in i_invoicedetail.Entities
                                join c in i_textileclass.Entities on t.ClassID equals c.ID
                                join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                from s in s_join.DefaultIfEmpty()
                                where t.InvType == requst.invType && t.HotelID == requst.hotelId && t.InvCreateTime >= startTime && t.InvCreateTime < endTime
                                group t by new { t.InvCreateTime, t.InvSubType } into m
                                orderby m.Key.InvCreateTime
                                select new
                                {
                                    m.Key.InvCreateTime,
                                    m.Key.InvSubType,
                                    Count = m.Sum(v => v.TextileCount)
                                };

                    var data = query.ToList();

                    DateTime d = startTime;
                    while (d < endTime)
                    {
                        DateTime d1 = d.AddDays(1);
                        model = new ResponseMonthSummaryModel();
                        model.date = d.ToString("M.d");
                        model.Count = data.Where(v => v.InvCreateTime >= d && v.InvCreateTime < d1 && v.InvSubType == 1).Sum(v => v.Count);
                        model.OtherCount = data.Where(v => v.InvCreateTime >= d && v.InvCreateTime < d1 && v.InvSubType == 2).Sum(v => v.Count);
                        total += model.Count;
                        othertotal += model.OtherCount;
                        list.Add(model);
                        d = d1;
                    }
                }
                else
                {
                    var query = from t in i_invoicedetail.Entities
                                join c in i_textileclass.Entities on t.ClassID equals c.ID
                                join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                from s in s_join.DefaultIfEmpty()
                                where t.InvType == requst.invType && t.HotelID == requst.hotelId && t.InvCreateTime >= startTime && t.InvCreateTime < endTime
                                group t by new { t.InvCreateTime } into m
                                orderby m.Key.InvCreateTime
                                select new
                                {
                                    m.Key.InvCreateTime,
                                    Count = m.Sum(v => v.TextileCount)
                                };

                    var data = query.ToList();

                    DateTime d = startTime;
                    while (d < endTime)
                    {
                        DateTime d1 = d.AddDays(1);
                        model = new ResponseMonthSummaryModel();
                        model.date = d.ToString("M.d");
                        model.Count = data.Where(v => v.InvCreateTime >= d && v.InvCreateTime < d1).Sum(v => v.Count);
                        total += model.Count;
                        othertotal += model.OtherCount;
                        list.Add(model);
                        d = d1;
                    }
                }
                model = new ResponseMonthSummaryModel();
                model.date = "合计";
                model.Count = total;
                model.OtherCount = othertotal;
                list.Add(model);

                msg.Result = list;
            }

            return msg;
        }

        public MsgModel FactoryDetail()
        {
            MsgModel msg = new MsgModel();

            sys_customer model = i_sys_customer.Entities.FirstOrDefault();
            msg.Result = model;
            return msg;
        }

        public MsgModel HotelComplain([FromBody]WeChatComplainParamModel requst)
        {
            MsgModel msg = new MsgModel();

            string guid = Guid.NewGuid().ToString();
            complain model = new complain();
            model.ID = guid;
            model.Cause = requst.cause;
            model.Remarks = requst.remarks;
            model.UserID = requst.userId;
            model.HotelID = requst.hotelId;
            model.CreateTime = DateTime.Now;
            model.HappenTime = Convert.ToDateTime(requst.happenDate);
            model.Comfirmed = false;

            i_complain.Insert(model);

            msg.Result = guid;

            return msg;
        }

        public Task<HttpResponseMessage> UploadAttach(HttpRequestMessage request)
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                new Log4NetFile().Log("上传格式不是multipart/form-data");
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + @"UploadFile\Complain";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }


            var provider = new MultipartFormDataStreamProvider(path);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<string> files = new List<string>();

            var task = request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
           {
               HttpResponseMessage response = null;
               if (t.IsFaulted || t.IsCanceled)
               {
                   response = request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
               }
               else
               {
                   string complainId = "";
                   foreach (var key in provider.FormData.AllKeys)
                   {
                       complainId = provider.FormData[key].ToString();
                   }
                   List<complainattach> list = new List<complainattach>();
                   complainattach model = null;
                   string dicName = DateTime.Now.ToString("yyyyMMdd");
                   foreach (var file in provider.FileData)
                   {
                       string fileName = file.Headers.ContentDisposition.FileName;
                       if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                       {
                           fileName = fileName.Trim('"');
                       }
                       if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                       {
                           fileName = Path.GetFileName(fileName);
                       }
                       String ext = System.IO.Path.GetExtension(fileName);
                       var newFileName = DateTime.Now.ToString("yyyyMMddhhmmssfff") + ext;
                       File.Copy(file.LocalFileName, Path.Combine(path, newFileName));

                       model = new complainattach();
                       model.ComplaintID = complainId;
                       model.AttachContent = newFileName;
                       model.AttachType = "Image";
                       list.Add(model);
                   }

                   i_complainattach.Insert(list);

                   response = request.CreateResponse(HttpStatusCode.OK, new { success = true });
               }
               return response;
           });
            return task;
        }

        public MsgModel ShopGoods()
        {
            List<goodscate> CateList = i_goodscate.Entities.Where(v => v.Deleted == false).OrderBy(v => v.Sort).ThenBy(v => v.CateName).ToList();
            List<goods> GoodsList = i_goods.Entities.Where(v => v.Deleted == false).OrderBy(v => v.CateID).ThenBy(v => v.Sort).ToList();

            MsgModel msg = new MsgModel();
            List<ResponseShopGoodsCateModel> MenuList = new List<ResponseShopGoodsCateModel>();
            ResponseShopGoodsCateModel menuItem = null;
            List<ResponseShopGoodsModel> GoodsItemList = null;
            ResponseShopGoodsModel goodsItem = null;

            var url = WebConfigurationManager.AppSettings["WebUrl"];

            string[] tag = new string[26] { "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo", "pp", "qq", "rr", "ss", "tt", "uu", "vv", "ww", "xx", "yy", "zz" };

            int index = 0;
            CateList.ForEach(q =>
            {
                menuItem = new ResponseShopGoodsCateModel();
                menuItem.id = q.ID.ToString();
                menuItem.name = q.CateName;
                menuItem.tag = tag[index];
                index++;

                GoodsItemList = new List<ResponseShopGoodsModel>();

                var query = GoodsList.Where(v => v.CateID == q.ID && v.Grounding == true && v.Deleted == false).ToList();

                query.ToList().ForEach(a =>
                {
                    goodsItem = new ResponseShopGoodsModel();
                    goodsItem.id = a.ID.ToString();
                    goodsItem.name = a.GoodsName;
                    goodsItem.price = a.GoodsMoney;
                    goodsItem.costprice = a.CostPrice;
                    goodsItem.pic = url + a.GoodsLogo;
                    goodsItem.sales = a.Hoting;
                    goodsItem.count = 0;

                    GoodsItemList.Add(goodsItem);
                });
                menuItem.dishs = GoodsItemList;
                MenuList.Add(menuItem);
            });

            msg.Result = MenuList;

            return msg;
        }

        public MsgModel SubmitOrder([FromBody]WeChatShopOrderParamModel request)
        {
            MsgModel msgModel = new MsgModel();
            string guid = request.guid.ToUpper();

            fc_invoice fcModel = i_fc_invoice.Entities.Where(v => v.No == guid).FirstOrDefault();

            if (fcModel == null)
            {
                fcModel = new fc_invoice();
                fcModel.CreateTime = DateTime.Now;
                fcModel.No = guid;
                fcModel.State = 0;
                fcModel.ResultMsg = "";
                try
                {
                    i_fc_invoice.Insert(fcModel);
                }
                catch
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "系统正在处理中.请稍后再试。";
                    return msgModel;
                }
            }
            else
            {//非第一次请求
                if (fcModel.State == 1)//State为1表示上次请求处理完成
                {
                    //返回上次内容
                    msgModel.Result = fcModel.ResultMsg;
                    return msgModel;
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "系统正在处理中.请稍后再试。";
                    return msgModel;
                }
            }

            int nubmer = GetOrderNubmer();
            string invNo = string.Format("{0}{1}{2}", "SC", DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
            DateTime time = DateTime.Now;

            goodsorder orderModel = new goodsorder();
            orderModel.ID = guid;
            orderModel.OrderNo = invNo;
            orderModel.CreateTime = time;
            orderModel.CreateUserID = request.userId;
            orderModel.HotelID = request.hotelId;
            orderModel.OrderState = 1;
            orderModel.PaymentType = 2;
            orderModel.Remarks = request.remarks;
            orderModel.DeliveryDate = Convert.ToDateTime(request.deliveryDate);
            orderModel.DeliveryTime = request.deliveryTime;

            List<goods> goodsList = i_goods.Entities.Where(v => v.Deleted == false).ToList();

            List<goodsorderdetail> orderDetailList = new List<goodsorderdetail>();
            goodsorderdetail orderDetailModel = null;
            int total = 0;
            decimal totalMoney = 0;
            foreach (var item in request.Detail)
            {
                goods goodsModel = goodsList.Where(v => v.ID == item.goodsId).FirstOrDefault();
                if (goodsModel != null)
                {
                    orderDetailModel = new goodsorderdetail();
                    orderDetailModel.OrderID = guid;
                    orderDetailModel.OrderNo = invNo;
                    orderDetailModel.HotelID = request.hotelId;
                    orderDetailModel.GoodsCount = item.goodsCount;
                    orderDetailModel.GoodsID = item.goodsId;
                    orderDetailModel.GoodMoney = goodsModel.GoodsMoney;
                    orderDetailModel.TotalMoney = goodsModel.GoodsMoney * item.goodsCount;
                    orderDetailModel.GoodsName = goodsModel.GoodsName;

                    orderDetailList.Add(orderDetailModel);

                    total += item.goodsCount;
                    totalMoney += orderDetailModel.TotalMoney;
                }
            }

            orderModel.TotalQuantity = total;
            orderModel.TotalMoney = totalMoney;

            i_goodsorder.Insert(orderModel, false);
            i_goodsorderdetail.Insert(orderDetailList, false);

            integraldetail integralModel = new integraldetail();
            integralModel.HotelID = request.hotelId;
            integralModel.CreateTime = time;
            integralModel.SubType = 4;
            integralModel.InvNo = invNo;
            integralModel.Points = Convert.ToDouble(totalMoney);
            i_integraldetail.Insert(integralModel, false);

            region_setting regionsettingModel = i_region_setting.GetByKey(request.hotelId);
            if (regionsettingModel != null)
            {
                regionsettingModel.Integral = regionsettingModel.Integral - Convert.ToDouble(totalMoney);
                i_region_setting.Update(regionsettingModel);
            }
            else
            {
                regionsettingModel = new region_setting();
                regionsettingModel.CreateTime = time;
                regionsettingModel.CreateUserID = request.userId;
                regionsettingModel.Integral = 0 - Convert.ToDouble(totalMoney);
                regionsettingModel.RegionID = request.hotelId;

                i_region_setting.Insert(regionsettingModel);
            }

            msgModel.Result = invNo + "|" + total.ToString() + "|" + totalMoney.ToString("N2");

            fcModel.ResultMsg = invNo + "|" + total.ToString() + "|" + totalMoney.ToString("N2");
            fcModel.State = 1;
            i_fc_invoice.Update(fcModel);

            return msgModel;
        }

        public MsgModel SelectOrder([FromBody]WeChatParamModel request)
        {
            MsgModel msg = new MsgModel();

            List<ResponseOrderModel> list = new List<ResponseOrderModel>();
            ResponseOrderModel orderModel = null;
            List<ResponseOrderDetailModel> detailList = null;
            ResponseOrderDetailModel detailModel = null;

            var query = from t in i_goodsorder.Entities
                        where t.CreateUserID == request.userId
                        orderby t.CreateTime descending
                        select new
                        {
                            t.ID,
                            t.OrderNo,
                            t.CreateTime,
                            t.OrderState,
                            t.TotalQuantity,
                            t.TotalMoney,
                            t.ComfirmTime,
                            t.DistributionTime,
                            t.SignTime,
                            t.DeliveryDate,
                            t.DeliveryTime,
                            t.Distributor,
                            t.DistributorTel
                        };

            query.ToList().ForEach(q =>
            {
                orderModel = new ResponseOrderModel();
                orderModel.id = q.ID;
                orderModel.invno = q.OrderNo;
                orderModel.state = q.OrderState;
                orderModel.stateName = Enum.GetName(typeof(OrderState), q.OrderState);
                orderModel.time = q.CreateTime.ToString("yyyy/MM/dd HH:mm");
                orderModel.total = q.TotalQuantity;
                orderModel.money = q.TotalMoney;
                orderModel.comfirmTime = q.ComfirmTime.HasValue ? q.ComfirmTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.distributionTime = q.DistributionTime.HasValue ? q.DistributionTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.signTime = q.SignTime.HasValue ? q.SignTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.deliveryTime = q.DeliveryDate.ToString("yyyy/MM/dd") + " " + q.DeliveryTime;
                orderModel.Distributor = q.Distributor;
                orderModel.DistributorTel = q.DistributorTel;

                detailList = new List<ResponseOrderDetailModel>();
                i_goodsorderdetail.Entities.Where(v => v.OrderID == q.ID).ToList().ForEach(a =>
                {
                    detailModel = new ResponseOrderDetailModel();
                    detailModel.goodsname = a.GoodsName;
                    detailModel.count = a.GoodsCount;
                    detailModel.price = a.GoodMoney * a.GoodsCount;

                    detailList.Add(detailModel);
                });
                orderModel.detail = detailList;

                list.Add(orderModel);
            });

            msg.Result = list;

            return msg;
        }

        public MsgModel SelectOrderDetail([FromBody]WeChatOrderDetailParamModel request)
        {
            MsgModel msg = new MsgModel();

            ResponseOrderModel orderModel = null;
            List<ResponseOrderDetailModel> detailList = null;
            ResponseOrderDetailModel detailModel = null;

            var query = from t in i_goodsorder.Entities
                        where t.OrderNo == request.invno
                        orderby t.CreateTime descending
                        select new
                        {
                            t.ID,
                            t.OrderNo,
                            t.CreateTime,
                            t.OrderState,
                            t.TotalQuantity,
                            t.TotalMoney,
                            t.ComfirmTime,
                            t.DistributionTime,
                            t.SignTime,
                            t.DeliveryDate,
                            t.DeliveryTime,
                            t.Distributor,
                            t.DistributorTel
                        };

            query.ToList().ForEach(q =>
            {
                orderModel = new ResponseOrderModel();
                orderModel.id = q.ID;
                orderModel.invno = q.OrderNo;
                orderModel.state = q.OrderState;
                orderModel.stateName = Enum.GetName(typeof(OrderState), q.OrderState);
                orderModel.time = q.CreateTime.ToString("yyyy/MM/dd HH:mm");
                orderModel.total = q.TotalQuantity;
                orderModel.money = q.TotalMoney;
                orderModel.comfirmTime = q.ComfirmTime.HasValue ? q.ComfirmTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.distributionTime = q.DistributionTime.HasValue ? q.DistributionTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.signTime = q.SignTime.HasValue ? q.SignTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                orderModel.deliveryTime = q.DeliveryDate.ToString("yyyy/MM/dd") + " " + q.DeliveryTime;
                orderModel.Distributor = q.Distributor;
                orderModel.DistributorTel = q.DistributorTel;

                detailList = new List<ResponseOrderDetailModel>();
                i_goodsorderdetail.Entities.Where(v => v.OrderID == q.ID).ToList().ForEach(a =>
                {
                    detailModel = new ResponseOrderDetailModel();
                    detailModel.goodsname = a.GoodsName;
                    detailModel.count = a.GoodsCount;
                    detailModel.price = a.GoodMoney * a.GoodsCount;

                    detailList.Add(detailModel);
                });
                orderModel.detail = detailList;

            });

            msg.Result = orderModel;

            return msg;
        }

        public MsgModel SelectInvoiceSettle([FromBody]WeChatInvoiceSettleParamModel request)
        {
            MsgModel msgModel = new MsgModel();
            ResponseInvoiceSettleModel model = new ResponseInvoiceSettleModel();

            var query = from t in i_invoice.Entities where t.InvNo == request.InvNo select new { t.CreateTime, t.CreateUserName, t.Comfirmed, t.ConfirmTime, t.ConfirmUserName, t.InvNo };
            query.ToList().ForEach(q =>
            {
                model.InvNo = q.InvNo;
                model.CreateTime = q.CreateTime.ToString("yyyy/MM/dd HH:mm:ss");
                model.CreateUserName = q.CreateUserName;
                if (q.Comfirmed == 1)
                {
                    model.ConfirmTime = q.ConfirmTime.HasValue ? q.ConfirmTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "";
                    model.ConfirmUserName = q.ConfirmUserName;
                }
                else
                {
                    model.ConfirmUserName = "系统自动确认";
                }
            });

            var dQuery = from t in i_invoicedetail.Entities
                         where t.InvNo == request.InvNo
                         select new
                         {
                             t.ClassName,
                             t.SizeName,
                             t.TextileCount,
                             t.WashingPrice,
                             t.RentPrice
                         };

            List<ResponseSettleDetailModel> list = new List<ResponseSettleDetailModel>();
            ResponseSettleDetailModel dModel = null;
            double washingtotal = 0, renttotal = 0;
            dQuery.ToList().ForEach(q =>
            {
                dModel = new ResponseSettleDetailModel();
                dModel.ClassName = q.ClassName;
                dModel.SizeName = q.SizeName;
                dModel.TextileCount = q.TextileCount;
                dModel.WashingMoney = q.WashingPrice.HasValue ? Convert.ToDouble(q.WashingPrice * q.TextileCount).ToString("N2") : "";
                dModel.RentMoney = q.RentPrice.HasValue ? Convert.ToDouble(q.RentPrice * q.TextileCount).ToString("N2") : "";

                model.TotalTextile += q.TextileCount;
                washingtotal += q.WashingPrice.HasValue ? Convert.ToDouble(q.WashingPrice * q.TextileCount) : 0;
                renttotal += q.RentPrice.HasValue ? Convert.ToDouble(q.RentPrice * q.TextileCount) : 0;


                list.Add(dModel);
            });
            model.WashingTotalMoney = washingtotal.ToString("N2");
            model.RentTotalMoney = renttotal.ToString("N2");
            model.Detail = list;
            msgModel.Result = model;
            return msgModel;
        }

        public MsgModel HotelDetail([FromBody]WeChatHotelDetailParamModel requst)
        {
            MsgModel msg = new MsgModel();

            hotel_detail model = i_hotel_detail.Entities.Where(v => v.HotelID == requst.hotelId).FirstOrDefault();

            msg.Result = model;

            return msg;
        }

        public MsgModel ModifyHotelDetail([FromBody]WeChatHotelDetailParamModel requst)
        {
            MsgModel msg = new MsgModel();

            hotel_detail model = i_hotel_detail.Entities.Where(v => v.HotelID == requst.hotelId).FirstOrDefault();
            if (model == null)
            {
                model = new hotel_detail();
                model.HotelID = requst.hotelId;
                model.HotelImg = requst.hotelImg == null ? "" : requst.hotelImg;
                model.HotelName = requst.hotelName == null ? "" : requst.hotelName;
                model.HotelPoint = requst.hotelPoint == null ? "" : requst.hotelPoint;
                model.HotelProfile = requst.hotelProfile == null ? "" : requst.hotelProfile;
                model.HotelTel = requst.hotelTel == null ? "" : requst.hotelTel;
                model.Slogan = requst.slogan == null ? "" : requst.slogan;
                model.Logo = requst.logo == null ? "" : requst.logo;

                i_hotel_detail.Insert(model);
            }
            else
            {
                model.HotelID = requst.hotelId;
                //model.HotelImg = requst.hotelImg == null ? "" : requst.hotelImg;
                model.HotelName = requst.hotelName == null ? "" : requst.hotelName;
                model.HotelPoint = requst.hotelPoint == null ? "" : requst.hotelPoint;
                model.HotelProfile = requst.hotelProfile == null ? "" : requst.hotelProfile;
                model.HotelTel = requst.hotelTel == null ? "" : requst.hotelTel;
                model.Slogan = requst.slogan == null ? "" : requst.slogan;
                model.Logo = requst.logo == null ? "" : requst.logo;

                if (requst.delImg != null && requst.delImg.Length > 0)
                {
                    foreach (var u in requst.delImg)
                    {
                        model.HotelImg = model.HotelImg.Replace(u + ";", "");
                    }
                }

                i_hotel_detail.Update(model);
            }

            return msg;
        }

        public Task<HttpResponseMessage> UploadHotelImage(HttpRequestMessage request)
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                new Log4NetFile().Log("上传格式不是multipart/form-data");
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + @"UploadFile\Customer";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }


            var provider = new MultipartFormDataStreamProvider(path);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<string> files = new List<string>();

            var task = request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
            {
                HttpResponseMessage response = null;
                if (t.IsFaulted || t.IsCanceled)
                {
                    response = request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }
                else
                {
                    int hotelId = 0;
                    foreach (var key in provider.FormData.AllKeys)
                    {
                        hotelId = Convert.ToInt32(provider.FormData[key].ToString());
                    }

                    hotel_detail model = i_hotel_detail.Entities.Where(v => v.HotelID == hotelId).FirstOrDefault();

                    string dicName = DateTime.Now.ToString("yyyyMMdd");
                    string sfile = "";
                    foreach (var file in provider.FileData)
                    {
                        string fileName = file.Headers.ContentDisposition.FileName;
                        if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                        {
                            fileName = fileName.Trim('"');
                        }
                        if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                        {
                            fileName = Path.GetFileName(fileName);
                        }
                        String ext = System.IO.Path.GetExtension(fileName);
                        var newFileName = DateTime.Now.ToString("yyyyMMddhhmmssfff") + ext;
                        File.Copy(file.LocalFileName, Path.Combine(path, newFileName));
                        sfile += newFileName + ";";
                    }
                    if (model != null)
                    {
                        model.HotelImg = model.HotelImg + sfile;

                        i_hotel_detail.Update(model);
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
                return response;
            });
            return task;
        }

        public Task<HttpResponseMessage> UploadHotelLogo()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                new Log4NetFile().Log("上传格式不是multipart/form-data");
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + @"UploadFile\Customer";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }


            var provider = new MultipartFormDataStreamProvider(path);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<string> files = new List<string>();

            var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
            {
                HttpResponseMessage response = null;
                if (t.IsFaulted || t.IsCanceled)
                {
                    response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }
                else
                {
                    int hotelId = 0;
                    foreach (var key in provider.FormData.AllKeys)
                    {
                        hotelId = Convert.ToInt32(provider.FormData[key].ToString());
                    }

                    hotel_detail model = i_hotel_detail.Entities.Where(v => v.HotelID == hotelId).FirstOrDefault();

                    string dicName = DateTime.Now.ToString("yyyyMMdd");
                    string sfile = "";
                    foreach (var file in provider.FileData)
                    {
                        string fileName = file.Headers.ContentDisposition.FileName;
                        if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                        {
                            fileName = fileName.Trim('"');
                        }
                        if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                        {
                            fileName = Path.GetFileName(fileName);
                        }
                        String ext = System.IO.Path.GetExtension(fileName);
                        var newFileName = DateTime.Now.ToString("yyyyMMddhhmmssfff") + ext;
                        File.Copy(file.LocalFileName, Path.Combine(path, newFileName));
                        sfile = newFileName;
                    }
                    if (model != null)
                    {
                        model.Logo = sfile;

                        i_hotel_detail.Update(model);
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
                return response;
            });
            return task;
        }

        private int GetOrderNubmer()
        {
            lock (invLcker)
            {
                int rtn = 1;
                if (API.Global.OrderTime.ToString("yyyy/MM/dd") != DateTime.Now.ToString("yyyy/MM/dd"))
                {
                    API.Global.OrderTime = DateTime.Now;
                    API.Global.OrderNubmer = rtn;
                }
                else
                {
                    rtn = API.Global.OrderNubmer + 1;
                    API.Global.OrderNubmer = rtn;
                }
                return rtn;
            }
        }
    }
}
