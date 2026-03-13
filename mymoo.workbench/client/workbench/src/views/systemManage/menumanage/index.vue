<template>
	<div class="menumanage">
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
			<el-button type="primary" style="marginBottom: 12px;float: right" @click="add">新增菜单</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<!-- <el-table-column prop="id" label="菜单编码" width="80" align="center"> </el-table-column>
				<el-table-column prop="parentId" label="父级编码" width="80" align="center"> </el-table-column> -->
				<el-table-column prop="appName" label="系统" width="80" align="center"> </el-table-column>
				<el-table-column prop="parentName" label="父级菜单" width="80" align="center"> </el-table-column>
				<el-table-column prop="title" label="名称" width="150" align="center"> </el-table-column>
				<el-table-column prop="path" label="菜单路径" align="center"> </el-table-column>
				<el-table-column prop="url" label="url路径" align="center"> </el-table-column>
				<el-table-column prop="isPublish" label="是否发布" width="80" align="center" :formatter="row => (row.isPublish ? '是' : '否')"> </el-table-column>
				<el-table-column prop="description" label="描述" align="center"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" width="100" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" width="120" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>
				<el-table-column prop="publishUser" label="发布人" width="100" align="center"> </el-table-column>
				<el-table-column prop="publishDate" label="发布时间" width="120" align="center" :formatter="row => row.publishDate && row.publishDate.slice(0, 10)"> </el-table-column>
				<el-table-column label="操作" align="center">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="publish(scope.row)" v-if="!scope.row.isPublish">发布</el-button>
						<el-button type="text" size="small" @click="off(scope.row)" v-if="scope.row.isPublish">下架</el-button>
						<el-button type="text" size="small" @click="edit(scope.row)" :disabled="scope.row.isPublish">编辑</el-button>
						<el-button type="text" size="small" @click="del(scope.row)" :disabled="scope.row.isPublish || !!tableData.find(item => item.parentId === scope.row.id)">删除</el-button>
					</template>
				</el-table-column>
			</el-table>
			<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="getMenu" />
		</div>
		<!-- 新增/编辑modal -->
		<el-dialog :title="form.id > 0 ? '编辑菜单' : '新增菜单'" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-form :model="form" ref="form" :rules="rules" label-width="80px">
				<el-form-item label="所属系统" prop="appId">
					<el-select @change="setPagelist" v-model="form.appId" placeholder="请选择系统" :disabled="isEditRoot">
						<el-option v-for="item in appIdOptions" :key="item.key" :label="item.value" :value="item.key"> </el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="父级菜单" prop="parentId">
					<el-select v-model="form.parentId" placeholder="请选择父级菜单" :disabled="isEditRoot">
						<el-option v-for="item in parentOptions" :key="item.value" :label="item.label" :value="item.value"> </el-option>
					</el-select>
					<el-switch
					v-model="isShwoChildren"
 					 active-text="过滤子菜单"
					  @change="setPagelist"
					active-color="#13ce66"
					inactive-color="#ff4949">
					</el-switch>
				</el-form-item>
				<el-form-item label="名称" prop="title">
					<el-input v-model="form.title" placeholder="请输入名称"></el-input>
				</el-form-item>
				<el-form-item label="菜单路径" prop="path">
					<el-input v-model="form.path" placeholder="请输入菜单路径"></el-input>
				</el-form-item>
				<el-form-item label="图标" prop="icon">
					<el-input v-model="form.icon" placeholder="请输入图标"></el-input>
				</el-form-item>
				<el-form-item label="排序" prop="sort">
					<el-input v-model="form.sort" placeholder="请输入排序"></el-input>
				</el-form-item>
				<el-form-item label="组件路径" prop="component">
					<el-input v-model="form.component" placeholder="请输入组件路径"></el-input>
				</el-form-item>
				<el-form-item label="路由名称" prop="name" :rules="form.parentId > 0 ? rules.name : [{ required: false }]">
					<el-input v-model="form.name" placeholder="请输入路由名称"></el-input>
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
import { getMenu,getMenuByFilter,getMenuByAppId, getAppList, addMenu, editMenu, delMenu, delMenuQuote, publishMenu, offMenu } from '@/api/system'
import { openLoading, closeLoading } from '@/utils/loading'
import elDragDialog from '@/directive/el-drag-dialog'
import Pagination from '@/components/Pagination'

