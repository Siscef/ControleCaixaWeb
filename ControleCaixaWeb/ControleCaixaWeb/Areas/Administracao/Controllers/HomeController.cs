using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{

	[HandleError(View = "Error")]
	[Authorize(Roles = "Administrador")]
	public class HomeController : Controller
	{
		//
		// GET: /Administracao/Home/

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
