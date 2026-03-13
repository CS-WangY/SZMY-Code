<template>
	<div class="systemparameter">
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-button type="primary" style="marginBottom: 12px;float: right" @click="add">新增系统参数</el-button>
			<el-table :data="tableData" style="width: 100%" border>
				<!-- <el-table-column prop="id" label="ID" align="center"> </el-table-column> -->
				<el-table-column prop="groupId" label="组ID" align="center" width="100"> </el-table-column>
				<el-table-column prop="systemParamKey" label="系统参数键" align="center" width="350"> </el-table-column>
				<el-table-column prop="systemParamValue" label="系统参数值" align="center" width="350"> </el-table-column>
				<el-table-column prop="systemParamDesc" label="描述" align="center"> </el-table-column>
				<el-table-column label="操作" align="center" width="220">
					<template slot-scope="scope">
						<el-button type="text" size="small" @click="edit(scope.row)">编辑</el-button>
						<el-button type="text" size="small" @click="del(scope.row)">删除</el-button>
					</template>
				</el-table-column>
			</el-table>
		</div>
		<!-- 新增/编辑modal -->
		<el-dialog :title="form.id > 0 ? '编辑系统参数' : '新增系统参数'" :visible.sync="visible" width="30%" :close-on-click-modal="false" v-el-drag-dialog>
			<el-form :model="form" ref="form" :rules="rules" label-width="100px">
				<el-form-item label="组ID" prop="groupId">
					<el-input v-model="form.groupId" placeholder="请输入组ID"></el-input>
				</el-form-item>
				<el-form-item label="系统参数键" prop="systemParamKey">
					<el-input v-model="form.systemParamKey" placeholder="请输入系统参数键"></el-input>
				</el-form-item>
				<el-form-item label="系统参数值" prop="systemParamValue">
					<el-input v-model="form.systemParamValue" placeholder="请输入系统参数值"></el-input>
				</el-form-item>
				<el-form-item label="描述" prop="systemParamDesc">
					<el-input type="textarea" :rows="3" v-model="form.systemParamDesc" placeholder="请输入描述"></el-input>
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
import { getSystemParam, addSystemParam, editSystemParam, delSystemParam } from '@/api/system'
import { openLoading, closeLoading } from '@/utils/loading'
import elDragDialog from '@/directive/el-drag-dialog'

export default {
	name: 'systemparameter',
	directives: { elDragDialog },
	data() {
		return {
			visible: false,
			form: {
				id: 0, // 大于0为编辑
				groupId: 0,
				systemParamKey: '',
				systemParamValue: '',
				systemParamDesc: ''
			},
			tableData: [],
			rules: {
				groupId: [{ required: true, message: '请输入组ID', trigger: 'blur' }],
				systemParamKey: [{ required: true, message: '请输入系统参数键', trigger: 'blur' }],
				systemParamValue: [{ required: true, message: '请输入系统参数值', trigger: 'blur' }],
				systemParamDesc: [{ required: true, message: '请输入描述', trigger: 'blur' }]
			}
		}
	},

	created() {
		this.getSystemParam()
	},

	mounted() {},

	methods: {
		getSystemParam() {
			openLoading()
			getSystemParam().then(res => {
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
				groupId: 0,
				systemParamKey: '',
				systemParamValue: '',
				systemParamDesc: ''
			}
		},

		edit(row) {
			this.visible = true
			this.$refs['form'] && this.$refs['form'].clearValidate()
			this.form = row
		},

		save(formName) {
			this.$refs[formName].validate(valid => {
				if (valid) {
					if (this.form.id > 0) {
						openLoading()
						editSystemParam({ ...this.form, groupId: +this.form.groupId }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getSystemParam()
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
						addSystemParam({ ...this.form, groupId: +this.form.groupId }).then(res => {
							if (res.code === 'success' && res.data) {
								this.getSystemParam()
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

		del(row) {
			this.$confirm('你确定要删除吗?', '提示', {
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				type: 'warning'
			})
				.then(() => {
					openLoading()
					delSystemParam({ id: row.id }).then(res => {
						if (res.code === 'success') {
							this.getSystemParam()
							this.$message({
								type: 'success',
								message: '删除成功!'
							})
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

		cancel() {
			this.visible = false
		}
	}
}
</script>

<style lang="scss" scoped>
.systemparameter {
	margin-top: 8px;
	background: #fff;
	.tableBox {
		padding: 20px;
	}
	::v-deep .el-select {
		width: 100%;
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
	.el-form {
		width: 85%;
		margin: 0 auto;
	}
}
</style>
