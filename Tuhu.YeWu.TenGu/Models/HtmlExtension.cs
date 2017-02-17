using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ThBiz.DataAccess.Entity;
using Tuhu.Component.Common.Models;

namespace Tuhu.YeWu.TenGu.Models
{
	public static class HtmlExtension
	{

		#region Pager
		/// <summary>生成分页Html</summary>
		/// <param name="helper">对象</param>
		/// <param name="pager">分页数据</param>
		/// <param name="func">Url生成函数</param>
		/// <returns>分页Html</returns>
		public static MvcHtmlString Pager(this HtmlHelper helper, PagerModel pager, Func<int, string> func)
		{
			var sb = new StringBuilder();
			sb.Append("<div class=\"pager\">");
			if (pager.CurrentPage < 2)
				sb.Append("<a class=\"disabled first-child\">&lt;&lt; 上一页</a>");
			else
				sb.Append("<a class=\"first-child\" href=\"" + func(pager.CurrentPage - 1) + "\">&lt;&lt; 上一页</a>");
			if (pager.TotalPage < 12)
			{
				for (var index = 1; index <= pager.TotalPage; index++)
				{
					if (index == pager.CurrentPage)
						sb.Append("<a class=\"current\">" + index + "</a>");
					else
						sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(index), index);
				}
			}
			else
			{
				if (pager.CurrentPage < 8)
				{
					for (var index = 1; index <= 8; index++)
					{
						if (index == pager.CurrentPage)
							sb.Append("<a class=\"current\">" + index + "</a>");
						else
							sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(index), index);
					}
					sb.Append("<span>...</span>");
					sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(pager.TotalPage - 1), pager.TotalPage - 1);
					sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(pager.TotalPage), pager.TotalPage);
				}
				else if (pager.CurrentPage > pager.TotalPage - 7)
				{
					sb.Append("<a href=\"" + func(1) + "\">1</a>");
					sb.Append("<a href=\"" + func(2) + "\">2</a>");
					sb.Append("<span>...</span>");
					for (var index = pager.TotalPage - 7; index <= pager.TotalPage; index++)
					{
						if (index == pager.CurrentPage)
							sb.Append("<a class=\"current\">" + index + "</a>");
						else
							sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(index), index);
					}
				}
				else
				{
					sb.Append("<a href=\"" + func(1) + "\">1</a>");
					sb.Append("<a href=\"" + func(2) + "\">2</a>");
					sb.Append("<span>...</span>");
					for (var index = pager.CurrentPage - 2; index <= pager.CurrentPage + 2; index++)
					{
						if (index == pager.CurrentPage)
							sb.Append("<a class=\"current\">" + index + "</a>");
						else
							sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(index), index);
					}
					sb.Append("<span>...</span>");
					sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(pager.TotalPage - 1), pager.TotalPage - 1);
					sb.AppendFormat("<a href=\"{0}\">{1}</a>", func(pager.TotalPage), pager.TotalPage);
				}
			}
			if (pager.CurrentPage >= pager.TotalPage)
				sb.Append("<a class=\"disabled last-child\">下一页 &gt;&gt;</a>");
			else
				sb.Append("<a class=\"last-child\" href=\"" + func(pager.CurrentPage + 1) + "\">下一页 &gt;&gt;</a>");
			sb.Append("</div>");

			return MvcHtmlString.Create(sb.ToString());
		}
		/// <summary>生成分页Html</summary>
		/// <param name="helper">对象</param>
		/// <param name="pager">分页数据</param>
		/// <param name="hideIfLessTowPage">小于两页是否不生成</param>
		/// <param name="func">Url生成函数</param>
		/// <returns>分页Html</returns>
		public static MvcHtmlString Pager(this HtmlHelper helper, PagerModel pager, bool hideIfLessTowPage, Func<int, string> func)
		{
			if (hideIfLessTowPage == true && pager.TotalPage < 2)
				return null;
			return Pager(helper, pager, func);
		}
		#endregion

		public static MvcHtmlString LineToBr(this HtmlHelper helper, object text)
		{
			if (text == null)
				return null;

			return MvcHtmlString.Create(Regex.Replace(helper.Encode(text), "\r?\n", "<br />"));
		}

        public static MvcHtmlString VenderAccountList(this HtmlHelper helper, List<VenderAccount> accounts)
        {
            var sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append(
                "function AddAccount(){if (CheckAccount() == 0) {return false;}; $(\".accountList>table #list\").append(\"" +
                AddNewAccount(string.Empty, string.Empty, string.Empty) + "\"); return true;}");
            sb.Append("function RemoveAccount(target){ $(target).closest(\"tr\").remove();}");
            sb.Append(
               "function CheckAccount() { var isValid = 1; $('.tr_accountInfo').each(function (i) {var columns = $(this).children();if (columns.eq(0).find('input[name=Account]').val().trim() == '') {alert('必须填银行账号！');columns.eq(0).find('input[name=Account]').focus();isValid = 0;return false;}else if (columns.eq(1).find('input[name=Bank]').val().trim() == '') {alert('必须填开户银行！');columns.eq(1).find('input[name=Bank]').focus();isValid = 0;return false;}else if (columns.eq(2).find('input[name=Payee]').val().trim() == '') {alert('必须填收款单位！');columns.eq(2).find('input[name=Payee]').focus();isValid = 0;return false;} isValid = 1;}); return isValid;}");
            sb.Append("</script>");
            sb.Append("<div class=\"accountList\">");
            sb.Append("<input type=\"button\" id=\"addAccount\" value=\"添加银行账户\" onclick=\"AddAccount()\">");
            sb.Append(
                "<table><thead><tr><th style='width: 30%'>银行账号</th><th style='width: 32%'>开户银行</th><th style='width: 32%'>收款单位</th><th>操作</th></tr></thead><tbody id='list'>");
            if (accounts != null && accounts.Count > 0)
            {
                foreach (var account in accounts)
                {
                    sb.Append(AddNewAccount(account.Account, account.Bank, account.Payee));
                }
            }
            else
            {
                sb.Append(AddNewAccount(string.Empty, string.Empty, string.Empty));
            }
            sb.Append("</tbody></table></div>");
            return MvcHtmlString.Create(sb.ToString());
        }

        private static string AddNewAccount(string account, string bank, string payee)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr class='tr_accountInfo'>");
            sb.Append(
                "<td><input style='width: 80%;' name='Account' type='text' value='" + account + "'><label style='color: Red;'>*</label></td>");
            sb.Append(
                "<td><input style='width: 95%;' name='Bank' type='text' value='" + bank + "'><label style='color: Red;'>*</label></td>");
            sb.Append(
                "<td><input style='width: 95%;' name='Payee' type='text' value='" + payee + "'><label style='color: Red;'>*</label></td>");
            sb.Append("<td><input type='button' value='删除' onclick='RemoveAccount(this)'></td>");
            sb.Append("</tr>");

            return sb.ToString();
        }
	}
}
