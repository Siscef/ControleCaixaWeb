using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ControleCaixaWeb.Areas.OperadorCaixa.Controllers
{
	[Authorize(Roles = "OperadorCaixa")]
	[HandleError(View = "Error")]
	public class HomeController : Controller
	{


		public ActionResult Index( )
		{
			return View( );
		}

		public ActionResult Sucesso( )
		{
			return View( );
		}

		public ActionResult Sair( )
		{
			FormsAuthentication.SignOut( );

			return RedirectToAction("Index", "Home");
		}

	}
}
