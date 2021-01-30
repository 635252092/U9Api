//using System;
//using System.Collections.Generic;
//using System.Data;
//using UFIDA.U9.Base.Currency;
//using UFIDA.U9.Base.Doc;
//using UFIDA.U9.Base.DTOs;
//using UFIDA.U9.Base.FlexField.DescFlexField;
//using UFIDA.U9.Base.PropertyTypes;
//using UFIDA.U9.CBO.DTOs;
//using UFIDA.U9.CBO.FI.MatchLayerSet;
//using UFIDA.U9.CBO.SCM.DTOs;
//using UFIDA.U9.CBO.SCM.Enums;
//using UFIDA.U9.CBO.SCM.Item;
//using UFIDA.U9.CBO.SCM.Lot;
//using UFIDA.U9.CBO.SCM.PropertyTypes;
//using UFIDA.U9.CBO.SCM.Supplier;
//using UFIDA.U9.CBO.SCM.Util;
//using UFIDA.U9.Lot;
//using UFIDA.U9.Lot.Proxy;
//using UFIDA.U9.PM.DTOs;
//using UFIDA.U9.PM.PropertyType;
//using UFIDA.U9.PM.Rcv;
//using UFIDA.U9.SM.Enums;
//using UFIDA.U9.SM.Ship;
//using UFIDA.U9.SM.SO;
//using UFSoft.UBF.AopFrame;
//using UFSoft.UBF.PL;
//using UFSoft.UBF.Sys.Database;

//namespace UFIDA.U9.SM.RMR
//{
//    internal class GetRcvDTOsByRMRImpementStrategy : BaseStrategy
//    {
//        internal class TransferRMRInfo
//        {
//            public DoubleQuantity TransQty;

//            public DateTime BusinessDate;

//            public DateTime ArriveDate;
//        }

//        private class CostInfoDTO
//        {
//            public long TC;

//            public long AC;

//            public decimal TCToACExRate;

//            public int TCToACExRateType = -1;

//            public int TCToFCExRateType = -1;

//            public int TCToACExRateRound;

//            public long TaxSchedule;

//            public long CU;

//            public long CBU;

//            public decimal CUToCBURate;

//            public decimal TBUToCBURate;

//            public decimal EvaluationMnyAC;

//            public decimal EvaluationMnyFC;

//            public decimal EvaluationMnyTC;

//            public decimal EvaluationPriceCU;

//            public decimal EvaluationPricePU;

//            public decimal EvaluationPriceTU;

//            public decimal EvaluationPriceTBU;

//            public long Operator;

//            public long Dept;

//            public decimal PurCostAC = 0m;

//            public decimal PurCostTC = 0m;

//            public decimal PurCostFC = 0m;

//            public long IU = 0L;

//            public long IBU = 0L;

//            public decimal IUToIBURate = 0m;

//            public decimal TBUToIBURate = 0m;

//            public bool IsZeroCost;
//        }

//        private GetRcvDTOsByRMR bpObj = null;

//        public override object Do(object obj)
//        {
//            this.bpObj = (GetRcvDTOsByRMR)obj;
//            RcvFromRMRDTO rcvFromRMRDTO = new RcvFromRMRDTO();
//            List<RMRLineAndQtyDTO> rMRLineAndQtys = this.bpObj.RMRLineAndQtys;
//            List<string> splitRules = this.bpObj.SplitRules;
//            object result;
//            if (rMRLineAndQtys == null || rMRLineAndQtys.Count == 0)
//            {
//                result = rcvFromRMRDTO;
//            }
//            else
//            {
//                RMRLine.EntityList rMRLines = this.GetRMRLines(rMRLineAndQtys);
//                Dictionary<long, RMRLineAndQtyDTO> dictionary = new Dictionary<long, RMRLineAndQtyDTO>();
//                foreach (RMRLineAndQtyDTO current in rMRLineAndQtys)
//                {
//                    if (!dictionary.ContainsKey(current.RMRLineID))
//                    {
//                        dictionary.Add(current.RMRLineID, current);
//                    }
//                }
//                if (rMRLines == null)
//                {
//                    result = rcvFromRMRDTO;
//                }
//                else
//                {
//                    FlexFieldMapingDTO rMRToRcvFlexMapping = this.GetRMRToRcvFlexMapping();
//                    List<string> splitDocStr = this.GetSplitDocStr(splitRules, rMRToRcvFlexMapping);
//                    Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo> dictionaryFromDTOes = this.GetDictionaryFromDTOes(rMRLines, dictionary);
//                    Dictionary<string, IList<RMRLine>> detachDocs = this.GetDetachDocs(dictionaryFromDTOes, splitDocStr);
//                    List<RcvHeadDTOData> rtnDTOs = this.GetRtnDTOs(detachDocs, rMRToRcvFlexMapping, dictionary);
//                    rcvFromRMRDTO.RcvHeadDatas = rtnDTOs;
//                    result = rcvFromRMRDTO;
//                }
//            }
//            return result;
//        }

