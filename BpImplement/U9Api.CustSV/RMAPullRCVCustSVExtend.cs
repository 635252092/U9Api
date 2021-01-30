namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// RMAPullRCVCustSV partial 
	/// </summary>	
	public partial class RMAPullRCVCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new RMAPullRCVCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class RMAPullRCVCustSVImpementStrategy : BaseStrategy
	{
		public RMAPullRCVCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			RMAPullRCVCustSV bpObj = (RMAPullRCVCustSV)obj;
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}