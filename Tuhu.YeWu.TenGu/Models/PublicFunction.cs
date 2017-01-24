using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using ThBiz.Business;
using ThBiz.Business.Monitor;
using ThBiz.Business.OprLogManagement;
using ThBiz.Business.OrderManagement;
using ThBiz.Business.Purchase;
using ThBiz.Common.Common;
using ThBiz.DataAccess.Entity;
using TheBiz.Common.Common;
using Tuhu.Component.Common;
using Tuhu.Component.Framework;
using Tuhu.Component.Framework.Identity;
using WMS.Stock.Business.INVManagement;

namespace Tuhu.YeWu.TenGu.Models
{
    public class PublicFunction
    {
        private static string strConn = ConfigurationManager.ConnectionStrings["Gungnir"].ConnectionString;
        private static SqlConnection connection = new SqlConnection(strConn.Decrypt());

        private static readonly ILog Logger = LoggerFactory.GetLogger("PublicFunction");

        /// <summary>
        /// 系统未分配订单或已分配订单数量统计
        /// </summary>
        /// <param name="type">“NotAssigned”:未分配；Assigned：已分配</param>
        /// <returns></returns>
        public static int OrderNum(string type)
        {
            var gdc = new GungnirDataContext();
            var th = new TuhuBiz();
            if (type == "NotAssigned")
            {
                int? notAssignedOrderNum = 0;
                gdc.Order_Statistics_AssignOrdersNum("NotAssigned", ref notAssignedOrderNum);
                return notAssignedOrderNum.Value;
            }
            if (type == "All")
            {
                int? AllAssignedOrderNum = 0;
                gdc.Order_Statistics_AssignOrdersNum("All", ref AllAssignedOrderNum);
                return AllAssignedOrderNum.Value;
            }
            int? assignedOrderNum = 0;
            gdc.Order_Statistics_AssignOrdersNum("Assigned", ref assignedOrderNum);
            return assignedOrderNum.Value;
        }