export default {
	name: 'MenuManage',
	directives: { elDragDialog },
	components: { Pagination },
	data() {
		return {
			visible: false,
			isShwoChildren:false,
			total: 0,
			listQuery: {
				page: 1,
				limit: 20
			},
			form: {
				id: 0, // 大于0为编辑
				parentId: '',
				title: '',
				path: '',
				// url: '',
				icon: '',
				sort: '',
				name: '',
				component: '',
				description: '',
				selectAppId: [],
				appId: []
			},
			tableData: [],
			rules: {
				appId: [{ required: true, message: '请选择系统', trigger: 'blur' }],
				parentId: [{ required: true, message: '请输入父级编码', trigger: 'blur' }],
				title: [{ required: true, message: '名称', trigger: 'blur' }],
				path: [{ required: true, message: '请输入菜单路径', trigger: 'blur' }],
				sort: [{ required: true, message: '请输入排序', trigger: 'blur' }],
				component: [{ required: true, message: '请输入组件路径', trigger: 'blur' }],
				name: [{ required: true, message: '请输入路由名称', trigger: 'blur' }]
			},
			parentOptions: [],
			appIdOptions: [],
			appIdSelectOptions: [],
			isEditRoot: false,
			AllMenuData: []
		}
	},

	created() {
		this.getMenu()
		this.getAppList()
	},

	mounted() {},

	methods: {
		
		addUrlPath(data) {
			var self = this
			return data.map(i => {
				if (i.parentId !== 0) {
					let parent = self.AllMenuData.find(j => i.parentId === j.id)
					if (parent) {
						i.url = parent.path + '/' + i.path
						i.parentName = parent.title
					}
				}
				return i
			})
		},

		handleSerach() {
			this.listQuery.page = 1
			//this.getList()
			this.getMenu()
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
			getMenuByAppId({ appId: appIdStr}).then(res => {
				if (res.code === 'success' && res.data) {
					this.tableData = this.addUrlPath(res.data)
					// 获取根菜单
					let rootArr = [
						{
							value: 0,
							label: '根菜单'
						}
					]
					res.data
					    .filter(item => item.parentId === 0)
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

		getMenu() {
			openLoading()
			var self = this
			getMenu().then(res => {
				if (res.code === 'success' && res.data) {
					self.AllMenuData = self.addUrlPath(res.data)
					const { page, limit } = self.listQuery
					var appIdStr = ''
					if(self.form.selectAppId != '' && self.form.selectAppId != undefined && self.form.selectAppId.length >0 ){
						self.form.selectAppId.forEach(item => {
							appIdStr += item + ','
						})
					}
					if(appIdStr.length > 0) {
						appIdStr = appIdStr.substr(0, appIdStr.length - 1);
					}
					getMenuByFilter({ pageIndex: page, pageSize: limit, filter: {appId: appIdStr} }).then(res => {
						if (res.code === 'success' && res.data) {
							const { total, rows } = res.data
							this.total = total
							this.tableData = this.addUrlPath(rows)
						}
						closeLoading()
					})
				}
			})

			
		},

		add() {
			this.visible = true
			this.isEditRoot = false
			this.form = {
				id: 0,
				parentId: '',
				title: '',
				path: '',
				// url: '',
				icon: '',
				sort: 10,
				name: '',
				component: '',
				description: '',
				appId: []
			}
		},
		setPagelist() {
			let appIdStr = this.form.appId
			getMenuByAppId({ appId: appIdStr,isShowChildMenu:!this.isShwoChildren}).then(res => {
				if (res.code === 'success' && res.data) {
					let rootArr = [
						{
							value: 0,
							label: '根菜单'
						}
					]
					res.data
						// .filter(item => item.parentId != 0)
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

		edit(row) {
			this.visible = true
			this.$refs['form'] && this.$refs['form'].clearValidate()
			this.setPagelist()
			this.form = row
			if (row.parentId === 0) {
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
					delMenu({ id: row.id }).then(res => {
						if (res.code === 'success') {
							if (res.data && res.data.isQuoted) {
								this.$confirm('该菜单包含引用,是否继续删除?', '提示', {
									confirmButtonText: '确定',
									cancelButtonText: '取消',
									type: 'warning'
								})
									.then(() => {
										delMenuQuote({ id: row.id }).then(res => {
											if (res.code === 'success') {
												this.$message({
													type: 'success',
													message: '删除成功!'
												})
												this.getMenu()
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
								this.getMenu()
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
						editMenu({ ...this.form, sort: +this.form.sort }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getMenu()
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
						addMenu({ ...this.form, sort: +this.form.sort }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getMenu()
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

		// 发布
		publish(row) {
			openLoading()
			publishMenu({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '发布成功!'
					})
					this.getMenu()
				}
				closeLoading()
			})
		},

		// 下架
		off(row) {
			openLoading()
			offMenu({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '下架成功!'
					})
					this.getMenu()
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
.menumanage {
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
	// ::v-deep .el-select {
	// 	width: 100%;
	// }
	::v-deep .el-dialog {
		transform: translateY(-50%);
		top: 50% !important;
		margin-top: 0 !important;
	}
	::v-deep .el-dialog__footer {
		width: 85%;
		margin: 0 auto;
	}
	// .el-form {
	// 	width: 85%;
	// 	margin: 0 auto;
	// }
}
</style>
