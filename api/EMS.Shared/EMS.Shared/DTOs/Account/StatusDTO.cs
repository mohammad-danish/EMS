using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Shared.DTOs.Account
{
    public class StatusDTO<T,V>
    {
        public T Id { get; set; }
        public string Name { get; set; }
        public V Status { get; set; }
        public string Error { get; set; }

    }
}
