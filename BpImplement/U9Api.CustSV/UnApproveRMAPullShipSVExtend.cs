namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// UnApproveRMAPullShipSV partial 
	/// </summary>	
	public partial class UnApproveRMAPullShipSV 
	{	
		internal BaseStrategy Select()
		{
			return new UnApproveRMAPullShipSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class UnApproveRMAPullShipSVImpementStrategy : BaseStrategy
	{
		public UnApproveRMAPullShipSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			UnApproveRMAPullShipSV bpObj = (UnApproveRMAPullShipSV)obj;
			
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}