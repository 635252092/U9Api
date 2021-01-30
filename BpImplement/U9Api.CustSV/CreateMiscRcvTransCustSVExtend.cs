namespace U9Api.CustSV
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using U9Api.CustSV.Base;
    using U9Api.CustSV.Model.Request;
    using U9Api.CustSV.Utils;
    using UFIDA.U9.Base;
    using UFIDA.U9.CBO.Enums;
    using UFIDA.U9.CBO.Pub.Controller;
	using UFIDA.U9.CBO.SCM.Item;
	using UFIDA.U9.CBO.SCM.Warehouse;
    using UFIDA.U9.InvDoc.MiscRcv;
    using UFIDA.U9.ISV.MiscRcvISV;
    using UFIDA.U9.Lot;
    using UFSoft.UBF.AopFrame;
	using UFSoft.UBF.Business;
	using UFSoft.UBF.PL.Engine;
    using UFSoft.UBF.Util.Context;

	/// <summary>
	/// CreateMiscRcvTransCustSV partial 
	/// </summary>	
	public partial class CreateMiscRcvTransCustSV 
	{	
		internal BaseStrategy Select()
		{
			return new CreateMiscRcvTransCustSVImpementStrategy();	
		}		
	}

	#region  implement strategy	
	/// <summary>
	/// 创建杂收
	/// 
	/// </summary>	
	internal partial class CreateMiscRcvTransCustSVImpementStrategy : BaseStrategy
	{
		public CreateMiscRcvTransCustSVImpementStrategy() { }

		public override object Do(object obj)
		{
			CreateMiscRcvTransCustSV bpObj = (CreateMiscRcvTransCustSV)obj;

			if (string.IsNullOrEmpty(bpObj.JsonRequest))
			{
				return JsonUtil.GetFailResponse("string.IsNullOrEmpty(bpObj.JsonRequest)");
			}
			CreateMiscRcvTransRequest reqHeader = null;
			try
			{
				reqHeader = JsonUtil.GetJsonObject<CreateMiscRcvTransRequest>(bpObj.JsonRequest);
			}
			catch (Exception ex)
			{
				return JsonUtil.GetFailResponse("JSON格式错误");
			}

			StringBuilder res = new StringBuilder();
			StringBuilder debugInfo = new StringBuilder();
			debugInfo.AppendLine("strat...");
			debugInfo.AppendLine(JsonUtil.GetJsonString(Context.LoginOrg == null ? "Context.LoginOrg==null" : "Context.LoginOrg ok" + Context.LoginOrg.ID));

			try
			{
				IC_MiscRcvDTO dtoHeader = new IC_MiscRcvDTO();

				//dto.m_miscRcvDocType.m_iD = 1001007096190830;
				MiscRcvDocType miscRcvDocType = MiscRcvDocType.Finder.Find(string.Format("Org={0} and Code='{1}'", Context.LoginOrg.ID, reqHeader.DocTypeCode));
				if (miscRcvDocType == null)
				{
					return JsonUtil.GetFailResponse(reqHeader.DocTypeCode+U9Contant.NoFindDocType, debugInfo);
				}
				dtoHeader.MiscRcvDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
				//dtoHeader.MiscRcvDocType.ID = miscRcvDocType.ID;
				dtoHeader.MiscRcvDocType.Code = reqHeader.DocTypeCode;
				//dto.MiscRcvDocType.Code = "MiscRcv001";
				//dto.MiscRcvDocType.Name = "库存杂收[默认]";

				dtoHeader.SysState = UFSoft.UBF.PL.Engine.ObjectState.Inserted;

				dtoHeader.BusinessDate = DateTime.Parse(reqHeader.BusinessDate);
				dtoHeader.DocNo = reqHeader.WmsDocNo;

				dtoHeader.MiscRcvTransLs = new List<IC_MiscRcvTransLsDTO>();
				//  单体
				foreach (var reqLine in reqHeader.CreateMiscRcvTransLines)
				{
					IC_MiscRcvTransLsDTO dtoLine = new IC_MiscRcvTransLsDTO();
					if (reqLine.IsZeroCost)
					{
						dtoLine.IsZeroCost = true;
					}
					dtoLine.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfo();
					dtoLine.ItemInfo.ItemCode = reqLine.ItemCode;
					dtoLine.StoreUOMQty = reqLine.ItemQty;
					dtoLine.CostUOMQty = reqLine.ItemQty;
					dtoLine.StoreType = UFIDA.U9.CBO.Enums.StorageTypeEnum.Useable;
					dtoLine.OwnerOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
					dtoLine.OwnerOrg.ID = Context.LoginOrg.ID;

					dtoLine.SUToCURate = 1;
					dtoLine.SUToSBURate = 1;

					long lotKey = 0L;
					if (!string.IsNullOrEmpty(reqLine.LotCode) && CommonUtil.IsNeedLot(reqLine.ItemCode, Context.LoginOrg.ID))
					{
						LotMaster curLot = CommonUtil.GetLot(reqLine.LotCode, reqLine.ItemCode, reqLine.FromWHCode, LotSnStatusEnum.I_MiscRcv.Value);
						lotKey = curLot.ID;
					}
					//批号赋值
					if (lotKey > 0L)
					{
						dtoLine.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfo();
						dtoLine.LotInfo.LotCode = reqLine.LotCode;
						dtoLine.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKey();
						dtoLine.LotInfo.LotMaster.EntityID = lotKey;
						//dtoLine.LotInfo.DisabledDatetime = null ;
						//dtoLine.LotInfo.LotValidDate = 10000;
					}

					dtoLine.Wh = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
					dtoLine.Wh.Code = reqLine.FromWHCode;
					Warehouse warehouse = Warehouse.FindByCode(Context.LoginOrg, reqLine.BenefitWHCode);
					if (warehouse == null)
					{
						return JsonUtil.GetFailResponse(reqLine.BenefitWHCode+ "：受益仓库不能为空", debugInfo);
					}
					dtoLine.BenefitWh = warehouse.ID;

					dtoLine.BenefitDept = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTO();
					UFIDA.U9.CBO.HR.Department.Department department = UFIDA.U9.CBO.HR.Department.Department.FindByCode(reqLine.BenefitDeptCode);
					if (department == null)
					{
						return JsonUtil.GetFailResponse(reqLine.BenefitDeptCode+"：受益部门不能为空", debugInfo);
					}
					dtoLine.BenefitDept.Code = department.Code;


					dtoLine.MiscRcvCost = new List<IC_MiscRcvCostDTO>();
					dtoLine.MiscRcvTransBins = new List<IC_MiscRcvTransBinsDTO>();
					dtoLine.SysState = ObjectState.Inserted;
					//UFIDA.U9.ISV.MiscRcvISV.IC_MiscRcvTransBinsDTO bin = new UFIDA.U9.ISV.MiscRcvISV.IC_MiscRcvTransBinsDTO();
					//bin.BinInfo = new UFIDA.U9.CBO.SCM.Bin.BinInfo();
					//bin.BinInfo.Code = string.Empty;
					//bin.StoreUOMQty = reqLine.ItemQty;
					//dtoLine.MiscRcvTransBins.Add(bin);

					//获取价格
					//ItemMaster itemMaster = ItemMaster.Finder.Find(string.Format("Org={0} and Code='{1}'", Context.LoginOrg.ID, reqLine.ItemCode));
					//if (itemMaster == null)
					//{
					//	return JsonUtil.GetFailResponse("itemMaster == null",debugInfo);
					//}
					//if (itemMaster.RefrenceCost > 0)
					//{
					//	dtoLine.CostPrice = itemMaster.RefrenceCost;
					//	dtoLine.CostMny = dtoLine.CostPrice * dtoLine.CostUOMQty;
					//}
					//else
					//{
					//	dtoLine.IsZeroCost = true;
					//}

					dtoHeader.MiscRcvTransLs.Add(dtoLine);
				}

				//ThreadContext context = CreateContextObj();// 入口参数：线程上下文
				CommonCreateMiscRcv Client = new CommonCreateMiscRcv();
				Client.MiscRcvDTOList = new List<IC_MiscRcvDTO>();
				Client.MiscRcvDTOList.Add(dtoHeader);
				List<CommonArchiveDataDTO> listRes = Client.Do();
				if (listRes == null || listRes.Count == 0)
				{
					return JsonUtil.GetFailResponse("listRes == null || listRes.Count == 0", debugInfo);
				}
				CommonArchiveDataDTO curDto = listRes[0];

				//修改批号的有效期
				string sql = string.Format(@"UPDATE line SET LotInfo_DisabledDatetime=NULL from  InvDoc_MiscRcvTransL line 
INNER JOIN InvDoc_MiscRcvTrans header on line.MiscRcvTrans = header.ID
where header.Org = {0} AND header.DocNo = '{1}'", Context.LoginOrg.ID, curDto.Code);
				CommonUtil.RunUpdateSQL(sql);

				if (reqHeader.IsAutoApprove)
				{
					U9Api.CustSV.ApproveMiscRcvTransCustSV sv2 = new
					  ApproveMiscRcvTransCustSV();
					CommonApproveRequest req = new CommonApproveRequest();
					req.DocNo = curDto.Code;
					sv2.JsonRequest = JsonUtil.GetJsonString(req);
					return sv2.Do();
				}
				return JsonUtil.GetSuccessResponse(curDto.Code, "", debugInfo);
			}
			catch (Exception ex)
			{
				LogUtil.WriteDebugInfoLog(debugInfo.ToString());
				throw Base.U9Exception.GetInnerException(ex);
			}
		}
	}

	#endregion
	
	
}