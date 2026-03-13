<template>
    <div class="matrixpricelistquery">
		<div class="queryPage">
			<div class="queryForm">
				<el-form ref="queryForm" :model="queryForm" label-width="100px" size="mini" inline>
					<el-form-item label="应用名称">
						<el-select v-model="queryForm.detailName" clearable placeholder="请选择">
                            <el-option label="审批应用" value="审批应用"></el-option>
                            <el-option label="通讯录应用" value="通讯录应用"></el-option>
							<el-option label="工作平台应用" value="工作平台应用"></el-option>
                        </el-select>
					</el-form-item>	
					<el-form-item label="创建日期">
						<el-date-picker
							v-model="queryForm.allDate"
							type="daterange"
    						value-format="timestamp"
							range-separator="至"
							start-placeholder=""
							end-placeholder="">
						</el-date-picker>
					</el-form-item>
					<el-form-item label="审批单号">
						<el-input v-model="queryForm.spno" placeholder="审批单号"></el-input>
					</el-form-item>
                    <el-form-item label="是否完成">
						<el-select v-model="queryForm.isComplete" clearable placeholder="请选择">
                            <el-option label="是" value="true"></el-option>
                            <el-option label="否" value="false"></el-option>
                        </el-select>
					</el-form-item>		
                    <el-form-item label="审批状态">
						<el-select v-model="queryForm.status" clearable placeholder="请选择">
                            <el-option v-if="item" v-for="(item,index) in status" :key="index" :label="item" :value="index"></el-option>
                        </el-select>
					</el-form-item>	
					<el-form-item>
						<el-button type="primary" style="margin-left: 16px;" @click="query">查询</el-button>
						<el-button type="text" style="padding-bottom: 0px;vertical-align: middle;" @click="clearCondition">清除条件</el-button>
						<el-button type="text" style="padding-bottom: 0px;vertical-align: middle;" @click="repairSp">审批修复</el-button>
					</el-form-item>
				</el-form>
			</div>
			<div class="tableBox" style="background-color: #f5f7fa;">
				<el-table :data="tableData" :stripe="true" style="width: 100%;" height="77vh" border size="mini">
					<el-table-column prop="detailName" label="应用名称" align="center" width="110"></el-table-column>
					<el-table-column prop="spno" label="审批单号" align="center" width="110"></el-table-column>
					<el-table-column label="审批状态" align="center" width="75">
						<template slot-scope="scope">
                            {{status[scope.row.status]}}
                        </template>
					</el-table-column>
					<el-table-column width="150" prop="createDate.format('YYYY-MM-DD HH:mm:ss')" label="创建日期" align="center" :formatter="row => row.createDate && row.createDate.replace('T', ' ').slice(0,19)"></el-table-column>
					<el-table-column width="150" prop="completeDate.format('YYYY-MM-DD HH:mm:ss')" label="完成日期" align="center" :formatter="row => row.completeDate && row.completeDate.replace('T', ' ').slice(0,19)"></el-table-column>
					<el-table-column label="是否完成" align="center" width="70">
                        <template slot-scope="scope">
                            {{scope.row.isComplete?"是":"否"}}
                        </template>
                    </el-table-column>
                    <el-table-column prop="result" label="结果" width="250" align="left"></el-table-column>
                    <el-table-column label="消息" align="left">
						<template slot-scope="scope">
							<div v-if="scope.row.message">{{scope.row.message.substring(0,40)}}</div>
                            <el-popover
								placement="bottom"
								title="消息"
								width="750"
								trigger="click"
								:content="scope.row.message">
								<el-button size="mini" type="text" slot="reference">查看</el-button>
								<el-button size="mini" :data-clipboard-text="scope.row.message" type="text" slot="reference" id="copyMessage" @click="copyMessage">复制</el-button>
							</el-popover>
                        </template>
					</el-table-column>
					<el-table-column  label="堆栈踪迹" align="left">
						<template slot-scope="scope">
							<div v-if="scope.row.stackTrace">{{scope.row.stackTrace.substring(0,40)}}</div>
                            <el-popover v-if="scope.row.stackTrace"
								placement="bottom"
								title="堆栈踪迹"
								width="750"
								trigger="click"
								:content="scope.row.stackTrace">
								<el-button size="mini" type="text" slot="reference">查看</el-button>
								<el-button size="mini" :data-clipboard-text="scope.row.stackTrace" type="text" slot="reference" id="copy" @click="copy">复制</el-button>
							</el-popover>
                        </template>
					</el-table-column>
					<el-table-column label="操作" align="center" width="100">
						<template slot-scope="scope">	
							<el-button v-if="scope.row.detailName == '审批应用'" type="text" size="small" @click="SpDetail(scope.row.spno)">详情</el-button>
							<el-button type="text" size="small" v-if="!scope.row.isComplete" @click="execute(scope.row.id)">重新执行</el-button>
						</template>
				</el-table-column>
				</el-table>
				<pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="query"/>
			</div>
		</div>
		<div v-if="dialogVisible">
            <div class="maskLayer"></div>
			<div class="detail">
				<el-button class="close" size="mini" type="primary" @click="closeDetail">X</el-button>
				<div class="detailTitle">{{info.sp_name}}（{{astatus[info.sp_status]}}）</div>
				<div class="contents" v-for="(item,index) in keyValues" :key="index">
					{{item.key+"："+item.value}}
				</div>
				<div style="margin: 12px 0 0 24px; font-weight: bold; font-size: 15px;">审批流程</div>
				<el-timeline class="innerDetail">
					<el-timeline-item color="#4b95f3" size="large" placement="top" timestamp="申请人">		
						{{info.applyer.userid}}
					</el-timeline-item>
					<el-timeline-item color="#4b95f3" size="large" placement="top" v-for="(item, index) in info.sp_record" :key="index" :timestamp="item.details.length>1?(item.approverattr==1?'审批人'+ test(item,index) +'（或签）':'审批人'+ test(item,index) +'（会签）'):'审批人'+ test(item,index) ">		
						<span style="margin-right: 8px;" v-for="(item2,index2) in item.details" :key="index2">{{item2.approver.userid}}</span>
					</el-timeline-item>
					<el-timeline-item v-if="info.notifyer?(info.notifyer.length>0?true:false):false" color="#4b95f3" size="large" placement="top" timestamp="抄送人">		
						<span style="margin-right: 8px;" v-for="(item, index) in info.notifyer" :key="index" >{{item.userid}}</span>
					</el-timeline-item>
				</el-timeline>
			</div>
		</div>
	</div>
