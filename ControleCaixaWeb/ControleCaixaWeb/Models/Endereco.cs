using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class Endereco
    {
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage = "O campo nome rua é obrigatório")]
        public virtual string NomeRua { get; set; }
        public virtual string Complemento { get; set; }
        [Required(ErrorMessage = "o campo Cep é obrigatório"), MaxLength(8, ErrorMessage = "O CEP não pode ter mais de 8 caracteres")]
        public virtual string CEP { get; set; }
        [Required(ErrorMessage = "o campo bairro é obrigatório")]
        public virtual string Bairro { get; set; }
        [Required(ErrorMessage = "o campo cidade é obrigatório")]
        public virtual string Cidade { get; set; }
        [Required(ErrorMessage = "o campo estado é obrigatório")]
        public virtual string Estado { get; set; }
    }
}