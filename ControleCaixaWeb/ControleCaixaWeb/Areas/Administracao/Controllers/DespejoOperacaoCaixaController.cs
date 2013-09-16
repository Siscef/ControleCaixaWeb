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
	[Authorize(Roles = "Administrador")]
	[HandleError(View = "Error")]
	public class DespejoOperacaoCaixaController : Controller
	{
		private IContextoDados _contextoDespejoOperacaoCaixa = new ContextoDadosNH( );

		public ActionResult Index( )
		{
			return View( );
		}



		public ActionResult Detalhes(int id)
		{
			DespejoOPeracaoCaixa DetalhesDespejo = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);
			return View(DetalhesDespejo);
		}



		public ActionResult AdicionarDespejoOperacaoCaixa( )
		{
			ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>( ), "Codigo", "RazaoSocial");
			ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>( ), "Codigo", "NomeTipoFormaPagamento");
			ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>( ), "Codigo", "Nome");

			return View( );
		}



		[HttpPost]
		public ActionResult AdicionarDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoOperacaoCaixa)
		{

			//Entrada No Despejo
			//Limpando os erros

			ModelState["FormaPagamentoUtilizada.NomeTipoFormaPagamento"].Errors.Clear( );

			//pegando o ultimo despejo

			string nomeUsuarioLogado = User.Identity.Name;

			OperacaoCaixa operacaoCaixaDespejo = new OperacaoCaixa( );

			operacaoCaixaDespejo.DataLancamento = DateTime.Now;
			operacaoCaixaDespejo.Descricao = "DESPEJO DO CAIXA " + despejoOperacaoCaixa.GenerateId( );

			if (despejoOperacaoCaixa.Valor < 0)
			{

				operacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor;
			}
			else
			{
				operacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor * -1;
			}

			long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>( )
										.Where(x => x.Nome == nomeUsuarioLogado)
										  select c.EstabelecimentoTrabalho.Codigo).First( );

			operacaoCaixaDespejo.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>( )
										.Where(x => x.Nome == nomeUsuarioLogado)
													 select c).First( );

			operacaoCaixaDespejo.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);

			operacaoCaixaDespejo.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);

			if (despejoOperacaoCaixa.Observacao != null)
			{
				operacaoCaixaDespejo.Observacao = despejoOperacaoCaixa.Observacao + "Lançado por:" + nomeUsuarioLogado + "Data: " + DateTime.Now + "PC " + Request.UserHostName.ToString( );
			}
			else
			{
				operacaoCaixaDespejo.Observacao = "Lançado por:" + nomeUsuarioLogado + " Data: " + DateTime.Now + " PC " + Environment.MachineName;

			}
            operacaoCaixaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            operacaoCaixaDespejo.DataHoraInsercao = DateTime.Now;
			_contextoDespejoOperacaoCaixa.Add<OperacaoCaixa>(operacaoCaixaDespejo);
			_contextoDespejoOperacaoCaixa.SaveChanges( );

			var numeroDaOperacao = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>( )
									.Where(x => x.DataLancamento.Date == despejoOperacaoCaixa.DataLancamento.Date && x.Valor == despejoOperacaoCaixa.Valor * -1 && x.UsuarioQueLancou.Codigo == operacaoCaixaDespejo.UsuarioQueLancou.Codigo && x.FormaPagamentoUtilizada.Codigo == operacaoCaixaDespejo.FormaPagamentoUtilizada.Codigo && x.Descricao == operacaoCaixaDespejo.Descricao)
									select c.Codigo).First( );

			despejoOperacaoCaixa.OperacaoCaixaOrigem = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(numeroDaOperacao);
			despejoOperacaoCaixa.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
			despejoOperacaoCaixa.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
			despejoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>( )
										.Where(x => x.Nome == nomeUsuarioLogado)
													 select c).First( );
			despejoOperacaoCaixa.DataLancamento = despejoOperacaoCaixa.DataLancamento.Date;
            despejoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            despejoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
			_contextoDespejoOperacaoCaixa.Add<DespejoOPeracaoCaixa>(despejoOperacaoCaixa);
			_contextoDespejoOperacaoCaixa.SaveChanges( );


			return RedirectToAction("Sucesso", "Home");

		}

		//
		// GET: /Administracao/DespejoOperacaoCaixa/Edit/5

		public ActionResult AlterarDespejoOperacaoCaixa(int id)
		{
			DespejoOPeracaoCaixa DespejoOperacaoCaixa = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);

			ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>( ), "Codigo", "NomeTipoFormaPagamento");
			ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>( ), "Codigo", "Nome");
			return View(DespejoOperacaoCaixa);
		}

		//
		// POST: /Administracao/DespejoOperacaoCaixa/Edit/5

		[HttpPost]
		public ActionResult AlterarDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoOperacaoCaixa)
		{
			DespejoOPeracaoCaixa DespejoAlterado = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(despejoOperacaoCaixa.Codigo);

			DespejoAlterado.DataLancamento = despejoOperacaoCaixa.DataLancamento;
			DespejoAlterado.Descricao = despejoOperacaoCaixa.Descricao;
			DespejoAlterado.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoOperacaoCaixa.EstabelecimentoOperacao.Codigo);
			DespejoAlterado.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
			DespejoAlterado.Observacao = despejoOperacaoCaixa.Observacao;
			DespejoAlterado.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoOperacaoCaixa.UsuarioQueLancou.Codigo);
			DespejoAlterado.Valor = despejoOperacaoCaixa.Valor;
            DespejoAlterado.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            DespejoAlterado.DataHoraInsercao = DateTime.Now;   

			_contextoDespejoOperacaoCaixa.SaveChanges( );

			OperacaoCaixa OperacaoCaixaAlterar = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(despejoOperacaoCaixa.OperacaoCaixaOrigem.Codigo);
			OperacaoCaixaAlterar.DataLancamento = despejoOperacaoCaixa.DataLancamento;
			OperacaoCaixaAlterar.Descricao = despejoOperacaoCaixa.Descricao;
			OperacaoCaixaAlterar.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoOperacaoCaixa.EstabelecimentoOperacao.Codigo);
			OperacaoCaixaAlterar.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoOperacaoCaixa.UsuarioQueLancou.Codigo);
			OperacaoCaixaAlterar.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
			OperacaoCaixaAlterar.Observacao = despejoOperacaoCaixa.Codigo.ToString( );
			OperacaoCaixaAlterar.Valor = despejoOperacaoCaixa.Valor * -1;
            OperacaoCaixaAlterar.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            OperacaoCaixaAlterar.DataHoraInsercao = DateTime.Now;   

			_contextoDespejoOperacaoCaixa.SaveChanges( );


			return RedirectToAction("Sucesso", "Home");
		}

		//
		// GET: /Administracao/DespejoOperacaoCaixa/Delete/5

		public ActionResult ExcluirDespejoOperacaoCaixa(int id)
		{

			DespejoOPeracaoCaixa despejoOperacaoParaExcluir = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);
			return View(despejoOperacaoParaExcluir);
		}

		//
		// POST: /Administracao/DespejoOperacaoCaixa/Delete/5

		[HttpPost]
		public ActionResult ExcluirDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoExcluido)
		{

			OperacaoCaixa OperacaoCaixaAlterar = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(despejoExcluido.OperacaoCaixaOrigem.Codigo);
			OperacaoCaixaAlterar.DataLancamento = despejoExcluido.DataLancamento.Date;
			OperacaoCaixaAlterar.Descricao = "EXCLUSÃO RETORNO";
			OperacaoCaixaAlterar.Valor = despejoExcluido.Valor;
			OperacaoCaixaAlterar.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoExcluido.EstabelecimentoOperacao.Codigo);
			OperacaoCaixaAlterar.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoExcluido.UsuarioQueLancou.Codigo);
			OperacaoCaixaAlterar.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoExcluido.FormaPagamentoUtilizada.Codigo);
			OperacaoCaixaAlterar.Observacao = despejoExcluido.Codigo.ToString( );

			_contextoDespejoOperacaoCaixa.SaveChanges( );

			DespejoOPeracaoCaixa DespejoParaExcluir = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(despejoExcluido.Codigo);
			_contextoDespejoOperacaoCaixa.Delete<DespejoOPeracaoCaixa>(DespejoParaExcluir);
			_contextoDespejoOperacaoCaixa.SaveChanges( );

			return RedirectToAction("Sucesso", "Home");
		}

		public ActionResult ListaDespejo( )
		{
			IList<DespejoOPeracaoCaixa> listaDespejo = null;
			listaDespejo = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>( ).OrderByDescending(x => x.DataLancamento).ToList( );

			return View(listaDespejo);
		}


		public ActionResult DespejoPorForma(int id, DateTime? data)
		{
			IList<DespejoOPeracaoCaixa> despejoDaForma = null;
			despejoDaForma = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>( ).Where(x => x.FormaPagamentoUtilizada.Codigo == id && x.DataLancamento.Date == data).OrderByDescending(x => x.DataLancamento)
							.ToList( );
			return View(despejoDaForma);
		}
		public ActionResult DespejoPorUsuário(int id, DateTime? data)
		{
			IList<DespejoOPeracaoCaixa> despejoUsuario = null;
			despejoUsuario = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>( ).Where(x => x.UsuarioQueLancou.Codigo == id && x.DataLancamento.Date == data).OrderByDescending(x => x.DataLancamento)
							.ToList( );
			return View(despejoUsuario);
		}

		public ActionResult DespejoEstabelecimento(int id, DateTime? data)
		{
			IList<DespejoOPeracaoCaixa> despejoEstabelecimento = null;
			despejoEstabelecimento = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>( ).Where(x => x.EstabelecimentoOperacao.Codigo == id && x.DataLancamento.Date == data).OrderByDescending(x => x.DataLancamento)
							.ToList( );
			return View(despejoEstabelecimento);
		}


	}
}