</template>

<script>
import { getWxCallbackMessage, getSpDetail, execute,repairSp } from '@/api/system'
import Pagination from '@/components/Pagination'
import { closeLoading, openLoading } from '../../../utils/loading'
import Clipboard from 'clipboard'

export default {
    name: 'WxCallbackMessage',
    components: { Pagination },
	data(){
        return{
			total: 0,
			listQuery: {
				page: 1,
				limit: 20
			},
			status: [],
			astatus: [],
			queryForm: {
				allDate: [],
				isComplete: null,
                spno: '',
				status: null,
				detailName: ''
			},
			tableData: [],
			info: {},
			dialogVisible: false,
			keyValues: []
		}
    },
    created(){
		this.status[1] = "审批中"
		this.status[2] = "已同意"
		this.status[3] = "已驳回"
		this.status[4] = "已转审"
		this.status[11] = "已退回"
		this.astatus[1] = "审批中"
		this.astatus[2] = "已通过"
		this.astatus[3] = "已驳回"
		this.astatus[4] = "已撤销"
		this.astatus[6] = "通过后撤销"
		this.astatus[7] = "已删除"
		this.astatus[10] = "已支付"
        this.query()
    },	
    methods:{
		execute(id){
			openLoading()
			execute({id:id}).then(res=>{
				if(res.code == 'success'){
					this.$message.success("执行成功")
					this.query()
				}
				closeLoading()
			})
		},
		repairSp(){
			openLoading()
			if(!this.queryForm.spno)
			{
			 this.$message.info('审批单号不能为空')
			 return
			}
			repairSp({spno:this.queryForm.spno}).then(res=>{
				if(res.code == 'success'){
					this.$message.success("执行成功")
					this.query()
				}
				closeLoading()
			})
		},
		copy () {
			let _this = this
			let clipboard = new Clipboard('#copy')
			//复制成功
			clipboard.on('success', function() {
				_this.$message.success('复制成功！')
				clipboard.destroy()
			})
			//复制失败
			clipboard.on('error', function() {
				_this.$message.error('复制失败！')
				clipboard.destroy()
			})
		},
		copyMessage () {
			let _this = this
			let clipboard = new Clipboard('#copyMessage')
			//复制成功
			clipboard.on('success', function() {
				_this.$message.success('复制成功！')
				clipboard.destroy()
			})
			//复制失败
			clipboard.on('error', function() {
				_this.$message.error('复制失败！')
				clipboard.destroy()
			})
		},
		test(item,index){
			return this.info.sp_record[index-1]? (this.info.sp_record[index-1].sp_status==2?'·'+this.status[item.sp_status]:'') : '·'+this.status[item.sp_status]
		},
        query(){
            openLoading()
            const { page, limit } = this.listQuery
			var queryForm = {
				createDate: this.queryForm.allDate && this.queryForm.allDate.length>0?new Date(this.queryForm.allDate[0] + 28800000):null,
				completeDate: this.queryForm.allDate && this.queryForm.allDate.length>0?new Date(this.queryForm.allDate[1] + 86400000 + 28800000):null,
				isComplete: this.queryForm.isComplete == null ? null : this.queryForm.isComplete == "true",
                spno: this.queryForm.spno.trim(),
				status: this.queryForm.status,
				detailName: this.queryForm.detailName
			}
			// console.log(queryForm)
            getWxCallbackMessage({ pageIndex: page, pageSize: limit, filter: queryForm }).then(res=>{
                if(res.code == "success"){
                    const { total, rows } = res.data
					this.total = total
					this.tableData = rows
                }
                closeLoading()
            })
        },
        clearCondition(){
            this.queryForm = {
				allDate: [],
				isComplete: null,
                spno: '',
				status: null,
				detailName: ''
			}
        },
		SpDetail(spno){
			openLoading()
			getSpDetail({spno}).then(res=>{
				if(res.code == "success"){
					// console.log(res)
					this.info = res.data.info
					var contents = res.data.info.apply_data.contents
					var keyValues = []
					for(let i in contents){
						if(contents[i].control == 'Text' || contents[i].control == 'Textarea'){
							keyValues.push({key: contents[i].title[0].text, value: contents[i].value.text})
						}
						if(contents[i].control == 'Selector'){
							keyValues.push({key: contents[i].title[0].text, value: contents[i].value.selector.options[0].value[0].text})
						}
						if(contents[i].control == 'Money'){
							keyValues.push({key: contents[i].title[0].text, value: contents[i].value.new_money})
						}
						if(contents[i].control == 'Number'){
							keyValues.push({key: contents[i].title[0].text, value: contents[i].value.new_number})
						}
						if(contents[i].control == 'Date'){
							var time = contents[i].value.date.s_timestamp				
							var unixTimestamp = new Date(time*1000);
							keyValues.push({key: contents[i].title[0].text, value: unixTimestamp.toLocaleString()})
						}
						// if(contents[i].control == 'File'){
						// 	keyValues.push({key: contents[i].title[0].text, value: contents[i].value.files[0].file_id})
						// }		
						// if(contents[i].control == 'Contact'){
						// 	keyValues.push({key: contents[i].title[0].text, value: contents[i].value.files[0].file_id})
						// }
					}
					this.keyValues = keyValues
					// console.log(this.keyValues)
					this.dialogVisible = true
				}
				closeLoading()
			})
		},
		closeDetail(){
			this.dialogVisible = false
		}
    }
}
</script>

