<template>
	<div class="postmanage">
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-button type="primary" style="marginBottom: 12px;float: right" @click="add">新增岗位</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<el-table-column prop="code" label="岗位编码" align="center"> </el-table-column>
				<el-table-column prop="name" label="岗位名称" align="center"> </el-table-column>
				<el-table-column prop="isForbidden" label="是否禁用" align="center" :formatter="row => (row.isForbidden ? '是' : '否')"> </el-table-column>
				<el-table-column prop="isAssistant" label="是否是助理" align="center" :formatter="row => (row.isAssistant ? '是' : '否')"> </el-table-column>
				<el-table-column prop="description" label="描述" width="200" align="center"> </el-table-column>
				<el-table-column prop="createUser" label="创建人" align="center"> </el-table-column>
				<el-table-column prop="createDate" label="创建时间" align="center" :formatter="row => row.createDate && row.createDate.slice(0, 10)"> </el-table-column>
				<el-table-column prop="forbiddenUser" label="禁用人" align="center"> </el-table-column>
				<el-table-column prop="forbiddenDate" label="禁用时间" align="center" :formatter="row => row.forbiddenDate && row.forbiddenDate.slice(0, 10)"> </el-table-column>
				<el-table-column label="操作" align="center" width="250">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="edit(scope.row)" :disabled="!scope.row.isForbidden">编辑</el-button>
						<el-button type="text" size="small" @click="forbidden(scope.row)" v-if="!scope.row.isForbidden">禁用</el-button>
						<el-button type="text" size="small" @click="enable(scope.row)" v-if="scope.row.isForbidden">启用</el-button>
					</template>
				</el-table-column>
			</el-table>
		</div>
		<!-- 新增/编辑modal -->
		<el-dialog :title="form.id > 0 ? '编辑岗位' : '新增岗位'" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-form :model="form" ref="form" :rules="rules" label-width="100px">
				<el-form-item label="岗位编码" prop="code">
					<el-input v-model="form.code" placeholder="请输入岗位编码"></el-input>
				</el-form-item>
				<el-form-item label="岗位名称" prop="name">
					<el-input v-model="form.name" placeholder="请输入岗位名称"></el-input>
				</el-form-item>
				<el-form-item label="是否是助理" prop="isAssistant">
					<el-radio-group v-model="form.isAssistant">
						<el-radio :label="true">是</el-radio>
						<el-radio :label="false">否</el-radio>
					</el-radio-group>
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
import { getPositionList, addPosition, editPosition, forbiddenPosition, enablePosition } from '@/api/system'
import { openLoading, closeLoading } from '@/utils/loading'
import elDragDialog from '@/directive/el-drag-dialog'

export default {
	name: 'PostManage',
	directives: { elDragDialog },
	data() {
		return {
			visible: false,
			form: {
				id: 0, // 大于0为编辑
				code: '',
				name: '',
				isAssistant: false,
				isForbidden: false
			},
			rules: {
				code: [{ required: true, message: '请输入岗位编码', trigger: 'blur' }],
				name: [{ required: true, message: '请输入岗位名称', trigger: 'blur' }],
				isAssistant: [{ required: true, message: '请选择是否是助理', trigger: 'change' }],
			},
			tableData: []
		}
	},

	created() {
		this.getPositionList()
	},

	mounted() {},

	methods: {
		getPositionList() {
			openLoading()
			getPositionList().then(res => {
				if (res.code === 'success' && res.data) {
					this.tableData = res.data
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
				isAssistant: false,
				isForbidden: false
			}
		},

		edit(row) {
			this.visible = true
			this.$refs['form'] && this.$refs['form'].clearValidate()
			this.form = row
		},

		forbidden(row) {
			openLoading()
			forbiddenPosition({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message.success('禁用成功')
					this.getPositionList()
				}
				closeLoading()
			})
		},

		enable(row) {
			openLoading()
			enablePosition({ id: row.id }).then(res => {
				if (res.code === 'success') {
					this.$message.success('启用成功')
					this.getPositionList()
				}
				closeLoading()
			})
		},

		save(formName) {
			this.$refs[formName].validate(valid => {
				if (valid) {
					if (this.form.id > 0) {
						openLoading()
						editPosition(this.form).then(res => {
							if (res.code === 'success' && res.data) {
								this.getPositionList()
								this.visible = false
								this.$message.success('编辑成功')
							}
							closeLoading()
						})
					} else {
						openLoading()
						addPosition(this.form).then(res => {
							if (res.code === 'success' && res.data) {
								this.getPositionList()
								this.visible = false
								this.$message.success('添加成功')
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
		}
	}
}
</script>

<style lang="scss" scoped>
.postmanage {
	margin-top: 8px;
	.tableBox {
		background-color: #fff;
		padding: 20px;
		.el-dropdown-link {
			cursor: pointer;
			color: #409eff;
			padding-left: 14px;
			font-size: 12px;
		}
		.el-icon-arrow-down {
			font-size: 12px;
		}
	}
}
</style>
