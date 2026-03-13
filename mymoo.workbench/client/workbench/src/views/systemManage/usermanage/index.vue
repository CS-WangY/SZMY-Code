<template>
	<div class="usermanage">

  <el-container style="height: 100%; border: 1px solid #eee">
  <el-aside width="300px">
  <div class="deptTree" style="height:100%;width:100%">
<el-tree
  :data="treedata"
  node-key="id"
  :default-expanded-keys="checkedId"
   @node-click="treeClick"
   :expand-on-click-node="false"
  :props="defaultProps">
</el-tree>

  </div>
  </el-aside>
  
  <el-container>
    <el-main>
     	<!-- 查询表单 -->
		<div class="queryForm">
			<el-form ref="form" :model="form" label-width="100px" size="medium" inline>
				<el-form-item label="用户名" prop="userId">
					<!-- <UserSearch :user.sync="form.userId" ref="userRef" placeholder="请输入用户名" /> -->
					<el-input placeholder="请输入用户名" v-model="form.userName" >

					</el-input>
				</el-form-item>
				<!-- <el-form-item label="所属部门" prop="department">
					<el-cascader ref="cascader" :options="options" :props="props" v-model="form.department" collapse-tags></el-cascader>
				</el-form-item> -->
				
				<el-form-item label="岗位" prop="post">
					<el-select v-model="form.post" placeholder="请选择岗位" clearable>
						<el-option v-for="item in postOptions" :key="item.value" :label="item.label" :value="item.value"> </el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="职位" prop="position">
					<el-select v-model="form.position" placeholder="请选择职位" clearable>
						<el-option v-for="item in positionOptions" :key="item.key" :label="item.value" :value="item.value"> </el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="助理" prop="isAssistant">
					<el-radio-group v-model="form.isAssistant" size="mini">
						<el-radio-button v-for="item in isAssistantOptions" :key="item.value" :label="item.value">{{ item.label }}</el-radio-button>
					</el-radio-group>
				</el-form-item>
				<el-form-item class="queryBox">
					<el-button type="primary" @click="handleSerach">查询</el-button>
					<el-button type="primary" @click="batchAssignAssistant">批量分配助理</el-button>
				</el-form-item>
				<el-button type="primary" size="medium" :disabled="diabledBtn" :loading="diabledBtn" style="marginBottom: 12px;margin-left:100px;" @click="synchronizeUser">同步通讯录ERP</el-button>

				<el-button type="primary" size="medium" :disabled="diabledBtn" :loading="diabledBtn" style="marginBottom: 12px;" @click="synchronizeUserToCapp">MQ同步通讯录CAPP</el-button>
				<el-button type="primary" size="medium" :disabled="diabledBtn" :loading="diabledBtn" style="marginBottom: 12px;" @click="synchronizeUserToMes">MQ同步通讯录MES</el-button>
				<el-button type="primary" size="medium" :disabled="diabledBtn" :loading="diabledBtn" @click="reloadCache">加载缓存</el-button>
				<!--<el-button type="primary" size="medium" :disabled="diabledBtn" :loading="diabledBtn" style="marginBottom: 12px;" @click="methods_mytest">加载缓存</el-button> -->
			</el-form>
		</div>
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-table :data="tableData" style="width: 100%" border @selection-change="handleSelectionChange">
				<el-table-column type="selection" width="55" align="center"> </el-table-column>
				<el-table-column prop="userid" label="用户编码" width="180" align="center"> </el-table-column>
				<el-table-column prop="name" label="用户姓名"  width="180" align="center">
					<template slot="header">
						<div :style="{ textAlign: 'center' }">用户姓名</div>
					</template>
					<template slot-scope="{ row }">
						<span class="link-type" @click="handleClickName(row)">{{ row.name }}</span><span :class="row.isLeaderInDept ?'senior':''" >{{row.isLeaderInDept?'负责人':''}}</span>
					</template>
				</el-table-column>
				<el-table-column prop="gender" label="性别" width="100" align="center" :formatter="row => (row.gender === '1' ? '男' : '女')"> </el-table-column>
				<el-table-column prop="position" label="职位" width="180" align="center"> </el-table-column>
				<el-table-column prop="isAssistant" label="是否是助理" width="120" align="center" :formatter="row => (row.isAssistant ? '是' : '否')"> </el-table-column>
				<el-table-column prop="mobile" label="手机" align="center"> </el-table-column>
				<el-table-column prop="email" label="邮箱" align="center"> </el-table-column>
				<el-table-column label="操作" align="center">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="assign(scope.row)">分配角色</el-button>
						<el-button type="text" size="small" @click="assignPost(scope.row)">任岗</el-button>
						<el-button type="text" size="small" @click="assignAssistant(scope.row)">分配助理</el-button>
					</template>
				</el-table-column>
			</el-table>
			<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="getList" />
		</div>
    </el-main>
  </el-container>
