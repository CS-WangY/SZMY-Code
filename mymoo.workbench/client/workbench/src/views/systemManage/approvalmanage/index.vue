<template>
	<div class="approvalmanage">
		<!-- 列表展示 -->
		<div class="tableBox" v-if="isTemplate">
			<el-button type="primary" style="margin-bottom: 12px;float: right" @click="dialogVisible = true" size="medium">新增审批模板</el-button>
			<el-button type="text" size="small"  style="margin-right: 12px;float: right" @click="reloadAllCache()" v-if="envIsProduction">加载缓存</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<el-table-column prop="templateName" width="250" label="模板名称" align="center"> </el-table-column>
				<el-table-column prop="templateId" width="500" label="模板Id" align="center"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>				
				<el-table-column label="操作" align="center" width="250">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="showField(scope.row.approvalTemplateFields,scope.row.templateId)">查看字段</el-button>
						<el-button type="text" size="small" @click="showConfig(scope.row.templateId,scope.row.templateName)">查看配置</el-button>
						<el-button type="text" size="small" @click="reloadCache(scope.row.templateId)" v-if="envIsProduction">加载缓存</el-button>
					</template>
				</el-table-column>
			</el-table>
			<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 10]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="getApprovalTemplateList" />
		</div>	
		<div class="tableBox" v-if="isLookField">
			<el-button type="primary" style="margin-bottom: 12px;float: right" @click="goBack" size="medium">返回</el-button>
			<el-button type="primary" style="margin-bottom: 12px;margin-right: 12px;float: right" @click="getField" size="medium">获取字段</el-button>
			<!-- <el-button type="primary" style="marginBottom: 12px;margin-right: 12px;float: right" @click="saveField" size="medium">保存</el-button> -->
			<el-table :data="fieldData" style="width: 100%" border>
				<el-table-column prop="fieldId" width="200" label="字段Id" align="center"> </el-table-column>
				<el-table-column prop="fieldName" width="120" label="字段名称" align="center"> </el-table-column>
				<el-table-column prop="fieldNumber" width="140" label="字段编号" align="center">
					<!-- <template slot-scope="scope">
						<el-input style="font-size: 11px" v-model="scope.row.fieldNumber" :placeholder="scope.row.fieldNumber"></el-input>
					</template> -->
				</el-table-column>
				<el-table-column prop="fieldType" label="字段类型" align="center"> </el-table-column>
				<!-- <el-table-column prop="keywordSeq" label="keywordSeq" align="center"> </el-table-column> -->
				<el-table-column prop="selectOptionJson" width="400" label="下拉列表配置选项" align="center"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>				
				<el-table-column label="操作" align="center" width="250">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="modifyNumber(scope)">修改字段编号</el-button>
					</template>
				</el-table-column>
			</el-table>			
		</div>	
		<div class="tableBox" v-if="isLookConfig">
			<div style="font-size: 16px; font-weight: bold">{{templateName}}</div>
			<el-button type="primary" style="margin-bottom: 12px;float: right;" @click="goBack" size="medium">返回</el-button>
			<el-button type="primary" style="margin-bottom: 12px;float: right;margin-right: 20px;" @click="dialogConfig = true" size="medium">添加配置</el-button>
			<el-table :data="configData" style="width: 100%" border>
				<el-table-column prop="templateId" label="模板Id" align="center"> </el-table-column>
				<el-table-column prop="approvalMode" label="审批模式" align="center"> </el-table-column>
				<el-table-column prop="envCode" label="环境名称" align="center"> </el-table-column>
				<el-table-column prop="notifyType" label="抄送方式" align="center"> </el-table-column>
				<el-table-column prop="createUserCode" label="创建人" align="center"> </el-table-column>
				<el-table-column prop="createTime" label="创建时间" align="center" :formatter="row => row.createTime && row.createTime.slice(0, 10)"> </el-table-column>				
				<el-table-column label="操作" align="center" width="250">
					<template slot-scope="scope">	
						<el-button type="text" size="small" @click="showFlow(scope.row.auditFlowConfigDetails,scope.row.id)">审批流程</el-button>
						<!-- <el-button type="text" size="small" @click="modConfig(scope.row)">修改</el-button> -->
						<el-button type="text" size="small" @click="delConfig(scope.row.id)">删除</el-button>
					</template>
				</el-table-column>
			</el-table>			
		</div>
		
		<el-button type="primary"  v-if="isLookFlow" style="float: right; margin-right: 20px; margin-top: 31px;" @click="goBackConfig" size="medium">返回</el-button>
		<Approve v-if="isLookFlow" :user.sync="user" :templateId.sync="templateId" :templateName.sync="templateName" :auditFlowConfigId.sync="auditFlowConfigId" :flowData.sync="flowData"></Approve>

		<el-dialog title="修改字段编号" :visible.sync="dialogNumber" width="20%">
			<el-input v-model="fieldNumber" style="margin-bottom: 12px"></el-input>
			<el-button type="primary" @click="saveField">确定</el-button>
			<el-button @click="cancel">取消</el-button>
		</el-dialog>

		<el-dialog title="新增审批模板" :visible.sync="dialogVisible" width="30%">
			<el-form ref="form" :model="form" :rules="rules" label-width="120px">
				<el-form-item label="模板名称" prop="templateName">
					<el-input v-model="form.templateName"></el-input>
				</el-form-item>
				<el-form-item label="模板Id" prop="templateId">
					<el-input v-model="form.templateId"></el-input>
				</el-form-item> 
				<el-form-item>
					<el-button type="primary" @click="onSubmit('form')">立即创建</el-button>
					<el-button @click="cancel">取消</el-button>
				</el-form-item>
			</el-form>
		</el-dialog>

		<el-dialog :title="templateName" :visible.sync="dialogConfig" width="30%">
			<el-form ref="formConfig" :model="formConfig" :rules="rulesConfig" label-width="120px">
				<el-form-item label="环境名称" prop="envCode">
					<el-select v-model="formConfig.envCode" placeholder="请选择">
						<el-option v-for="(item,index) in envCodes" :key="index" :label="item.envName" :value="item.envCode"></el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="审批模式" prop="approvalMode">
					<el-select v-model="formConfig.approvalMode" placeholder="请选择">
						<el-option
							v-for="(item,index) in mode"
							:key="index"
							:label="item"
							:value="index">
						</el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="抄送方式" prop="notifyType">
					<el-select v-model="formConfig.notifyType" placeholder="请选择">
						<el-option
							v-for="(item,index) in notify"
							:key="index"
							:label="item"
							:value="index+1">
						</el-option>
					</el-select>
				</el-form-item>	
				<el-form-item>
					<el-button type="primary" @click="onSubmitConfig('formConfig')">确定</el-button>
					<el-button @click="cancel">取消</el-button>
				</el-form-item>
			</el-form>
		</el-dialog>
	</div>