//        private List<RcvHeadDTOData> GetRtnDTOs(Dictionary<string, IList<RMRLine>> lineDocs, FlexFieldMapingDTO flexmapping, Dictionary<long, RMRLineAndQtyDTO> rmrDtos)
//        {
//            List<RcvHeadDTOData> list = new List<RcvHeadDTOData>();
//            List<RcvHeadDTOData> result;
//            if (lineDocs == null || lineDocs.Count == 0)
//            {
//                result = list;
//            }
//            else
//            {
//                RcvHeadDTOData rcvHeadDTOData = null;
//                using (Dictionary<string, IList<RMRLine>>.ValueCollection.Enumerator enumerator = lineDocs.Values.GetEnumerator())
//                {
//                    while (enumerator.MoveNext())
//                    {
//                        List<RMRLine> list2 = (List<RMRLine>)enumerator.Current;
//                        bool flag = true;
//                        bool[] array = new bool[30];
//                        for (int i = 0; i < 30; i++)
//                        {
//                            array[i] = true;
//                        }
//                        rcvHeadDTOData = new RcvHeadDTOData();
//                        rcvHeadDTOData.set_RcvFees(new List<RcvFeeData>());
//                        RMRLine rMRLine = list2[0];
//                        rcvHeadDTOData.set_RcvHead(new ReceivementData());
//                        rcvHeadDTOData.get_RcvHead().set_BizType(BusinessTypeEnum.get_SD_StandardSale().get_Value());
//                        GetRcvDTOsByRMRImpementStrategy.CostInfoDTO srcDocCostInfo = this.GetSrcDocCostInfo(rMRLine);
//                        rcvHeadDTOData.get_RcvHead().set_TC(srcDocCostInfo.TC);
//                        rcvHeadDTOData.get_RcvHead().set_AC(srcDocCostInfo.AC);
//                        rcvHeadDTOData.get_RcvHead().set_TCToACExRate(srcDocCostInfo.TCToACExRate);
//                        rcvHeadDTOData.get_RcvHead().set_TCToACExRateType(srcDocCostInfo.TCToACExRateType);
//                        rcvHeadDTOData.get_RcvHead().set_SrcDocType(4);
//                        rcvHeadDTOData.get_RcvHead().set_RtnCustomer(rMRLine.get_RMR().get_RMRHead().get_Customer().ToEntityData());
//                        rcvHeadDTOData.get_RcvHead().set_Status(0);
//                        rcvHeadDTOData.get_RcvHead().set_Org((rMRLine.get_RMR().get_ReceiveOrg() != null) ? rMRLine.get_RMR().get_ReceiveOrgKey().get_ID() : 0L);
//                        rcvHeadDTOData.get_RcvHead().set_PurOrg((rMRLine.get_RMR().get_OperatingOrg() != null) ? rMRLine.get_RMR().get_OperatingOrgKey().get_ID() : 0L);
//                        rcvHeadDTOData.get_RcvHead().set_BalanceOrg(rMRLine.get_RMR().get_RMRHead().get_BalanceOrgKey().get_ID());
//                        rcvHeadDTOData.get_RcvHead().set_BusinessDate(rmrDtos[rMRLine.get_ID()].TransformDate);
//                        string memo = this.isFromSameRMR(list2) ? list2[0].get_RMR().get_RMRHead().get_Remark() : string.Empty;
//                        rcvHeadDTOData.get_RcvHead().set_Memo(memo);
//                        if (rMRLine.get_RMR().get_TCKey() != null)
//                        {
//                            rcvHeadDTOData.get_RcvHead().set_TC(rMRLine.get_RMR().get_TCKey().get_ID());
//                        }
//                        rcvHeadDTOData.get_RcvHead().set_IsRelationCompany(rMRLine.get_RMR().get_IsRelationOrg());
//                        if (rMRLine.get_RMR().get_SrcDocType() == RMRSrcDocTypeEnum.get_Empty())
//                        {
//                            rcvHeadDTOData.get_RcvHead().set_IsACFromSrc(false);
//                        }
//                        else
//                        {
//                            rcvHeadDTOData.get_RcvHead().set_IsACFromSrc(true);
//                        }
//                        rcvHeadDTOData.set_RcvAddrs(this.GetAddresses(rMRLine));
//                        rcvHeadDTOData.set_RcvContacts(this.GetContacts(rMRLine));
//                        this.SetDescFlexValues(rMRLine, ref flag, ref array, flexmapping, rcvHeadDTOData);
//                        rcvHeadDTOData.set_RcvLineDTOs(new List<RcvLineDTOData>());
//                        foreach (RMRLine current in list2)
//                        {
//                            RMRLineAndQtyDTO dealQty = rmrDtos[current.get_ID()];
//                            RcvLineDTOData item = this.FillRcvLine(current, dealQty, flexmapping);
//                            rcvHeadDTOData.get_RcvLineDTOs().Add(item);
//                        }
//                        if (rcvHeadDTOData != null)
//                        {
//                            list.Add(rcvHeadDTOData);
//                        }
//                    }
//                }
//                result = list;
//            }
//            return result;
//        }

//        private bool isFromSameRMR(List<RMRLine> docLines)
//        {
//            bool result;
//            if (docLines.Count == 1)
//            {
//                result = true;
//            }
//            else
//            {
//                RMRHead.EntityKey rMRHeadKey = docLines[0].get_RMR().get_RMRHeadKey();
//                for (int i = 1; i < docLines.Count; i++)
//                {
//                    if (rMRHeadKey != docLines[i].get_RMR().get_RMRHeadKey())
//                    {
//                        result = false;
//                        return result;
//                    }
//                }
//                result = true;
//            }
//            return result;
//        }