</el-container>


	
		<!-- 分配modal -->
		<el-dialog :title="`分配角色-${currentRow.name}`" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-select v-model="assignValue" multiple placeholder="请选择角色">
				<el-option v-for="item in roleOptions" :key="item.value" :label="item.label" :value="item.value" :disabled="item.disabled"> </el-option>
			</el-select>
			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="save('form')">保 存</el-button>
				<el-button @click="cancel('form')">取 消</el-button>
			</span>
		</el-dialog>
		<!-- 分派岗位modal -->
		<el-dialog :title="`分配岗位-${currentRow.name}`" :visible.sync="assignPostVisible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-select v-model="assignPostValue" multiple placeholder="请选择岗位">
				<el-option v-for="item in postOptions" :key="item.value" :label="item.label" :value="item.value" :disabled="item.disabled"> </el-option>
			</el-select>
			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="savePost('form')">保 存</el-button>
				<el-button @click="cancel('assignPostForm')">取 消</el-button>
			</span>
		</el-dialog>
		<!-- 分配助理modal -->
		<el-dialog title="分配助理" :visible.sync="assignAssistantDialog" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<UserSearch :user.sync="assignAssistantValue" :isAssistant="true" ref="userRef" placeholder="请输入用户名" />
			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="assignAssistantSave">分配</el-button>
				<el-button type="primary" @click="cancel('assignAssistantForm')">取消</el-button>
			</span>
		</el-dialog>
		<!-- 查看助理modal -->
		<el-dialog
			title="详情"
			:visible.sync="showAssistantDialog"
			width="30%"
			:close-on-click-modal="false"
			v-el-drag-dialog
			v-if="showAssistantDialog"
			@close="handleAssistantDialogClose"
			class="showListClass"
		>
			<el-tabs v-model="activeName" @tab-click="handleTabClick">
				<el-tab-pane label="负责员工" name="user">
					<el-table :data="userTableData" border height="350" size="small">
						<el-table-column prop="userCode" label="用户编码" align="center"> </el-table-column>
						<el-table-column prop="userName" label="用户名称" align="center"> </el-table-column>
						<el-table-column
							prop="mymoooCompany"
							label="所属公司"
							align="center"
							:formatter="row => (row.mymoooCompany.toLowerCase() === 'dgweixinwork' ? '东莞蚂蚁' : row.mymoooCompany.toLowerCase() === 'weixinwork' ? '深圳蚂蚁' : '')"
						>
						</el-table-column>
						<el-table-column label="操作" align="center">
							<template slot-scope="scope">
								<el-button type="text" size="small" @click="delUser(scope.row)">删除</el-button>
							</template>
						</el-table-column>
					</el-table>
				</el-tab-pane>
				<el-tab-pane label="助理列表" name="assistant">
					<el-table :data="assistantTableData" border height="350" size="small">
						<el-table-column prop="assistantCode" label="助理编码" align="center"> </el-table-column>
						<el-table-column prop="assistantName" label="助理名称" align="center"> </el-table-column>
						<el-table-column
							prop="mymoooComapny"
							label="所属公司"
							align="center"
							:formatter="row => (row.mymoooCompany.toLowerCase() === 'dgweixinwork' ? '东莞蚂蚁' : row.mymoooCompany.toLowerCase() === 'weixinwork' ? '深圳蚂蚁' : '')"
						>
						</el-table-column>
						<el-table-column label="操作" align="center">
							<template slot-scope="scope">
								<el-button type="text" size="small" @click="delAssistant(scope.row)">删除</el-button>
							</template>
						</el-table-column>
					</el-table>
				</el-tab-pane>
				<el-tab-pane label="所属部门" name="deptment">
					<el-table :data="deptData" border height="350" size="small">
						<el-table-column prop="name" label="部门名称" align="center"> </el-table-column>
						<el-table-column :formatter="row=>(row.isLeaderInDepartment==1?'是':'否')" label="是否负责人" align="center"> </el-table-column>
						<el-table-column :formatter="row=>(row.isMainDepartMent==1?'是':'否')" label="是否主部门" align="center"> </el-table-column>
					</el-table>
				</el-tab-pane>
			</el-tabs>
		</el-dialog>
        <!-- 同步通讯录 -->
		<el-dialog :visible.sync="syncVisible" :show-close="false"  :close-on-click-modal="false"
      :close-on-press-escape="false" width="40%" v-el-drag-dialog>
			<span slot="title">
				<div style="line-height: 24px;font-size: 18px;color: #303133;">
					<span>同步通讯录</span>
				</div>
			</span>
				<div>
					<el-progress :text-inside="true" :stroke-width="14" :percentage="percentage" style="margin-bottom: 8px" :color="progressColors"></el-progress>
				</div>
			<!-- <span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="syscuser"  :disabled="diabledBtn" :loading="diabledBtn">同步通讯录</el-button>
				<el-button >取 消</el-button>
			</span> -->
		</el-dialog>
	</div>
