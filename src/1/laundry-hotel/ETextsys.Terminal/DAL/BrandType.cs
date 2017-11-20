﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.DAL
{
    [Table(name: "t5")]
    public class BrandType
    {
        [Required]
        [Column(name: "c0")]
        public int ID { get; set; }

        [Column(name: "c1")]
        public string Name { get; set; }

        [Column(name: "c2")]
        public string Code { get; set; }

        [Column(name: "c3")]
        public string Desc { get; set; }

        [Column(name: "c4")]
        public int Sort { get; set; }        
    }
}
