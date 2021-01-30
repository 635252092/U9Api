







namespace U9Api.CustSV
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.Runtime.Serialization;
	using System.IO;
	using UFSoft.UBF.Util.Context;
	using UFSoft.UBF;
	using UFSoft.UBF.Exceptions;
	using UFSoft.UBF.Service.Base ;

    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.UFIDA.org", Name="U9Api.CustSV.ICreateRMRSV")]
    public interface ICreateRMRSV
    {
	[OperationContract()]
        System.String Do(UFSoft.UBF.Service.ISVContext context ,System.String jsonRequest);
    }

    [UFSoft.UBF.Service.ServiceImplement]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class CreateRMRSVStub : ISVStubBase, ICreateRMRSV
    {
        #region ICreateRMRSV Members

        //[OperationBehavior]
        public System.String Do(UFSoft.UBF.Service.ISVContext context , System.String jsonRequest)
        {
			
			ICommonDataContract commonData = CommonDataContractFactory.GetCommonData(context);
			return DoEx(commonData, jsonRequest);
        }
        
        //[OperationBehavior]
        public System.String DoEx(ICommonDataContract commonData, System.String jsonRequest)
        {
			this.CommonData = commonData ;
            try
            {
                BeforeInvoke("U9Api.CustSV.CreateRMRSV");                
                CreateRMRSV objectRef = new CreateRMRSV();
	
				objectRef.JsonRequest = jsonRequest;

				//处理返回类型.
				System.String result = objectRef.Do();
				return result ;
						return result;

	        }
			catch (System.Exception e)
            {
				DealException(e);
				throw;
            }
            finally
            {
				FinallyInvoke("U9Api.CustSV.CreateRMRSV");
            }
        }
	#endregion
    }
}