</template>

<script>
import { updateTemplateFieldNumber, getField, delApprovalConfig, addApprovalConfig,getEnvCodes,getEnvIsProduction, getUserList, getApprovalTemplateList, getAuditFlowConfigList,reloadCache,reloadAllCache, addApprovalTemplate } from '@/api/system'
import { openLoading, closeLoading } from '@/utils/loading'
import Pagination from '@/components/Pagination'
import Approve from '@/components/Approve'

export default {
	name: 'ApprovalManage',
	components: { Pagination, Approve },
	data() {
		return {
			total: 0,
			listQuery: {
				page: 1,
				limit: 20
			},		
			tableData: [],
			fieldData: [],
			configData: [],
			flowData: [],
			isTemplate: true,
			isLookField: false,
			isLookConfig: false,
			isLookFlow: false,
			type: ["Text","Textarea","Date","Money","Number","File","Selector","Contact","RelatedApproval"],
			notify: ["提单时抄送","单据通过后抄送","提单和单据通过后抄送"],
			mode: ["通过接口指定审批人、抄送人","使用此模板在管理后台设置的审批流程，支持条件审批"],
			flowType: ["Approve","Copy","CreateUser","Condition"],
			flowSPType: ["Null","Or","And"],
			dialogVisible: false,
			dialogConfig: false,
			dialogNumber: false,
			form: {
				templateName: '',
				templateId: '',
			},
			rules:{
				templateName: [
					{ required: true, message: '请输入模板名称', trigger: 'blur' },
				],
				templateId: [
					{ required: true, message: '请输入模板Id', trigger: 'blur' },
				],
			},
			formConfig: {
				templateId: '',
				envCode:'',
				notifyType: '',
				approvalMode: 0
			},
			rulesConfig:{
				envCode: [
					{ required: true, message: '请选择环境变量', trigger: 'blur' },
				],
				notifyType: [
					{ required: true, message: '请选择抄送方式', trigger: 'blur' },
				],
				approvalMode: [
					{ required: true, message: '请选择审批模式', trigger: 'blur' },
				],
			},
			currentRowId: '', // 当前行id
			ele: '',
			templateId: 0,
			auditFlowConfigId: 0,
			user: [],
			templateName: '',
			add: [],
			envCodes: [],
			fieldNumber: "",
			fieldRow: "",
			envIsProduction: false, // 是否生产环境
		}
	},

	created() {
		this.getApprovalTemplateList();
		getUserList().then(res=>{
            if(res.code == 'success'){
                this.user = res.data
            }
        })
		getEnvCodes().then(res=>{
			if(res.code === 'success'){
				this.envCodes = res.data
			}
		})
		getEnvIsProduction().then(res=>{
			if(res.code === 'success'){
				this.envIsProduction = res.data
			}
		})
	},

	methods: {
		onSubmit(formName) {
			this.$refs[formName].validate((valid)=>{
				if (valid) {
					openLoading()
					addApprovalTemplate(this.form).then(res =>{
						if(res.code === 'success'){
							this.clearForm();
							this.$message({
								message: '新增成功',
								type: 'success'
						});
						this.getApprovalTemplateList();
					}
					closeLoading()
			})
				} else {
					return false;
				}
			})		
		},
		onSubmitConfig(formName) {
			this.$refs[formName].validate((valid)=>{
				if (valid) {
					openLoading()
					this.formConfig.templateId = this.templateId
					addApprovalConfig(this.formConfig).then(res =>{
						if(res.code === 'success'){
							this.clearFormConfig();
							this.$message({
								message: '新增成功',
								type: 'success'
						});
						this.getConfig(this.templateId)
					}
					closeLoading()
			})
				} else {
					return false;
				}
			})		
		},
		delConfig(id){
			this.$confirm('是否删除?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			}).then(() => {
				delApprovalConfig({id: id}).then(res=>{
					if(res.code === 'success'){
						this.$message({
							type: 'success',
							message: '删除成功!'
						});
						this.getConfig(this.templateId)
					}
				})
			}).catch(() => {

			});
		},
		modConfig(row){
			this.dialogConfig = true;
			this.formConfig.envCode = row.envCode;
			this.formConfig.notifyType = row.notifyType;
			this.formConfig.approvalMode = row.approvalMode
		},
		modifyNumber(scope){
			this.fieldNumber = scope.row.fieldNumber
			this.fieldRow = scope
			this.dialogNumber = true
		},
		cancel(){
			this.clearFormConfig();
			this.clearForm();
			this.fieldNumber = ""
			this.dialogNumber = false
		},
		clearForm(){
			this.dialogVisible = false;
			this.form.templateName = '';
			this.form.templateId = '';
		},
		clearFormConfig(){
			this.dialogConfig = false;
			this.formConfig.notifyType = '';
			this.formConfig.approvalMode = 0
		},
		getApprovalTemplateList() {
			openLoading()
			const { page, limit } = this.listQuery
			getApprovalTemplateList({ pageIndex: page, pageSize: limit, filter: {} }).then(res => {
				if (res.code === 'success' && res.data) {
					const { total, rows } = res.data
					this.total = total
					this.tableData = rows
				}
				closeLoading()
			})
		},
		showField(approvalTemplateFields,templateId){
			this.templateId = templateId;
			this.isLookField = true;
			this.isTemplate = false;
			approvalTemplateFields.forEach(element => {				
				if(typeof element.fieldType == "number"){
					element.fieldType = this.type[element.fieldType];
				}		
			});
			this.fieldData = approvalTemplateFields;
		},
		getField(){
			this.add = []
			this.$confirm('是否继续?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			}).then(() => {
				getField({templateId: this.templateId}).then(res =>{
				if(res.code == 'success'){
					var fields = res.data
					fields.template_content.controls.forEach(item =>{
						var flag = true
						for(var i =0; i < this.fieldData.length; i++){
							if(item.property.id == this.fieldData[i].fieldId){
								flag = false
								break
							}
						}
						if(flag){
							if(item.property.control != "Tips")
								this.add.push( { templateId: this.templateId, fieldId: item.property.id, fieldName: item.property.title[0].text, fieldType: item.property.control, selectOptionJson: item.config && item.config.selector? JSON.stringify(item.config.selector.options) : ''} )
						}		
					})
					this.add.forEach(item =>{
						this.fieldData.push(item)
						this.addField = true
					})
					if(this.add.length>0){
						this.$message.info("有"+this.add.length+"个新的字段")
					}else{
						this.$message.success("没有新的字段")
					}
					// console.log(fields.template_content.controls)
				}
			})
			}).catch(() => {
			       
			});
		},
		saveField(){
			if( !this.fieldNumber || this.fieldNumber == ''){
				this.$message.error("请输入字段编号")
				return
			}
			this.$confirm('是否保存?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			}).then(() => {
				this.fieldRow.row.fieldNumber = this.fieldNumber
				updateTemplateFieldNumber(this.fieldRow.row).then(res=>{
					if(res.code === 'success'){
						this.fieldData[this.fieldRow.$index].fieldNumber = this.fieldNumber;
						this.fieldData[this.fieldRow.$index].id = res.data.id;
						this.fieldData[this.fieldRow.$index].createDate = res.data.createDate;
						this.fieldData[this.fieldRow.$index].createUser = res.data.createUser;
						this.$message.success("保存成功");
						this.dialogNumber = false;
					}
				})
			}).catch(() => {
				
			});
		},
		showConfig(templateId,templateName){
			this.templateId = templateId
			this.templateName = templateName
			this.isLookConfig = true;
			this.isTemplate = false;
			this.getConfig(templateId);
		},
		reloadCache(templateId)
		{
			reloadCache({templateId: templateId}).then(res=>{
				if(res.code === 'success'){
					closeLoading();
					this.$message.success("加载缓存成功")
				}
			})
		},
		reloadAllCache()
		{
			reloadAllCache().then(res=>{
				if(res.code === 'success'){
					closeLoading();
					this.$message.success("加载缓存成功")
				}
			})
		},
		getConfig(templateId){
			getAuditFlowConfigList({templateId: templateId}).then(res=>{
				if(res.code === 'success' && res.data){
					this.configData = res.data;
					this.configData.forEach(element => {
						element.notifyType = this.notify[element.notifyType-1];
						element.approvalMode = this.mode[element.approvalMode];
					});				
					closeLoading();
				}
			})
		},
		showFlow(auditFlowConfigDetails,auditFlowConfigId){
			this.auditFlowConfigId = auditFlowConfigId
			this.isLookConfig = false;
			this.isLookFlow = true;
			auditFlowConfigDetails.forEach(element => {
				if(typeof element.type == "number"){
					element.type = this.flowType[element.type];
				}	
				if(typeof element.sptype == "number"){
					element.sptype = this.flowSPType[element.sptype];
				}
			})
			this.flowData = auditFlowConfigDetails;
		},
		goBack(){
			this.isLookField = false;
			this.isLookConfig = false;
			this.isTemplate = true;
			this.add = []
			this.getApprovalTemplateList();
		},
		goBackConfig(){
			this.getConfig(this.templateId);
			this.isLookConfig = true;
			this.isLookFlow = false;
		}
	}
}
</script>

