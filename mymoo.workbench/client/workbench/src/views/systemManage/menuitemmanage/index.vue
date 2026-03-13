<template>
	<div class="menuitemmanage">
		<div class="queryForm">
			<el-form ref="form" :model="form" label-width="100px">
				<el-row>
					<el-col :span="7">
						<el-form-item label="选择系统" prop="selectAppId">
							<el-select
								v-model="form.selectAppId"
								multiple
								collapse-tags
								style="margin-left: 20px;"
								placeholder="请选择">
								<el-option
								v-for="item in appIdSelectOptions"
								:key="item.key"
								:label="item.value"
								:value="item.key">
								</el-option>
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :span="8">
						<el-form-item class="queryBox">
							<el-button type="primary" @click="handleSerach">查询</el-button>
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
		</div>
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-button type="primary" style="marginBottom: 12px;float: right" @click="add">新增功能</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<el-table-column prop="appName" label="系统" width="80" align="center"> </el-table-column>
				<el-table-column prop="menuTitle" label="所属页面" width="80" align="center"> </el-table-column>
				<el-table-column prop="title" label="按钮名称" width="150" align="center"> </el-table-column>
				<el-table-column prop="path" label="路径" align="center"> </el-table-column>
				<el-table-column prop="description" label="描述" align="center"> </el-table-column>
				<el-table-column prop="controlPrivilege" label="控制权限" align="center" :formatter="row => (row.controlPrivilege ? '是' : '否')"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" width="100" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" width="120" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>
				<el-table-column prop="enableUser" label="启用人" width="100" align="center"> </el-table-column>
				<el-table-column prop="enableDate" label="启用时间" width="120" align="center" :formatter="row => row.enableDate && row.enableDate.slice(0, 10)"> </el-table-column>
				<el-table-column label="操作" align="center">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="enable(scope.row)" v-if="!scope.row.controlPrivilege">启用</el-button>
						<el-button type="text" size="small" @click="forbidden(scope.row)" v-if="scope.row.controlPrivilege">禁用</el-button>
						<el-button type="text" size="small" @click="edit(scope.row)" :disabled="scope.row.isPublish">编辑</el-button>
						<el-button type="text" size="small" @click="del(scope.row)" :disabled="scope.row.isPublish">删除</el-button>
					</template>
				</el-table-column>
			</el-table>
			<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="getMenuItemList" />
		</div>
		<!-- 新增/编辑modal -->
		<el-dialog :title="form.id > 0 ? '编辑功能' : '新增功能'" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-form :model="form" ref="form" :rules="rules" label-width="80px">
				<el-form-item label="所属系统" prop="appId">
					<el-select @change="setPagelist" v-model="form.appId" placeholder="请选择系统" :disabled="isEditRoot">
						<el-option v-for="item in appIdOptions" :key="item.key" :label="item.value" :value="item.key"> </el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="所属页面" prop="menuId">
					<el-select v-model="form.menuId" placeholder="请选择所属页面" :disabled="isEditRoot">
						<el-option v-for="item in parentOptions" :key="item.value" :label="item.label" :value="item.value"> </el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="按钮名称" prop="title">
					<el-input v-model="form.title" placeholder="请输入名称"></el-input>
				</el-form-item>
				<el-form-item label="路径" prop="path">
					<el-input v-model="form.path" placeholder="请输入路径"></el-input>
				</el-form-item>
				<el-form-item label="描述" prop="description">
					<el-input type="textarea" :rows="3" v-model="form.description" placeholder="请输入描述"></el-input>
				</el-form-item>
			</el-form>
			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="save('form')">保 存</el-button>
				<el-button @click="cancel">取 消</el-button>
			</span>
		</el-dialog>
	</div>
</template>

<script>
import { getMenuItemList,getMenuItemListByFilter,getMenuItemByAppId, getAppList, addMenuItem, editMenuItem, delMenuItem, getMenuByAppId, delMenuItemQuote, enableMenuItem, forbiddenMenuItem} from '@/api/system'
import { openLoading, closeLoading } from '@/utils/loading'
import elDragDialog from '@/directive/el-drag-dialog'
import Pagination from '@/components/Pagination'

