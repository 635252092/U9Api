namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ApproveMaterialInSV partial 
	/// </summary>	
	public partial class ApproveMaterialInSV 
	{	
		internal BaseStrategy Select()
		{
			return new ApproveMaterialInSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ApproveMaterialInSVImpementStrategy : BaseStrategy
	{
		public ApproveMaterialInSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			ApproveMaterialInSV bpObj = (ApproveMaterialInSV)obj;
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}