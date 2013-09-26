using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class ConfiguracaoController : Controller
    {
        private IContextoDados _contextoConfiguracao = new ContextoDadosNH();


        public ActionResult Create()
        {
            ViewBag.Loja = new SelectList(_contextoConfiguracao.GetAll<Estabelecimento>().AsParallel().OrderBy(x => x.RazaoSocial), "Codigo", "RazaoSocial");
            IList<Configuracao> ListVerifica = _contextoConfiguracao.GetAll<Configuracao>()
                                                   .ToList();
            if (ListVerifica.Count() > 0)
            {
                foreach (var item in ListVerifica)
                {
                    return View(item);

                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(Configuracao configuracao)
        {
            ModelState["EstabelecimentoPadrao.RazaoSocial"].Errors.Clear();
            ModelState["EstabelecimentoPadrao.InscricaoEstadual"].Errors.Clear();
            ModelState["EstabelecimentoPadrao.CNPJ"].Errors.Clear();
            if (ModelState.IsValid)
            {
                IList<Configuracao> ListVerifica = _contextoConfiguracao.GetAll<Configuracao>()
                                                   .ToList();
                if (ListVerifica.Count() == 0)
                {
                    Configuracao configuracaoInicial = new Configuracao();

                    configuracaoInicial.CasasDecimais = configuracao.CasasDecimais;
                    configuracaoInicial.EnviarEmailCaixaAlterado = configuracao.EnviarEmailCaixaAlterado;
                    configuracaoInicial.FazerLancamentoContaCorrente = configuracao.FazerLancamentoContaCorrente;
                    configuracaoInicial.EstabelecimentoPadrao = _contextoConfiguracao.Get<Estabelecimento>(configuracao.EstabelecimentoPadrao.Codigo);
                    _contextoConfiguracao.Add<Configuracao>(configuracaoInicial);
                    _contextoConfiguracao.SaveChanges();
                    return RedirectToAction("Sucesso", "Home");

                }
                else
                {
                    foreach (var item in ListVerifica)
                    {
                        Edit(item);
                    }
                    
                }


            }

            ViewBag.Loja = new SelectList(_contextoConfiguracao.GetAll<Estabelecimento>().AsParallel().OrderBy(x => x.RazaoSocial), "Codigo", "RazaoSocial");
            return View();
        }


        public ActionResult Edit(int id)
        {
            Configuracao configuracaoParaEdicao = _contextoConfiguracao.Get<Configuracao>(id);
            ViewBag.Loja = new SelectList(_contextoConfiguracao.GetAll<Estabelecimento>().AsParallel().OrderBy(x => x.RazaoSocial), "Codigo", "RazaoSocial");
            return View(configuracaoParaEdicao);
        }

        [HttpPost]
        public ActionResult Edit(Configuracao configuracao)
        {
            Configuracao configuracaoEditada = _contextoConfiguracao.Get<Configuracao>(configuracao.Codigo);

            configuracaoEditada.CasasDecimais = configuracao.CasasDecimais;
            configuracaoEditada.EnviarEmailCaixaAlterado = configuracao.EnviarEmailCaixaAlterado;
            configuracaoEditada.FazerLancamentoContaCorrente = configuracao.FazerLancamentoContaCorrente;
            configuracaoEditada.EstabelecimentoPadrao = _contextoConfiguracao.Get<Estabelecimento>(configuracao.EstabelecimentoPadrao.Codigo);
            TryUpdateModel<Configuracao>(configuracaoEditada);
            _contextoConfiguracao.SaveChanges();
            return RedirectToAction("Sucesso", "Home");
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult TrocarSenha()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult TrocarSenha(TrocarSenha model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.SenhaAtual, model.NovaSenha);



                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("TrocaSenhaSucesso", "Configuracao");
                }
                else
                {
                    ModelState.AddModelError("", "A senha atual esta incorreta ou a nova senha é inválida.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult TrocaSenhaSucesso()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Nome de usuário já existe. Digite um nome de usuário diferente.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Um nome de usuário para esse endereço de e-mail já existe. Digite um endereço de e-mail diferente.";

                case MembershipCreateStatus.InvalidPassword:
                    return "A senha fornecida é inválida. Por favor, insira um valor de senha válida.";

                case MembershipCreateStatus.InvalidEmail:
                    return "O endereço de e-mail fornecido é inválido. Por favor, verifique o valor e tente novamente.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "A resposta de recuperação de senha fornecida é inválida. Por favor, verifique o valor e tente novamente.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "A questão de recuperação de senha fornecida é inválida. Por favor, verifique o valor e tente novamente.";

                case MembershipCreateStatus.InvalidUserName:
                    return "O nome de usuário fornecido é inválido. Por favor, verifique o valor e tente novamente.";

                case MembershipCreateStatus.ProviderError:
                    return "O provedor de autenticação retornou um erro. Por favor, verifique a sua entrada e tente novamente. Se o problema persistir, contate o administrador do sistema.";

                case MembershipCreateStatus.UserRejected:
                    return "O pedido de criação do usuário foi cancelado. Por favor, verifique a sua entrada e tente novamente. Se o problema persistir, contate o administrador do sistema.";

                default:
                    return "Ocorreu um erro desconhecido. Por favor, verifique a sua entrada e tente novamente. Se o problema persistir, contate o administrador do sistema.";
            }
        }
        #endregion
    }


}