<style lang="scss" scoped>
.approvalmanage {
	margin-top: 8px;
	background: #fff;
	.tableBox {
		// margin-top: 20px;
		padding: 20px;
	}
	::v-deep .el-dialog {
		transform: translateY(-50%);
		top: 50% !important;
		margin-top: 0 !important;
	}
	::v-deep .el-dialog__footer {
		width: 85%;
		margin: 0 auto;
	}
	.assignDialog {
		::v-deep .el-dialog {
			height: 80%;
			overflow: auto;
		}
		::v-deep .el-dialog__body {
			position: absolute;
			left: 0;
			top: 54px;
			bottom: 70px;
			right: 0;
			padding: 0;
			z-index: 1;
			overflow: hidden;
			overflow-y: auto;
		}
		::v-deep .el-dialog__footer {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
		}
	}
	.assignDialogBotton {
		::v-deep .el-dialog {
			height: 80%;
			overflow: auto;
		}
		::v-deep .el-dialog__body {
			position: absolute;
			left: 0;
			top: 54px;
			bottom: 70px;
			right: 0;
			padding: 0;
			z-index: 1;
			overflow: hidden;
			overflow-y: auto;
		}
		::v-deep .el-dialog__footer {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
		}
		::v-deep .el-tabs {
			height: 100%;
			border: none;
			//width: 60% !important;
		}
		::v-deep .el-tabs__content {
			height: 100%;
			//width: 40% !important;
			overflow: hidden;
			position: relative;
		}
	}
	.el-form,
	.el-tree {
		width: 100%;
		margin: 0 auto;
	}
	/* 弹出框滚动条 */
	/* 设置滚动条的样式 */
	/**解决了滚动条之间发生错位的现象 */
	::-webkit-scrollbar {
		width: 10px !important;
		height: 10px !important;
		border-radius: 5px;
	}
	::-webkit-scrollbar-thumb {
		border-radius: 5px;
		box-shadow: inset 0 0 6px rgba(0, 0, 0, 0.2);
		/* 滚动条的颜色 */
		background-color: #e4e4e4;
	}

	// 设置弹出框树滚动条
	.el-tabs {
		height: 100%;
		::v-deep .el-tabs__content {
			height: 100%;
			overflow: hidden;
			position: relative;
		}
	}

	.el-tab-pane {
		height: 100%;
		overflow: auto;
	}

	.el-dropdown-link {
		cursor: pointer;
		color: #409eff;
		padding-left: 14px;
	}
	.el-icon-arrow-down {
		font-size: 12px;
	}
	.pagination-container {
		margin-top: 0px;
	}
}
</style>
