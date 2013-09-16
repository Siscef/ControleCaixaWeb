using System.Web.Mvc;

namespace ControleCaixaWeb.Areas.OperadorCaixa
{
    public class OperadorCaixaAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "OperadorCaixa";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "OperadorCaixa_default",
                "OperadorCaixa/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "ControleCaixaWeb.Areas.OperadorCaixa.Controllers" }
            );
        }
    }
}
