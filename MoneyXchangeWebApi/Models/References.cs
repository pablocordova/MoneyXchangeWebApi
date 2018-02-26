using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyXchangeWebApi.Models
{
    public class References
    {
        [Key]
        public string Symbol { get; set; }
    }
}
