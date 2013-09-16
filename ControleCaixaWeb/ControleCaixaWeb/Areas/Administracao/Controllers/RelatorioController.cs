
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles="Administrador")]
	[HandleError(View="Error")]
    public class RelatorioController : Controller
    {
        private IContextoDados _contextoRelatorio = new ContextoDadosNH( );

        public ActionResult Index()
        {
            return View();
        }

       

        [GridAction(EnableCustomBinding = true)]
        public ActionResult PagamentosFeitosParaUmFornecedor( )
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoRelatorio.GetAll<CadastrarUsuario>( )
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First( );


            var listadosPagamentos = _contextoRelatorio.GetAll<Pagamento>( ).Where(x => x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento).ToList( );

            return View(new GridModel(listadosPagamentos));
        }

        public ActionResult ListaPagamento( )
        {


            return View(_contextoRelatorio.GetAll<Pagamento>( ).ToList( ));
        }

    }
}
