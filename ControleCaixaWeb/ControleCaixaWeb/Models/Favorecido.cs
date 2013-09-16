using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
   public class Favorecido
    {
       public virtual long Codigo { get; set; }
       [Required(ErrorMessage = "O campo nome favorecido é obrigatório"),MaxLength(26,ErrorMessage="O campo nome favorecido não pode ter mais que 26 caracteres")]
       public virtual string NomeFavorecido { get; set; }
       public virtual string Observacao { get; set; }
       public virtual IList<Pagamento> PagamentosRecebidos { get; set; }
    }
}
