
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.dll  E:\Portal\20210207\Portal\ApplicationLib
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.pdb  E:\Portal\20210207\Portal\ApplicationLib
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.dll  E:\Portal\20210207\Portal\ApplicationLib
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.pdb  E:\Portal\20210207\Portal\ApplicationLib

copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.dll  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.pdb  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.dll  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.pdb  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.dll  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.pdb  E:\Portal\20210207\Portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.ubfsvc  E:\Portal\20210207\Portal\ApplicationServer\Libs


copy .\BpImplement\U9Api.CustSV.IPullShipSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IPullShipWithLotSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IPullShipBatchSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateRMRSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IToRMRSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromRMRSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IShipApproveSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromPOSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IRcvApproveSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateRcvRptDocCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateLotSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IRcvRptDocApproveSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateMaterialDeliveryDocSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveMaterialDeliveryDocSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateMaterialInSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateTransferOutCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateTransferInCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateMiscRcvTransCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateMiscShipCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveTransferOutCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveTransferInCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveMaterialInSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveMiscShipCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IApproveMiscRcvTransCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IRMAPullShipCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateTransferInTwoStepCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromRtnSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IDeleteCustSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IUnApproveRcvSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IUnApproveShipSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IUnApproveTransferOutSV.svc  E:\Portal\20210207\Portal\CustService
copy .\BpImplement\U9Api.CustSV.IUpdateTransferOutSV.svc  E:\Portal\20210207\Portal\CustService

echo 请手工将该bat文件打开，将下面这段内容与E:\Portal\20210207\Portal\RestServices\web.config进行合并。
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IPullShipSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IPullShipWithLotSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IPullShipBatchSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateRMRSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IToRMRSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateRcvFromRMRSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IShipApproveSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateRcvFromPOSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IRcvApproveSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateRcvRptDocCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateLotSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IRcvRptDocApproveSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateMaterialDeliveryDocSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveMaterialDeliveryDocSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateMaterialInSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateTransferOutCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateTransferInCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateMiscRcvTransCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateMiscShipCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveTransferOutCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveTransferInCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveMaterialInSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveMiscShipCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IApproveMiscRcvTransCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IRMAPullShipCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateTransferInTwoStepCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.ICreateRcvFromRtnSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IDeleteCustSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IUnApproveRcvSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IUnApproveShipSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IUnApproveTransferOutSV" /> 
	</service>
	<service name="{type.FullName}Stub"  behaviorConfiguration="U9SrvTypeBehaviors">
		<endpoint address="" behaviorConfiguration="U9RestSrvBehaviors" binding="basicHttpBinding" contract="{type.Namespace.FullName}.IUpdateTransferOutSV" /> 
	</service>


pause