//        private GetRcvDTOsByRMRImpementStrategy.CostInfoDTO GetSrcDocCostInfo(RMRLine rmrLine)
//        {
//            GetRcvDTOsByRMRImpementStrategy.CostInfoDTO costInfoDTO = new GetRcvDTOsByRMRImpementStrategy.CostInfoDTO();
//            if (rmrLine.get_RMR().get_SrcDocType().get_Value() == RMRSrcDocTypeEnum.get_SaleOrder().get_Value())
//            {
//                SO sO = SO.get_Finder().FindByID(rmrLine.get_RMR().get_SrcDoc());
//                SOLine sOLine = SOLine.get_Finder().FindByID(rmrLine.get_RMR().get_SrcDocLine());
//                SOShipline sOShipline = SOShipline.get_Finder().FindByID(rmrLine.get_RMR().get_SrcDocSubLine());
//                if (sO != null)
//                {
//                    if (sO.get_SellerKey() != null)
//                    {
//                        costInfoDTO.Operator = sO.get_SellerKey().get_ID();
//                    }
//                    if (sO.get_SaleDepartmentKey() != null)
//                    {
//                        costInfoDTO.Dept = sO.get_SaleDepartmentKey().get_ID();
//                    }
//                    if (sO.get_TCKey() != null)
//                    {
//                        costInfoDTO.TC = sO.get_TCKey().get_ID();
//                    }
//                    if (sO.get_ACKey() != null)
//                    {
//                        costInfoDTO.AC = sO.get_ACKey().get_ID();
//                    }
//                    costInfoDTO.TCToACExRate = sO.get_TCToACExchRate();
//                    if (sO.get_TCToACExchRateType() != null)
//                    {
//                        costInfoDTO.TCToFCExRateType = sO.get_ACToFCRateType().get_Value();
//                        costInfoDTO.TCToACExRateType = sO.get_ACToFCRateType().get_Value();
//                    }
//                    if (sO.get_TC().get_MoneyRound() != null)
//                    {
//                        costInfoDTO.TCToACExRateRound = sO.get_TC().get_MoneyRound().get_Precision();
//                    }
//                    costInfoDTO.CU = ((sOShipline.get_CUKey() != null) ? sOShipline.get_CUKey().get_ID() : -1L);
//                    costInfoDTO.CBU = ((sOShipline.get_CBUKey() != null) ? sOShipline.get_CBUKey().get_ID() : -1L);
//                    costInfoDTO.CUToCBURate = sOShipline.get_CUToCBURate();
//                    costInfoDTO.TBUToCBURate = sOShipline.get_TUBaseToCUBaseRate();
//                    if (sOLine.get_TaxScheduleKey() != null)
//                    {
//                        costInfoDTO.TaxSchedule = sOLine.get_TaxScheduleKey().get_ID();
//                    }
//                    costInfoDTO.IU = ((sOShipline.get_IUKey() != null) ? sOShipline.get_IUKey().get_ID() : -1L);
//                    costInfoDTO.IBU = ((sOShipline.get_IBUKey() != null) ? sOShipline.get_IBUKey().get_ID() : -1L);
//                    costInfoDTO.TBUToIBURate = sOShipline.get_TUBaseToIUBaseRate();
//                    Money money = new QueryRealCost
//                    {
//                        SaleInfoes = new List<SaleInfoDTO>
//                        {
//                            new SaleInfoDTO
//                            {
//                                Currency = sO.get_TCKey(),
//                                ItemInfo = rmrLine.get_ItemInfo(),
//                                No = 1,
//                                SOLineID = rmrLine.get_RMR().get_SrcDocLine()
//                            }
//                        }
//                    }.Do();
//                    costInfoDTO.EvaluationMnyTC = sOShipline.get_TotalMoneyTC();
//                    costInfoDTO.EvaluationMnyAC = sOShipline.get_TotalMoneyTC() * sO.get_TCToACExchRate();
//                    costInfoDTO.EvaluationMnyFC = sOShipline.get_TotalMoneyTC() * sO.get_TCToFCExchRate();
//                }
//            }
//            else if (rmrLine.get_RMR().get_SrcDocType().get_Value() == RMRSrcDocTypeEnum.get_Shipment().get_Value())
//            {
//                Ship ship = Ship.get_Finder().FindByID(rmrLine.get_RMR().get_SrcDoc());
//                ShipLine shipLine = ShipLine.get_Finder().FindByID(rmrLine.get_RMR().get_SrcDocLine());
//                if (shipLine != null)
//                {
//                    if (ship.get_TCKey() != null)
//                    {
//                        costInfoDTO.TC = ship.get_TCKey().get_ID();
//                    }
//                    if (ship.get_ACKey() != null)
//                    {
//                        costInfoDTO.AC = ship.get_ACKey().get_ID();
//                    }
//                    costInfoDTO.TCToACExRate = shipLine.get_TCToACExRate();
//                    if (ship.get_ExchangeRateType() != null)
//                    {
//                        costInfoDTO.TCToFCExRateType = ship.get_ExchangeRateType().get_Value();
//                        costInfoDTO.TCToACExRateType = ship.get_ExchangeRateType().get_Value();
//                    }
//                    if (shipLine.get_CostUomKey() != null)
//                    {
//                        costInfoDTO.CU = shipLine.get_CostUomKey().get_ID();
//                    }
//                    if (shipLine.get_CostBaseUomKey() != null)
//                    {
//                        costInfoDTO.CBU = shipLine.get_CostBaseUomKey().get_ID();
//                    }
//                    costInfoDTO.CUToCBURate = shipLine.get_CostRatetoBase();
//                    costInfoDTO.TBUToCBURate = shipLine.get_TUCUConvRatio();
//                    if (shipLine.get_InvUom() != null)
//                    {
//                        costInfoDTO.IU = shipLine.get_InvUomKey().get_ID();
//                    }
//                    if (shipLine.get_InvBaseUom() != null)
//                    {
//                        costInfoDTO.IBU = shipLine.get_InvBaseUom().get_ID();
//                    }
//                    costInfoDTO.IUToIBURate = shipLine.get_InvRatetoBase();
//                    costInfoDTO.TBUToIBURate = 0m;
//                    if (shipLine.get_TaxScheduleKey() != null)
//                    {
//                        costInfoDTO.TaxSchedule = shipLine.get_TaxScheduleKey().get_ID();
//                    }
//                    if (ship.get_SellerKey() != null)
//                    {
//                        costInfoDTO.Operator = ship.get_SellerKey().get_ID();
//                    }
//                    if (ship.get_SaleDeptKey() != null)
//                    {
//                        costInfoDTO.Dept = ship.get_SaleDeptKey().get_ID();
//                    }
//                    FindShipCostInfo findShipCostInfo = new FindShipCostInfo();
//                    List<UFIDA.U9.CBO.SCM.DTOs.CostInfoDTO> list = new List<UFIDA.U9.CBO.SCM.DTOs.CostInfoDTO>();
//                    UFIDA.U9.CBO.SCM.DTOs.CostInfoDTO costInfoDTO2 = new UFIDA.U9.CBO.SCM.DTOs.CostInfoDTO();
//                    costInfoDTO2.set_InvBaseUOMKey(rmrLine.get_IBUKey());
//                    costInfoDTO2.set_InvUOMKey(rmrLine.get_IUKey());
//                    costInfoDTO2.set_InvUomToBaseRatio(rmrLine.get_IUToIBURate());
//                    costInfoDTO2.set_SrcDocLineID(rmrLine.get_RMR().get_SrcDocLine());
//                    costInfoDTO2.set_SrcDocSubLineID(rmrLine.get_RMR().get_SrcDocSubLine());
//                    costInfoDTO2.set_TBUToCBURatio(costInfoDTO.TBUToCBURate);
//                    costInfoDTO2.set_TBUToIBURatio(rmrLine.get_TBUToIBURate());
//                    costInfoDTO2.set_CostBaseUOMKey(shipLine.get_CostBaseUomKey());
//                    costInfoDTO2.set_CostUOMKey(shipLine.get_CostUomKey());
//                    costInfoDTO2.set_CostUomBaseRatio(shipLine.get_CostRatetoBase());
//                    list.Add(costInfoDTO2);
//                    findShipCostInfo.CostInfos = list;
//                    List<UFIDA.U9.CBO.SCM.DTOs.CostInfoDTO> list2 = findShipCostInfo.Do();
//                    if (list2 != null && list2.Count > 0)
//                    {
//                        costInfoDTO.EvaluationPriceCU = list2[0].get_CostPrice().get_Amount();
//                        costInfoDTO.EvaluationPriceTU = ship.get_TC().get_PriceRound().GetRoundValue(costInfoDTO.EvaluationPriceCU / costInfoDTO.CUToCBURate * rmrLine.get_TUToTBURate() * costInfoDTO.TBUToCBURate);
//                        costInfoDTO.EvaluationPriceTBU = ship.get_TC().get_PriceRound().GetRoundValue(costInfoDTO.EvaluationPriceCU / costInfoDTO.CUToCBURate * costInfoDTO.TBUToCBURate);
//                        costInfoDTO.EvaluationPricePU = ship.get_TC().get_PriceRound().GetRoundValue(costInfoDTO.EvaluationPriceTBU / rmrLine.get_TBUToPBURate() * rmrLine.get_PUToPBURate());
//                    }
//                    costInfoDTO.EvaluationMnyTC = shipLine.get_TotalMoneyTC();
//                    costInfoDTO.EvaluationMnyAC = shipLine.get_TotalMoney();
//                    costInfoDTO.EvaluationMnyFC = shipLine.get_TotalMoneyFC();
//                    costInfoDTO.IsZeroCost = shipLine.get_IsZeroCost();
//                }
//            }
//            return costInfoDTO;
//        }

//        private decimal PriceConvert(decimal fromPrice, long fromCurrency, long toCurrency)
//        {
//            decimal result;
//            if (fromCurrency == toCurrency)
//            {
//                result = fromPrice;
//            }
//            else
//            {
//                decimal dateExchangeRate = BaseBPAccessor.GetDateExchangeRate(new Currency.EntityKey(fromCurrency), new Currency.EntityKey(toCurrency), SCMProfile.get_SD_SDAndFI_ExchangeRateTypes(), DateTime.Now);
//                result = fromPrice * dateExchangeRate;
//            }
//            return result;
//        }

