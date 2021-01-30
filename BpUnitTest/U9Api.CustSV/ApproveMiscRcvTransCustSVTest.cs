

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
	public class ApproveMiscRcvTransCustSVTest
	{
		private Proxy.ApproveMiscRcvTransCustSVProxy obj = new Proxy.ApproveMiscRcvTransCustSVProxy();

		public ApproveMiscRcvTransCustSVTest()
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