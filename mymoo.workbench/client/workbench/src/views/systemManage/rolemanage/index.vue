<template>
	<div class="rolemanage">
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-button type="primary" style="marginBottom: 12px;float: right" @click="add" size="medium">新增角色</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<el-table-column prop="code" label="角色编码" align="center"> </el-table-column>
				<el-table-column prop="name" label="角色名称" align="center"> </el-table-column>
				<el-table-column prop="isForbidden" label="是否禁用" align="center" :formatter="row => (row.isForbidden ? '是' : '否')"> </el-table-column>
				<el-table-column prop="isAdmin" label="是否是管理员" align="center" :formatter="row => (row.isAdmin ? '是' : '否')"> </el-table-column>
				<el-table-column prop="description" label="描述" align="center"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>
				<el-table-column prop="forbiddenUser" label="禁用人" align="center"> </el-table-column>
				<el-table-column prop="forbiddenDate" label="禁用时间" align="center" :formatter="row => row.forbiddenDate && row.forbiddenDate.slice(0, 10)"> </el-table-column>
				<el-table-column label="操作" align="center" width="250">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="assign(scope.row)" :disabled="scope.row.isForbidden">菜单</el-button>
						<el-button type="text" size="small" @click="assignBotton(scope.row)" :disabled="scope.row.isForbidden">功能</el-button>
						<el-button type="text" size="small" @click="setAdmin(scope.row)" :disabled="scope.row.isAdmin">设为管理员</el-button>
						<el-dropdown>
							<!-- <el-button size="mini" type="primary">
								更多<i class="el-icon-arrow-down el-icon--right"></i>
							</el-button> -->
							<span class="el-dropdown-link"> 更多<i class="el-icon-arrow-down el-icon--right"></i> </span>
							<el-dropdown-menu slot="dropdown">
								<el-dropdown-item><el-button type="text" size="small" @click="edit(scope.row)" :disabled="!scope.row.isForbidden">编辑</el-button></el-dropdown-item>
								<el-dropdown-item><el-button type="text" size="small" @click="del(scope.row)" :disabled="!scope.row.isForbidden">删除</el-button></el-dropdown-item>
								<el-dropdown-item><el-button type="text" size="small" @click="forbidden(scope.row)" v-if="!scope.row.isForbidden">禁用</el-button></el-dropdown-item>
								<el-dropdown-item><el-button type="text" size="small" @click="enable(scope.row)" v-if="scope.row.isForbidden">启用</el-button></el-dropdown-item>
							</el-dropdown-menu>
						</el-dropdown>
					</template>
				</el-table-column>
			</el-table>
			<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="getRoleList" />
		</div>
		<!-- 新增/编辑modal -->
		<el-dialog :title="form.id > 0 ? '编辑角色' : '新增角色'" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-form :model="form" ref="form" :rules="rules" label-width="80px">
				<el-form-item label="角色编码" prop="code">
					<el-input v-model="form.code" placeholder="请输入角色编码"></el-input>
				</el-form-item>
				<el-form-item label="角色名称" prop="name">
					<el-input v-model="form.name" placeholder="请输入角色名称"></el-input>
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
		<!-- 分配菜单modal -->
		<el-dialog title="分配菜单" :visible.sync="assignVisible" width="50%" class="assignDialog">
			<el-tabs v-model="editableTabsValue" type="border-card" @tab-click="handleClick" editdisable :tab-position="tabPosition">
				<el-tab-pane :key="item.key" v-for="item in appIdOptions"   :label="item.value"  :name="item.key">
					<el-tree :data="menuData" ref="tree" show-checkbox node-key="id" :default-expanded-keys="defaultExpandedKeys" :props="defaultProps"> </el-tree>
				</el-tab-pane>
			</el-tabs>

			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="assignSave">保 存</el-button>
				<el-button @click="assignVisible = false">取 消</el-button>
			</span>
		</el-dialog>

		<el-dialog title="分配按钮" :visible.sync="assignBottonVisible" width="50%" class="assignDialogBotton">
			<div style="height:100%;">
				<div style="width:60%;float:left;height:100%;">
					<el-tabs v-model="editableBottonTabsValue" type="border-card" @tab-click="assignBottonClick" editdisable :tab-position="tabPosition">
						<el-tab-pane class="tabs__content" :key="item.key" v-for="item in appIdOptions"  :label="item.value" :name="item.key">
							<el-tree :data="bottonData" ref="bott" node-key="id" @node-click="treeHandleClick" :default-expanded-keys="defaultBottonExpandedKeys" :props="defaultProps1"> </el-tree>
						</el-tab-pane>
					</el-tabs>
				</div>
				<div style="width:40%;float:left;height:100%;padding-top: 14px;padding-left: 16px;">
					<el-checkbox :indeterminate="isIndeterminate" v-model="checkAll" @change="handleCheckAllChange">全选</el-checkbox>
					<div style="margin: 15px 0;"></div>
					<el-checkbox-group v-model="checkedBotton" @change="handleCheckedCitiesChange">
						<el-checkbox v-for="botton in bottonOptions" :label="botton.id" :key="botton.id" style="display:block; margin: 15px 0;">{{ botton.title }}</el-checkbox>
					</el-checkbox-group>
				</div>
			</div>

			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="assignBottonSave">保 存</el-button>
				<el-button @click="assignBottonVisible = false">取 消</el-button>
			</span>
		</el-dialog>
	</div>