</template>

<script>
import {
	GetWeiXinWorkDepartmentDetials,
	getDepartmentUserList,
	getRoleList,
	getUserRoles,
	assignRole,
	getPositionList,
	getUserPosition,
	assignPostion,
	assignAssistant,
	getUserAssistant,
	deleteAssistant,
	getDeptByUser,
	getWeiXinPositionList
} from '@/api/system'
import { synchronizeToCapp,synchronizeToMes,reloadCache, mytest} from '@/api/user'
import Pagination from '@/components/Pagination'
import UserSearch from '@/components/UserSearch'
import { openLoading, closeLoading } from '@/utils/loading'
import { newListToTree } from '@/utils'
import elDragDialog from '@/directive/el-drag-dialog'

export default {
	name: 'UserManage',
	components: { Pagination, UserSearch },
	directives: { elDragDialog },
	data() {
		return {
			percentage: 0,
			diabledBtn: false,
			progressColors: [
				{ color: '#f56c6c', percentage: 20 },
				{ color: '#e6a23c', percentage: 40 },
				{ color: '#5cb87a', percentage: 60 },
				{ color: '#1989fa', percentage: 80 },
				{ color: '#67C23A', percentage: 100 }
			],
			visible: false,
			syncVisible:false,
			assignPostVisible: false,
			assignAssistantDialog: false,
			showAssistantDialog: false,
			checkedId:[],//默认要展开的节点id
			total: 0,
			deptData:null,
			listQuery: {
				page: 1,
				limit: 20
			},
			form: {
				userId: null,
				userName:null,
				isAssistant: null,
				department: [],
				post: ''
			},
			selectedDeptId:[], //当前选中的部门Id
			props: { multiple: true, checkStrictly: true, value: 'id', label: 'name' },
			options: [],
			tableData: [],
			userData: [],
			roleOptions: [],
			assignValue: [],
			currentRow: {},
			appIdNameList: [],
			postOptions: [], // 岗位option
			assignPostValue: [],
			treedata:[],
			isAssistantOptions: [
				{
					value: null,
					label: '所有'
				},
				{
					value: true,
					label: '是'
				},
				{
					value: false,
					label: '否'
				}
			],
			selectedArr: [],
			assignAssistantValue: null, // 分配助理
			batchFlag: false, // 批量分配标记
			assistantTableData: [], // 负责助理列表
			userTableData: [], // 负责员工列表
			activeName: 'user', // 默认激活员工列表
			positionOptions: [],
			defaultProps: {
          children: 'children',
          label: 'name'
        }
		}
	},

	async created() {
		 this.GetWeiXinWorkDepartmentDetials(this.getList)
		 this.getRoleList()
		  this.getPositionList()
		  this.getWeiXinPositionList()
	},

	mounted() {},

	methods: {
		// 获取部门详情
		GetWeiXinWorkDepartmentDetials(cb) {
			openLoading()
			var self = this
			GetWeiXinWorkDepartmentDetials().then(res => {
				if (res.code === 'success' && res.data) {
					var rootNode = newListToTree(res.data[0], 0, 'parentId',res.data[0][0].appId)
					for (var i = 1; i < res.data.length; i++) {
						var otherNode = newListToTree(res.data[i], 0, 'parentId',res.data[i][0].appId)
						rootNode.push(otherNode[0])
					}
					// this.options = rootNode
					this.treedata=rootNode
				    let re= rootNode.find(it=>it.appId=="weixinwork")
					this.selectedDeptId.push(re.id)
				    this.checkedId.push(re.id)
					//this.options = listToTree(res.data, 'parentid')
					cb && cb()
				}

				closeLoading()
			})
		},
		
		// 获取角色列表
		getRoleList() {
			openLoading()
			getRoleList().then(res => {
				if (res.code === 'success' && res.data) {
					let roleArr = []
					res.data.forEach(item => {
						roleArr.push({ value: item.id, label: item.name, disabled: item.isForbidden })
					})
					this.roleOptions = roleArr
				}
				closeLoading()
			})
		},

		handleSerach() {
			this.listQuery.page = 1
			this.getList()
		},
		treeClick(data,node){
			this.selectedDeptId=[]
			this.selectedDeptId.push(data.id)
			this.getList()
			},
		syscuser(){
			const signalR = this.signalR
			var that = this
			this.connection = new signalR.HubConnectionBuilder()
			.withUrl('/chatHub', {
				skipNegotiation: true,
				transport: signalR.HttpTransportType.WebSockets
			})
			.configureLogging(signalR.LogLevel.Information)
			.build()
			this.connection.on('InputData', function(message) {
							let res = JSON.parse(message)
							that.percentage = res.progress
							if (res.progress===100 && res.isEnd) {
								that.diabledBtn = false 
								that.syncVisible=false
								if(res.code=='success')
								{
									 that.reloadCache()
									that.$message.success('同步成功!')
								}else
								{
									that.$message.info(res.errorMessage)
								}
							}
						})
			let params = {
				type: 'adressBook'
			}
			this.connection
				.start()
				.then(() => {
					this.connection.invoke('InputData', JSON.stringify(params)).catch(function (err) {
						return console.error(err)
					})
				})
				.catch(() => {
					that.diabledBtn = false
				})
		},
		synchronizeUser() {
			this.syncVisible=true
			this.diabledBtn=true
			this.percentage=0
			this.syscuser()
			// openLoading()
			// synchronize().then(res => {
			// 	if (res.code === 'success') {
			// 		this.$message.success('同步成功!')
			// 		closeLoading()
			// 	}
			// })
		},
		synchronizeUserToCapp() {
			openLoading()
			synchronizeToCapp().then(res => {
			 	if (res.code === 'success') {
			 		this.$message.success('同步成功!')
			 		closeLoading()
			 	}
			})
		},
		methods_mytest(){
			openLoading()
			mytest().then(res=>{
				if (res.code === 'success') {
			 		this.$message.success('加载缓存成功!')
			 		closeLoading()
			 	}
			});
		},
		synchronizeUserToMes() {
			openLoading()
			synchronizeToMes().then(res => {
			 	if (res.code === 'success') {
			 		this.$message.success('同步成功!')
			 		closeLoading()
			 	}
			})
		},
		
		reloadCache(){
			openLoading()
			reloadCache().then(res => {
			 	if (res.code === 'success') {
			 		this.$message.success('加载缓存成功!')
			 		closeLoading()
			 	}
			})
		},
		getList() {
			// var checkedNodesData = this.$refs['cascader'].getCheckedNodes()
			//默认当前一次只查一个公司的成员
			// checkedNodesData.forEach(item => {
			// 	mymoooCompany.push(item.data.appId)
			// })
			const { page, limit } = this.listQuery
			let departmentIdList = []
			this.form.department.forEach(item => {
				departmentIdList.push(item[item.length - 1])
			})
			openLoading()
			getDepartmentUserList({
				pageIndex: page,
				pageSize: limit,
				filter: {
					userName: this.form.userName,
					departmentIdList,
					fetchChild: 1,
					departmentIdList:this.selectedDeptId,
					isAssistant: this.form.isAssistant,
					post: !this.form.post ? 0 : this.form.post,
					position: this.form.position
				}
			}).then(res => {
				if (res.code === 'success' && res.data) {
					const { total, rows } = res.data
					this.total = total
					this.tableData = rows
				}
				closeLoading()
			})
		},

		getDepartmentUserList() {
			const { page, limit } = this.listQuery
			let departmentIdList = []
			this.form.department.forEach(item => {
				departmentIdList.push(item[item.length - 1])
			})
			openLoading()
			getDepartmentUserList({ pageIndex: page, pageSize: limit, filter: { userId: this.form.userId ?? 0, departmentIdList, fetchChild: 1 } }).then(res => {
				if (res.code === 'success' && res.data) {
					const { total, rows } = res.data
					this.total = total
					this.tableData = rows
				}
				closeLoading()
			})
		},

		assign(row) {
			this.visible = true
			this.currentRow = row
			this.assignValue = []
			getUserRoles({ userId: row.id }).then(res => {
				if (res.code === 'success' && res.data) {
					let selectedArr = []
					res.data.forEach(item => {
						selectedArr.push(item.roleId)
					})
					this.assignValue = selectedArr
				}
			})
		},

		// 分配保存
		save() {
			openLoading()
			assignRole({ roleId: this.assignValue, userId: this.currentRow.id }).then(res => {
				if (res.code === 'success') {
					this.$message.success('分配成功!')
					this.visible = false
				}
				closeLoading()
			})
		},

		// modal 取消
		cancel(formName) {
			switch (formName) {
				case 'form':
					this.visible = false
					break
				case 'assignPostForm':
					this.assignPostVisible = false
					break
				case 'assignAssistantForm':
					this.assignAssistantDialog = false
					break

				default:
					break
			}
		},

		// 获取岗位列表
		getPositionList() {
			return new Promise((resolve, reject) => {
				getPositionList().then(res => {
					if (res.code === 'success' && res.data) {
						let postArr = []
						res.data.forEach(item => {
							postArr.push({ value: item.id, label: item.name, disabled: item.isForbidden })
						})
						this.postOptions = postArr
						resolve()
					}
				})
			})
		},

		// 获取用户岗位表
		getUserPosition(row) {
			getUserPosition({ userId: row.id }).then(res => {
				if (res.code === 'success' && res.data) {
					let selectedArr = []
					res.data.forEach(item => {
						selectedArr.push(item.positionId)
					})
					this.assignPostValue = selectedArr
				}
			})
		},

		// 分派岗位
		assignPost(row) {
			this.assignPostVisible = true
			this.currentRow = row
			this.assignPostValue = []
			this.getUserPosition(row)
		},

		// 分派岗位保存
		savePost() {
			if (this.assignPostValue.length <= 0) {
				this.$message.warning('请选择要分派的岗位')
				return
			}
			openLoading()
			assignPostion({ positionId: this.assignPostValue, userId: this.currentRow.id }).then(res => {
				if (res.code === 'success') {
					this.$message.success('分派成功!')
					this.assignPostVisible = false
					this.handleSerach()
				}
				closeLoading()
			})
		},

		// 多选分配助理
		handleSelectionChange(val) {
			this.selectedArr = val.map(item => item.id)
		},

		// 分配助理
		assignAssistant(row) {
			this.assignAssistantDialog = true
			this.currentRow = row
			this.assignAssistantValue = null
			this.batchFlag = false
		},

		// 确认分配助理
		assignAssistantSave() {
			if (!this.assignAssistantValue) {
				this.$message.warning('请选择要分配的助理')
				return
			}
			let userId = this.batchFlag ? this.selectedArr : [this.currentRow.id]
			openLoading()
			assignAssistant({ assistantId: this.assignAssistantValue ?? 0, userId }).then(res => {
				if (res.code === 'success') {
					this.$message.success('分配成功!')
					this.assignAssistantDialog = false
				}
				closeLoading()
			})
		},

		// 批量分配助理
		batchAssignAssistant() {
			if (this.selectedArr.length === 0) {
				this.$message.warning('请选择要分配助理的用户')
				return
			}
			this.assignAssistantDialog = true
			this.assignAssistantValue = null
			this.batchFlag = true
		},

		handleClickName(row) {
			this.showAssistantDialog = true
			this.currentRow = row
			this.activeName = 'user'
			this.getUserAssistant(row)
		},

		// 获取员工助理列表
		getUserAssistant(row) {
			let isAssistant, tableData
			if (this.activeName === 'user') {
				isAssistant = false
				tableData = 'userTableData'
			} else if (this.activeName === 'assistant') {
				isAssistant = true
				tableData = 'assistantTableData'
			}
			getUserAssistant({ userId: row.id, isAssistant }).then(res => {
				if (res.code === 'success' && res.data) {
					this[tableData] = res.data
				}
			})
		},
		getDeptByUser(row){
           getDeptByUser({userId:row.id}).then(res=>{
			 if(res.code==='success' && res.data)
			 {
				this.deptData =res.data
			 }
		   })
		},

		// 点击tab
		handleTabClick(tab, event) {
			if(tab.name!='deptment')
			{
				this.getUserAssistant(this.currentRow)
			}else
			{
				this.getDeptByUser(this.currentRow)
			}
		},

		// 给用户删除助理
		delAssistant(row) {
			deleteAssistant({ userId: this.currentRow.id, assistantId: row.assistantId }).then(res => {
				if (res.code === 'success') {
					this.$message.success('删除成功')
					this.getUserAssistant(this.currentRow)
				}
			})
		},

		//给助理删除用户
		delUser(row) {
			deleteAssistant({ userId: row.userId, assistantId: this.currentRow.id }).then(res => {
				if (res.code === 'success') {
					this.$message.success('删除成功')
					this.getUserAssistant(this.currentRow)
				}
			})
		},

		// 助理/用户列表模态框关闭处理
		handleAssistantDialogClose() {
			this.userTableData = []
			this.assistantTableData = []
		},
		// 获取微信职位列表
		getWeiXinPositionList() {
			getWeiXinPositionList().then(res => {
				if (res.code === 'success' && res.data) {
					this.positionOptions = res.data
				}
			})
		}
	}
}
</script>