export default {
	name: 'MenuItemManage',
	directives: { elDragDialog },
	components: { Pagination },
	data() {
		return {
			visible: false,
			total: 0,
			listQuery: {
				page: 1,
				limit: 20
			},
			form: {
				id: 0, // 大于0为编辑
				title: '',
				path: '',
				description: '',
				selectAppId: [],
				appId: [],
				controlPrivilege: true
			},
			tableData: [],
			rules: {
				appId: [{ required: true, message: '请选择所属系统', trigger: 'blur' }],
				menuId: [{ required: true, message: '请选择所属页面', trigger: 'blur' }],
				title: [{ required: true, message: '请输入按钮名称', trigger: 'blur' }],
				path: [{ required: true, message: '请输入路径', trigger: 'blur' }]
			},
			parentOptions: [],
			appIdOptions: [],
			appIdSelectOptions: [],
			isEditRoot: false
		}
	},

	created() {
		this.getMenuItemList()
		this.getAppList()
	},

	mounted() {},

	methods: {

		handleSerach() {
			this.listQuery.page = 1
			//this.getList()
			this.getMenuItemList()
		},

		getList() {
			var appIdStr = ''
			this.form.selectAppId.forEach(item => {
				appIdStr += item + ','
			})
			if(appIdStr.length > 0) {
				appIdStr = appIdStr.substr(0, appIdStr.length - 1);
			}
			openLoading()
			getMenuItemByAppId({ appId: appIdStr}).then(res => {
				if (res.code === 'success' && res.data) {
					this.tableData = res.data
				}
				closeLoading()
			})
		},

		setPagelist() {
			let appIdStr = this.form.appId
			getMenuByAppId({ appId: appIdStr,isShowChildMenu:true}).then(res => {
				if (res.code === 'success' && res.data) {
					let rootArr = []
					res.data
						.filter(item => item.parentId != 0)
						.forEach(item => {
							rootArr.push({
								value: item.id,
								label: item.title
							})
						})
					this.parentOptions = rootArr
				}
				closeLoading()
			})

		},

		getAppList() {
			var self = this
			openLoading()
			getAppList().then(res => {
				if (res.code === 'success' && res.data) {
					let appArr = []
					res.data.forEach(item => {
						appArr.push({ value: item.appName, key: item.appId})
					})
					self.appIdOptions = appArr
					self.appIdSelectOptions = appArr
				}
				closeLoading()
			})
		},

		getMenuItemList() {
			openLoading()
			// getMenuItemList().then(res => {
			// 	if (res.code === 'success' && res.data) {
			// 		this.tableData = res.data
			// 	}
			// 	closeLoading()
			// })
			

			const { page, limit } = this.listQuery
			var appIdStr = ''
			if(this.form.selectAppId != '' && this.form.selectAppId != undefined && this.form.selectAppId.length >0 ){
				this.form.selectAppId.forEach(item => {
				appIdStr += item + ','
			})
			if(appIdStr.length > 0) {
				appIdStr = appIdStr.substr(0, appIdStr.length - 1);
			}
			}
			
			getMenuItemListByFilter({ pageIndex: page, pageSize: limit, filter: {appId: appIdStr} }).then(res => {
				if (res.code === 'success' && res.data) {
					const { total, rows } = res.data
					this.total = total
					this.tableData = rows
				}
				closeLoading()
			})
		},

		add() {
			this.visible = true
			this.isEditRoot = false
			this.form = {
				id: 0,
				menuId: '',
				title: '',
				path: '',
				description: '',
				appId: []
			}
		},

		edit(row) {
			this.visible = true
			this.$refs['form'] && this.$refs['form'].clearValidate()
			this.setPagelist()
			this.form = row
			
			if (row.menuId === 0) {
				this.isEditRoot = true
			} else {
				this.isEditRoot = false
			}
		},

		del(row) {
			this.$confirm('你确定要删除吗?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			})
				.then(() => {
					openLoading()
					delMenuItem({ id: row.id }).then(res => {
						if (res.code === 'success') {
							if (res.data && res.data.isQuoted) {
								this.$confirm('该按钮包含引用,是否继续删除?', '提示', {
									confirmButtonText: '确定',
									cancelButtonText: '取消',
									type: 'warning'
								})
									.then(() => {
										delMenuItemQuote({ id: row.id }).then(res => {
											if (res.code === 'success') {
												this.$message({
													type: 'success',
													message: '删除成功!'
												})
												this.getMenuItemList()
											}
										})
									})
									.catch(() => {
										this.$message({
											type: 'info',
											message: '已取消删除'
										})
									})
							} else {
								this.$message({
									type: 'success',
									message: '删除成功!'
								})
								this.getMenuItemList()
							}
						}
						closeLoading()
					})
				})
				.catch(() => {
					this.$message({
						type: 'info',
						message: '已取消删除'
					})
				})
		},

		save(formName) {
			this.$refs[formName].validate(valid => {
				if (valid) {
					if (this.form.id > 0) {
						openLoading()
						editMenuItem({ ...this.form, sort: +this.form.sort }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getMenuItemList()
								this.visible = false
								this.$message({
									type: 'success',
									message: '编辑成功!'
								})
							}
							closeLoading()
						})
					} else {
						openLoading()
						addMenuItem({ ...this.form, sort: +this.form.sort }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getMenuItemList()
								this.visible = false
								this.$message({
									type: 'success',
									message: '添加成功!'
								})
							}
							closeLoading()
						})
					}
				} else {
					console.log('error submit!!')
					return false
				}
			})
		},

		forbidden(row) {
			openLoading()
			forbiddenMenuItem({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '禁用成功!'
					})
					this.getMenuItemList()
				}
				closeLoading()
			})
		},

		enable(row) {
			openLoading()
			enableMenuItem({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '启用成功!'
					})
					this.getMenuItemList()
				}
				closeLoading()
			})
		},

		cancel() {
			this.visible = false
		}
	}
}
</script>

<style lang="scss" scoped>
.menuitemmanage {
	margin-top: 8px;
	.queryForm {
		background-color: #fff;
		padding: 30px 20px 20px 0px;
	}
	.el-form {
		::v-deep .el-form-item {
			display: inline-block;
			input {
				width: 300px;
			}
			.el-date-editor {
				width: 420px;
			}
		}
		::v-deep .el-cascader {
			input {
				width: 380px;
			}
		}
		.queryBox {
			::v-deep .el-form-item__content {
				margin-left: 30px !important;
			}
		}
	}
	.tableBox {
		background-color: #fff;
		margin-top: 20px;
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
}
</style>
