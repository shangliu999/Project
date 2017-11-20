﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.DAL
{
    [Table(name: "t11")]
    public class Color
    {
        [Required]
        [Column(name: "c0")]
        public int ID { get; set; }

        [Column(name: "c1")]
        public string Name { get; set; }

        [Column(name: "c2")]
        public int Sort { get; set; }
    }
}