//        private void GetDescFlexSegmentsData(RMRLine line, FlexFieldMapingDTO flexmapping, bool ishead, RcvLineData targetLine)
//        {
//            Dictionary<string, SourceEntityAndDescFieldsDTO> dictionary = new Dictionary<string, SourceEntityAndDescFieldsDTO>();
//            SourceEntityAndDescFieldsDTO sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//            sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_DescFlexField());
//            sourceEntityAndDescFieldsDTO.set_SourceEntity(line);
//            dictionary.Add(RMRLine.EntityRes.get_BE_FullName(), sourceEntityAndDescFieldsDTO);
//            sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//            sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_RMR().get_DescFlexField());
//            sourceEntityAndDescFieldsDTO.set_SourceEntity(line.get_RMR());
//            dictionary.Add(RMR.EntityRes.get_BE_FullName(), sourceEntityAndDescFieldsDTO);
//            sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//            sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_RMR().get_RMRHead().get_DescFlexField());
//            sourceEntityAndDescFieldsDTO.set_SourceEntity(line.get_RMR().get_RMRHead());
//            dictionary.Add("UFIDA.U9.SM.RMR.RMRHead", sourceEntityAndDescFieldsDTO);
//            DescFlexSegments flexValue = flexmapping.GetFlexValue(dictionary, "UFIDA.U9.PM.Rcv.RcvLine");
//            if (flexValue != null)
//            {
//                targetLine.set_DescFlexSegments(flexValue.ToEntityData());
//            }
//        }

//        private List<RcvContactData> GetContacts(RMRLine line)
//        {
//            List<RcvContactData> list = new List<RcvContactData>();
//            foreach (RMRContact current in line.get_RMR().get_RMRContacts())
//            {
//                RcvContactData rcvContactData = new RcvContactData();
//                rcvContactData.set_LinkMan(new LinkManInfoData());
//                rcvContactData.get_LinkMan().set_EMail(current.get_LinkMan().get_EMail());
//                rcvContactData.get_LinkMan().set_Fax(current.get_LinkMan().get_Fax());
//                rcvContactData.get_LinkMan().set_FixPhone(current.get_LinkMan().get_FixPhone());
//                rcvContactData.get_LinkMan().set_Mobile(current.get_LinkMan().get_Mobile());
//                rcvContactData.get_LinkMan().set_Name(current.get_LinkMan().get_Name());
//                rcvContactData.get_LinkMan().set_OwnerType(current.get_LinkMan().get_OwnerType().get_Value());
//                list.Add(rcvContactData);
//            }
//            return list;
//        }

//        private List<RcvAddressData> GetAddresses(RMRLine line)
//        {
//            List<RcvAddressData> list = new List<RcvAddressData>();
//            foreach (RMRAddress current in line.get_RMR().get_RMRAddresses())
//            {
//                if (!(current.get_AddressLine() == null))
//                {
//                    RcvAddressData rcvAddressData = new RcvAddressData();
//                    rcvAddressData.set_AddressInfo(new AddressInfoData());
//                    rcvAddressData.get_AddressInfo().set_Address1(current.get_AddressLine().get_AddressA());
//                    rcvAddressData.get_AddressInfo().set_Address2(current.get_AddressLine().get_AddressB());
//                    rcvAddressData.get_AddressInfo().set_Address3(current.get_AddressLine().get_AddressC());
//                    rcvAddressData.get_AddressInfo().set_AddressType(current.get_AddressLine().get_AddressType().get_Value());
//                    if (current.get_AddressLine().get_CityKey() != null)
//                    {
//                        rcvAddressData.get_AddressInfo().set_City(current.get_AddressLine().get_CityKey().get_ID());
//                    }
//                    if (current.get_AddressLine().get_CountryKey() != null)
//                    {
//                        rcvAddressData.get_AddressInfo().set_Country(current.get_AddressLine().get_CountryKey().get_ID());
//                    }
//                    if (current.get_AddressLine().get_CountyKey() != null)
//                    {
//                        rcvAddressData.get_AddressInfo().set_County(current.get_AddressLine().get_CountyKey().get_ID());
//                    }
//                    if (current.get_AddressLine().get_PostcodeKey() != null)
//                    {
//                        rcvAddressData.get_AddressInfo().set_Postcode(current.get_AddressLine().get_PostcodeKey().get_ID());
//                    }
//                    if (current.get_AddressLine().get_ProvinceKey() != null)
//                    {
//                        rcvAddressData.get_AddressInfo().set_Province(current.get_AddressLine().get_ProvinceKey().get_ID());
//                    }
//                    list.Add(rcvAddressData);
//                }
//            }
//            return list;
//        }

//        private RMRLine.EntityList GetRMRLines(List<RMRLineAndQtyDTO> rmrLineAndQtys)
//        {
//            RMRLine.EntityList result;
//            if (rmrLineAndQtys == null || rmrLineAndQtys.Count == 0)
//            {
//                result = null;
//            }
//            else
//            {
//                string text = string.Empty;
//                OqlParam oqlParam = null;
//                string text2 = "rmrlineID";
//                string text3 = text2 + "__IDList";
//                DataTable dataTable = SysUtils.CreateIDTable();
//                foreach (RMRLineAndQtyDTO current in rmrLineAndQtys)
//                {
//                    dataTable.Rows.Add(new object[]
//                    {
//                        current.RMRLineID
//                    });
//                }
//                if (dataTable.Rows.Count > 0)
//                {
//                    if (dataTable.Rows.Count == 1)
//                    {
//                        text = string.Format("{0} = (@{1})", "ID", text2);
//                        oqlParam = new OqlParam(text2, dataTable.Rows[0][0]);
//                    }
//                    else
//                    {
//                        text = string.Format("{0} in (@{1})", "ID", text3);
//                        oqlParam = new OqlParam(text3, SysUtils.ToXml(dataTable));
//                    }
//                }
//                result = RMRLine.get_Finder().FindAll(text, new OqlParam[]
//                {
//                    oqlParam
//                });
//            }
//            return result;
//        }

//        private FlexFieldMapingDTO GetRMRToRcvFlexMapping()
//        {
//            List<string> list = new List<string>();
//            list.Add(RMR.EntityRes.get_BE_FullName());
//            list.Add(RMRHead.EntityRes.get_BE_FullName());
//            List<string> list2 = new List<string>();
//            list2.Add("UFIDA.U9.PM.Rcv.RcvLine");
//            list2.Add("UFIDA.U9.PM.Rcv.Receivement");
//            QueryPublicFlexFieldReferenceBP queryPublicFlexFieldReferenceBP = new QueryPublicFlexFieldReferenceBP();
//            queryPublicFlexFieldReferenceBP.set_SourceEntityFullNames(list);
//            queryPublicFlexFieldReferenceBP.set_DestinationEntityFullNames(list2);
//            return queryPublicFlexFieldReferenceBP.Do();
//        }

