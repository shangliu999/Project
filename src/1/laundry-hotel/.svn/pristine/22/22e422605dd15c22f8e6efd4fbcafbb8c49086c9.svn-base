using ETexsys.APIRequestModel;
using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.Common.Log;
using ETexsys.Common.Rabbit;
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
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    [SupportFilter]
    public class TextileController : ApiController
    {
        private static readonly object invLcker = new object();

        private static readonly string querysql = "SELECT t.ID,t.ClassID,tc.ClassName,tc.Sort AS ClassSort,t.SizeID,s.SizeName,s.Sort AS SizeSort,t.BrandID,bt.BrandName,bt.Sort AS BrandSort,t.TextileState,t.TagNo,t.RegisterTime,t.Washtimes,t.UpdateTime,t.HotelID,h.RegionName AS HotelName,t.RegionID,t.LastReceiveRegionID,rt.RFIDWashtime,tc.ClassLeft AS ClassLeft,rt.CostTime AS RFIDCostTime,tc.PackCount,t.LastReceiveInvID FROM textile AS t INNER JOIN rfidtag as rt ON t.tagno=RT.rfidtagno LEFT JOIN textileclass AS tc ON t.ClassID=tc.ID LEFT JOIN size AS s ON t.SizeID= s.ID LEFT JOIN brandtype AS bt on t.BrandID= BT.ID LEFT JOIN region as h on t.HotelID= h.ID WHERE RT.RFIDState=1 AND t.LogoutType= 0 AND t.IsFlag=1 ";

        [Dependency]
        public IRepository<rfidtag> i_rfidtag { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<v_textile_tag> i_v_textile_tag { get; set; }

        [Dependency]
        public IRepository<V_TextileTag> i_V_TextileTag { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<invoicerfid> i_invoicerfid { get; set; }

        [Dependency]
        public IRepository<invoiceattach> i_invoiceattch { get; set; }

        [Dependency]
        public IRepository<repeatop> i_repeatop { get; set; }

        [Dependency]
        public IRepository<task> i_task { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<scrapdetail> i_scrapdetail { get; set; }

        [Dependency]
        public IRepository<qrcode> i_qrcode { get; set; }

        [Dependency]
        public IRepository<rfidreplace> i_rfidreplace { get; set; }

        [Dependency]
        public IRepository<fc_invoice> i_fc_invoice { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_customer { get; set; }

        [Dependency]
        public IRepository<bag> i_bag { get; set; }

        [Dependency]
        public IRepository<truck> i_truck { get; set; }

        [Dependency]
        public IRepository<truck_textile> i_truck_textile { get; set; }

        [Dependency]
        public IRepository<textilegroup> i_textilegroup { get; set; }

        public MsgModel Register([FromBody]RegisterParamModel requestRegister)
        {
            MsgModel msgModel = new MsgModel();

            textile textileModel = null;
            List<textile> textileList = new List<textile>();
            rfidtag rfidtagModel = null;
            List<rfidtag> rfidtagList = new List<rfidtag>();
            //更新集合
            List<rfidtag> mrfidtagList = new List<rfidtag>();

            DateTime time = DateTime.Now;

            var tags = requestRegister.Tags.Distinct().ToList();

            List<textile> tList = i_textile.Entities.Where(v => tags.Contains(v.TagNo) && v.IsFlag == 1).ToList();
            if (tList.Count > 0)
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "绑定出现异常，部分纺织品已经绑定。";
                return msgModel;
            }
            List<rfidtag> rList = i_rfidtag.Entities.Where(v => tags.Contains(v.RFIDTagNo)).ToList();
            if (rList.Where(v => v.RFIDState == true).Count() > 0)
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "绑定出现异常，部分纺织品已经绑定。";
                return msgModel;
            }

            foreach (var t in tags)
            {
                //纺织品&芯片表都不存在正常数据，即可注册
                textileModel = new textile();
                textileModel.BrandID = requestRegister.BrandID;
                textileModel.ClassID = requestRegister.ClassID;
                textileModel.FabricID = requestRegister.FabricID;
                textileModel.TextileBrandID = requestRegister.TextileBrandID;
                textileModel.IsFlag = 1;
                textileModel.RegisterTime = time;
                textileModel.SizeID = requestRegister.SizeID;
                textileModel.TagNo = t;
                textileModel.TextileState = 0;
                textileModel.UpdateTime = time;
                textileModel.Washtimes = 0;
                textileModel.ColorID = requestRegister.ColorID;
                textileModel.RegionID = requestRegister.StoreID;
                textileModel.HotelID = 0;
                textileModel.LastReceiveRegionID = 0;
                textileModel.LogoutType = 0;

                textileList.Add(textileModel);

                rfidtagModel = rList.Where(v => v.RFIDTagNo == t).FirstOrDefault();
                if (rfidtagModel == null)
                {
                    rfidtagModel = new rfidtag();
                    rfidtagModel.CostTime = time;
                    rfidtagModel.RFIDState = true;
                    rfidtagModel.RFIDTagNo = t;
                    rfidtagModel.RFIDWashtime = 0;

                    rfidtagList.Add(rfidtagModel);
                }
                else
                {
                    rfidtagModel.RFIDState = true;
                    mrfidtagList.Add(rfidtagModel);
                }
            }

            i_textile.Insert(textileList, false);
            foreach (var item in mrfidtagList)
            {
                i_rfidtag.Update(item, false);
            }
            i_rfidtag.Insert(rfidtagList);

            return msgModel;
        }

        public MsgModel ComplexRFIDTagAnalysis([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            List<textilegroup> tgList = i_textilegroup.Entities.Where(v => requestRFIDTag.TagList.Contains(v.TextileTagNo)).OrderBy(v => v.TextileTagNo).ThenByDescending(v => v.CreateTime).ToList();
            tgList = tgList.Where((x, y) => tgList.FindIndex(z => z.TextileTagNo == x.TextileTagNo) == y).ToList();

            List<string> groupList = tgList.GroupBy(v => v.GroupNo).Select(v => v.Key).ToList();

            List<string> tagList = i_textilegroup.Entities.Where(v => groupList.Contains(v.GroupNo)).Select(v => v.TextileTagNo).ToList();

            tagList.AddRange(requestRFIDTag.TagList);

            tagList = tagList.Distinct().ToList();

            string tags = tagList.Aggregate((x, y) => x + "','" + y);
            string sql = string.Format("{0} AND t.TagNo IN ('{1}')", querysql, tags);

            var query = i_v_textile_tag.SQLQuery(sql, "");

            //var query = from t in i_v_textile_tag.Entities
            //            where tagList.Contains(t.TagNo)
            //            select new
            //            {
            //                t.ID,
            //                t.BrandID,
            //                t.BrandName,
            //                t.BrandSort,
            //                t.ClassID,
            //                t.ClassName,
            //                t.ClassSort,
            //                t.SizeID,
            //                t.SizeName,
            //                t.SizeSort,
            //                t.TextileState,
            //                t.TagNo,
            //                t.Washtimes,
            //                t.UpdateTime,
            //                t.RegisterTime,
            //                t.RegionID,
            //                t.HotelID,
            //                t.HotelName,
            //                t.RFIDWashtime,
            //                t.LastReceiveRegionID,
            //                t.ClassLeft,
            //                t.RFIDCostTime,
            //                t.PackCount
            //            };

            List<ResponseRFIDTagModel> list = new List<ResponseRFIDTagModel>();
            ResponseRFIDTagModel model = null;
            query.ToList().ForEach(q =>
            {
                model = new ResponseRFIDTagModel();
                model.ID = (int)q.ID;
                model.BrandID = (int)q.BrandID;
                model.BrandName = q.BrandName;
                model.BrandSort = (int)q.BrandSort;
                model.ClassID = (int)q.ClassID;
                model.ClassName = q.ClassName;
                model.ClassSort = q.ClassSort == null ? 0 : (int)q.ClassSort;
                model.SizeID = q.SizeID == null ? 0 : (int)q.SizeID;
                model.SizeName = q.SizeName;
                model.SizeSort = q.SizeSort == null ? 0 : (int)q.SizeSort;
                model.TagNo = q.TagNo;
                model.TextileState = (int)q.TextileState;
                model.Washtimes = (int)q.Washtimes;
                model.UpdateTime = q.UpdateTime;
                model.RegionID = q.RegionID;
                model.HotelID = q.HotelID;
                model.HotelName = q.HotelName;
                model.LastReceiveRegionID = q.LastReceiveRegionID;
                model.CostTime = (DateTime)q.RegisterTime;
                model.RFIDWashtime = q.RFIDWashtime;
                model.ClassLeft = q.ClassLeft == null ? 0 : (int)q.ClassLeft;
                model.RFIDCostTime = q.RFIDCostTime;
                model.PackCount = q.PackCount.HasValue ? q.PackCount.Value : 0;
                list.Add(model);
            });

            msg.Result = list;
            msg.OtherResult = requestRFIDTag.RequestTime;

            return msg;
        }

        /// <summary>
        /// 芯片解析
        /// </summary>
        /// <returns></returns>
        public MsgModel RFIDTagAnalysis([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msgModel = new MsgModel();

            string tags = requestRFIDTag.TagList.Aggregate((x, y) => x + "','" + y);
            string sql = string.Format("{0} AND t.TagNo IN ('{1}')", querysql, tags);

            var query = i_v_textile_tag.SQLQuery(sql, "");

            //var query = from t in i_v_textile_tag.Entities
            //            where requestRFIDTag.TagList.Contains(t.TagNo)
            //            select new
            //            {
            //                t.ID,
            //                t.BrandID,
            //                t.BrandName,
            //                t.BrandSort,
            //                t.ClassID,
            //                t.ClassName,
            //                t.ClassSort,
            //                t.SizeID,
            //                t.SizeName,
            //                t.SizeSort,
            //                t.TextileState,
            //                t.TagNo,
            //                t.Washtimes,
            //                t.UpdateTime,
            //                t.RegisterTime,
            //                t.RegionID,
            //                t.HotelID,
            //                t.HotelName,
            //                t.RFIDWashtime,
            //                t.LastReceiveRegionID,
            //                t.ClassLeft,
            //                t.RFIDCostTime,
            //                t.PackCount,
            //                t.LastReceiveInvID,
            //            };

            List<ResponseRFIDTagModel> list = new List<ResponseRFIDTagModel>();
            ResponseRFIDTagModel model = null;
            query.ToList().ForEach(q =>
            {
                model = new ResponseRFIDTagModel();
                model.ID = (int)q.ID;
                model.BrandID = (int)q.BrandID;
                model.BrandName = q.BrandName;
                model.BrandSort = (int)q.BrandSort;
                model.ClassID = (int)q.ClassID;
                model.ClassName = q.ClassName;
                model.ClassSort = q.ClassSort == null ? 0 : (int)q.ClassSort;
                model.SizeID = q.SizeID == null ? 0 : (int)q.SizeID;
                model.SizeName = q.SizeName;
                model.SizeSort = q.SizeSort == null ? 0 : (int)q.SizeSort;
                model.TagNo = q.TagNo;
                model.TextileState = (int)q.TextileState;
                model.Washtimes = (int)q.Washtimes;
                model.UpdateTime = q.UpdateTime;
                model.RegionID = q.RegionID;
                model.HotelID = q.HotelID;
                model.HotelName = q.HotelName;
                model.LastReceiveRegionID = q.LastReceiveRegionID;
                model.CostTime = (DateTime)q.RegisterTime;
                model.RFIDWashtime = q.RFIDWashtime;
                model.ClassLeft = q.ClassLeft == null ? 0 : (int)q.ClassLeft;
                model.RFIDCostTime = q.RFIDCostTime;
                model.PackCount = q.PackCount.HasValue ? q.PackCount.Value : 0;
                model.LastReceiveInvID = q.LastReceiveInvID;
                list.Add(model);
            });

            msgModel.Result = list;
            msgModel.OtherResult = requestRFIDTag.RequestTime;

            return msgModel;
        }

        public MsgModel InsertRFIDInvoice([FromBody]RFIDInvoiceParamModel requestInvoice)
        {
            MsgModel msgModel = new MsgModel();
            string guid = requestInvoice.GUID.ToUpper();

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

            string px = GetInvoicePX(requestInvoice.InvType);
            int nubmer = GetInvoiceNubmer();
            string invNo = string.Format("{0}{1}{2}", px, DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
            DateTime time = DateTime.Now;

            invoice inv = new invoice();
            Guid id = Guid.NewGuid();
            inv.ID = id.ToString();
            inv.InvNo = invNo;
            inv.InvType = requestInvoice.InvType;
            inv.InvSubType = requestInvoice.InvSubType;
            inv.DataType = 1;
            inv.InvState = 1;
            inv.HotelID = requestInvoice.HotelID;
            inv.RegionID = requestInvoice.RegionID;
            inv.Quantity = requestInvoice.Quantity;
            inv.Remrak = requestInvoice.Remrak;
            inv.TaskType = requestInvoice.TaskType;
            inv.CreateUserID = requestInvoice.CreateUserID;
            inv.CreateUserName = requestInvoice.CreateUserName;
            inv.CreateTime = time;
            inv.MacCode = requestInvoice.UUID;
            inv.Comfirmed = 0;
            inv.Processed = false;
            inv.PaymentStatus = 0;

            var query = from t in i_textile.Entities
                        join b in i_brandtype.Entities on t.BrandID equals b.ID
                        join c in i_textileclass.Entities on t.ClassID equals c.ID
                        join s in i_size.Entities on t.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        where t.IsFlag == 1 && requestInvoice.Tags.Contains(t.TagNo)
                        select new
                        {
                            t.ID,
                            t.TagNo,
                            t.ClassID,
                            c.ClassName,
                            t.SizeID,
                            SizeName = s == null ? "" : s.SizeName,
                            BrandID = b.ID,
                            b.BrandName,
                            t.RegionID
                        };
            List<invoicerfid> rfidList = new List<invoicerfid>();
            List<textile> textileList = new List<textile>();
            invoicerfid rfidmodel = null;
            List<int> textileId = new List<int>();
            query.ToList().ForEach(q =>
            {
                rfidmodel = new invoicerfid();
                rfidmodel.InvID = inv.ID;
                rfidmodel.InvNo = invNo;
                rfidmodel.InvType = requestInvoice.InvType;
                rfidmodel.InvSubType = requestInvoice.InvSubType;
                rfidmodel.HotelID = requestInvoice.HotelID;
                rfidmodel.RegionID = requestInvoice.RegionID;
                rfidmodel.SourceRegionID = q.RegionID.Value;
                rfidmodel.TextileTagNo = q.TagNo;
                rfidmodel.TextileID = q.ID;
                rfidmodel.InvCreateTime = time;
                rfidmodel.ClassID = q.ClassID;
                rfidmodel.ClassName = q.ClassName;
                rfidmodel.SizeID = q.SizeID;
                rfidmodel.SizeName = q.SizeName;
                rfidmodel.BrandID = q.BrandID;
                rfidmodel.BrandName = q.BrandName;
                rfidmodel.CreateUserID = requestInvoice.CreateUserID;
                rfidmodel.CreateUserName = requestInvoice.CreateUserName;
                rfidList.Add(rfidmodel);

                textileId.Add(q.ID);
            });

            textileList = i_textile.Entities.Where(v => textileId.Contains(v.ID)).ToList();

            List<invoicedetail> detailList = new List<invoicedetail>();
            invoicedetail detailModel = null;
            query.GroupBy(v => new { v.BrandID, v.BrandName, v.ClassID, v.ClassName, v.SizeID, v.SizeName })
                .Select(x => new { x.Key.BrandID, x.Key.BrandName, x.Key.ClassID, x.Key.ClassName, x.Key.SizeID, x.Key.SizeName, TextileCount = x.Count() })
                .ToList().ForEach(q =>
                {
                    detailModel = new invoicedetail();
                    detailModel.InvID = inv.ID;
                    detailModel.InvNo = invNo;
                    detailModel.InvType = requestInvoice.InvType;
                    detailModel.InvSubType = requestInvoice.InvSubType;
                    detailModel.DataType = 1;
                    detailModel.HotelID = requestInvoice.HotelID;
                    detailModel.RegionID = requestInvoice.RegionID;
                    detailModel.BrandID = q.BrandID;
                    detailModel.BrandName = q.BrandName;
                    detailModel.ClassID = q.ClassID;
                    detailModel.ClassName = q.ClassName;
                    detailModel.SizeID = q.SizeID;
                    detailModel.SizeName = q.SizeName;
                    detailModel.TextileCount = q.TextileCount;
                    detailModel.InvCreateTime = time;
                    detailList.Add(detailModel);
                });

            if (requestInvoice.InvType == 5)
            {
                //入厂补单
                List<textile> filltexilte = textileList.Where(v => v.TextileState == 2 || v.TextileState == 9).ToList();

                int isfilled = 0;
                string f = System.Configuration.ConfigurationManager.AppSettings["IsFilled"];
                int.TryParse(f, out isfilled);
                if (isfilled == 1 && filltexilte.Count > 0)
                {
                    try
                    {
                        FillInvoice(filltexilte, requestInvoice);
                    }
                    catch (Exception ex)
                    {
                        new Log4NetFile().Log("补单错误：" + ex.Message);
                    }
                }
            }

            //当为入厂单并且任务ID不为空时，需要修改污物送洗单的状态
            if (requestInvoice.InvType == 5 && requestInvoice.TaskInv != null)
            {
                List<invoice> recinvs = i_invoice.Entities.Where(c => requestInvoice.TaskInv.Contains(c.ID)).ToList();
                for (int i = 0; i < recinvs.Count; i++)
                {
                    invoice recinv = recinvs[i];
                    recinv.InvState = 2;

                    i_invoice.Update(recinv, false);
                }
            }

            invoiceattach attchModel = null;
            List<invoiceattach> attchList = new List<invoiceattach>();
            if (requestInvoice.Attach != null)
            {
                string rootpath = AppDomain.CurrentDomain.BaseDirectory;
                string filename = "";
                string path = string.Format("{0}/UploadFile/Sign", rootpath);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                for (int i = 0; i < requestInvoice.Attach.Count; i++)
                {
                    if (requestInvoice.Attach[i].Type == "img")
                    {
                        FileStream fs = null;
                        string picName = DateTime.Now.ToString("MMddHHmmssfff") + i + ".png";
                        filename = string.Format("{0}\\{1}", path, picName);

                        fs = File.Create(filename);
                        byte[] bytes = Convert.FromBase64String(requestInvoice.Attach[i].Value.Substring(22));
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();

                        attchModel = new invoiceattach();
                        attchModel.InvNo = invNo;
                        attchModel.ParamType = requestInvoice.Attach[i].Type;
                        attchModel.ParamValue = string.Format("UploadFile/Sign/{0}", picName);
                        attchList.Add(attchModel);
                    }
                    else
                    {
                        attchModel = new invoiceattach();
                        attchModel.InvNo = invNo;
                        attchModel.ParamType = requestInvoice.Attach[i].Type;
                        attchModel.ParamValue = requestInvoice.Attach[i].Value;
                        attchList.Add(attchModel);
                    }
                }
            }

            i_invoice.Insert(inv, false);
            i_invoicerfid.Insert(rfidList, false);
            i_invoiceattch.Insert(attchList, false);
            foreach (var item in textileList)
            {
                item.TextileState = requestInvoice.InvType;
                item.UpdateTime = time;
                if (requestInvoice.InvType == 1)
                {
                    item.LastReceiveInvID = id.ToString();
                }
                //入库需要修改位置信息
                else if (requestInvoice.InvType == 3 || requestInvoice.InvType == 7)
                {
                    item.RegionID = requestInvoice.RegionID;
                }
                i_textile.Update(item, false);
            }
            i_invoicedetail.Insert(detailList, false);

            fcModel.ResultMsg = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss");
            fcModel.State = 1;
            i_fc_invoice.Update(fcModel);

            sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
            string code = sc != null ? sc.Code : "A";

            RabbitMQModel mqModel = new RabbitMQModel();
            mqModel.Type = "Invoice";
            mqModel.Value = id.ToString();
            mqModel.Code = code;
            RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

            msgModel.Result = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss") + "|" + id.ToString();

            return msgModel;
        }

        public MsgModel SendTask([FromBody]SendTaskParamModel requestSendTask)
        {
            MsgModel msg = new MsgModel();

            List<ResponseSendTaskModel> list = new List<ResponseSendTaskModel>();

            if (requestSendTask.RegionMode == 1)
            {
                var query = from t in i_task.Entities
                            join r in i_region.Entities on t.RegionID equals r.ID
                            join r1 in i_region.Entities on r.ParentID equals r1.ID
                            join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where r1.ID == requestSendTask.ID && t.TaskCount > 0
                            select new
                            {
                                t.ClassID,
                                t.ClassName,
                                ClassSort = tc.Sort,
                                t.SizeID,
                                t.SizeName,
                                SizeSort = s == null ? 0 : s.Sort,
                                t.TaskTime,
                                t.TaskType,
                                TaskCount = t.TaskCount - t.CheckCount,
                            };

                ResponseSendTaskModel rstm = null;
                query.ToList().ForEach(c =>
                {
                    rstm = new ResponseSendTaskModel();

                    rstm.ClassID = c.ClassID;
                    rstm.ClassName = c.ClassName;
                    rstm.ClassSort = c.ClassSort;
                    rstm.SizeID = c.SizeID;
                    rstm.SizeName = c.SizeName;
                    rstm.SizeSort = c.SizeSort;
                    rstm.TaskTime = c.TaskTime;
                    rstm.TaskType = c.TaskType;
                    rstm.TaskCount = c.TaskCount;

                    list.Add(rstm);
                });
            }
            else
            {
                var query = from t in i_task.Entities
                            join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where t.RegionID == requestSendTask.ID && t.TaskCount > 0
                            select new
                            {
                                t.ClassID,
                                t.ClassName,
                                ClassSort = tc.Sort,
                                t.SizeID,
                                t.SizeName,
                                SizeSort = s == null ? 0 : s.Sort,
                                t.TaskTime,
                                t.TaskType,
                                TaskCount = t.TaskCount - t.CheckCount,
                            };

                ResponseSendTaskModel rstm = null;
                query.ToList().ForEach(c =>
                {
                    rstm = new ResponseSendTaskModel();

                    rstm.ClassID = c.ClassID;
                    rstm.ClassName = c.ClassName;
                    rstm.ClassSort = c.ClassSort;
                    rstm.SizeID = c.SizeID;
                    rstm.SizeName = c.SizeName;
                    rstm.SizeSort = c.SizeSort;
                    rstm.TaskTime = c.TaskTime;
                    rstm.TaskType = c.TaskType;
                    rstm.TaskCount = c.TaskCount;

                    list.Add(rstm);
                });
            }

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        public MsgModel SendTask1([FromBody]ParamBaseModel requestSendTask)
        {
            MsgModel msg = new MsgModel();

            List<ResponseSendTaskModel> list = new List<ResponseSendTaskModel>();

            var query = from t in i_task.Entities
                        join r in i_region.Entities on t.RegionID equals r.ID
                        join r1 in i_region.Entities on r.ParentID equals r1.ID
                        join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                        join s in i_size.Entities on t.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        select new
                        {
                            HotelID = r1.ID,
                            HotelName = r1.RegionName,
                            RegionMode = r1.RegionMode,
                            RegionID = r.ID,
                            RegionName = r.RegionName,
                            TaskTime = t.TaskTime,
                            TaskType = t.TaskType,
                            ClassID = tc.ID,
                            ClassName = tc.ClassName,
                            ClassSort = tc.Sort,
                            SizeID = s == null ? 0 : s.ID,
                            SizeName = s == null ? "" : s.SizeName,
                            SizeSort = s == null ? 0 : s.Sort,
                            CheckCount = t.CheckCount,
                            TaskCount = t.TaskCount,
                        };

            ResponseSendTaskModel rstm = null;
            query.ToList().ForEach(c =>
            {
                if (c.TaskCount - c.CheckCount > 0)
                {
                    rstm = new ResponseSendTaskModel();

                    rstm.HotelID = c.HotelID;
                    rstm.HotelName = c.HotelName;
                    rstm.RegionMode = c.RegionMode;
                    rstm.RegionID = c.RegionID;
                    rstm.RegionName = c.RegionName;
                    rstm.ClassID = c.ClassID;
                    rstm.ClassName = c.ClassName;
                    rstm.ClassSort = c.ClassSort;
                    rstm.SizeID = c.SizeID;
                    rstm.SizeName = c.SizeName;
                    rstm.SizeSort = c.SizeSort;
                    rstm.TaskTime = c.TaskTime;
                    rstm.TaskType = c.TaskType;
                    rstm.TaskCount = c.TaskCount - c.CheckCount;

                    list.Add(rstm);
                }
            });

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        /// <summary>
        /// 纺织品报废
        /// </summary>
        /// <param name="requstScrap"></param>
        /// <returns></returns>
        public MsgModel TextileScrap([FromBody]ScrapParamModel requstScrap)
        {
            MsgModel msgModel = new MsgModel();

            var query = from t in i_textile.Entities
                        join b in i_brandtype.Entities on t.BrandID equals b.ID
                        join c in i_textileclass.Entities on t.ClassID equals c.ID
                        join s in i_size.Entities on t.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        join h in i_region.Entities on t.HotelID equals h.ID into h_join
                        from h in h_join.DefaultIfEmpty()
                        where t.IsFlag == 1 && requstScrap.Tags.Contains(t.TagNo)
                        select new
                        {
                            t.ID,
                            t.TagNo,
                            t.ClassID,
                            c.ClassName,
                            t.SizeID,
                            SizeName = s == null ? "" : s.SizeName,
                            t.BrandID,
                            b.BrandName,
                            t.FabricID,
                            t.Washtimes,
                            t.UpdateTime,
                            h.RegionName
                        };
            DateTime scrapTime = DateTime.Now;
            List<scrapdetail> scrapList = new List<scrapdetail>();
            scrapdetail scrapModel = null;
            query.ToList().ForEach(q =>
            {
                scrapModel = new scrapdetail();
                scrapModel.TextileID = q.ID;
                scrapModel.RFIDTagNo = q.TagNo;
                scrapModel.ClassID = q.ClassID;
                scrapModel.ClassName = q.ClassName;
                scrapModel.SizeID = q.SizeID;
                scrapModel.SizeName = q.SizeName;
                scrapModel.FabricID = q.FabricID;
                scrapModel.BrandID = q.BrandID;
                scrapModel.BrandName = q.BrandName;
                scrapModel.ResponsibleType = requstScrap.ResponsibleType;
                scrapModel.ResponsibleID = requstScrap.HotelID;
                scrapModel.ResponsibleName = requstScrap.HotelName;
                scrapModel.ScrapID = requstScrap.ScrapID;
                scrapModel.ScrapName = requstScrap.ScrapName;
                scrapModel.Washtime = q.Washtimes;
                scrapModel.CreateTime = scrapTime;
                scrapModel.CreateUserID = requstScrap.CreateUserID;
                scrapModel.CreateUserName = requstScrap.CreateUserName;
                scrapModel.LastUsingArea = q.RegionName == null ? "" : q.RegionName;
                scrapModel.LastUsingTime = q.UpdateTime.Value;
                scrapList.Add(scrapModel);
            });

            List<textile> texteileList = i_textile.Entities.Where(v => v.IsFlag == 1 && requstScrap.Tags.Contains(v.TagNo)).ToList();
            foreach (var item in texteileList)
            {
                item.LogoutType = 1;
                item.LogoutTime = scrapTime;
                item.IsFlag = 0;
                i_textile.Update(item, false);
            }

            List<rfidtag> rfidtagList = i_rfidtag.Entities.Where(v => requstScrap.Tags.Contains(v.RFIDTagNo)).ToList();
            foreach (var item in rfidtagList)
            {
                item.RFIDState = false;
                i_rfidtag.Update(item, false);
            }

            i_scrapdetail.Insert(scrapList);

            return msgModel;
        }

        /// <summary>
        /// 纺织品信息清除
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public MsgModel TextileReset([FromBody]TextileResetParamModel requestParam)
        {
            MsgModel msgModel = new MsgModel();

            DateTime time = DateTime.Now;

            List<textile> texteileList = i_textile.Entities.Where(v => v.IsFlag == 1 && requestParam.TagList.Contains(v.TagNo)).ToList();
            foreach (var item in texteileList)
            {
                item.LogoutType = 2;
                item.LogoutTime = time;
                item.IsFlag = 0;
                i_textile.Update(item, false);
            }

            List<rfidtag> rfidtagList = i_rfidtag.Entities.Where(v => requestParam.TagList.Contains(v.RFIDTagNo)).ToList();
            int index = 0;
            foreach (var item in rfidtagList)
            {
                index++;
                item.RFIDState = false;
                if (index == rfidtagList.Count)
                {
                    i_rfidtag.Update(item);
                }
                else
                {
                    i_rfidtag.Update(item, false);
                }
            }

            return msgModel;
        }

        public MsgModel QRCodeCheckOut([FromBody]QRCodeBindingParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            qrcode model = i_qrcode.Entities.Where(v => v.QRCodeValue == requestParam.QRCode && v.RFIDTagNo == requestParam.RFIDTagNo).FirstOrDefault();
            if (model == null)
            {
                msg.ResultCode = 1;
            }
            else
            {
                if (!model.IsLock)
                {
                    model.IsLock = true;
                    i_qrcode.Update(model);
                }
                msg.ResultCode = 0;
            }
            return msg;
        }

        public MsgModel QRCodeBinding([FromBody]QRCodeBindingParamModel requestParam)
        {
            MsgModel msg = new MsgModel();
            qrcode model = null;
            List<qrcode> list = i_qrcode.Entities.Where(v => v.QRCodeValue == requestParam.QRCode || v.RFIDTagNo == requestParam.RFIDTagNo).ToList();
            if (list == null || list.Count == 0)
            {
                model = new qrcode();
                model.QRCodeValue = requestParam.QRCode;
                model.RFIDTagNo = requestParam.RFIDTagNo;
                model.BindingTime = DateTime.Now;

                i_qrcode.Insert(model);
            }
            else
            {
                if (list.Count == 1)
                {
                    model = list.FirstOrDefault();
                    if (model.RFIDTagNo == requestParam.RFIDTagNo && model.QRCodeValue != requestParam.QRCode)
                    {
                        if (model.IsLock)
                        {
                            msg.ResultCode = 1;
                            msg.OtherCode = "0";
                            msg.ResultMsg = "当前芯片已经锁定.";
                        }
                        else
                        {
                            msg.ResultCode = 1;
                            msg.OtherCode = "1";
                            msg.ResultMsg = "当前芯片已经绑定.";
                        }
                    }
                    else if (model.RFIDTagNo != requestParam.RFIDTagNo && model.QRCodeValue == requestParam.QRCode)
                    {
                        if (model.IsLock)
                        {
                            msg.ResultCode = 1;
                            msg.OtherCode = "0";
                            msg.ResultMsg = "当前二维码已经锁定.";
                        }
                        else
                        {
                            msg.ResultCode = 1;
                            msg.OtherCode = "1";
                            msg.ResultMsg = "当前二维码已经绑定.";
                        }
                    }
                }
                else
                {
                    var m = list.Where(v => v.IsLock == true).FirstOrDefault();
                    if (m == null)
                    {
                        msg.ResultCode = 1;
                        msg.OtherCode = "1";
                        msg.ResultMsg = "当前二维码&芯片已经分别绑定.";
                    }
                    else
                    {
                        msg.ResultCode = 1;
                        msg.OtherCode = "0";
                        msg.ResultMsg = "当前二维码或者芯片已经锁定.";
                    }
                }
            }
            return msg;
        }

        public MsgModel QRCodeRemoveBinding([FromBody]QRCodeBindingParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            qrcode model = i_qrcode.Entities.Where(v => v.QRCodeValue == requestParam.QRCode || v.RFIDTagNo == requestParam.RFIDTagNo).FirstOrDefault();

            if (model != null)
            {
                i_qrcode.Delete(model);

                model = new qrcode();
                model.QRCodeValue = requestParam.QRCode;
                model.RFIDTagNo = requestParam.RFIDTagNo;
                model.BindingTime = DateTime.Now;

                i_qrcode.Insert(model);
            }

            return msg;
        }

        public MsgModel RFIDTagAnalysisByQRCode([FromBody]RFIDRelaceQRCodeParamModel requestParam)
        {
            MsgModel msgModel = new MsgModel();

            qrcode qrmodel = i_qrcode.Entities.Where(v => v.QRCodeValue == requestParam.QRCode).OrderByDescending(v => v.BindingTime).FirstOrDefault();

            if (qrmodel != null)
            {
                string sql = string.Format("{0} AND t.TagNo='{1}'", querysql, qrmodel.RFIDTagNo);
                var query = i_v_textile_tag.SQLQuery(sql, "");

                //var query = from t in i_v_textile_tag.Entities
                //            where t.TagNo == qrmodel.RFIDTagNo
                //            select new
                //            {
                //                t.ID,
                //                t.BrandID,
                //                t.BrandName,
                //                t.BrandSort,
                //                t.ClassID,
                //                t.ClassName,
                //                t.ClassSort,
                //                t.SizeID,
                //                t.SizeName,
                //                t.SizeSort,
                //                t.TextileState,
                //                t.TagNo,
                //                t.Washtimes,
                //                t.UpdateTime,
                //                t.RegisterTime,
                //                t.RegionID,
                //                t.HotelID,
                //                t.HotelName,
                //                t.RFIDWashtime,
                //                t.LastReceiveRegionID,
                //                t.ClassLeft,
                //                t.RFIDCostTime,
                //                t.PackCount,
                //                t.LastReceiveInvID,
                //            };

                List<ResponseRFIDTagModel> list = new List<ResponseRFIDTagModel>();
                ResponseRFIDTagModel model = null;
                query.ToList().ForEach(q =>
                {
                    model = new ResponseRFIDTagModel();
                    model.ID = (int)q.ID;
                    model.BrandID = (int)q.BrandID;
                    model.BrandName = q.BrandName;
                    model.BrandSort = (int)q.BrandSort;
                    model.ClassID = (int)q.ClassID;
                    model.ClassName = q.ClassName;
                    model.ClassSort = q.ClassSort == null ? 0 : (int)q.ClassSort;
                    model.SizeID = q.SizeID == null ? 0 : (int)q.SizeID;
                    model.SizeName = q.SizeName;
                    model.SizeSort = q.SizeSort == null ? 0 : (int)q.SizeSort;
                    model.TagNo = q.TagNo;
                    model.TextileState = (int)q.TextileState;
                    model.Washtimes = (int)q.Washtimes;
                    model.UpdateTime = q.UpdateTime;
                    model.RegionID = q.RegionID;
                    model.HotelID = q.HotelID;
                    model.HotelName = q.HotelName;
                    model.LastReceiveRegionID = q.LastReceiveRegionID;
                    model.CostTime = (DateTime)q.RegisterTime;
                    model.RFIDWashtime = q.RFIDWashtime;
                    model.ClassLeft = q.ClassLeft == null ? 0 : (int)q.ClassLeft;
                    model.RFIDCostTime = q.RFIDCostTime;
                    model.PackCount = q.PackCount.HasValue ? q.PackCount.Value : 0;
                    model.LastReceiveInvID = q.LastReceiveInvID;
                    list.Add(model);
                });

                msgModel.Result = list;
                msgModel.OtherResult = requestParam.RequestTime;
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "二维码未绑定.";
            }
            return msgModel;
        }

        /// <summary>
        /// 芯片更换
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public MsgModel RFIDReplace([FromBody]RFIDReplaceParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            textile model = i_textile.Entities.Where(v => v.ID == requestParam.TextileID && v.TagNo == requestParam.OldTagNo && v.IsFlag == 1).FirstOrDefault();
            DateTime time = DateTime.Now;
            if (model != null)
            {
                textile newTextileModel = i_textile.Entities.Where(v => v.TagNo == requestParam.NewTagNo && v.IsFlag == 1).FirstOrDefault();
                rfidtag tagModel = i_rfidtag.Entities.Where(v => v.RFIDTagNo == requestParam.NewTagNo).FirstOrDefault();
                rfidtag oldTagModel = i_rfidtag.Entities.Where(v => v.RFIDTagNo == requestParam.OldTagNo).FirstOrDefault();

                if (newTextileModel != null)
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "待替换的芯片已经登记.";
                }
                else if (oldTagModel != null && oldTagModel.RFIDState)
                {
                    if (tagModel == null)
                    {
                        tagModel = new rfidtag();
                        tagModel.CostTime = time;
                        tagModel.RFIDState = true;
                        tagModel.RFIDTagNo = requestParam.NewTagNo;
                        tagModel.RFIDWashtime = 0;

                        i_rfidtag.Insert(tagModel, false);
                    }
                    else
                    {
                        tagModel.RFIDState = true;

                        i_rfidtag.Update(tagModel, false);
                    }
                    oldTagModel.RFIDState = false;
                    i_rfidtag.Update(oldTagModel, false);

                    model.TagNo = requestParam.NewTagNo;
                    model.UpdateTime = time;

                    i_textile.Update(model, false);

                    rfidreplace rfidreplace = new rfidreplace();
                    rfidreplace.NewTagNo = requestParam.NewTagNo;
                    rfidreplace.OldTagNo = requestParam.OldTagNo;
                    rfidreplace.TagWashtime = oldTagModel.RFIDWashtime;
                    rfidreplace.TextileID = model.ID;
                    rfidreplace.CreateTime = time;
                    rfidreplace.CreateUserID = requestParam.CreateUserId;

                    i_rfidreplace.Insert(rfidreplace);
                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "原纺织品信息错误.";
                }
            }
            else
            {
                msg.ResultCode = 1;
                msg.ResultMsg = "原纺织品信息错误.";
            }
            return msg;
        }

        public MsgModel BagRFIDTagAnalysis([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            List<ResponseBagRFIDTagModel> list = new List<ResponseBagRFIDTagModel>();
            ResponseBagRFIDTagModel model = null;

            var query = from t in i_bag.Entities where t.Flag == true && requestRFIDTag.TagList.Contains(t.BagRFIDNo) select new { t.ID, t.BagNo, t.BagRFIDNo };
            query.ToList().ForEach(q =>
            {
                model = new ResponseBagRFIDTagModel();
                model.BagID = q.ID;
                model.BagNo = q.BagNo;
                model.BagRFIDTagNo = q.BagRFIDNo;

                list.Add(model);
            });

            msg.OtherResult = requestRFIDTag.RequestTime;
            msg.Result = list;

            return msg;
        }

        public MsgModel LoadTextileByTruck([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msgModel = new MsgModel();

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT t.ID,t.ClassID,tc.ClassName,tc.Sort AS ClassSort,t.SizeID,s.SizeName,s.Sort AS SizeSort,t.BrandID,bt.BrandName,bt.Sort AS BrandSort,t.TextileState,t.TagNo,");
            sb.Append("t.RegisterTime,t.Washtimes,t.UpdateTime,t.HotelID,h.RegionName AS HotelName,t.RegionID,t.LastReceiveRegionID,rt.RFIDWashtime,tc.ClassLeft AS ClassLeft,rt.CostTime AS RFIDCostTime,");
            sb.Append("tc.PackCount,t.LastReceiveInvID,tt.TruckRFIDNo FROM textile AS t INNER JOIN rfidtag as rt ON t.tagno=RT.rfidtagno LEFT JOIN textileclass AS tc ON t.ClassID=tc.ID ");
            sb.Append("LEFT JOIN size AS s ON t.SizeID=s.ID LEFT JOIN brandtype AS bt on t.BrandID=BT.ID LEFT JOIN region as h on t.HotelID=h.ID LEFT JOIN truck_textile AS tt ON t.TagNo=tt.TextileTagNo ");
            sb.Append("WHERE RT.RFIDState=1 AND t.LogoutType=0 AND t.IsFlag=1 ");

            string tags = requestRFIDTag.TagList.Aggregate((x, y) => x + "','" + y);
            string sql = string.Format("{0} AND tt.TruckRFIDNo IN ('{1}')", sb.ToString(), tags);
            var query = i_V_TextileTag.SQLQuery(sql, "");

            //var query = from t in i_v_textile_tag.Entities
            //            join tr in i_truck_textile.Entities on t.ID equals tr.TextileID
            //            where requestRFIDTag.TagList.Contains(tr.TruckRFIDNo)
            //            select new
            //            {
            //                t.ID,
            //                t.BrandID,
            //                t.BrandName,
            //                t.BrandSort,
            //                t.ClassID,
            //                t.ClassName,
            //                t.ClassSort,
            //                t.SizeID,
            //                t.SizeName,
            //                t.SizeSort,
            //                t.TextileState,
            //                t.TagNo,
            //                t.Washtimes,
            //                t.UpdateTime,
            //                t.RegisterTime,
            //                t.RegionID,
            //                t.HotelID,
            //                t.HotelName,
            //                t.RFIDWashtime,
            //                t.LastReceiveRegionID,
            //                t.ClassLeft,
            //                t.RFIDCostTime,
            //                tr.TruckRFIDNo
            //            };

            List<ResponseRFIDTagModel> list = new List<ResponseRFIDTagModel>();
            ResponseRFIDTagModel model = null;
            query.ToList().ForEach(q =>
            {
                model = new ResponseRFIDTagModel();
                model.ID = (int)q.ID;
                model.BrandID = (int)q.BrandID;
                model.BrandName = q.BrandName;
                model.BrandSort = (int)q.BrandSort;
                model.ClassID = (int)q.ClassID;
                model.ClassName = q.ClassName;
                model.ClassSort = q.ClassSort == null ? 0 : (int)q.ClassSort;
                model.SizeID = q.SizeID == null ? 0 : (int)q.SizeID;
                model.SizeName = q.SizeName;
                model.SizeSort = q.SizeSort == null ? 0 : (int)q.SizeSort;
                model.TagNo = q.TagNo;
                model.TextileState = (int)q.TextileState;
                model.Washtimes = (int)q.Washtimes;
                model.UpdateTime = q.UpdateTime;
                model.RegionID = q.RegionID;
                model.HotelID = q.HotelID;
                model.HotelName = q.HotelName;
                model.LastReceiveRegionID = q.LastReceiveRegionID;
                model.CostTime = (DateTime)q.RegisterTime;
                model.RFIDWashtime = q.RFIDWashtime;
                model.ClassLeft = q.ClassLeft == null ? 0 : (int)q.ClassLeft;
                model.RFIDCostTime = q.RFIDCostTime;
                model.TruckTagNo = q.TruckRFIDNo;
                list.Add(model);
            });

            msgModel.Result = list;
            msgModel.OtherResult = requestRFIDTag.RequestTime;

            return msgModel;
        }

        public MsgModel TruckRFIDTagAnalysis([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            List<ResponseTruckRFIDTagModel> list = new List<ResponseTruckRFIDTagModel>();
            ResponseTruckRFIDTagModel model = null;

            var query = from t in i_truck.Entities where t.Flag == true && requestRFIDTag.TagList.Contains(t.TruckRFIDNo) select new { t.ID, t.TruckRFIDNo };
            query.ToList().ForEach(q =>
            {
                model = new ResponseTruckRFIDTagModel();
                model.TruckID = q.ID;
                model.TrunckRFIDTagNo = q.TruckRFIDNo;

                list.Add(model);
            });

            msg.OtherResult = requestRFIDTag.RequestTime;
            msg.Result = list;

            return msg;
        }

        public MsgModel AssetsReg([FromBody]AssetsRegParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            DateTime time = DateTime.Now;
            bool abnormal = false;

            if (requestParam.AssetsType == 1)
            {
                if (i_bag.Entities.Where(v => v.Flag == true && v.BagNo == requestParam.Code).Count() == 0)
                {
                    List<bag> bagList = new List<bag>();
                    foreach (var t in requestParam.Tags)
                    {
                        if (i_bag.Entities.Where(v => v.Flag == true && v.BagRFIDNo == t).Count() == 0)
                        {
                            bagList.Add(new bag { BagRFIDNo = t, BagNo = requestParam.Code, CreateTime = time, CreateUserID = requestParam.UserID, Flag = true, IsDelete = false, RegionID = 0, State = 3 });
                        }
                        else { abnormal = true; }
                    }
                    if (!abnormal)
                    {
                        i_bag.Insert(bagList);
                    }
                    else
                    {
                        msg.ResultCode = 1;
                    }
                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "编码已经存在";
                }
            }
            else
            {
                if (i_truck.Entities.Where(v => v.Flag == true && v.TruckNo == requestParam.Code).Count() == 0)
                {

                    List<truck> frameList = new List<truck>();
                    foreach (var t in requestParam.Tags)
                    {
                        if (i_truck.Entities.Where(v => v.Flag == true && v.TruckRFIDNo == t).Count() == 0)
                        {
                            frameList.Add(new truck { TruckRFIDNo = t, TruckNo = requestParam.Code, CreateTime = time, CreateUserID = requestParam.UserID, Flag = true, RegionID = 0, State = 1 });
                        }
                        else { abnormal = true; }
                    }
                    if (!abnormal)
                    {
                        i_truck.Insert(frameList);
                    }
                    else
                    {
                        msg.ResultCode = 1;
                    }
                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "编码已经存在";
                }
            }
            return msg;
        }

        public MsgModel TextileGrouping([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            var data = i_textile.Entities.Where(v => v.IsFlag == 1 && requestRFIDTag.TagList.Contains(v.TagNo)).ToList();
            if (data.Count > 0)
            {
                List<textile> textileList = new List<textile>();
                var classCount = data.GroupBy(v => new { v.ClassID, v.SizeID }).Count();
                if (classCount == 1)
                {
                    int classId = data.FirstOrDefault().ClassID;
                    var packcount = i_textileclass.Entities.Where(v => v.ID == classId).FirstOrDefault().PackCount.Value;
                    if (packcount == data.Count && packcount > 0)
                    {
                        List<textilegroup> tglist = new List<textilegroup>();
                        textilegroup model = null;
                        string guid = Guid.NewGuid().ToString();
                        DateTime time = DateTime.Now;
                        foreach (var item in data)
                        {
                            model = new textilegroup();
                            model.GroupNo = guid;
                            model.TextileID = item.ID;
                            model.TextileTagNo = item.TagNo;
                            model.CreateTime = time;
                            tglist.Add(model);

                            item.TextileState = 11;
                            item.UpdateTime = time;
                            i_textile.Update(item, false);
                        }

                        i_textilegroup.Insert(tglist);
                    }
                    else
                    {
                        msg.ResultCode = 1;
                        msg.ResultMsg = "打扎数量不一致.";
                    }
                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "多种类型不能同时打扎.";
                }

            }
            else
            {
                msg.ResultCode = 1;
                msg.ResultMsg = "纺织品未登记";
            }

            return msg;
        }

        public MsgModel RemoveTextileGroupingAnalysis([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            List<textilegroup> tgList = i_textilegroup.Entities.Where(t => requestRFIDTag.TagList.Contains(t.TextileTagNo)).OrderBy(v => v.TextileTagNo).ThenByDescending(v => v.CreateTime).ToList();
            tgList = tgList.Where((x, y) => tgList.FindIndex(z => z.TextileTagNo == x.TextileTagNo) == y).ToList();

            string tags = requestRFIDTag.TagList.Aggregate((x, y) => x + "','" + y);
            string sql = string.Format("{0} AND t.TagNo IN ('{1}')", querysql, tags);
            var query = i_v_textile_tag.SQLQuery(sql, "");

            //var query = from t in i_v_textile_tag.Entities
            //            where requestRFIDTag.TagList.Contains(t.TagNo)
            //            select new
            //            {
            //                t.ID,
            //                t.BrandID,
            //                t.BrandName,
            //                t.BrandSort,
            //                t.ClassID,
            //                t.ClassName,
            //                t.ClassSort,
            //                t.SizeID,
            //                t.SizeName,
            //                t.SizeSort,
            //                t.TextileState,
            //                t.TagNo,
            //                t.Washtimes,
            //                t.UpdateTime,
            //                t.RegisterTime,
            //                t.RegionID,
            //                t.HotelID,
            //                t.HotelName,
            //                t.RFIDWashtime,
            //                t.LastReceiveRegionID,
            //                t.ClassLeft,
            //                t.RFIDCostTime,
            //                t.PackCount
            //            };

            List<ResponseRFIDTagModel> list = new List<ResponseRFIDTagModel>();
            ResponseRFIDTagModel model = null;
            query.ToList().ForEach(q =>
            {
                model = new ResponseRFIDTagModel();
                model.ID = (int)q.ID;
                model.BrandID = (int)q.BrandID;
                model.BrandName = q.BrandName;
                model.BrandSort = (int)q.BrandSort;
                model.ClassID = (int)q.ClassID;
                model.ClassName = q.ClassName;
                model.ClassSort = q.ClassSort == null ? 0 : (int)q.ClassSort;
                model.SizeID = q.SizeID == null ? 0 : (int)q.SizeID;
                model.SizeName = q.SizeName;
                model.SizeSort = q.SizeSort == null ? 0 : (int)q.SizeSort;
                model.TagNo = q.TagNo;
                model.TextileState = (int)q.TextileState;
                model.Washtimes = (int)q.Washtimes;
                model.UpdateTime = q.UpdateTime;
                model.RegionID = q.RegionID;
                model.HotelID = q.HotelID;
                model.HotelName = q.HotelName;
                model.LastReceiveRegionID = q.LastReceiveRegionID;
                model.CostTime = (DateTime)q.RegisterTime;
                model.RFIDWashtime = q.RFIDWashtime;
                model.ClassLeft = q.ClassLeft == null ? 0 : (int)q.ClassLeft;
                model.RFIDCostTime = q.RFIDCostTime;
                model.PackCount = q.PackCount.HasValue ? q.PackCount.Value : 0;
                var groupModel = tgList.Where(v => v.TextileTagNo == q.TagNo).FirstOrDefault();
                model.VirtualTagNo = groupModel == null ? "" : groupModel.GroupNo;
                list.Add(model);
            });

            msg.Result = list;
            msg.OtherResult = requestRFIDTag.RequestTime;

            return msg;
        }

        public MsgModel RemoveTextileGrouping([FromBody]RFIDTagAnalysisParamModel requestRFIDTag)
        {
            MsgModel msg = new MsgModel();

            List<textilegroup> tgList = i_textilegroup.Entities.Where(v => requestRFIDTag.TagList.Contains(v.TextileTagNo)).OrderBy(v => v.TextileTagNo).ThenByDescending(v => v.CreateTime).ToList();
            List<string> groupList = tgList.GroupBy(v => v.GroupNo).Select(v => v.Key).ToList();

            List<textilegroup> removeList = i_textilegroup.Entities.Where(v => groupList.Contains(v.GroupNo)).ToList();

            int rtn = i_textilegroup.Delete(removeList);

            msg.ResultCode = rtn > 0 ? 0 : 1;

            return msg;
        }

        public MsgModel InfactoryTask([FromBody]InfactoryTaskParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            List<ResponseInfactoryTaskModel> list = new List<ResponseInfactoryTaskModel>();

            DateTime date = DateTime.Now.Date.AddDays(-3);

            var query = from i in i_invoice.Entities
                        join h in i_region.Entities on i.HotelID equals h.ID
                        join r in i_region.Entities on i.RegionID equals r.ID
                        where i.InvType == 1 && i.DataType == 1 && i.InvState == 1 && i.CreateTime >= date//&& i.HotelID == requestParam.HotelID
                        orderby i.CreateTime descending, h.RegionName, r.RegionName
                        select new
                        {
                            i.ID,
                            i.InvNo,
                            HotelName = h.RegionName,
                            r.RegionName,
                            i.Quantity
                        };

            ResponseInfactoryTaskModel ritm = null;
            query.ToList().ForEach(c =>
            {
                ritm = new ResponseInfactoryTaskModel();

                ritm.InvID = c.ID;
                ritm.InvNo = c.InvNo;
                ritm.HotelName = c.HotelName;
                ritm.RegionName = c.RegionName;
                ritm.TaskCount = c.Quantity;
                ritm.TextileCount = 0;
                ritm.DiffCount = -c.Quantity;

                list.Add(ritm);
            });

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        public MsgModel TextileFlow([FromBody]TextileFlowParamModel requstParam)
        {
            MsgModel msg = new MsgModel();

            List<ResponseTextileFlowModel> list = new List<ResponseTextileFlowModel>();

            List<invoicerfid> rfidList = new List<invoicerfid>();

            if (requstParam.Type == 1)
            {
                List<int> type = new List<int>();
                type.Add(1);
                type.Add(2);
                //收发
                rfidList = i_invoicerfid.Entities.Where(v => v.TextileID == requstParam.TextileId && type.Contains(v.InvType)).OrderByDescending(v => v.InvCreateTime).ToList();
            }
            else
            {
                rfidList = i_invoicerfid.Entities.Where(v => v.TextileID == requstParam.TextileId).OrderByDescending(v => v.InvCreateTime).ToList();
            }

            List<region> regionList = i_region.Entities.Where(v => v.IsDelete == false).ToList();

            ResponseTextileFlowModel model = null;
            rfidList.ForEach(q =>
            {
                model = new ResponseTextileFlowModel();
                region rmodel = regionList.Where(v => v.ID == q.HotelID).FirstOrDefault();
                model.PositionName = rmodel == null ? "" : rmodel.RegionName;
                model.FlowName = GetFlowName(q.InvType);
                model.OperationUser = q.CreateUserName;
                model.OperationTime = q.InvCreateTime.ToString("yyyy/MM/dd HH:mm");
                list.Add(model);
            });

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        public MsgModel TextileMergeTruck([FromBody]TextileMergeParamModel requstParam)
        {
            MsgModel msg = new MsgModel();

            List<textile> list = i_textile.Entities.Where(v => v.IsFlag == 1 && requstParam.Tags.Contains(v.TagNo)).ToList();
            List<truck_textile> truck_textileList = new List<truck_textile>();
            truck_textile model = null;
            foreach (var item in list)
            {
                model = new truck_textile();
                model.TextileID = item.ID;
                model.TextileTagNo = item.TagNo;
                model.TruckRFIDNo = requstParam.TruckNo;
                truck_textileList.Add(model);
            }

            var delTruckList = i_truck_textile.Entities.Where(v => requstParam.Tags.Contains(v.TextileTagNo)).GroupBy(v => v.TruckRFIDNo).Select(v => new { v.Key }).ToList();
            foreach (var ditem in delTruckList)
            {
                truck truckModel = i_truck.Entities.Where(v => v.TruckRFIDNo == ditem.Key).FirstOrDefault();
                if (truckModel != null)
                {
                    truckModel.State = 1;
                    i_truck.Update(truckModel, false);
                }
            }
            i_truck_textile.Delete(p => requstParam.Tags.Contains(p.TextileTagNo), false);
            i_truck_textile.Insert(truck_textileList, false);
            truck tModel = i_truck.Entities.Where(v => v.TruckRFIDNo == requstParam.TruckNo).FirstOrDefault();
            tModel.State = 2;
            i_truck.Update(tModel);

            return msg;
        }

        #region android

        public MsgModel InsertLogisticsRFIDInvoice([FromBody]RFIDInvoiceParamModel requestInvoice)
        {
            MsgModel msgModel = new MsgModel();
            string guid = requestInvoice.GUID.ToUpper();

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

            string px = GetInvoicePX(requestInvoice.InvType);
            int nubmer = GetInvoiceNubmer();
            string invNo = string.Format("{0}{1}{2}", px, DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
            DateTime time = DateTime.Now;

            invoice inv = new invoice();
            Guid id = Guid.NewGuid();
            inv.ID = id.ToString();
            inv.InvNo = invNo;
            inv.InvType = requestInvoice.InvType;
            inv.InvSubType = requestInvoice.InvSubType;
            inv.DataType = 1;
            inv.InvState = 1;
            inv.HotelID = requestInvoice.HotelID;
            inv.RegionID = requestInvoice.RegionID;
            inv.Quantity = requestInvoice.Quantity;
            inv.Remrak = requestInvoice.Remrak;
            inv.TaskType = requestInvoice.TaskType;
            inv.CreateUserID = requestInvoice.CreateUserID;
            inv.CreateUserName = requestInvoice.CreateUserName;
            inv.CreateTime = time;
            inv.MacCode = requestInvoice.UUID;
            inv.Comfirmed = 0;
            inv.Processed = false;
            inv.PaymentStatus = 0;

            if (requestInvoice.ConfirmTime != null && requestInvoice.ConfirmTime != DateTime.MinValue)
                inv.ConfirmTime = requestInvoice.ConfirmTime;
            if (requestInvoice.ConfirmUserID != 0)
                inv.ConfirmUserID = requestInvoice.ConfirmUserID;
            if (!string.IsNullOrEmpty(requestInvoice.ConfirmUserName))
                inv.ConfirmUserName = requestInvoice.ConfirmUserName;

            //是否开启重复收货
            int isRepeat = 0;
            string repeat = System.Configuration.ConfigurationManager.AppSettings["IsRepeat"];
            int.TryParse(repeat, out isRepeat);

            List<invoicerfid> rfidList = new List<invoicerfid>();
            List<textile> textileList = new List<textile>();
            List<invoicedetail> detailList = new List<invoicedetail>();
            List<repeatop> repopList = new List<repeatop>();
            List<invoice> invoiceList = new List<invoice>();

            if (requestInvoice.InvType == 1)
            {
                #region 污物送洗

                var query = from t in i_textile.Entities
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where t.IsFlag == 1 && requestInvoice.Tags.Contains(t.TagNo)
                            select new
                            {
                                t.ID,
                                t.TagNo,
                                t.ClassID,
                                c.ClassName,
                                t.SizeID,
                                t.TextileState,
                                SizeName = s == null ? "" : s.SizeName,
                                BrandID = b.ID,
                                b.BrandName,
                                t.RegionID
                            };

                if (isRepeat == 1)
                {
                    query = query.Where(c => c.TextileState != 1);
                }

                invoicerfid rfidmodel = null;
                List<int> textileId = new List<int>();
                query.ToList().ForEach(q =>
                {
                    rfidmodel = new invoicerfid();
                    rfidmodel.InvID = inv.ID;
                    rfidmodel.InvNo = invNo;
                    rfidmodel.InvType = requestInvoice.InvType;
                    rfidmodel.InvSubType = requestInvoice.InvSubType;
                    rfidmodel.HotelID = requestInvoice.HotelID;
                    rfidmodel.RegionID = requestInvoice.RegionID;
                    rfidmodel.SourceRegionID = q.RegionID.Value;
                    rfidmodel.TextileTagNo = q.TagNo;
                    rfidmodel.TextileID = q.ID;
                    rfidmodel.InvCreateTime = time;
                    rfidmodel.ClassID = q.ClassID;
                    rfidmodel.ClassName = q.ClassName;
                    rfidmodel.SizeID = q.SizeID;
                    rfidmodel.SizeName = q.SizeName;
                    rfidmodel.BrandID = q.BrandID;
                    rfidmodel.BrandName = q.BrandName;
                    rfidmodel.CreateUserID = requestInvoice.CreateUserID;
                    rfidmodel.CreateUserName = requestInvoice.CreateUserName;
                    rfidList.Add(rfidmodel);

                    textileId.Add(q.ID);
                });

                textileList = i_textile.Entities.Where(v => textileId.Contains(v.ID)).ToList();

                invoicedetail detailModel = null;
                query.GroupBy(v => new { v.BrandID, v.BrandName, v.ClassID, v.ClassName, v.SizeID, v.SizeName })
                    .Select(x => new { x.Key.BrandID, x.Key.BrandName, x.Key.ClassID, x.Key.ClassName, x.Key.SizeID, x.Key.SizeName, TextileCount = x.Count() })
                    .ToList().ForEach(q =>
                    {
                        detailModel = new invoicedetail();
                        detailModel.InvID = inv.ID;
                        detailModel.InvNo = invNo;
                        detailModel.InvType = requestInvoice.InvType;
                        detailModel.InvSubType = requestInvoice.InvSubType;
                        detailModel.DataType = 1;
                        detailModel.HotelID = requestInvoice.HotelID;
                        detailModel.RegionID = requestInvoice.RegionID;
                        detailModel.BrandID = q.BrandID;
                        detailModel.BrandName = q.BrandName;
                        detailModel.ClassID = q.ClassID;
                        detailModel.ClassName = q.ClassName;
                        detailModel.SizeID = q.SizeID;
                        detailModel.SizeName = q.SizeName;
                        detailModel.TextileCount = q.TextileCount;
                        detailModel.InvCreateTime = time;
                        detailList.Add(detailModel);
                    });

                if (isRepeat == 1)
                {
                    var query1 = from t in i_textile.Entities
                                 join r in i_region.Entities on t.LastReceiveRegionID equals r.ID into r_join
                                 from r in r_join.DefaultIfEmpty()
                                 join r1 in i_region.Entities on r.ParentID equals r1.ID into r1_join
                                 from r1 in r1_join.DefaultIfEmpty()
                                 where t.IsFlag == 1 && t.TextileState == 1 && requestInvoice.Tags.Contains(t.TagNo)
                                 select new
                                 {
                                     t.ID,
                                     t.TagNo,
                                     t.ClassID,
                                     t.SizeID,
                                     t.UpdateTime,
                                     t.LastReceiveRegionID,
                                     PreHotelID = r1 == null ? 0 : r1.ID,
                                 };

                    repeatop repeatopModel = null;
                    query1.ToList().ForEach(q =>
                    {
                        repeatopModel = new repeatop();

                        repeatopModel.InvID = id.ToString();
                        repeatopModel.PreHotelID = q.PreHotelID;
                        repeatopModel.PreRegionID = q.LastReceiveRegionID;
                        repeatopModel.PreCreateTime = Convert.ToDateTime(q.UpdateTime);
                        repeatopModel.TagNo = q.TagNo;
                        repeatopModel.TextileID = q.ID;
                        repeatopModel.ClassID = q.ClassID;
                        repeatopModel.SizeID = q.SizeID;
                        repeatopModel.HotelID = requestInvoice.HotelID;
                        repeatopModel.RegionID = requestInvoice.RegionID;
                        repeatopModel.CreateTime = time;
                        repeatopModel.InvType = requestInvoice.InvType;

                        repopList.Add(repeatopModel);
                    });
                }

                #endregion
            }
            else
            {
                List<string> tagList = new List<string>();

                if (requestInvoice.InvType == 9 && requestInvoice.CreateMode == 1)
                {
                    #region 净物签收，签收依据为配送任务

                    tagList = i_invoicerfid.Entities.Where(c => requestInvoice.TaskInv.Contains(c.InvID)).Select(c => c.TextileTagNo).ToList();

                    #endregion
                }
                else
                {
                    tagList.AddRange(requestInvoice.Tags);
                }
                //扫描签收时，标记净物配送的任务
                if (requestInvoice.InvType == 9 && requestInvoice.TaskInv != null && requestInvoice.TaskInv.Count > 0)
                {
                    invoiceList = i_invoice.Entities.Where(c => requestInvoice.TaskInv.Contains(c.ID)).ToList();
                }

                #region 其他

                var query = from t in i_textile.Entities
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where t.IsFlag == 1 && tagList.Contains(t.TagNo)
                            select new
                            {
                                t.ID,
                                t.TagNo,
                                t.ClassID,
                                c.ClassName,
                                t.SizeID,
                                SizeName = s == null ? "" : s.SizeName,
                                BrandID = b.ID,
                                b.BrandName,
                                t.RegionID
                            };

                invoicerfid rfidmodel = null;
                List<int> textileId = new List<int>();
                query.ToList().ForEach(q =>
                {
                    rfidmodel = new invoicerfid();
                    rfidmodel.InvID = inv.ID;
                    rfidmodel.InvNo = invNo;
                    rfidmodel.InvType = requestInvoice.InvType;
                    rfidmodel.InvSubType = requestInvoice.InvSubType;
                    rfidmodel.HotelID = requestInvoice.HotelID;
                    rfidmodel.RegionID = requestInvoice.RegionID;
                    rfidmodel.SourceRegionID = q.RegionID.Value;
                    rfidmodel.TextileTagNo = q.TagNo;
                    rfidmodel.TextileID = q.ID;
                    rfidmodel.InvCreateTime = time;
                    rfidmodel.ClassID = q.ClassID;
                    rfidmodel.ClassName = q.ClassName;
                    rfidmodel.SizeID = q.SizeID;
                    rfidmodel.SizeName = q.SizeName;
                    rfidmodel.BrandID = q.BrandID;
                    rfidmodel.BrandName = q.BrandName;
                    rfidmodel.CreateUserID = requestInvoice.CreateUserID;
                    rfidmodel.CreateUserName = requestInvoice.CreateUserName;
                    rfidList.Add(rfidmodel);

                    textileId.Add(q.ID);
                });

                textileList = i_textile.Entities.Where(v => textileId.Contains(v.ID)).ToList();
                //净物签收时的合计
                if (requestInvoice.InvType == 9)
                {
                    inv.Quantity = textileList.Count;
                }

                invoicedetail detailModel = null;
                query.GroupBy(v => new { v.BrandID, v.BrandName, v.ClassID, v.ClassName, v.SizeID, v.SizeName })
                    .Select(x => new { x.Key.BrandID, x.Key.BrandName, x.Key.ClassID, x.Key.ClassName, x.Key.SizeID, x.Key.SizeName, TextileCount = x.Count() })
                    .ToList().ForEach(q =>
                    {
                        detailModel = new invoicedetail();
                        detailModel.InvID = inv.ID;
                        detailModel.InvNo = invNo;
                        detailModel.InvType = requestInvoice.InvType;
                        detailModel.InvSubType = requestInvoice.InvSubType;
                        detailModel.DataType = 1;
                        detailModel.HotelID = requestInvoice.HotelID;
                        detailModel.RegionID = requestInvoice.RegionID;
                        detailModel.BrandID = q.BrandID;
                        detailModel.BrandName = q.BrandName;
                        detailModel.ClassID = q.ClassID;
                        detailModel.ClassName = q.ClassName;
                        detailModel.SizeID = q.SizeID;
                        detailModel.SizeName = q.SizeName;
                        detailModel.TextileCount = q.TextileCount;
                        detailModel.InvCreateTime = time;
                        detailList.Add(detailModel);
                    });

                #endregion
            }

            if (requestInvoice.InvType == 5)
            {
                //入厂补单
                List<textile> filltexilte = textileList.Where(v => v.TextileState == 2 || v.TextileState == 9).ToList();

                int isfilled = 0;
                string f = System.Configuration.ConfigurationManager.AppSettings["IsFilled"];
                int.TryParse(f, out isfilled);
                if (isfilled == 1 && filltexilte.Count > 0)
                {
                    try
                    {
                        FillInvoice(filltexilte, requestInvoice);
                    }
                    catch (Exception ex)
                    {
                        new Log4NetFile().Log("补单错误：" + ex.Message);
                    }
                }
            }

            #region 重新组装AttchModel

            List<AttchModel> attchModels = new List<AttchModel>();
            AttchModel am = null;
            if (requestInvoice.TaskInv != null)
            {
                for (int i = 0; i < requestInvoice.TaskInv.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "TaskID";
                    am.Value = requestInvoice.TaskInv[i];
                    attchModels.Add(am);
                }
            }

            if (requestInvoice.Bag != null)
            {
                for (int i = 0; i < requestInvoice.Bag.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "Bag";
                    am.Value = requestInvoice.Bag[i];
                    attchModels.Add(am);
                }
            }
            if (requestInvoice.Sign != null)
            {
                for (int i = 0; i < requestInvoice.Sign.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "img";
                    am.Value = requestInvoice.Sign[i];
                    attchModels.Add(am);
                }
            }
            requestInvoice.Attach = attchModels;

            #endregion

            invoiceattach attchModel = null;
            List<invoiceattach> attchList = new List<invoiceattach>();
            if (requestInvoice.Attach != null)
            {
                string rootpath = AppDomain.CurrentDomain.BaseDirectory;
                string filename = "";
                string path = string.Format("{0}/UploadFile/Sign", rootpath);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < requestInvoice.Attach.Count; i++)
                {
                    if (requestInvoice.Attach[i].Type == "img")
                    {
                        FileStream fs = null;
                        string picName = DateTime.Now.ToString("MMddHHmmssfff") + i + ".png";
                        filename = string.Format("{0}\\{1}", path, picName);
                        fs = File.Create(filename);

                        byte[] bytes = Convert.FromBase64String(requestInvoice.Attach[i].Value.Substring(22));
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();

                        attchModel = new invoiceattach();
                        attchModel.InvNo = invNo;
                        attchModel.ParamType = requestInvoice.Attach[i].Type;
                        attchModel.ParamValue = string.Format("UploadFile/Sign/{0}", picName);
                        attchList.Add(attchModel);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestInvoice.Attach[i].Value))
                        {
                            attchModel = new invoiceattach();
                            attchModel.InvNo = invNo;
                            attchModel.ParamType = requestInvoice.Attach[i].Type;
                            attchModel.ParamValue = requestInvoice.Attach[i].Value;
                            attchList.Add(attchModel);
                        }
                    }
                }
            }

            i_invoice.Insert(inv, false);
            for (int i = 0; i < invoiceList.Count; i++)
            {
                //净物配送单已签收
                invoiceList[i].InvState = 2;
                i_invoice.Update(invoiceList[i], false);
            }

            i_invoicerfid.Insert(rfidList, false);
            i_invoiceattch.Insert(attchList, false);
            foreach (var item in textileList)
            {
                item.TextileState = requestInvoice.InvType;
                item.UpdateTime = time;

                if (requestInvoice.InvType == 1)
                {
                    item.LastReceiveInvID = id.ToString();
                }
                i_textile.Update(item, false);
            }
            i_invoicedetail.Insert(detailList, false);
            if (isRepeat == 1 && repopList.Count > 0)
            {
                i_repeatop.Insert(repopList, false);
            }

            fcModel.ResultMsg = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss");
            fcModel.State = 1;
            i_fc_invoice.Update(fcModel);

            sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
            string code = sc != null ? sc.Code : "A";

            RabbitMQModel mqModel = new RabbitMQModel();
            mqModel.Type = "Invoice";
            mqModel.Value = id.ToString();
            mqModel.Code = code;
            RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

            msgModel.Result = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss");

            return msgModel;
        }

        public MsgModel RepeatOperator([FromBody]RFIDInvoiceParamModel requestInvoice)
        {
            MsgModel msg = new MsgModel();

            List<ResponseRepeatRecModel> list = new List<ResponseRepeatRecModel>();

            //是否开启重复收货
            int isRepeat = 0;
            string repeat = System.Configuration.ConfigurationManager.AppSettings["IsRepeat"];
            int.TryParse(repeat, out isRepeat);

            if (isRepeat == 1)
            {
                var query = from t in i_textile.Entities
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            join r in i_region.Entities on t.LastReceiveRegionID equals r.ID into r_join
                            from r in r_join.DefaultIfEmpty()
                            join cus in i_region.Entities on r.ParentID equals cus.ID into cus_join
                            from cus in cus_join.DefaultIfEmpty()
                            where t.IsFlag == 1 && t.TextileState == 1 && requestInvoice.Tags.Contains(t.TagNo)
                            select new
                            {
                                t.ID,
                                t.TagNo,
                                t.ClassID,
                                c.ClassName,
                                t.SizeID,
                                t.UpdateTime,
                                SizeName = s == null ? "" : s.SizeName,
                                ShortName = cus == null ? "" : cus.RegionName,
                                RegionName = r == null ? "" : r.RegionName,
                            };

                ResponseRepeatRecModel rrrm = null;
                query.ToList().ForEach(c =>
                {
                    rrrm = new ResponseRepeatRecModel();
                    rrrm.ShortName = c.ShortName;
                    rrrm.RegionName = c.RegionName;
                    rrrm.TextileName = c.ClassName;
                    rrrm.SizeName = c.SizeName;
                    rrrm.RepeatInvCreateTime = c.UpdateTime == null ? "" : Convert.ToDateTime(c.UpdateTime).ToString("yyyy-MM-dd HH:mm:ss");
                    list.Add(rrrm);
                });
            }

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        public MsgModel SignTask([FromBody]SendTaskParamModel requestSignTask)
        {
            MsgModel msg = new MsgModel();

            List<ResponseSignTaskModel> list = new List<ResponseSignTaskModel>();

            DateTime begin = DateTime.Now.AddDays(-30);

            if (requestSignTask.RegionMode == 1)
            {
                var query = from id in i_invoicedetail.Entities
                            join i in i_invoice.Entities on id.InvID equals i.ID
                            join tc in i_textileclass.Entities on id.ClassID equals tc.ID
                            join s in i_size.Entities on id.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where id.InvType == 2 && i.InvState == 1 && id.HotelID == requestSignTask.ID && id.InvCreateTime >= begin
                            orderby tc.Sort, tc.ClassName, s.Sort, s.SizeName
                            select new
                            {
                                id.InvID,
                                id.InvNo,
                                id.InvCreateTime,
                                tc.ClassName,
                                SizeName = s == null ? "" : s.SizeName,
                                tc.Sort,
                                id.TextileCount,
                            };

                ResponseSignTaskModel rstm = null;
                ResponseSignTaskItemModel rstim = null;

                query.GroupBy(v => new { v.InvID, v.InvNo, v.InvCreateTime })
                   .Select(x => new { x.Key.InvID, x.Key.InvNo, x.Key.InvCreateTime, Total = x.Sum(c => c.TextileCount) })
                   .ToList().ForEach(q =>
                   {
                       rstm = new ResponseSignTaskModel();
                       rstm.ID = q.InvID;
                       rstm.InvNo = q.InvNo;
                       rstm.CreateTime = q.InvCreateTime;
                       rstm.Total = q.Total;

                       List<string> bags = i_invoiceattch.Entities.Where(c => c.InvNo.Equals(q.InvNo) && c.ParamType.Equals("Bag")).Select(c => c.ParamValue).ToList();
                       rstm.Bags = string.Join("、", bags);

                       rstm.Data = new List<ResponseSignTaskItemModel>();
                       list.Add(rstm);
                   });

                list.ForEach(c =>
                {
                    query.Where(v => v.InvNo.Equals(c.InvNo)).ToList().ForEach(x =>
                    {
                        rstim = new ResponseSignTaskItemModel();
                        rstim.Name = x.ClassName;
                        rstim.SizeName = x.SizeName;
                        rstim.Count = x.TextileCount;

                        c.Data.Add(rstim);
                    });
                });
            }
            else
            {
                var query = from id in i_invoicedetail.Entities
                            join i in i_invoice.Entities on id.InvID equals i.ID
                            join tc in i_textileclass.Entities on id.ClassID equals tc.ID
                            join s in i_size.Entities on id.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where id.InvType == 2 && i.InvState == 1 && id.RegionID == requestSignTask.ID && id.InvCreateTime >= begin
                            orderby tc.Sort, tc.ClassName, s.Sort, s.SizeName
                            select new
                            {
                                id.InvID,
                                id.InvNo,
                                id.InvCreateTime,
                                tc.ClassName,
                                SizeName = s == null ? "" : s.SizeName,
                                tc.Sort,
                                id.TextileCount,
                            };

                ResponseSignTaskModel rstm = null;
                ResponseSignTaskItemModel rstim = null;

                query.GroupBy(v => new { v.InvID, v.InvNo, v.InvCreateTime })
                   .Select(x => new { x.Key.InvID, x.Key.InvNo, x.Key.InvCreateTime, Total = x.Sum(c => c.TextileCount) })
                   .ToList().ForEach(q =>
                   {
                       rstm = new ResponseSignTaskModel();
                       rstm.ID = q.InvID;
                       rstm.InvNo = q.InvNo;
                       rstm.CreateTime = q.InvCreateTime;
                       rstm.Total = q.Total;

                       List<string> bags = i_invoiceattch.Entities.Where(c => c.InvNo.Equals(q.InvNo) && c.ParamType.Equals("Bag")).Select(c => c.ParamValue).ToList();
                       rstm.Bags = string.Join("、", bags);

                       rstm.Data = new List<ResponseSignTaskItemModel>();
                       list.Add(rstm);
                   });

                list.ForEach(c =>
                {
                    query.Where(v => v.InvNo.Equals(c.InvNo)).ToList().ForEach(x =>
                    {
                        rstim = new ResponseSignTaskItemModel();
                        rstim.Name = x.ClassName;
                        rstim.SizeName = x.SizeName;
                        rstim.Count = x.TextileCount;

                        c.Data.Add(rstim);
                    });
                });
            }

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        public MsgModel InsertHandInvoice([FromBody]RFIDInvoiceParamModel requestInvoice)
        {
            MsgModel msgModel = new MsgModel();
            string guid = requestInvoice.GUID.ToUpper();

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

            string px = GetInvoicePX(requestInvoice.InvType);
            int nubmer = GetInvoiceNubmer();
            string invNo = string.Format("{0}{1}{2}", px, DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
            DateTime time = DateTime.Now;

            invoice inv = new invoice();
            Guid id = Guid.NewGuid();
            inv.ID = id.ToString();
            inv.InvNo = invNo;
            inv.InvType = requestInvoice.InvType;
            inv.InvSubType = requestInvoice.InvSubType;
            inv.DataType = 2;
            inv.InvState = 1;
            inv.HotelID = requestInvoice.HotelID;
            inv.RegionID = requestInvoice.RegionID;
            inv.Quantity = requestInvoice.Quantity;
            inv.Remrak = requestInvoice.Remrak;
            inv.TaskType = requestInvoice.TaskType;
            inv.CreateUserID = requestInvoice.CreateUserID;
            inv.CreateUserName = requestInvoice.CreateUserName;
            inv.CreateTime = time;
            inv.MacCode = requestInvoice.UUID;
            inv.Comfirmed = 0;
            inv.Processed = false;
            inv.PaymentStatus = 0;

            if (requestInvoice.ConfirmTime != null && requestInvoice.ConfirmTime != DateTime.MinValue)
                inv.ConfirmTime = requestInvoice.ConfirmTime;
            if (requestInvoice.ConfirmUserID != 0)
                inv.ConfirmUserID = requestInvoice.ConfirmUserID;
            if (!string.IsNullOrEmpty(requestInvoice.ConfirmUserName))
                inv.ConfirmUserName = requestInvoice.ConfirmUserName;

            //查询选择客户的流通
            var brandquery = from c in i_brandtype.Entities
                             join r in i_region.Entities on c.ID equals r.BrandID
                             where r.ID == requestInvoice.HotelID
                             select c;
            brandtype brandtype = new brandtype() { ID = 0, BrandName = "" };
            if (brandquery != null && brandquery.Count() > 0)
            {
                brandtype = brandquery.FirstOrDefault();
            }

            List<invoicedetail> detailList = new List<invoicedetail>();
            invoicedetail detailModel = null;
            for (int i = 0; i < requestInvoice.Details.Length; i++)
            {
                detailModel = new invoicedetail();
                string[] array = requestInvoice.Details[i].Split(';');

                if (array.Length > 3)
                {
                    detailModel.InvID = inv.ID;
                    detailModel.InvNo = invNo;
                    detailModel.InvType = requestInvoice.InvType;
                    detailModel.DataType = 2;
                    detailModel.InvSubType = short.Parse(array[3]);
                    detailModel.HotelID = requestInvoice.HotelID;
                    detailModel.RegionID = requestInvoice.RegionID;
                    detailModel.ClassID = int.Parse(array[0]);
                    detailModel.ClassName = array[1];
                    detailModel.TextileCount = int.Parse(array[2]);
                    detailModel.InvCreateTime = time;

                    detailModel.SizeID = 0;
                    detailModel.SizeName = "";
                    detailModel.BrandName = brandtype.BrandName;
                    detailModel.BrandID = brandtype.ID;
                }
                else if (array.Length == 3)
                {
                    detailModel.InvID = inv.ID;
                    detailModel.InvNo = invNo;
                    detailModel.InvType = requestInvoice.InvType;
                    detailModel.DataType = 2;
                    detailModel.HotelID = requestInvoice.HotelID;
                    detailModel.RegionID = requestInvoice.RegionID;
                    detailModel.ClassID = int.Parse(array[0]);
                    detailModel.ClassName = array[1];
                    detailModel.TextileCount = int.Parse(array[2]);
                    detailModel.InvCreateTime = time;

                    detailModel.SizeID = 0;
                    detailModel.SizeName = "";
                    detailModel.BrandName = brandtype.BrandName;
                    detailModel.BrandID = brandtype.ID;
                }
                detailList.Add(detailModel);
            }

            #region 重新组装AttchModel

            List<AttchModel> attchModels = new List<AttchModel>();
            AttchModel am = null;
            if (requestInvoice.TaskInv != null)
            {
                for (int i = 0; i < requestInvoice.TaskInv.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "TaskID";
                    am.Value = requestInvoice.TaskInv[i];
                    attchModels.Add(am);
                }
            }

            if (requestInvoice.Bag != null)
            {
                for (int i = 0; i < requestInvoice.Bag.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "Bag";
                    am.Value = requestInvoice.Bag[i];
                    attchModels.Add(am);
                }
            }
            if (requestInvoice.Sign != null)
            {
                for (int i = 0; i < requestInvoice.Sign.Count; i++)
                {
                    am = new AttchModel();
                    am.Type = "img";
                    am.Value = requestInvoice.Sign[i];
                    attchModels.Add(am);
                }
            }
            if (!string.IsNullOrEmpty(requestInvoice.ReciveDate))
            {
                am = new AttchModel();
                am.Type = "ReciveDate";
                am.Value = requestInvoice.ReciveDate;
                attchModels.Add(am);
            }
            requestInvoice.Attach = attchModels;

            #endregion

            invoiceattach attchModel = null;
            List<invoiceattach> attchList = new List<invoiceattach>();
            if (requestInvoice.Attach != null)
            {
                string filename = "";
                string path = HttpContext.Current.Server.MapPath(@"/UploadFile/Sign");

                for (int i = 0; i < requestInvoice.Attach.Count; i++)
                {
                    if (requestInvoice.Attach[i].Type == "img")
                    {
                        FileStream fs = null;
                        string picName = DateTime.Now.ToString("MMddHHmmssfff") + i + ".png";
                        filename = string.Format("{0}\\{1}", path, picName);
                        fs = File.Create(filename);
                        byte[] bytes = Convert.FromBase64String(requestInvoice.Attach[i].Value);

                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();

                        attchModel = new invoiceattach();
                        attchModel.InvNo = invNo;
                        attchModel.ParamType = requestInvoice.Attach[i].Type;
                        attchModel.ParamValue = string.Format("UploadFile/Sign/{0}", picName);
                        attchList.Add(attchModel);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestInvoice.Attach[i].Value))
                        {
                            attchModel = new invoiceattach();
                            attchModel.InvNo = invNo;
                            attchModel.ParamType = requestInvoice.Attach[i].Type;
                            attchModel.ParamValue = requestInvoice.Attach[i].Value;
                            attchList.Add(attchModel);
                        }
                    }
                }
            }

            i_invoice.Insert(inv, false);
            i_invoiceattch.Insert(attchList, false);

            i_invoicedetail.Insert(detailList, false);

            fcModel.ResultMsg = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss");
            fcModel.State = 1;
            i_fc_invoice.Update(fcModel);

            sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
            string code = sc != null ? sc.Code : "A";

            RabbitMQModel mqModel = new RabbitMQModel();
            mqModel.Type = "Invoice";
            mqModel.Value = id.ToString();
            mqModel.Code = code;
            RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

            msgModel.Result = invNo + "|" + time.ToString("yyyy/MM/dd HH:mm:ss");

            return msgModel;
        }

        public MsgModel ValidationQRCode([FromBody]QRCodeValidationParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            List<ResponseQRCodeValidationModel> result = new List<ResponseQRCodeValidationModel>();

            //List<string> qrcodes = new List<string>();

            List<qrcode> list = i_qrcode.Entities.Where(v => requestParam.Tags.Contains(v.RFIDTagNo)).ToList();

            string tagno = string.Empty;
            IEnumerable<qrcode> ienumerable = null;
            //qrcode model = null;
            ResponseQRCodeValidationModel qrcodevalidation = null;
            for (int i = 0; i < requestParam.Tags.Length; i++)
            {
                tagno = requestParam.Tags[i];

                ienumerable = list.Where(c => c.RFIDTagNo.Equals(tagno));

                qrcodevalidation = new ResponseQRCodeValidationModel();
                if (ienumerable.Count() > 1)
                {
                    //芯片绑定了多次
                    qrcodevalidation.IsRFIDRepeat = true;
                    qrcodevalidation.Key = tagno;
                    qrcodevalidation.Values = ienumerable.Select(c => c.QRCodeValue).ToList();
                    result.Add(qrcodevalidation);
                }
                else if (ienumerable.Count() == 1)
                {
                    //从芯片来看绑定正常，倒推二维码是否绑定过多次
                    //model = ienumerable.FirstOrDefault();
                    //qrcodes.Add(model.QRCodeValue);
                }
                else
                {
                    //未绑定
                    qrcodevalidation.Key = tagno;
                    result.Add(qrcodevalidation);
                }
            }

            //list = i_qrcode.Entities.Where(v => qrcodes.Contains(v.QRCodeValue)).ToList();

            msg.Result = result;
            if (result.Count > 0)
            {
                msg.ResultCode = 1;
            }
            else
            {
                msg.ResultCode = 0;
            }

            return msg;
        }

        public MsgModel InStoreTask([FromBody]InStoreTaskParamModel requestParam)
        {
            MsgModel msg = new MsgModel();

            DateTime end = requestParam.RecieveDate.AddDays(1);

            List<ResponseInvoiceDetailModel> list = new List<ResponseInvoiceDetailModel>();

            var query = from id in i_invoicedetail.Entities
                        join tc in i_textileclass.Entities on id.ClassID equals tc.ID
                        join s in i_size.Entities on id.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        where id.InvType == 1 && id.HotelID == requestParam.HotelID
                        && id.InvCreateTime >= requestParam.RecieveDate && id.InvCreateTime < end
                        group id by new { tc.ClassName }
                        into m
                        orderby m.Key.ClassName
                        select new
                        {
                            m.Key.ClassName,
                            TextileCount = m.Sum(c => c.TextileCount),
                        };

            ResponseInvoiceDetailModel ridm = null;
            query.ToList().ForEach(c =>
            {
                ridm = new ResponseInvoiceDetailModel();
                ridm.ClassName = c.ClassName;
                ridm.TextileCount = c.TextileCount;
                list.Add(ridm);
            });

            msg.ResultCode = 0;
            msg.Result = list;

            return msg;
        }

        #endregion

        private void FillInvoice(List<textile> list, RFIDInvoiceParamModel requestInvoice)
        {
            sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
            string code = sc != null ? sc.Code : "A";

            string px = GetInvoicePX(1);

            DateTime time = DateTime.Now;

            var querylist = from q in list
                            group q by new
                            {
                                q.HotelID,
                                q.RegionID
                            } into m
                            select new { hotelId = m.Key.HotelID, regionId = m.Key.RegionID, quantity = m.Count() };

            querylist.ToList().ForEach(ql =>
            {
                int nubmer = GetInvoiceNubmer();
                string invNo = string.Format("{0}{1}{2}", px, DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
                invoice inv = new invoice();
                Guid id = Guid.NewGuid();
                inv.ID = id.ToString();
                inv.InvNo = invNo;
                inv.InvType = 1;
                inv.InvSubType = 1;
                inv.DataType = 1;
                inv.InvState = 1;
                inv.HotelID = ql.hotelId.Value;
                inv.RegionID = ql.regionId.Value;
                inv.Quantity = ql.quantity;
                inv.Remrak = "";
                inv.TaskType = requestInvoice.TaskType;
                inv.CreateUserID = requestInvoice.CreateUserID;
                inv.CreateUserName = requestInvoice.CreateUserName;
                inv.CreateTime = time;
                inv.MacCode = requestInvoice.UUID;
                inv.Comfirmed = 0;
                inv.Processed = false;
                inv.PaymentStatus = 0;

                var query = from t in i_textile.Entities
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            where t.IsFlag == 1 && requestInvoice.Tags.Contains(t.TagNo) && (t.TextileState == 2 || t.TextileState == 9) && t.HotelID == ql.hotelId && t.RegionID == ql.regionId
                            select new
                            {
                                t.ID,
                                t.TagNo,
                                t.ClassID,
                                c.ClassName,
                                t.SizeID,
                                SizeName = s == null ? "" : s.SizeName,
                                BrandID = b.ID,
                                b.BrandName,
                                t.RegionID
                            };
                List<invoicerfid> rfidList = new List<invoicerfid>();
                List<textile> textileList = new List<textile>();
                invoicerfid rfidmodel = null;
                List<int> textileId = new List<int>();
                query.ToList().ForEach(q =>
                {
                    rfidmodel = new invoicerfid();
                    rfidmodel.InvID = inv.ID;
                    rfidmodel.InvNo = invNo;
                    rfidmodel.InvType = 1;
                    rfidmodel.InvSubType = 1;
                    rfidmodel.HotelID = ql.hotelId.Value;
                    rfidmodel.RegionID = ql.regionId.Value;
                    rfidmodel.SourceRegionID = q.RegionID.Value;
                    rfidmodel.TextileTagNo = q.TagNo;
                    rfidmodel.TextileID = q.ID;
                    rfidmodel.InvCreateTime = time;
                    rfidmodel.ClassID = q.ClassID;
                    rfidmodel.ClassName = q.ClassName;
                    rfidmodel.SizeID = q.SizeID;
                    rfidmodel.SizeName = q.SizeName;
                    rfidmodel.BrandID = q.BrandID;
                    rfidmodel.BrandName = q.BrandName;
                    rfidmodel.CreateUserID = requestInvoice.CreateUserID;
                    rfidmodel.CreateUserName = requestInvoice.CreateUserName;
                    rfidList.Add(rfidmodel);

                    textileId.Add(q.ID);
                });

                List<invoicedetail> detailList = new List<invoicedetail>();
                invoicedetail detailModel = null;
                query.GroupBy(v => new { v.BrandID, v.BrandName, v.ClassID, v.ClassName, v.SizeID, v.SizeName })
                    .Select(x => new { x.Key.BrandID, x.Key.BrandName, x.Key.ClassID, x.Key.ClassName, x.Key.SizeID, x.Key.SizeName, TextileCount = x.Count() })
                    .ToList().ForEach(q =>
                    {
                        detailModel = new invoicedetail();
                        detailModel.InvID = inv.ID;
                        detailModel.InvNo = invNo;
                        detailModel.InvType = requestInvoice.InvType;
                        detailModel.InvSubType = requestInvoice.InvSubType;
                        detailModel.DataType = 1;
                        detailModel.HotelID = ql.hotelId.Value;
                        detailModel.RegionID = ql.regionId.Value;
                        detailModel.BrandID = q.BrandID;
                        detailModel.BrandName = q.BrandName;
                        detailModel.ClassID = q.ClassID;
                        detailModel.ClassName = q.ClassName;
                        detailModel.SizeID = q.SizeID;
                        detailModel.SizeName = q.SizeName;
                        detailModel.TextileCount = q.TextileCount;
                        detailModel.InvCreateTime = time;
                        detailList.Add(detailModel);
                    });

                i_invoice.Insert(inv, false);
                i_invoicedetail.Insert(detailList, false);
                i_invoicerfid.Insert(rfidList);


                RabbitMQModel mqModel = new RabbitMQModel();
                mqModel.Type = "Invoice";
                mqModel.Value = id.ToString();
                mqModel.Code = code;
                RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

            });
        }

        private string GetFlowName(int type)
        {
            string px = "";
            switch (type)
            {
                case 1:
                    px = "污物送洗";
                    break;
                case 2:
                    px = "净物配送";
                    break;
                case 3:
                    px = "入库";
                    break;
                case 4:
                    px = "出库";
                    break;
                case 5:
                    px = "入厂";
                    break;
                case 7:
                    px = "退回";
                    break;
                case 8:
                    px = "工厂返洗";
                    break;
                case 9:
                    px = "签收";
                    break;
                case 10:
                    px = "盘库";
                    break;
                case 11:
                    px = "打扎";
                    break;
                default:
                    break;
            }
            return px;
        }

        private string GetInvoicePX(int invType)
        {
            string px = string.Empty;
            switch (invType)
            {
                case 1:
                    px = "SH";
                    break;
                case 2:
                    px = "PH";
                    break;
                case 3:
                    px = "RK";
                    break;
                case 4:
                    px = "CK";
                    break;
                case 5:
                    px = "RC";
                    break;
                case 6:
                    px = "DD";
                    break;
                case 7:
                    px = "TH";
                    break;
                case 8:
                    px = "GF";
                    break;
                case 9:
                    px = "QS";
                    break;
                case 10:
                    px = "PK";
                    break;
                default:
                    break;
            }
            return px;
        }

        private int GetInvoiceNubmer()
        {
            lock (invLcker)
            {
                int rtn = 1;
                if (API.Global.InvoiceTime.ToString("yyyy/MM/dd") != DateTime.Now.ToString("yyyy/MM/dd"))
                {
                    API.Global.InvoiceTime = DateTime.Now;
                    API.Global.InvoiceNubmer = rtn;
                }
                else
                {
                    rtn = API.Global.InvoiceNubmer + 1;
                    API.Global.InvoiceNubmer = rtn;
                }
                return rtn;
            }
        }
    }
}
