

namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using NUnit.Framework;
	
	/// <summary>
	/// Business operation test
	/// </summary> 
	[TestFixture]		
	public class UnApproveRMAPullShipSVTest
	{
		private Proxy.UnApproveRMAPullShipSVProxy obj = new Proxy.UnApproveRMAPullShipSVProxy();

		public UnApproveRMAPullShipSVTest()
		{
		}
		#region AutoTestCode ...
		[Test]
		public void TestDo()
		{
			obj.Do() ;  
		
		}
		#endregion 				
	}
	
}