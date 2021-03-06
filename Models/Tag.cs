﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace radio.Models
{
    [Serializable]
    public class Tag
    {
        [XmlElement]
        public String tag { get; set; }

        public Tag()
        {

        }

        public Tag(String tag)
        {
            this.tag = tag;
        }

        public override string ToString()
        {
            return tag;
        }

    }
}
