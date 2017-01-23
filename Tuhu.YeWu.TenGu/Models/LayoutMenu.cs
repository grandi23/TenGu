using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tuhu.YeWu.TenGu.Models
{
    public class LayoutMenu
    {
        public LayoutMenu()
        {
            MenuList = new List<MenuNode>()
            {
                new MenuNode(){ Icon = "icon icon-attachment", DisplayName = "销售订单采购",Url = "/InfoList/PurchaseOrderIndex", ParentName = "采购下单", ParentIcon = "icon icon-direction"},
                new MenuNode(){ Icon = "icon icon-calendar", DisplayName = "采购订货列表",Url = "/Home/Show", ParentName = "采购下单", ParentIcon = "icon icon-direction"},
                new MenuNode(){ Icon = "icon icon-move", DisplayName = "采购任务化",Url = "/InfoList/PurchaseOrderIndex", ParentName = "采购下单", ParentIcon = "icon icon-direction"},

                new MenuNode(){ Icon = "icon-document-edit", DisplayName = "采购订单列表",Url = "#", ParentName = "采购信息查询", ParentIcon = "icon icon-align-left"},
                new MenuNode(){ Icon = "icon icon-graph-pie", DisplayName = "采购改仓库列表",Url = "#", ParentName = "采购信息查询", ParentIcon = "icon icon-align-left"},
                new MenuNode(){ Icon = "icon icon-help", DisplayName = "入库批次",Url = "#", ParentName = "采购信息查询", ParentIcon = "icon icon-align-left"},
                new MenuNode(){ Icon = "icon icon-information", DisplayName = "供应商管理",Url = "#", ParentName = "采购信息查询", ParentIcon = "icon icon-align-left"},

                new MenuNode(){ Icon = "icon icon-mail", DisplayName = "采购单零采审核",Url = "#", ParentName = "采购审核", ParentIcon = "icon icon-lock"},
                new MenuNode(){ Icon = "icon icon-map", DisplayName = "采购单批采审核",Url = "#", ParentName = "采购审核", ParentIcon = "icon icon-lock"},
                new MenuNode(){ Icon = "icon icon-media-loop", DisplayName = "采购调价审核",Url = "#", ParentName = "采购审核", ParentIcon = "icon icon-lock"},
                new MenuNode(){ Icon = "icon icon-mobile-portrait", DisplayName = "供应商资质审核",Url = "#", ParentName = "采购审核", ParentIcon = "icon icon-lock"},

                new MenuNode(){ Icon = "icon icon-plus", DisplayName = "采购任务分配配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},
                new MenuNode(){ Icon = "icon icon-phone", DisplayName = "订单更改仓库配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},
                new MenuNode(){ Icon = "icon icon-preview", DisplayName = "采购信息配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},
                new MenuNode(){ Icon = "icon icon-reply", DisplayName = "订单可用周期配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},
                new MenuNode(){ Icon = "icon icon-retweet", DisplayName = "邀约产品配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},
                new MenuNode(){ Icon = "icon icon-search", DisplayName = "保养包装数配置",Url = "#", ParentName = "系统配置", ParentIcon = "icon icon-media-shuffle"},

                new MenuNode(){ Icon = "icon icon-checkmark", DisplayName = "采购红冲监控",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"},
                new MenuNode(){ Icon = "icon icon-upload", DisplayName = "采购取货跟踪",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"},
                new MenuNode(){ Icon = "icon icon-user-group", DisplayName = "采购到货跟踪",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"},
                new MenuNode(){ Icon = "icon icon-print", DisplayName = "采购用款审核",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"},
                new MenuNode(){ Icon = "icon icon-tag", DisplayName = "邀约产品配置",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"},
                new MenuNode(){ Icon = "icon icon-window", DisplayName = "采购任务跟踪",Url = "#", ParentName = "流程监控", ParentIcon = "icon icon-star"}
            };
        }
        public List<MenuNode> MenuList { get; set; }
    }
    public class MenuNode
    {
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 展示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 链接路径
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 父列表名称
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// 父列表图标
        /// </summary>
        public string ParentIcon { get; set; }
    }
}