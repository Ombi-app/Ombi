using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Requests
{
    public class MovieRequestModel : BaseRequestModel
    {
       
        public string ImdbId { get; set; }
      
    }
}