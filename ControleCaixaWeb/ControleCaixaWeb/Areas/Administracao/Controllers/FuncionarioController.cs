using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using System.Web.Security;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{

    [Authorize(Roles = "Administrador")]
	[HandleError(View = "Error")]
    public class FuncionarioController : Controller
    {
        private IContextoDados _contextoFuncionario = new ContextoDadosNH( );
        

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListarFuncionario( )
        {
            IList<CadastrarUsuario> listaFuncionarios = null;

            listaFuncionarios = _contextoFuncionario.GetAll<CadastrarUsuario>( )
			.Where(x => x.Codigo != 1).ToList( );
           
            return View(listaFuncionarios);
        }

        //
        // GET: /Administracao/Funcionario/Detalhes/5

        public ActionResult Detalhes(int id)
        {
            CadastrarUsuario detalhesFuncionarios = _contextoFuncionario.Get<CadastrarUsuario>(id);
            _contextoFuncionario.Dispose( );
            return View(detalhesFuncionarios);
        }

        //
        // GET: /Administracao/Funcionario/Create

        public ActionResult CadastrarFuncionario()
        {
            ViewBag.Estabelecimento = new SelectList(_contextoFuncionario.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial");
            ViewBag.Papel = new SelectList(_contextoFuncionario.GetAll<Papel>( ), "Codigo", "NomePapel");
            return View();
        } 

       

        [HttpPost]
        public ActionResult CadastrarFuncionario(CadastrarUsuario funcionario)
        {
            ModelState["EstabelecimentoTrabalho.RazaoSocial"].Errors.Clear( );
            ModelState["EstabelecimentoTrabalho.CNPJ"].Errors.Clear( );
            ModelState["EstabelecimentoTrabalho.InscricaoEstadual"].Errors.Clear( );
            ModelState["NomeFuncao.NomePapel"].Errors.Clear( );

            if(ModelState.IsValid)
            {
                MembershipUser user = FindUserByEmail(funcionario.Email);
                if (user != null)
                {
                    Membership.DeleteUser(funcionario.Nome);  
                }               

                MembershipCreateStatus status;
                Membership.CreateUser(funcionario.Nome.ToUpper(), funcionario.Senha, funcionario.Email.ToLower(), null, null, true, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    
                    funcionario.EstabelecimentoTrabalho = _contextoFuncionario.Get<Estabelecimento>(funcionario.EstabelecimentoTrabalho.Codigo);
                    funcionario.NomeFuncao = _contextoFuncionario.Get<Papel>(funcionario.NomeFuncao.Codigo);
                    _contextoFuncionario.Add<CadastrarUsuario>(funcionario);
                    _contextoFuncionario.SaveChanges( );                    
                    string funcao = (from c in _contextoFuncionario.GetAll<Papel>( )
                                     .Where(x => x.Codigo == funcionario.NomeFuncao.Codigo)
                                     select c.NomePapel).First();
                   

                    Roles.AddUserToRole(funcionario.Nome, funcao);
                    
                    return RedirectToAction("Sucesso", "Home");
                }
            }
            ViewBag.Estabelecimento = new SelectList(_contextoFuncionario.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial", funcionario.EstabelecimentoTrabalho);
            ViewBag.Papel = new SelectList(_contextoFuncionario.GetAll<Papel>( ), "Codigo", "NomePapel", funcionario.NomeFuncao);
            return View( );
        }
        
        //
        // GET: /Administracao/Funcionario/Edit/5
 
        public ActionResult AlterarFuncionario(int id)
        {
            CadastrarUsuario usuarioParaAlterar = _contextoFuncionario.Get<CadastrarUsuario>(id);
            Endereco enderecoFuncionario = _contextoFuncionario.Get<Endereco>(usuarioParaAlterar.EnderecoUsuario.Codigo);
            ViewBag.Estabelecimento = new SelectList(_contextoFuncionario.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial");
            ViewBag.Papel = new SelectList(_contextoFuncionario.GetAll<Papel>( ), "Codigo", "NomePapel");
            return View(usuarioParaAlterar);
        }

       
     

        [HttpPost]
        public ActionResult AlterarFuncionario(CadastrarUsuario cadastrarUsuario)
        {

                MembershipUser usuarioMembership = FindUserByEmail(cadastrarUsuario.Email);

                Membership.UpdateUser(usuarioMembership);
                CadastrarUsuario novousuario = _contextoFuncionario.Get<CadastrarUsuario>(cadastrarUsuario.Codigo);

                string senhaAtual = (from u in _contextoFuncionario.GetAll<CadastrarUsuario>( )
                                    .Where(x => x.Codigo == cadastrarUsuario.Codigo)
                                     select u.Senha).First();
                                    

                novousuario.Nome = cadastrarUsuario.Nome.ToUpper();
                novousuario.Email = cadastrarUsuario.Email.ToLower();
                novousuario.EnderecoUsuario = cadastrarUsuario.EnderecoUsuario;
                novousuario.EstabelecimentoTrabalho = _contextoFuncionario.Get<Estabelecimento>(cadastrarUsuario.EstabelecimentoTrabalho.Codigo);
                novousuario.NomeFuncao = _contextoFuncionario.Get<Papel>(cadastrarUsuario.NomeFuncao.Codigo);
                novousuario.Senha = senhaAtual;
                novousuario.ConfirmeSenha = senhaAtual;
                novousuario.Telefone = cadastrarUsuario.Telefone;
                       
                _contextoFuncionario.SaveChanges( );            

                return RedirectToAction("Sucesso", "Home");
                
           
            
        }

       
 
        public ActionResult ExcluirFuncionario(int id)
        {
            CadastrarUsuario usuarioParaExcluir = _contextoFuncionario.Get<CadastrarUsuario>(id);
            return View(usuarioParaExcluir);
        }

      

        [HttpPost]
        public ActionResult ExcluirFuncionario(CadastrarUsuario cadastrarUsuario)
        {
            CadastrarUsuario usuarioParaExcluir = _contextoFuncionario.Get<CadastrarUsuario>(cadastrarUsuario.Codigo);
            Membership.DeleteUser(usuarioParaExcluir.Nome);
            _contextoFuncionario.Delete<CadastrarUsuario>(usuarioParaExcluir);
            _contextoFuncionario.SaveChanges( );
                 
            
            return RedirectToAction("Sucesso","Home" );
        }

        MembershipUser FindUserByEmail(string email)
        {
            MembershipUserCollection members = Membership.FindUsersByEmail(email);

            if (members.Count > 0)
            {
                foreach (MembershipUser member in members)
                {
                    return member;
                }
            }

            return null;
        }
    }
}