//        private List<string> GetSplitDocStr(List<string> optSplitDocStrs, FlexFieldMapingDTO flexdto)
//        {
//            List<string> list = new List<string>();
//            if (optSplitDocStrs != null && optSplitDocStrs.Count > 0)
//            {
//                if (optSplitDocStrs.Contains("RMR.RMRHead.Customer.Customer"))
//                {
//                    optSplitDocStrs.Remove("RMR.RMRHead.Customer.Customer");
//                }
//                if (optSplitDocStrs.Contains("RMR.RMRHead.Org"))
//                {
//                    optSplitDocStrs.Remove("RMR.RMRHead.Org");
//                }
//                list.AddRange(optSplitDocStrs);
//            }
//            list.Add("RMR.RMRHead.Customer.Customer");
//            list.Add("RMR.RMRHead.Org");
//            if (!optSplitDocStrs.Contains("RMR.AccountOrg"))
//            {
//                list.Add("RMR.AccountOrg");
//            }
//            if (!optSplitDocStrs.Contains("RMR.RMRHead.BalanceOrg"))
//            {
//                list.Add("RMR.RMRHead.BalanceOrg");
//            }
//            if (!optSplitDocStrs.Contains("RMR.TC"))
//            {
//                list.Add("RMR.TC");
//            }
//            if (!optSplitDocStrs.Contains("RMR.IsRelationOrg"))
//            {
//                list.Add("RMR.IsRelationOrg");
//            }
//            List<PublicFlexFieldReferenceDTO> splitFields = flexdto.GetSplitFields();
//            foreach (PublicFlexFieldReferenceDTO current in splitFields)
//            {
//                list.Add(SMTools.ConvertFlexFieldStringByRMA("RMR.DescFlexField." + current.get_SegFieldName()));
//            }
//            return list;
//        }

//        private object GetSrcDocCostInfo()
//        {
//            return null;
//        }

//        private List<RcvHeadDTOData> GetRcvHeadDTODatas()
//        {
//            return new List<RcvHeadDTOData>();
//        }

//        private Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo> GetDictionaryFromDTOes(RMRLine.EntityList rmrLines, Dictionary<long, RMRLineAndQtyDTO> rmrDtos)
//        {
//            Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo> dictionary = new Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo>();
//            foreach (RMRLine current in rmrLines)
//            {
//                this.DetachDocValidate(current);
//                DoubleQuantity transQty = new DoubleQuantity(current.get_ApplyQtyTU1(), current.get_ApplyQtyTU2(), current.get_UomInfoDTO1(), current.get_UomInfoDTO2());
//                dictionary.Add(current, new GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo
//                {
//                    TransQty = transQty,
//                    BusinessDate = rmrDtos[current.get_ID()].TransformDate,
//                    ArriveDate = rmrDtos[current.get_ID()].ArriveTime
//                });
//            }
//            return dictionary;
//        }

//        private void DetachDocValidate(RMRLine subLine)
//        {
//            if (subLine.get_RMR().get_Status() != RMRStatusEnum.get_Approved())
//            {
//                throw new SubLineRMRIsNotApprovedException();
//            }
//            if (subLine.get_RMR().get_RMRHead().get_IsHolded())
//            {
//                throw new RMRHeadIsHoldedExcepion();
//            }
//            if (subLine.get_RMR().get_RMRHead().get_Cancel().get_Canceled())
//            {
//                throw new RMRHeadIsCanceledExcepion();
//            }
//        }

//        private Dictionary<string, IList<RMRLine>> GetDetachDocs(Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo> diction, List<string> splitTerms)
//        {
//            Dictionary<string, IList<RMRLine>> result;
//            if (diction.Keys.Count > 0)
//            {
//                List<DocSplitor.CustomSplitAction<RMRLine>> list = new List<DocSplitor.CustomSplitAction<RMRLine>>();
//                List<RMRLine> list2 = new List<RMRLine>();
//                foreach (RMRLine current in diction.Keys)
//                {
//                    list2.Add(current);
//                }
//                if (splitTerms.Contains("ItemCategory"))
//                {
//                    splitTerms.Remove("ItemCategory");
//                }
//                Dictionary<string, IList<RMRLine>> rmrlinelist = DocSplitor.Split<RMRLine>(list2, splitTerms, list) as Dictionary<string, IList<RMRLine>>;
//                result = this.DiveByIsBusinessDate(rmrlinelist, diction);
//            }
//            else
//            {
//                result = null;
//            }
//            return result;
//        }

//        private Dictionary<string, IList<RMRLine>> DiveByIsBusinessDate(Dictionary<string, IList<RMRLine>> rmrlinelist, Dictionary<RMRLine, GetRcvDTOsByRMRImpementStrategy.TransferRMRInfo> diction)
//        {
//            Dictionary<string, IList<RMRLine>> result;
//            if (rmrlinelist == null || rmrlinelist.Count <= 0)
//            {
//                result = null;
//            }
//            else
//            {
//                Dictionary<string, IList<RMRLine>> dictionary = new Dictionary<string, IList<RMRLine>>();
//                foreach (string current in rmrlinelist.Keys)
//                {
//                    foreach (RMRLine current2 in rmrlinelist[current])
//                    {
//                        string key = current + diction[current2].BusinessDate.ToString();
//                        if (dictionary.ContainsKey(key))
//                        {
//                            dictionary[key].Add(current2);
//                        }
//                        else
//                        {
//                            dictionary.Add(key, new List<RMRLine>
//                            {
//                                current2
//                            });
//                        }
//                    }
//                }
//                result = dictionary;
//            }
//            return result;
//        }

//        private void SetDescFlexValues(RMRLine line, ref bool isFirst, ref bool[] isNullValue, FlexFieldMapingDTO flexMapping, RcvHeadDTOData dto)
//        {
//            if (flexMapping != null)
//            {
//                Dictionary<string, SourceEntityAndDescFieldsDTO> dictionary = new Dictionary<string, SourceEntityAndDescFieldsDTO>();
//                SourceEntityAndDescFieldsDTO sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//                sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_DescFlexField());
//                sourceEntityAndDescFieldsDTO.set_SourceEntity(line);
//                dictionary.Add(RMRLine.EntityRes.get_BE_FullName(), sourceEntityAndDescFieldsDTO);
//                sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//                sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_RMR().get_DescFlexField());
//                sourceEntityAndDescFieldsDTO.set_SourceEntity(line.get_RMR());
//                dictionary.Add(RMR.EntityRes.get_BE_FullName(), sourceEntityAndDescFieldsDTO);
//                sourceEntityAndDescFieldsDTO = new SourceEntityAndDescFieldsDTO();
//                sourceEntityAndDescFieldsDTO.set_DescFlexSegments(line.get_RMR().get_RMRHead().get_DescFlexField());
//                sourceEntityAndDescFieldsDTO.set_SourceEntity(line.get_RMR().get_RMRHead());
//                dictionary.Add("UFIDA.U9.SM.RMR.RMRHead", sourceEntityAndDescFieldsDTO);
//                DescFlexSegments flexValue = flexMapping.GetFlexValue(dictionary, "UFIDA.U9.PM.Rcv.Receivement");
//                if (flexValue != null)
//                {
//                    if (isFirst)
//                    {
//                        dto.get_RcvHead().set_DescFlexField(flexValue.ToEntityData());
//                        isFirst = false;
//                    }
//                    else if (dto.get_RcvHead().get_DescFlexField() != flexValue.ToEntityData())
//                    {
//                        DescFlexHelper.SetDescFlexItems(isNullValue, flexValue, line.get_RMR().get_DescFlexField());
//                    }
//                }
//            }
//        }

