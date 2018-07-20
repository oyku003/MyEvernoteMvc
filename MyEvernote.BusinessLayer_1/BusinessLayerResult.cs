﻿using MyEvernote.Entities_1.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.BusinessLayer_1
{
  public class BusinessLayerResult<T> where T:class
    {
        public List<ErrorMessageObj> Errors { get; set; }

        public T Result { get; set; }

        public BusinessLayerResult()
        {
            Errors =new List<ErrorMessageObj>();//error null gelirse kod patlamasın diye...
        }
        public void AddError(ErrorMessageCode code, string message )
        {
            Errors.Add(new ErrorMessageObj() {
              Code=code,
              Message=message
            });
        }
    }
}