<style lang="scss" scoped>
.senior{
	display: inline-block;
	margin-left: 1em;
	 color: #afafaf;
	  border:1px solid #afafaf;
	   border-radius: 5px; 
	   font-size: 11px;
}
.usermanage {
	margin-top: 8px;
	.queryForm {
		background-color: #fff;
		padding: 30px 20px 20px 0px;
		border-radius: 6px;
	}
	.el-aside{
		padding: 0;
		padding-top: 20px;
		padding-left: 20px;
	}
	.el-form {
		::v-deep .el-form-item {
			input {
				width: 300px;
			}
		}
		.queryBox {
			::v-deep .el-form-item__content {
				margin-left: 30px !important;
			}
		}
	}
	.deptTree{
		padding: 20px;
		background-color: #fff;
		border-radius: 6px;
	}
	.tableBox {
		background-color: #fff;
		margin-top: 8px;
		padding: 20px;
		border-radius: 6px;
	}
	::v-deep .el-dialog {
		margin-top: 30vh !important;
		.el-select {
			width: 60%;
		}
	}
	::v-deep .el-dialog__footer {
		margin: 0 auto;
	}
	.showListClass {
		::v-deep.el-dialog__body {
			padding: 15px 20px 30px 20px;
		}
	}
	// 滚动条的宽度
	::v-deep .el-table__body-wrapper::-webkit-scrollbar {
		width: 6px; // 横向滚动条
		height: 6px; // 纵向滚动条 必写
	}
	// 滚动条的滑块
	::v-deep .el-table__body-wrapper::-webkit-scrollbar-thumb {
		background-color: #ddd;
		border-radius: 3px;
	}
}
</style>
