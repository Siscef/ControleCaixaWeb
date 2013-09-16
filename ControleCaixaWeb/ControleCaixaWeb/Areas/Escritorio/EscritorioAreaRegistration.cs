using System.Web.Mvc;

namespace ControleCaixaWeb.Areas.Administracao
{
    public class EscritorioAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Escritorio";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Escritorio_default",
                "Escritorio/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new string[] { "ControleCaixaWeb.Areas.Escritorio.Controllers" }
            );
        }
    }
}
