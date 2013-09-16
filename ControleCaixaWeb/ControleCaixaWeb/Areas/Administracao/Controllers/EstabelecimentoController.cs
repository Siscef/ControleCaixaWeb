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
    public class EstabelecimentoController : Controller
    {
        private IContextoDados _contextoEstabelecimento = new ContextoDadosNH( );
       

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListarEstabelecimento( )
        {
            IList<Estabelecimento> listaEstabelecimento = null;
            listaEstabelecimento = _contextoEstabelecimento.GetAll<Estabelecimento>( )
			.Where(x => x.Codigo != 1).ToList( );
            return View(listaEstabelecimento );
        }
       

        public ActionResult Detalhes(int id)
        {
            Estabelecimento detalhesEstabelecimento = _contextoEstabelecimento.Get<Estabelecimento>(id);          
            return View(detalhesEstabelecimento);
        }

        public ActionResult ListadeContas(int id)
        {
            var listadecontas = _contextoEstabelecimento.GetAll<ContaCorrente>().Where(x => x.EstabelecimentoDaConta.Codigo.Equals(id));
            return View( listadecontas);
        }
       

        public ActionResult CadastrarEstabelecimento()
        {
            ViewBag.UsuarioResponsavel = new SelectList(_contextoEstabelecimento.GetAll<CadastrarUsuario>(),"Codigo","Nome");
                
            return View();
        } 

        

        [HttpPost]
        public ActionResult CadastrarEstabelecimento(Estabelecimento estabelecimento)
        {
            ModelState["UsuarioResponsavel.Nome"].Errors.Clear( );
            ModelState["UsuarioResponsavel.Senha"].Errors.Clear( );
            ModelState["UsuarioResponsavel.Email"].Errors.Clear( );
            ModelState["UsuarioResponsavel.ConfirmeSenha"].Errors.Clear( );
            ModelState["UsuarioResponsavel.EnderecoUsuario"].Errors.Clear( );         
           




            if (ModelState.IsValid)
            {
                estabelecimento.UsuarioResponsavel = _contextoEstabelecimento.Get<CadastrarUsuario>(estabelecimento.UsuarioResponsavel.Codigo);
                _contextoEstabelecimento.Add<Estabelecimento>(estabelecimento);
                _contextoEstabelecimento.SaveChanges( );
               
            }
            return RedirectToAction("Sucesso", "Home", new { area = "Administracao"});
        }
        
        
 
        public ActionResult AlterarEstabelecimento(int id)
        {
            Estabelecimento estabelecimentoParaAlterar = _contextoEstabelecimento.Get<Estabelecimento>(id);
            Endereco enderecoEstabelecimento = _contextoEstabelecimento.Get<Endereco>(estabelecimentoParaAlterar.EnderecoEstabelecimento.Codigo);
            ViewBag.UsuarioResponsavel = new SelectList(_contextoEstabelecimento.GetAll<CadastrarUsuario>( ), "Codigo", "Nome");
           
            return View(estabelecimentoParaAlterar);
        }

      

        [HttpPost]
        public ActionResult AlterarEstabelecimento(Estabelecimento estabelecimento)
        {
            Estabelecimento AlterarEstabelecimento = _contextoEstabelecimento.Get<Estabelecimento>(estabelecimento.Codigo);
            AlterarEstabelecimento.CNPJ = estabelecimento.CNPJ;
            AlterarEstabelecimento.EnderecoEstabelecimento = estabelecimento.EnderecoEstabelecimento;
            AlterarEstabelecimento.InscricaoEstadual = estabelecimento.InscricaoEstadual;
            AlterarEstabelecimento.RazaoSocial = estabelecimento.RazaoSocial;
            AlterarEstabelecimento.Telefone = estabelecimento.Telefone;
            AlterarEstabelecimento.UsuarioResponsavel = _contextoEstabelecimento.Get<CadastrarUsuario>(estabelecimento.UsuarioResponsavel.Codigo);
            _contextoEstabelecimento.SaveChanges( );
            return RedirectToAction("Sucesso","Home" );
        }

        
 
        public ActionResult ExcluirEstabelecimento(int id)
        {
            Estabelecimento estabelecimentoParaExcluir = _contextoEstabelecimento.Get<Estabelecimento>(id);
            return View(estabelecimentoParaExcluir);
        }

        

        [HttpPost]
        public ActionResult ExcluirEstabelecimento(Estabelecimento estabelecimento)
        {
            Estabelecimento estabelecimentoExcluido = _contextoEstabelecimento.Get<Estabelecimento>(estabelecimento.Codigo);
            _contextoEstabelecimento.Delete<Estabelecimento>(estabelecimentoExcluido);
            _contextoEstabelecimento.SaveChanges( );
            _contextoEstabelecimento.Dispose( );
            return RedirectToAction("Sucesso","Home" );
        }
    }
}
