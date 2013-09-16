using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ControleCaixaWeb.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/

		public ActionResult Index( )
		{

			return View( );
		}

		public ActionResult Sair( )
		{
			FormsAuthentication.SignOut( );

			return RedirectToAction("Index", "Home");
		}

		public ActionResult CadastrarData( )
		{
			if(ModelState.IsValid){

			}
			else
			{
				ModelState.AddModelError("", "O Nome do Usuário Ou Senha Estão Incorretos.");
			}
			return View( );
		}


	}
}