        /// <summary>
        /// 强制分配订单时，更新订单分配任务
        /// </summary>
        /// <param name="th"></param>
        /// <param name="PKID">订单的PKID</param>
        public static void UpdateOrderAssignTask(TuhuBiz th, int PKID, string emailAddr)
        {
            var emp = TransactionScopeHelper.CreateNoLock(() => th.HrEmployee.FirstOrDefault(o => o.EmailAddress.Equals(emailAddr)));
            var task = TransactionScopeHelper.CreateNoLock(() => th.Tasks.FirstOrDefault(t => t.RelatedObjID == PKID && t.CreatedByName.Equals("System") && t.TaskStatus.Equals("10新建") && t.TaskType.Equals("分配订单")));
            if (task != null)
            {
                task.TaskStatus = "30完成";
                task.UpdatedByName = emailAddr;
                task.UpdatedTime = DateTime.Now;
                task.FinishedBy = emailAddr;
                task.FinishedTime = DateTime.Now;
                task.TaskDescription = "该订单被强制分配！";
                th.SaveChanges();
            }
            //重新添加该订单分配任务
            var newTask = new Tasks
            {
                Subject = "强制分配订单",
                TaskStatus = "10新建",
                Priority = "2中",
                RelatedObject = "销",
                RelatedObjID = PKID,
                CreatedTime = DateTime.Now,
                AssignedTo = emp.PKID,
                AssignedToName = emp.EmployeeName,
                AssignedTime = DateTime.Now,
                TaskType = "分配订单",
                CreatedByName = "System"
            };
            th.Tasks.AddObject(newTask);
            th.SaveChanges();
        }
        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public static string GetUserId(string mobile)
        {
            string guid = "";
            string _md5 = md5(mobile);
            guid = new Guid(_md5).ToString("B");
            return guid;
        }
        /// <summary>
        /// Hash加密
        /// </summary>
        /// <param name="str">手机号</param>
        /// <returns></returns>
        public static string md5(string str)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str + (new DateTime()).ToString(), "MD5");
        }

        /// <summary>
        /// 系统未分配到店跟踪或已分配到店跟踪数量统计
        /// </summary>
        /// <param name="type">“NotAssigned”:未分配；Assigned：已分配</param>
        /// <returns></returns>
        public static int TrackReceivedNum(string type)
        {
            var gdc = new GungnirDataContext();
            if (type == "NotAssigned")
            {
                int? notAssignedTrackNum = 0;
                gdc.Order_Statistics_AssignTrackReceivedNum("NotAssigned", ref notAssignedTrackNum);
                return notAssignedTrackNum.Value;
            }
            if (type == "All")
            {
                int? AllAssignedTrackNum = 0;
                gdc.Order_Statistics_AssignTrackReceivedNum("All", ref AllAssignedTrackNum);
                return AllAssignedTrackNum.Value;
            }
            int? assignedTrackNum = 0;
            gdc.Order_Statistics_AssignTrackReceivedNum("Assigned", ref assignedTrackNum);
            return assignedTrackNum.Value;
        }

        /// <summary>
        /// 系统未分配逾期未安装或已分配逾期未安装数量统计
        /// </summary>
        /// <param name="type">“NotAssigned”:未分配；Assigned：已分配</param>
        /// <returns></returns>
        public static int OverdueOrdersNum(string type, int day)
        {
            var gdc = new GungnirDataContext();
            var nowtime = DateTime.Today.AddHours(Math.Min(DateTime.Now.Hour, 12));
            var assignOverdue = gdc.Order_Overdue_SelectNotInstallOrders(nowtime.AddDays(0 - day)).Select(o => new { o.PKID, o.AssignedTo }).Distinct();
            if (type == "NotAssigned")
            {
                var notAssignedOverdueNum = 0;
                notAssignedOverdueNum = assignOverdue.Count(o => o.AssignedTo == null);
                return notAssignedOverdueNum;
            }
            else
            {
                var allAssignedOverdueNum = 0;
                allAssignedOverdueNum = assignOverdue.Count(o => o.AssignedTo != null);
                return allAssignedOverdueNum;
            }
        }

        public static void UpdateTaskStatus(int orderId, string taskType, string employeeEmail)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@OrderID", orderId),
                new SqlParameter("@TaskType", taskType), 
                new SqlParameter("@EmployeeEmail", employeeEmail)
            };
            DbHelper.ExecuteNonQuery("Task_UpdateTaskStatusByRelatedObjIDAndTaskType", sqlParams);
        }


        public static PurchaseOccupyModel CreatePurchase(int id, out int booked)
        {
            try
            {
                var pm = new PurchaseManager();
                var orderManager = new OrderManager();
                var nvManager = new INVManager();

                var orderList = orderManager.SelectOrderListByOrderListId(id);
                var od = orderManager.SelectOrderByOrderId(orderList.OrderId);

                booked = orderList.Num- pm.GetOccupationQuantity(id); //购买产品数量 - 占用数量总和
                var result = new PurchaseOccupyModel
                {
                    OrderId = orderList.OrderId,
                    SoNo = orderList.OrderNo,
                    OrderStatus = od.Status,
                    OrderPurchaseStatus=od.PurchaseStatus,
                    OrderlistId = orderList.PKID,
                    OrderChannel = od.OrderChannel,
                    InstallShopId = od.InstallShopID,
                    Pid = orderList.PID,
                    ProductName = orderList.Name,
                    SoItemPurchaseStatus = orderList.PurchaseStatus,
                    SoItemNum = orderList.Num,
                    DeliveryStatus = od.DeliveryStatus,
                    WareHouseName = od.WareHouseName,
                    WareHouseId = od.WareHouseId,
                    WeekYear = orderList.WeekYear
                };

                var bookedStockList = new List<PurchaseSoStock>();
                if (od.DeliveryDate != null)
                {
                    #region 若订单已发货，则只显示物流的信息
                    var logisticTaskStockForOrder =
                        nvManager.SelectLogisticTaskInfoForOrderList(od.PKID, orderList.PID)
                            .ConvertTo<PurchaseSoStock>()
                            .ToList();
                    if (logisticTaskStockForOrder.Count > 0)
                    {
                        bookedStockList.AddRange(logisticTaskStockForOrder);
                    }
                    #endregion
                }
                else
                {
                    #region 取占用库存
                    bookedStockList = pm.GetBookedStockListByOrderListId(orderList.PKID).ConvertTo<PurchaseSoStock>().ToList();
                    var batchs = nvManager.SelectAvailableBatches(od.WareHouseId, orderList.PID);
                    if (batchs != null && batchs.Count > 0)
                    {
                        foreach (var bookedStock in bookedStockList)
                        {
                            var b = batchs.SingleOrDefault(a => a.PKID == bookedStock.BatchId && a.OwnerId == bookedStock.OwnerId);
                            if (b != null)
                            {
                                bookedStock.PoItemId = b.POId;
                                bookedStock.VendorName = b.VendorName;
                                bookedStock.Pid = b.PID;
                                bookedStock.Name = b.Name;
                                bookedStock.CostPrice = b.CostPrice;
                                bookedStock.Number = b.Number;
                                bookedStock.WeekYear = b.WeekYear;
                            }
                        }
                    }
                    #endregion
                }
                
                var shops = new ShopsAlwaysOnReadManager().SelectWareHouses();
                result.BookedStockList = bookedStockList;
                result.BookedPoItemList = (orderList.PID != "LF-QIMENZUI-LVHEJIN|"&&!orderList.PID.StartsWith("FU-"))
                    ? pm.GetBookedPoItemListByOrderListId(orderList.PKID).ConvertTo<PurchaseSoPo>()
                    : null;
                var wareHouse = shops.FirstOrDefault(_ => _.Pkid == od.WareHouseId);
                if (wareHouse != null)
                {
                    result.WareHouseColor = wareHouse.Description;
                }
                
                if (!string.IsNullOrWhiteSpace(od.WareHouseName))
                {
                    if (shops.Any())
                    {
                        result.WareHouseList = shops.Where(_ => _.Pkid != od.WareHouseId).ToList();
                        //bool enableSwitchWareHouseConfig =ConfigurationManager.AppSettings["SwitchWareHouseConfig:Enable"] == "true";
                        //if (enableSwitchWareHouseConfig)
                        //{
                        //    //获取快递配送的订单下可随意改向的仓库列表
                        //    var noLimitedWareHouseLists = new OrderAlwaysOnReadManager().GetOrderNoLimitedSwitchWareHouses(od);
                        //    if (noLimitedWareHouseLists != null && noLimitedWareHouseLists.Any())
                        //    {
                        //        var noLimitedShops = (from shop in shops
                        //                              where noLimitedWareHouseLists.Contains(shop.Pkid)
                        //                              select shop).ToList();
                        //        result.NoLimitedWareHouseList = noLimitedShops;
                        //    }
                        //}
                    }
                }
                else
                {
                    result.WareHouseList = null;
                    result.NoLimitedWareHouseList = null;
                }

                return result;
            }
            catch (Exception innerExce)
            {
                ExceptionMonitor.AddNewMonitor("Purchase","", innerExce.ToString(),ThreadIdentity.Operator.Name, "采购", MonitorLevel.Critial,MonitorModule.Purchase);
                throw new Exception("An error occured in PublicFunction.CreatePurchase!");
            }
        }

        public static decimal ComputeDeliveryFee(decimal firstWeightFee, decimal firstWeightKg, decimal continueWeight, int productQuantity)
        {
            return firstWeightFee * firstWeightKg + ((productQuantity * 9) - firstWeightKg) * continueWeight;
        }

        public static List<Shops> GetShopMessagesByShopType(int type)
        {
            using (var gdc = new GungnirDataContext())
            {
                return gdc.Shop_GetMessages_GetShopMessagesByShopType(type)
                                .Where(s => s.SimpleName.EndsWith("仓库"))
                                .Select(s => new Shops() { PKID = s.PKID, SimpleName = s.SimpleName })
                                .ToList();
            }
        }
    }
}