<style lang="scss" scoped>
    .matrixpricelistquery {
	// margin-top: 8px;
	.queryForm {
		background-color: #fff;
		padding: 30px 20px 0px 0px;
		.el-form {
			.searchIcon {
				cursor: pointer;
				font-size: 16px;
			}
		}
	}
	.tableBox {
		// margin-top: 8px;
		background: #fff;
		::v-deep .el-table__body {
			font-size: 12px;
		}
		.headerTop {
			color: rgb(255, 102, 0);
		}
		.headerBottom {
			color: rgb(51, 125, 221);
		}
	}
	.contentTop {
		color: #ff6600;
	}
	.contentBottom {
		color: rgb(0, 0, 255);
	}
	.excelIcon {
		font-size: 18px;
		color: #409eff;
	}
	.navtitle {
		font-size: 15px;
		font-weight: 700;
		background-color: #fff;
		padding: 8px;
		.el-divider--vertical {
			width: 4px;
			background-color: rgb(64, 158, 255);
		}
		.titlecontent {
			vertical-align: middle;
		}
	}
}
.pagination-container{
    background-color: #f5f7fa;
	padding: 6px 0;
	margin: 0;
}
.maskLayer{
    background-color: #808080; 
    z-index: 10000; 
    width: 100%; 
    height: 100%; 
    opacity: .5; 
    top: 0; 
    left: 0; 
    position: fixed;
}
.detail{
    position: absolute; 
    top: 0%; 
    left: 35%; 
    width: 30%; 
    max-height: 100%; 
    overflow: auto; 
    z-index: 10000; 
    background-color: #ffffff;
}
.innerDetail{
	margin-top: 20px;
}
.close{
	position: absolute; 
    top: 10px; 
    right: 10px;
	cursor: pointer;
}
.detailTitle{
	margin: 12px 0 0 24px;
	font-weight: bolder;
}
.contents{
	margin: 12px 0 0 24px;
	font-size: 14px;
}
</style>