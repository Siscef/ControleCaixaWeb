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
    public class ContaCorrenteController : Controller
    {
        private IContextoDados _contextoContaCorrente = new ContextoDadosNH( );

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListarContaCorrente( )
        {
            IList<ContaCorrente> listarcontaCorrente;
            listarcontaCorrente = _contextoContaCorrente.GetAll<ContaCorrente>( ).ToList( );

            return View(listarcontaCorrente );
        }
      

        public ActionResult Detalhes(int id)
        {
            ContaCorrente contacorrenteDetalhes = _contextoContaCorrente.Get<ContaCorrente>(id);
            return View(contacorrenteDetalhes);
        }


        public ActionResult CadastrarContaCorrente()
        {
            ViewBag.Estabelecimento = new SelectList(_contextoContaCorrente.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial");
            return View();
        } 

       

        [HttpPost]
        public ActionResult CadastrarContaCorrente(ContaCorrente contacorrente)
        {
            ModelState["EstabelecimentoDaConta.RazaoSocial"].Errors.Clear( );
            ModelState["EstabelecimentoDaConta.CNPJ"].Errors.Clear( );
            ModelState["EstabelecimentoDaConta.InscricaoEstadual"].Errors.Clear( );
            if (ModelState.IsValid)
            {
                contacorrente.EstabelecimentoDaConta = _contextoContaCorrente.Get<Estabelecimento>(contacorrente.EstabelecimentoDaConta.Codigo);
                contacorrente.Agencia = contacorrente.Agencia.ToUpper();
                contacorrente.Banco = contacorrente.Banco.ToUpper();
                
                _contextoContaCorrente.Add<ContaCorrente>(contacorrente);
                _contextoContaCorrente.SaveChanges( );
                return RedirectToAction("Sucesso", "Home");
            }
            ViewBag.Estabelecimento = new SelectList(_contextoContaCorrente.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial", contacorrente.EstabelecimentoDaConta);
            return View( );
        }
        
       
 
        public ActionResult AlterarContaCorrente(int id)
        {
            ContaCorrente contacorrente = _contextoContaCorrente.Get<ContaCorrente>(id);
            ViewBag.Estabelecimento = new SelectList(_contextoContaCorrente.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial");
          
            return View(contacorrente);
        }

      

        [HttpPost]
        public ActionResult AlterarContaCorrente(ContaCorrente contacorrente)
        {
            ContaCorrente contaCorrenteAlterada = _contextoContaCorrente.Get<ContaCorrente>(contacorrente.Codigo);
            contaCorrenteAlterada.Agencia = contacorrente.Agencia.ToUpper();
            contaCorrenteAlterada.Banco = contacorrente.Banco.ToUpper();
            contaCorrenteAlterada.EstabelecimentoDaConta = _contextoContaCorrente.Get<Estabelecimento>(contacorrente.EstabelecimentoDaConta.Codigo);
            contaCorrenteAlterada.Numero = contacorrente.Numero;
            
            _contextoContaCorrente.SaveChanges( );
            return RedirectToAction( "Sucesso","Home");
        }
       
       
 
        public ActionResult ExcluirContaCorrente(int id)
        {
            ContaCorrente contaCorrenteParaExcluir = _contextoContaCorrente.Get<ContaCorrente>(id);
            return View(contaCorrenteParaExcluir);
        }

       

        [HttpPost]
        public ActionResult ExcluirContaCorrente(ContaCorrente contaCorrente)
        {
            ContaCorrente contaCorrrenteExcluida = _contextoContaCorrente.Get<ContaCorrente>(contaCorrente.Codigo);
            _contextoContaCorrente.Delete<ContaCorrente>(contaCorrente);
            _contextoContaCorrente.SaveChanges( );

            return RedirectToAction( "Sucesso","Home");
        }
    }
}
