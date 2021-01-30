








namespace U9Api.CustSV.Proxy
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using System.ServiceModel;
	using System.Runtime.Serialization;
	using UFSoft.UBF;
	using UFSoft.UBF.Exceptions;
	using UFSoft.UBF.Util.Context;
	using UFSoft.UBF.Service;
	using UFSoft.UBF.Service.Base ;

    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.UFIDA.org", Name="U9Api.CustSV.IDeleteCustSV")]
    public interface IDeleteCustSV
    {
		[OperationContract()]
		System.String Do(UFSoft.UBF.Service.ISVContext context ,System.String jsonRequest);
    }
	[Serializable]    
    public class DeleteCustSVProxy : ServiceProxyBase//, U9Api.CustSV.Proxy.IDeleteCustSV
    {
	#region Fields	
				private System.String jsonRequest ;
			
	#endregion	
		
	#region Properties
	
				

		/// <summary>
		/// Json请求 (该属性可为空,且无默认值)
		/// 删除单据.Misc.Json请求
		/// </summary>
		/// <value>System.String</value>
		public System.String JsonRequest
		{
			get	
			{	
				return this.jsonRequest;
			}

			set	
			{	
				this.jsonRequest = value;	
			}
		}		
			
	#endregion	


	#region Constructors
        public DeleteCustSVProxy()
        {
        }
        #endregion
        
        #region 跨site调用
        public System.String Do(string targetSite)
        {
  			InitKeyList() ;
			System.String result = (System.String)InvokeBySite<U9Api.CustSV.Proxy.IDeleteCustSV>(targetSite);
			return GetRealResult(result);
        }
        #endregion end跨site调用

		#region 跨组织调用
        public System.String Do(long targetOrgId)
        {
  			InitKeyList() ;
			System.String result = (System.String)InvokeByOrg<U9Api.CustSV.Proxy.IDeleteCustSV>(targetOrgId);
			return GetRealResult(result);
        }
		#endregion end跨组织调用

		#region Public Method
		
        public System.String Do()
        {
  			InitKeyList() ;
 			System.String result = (System.String)InvokeAgent<U9Api.CustSV.Proxy.IDeleteCustSV>();
			return GetRealResult(result);
        }
        
		protected override object InvokeImplement<T>(T oChannel)
        {
			IContext context = ContextManager.Context;			

            IDeleteCustSV channel = oChannel as IDeleteCustSV;
            if (channel != null)
            {
				UFSoft.UBF.Service.ISVContext isvContext =  GetISVContext(context);
				return channel.Do(isvContext , jsonRequest);
	    }
            return  null;
        }
		#endregion
		
		//处理由于序列化导致的返回值接口变化，而进行返回值的实际类型转换处理．
		private System.String GetRealResult(System.String result)
		{

				return result ;
		}
		#region  Init KeyList 
		//初始化SKey集合--由于接口不一样.BP.SV都要处理
		private void InitKeyList()
		{
			System.Collections.Hashtable dict = new System.Collections.Hashtable() ;
					
		}
		#endregion 

    }
}



