
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.dll  D:\yonyou\U9V50\portal\ApplicationLib
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.pdb  D:\yonyou\U9V50\portal\ApplicationLib
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.dll  D:\yonyou\U9V50\portal\ApplicationLib
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.pdb  D:\yonyou\U9V50\portal\ApplicationLib

copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.dll  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.Deploy.pdb  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.dll  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpAgent\bin\Debug\U9Api.CustSV.Agent.pdb  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.dll  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.pdb  D:\yonyou\U9V50\portal\ApplicationServer\Libs
copy .\BpImplement\bin\Debug\U9Api.CustSV.ubfsvc  D:\yonyou\U9V50\portal\ApplicationServer\Libs


copy .\BpImplement\U9Api.CustSV.IPullShipSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IPullShipWithLotSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IPullShipBatchSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateRMRSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IToRMRSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromRMRSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IShipApproveSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromPOSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IRcvApproveSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateRcvRptDocCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateLotSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IRcvRptDocApproveSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateMaterialDeliveryDocSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveMaterialDeliveryDocSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateMaterialInSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateTransferOutCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateTransferInCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateMiscRcvTransCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateMiscShipCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveTransferOutCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveTransferInCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveMaterialInSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveMiscShipCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IApproveMiscRcvTransCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IRMAPullShipCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateTransferInTwoStepCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.ICreateRcvFromRtnSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IDeleteCustSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IUnApproveRcvSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IUnApproveShipSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IUnApproveTransferOutSV.svc  D:\yonyou\U9V50\portal\Services
copy .\BpImplement\U9Api.CustSV.IUpdateTransferOutSV.svc  D:\yonyou\U9V50\portal\Services

echo 请手工将该bat文件打开，将下面这段内容与D:\yonyou\U9V50\portal\RestServices\web.config进行合并。
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