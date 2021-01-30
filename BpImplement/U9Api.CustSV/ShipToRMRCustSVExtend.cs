namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text; 
	using UFSoft.UBF.AopFrame;	
	using UFSoft.UBF.Util.Context;

	/// <summary>
	/// ShipToRMRCustSV partial 
	/// </summary>	
	public partial class ShipToRMRCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new ShipToRMRCustSVImpementStrategy();	
		}		
	}
	
	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class ShipToRMRCustSVImpementStrategy : BaseStrategy
	{
		public ShipToRMRCustSVImpementStrategy() { }

		public override object Do(object obj)
		{						
			ShipToRMRCustSV bpObj = (ShipToRMRCustSV)obj;
            UFIDA.U9.SM.Ship.ShipToRMR proxy = new UFIDA.U9.SM.Ship.ShipToRMR();
            proxy.Do();
            proxy.SrcDocInfo = new List<UFIDA.U9.SM.Ship.ToRMRTransformDTO>();
            UFIDA.U9.SM.Ship.ToRMRTransformDTO dto = new UFIDA.U9.SM.Ship.ToRMRTransformDTO();
            UFIDA.U9.SM.SO.SOTORMR proxy2 = new UFIDA.U9.SM.SO.SOTORMR();
            proxy2.
            dto.SrcDocLineKey = 0;
			//get business operation context is as follows
			//IContext context = ContextManager.Context	
			
			//auto generating code end,underside is user custom code
			//and if you Implement replace this Exception Code...
			throw new NotImplementedException();
		}		
	}

	#endregion
	
	
}