//        private RcvLineDTOData FillRcvLine(RMRLine subLine, RMRLineAndQtyDTO dealQty, FlexFieldMapingDTO flexmapping)
//        {
//            RcvLineDTOData rcvLineDTOData = new RcvLineDTOData();
//            rcvLineDTOData.set_RcvLine(new RcvLineData());
//            this.GetDescFlexSegmentsData(subLine, flexmapping, false, rcvLineDTOData.get_RcvLine());
//            rcvLineDTOData.get_RcvLine().set_RMAType(subLine.get_RMR().get_ApprovedType().get_Value());
//            rcvLineDTOData.get_RcvLine().set_CurrentOrg((subLine.get_RMR().get_ReceiveOrg() != null) ? subLine.get_RMR().get_ReceiveOrg().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_AccountOrg((subLine.get_RMR().get_AccountOrgKey() != null) ? subLine.get_RMR().get_AccountOrgKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_IsRelationCompany(subLine.get_RMR().get_IsRelationOrg());
//            rcvLineDTOData.get_RcvLine().set_MatchLayer(MatchGradeEnum.get_Way0().get_Value());
//            rcvLineDTOData.get_RcvLine().set_ConfirmAccording(-1L);
//            rcvLineDTOData.get_RcvLine().set_CustomerShipToSite((subLine.get_RMR().get_RMRHead().get_ShipToSite() == null) ? null : subLine.get_RMR().get_RMRHead().get_ShipToSite().ToEntityData());
//            rcvLineDTOData.get_RcvLine().set_OwnOrg((subLine.get_RMR().get_ShipperOrgKey() != null) ? subLine.get_RMR().get_ShipperOrgKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_PurOrg((subLine.get_RMR().get_OperatingOrgKey() != null) ? subLine.get_RMR().get_OperatingOrgKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_PurOper((subLine.get_RMR().get_SellerKey() != null) ? subLine.get_RMR().get_SellerKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_PurDept((subLine.get_RMR().get_SaleDeptKey() != null) ? subLine.get_RMR().get_SaleDeptKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_RequireOrg((subLine.get_RMR().get_RMRHead().get_OrgKey() != null) ? subLine.get_RMR().get_RMRHead().get_OrgKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_RequireMan((subLine.get_RMR().get_RMRHead().get_OperatorKey() != null) ? subLine.get_RMR().get_RMRHead().get_OperatorKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_RequireDept((subLine.get_RMR().get_RMRHead().get_DepartmentKey() != null) ? subLine.get_RMR().get_RMRHead().get_DepartmentKey().get_ID() : 0L);
//            if (subLine.get_RMR().get_SupplierInfo() != null)
//            {
//                rcvLineDTOData.get_RcvLine().set_ReturnSupplier(subLine.get_RMR().get_SupplierInfo().ToEntityData());
//            }
//            rcvLineDTOData.get_RcvLine().set_RcvProcedure(subLine.get_IsChecking() ? ReceiptModeEnum.get_ArriveQCConfirm().get_Value() : ReceiptModeEnum.get_ArriveConfirm().get_Value());
//            rcvLineDTOData.get_RcvLine().set_SplitFlag(0);
//            rcvLineDTOData.get_RcvLine().set_ItemInfo(subLine.get_ItemInfo().ToEntityData());
//            rcvLineDTOData.get_RcvLine().set_SupplierItemCode(subLine.get_RMR().get_CustomerItemCode());
//            rcvLineDTOData.get_RcvLine().set_SupplierItemName(subLine.get_RMR().get_CustomerItemName());
//            rcvLineDTOData.get_RcvLine().set_FAS((subLine.get_RMR().get_FasItem() != null && subLine.get_RMR().get_FasItem().get_ItemIDKey() != null && subLine.get_RMR().get_ItemType() == RMRItemTypeEnum.get_Whole()) ? subLine.get_RMR().get_FasItem().get_ItemIDKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_Mfc((subLine.get_ItemManufacturer() == null) ? null : subLine.get_ItemManufacturer().ToEntityData());
//            rcvLineDTOData.get_RcvLine().set_IsPresent(false);
//            rcvLineDTOData.get_RcvLine().set_SrcDocType(4);
//            rcvLineDTOData.get_RcvLine().set_SrcDoc(new SrcDocInfoData());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocNo(subLine.get_RMR().get_RMRHead().get_DocNo());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocVer(subLine.get_RMR().get_RMRHead().get_Version());
//            rcvLineDTOData.get_RcvLine().set_SrcSysVersion(subLine.get_SysVersion());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocLineNo(subLine.get_RMR().get_RMRNo());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocOrg(subLine.get_RMR().get_RMRHead().get_OrgKey().get_ID());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocBusiType(BusinessTypeEnum.get_SD_StandardSale().get_Value());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocDate(subLine.get_RMR().get_RMRHead().get_BusinessDate());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDoc(new BizEntityKeyData());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDoc().set_EntityID(subLine.get_RMR().get_RMRHeadKey().get_ID());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDoc().set_EntityType(subLine.get_RMR().get_RMRHeadKey().get_EntityType());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocLine(new BizEntityKeyData());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocLine().set_EntityID(subLine.get_RMRKey().get_ID());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocLine().set_EntityType(subLine.get_RMRKey().get_EntityType());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocSubLine(new BizEntityKeyData());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocSubLine().set_EntityID(subLine.get_ID());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocSubLine().set_EntityType(subLine.get_Key().get_EntityType());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocSubLineNo(subLine.get_DocLineNo());
//            rcvLineDTOData.get_RcvLine().set_RMATransType(subLine.get_RMR().get_RMRHead().get_DocumentType().get_RMRTransType().get_Value());
//            rcvLineDTOData.get_RcvLine().set_ConfigResult(subLine.get_RMR().get_OptionConfig());
//            rcvLineDTOData.get_RcvLine().set_ConfigResultOrg((subLine.get_RMR().get_BOMOrgKey() != null) ? subLine.get_RMR().get_BOMOrgKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_RCVToRMAType(this.GetRCVToRMAType(subLine));
//            rcvLineDTOData.get_RcvLine().set_RMAType(subLine.get_RMR().get_ApprovedType().get_Value());
//            rcvLineDTOData.get_RcvLine().set_RMATransType(subLine.get_RMR().get_RMRHead().get_DocumentType().get_RMRTransType().get_Value());
//            rcvLineDTOData.get_RcvLine().set_SeiBan((subLine.get_RMR().get_SeibanKey() == null) ? 0L : subLine.get_RMR().get_SeibanKey().get_ID());
//            rcvLineDTOData.get_RcvLine().set_SeiBanCode(subLine.get_RMR().get_SeibanCode());
//            rcvLineDTOData.get_RcvLine().set_IsEditSeiBan(!(subLine.get_RMR().get_SeibanKey() != null));
//            rcvLineDTOData.get_RcvLine().set_TaskOutput(subLine.get_RMR().get_SaleOrFYTask());
//            rcvLineDTOData.get_RcvLine().set_TBMainUOM((subLine.get_TU2Key() == null) ? 0L : subLine.get_TU2Key().get_ID());
//            rcvLineDTOData.get_RcvLine().set_TBSubUOM((subLine.get_TU2AssociateKey() == null) ? 0L : subLine.get_TU2AssociateKey().get_ID());
//            rcvLineDTOData.get_RcvLine().set_TBMainUOMToSubUOM(subLine.get_TU2ToTU2Associate());
//            rcvLineDTOData.get_RcvLine().set_FreeType(subLine.get_RMR().get_FreeType().get_Value());
//            rcvLineDTOData.get_RcvLine().set_FreeReason(subLine.get_RMR().get_FreeCourse().get_Value());
//            rcvLineDTOData.get_RcvLine().set_Memo(subLine.get_RMR().get_Remark());
//            rcvLineDTOData.get_RcvLine().set_IsKitWholeSet(subLine.get_ItemInfo().get_ItemID().get_ItemFormAttribute() == ItemTypeAttributeEnum.get_Kit());
//            rcvLineDTOData.get_RcvLine().set_KITRcvMode((subLine.get_ItemInfo().get_ItemID().get_ItemFormAttribute() == ItemTypeAttributeEnum.get_Kit()) ? KITShipModeEnum.get_Kit().get_Value() : -1);
//            rcvLineDTOData.get_RcvLine().set_TradeUOM((subLine.get_TUKey() != null) ? subLine.get_TUKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_TradeBaseUOM((subLine.get_TBUKey() != null) ? subLine.get_TBUKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_TUToTBURate(subLine.get_TUToTBURate());
//            rcvLineDTOData.get_RcvLine().set_PriceUOM((subLine.get_PUKey() != null) ? subLine.get_PUKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_PriceBaseUOM((subLine.get_PBUKey() != null) ? subLine.get_PBUKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_PUToPBURate(subLine.get_PUToPBURate());
//            rcvLineDTOData.get_RcvLine().set_TBUToPBURate(subLine.get_TBUToPBURate());
//            rcvLineDTOData.get_RcvLine().set_ArrivedTime(dealQty.ArriveTime);
//            if (subLine.get_RMR().get_SrcDocType() == RMRSrcDocTypeEnum.get_Shipment())
//            {
//                GetRcvDTOsByRMRImpementStrategy.CostInfoDTO srcDocCostInfo = this.GetSrcDocCostInfo(subLine);
//                rcvLineDTOData.get_RcvLine().set_IsZeroCost(srcDocCostInfo.IsZeroCost);
//                if (subLine.get_RMR().get_ItemType() != RMRItemTypeEnum.get_PartsOther())
//                {
//                    rcvLineDTOData.get_RcvLine().set_CostUOM(srcDocCostInfo.CU);
//                    rcvLineDTOData.get_RcvLine().set_CostBaseUOM(srcDocCostInfo.CBU);
//                    rcvLineDTOData.get_RcvLine().set_CUToCBURate(srcDocCostInfo.CUToCBURate);
//                    rcvLineDTOData.get_RcvLine().set_TBUToCBURate(srcDocCostInfo.TBUToCBURate);
//                    rcvLineDTOData.get_RcvLine().set_StoreUOM(srcDocCostInfo.IU);
//                    rcvLineDTOData.get_RcvLine().set_StoreBaseUOM(srcDocCostInfo.IBU);
//                    rcvLineDTOData.get_RcvLine().set_SUToSBURate(srcDocCostInfo.IUToIBURate);
//                    rcvLineDTOData.get_RcvLine().set_TBUToSBURate(srcDocCostInfo.TBUToIBURate);
//                }
//            }
//            if (subLine.get_ItemInfo().get_ItemID().get_IsDualQuantity())
//            {
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyPU((dealQty.RcvQty.get_Amount1() * subLine.get_TUToTBURate() + dealQty.RcvQty.get_Amount2()) * subLine.get_TBUToPBURate());
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyTBU(dealQty.RcvQty.get_Amount2());
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyTU(dealQty.RcvQty.get_Amount1());
//                rcvLineDTOData.get_RcvLine().set_PlanQtyTBU(dealQty.RcvQty.get_Amount2());
//                rcvLineDTOData.get_RcvLine().set_PlanQtyTU(dealQty.RcvQty.get_Amount1());
//            }
//            else
//            {
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyPU(dealQty.RcvQty.get_Amount1() * subLine.get_TBUToPBURate());
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyTBU(dealQty.RcvQty.get_Amount2());
//                rcvLineDTOData.get_RcvLine().set_ArriveQtyTU(dealQty.RcvQty.get_Amount1());
//                rcvLineDTOData.get_RcvLine().set_PlanQtyTBU(dealQty.RcvQty.get_Amount2());
//                rcvLineDTOData.get_RcvLine().set_PlanQtyTU(dealQty.RcvQty.get_Amount1());
//            }
//            rcvLineDTOData.get_RcvLine().set_Project((subLine.get_RMR().get_ProjectKey() != null) ? subLine.get_RMR().get_ProjectKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_Task((subLine.get_RMR().get_TaskKey() != null) ? subLine.get_RMR().get_TaskKey().get_ID() : 0L);
//            rcvLineDTOData.get_RcvLine().set_Status(0);
//            if (subLine.get_RMR().get_ItemType() != RMRItemTypeEnum.get_PartsOther() && (subLine.get_RMR().get_SrcDocType() == RMRSrcDocTypeEnum.get_SaleOrder() || subLine.get_RMR().get_SrcDocType() == RMRSrcDocTypeEnum.get_Shipment()) && subLine.get_RMR().get_SrcDoc() > 0L)
//            {
//                rcvLineDTOData.get_RcvLine().set_OutStoreActualCost(true);
//            }
//            else
//            {
//                rcvLineDTOData.get_RcvLine().set_OutStoreActualCost(false);
//            }
//            rcvLineDTOData.get_RcvLine().set_IsEvaluationChangeable(true);
//            rcvLineDTOData.get_RcvLine().set_IsSubCostChangeable(true);
//            if (subLine.get_ItemInfo().get_ItemID().get_InventoryInfo() != null && subLine.get_ItemInfo().get_ItemID().get_InventoryInfo().get_LotParam() != null)
//            {
//                GetLotInvalidDateSVProxy getLotInvalidDateSVProxy = new GetLotInvalidDateSVProxy();
//                List<LotInvalidDateDTOData> list = new List<LotInvalidDateDTOData>();
//                LotInvalidDateDTOData lotInvalidDateDTOData = new LotInvalidDateDTOData();
//                lotInvalidDateDTOData.set_Item(subLine.get_RMR().get_ItemInfo().get_ItemID().get_Key().get_ID());
//                lotInvalidDateDTOData.set_ItemCode(subLine.get_RMR().get_ItemInfo().get_ItemCode());
//                lotInvalidDateDTOData.set_LotInfo(new LotInfoData());
//                lotInvalidDateDTOData.get_LotInfo().set_LotCode(subLine.get_RMR().get_LotCode());
//                lotInvalidDateDTOData.set_Org(subLine.get_RMR().get_RMRHead().get_Org().get_Key().get_ID());
//                list.Add(lotInvalidDateDTOData);
//                getLotInvalidDateSVProxy.set_LotInvalidDateDTOList(list);
//                list = getLotInvalidDateSVProxy.Do();
//                if (subLine.get_RMR().get_LotKey() != null)
//                {
//                    rcvLineDTOData.get_RcvLine().set_InvLot(subLine.get_RMR().get_LotKey().get_ID());
//                }
//                rcvLineDTOData.get_RcvLine().set_InvLotCode(subLine.get_RMR().get_LotCode());
//                if (list != null && list.Count > 0 && list[0].get_LotInfo() != null)
//                {
//                    if (rcvLineDTOData.get_RcvLine().get_InvLot() <= 0L)
//                    {
//                        rcvLineDTOData.get_RcvLine().set_InvLot(list[0].get_LotInfo().get_LotMaster().get_EntityID());
//                    }
//                }
//                if (subLine.get_RMR().get_LotInvalidDate().Equals(DateTime.MinValue) || subLine.get_RMR().get_LotInvalidDate().Equals(DateTime.MaxValue))
//                {
//                    if (list != null && list.Count > 0 && list[0].get_LotInfo() != null && !list[0].get_LotInfo().get_DisabledDatetime().Equals(DateTime.MinValue))
//                    {
//                        rcvLineDTOData.get_RcvLine().set_InvDisabledTime(list[0].get_LotInfo().get_DisabledDatetime());
//                        rcvLineDTOData.get_RcvLine().set_InvLotValidDate(list[0].get_LotInfo().get_LotValidDate());
//                        rcvLineDTOData.get_RcvLine().set_InvLotEnableDate(rcvLineDTOData.get_RcvLine().get_InvDisabledTime().AddDays((double)(-(double)rcvLineDTOData.get_RcvLine().get_InvLotValidDate())));
//                    }
//                }
//                else
//                {
//                    rcvLineDTOData.get_RcvLine().set_InvDisabledTime(subLine.get_RMR().get_LotInvalidDate());
//                }
//                if (subLine.get_RMR().get_ItemInfo().get_ItemID().get_InventoryInfo() != null && subLine.get_RMR().get_ItemInfo().get_ItemID().get_InventoryInfo().get_LotParam() != null)
//                {
//                    if (subLine.get_RMR().get_ItemInfo().get_ItemID().get_InventoryInfo().get_LotParam().get_LotControlType().get_Value() == LotControlTypeEnum.get_AllProcedure().get_Value())
//                    {
//                        rcvLineDTOData.get_RcvLine().set_RcvLot(rcvLineDTOData.get_RcvLine().get_InvLot());
//                        rcvLineDTOData.get_RcvLine().set_RcvLotCode(rcvLineDTOData.get_RcvLine().get_InvLotCode());
//                    }
//                }
//            }
//            rcvLineDTOData.get_RcvLine().set_SO(subLine.get_RMR().get_SrcSO());
//            rcvLineDTOData.get_RcvLine().set_SONo(subLine.get_RMR().get_SrcSODoNo());
//            rcvLineDTOData.set_RcvAddrs(this.GetAddresses(subLine));
//            rcvLineDTOData.set_RcvContacts(this.GetContacts(subLine));
//            rcvLineDTOData.set_RcvAddrs(this.GetAddresses(subLine));
//            rcvLineDTOData.set_RcvContacts(this.GetContacts(subLine));
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocTransType(new BizEntityKeyData());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocTransType().set_EntityID(subLine.get_RMR().get_DocmentTypeKey().get_ID());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().get_SrcDocTransType().set_EntityType(subLine.get_RMR().get_DocmentTypeKey().get_EntityType());
//            rcvLineDTOData.get_RcvLine().get_SrcDoc().set_SrcDocTransTypeName(subLine.get_RMR().get_DocmentType().get_Name());
//            return rcvLineDTOData;
//        }

