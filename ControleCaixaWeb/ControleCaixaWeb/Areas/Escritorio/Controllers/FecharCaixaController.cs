using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
	[Authorize(Roles = "Escritorio")]
	[HandleError(View = "Error")]
	public class FecharCaixaController : Controller
	{
		private IContextoDados _contextoFecharCaixa = new ContextoDadosNH( );

		public ActionResult Index( )
		{
			return View( );
		}

		public ActionResult ListaDosFechamentosCaixa( )
		{
			IList<FechamentoCaixa> listaDosFechamentos = null;
			listaDosFechamentos = _contextoFecharCaixa.GetAll<FechamentoCaixa>( ).ToList( );
			return View(listaDosFechamentos);
		}



		public ActionResult Detalhes(int id)
		{
			FechamentoCaixa FecharCaixa = _contextoFecharCaixa.Get<FechamentoCaixa>(id);

			return View(FecharCaixa);
		}

		public ActionResult LancarFechamentoCaixa( )
		{
			string nomeusuario = User.Identity.Name;

			long codigoestabelecimento = (from c in _contextoFecharCaixa.GetAll<CadastrarUsuario>( )
										.Where(x => x.Nome == nomeusuario)
										  select c.EstabelecimentoTrabalho.Codigo).First( );
			ViewBag.Usuario = new SelectList(_contextoFecharCaixa.GetAll<CadastrarUsuario>( ).Where(x => x.EstabelecimentoTrabalho.Codigo == codigoestabelecimento), "Codigo", "Nome");
			ViewBag.Loja = new SelectList(_contextoFecharCaixa.GetAll<Estabelecimento>( ).Where(x => x.Codigo == codigoestabelecimento), "Codigo", "RazaoSocial");

			return View( );
		}



		[HttpPost]
		public ActionResult LancarFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
		{
			ModelState["FaturamentoEstabelecimento.RazaoSocial"].Errors.Clear( );
			ModelState["FaturamentoEstabelecimento.CNPJ"].Errors.Clear( );
			ModelState["FaturamentoEstabelecimento.InscricaoEstadual"].Errors.Clear( );
			ModelState["FaturamentoUsuario.Nome"].Errors.Clear( );
			ModelState["FaturamentoUsuario.Email"].Errors.Clear( );
			ModelState["FaturamentoUsuario.Senha"].Errors.Clear( );
			ModelState["FaturamentoUsuario.ConfirmeSenha"].Errors.Clear( );
			ModelState["FaturamentoUsuario.EnderecoUsuario"].Errors.Clear( );

            if (fechamentoCaixa.DataLancamento != null)
            {
                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(fechamentoCaixa.DataLancamento.ToString("yyyy/MM/dd HH:mm:ss"));
                if (diferenca.TotalDays < 0)
                {
                    ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                    ViewBag.Usuario = new SelectList(_contextoFecharCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome",fechamentoCaixa.FaturamentoUsuario);
                    ViewBag.Loja = new SelectList(_contextoFecharCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial",fechamentoCaixa.FaturamentoEstabelecimento);
                    return View();

                }
                if (diferenca.TotalDays > 31)
                {
                    ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                    ViewBag.Usuario = new SelectList(_contextoFecharCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", fechamentoCaixa.FaturamentoUsuario);
                    ViewBag.Loja = new SelectList(_contextoFecharCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", fechamentoCaixa.FaturamentoEstabelecimento);
                    return View();

                }


            }


			if (ModelState.IsValid)
			{

				fechamentoCaixa.FaturamentoUsuario = _contextoFecharCaixa.Get<CadastrarUsuario>(fechamentoCaixa.FaturamentoUsuario.Codigo);
				fechamentoCaixa.FaturamentoEstabelecimento = _contextoFecharCaixa.Get<Estabelecimento>(fechamentoCaixa.FaturamentoEstabelecimento.Codigo);
                fechamentoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                fechamentoCaixa.DataHoraInsercao = DateTime.Now;
				_contextoFecharCaixa.Add<FechamentoCaixa>(fechamentoCaixa);
				_contextoFecharCaixa.SaveChanges( );
				return RedirectToAction("Sucesso", "Home");
			}
			ViewBag.Usuario = new SelectList(_contextoFecharCaixa.GetAll<CadastrarUsuario>( ), "Codigo", "Nome", fechamentoCaixa.FaturamentoUsuario);
			ViewBag.Estabelecimento = new SelectList(_contextoFecharCaixa.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial", fechamentoCaixa.FaturamentoEstabelecimento);

			return View( );
		}




		public ActionResult AlterarFechamentoCaixa(int id)
		{
			FechamentoCaixa fecharCaixaAlterar = _contextoFecharCaixa.Get<FechamentoCaixa>(id);
			return View(fecharCaixaAlterar);
		}



		[HttpPost]
		public ActionResult AlterarFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
		{
			FechamentoCaixa fecharCaixaAlterada = _contextoFecharCaixa.Get<FechamentoCaixa>(fechamentoCaixa.Codigo);
            fecharCaixaAlterada.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            fecharCaixaAlterada.DataHoraInsercao = DateTime.Now; 
			TryUpdateModel(fecharCaixaAlterada);
			_contextoFecharCaixa.SaveChanges( );
			return RedirectToAction("Sucesso", "Home");
		}



		public ActionResult ExcluirFechamentoCaixa(int id)
		{
			FechamentoCaixa fecharCaixaExcluir = _contextoFecharCaixa.Get<FechamentoCaixa>(id);
			return View(fecharCaixaExcluir);
		}



		[HttpPost]
		public ActionResult ExcluirFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
		{
			FechamentoCaixa fecharCaixaExluida = _contextoFecharCaixa.Get<FechamentoCaixa>(fechamentoCaixa.Codigo);
			_contextoFecharCaixa.Delete<FechamentoCaixa>(fecharCaixaExluida);
			_contextoFecharCaixa.SaveChanges( );

			return RedirectToAction("Sucesso", "Home");
		}

		public ActionResult VerMeuCaixa( )
		{

			return View( );
		}

		[HandleError(View = "Error")]
		public ActionResult VerNestaData(DateTime? id, long id2)
		{
			string NomeUsuario = User.Identity.Name;
			ViewBag.DataCaixa = id.ToString( ).Replace("00:", "").Replace("00", "");
			ViewBag.Caixa = (from c in _contextoFecharCaixa.GetAll<CadastrarUsuario>( )
							 .Where(x => x.Codigo == id2)
							 select c.Nome).First( );

			IList<FechamentoCaixa> listaVerMeuCaixaNumaData = null;

			listaVerMeuCaixaNumaData = _contextoFecharCaixa.GetAll<FechamentoCaixa>( )
                                       .Where(x => x.FaturamentoUsuario.Codigo == id2 && x.DataLancamento.Date == id) 
                                       .OrderByDescending(x => x.DataLancamento).ToList( );


			var valores = (from c in _contextoFecharCaixa.GetAll<OperacaoCaixa>( )
                           .Where(x => x.DataLancamento.Date == id && x.UsuarioQueLancou.Codigo == id2 && x.Valor > 0)
						   select c.Valor).Sum( );

			var caixaFechamento = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>( )
                                   .Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Codigo == id2)
								   select l.CaixaFechamento).Sum( );

			var caixaAbertura = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>( )
                                 .Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Codigo == id2)
								 select l.CaixaAbertura).Sum( );

			var faturamento = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>( )
                              .Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Codigo == id2)
							   select l.Faturamento).Sum( );

			ViewBag.listaParaResultadoFechamento = (((valores + caixaFechamento) - caixaAbertura) - faturamento);

			ViewBag.Vazio =" Sem valores lançados. ";

			return View(listaVerMeuCaixaNumaData);
		}

		public ActionResult FechamentoCaixaData( )
		{
			return View( );
		}

		[HttpPost]
		public ActionResult FechamentoCaixaData(ValidarData Datas)
		{
			if (Datas.DataFinal < Datas.DataInicial)
			{
				ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
			}
			else
			{
				if (ModelState.IsValid)
				{
					string NomeUsuarioLogado = User.Identity.Name;
					long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
					ViewBag.DataInicio = Datas.DataInicial;
					ViewBag.DataFim = Datas.DataFinal;
					ViewBag.Loja = (from c in _contextoFecharCaixa.GetAll<Estabelecimento>( )
									.Where(x => x.Codigo == codigoEstabelecimento)
									select c.RazaoSocial).First( );

					IList<FechamentoCaixa> listaVerMeuCaixa = null;
					listaVerMeuCaixa = _contextoFecharCaixa.GetAll<FechamentoCaixa>( ).Where(x => x.FaturamentoEstabelecimento.Codigo == codigoEstabelecimento && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento <= Datas.DataFinal).OrderByDescending(x => x.DataLancamento).ToList( );


					return View("VerMeuCaixa", listaVerMeuCaixa);
				}
			}

			return View( );
		}

		private long BuscaEstabelecimento(string nomeUsuario)
		{
			long codEstabelecimento = (from c in _contextoFecharCaixa.GetAll<CadastrarUsuario>( )
									   .Where(x => x.Nome == nomeUsuario)
									   select c.EstabelecimentoTrabalho.Codigo).First( );
			return codEstabelecimento;
		}
	}
}
