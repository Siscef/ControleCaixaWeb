using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using System.Web.Security;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{

	[Authorize(Roles = "Escritorio")]
	[HandleError(View = "Error")]
	public class FuncionarioController : Controller
	{
		private IContextoDados _contextoFuncionario = new ContextoDadosNH( );


		public ActionResult Index( )
		{
			return View( );
		}

		public ActionResult ErroFuncionario( )
		{
			return View( );
		}

		public ActionResult ListarFuncionario( )
		{
			string NomeUsuarioLogado = User.Identity.Name;

			long codigoEstabelecimento = (from c in _contextoFuncionario.GetAll<CadastrarUsuario>( )
										  .Where(x => x.Nome == NomeUsuarioLogado)
										  select c.EstabelecimentoTrabalho.Codigo).First( );
			IList<CadastrarUsuario> listaFuncionarios = null;

			listaFuncionarios = _contextoFuncionario.GetAll<CadastrarUsuario>( )
				.Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).ToList( );
			_contextoFuncionario.Dispose( );
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

		public ActionResult CadastrarFuncionario( )
		{
            string NomeFuncionario = User.Identity.Name;
            long CodLocalTrabalho = BuscaEstabelecimento(NomeFuncionario);
			ViewBag.Estabelecimento = new SelectList(_contextoFuncionario.GetAll<Estabelecimento>( ).Where(x => x.Codigo == CodLocalTrabalho), "Codigo", "RazaoSocial");
			ViewBag.Papel = new SelectList(_contextoFuncionario.GetAll<Papel>( ).Where(x => x.Codigo != 1), "Codigo", "NomePapel");
			return View( );
		}

		//
		// POST: /Administracao/Funcionario/Create

		[HttpPost]
		public ActionResult CadastrarFuncionario(CadastrarUsuario funcionario)
		{
			ModelState["EstabelecimentoTrabalho.RazaoSocial"].Errors.Clear( );
			ModelState["EstabelecimentoTrabalho.CNPJ"].Errors.Clear( );
			ModelState["EstabelecimentoTrabalho.InscricaoEstadual"].Errors.Clear( );
			ModelState["NomeFuncao.NomePapel"].Errors.Clear( );

			var FuncionarioExistente = _contextoFuncionario.GetAll<CadastrarUsuario>()
			                           .Where(x => x.Email == funcionario.Email).ToList();
			
			if(FuncionarioExistente.Count() > 0){

				return RedirectToAction("ErroFuncionario");
			 
			}


			if (ModelState.IsValid)
			{
				MembershipCreateStatus status;
				Membership.CreateUser(funcionario.Nome.ToUpper(), funcionario.Senha, funcionario.Email.ToLower(), null, null, true, out status);
				if (status == MembershipCreateStatus.Success)
				{

					funcionario.EstabelecimentoTrabalho = _contextoFuncionario.Get<Estabelecimento>(funcionario.EstabelecimentoTrabalho.Codigo);
					funcionario.NomeFuncao = _contextoFuncionario.Get<Papel>(funcionario.NomeFuncao.Codigo);
                    funcionario.Nome = funcionario.Nome.ToUpper();
					_contextoFuncionario.Add<CadastrarUsuario>(funcionario);
					_contextoFuncionario.SaveChanges( );
                    				
					string funcao = (from c in _contextoFuncionario.GetAll<Papel>( )
									 .Where(x => x.Codigo == funcionario.NomeFuncao.Codigo)
									 select c.NomePapel).First( );


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
								 select u.Senha).First( );

			string NomeUsuarioLogado = User.Identity.Name;

			long codigoEstabelecimento = (from c in _contextoFuncionario.GetAll<CadastrarUsuario>( )
										  .Where(x => x.Nome == NomeUsuarioLogado)
										  select c.EstabelecimentoTrabalho.Codigo).First( );
			long funcaoAtual = (from c in _contextoFuncionario.GetAll<CadastrarUsuario>()
			                    .Where(x => x.Codigo == novousuario.Codigo)
								select c.NomeFuncao.Codigo).First();



			novousuario.Nome = cadastrarUsuario.Nome;
			novousuario.Email = cadastrarUsuario.Email;
			novousuario.EnderecoUsuario = cadastrarUsuario.EnderecoUsuario;
			novousuario.EstabelecimentoTrabalho = _contextoFuncionario.Get<Estabelecimento>(codigoEstabelecimento);
			novousuario.NomeFuncao = _contextoFuncionario.Get<Papel>(funcaoAtual);
			novousuario.Senha = senhaAtual;
			novousuario.ConfirmeSenha = senhaAtual;
			novousuario.Telefone = cadastrarUsuario.Telefone;
			//  TryUpdateModel(novousuario);                
			_contextoFuncionario.SaveChanges( );


			Usuario usuarioAlterado = new Usuario( );
			usuarioAlterado.Nome = cadastrarUsuario.Nome;
			usuarioAlterado.Senha = cadastrarUsuario.Senha;
			usuarioAlterado.Lembrar = false;
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
			Usuario usuarioExcluido = _contextoFuncionario.Get<Usuario>(cadastrarUsuario.Codigo);
			_contextoFuncionario.Delete<Usuario>(usuarioExcluido);
			_contextoFuncionario.SaveChanges( );

			return RedirectToAction("Sucesso", "Home");
		}

        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoFuncionario.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
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
