using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ControleCaixaWeb.Models;

namespace ControleCaixaWeb.Controllers
{
    public class ContaUsuarioController : Controller
    {
                   //
            // GET: /Account/LogOn

            public ActionResult Entrar( )
            {
                return View( );
            }

            //
            // POST: /Account/LogOn

            [HttpPost]
            public ActionResult Entrar(Usuario model, string returnUrl)
            {
                if (ModelState.IsValid)
                {
                    if (Membership.ValidateUser(model.Nome, model.Senha))
                    {
                        FormsAuthentication.SetAuthCookie(model.Nome, model.Lembrar);
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "O Nome do Usuário Ou Senha Estão Incorretos.");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }

            //
            // GET: /Account/LogOff

            public ActionResult Sair( )
            {
                FormsAuthentication.SignOut( );

                return RedirectToAction("Index", "Home");
            }

            //
            // GET: /Account/Register

            public ActionResult CadastrarUsuario( )
            {
                return View( );
            }

            //
            // POST: /Account/Register

            [HttpPost]
            public ActionResult CadastrarUsuario(CadastrarUsuario model)
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(model.Nome, model.Senha, model.Email, null, null, true, null, out createStatus);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsAuthentication.SetAuthCookie(model.Nome, false /* createPersistentCookie */);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }

            //
            // GET: /Account/ChangePassword

            [Authorize]
            public ActionResult TrocarSenha( )
            {
                return View( );
            }

            //
            // POST: /Account/ChangePassword

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
                        return RedirectToAction("TrocaSenhaSucesso");
                    }
                    else
                    {
                        ModelState.AddModelError("", "A senha atual esta incorreta ou a nova senha é inválida.");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }

            //
            // GET: /Account/ChangePasswordSuccess

            public ActionResult TrocaSenhaSucesso( )
            {
                return View( );
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