</template>

<script>
import {
	getRoleListByFilter,
	addRole,
	getAppList,
	getMenuByAppId,
	editRole,
	delRole,
	delRoleQuoted,
	getRolesMenu,
	assignMenu,
	setRoleAdmin,
	forbiddenRole,
	enableRole,
	getBottonByMenuId,
	getRolesMenuItem,
	getBottonByAppId,
	getRolesMenuItemByRoleId,
	assignBottonSave
} from '@/api/system'
import { listToTree } from '@/utils'
import { openLoading, closeLoading } from '@/utils/loading'
import elDragDialog from '@/directive/el-drag-dialog'
import Pagination from '@/components/Pagination'

export default {
	name: 'RoleManage',
	directives: { elDragDialog },
	components: { Pagination },
	data() {
		return {
			tabPosition: 'left',
			visible: false,
			assignVisible: false,
			assignBottonVisible: false,
			form: {
				id: 0, // 大于0为编辑
				code: '',
				name: '',
				isAdmin: false,
				isForbidden: false
			},
			total: 0,
			listQuery: {
				page: 1,
				limit: 20
			},
			rules: {
				code: [{ required: true, message: '请输入角色编码', trigger: 'blur' }],
				name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
				isAdmin: [{ required: true, message: '请选择是否是管理员', trigger: 'change' }]
				// isForbidden: [{ required: true, message: '请选择是否禁用', trigger: 'change' }]
			},
			tableData: [],
			menuData: [],
			defaultProps: {
				children: 'children',
				label: 'title',
				disabled: data => {
					if (data.children) {
						let flag = data.children.every(item => !item.isPublish)
						if (flag) {
							return true
						}
					}
					return data.isPublish !== true
				}
			},
			defaultProps1: {
				children: 'children',
				label: 'title',
				disabled: data => {
					if (data.children) {
						let flag = data.children.every(item => !item.isPublish)
						if (flag) {
							return true
						}
					}
					return data.isPublish !== true
				}
			},
			defaultExpandedKeys: [], // 默认展开项
			currentRowId: '', // 当前行id

			editableTabsValue: '',
			appIdOptions: [],
			currRolesMenu: [],

			defaultBottonExpandedKeys: [],
			editableBottonTabsValue: '',
			bottonData: [],
			menuItem: [],
			rolesMenuItem: [],

			//选择按钮权限
			checkAll: false,
			checkedBotton: [],
			bottonOptions: [],
			isIndeterminate: true,
			menuIdData: ''
		}
	},

	created() {
		this.getRoleList()
		this.getAppList()
	},

	mounted() {},

	methods: {
		getRoleList() {
			openLoading()
			const { page, limit } = this.listQuery
			getRoleListByFilter({ pageIndex: page, pageSize: limit, filter: {} }).then(res => {
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
			this.form = {
				id: 0,
				code: '',
				name: '',
				isAdmin: false,
				isForbidden: false
			}
		},

		edit(row) {
			this.visible = true
			this.$refs['form'] && this.$refs['form'].clearValidate()
			this.form = row
		},

		del(row) {
			this.$confirm('你确定要删除吗?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			})
				.then(() => {
					openLoading()
					delRole({ id: row.id }).then(res => {
						if (res.code === 'success') {
							if (res.data && res.data.isQuoted) {
								this.$confirm('该角色包含引用,是否继续删除?', '提示', {
									confirmButtonText: '确定',
									cancelButtonText: '取消',
									type: 'warning'
								})
									.then(() => {
										delRoleQuoted({ id: row.id }).then(res => {
											if (res.code === 'success') {
												this.$message({
													type: 'success',
													message: '删除成功!'
												})
												this.getRoleList()
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
								this.getRoleList()
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

		setAdmin(row) {
			openLoading()
			setRoleAdmin({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '设置成功!'
					})
					this.getRoleList()
				}
				closeLoading()
			})
		},

		forbidden(row) {
			openLoading()
			forbiddenRole({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '禁用成功!'
					})
					this.getRoleList()
				}
				closeLoading()
			})
		},

		enable(row) {
			openLoading()
			enableRole({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '启用成功!'
					})
					this.getRoleList()
				}
				closeLoading()
			})
		},

		save(formName) {
			this.$refs[formName].validate(valid => {
				if (valid) {
					if (this.form.id > 0) {
						openLoading()
						editRole(this.form).then(res => {
							if (res.code === 'success' && res.data) {
								this.getRoleList()
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
						addRole(this.form).then(res => {
							if (res.code === 'success' && res.data) {
								this.getRoleList()
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

		cancel() {
			this.visible = false
		},

		// 分配保存

		assignSave() {
			var selectData = this.editableTabsValue
			var appIdDataLength = this.appIdOptions.length
			var index = 0
			for (var i = 0; i < appIdDataLength; i++) {
				if (this.appIdOptions[i].key == selectData) {
					index = i
					break
				}
			}
			let checkedArr = []
			this.$refs.tree[index].getCheckedNodes(false, true).forEach(item => {
				checkedArr.push(item.id)
			})

			openLoading()
			assignMenu({ roleId: this.currentRowId, menuId: checkedArr, appId: selectData }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '分配成功!'
					})
					this.assignVisible = false
				}
				closeLoading()
			})
		},

		//点击按钮打开弹出框默认选中第一个tab
		assign(row) {
			var self = this
			this.assignVisible = true
			this.defaultExpandedKeys = []
			this.currentRowId = row.id
			this.editableTabsValue = this.appIdOptions[0].key
			let rootIdArr = []
			getMenuByAppId({ appId: this.appIdOptions[0].key,isShowChildMenu:true }).then(res => {
				if (res.code === 'success' && res.data) {
					let listData = res.data
					this.menuData = listToTree(res.data)
					this.menuData.forEach(item => {
						rootIdArr.push(item.id)
					})
					getRolesMenu({ roleId: row.id, appId: this.appIdOptions[0].key }).then(res => {
						if (res.code === 'success' && res.data) {
							let checkedArr = [],
								expandedArr = [],
								parentIdArr = []
							//收集所有父节点
							res.data.forEach(item => {
								parentIdArr.push(item.menu.parentId)
							})
							res.data.forEach(item => {
								if (!rootIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
								//如果根节点被选中且下面没有子节点，则勾上该节点
								if (rootIdArr.includes(item.menuId) && !parentIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
							})
							checkedArr.forEach(i => {
								let flag = listData.find(j => j.id === i)
								if (flag) {
									expandedArr.push(flag.parentId)
								}
							})
							self.defaultExpandedKeys = [...new Set(expandedArr)]
							self.$refs.tree[0].setCheckedKeys(checkedArr)
						}
					})
				}
			})
		},

		getAppList() {
			var self = this
			getAppList().then(res => {
				if (res.code === 'success' && res.data) {
					let appArr = []
					res.data.forEach(item => {
						appArr.push({ value: item.appName, key: item.appId })
					})
					self.appIdOptions = appArr
				}
			})
		},

		handleClick(tab, event) {
			var self = this
			this.assignVisible = true
			this.defaultExpandedKeys = []
			let rootIdArr = []
			getMenuByAppId({ appId: tab.name ,isShowChildMenu:true}).then(res => {
				if (res.code === 'success' && res.data) {
					let listData = res.data
					this.menuData = listToTree(res.data)
					this.menuData.forEach(item => {
						rootIdArr.push(item.id)
					})
					getRolesMenu({ roleId: self.currentRowId, appId: tab.name }).then(res => {
						if (res.code === 'success' && res.data) {
							let checkedArr = [],
								expandedArr = [],
							    parentIdArr = []
							//收集所有父节点
							res.data.forEach(item => {
								parentIdArr.push(item.menu.parentId)
							})
							res.data.filter(item => !parentIdArr.includes(item.menuId)).forEach(item => {
								if (!rootIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
								//如果根节点被选中且下面没有子节点，则勾上该节点
								if(rootIdArr.includes(item.menuId) && !parentIdArr.includes(item.menuId)){
									checkedArr.push(item.menuId)
								}
							})
							checkedArr.forEach(i => {
								let flag = listData.find(j => j.id === i)
								if (flag) {
									expandedArr.push(flag.parentId)
								}
							})
							self.defaultExpandedKeys = [...new Set(expandedArr)]
							self.$refs.tree[tab.index].setCheckedKeys(checkedArr)
						}
					})
				}
			})
		},

		//点击分配按钮权限
		assignBotton(row) {
			this.bottonOptions = [] //每次点击tab清空按钮选择
			var self = this
			this.assignBottonVisible = true
			this.defaultBottonExpandedKeys = []
			this.currentRowId = row.id
			this.editableBottonTabsValue = this.appIdOptions[1].key
			let rootIdArr = []
			getMenuByAppId({ appId: this.appIdOptions[0].key,isShowChildMenu:true }).then(res => {
				if (res.code === 'success' && res.data) {
					let listData = res.data
					this.bottonData = listToTree(res.data)
					this.bottonData.forEach(item => {
						rootIdArr.push(item.id)
					})
					getRolesMenu({ roleId: row.id, appId: this.appIdOptions[0].key }).then(res => {
						if (res.code === 'success' && res.data) {
							let checkedArr = [],
								expandedArr = [],
								parentIdArr = []
							//收集所有父节点
							res.data.forEach(item => {
								parentIdArr.push(item.menu.parentId)
							})
							res.data.forEach(item => {
								if (!rootIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
								//如果根节点被选中且下面没有子节点，则勾上该节点
								if (rootIdArr.includes(item.menuId) && !parentIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
							})
							checkedArr.forEach(i => {
								let flag = listData.find(j => j.id === i)
								if (flag) {
									expandedArr.push(flag.parentId)
								}
							})
							self.defaultBottonExpandedKeys = [...new Set(expandedArr)]
							self.$refs.bott[0].setCheckedKeys(checkedArr)
							let tab={index:1,name:this.appIdOptions[1].key}
							this.assignBottonClick(tab)
						}
					})
				}
			})
		},

		//tab点击事件
		assignBottonClick(tab, event) {
			this.bottonOptions = [] //每次点击tab清空按钮选择
			var self = this
			this.assignBottonVisible = true
			this.defaultBottonExpandedKeys = []
			let rootIdArr = []
			getMenuByAppId({ appId: tab.name ,isShowChildMenu:true}).then(res => {
				if (res.code === 'success' && res.data) {
					let listData = res.data
					this.bottonData = listToTree(res.data)
					this.bottonData.forEach(item => {
						rootIdArr.push(item.id)
					})
					getRolesMenu({ roleId: self.currentRowId, appId: tab.name }).then(res => {
						if (res.code === 'success' && res.data) {
							let checkedArr = [],
								expandedArr = [],
								parentIdArr = []
							//收集所有父节点
							res.data.forEach(item => {
								parentIdArr.push(item.menu.parentId)
							})
							res.data.forEach(item => {
								if (!rootIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
								//如果根节点被选中且下面没有子节点，则勾上该节点
								if (rootIdArr.includes(item.menuId) && !parentIdArr.includes(item.menuId)) {
									checkedArr.push(item.menuId)
								}
							})
							checkedArr.forEach(i => {
								let flag = listData.find(j => j.id === i)
								if (flag) {
									expandedArr.push(flag.parentId)
								}
							})
							self.defaultBottonExpandedKeys = [...new Set(expandedArr)]
							self.$refs.bott[tab.index].setCheckedKeys(checkedArr)

							//根据appId查询所有menuItem
							getBottonByAppId({ appId: tab.name }).then(res => {
								if (res.code === 'success' && res.data) {
									self.menuItem = res.data
								}
							})
							//根据roleId查询所有RolesMenuItem
							getRolesMenuItemByRoleId({ roleId: self.currentRowId }).then(res => {
								if (res.code === 'success' && res.data) {
									self.rolesMenuItem = res.data
								}
							})
						}
					})
				}
			})
		},

		//保存按钮权限
		assignBottonSave() {
			var selectData = this.editableBottonTabsValue
			let checkArr = []
			this.rolesMenuItem.forEach(item => {
				checkArr.push(item.menuItemId)
			})
			openLoading()
			assignBottonSave({ roleId: this.currentRowId, menuId: checkArr, appId: selectData }).then(res => {
				if (res.code === 'success') {
					this.$message({
						type: 'success',
						message: '分配成功!'
					})
					this.assignBottonVisible = false
				}
				closeLoading()
			})
		},

		//tree点击事件
		treeHandleClick(data, node) {
			var self = this
			this.bottonOptions = []
			this.checkedBotton = []
			this.menuIdData = data.id

			let bottonIdList = []
			this.menuItem.forEach(item => {
				if (item.menuId === data.id) {
					self.bottonOptions.push({ title: item.title, id: item.id })
					bottonIdList.push(item.id)
				}
			})
			this.rolesMenuItem.forEach(item => {
				if (bottonIdList.includes(item.menuItemId)) {
					self.checkedBotton.push(item.menuItemId)
				}
			})
			if (bottonIdList.length == 0) {
				//未选中
				this.checkAll = false
				this.isIndeterminate = false
			} else if (self.checkedBotton.length == bottonIdList.length) {
				//全部选中
				this.checkAll = true
				this.isIndeterminate = false
			} else {
				//未完全选中
				this.checkAll = false
				this.isIndeterminate = true
			}
		},

		handleCheckAllChange(val) {
			var self = this
			const arr = val ? this.bottonOptions : []
			this.checkedBotton = []
			arr.map(item => {
				this.checkedBotton.push(item.id)
			})
			this.isIndeterminate = false

			if (arr.length > 0) {
				//全选
				arr.forEach(item => {
					this.rolesMenuItem.push({ menuItemId: item.id })
				})
			} else {
				//取消
				let cancelCheckAll = []
				this.bottonOptions.forEach(item => {
					cancelCheckAll.push(item.id)
				})
				let newList = []
				this.rolesMenuItem.forEach(item => {
					if (!cancelCheckAll.includes(item.menuItemId)) {
						newList.push({ menuItemId: item.menuItemId })
					}
				})
				this.rolesMenuItem = newList
			}
		},
		handleCheckedCitiesChange(value) {
			var self = this
			let arrbotton = value
			this.checkedBotton = arrbotton
			let checkedCount = value.length
			this.checkAll = checkedCount === this.bottonOptions.length
			this.isIndeterminate = checkedCount > 0 && checkedCount < this.bottonOptions.length

			let deleteMenuItem = []
			let currMenuItem = []
			this.rolesMenuItem.forEach(item => {
				currMenuItem.push(item.menuItemId)
			})
			this.menuItem.forEach(item => {
				if (self.menuIdData && item.menuId != self.menuIdData && currMenuItem.includes(item.id)) {
					deleteMenuItem.push(item.id)
				}
			})
			value.forEach(item => {
				deleteMenuItem.push(item)
			})

			let newList = []
			deleteMenuItem.forEach(item => {
				newList.push({ menuItemId: item })
			})
			this.rolesMenuItem = newList
		}
	}
}
</script>

<style lang="scss" scoped>
.rolemanage {
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
