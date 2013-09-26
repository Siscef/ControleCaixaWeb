using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using System.Web.Helpers;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class HomeController : Controller
    {
        //
        // GET: /Administracao/Home/

        public ActionResult Index()
        {
                      
           
            using (IContextoDados ctc = new ContextoDadosNH())
            {
                IList<Configuracao> ListVerifica = ctc.GetAll<Configuracao>()
                                                   .ToList();
                if (ListVerifica.Count() == 0)
                {
                    return RedirectToAction("Create", "Configuracao");

                }
            }

            return View();
        }

        public ActionResult Sucesso()
        {
            return View();
        }

        public ActionResult Sair()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }



    }
}
