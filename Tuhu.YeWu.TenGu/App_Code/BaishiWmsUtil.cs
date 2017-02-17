
namespace Tuhu.YeWu.TenGu
{
    public class BaishiWmsUtil
    {
        private static readonly string IsOpenPushBaishiService = System.Configuration.ConfigurationManager.AppSettings["IsOpenPushBaishiService"];

        /// <summary>
        /// 验证是否开启百世对接功能
        /// </summary>
        /// <returns></returns>
        /// created by cuibaoxiong 2014.12.30
        public static bool GetIsOpenBaishiWMS()
        {
            return (!string.IsNullOrEmpty(IsOpenPushBaishiService)
                && IsOpenPushBaishiService.Equals("1"));
        }
    }
}