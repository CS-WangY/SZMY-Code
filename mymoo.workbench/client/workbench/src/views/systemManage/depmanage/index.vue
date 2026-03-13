<template>
	<div class="depmanage">
		<!-- 列表展示 -->
		<div class="tableBox">
			<el-table :data="tableData" style="width: 100%;margin-bottom: 20px;" row-key="id" border default-expand-all :tree-props="{ children: 'children', hasChildren: 'hasChildren' }">
				<el-table-column prop="name" label="部门名称"> </el-table-column>
				<el-table-column prop="functionAttr" label="职能属性">
					<template slot-scope="{ row }">
						<template v-if="row.edit">
							<!-- <el-input v-model="row.functionAttr" class="edit-input" size="small" style="width: 80%" /> -->
							<el-select v-model="row.functionAttr" placeholder="请选择职能属性" size="small" style="width: 80%">
								<el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value"> </el-option>
							</el-select>
							<el-button class="cancel-btn" size="small" icon="el-icon-refresh" type="warning" @click="cancelEdit(row)" style="margin-left: 10px">
								取消
							</el-button>
						</template>
						<span v-else>{{ options.find(item => item.value === row.functionAttr) && options.find(item => item.value === row.functionAttr).label }}</span>
					</template>
				</el-table-column>
				<el-table-column label="操作" align="center">
					<template slot-scope="{ row }">
						<el-button v-if="row.edit" type="success" size="small" icon="el-icon-circle-check" @click="confirmEdit(row)">
							确认
						</el-button>
						<el-button v-else type="primary" size="small" icon="el-icon-edit" @click="row.edit = !row.edit">
							编辑
						</el-button>
					</template>
				</el-table-column>
			</el-table>
		</div>
	</div>
</template>

<script>
import { GetWeiXinWorkDepartmentDetials, updateDepartmentFunctionAttr, getFunctionAttrLsit } from '@/api/system'
import { newListToTree } from '@/utils'
import { openLoading, closeLoading } from '@/utils/loading'

export default {
	name: 'depmanage',
	data() {
		return {
			tableData: [],
			options: []
		}
	},

	created() {
		this.GetWeiXinWorkDepartmentDetials()
		this.getFunctionAttrLsit()
	},

	mounted() {},

	methods: {
		// 获取部门详情
		GetWeiXinWorkDepartmentDetials() {
			openLoading()
			GetWeiXinWorkDepartmentDetials().then(res => {
				if (res.code === 'success' && res.data) {
					res.data = res.data.map(i =>
						i.map(j => {
							j.edit = false
							j.originalfunctionAttr = j.functionAttr
							return j
						})
					)
					var rootNode = newListToTree(res.data[0], 0, 'parentId')
					for (var i = 1; i < res.data.length; i++) {
						var otherNode = newListToTree(res.data[i], 0, 'parentId')
						rootNode.push(otherNode[0])
					}
					this.tableData = rootNode
				}
				closeLoading()
			})
		},

		// 获取职能属性列表
		getFunctionAttrLsit() {
			getFunctionAttrLsit().then(res => {
				if (res.code === 'success' && res.data) {
					this.options = res.data.map(item => {
						return { value: item.systemParamKey, label: item.systemParamValue }
					})
				}
			})
		},

		cancelEdit(row) {
			row.functionAttr = row.originalfunctionAttr
			row.edit = false
			this.$message.warning('已取消')
		},

		confirmEdit(row) {
			row.edit = false
			row.originalfunctionAttr = row.functionAttr
			updateDepartmentFunctionAttr({ id: row.id, functionAttr: row.functionAttr }).then(res => {
				if (res.code === 'success') {
					this.$message.success('编辑成功')
				}
			})
		}
	}
}
</script>

<style lang="scss" scoped>
.depmanage {
	margin-top: 8px;
	.tableBox {
		background-color: #fff;
		padding: 20px;
	}
}
</style>