//        private int GetRCVToRMAType(RMRLine line)
//        {
//            int value;
//            if (((line.get_RMR().get_RMRHead().get_DocumentType().get_RMRTransType() == RMRTransTypeEnum.get_SampleRMR() || line.get_RMR().get_RMRHead().get_DocumentType().get_RMRTransType() == RMRTransTypeEnum.get_ExportRMR()) && (line.get_RMR().get_RMRHead().get_DocumentType().get_ReturnFlow() == RMRRetrunFlowEnum.get_RMRRCVRMA() && (line.get_RMR().get_ApprovedType() == RMRProcessTypeEnum.get_ExchangeRMA() || line.get_RMR().get_ApprovedType() == RMRProcessTypeEnum.get_RcvAndRtnRMA())) && line.get_ApplyType() == RMRApplyTypeEnum.get_NormalRMR()) || (line.get_RMR().get_RMRHead().get_DocumentType().get_RMRTransType() == RMRTransTypeEnum.get_NormalRMR() && line.get_RMR().get_RMRHead().get_DocumentType().get_ReturnFlow() == RMRRetrunFlowEnum.get_RMRRCVRMA() && (line.get_RMR().get_ApprovedType() == RMRProcessTypeEnum.get_ExchangeRMA() || line.get_RMR().get_ApprovedType() == RMRProcessTypeEnum.get_RcvAndRtnRMA() || line.get_RMR().get_ApprovedType() == RMRProcessTypeEnum.get_MaintainRMA())))
//            {
//                if (line.get_RMR().get_RMRHead().get_DocumentType().get_AutoToRMA())
//                {
//                    value = RmaReceiveModeEnum.get_NoBillConfirm().get_Value();
//                }
//                else
//                {
//                    value = RmaReceiveModeEnum.get_BillConfirm().get_Value();
//                }
//            }
//            else
//            {
//                value = RmaReceiveModeEnum.get_Empty().get_Value();
//            }
//            return value;
//        }

//        private DoubleQuantity GetDealQtyByRmrLine(long rmrLineId)
//        {
//            DoubleQuantity result;
//            foreach (RMRLineAndQtyDTO current in this.bpObj.RMRLineAndQtys)
//            {
//                if (rmrLineId == current.RMRLineID)
//                {
//                    result = current.RcvQty;
//                    return result;
//                }
//            }
//            result = null;
//            return result;
//        }
//    }
//}
