using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles="Administrador")]
	[HandleError(View = "Error")]
    public class FavorecidoController : Controller
    {
      
        private IContextoDados _contextoFavorecido = new ContextoDadosNH( );

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ListaFavorecidos( )
        {
            IList<Favorecido> listaFavorecido = null;
            listaFavorecido = _contextoFavorecido.GetAll<Favorecido>( ).ToList( );
            return View(listaFavorecido);
        }

        public ActionResult Detalhes(int id)
        {
            Favorecido detalhesFavorecido = _contextoFavorecido.Get<Favorecido>(id);

            return View(detalhesFavorecido);
        }

        

        public ActionResult CadastrarFavorecido()
        {
            return View();
        } 

       

        [HttpPost]
        public ActionResult CadastrarFavorecido(Favorecido favorecido)
        {
            if (ModelState.IsValid)
            {

                _contextoFavorecido.Add<Favorecido>(favorecido);
                _contextoFavorecido.SaveChanges( );

                return RedirectToAction("Sucesso", "Home");
            }
            return View( );
        }
        
      
 
        public ActionResult AlterarFavorecido(int id)
        {
            Favorecido alterarFavorecido = _contextoFavorecido.Get<Favorecido>(id);
            return View(alterarFavorecido);
        }

        

        [HttpPost]
        public ActionResult AlterarFavorecido(Favorecido favorecido)
        {
            Favorecido favorecidoAlterar = _contextoFavorecido.Get<Favorecido>(favorecido.Codigo);
            TryUpdateModel(favorecidoAlterar);
            _contextoFavorecido.SaveChanges( );

            return RedirectToAction("Sucesso", "Home");

        }

         
        public ActionResult ExcluirFavorecido(int id)
        {
            Favorecido favorecidoParaExcluir = _contextoFavorecido.Get<Favorecido>(id);
            return View(favorecidoParaExcluir);
        }

       

        [HttpPost]
        public ActionResult ExcluirFavorecido(Favorecido favorecido)
        {
            Favorecido favorecidoExcluido = _contextoFavorecido.Get<Favorecido>(favorecido.Codigo);
            _contextoFavorecido.Delete<Favorecido>(favorecidoExcluido);
            _contextoFavorecido.SaveChanges( );
            return RedirectToAction("Sucesso", "Home");
        }
    }
}
