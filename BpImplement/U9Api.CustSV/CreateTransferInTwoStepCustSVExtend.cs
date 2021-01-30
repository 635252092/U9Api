namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using U9Api.CustSV.Base;
	using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.InvDoc.TransferIn;
    using UFIDA.U9.InvDoc.TransferOut;
    using UFSoft.UBF.AopFrame;
    using UFSoft.UBF.Business;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// CreateTransferInTwoStepCustSV partial 
	/// </summary>	
	public partial class CreateTransferInTwoStepCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateTransferInTwoStepCustSVImpementStrategy();	
		}		
	}

	#region  implement strategy	
	/// <summary>
	/// Impement Implement
	/// 
	/// </summary>	
	internal partial class CreateTransferInTwoStepCustSVImpementStrategy : BaseStrategy
	{
		public CreateTransferInTwoStepCustSVImpementStrategy() { }

		public override object Do(object obj)
		{
			CreateTransferInTwoStepCustSV bpObj = (CreateTransferInTwoStepCustSV)obj;

			if (string.IsNullOrEmpty(bpObj.JsonRequest))
			{
				return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
			}
			CreateTransferInTwoStepRequest reqHeader = null;
			try
			{
				reqHeader = JsonUtil.GetJsonObject<CreateTransferInTwoStepRequest>(bpObj.JsonRequest);
				if (reqHeader == null)
				{
					throw new Exception("request == null");
				}
			}
			catch (Exception ex)
			{
				return JsonUtil.GetFailResponse("JSON格式错误;"+ ex.Message);
			}
			StringBuilder res = new StringBuilder();
			StringBuilder debugInfo = new StringBuilder();
			debugInfo.AppendLine("strat...");
			debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

			UFIDA.U9.Base.Organization.Organization organization = UFIDA.U9.Base.Organization.Organization.FindByCode(reqHeader.OutOrg);
			if (organization == null)
				return JsonUtil.GetFailResponse(reqHeader.OutOrg+U9Contant.NoFindOrg, debugInfo);
			using (UFSoft.UBF.Transactions.UBFTransactionScope scope = new UFSoft.UBF.Transactions.UBFTransactionScope(UFSoft.UBF.Transactions.TransactionOption.RequiresNew))
			{
				try
			{
				List<TransferOutToTransInDTO> list = new List<TransferOutToTransInDTO>();

				foreach (var reqLine in reqHeader.CreateTransferInTwoStepLines)
				{
					TransferOut transferOut = TransferOut.Finder.Find(string.Format("Org={0} and DocNo='{1}'", organization.ID, reqLine.SrcDocNo));
					if (transferOut == null)
					{
						return JsonUtil.GetFailResponse(string.Format("组织:{0} 单号:{1} 没有找到【库存调出单】",reqHeader.OutOrg, reqLine.SrcDocNo), debugInfo);
					}
					foreach (var transOutLine in transferOut.TransOutLines)
					{
						foreach (var curLine in transOutLine.TransOutSubLines)
						{
							TransferOutToTransInDTO dto = new TransferOutToTransInDTO();
							dto.DocSubLine = curLine.ID;
							if (curLine.TransInDept != null)
							{
								dto.ToDept = curLine.TransInDept.Key;
							}
							if (curLine.Project != null)
							{
								dto.ToProject = curLine.Project.Key;
							}
							if (curLine.Task != null)
							{
								dto.ToTask = curLine.Task.Key;
							}
							Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.WHCode);
							if (warehouse == null)
							{
								return JsonUtil.GetFailResponse(reqLine.WHCode+U9Contant.NoFindWH,debugInfo);
							}
							dto.ToWh = warehouse.Key;
							if (warehouse.Manager != null)
							{
								dto.ToWhMan = warehouse.Manager.Key;
							}

							dto.TransferInQty = curLine.TransOutSUQty;
							dto.ToCustomerInfo = curLine.TransInCustInfo;

							dto.ToSupplierInfo = curLine.TransInSuppInfo;

							list.Add(dto);
						}
					}
				}
				
				TransfeOutToTransferInDTOListSV sv1 = new TransfeOutToTransferInDTOListSV();
				sv1.TransOutToTransInDTOList = new List<TransferOutToTransInDTO>();
				sv1.TransOutToTransInDTOList = list;
				List<TransferInDTO> list2 = sv1.Do();

				CreateTransferInSRV sv2 = new CreateTransferInSRV();
				sv2.TransferInDTOList = new List<TransferInDTO>();
				sv2.TransferInDTOList = list2;
				sv2.SplitMergeRule = new List<string>();
				List<long> listRes = sv2.Do();
				if (listRes == null || listRes.Count == 0)
				{
					return JsonUtil.GetFailResponse("listRes == null || listRes.Count == 0", debugInfo);
				}
				TransferIn doc = TransferIn.Finder.FindByID(listRes[0]);
				using (ISession session = Session.Open())
				{

					doc.DocNo = reqHeader.WmsDocNo;
					doc.Memo = reqHeader.Remark;
					doc.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
					doc.SOBAccountPeriod =CommonUtil.GetSOBAccountPeriodEntity(doc.BusinessDate, Context.LoginOrg.ID);
					foreach (var current in doc.TransInAccountPeriods)
					{
						current.SOBAccountPeriod = doc.SOBAccountPeriod;
					}
					foreach (var transInLine in doc.TransInLines)
					{
						transInLine.StorageType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
						foreach (var current in transInLine.TransInSubLines)
						{
							current.SOBAccountPeriod = doc.SOBAccountPeriod;
							foreach (var current2 in current.TransInAccountPeriods)
							{
								current2.SOBAccountPeriod = doc.SOBAccountPeriod;
							}
						}
					}
					
					session.Commit();
				}

				if (reqHeader.IsAutoApprove)
				{
					U9Api.CustSV.ApproveTransferInCustSV sv3 = new
					ApproveTransferInCustSV();
					CommonApproveRequest req = new CommonApproveRequest();
					req.DocNo = doc.DocNo;
					req.BusinessDate = reqHeader.BusinessDate;
					sv3.JsonRequest = JsonUtil.GetJsonString(req);
						res.Clear();
						res.Append(sv3.Do());
						scope.Commit();
						return res.ToString();
					}

					scope.Commit();
					return JsonUtil.GetSuccessResponse(doc.DocNo, doc.Status.Name, debugInfo);
			}
				catch (Exception ex)
				{
					LogUtil.WriteDebugInfoLog(debugInfo.ToString());
					//throw Base.U9Exception.GetInnerException(ex);
					scope.Rollback();
					return JsonUtil.GetFailResponse(ex, debugInfo);//返回，不执行 scope.Commit();就会回滚
				}
			}
		}
	}

	#endregion
	
	
}