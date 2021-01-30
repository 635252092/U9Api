namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// SOPushSMSV partial 
	/// </summary>	
	public partial class SOPushSMSV 
	{	
		internal BaseStrategy Select()
		{
			return new SOPushSMSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class SOPushSMSVImpementStrategy : BaseStrategy
	{
		public SOPushSMSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			SOPushSMSV bpObj = (SOPushSMSV)obj;
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}