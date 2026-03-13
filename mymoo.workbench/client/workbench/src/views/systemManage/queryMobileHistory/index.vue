<template>
    
    <div class="matrixpricelistquery">
		<div class="queryPage">
			<div class="queryForm">
				<el-form ref="queryForm" :model="form" label-width="100px" size="mini" inline>
                    <el-form-item label="操作人" prop="userId">
					<UserSearch :user.sync="form.userId" :isMymo="true"  ref="userRef" placeholder="请输入操作人" />
                </el-form-item>
                <el-form-item label="日期" prop="date">
							<el-date-picker
								v-model="allDate"
								type="daterange"
								align="right"
								unlink-panels
								range-separator="至"
								start-placeholder="开始日期"
								end-placeholder="结束日期"
								value-format="yyyy-MM-dd"
								size="small"
								@change="setShortcutDate"
							>
							</el-date-picker>
						</el-form-item>
						<!-- 快捷选择时间 -->
						<el-form-item label="" prop="shortcutDate" style="margin-left: 20px;">
							<el-radio-group v-model="shortcutDate" @change="radioChange" size="small">
								<el-radio-button label="3">当月</el-radio-button>
								<el-radio-button label="4">当季</el-radio-button>
								<el-radio-button label="5">当年</el-radio-button>
							</el-radio-group>
						</el-form-item>
					<el-form-item>
						<el-button type="primary" style="margin-left: 1.5em" @click="queryMoblieTimes">查询</el-button>
					</el-form-item>
				</el-form>
			</div>
			<div class="tableBox" style="background-color: #f5f7fa;">
				<el-table :data="tableData" :stripe="true" style="width: 100%;" border >
					<el-table-column prop="key" label="查询人" align="center"></el-table-column>
					<el-table-column prop="value" label="查询次数" align="center" ></el-table-column>
                    <el-table-column label="操作" align="center">
					<template slot-scope="scope">
						<el-button @click="showddetail(scope.row)" type="text" size="small">查看明细</el-button>
					</template>
				</el-table-column>
				</el-table>
                <Pagination v-show="total > 0" :total="total" :pageSizes="[20, 50, 100, 200, 500, 1000]" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="queryMoblieTimes" />
			</div>
		</div>
		
        <el-dialog title="查询历史详情" :visible.sync="queryhistorySync" width="60%"  :close-on-click-modal="false" >
                <el-table :data="tabledetailData" style="width: 100%" height="400" border>
				<el-table-column prop="name" label="查询姓名" align="center"> </el-table-column>
				<el-table-column prop="deptName" label="查询部门" align="center"> </el-table-column>
				<el-table-column prop="mobile" label="查询手机号" align="center"> </el-table-column>
				<el-table-column prop="queryByName" label="查询人" align="center"> </el-table-column>
				<el-table-column prop="createTime" label="查询时间" align="center" :formatter="row=> row.createTime && this.dayjs(row.createTime).format('YYYY-MM-DD HH:mm:ss')"> </el-table-column>
			</el-table>
			<Pagination v-show="total > 0" :total="historytotal" :pageSizes="[5, 10, 20, 50, 100]" :page.sync="listhistoryQuery.page" :limit.sync="listhistoryQuery.limit" @pagination="getList" />
    </el-dialog>

	</div>
</template>


<script>
import { queryMoblieTimes,queryMobileHistory} from '@/api/system'
import Pagination from '@/components/Pagination'
import UserSearch from '@/components/UserSearch'
import { openLoading, closeLoading } from '@/utils/loading'

export default {
    name: 'queryMobileHistory',
	components: { Pagination, UserSearch },
	data(){
        return{
            timeType:null,
			form: {
                userId:null
			},
            timeDefaultShow:null,
			tableData: [],
            tabledetailData:[],
            queryhistorySync:false,
            historytotal:0,
            listhistoryQuery: {
				page: 1,
				limit: 5
			},
            total:0,
            allDate:null,
            shortcutDate:'',
            listQuery: {
				page: 1,
				limit: 20
			},
            filter:null
		}
    },
    created(){
      
        this.allDate = [
			this.dayjs()
				.subtract(1, 'month')
				.format('YYYY-MM-DD'),
			this.dayjs().format('YYYY-MM-DD')
		]
    },
    mounted(){    
        
        this.queryMoblieTimes()  
    },	
    methods:{
		queryMoblieTimes(){
            openLoading()
            const { page, limit } = this.listQuery
            let start= this.allDate && this.allDate.length>0? new Date(this.dayjs(this.allDate[0]).format('YYYY-MM-DD')) :null
            let end = this.allDate && this.allDate.length>0?new Date(this.dayjs(this.allDate[1]).format('YYYY-MM-DD')):null
            queryMoblieTimes({pageIndex: page, pageSize: limit, filter:{id:this.form.userId,startDate:start,endDate:end}}).then(res=>{
            if(res.isSuccess)
            {
                const { total, rows } = res.data
                        this.tableData=rows
                        this.total=total
            }
            })
            closeLoading()
        },
        showddetail(row){
            let start= this.allDate && this.allDate.length>0? new Date(this.dayjs(this.allDate[0]).format('YYYY-MM-DD')) :null
            let end = this.allDate && this.allDate.length>0?new Date(this.dayjs(this.allDate[1]).format('YYYY-MM-DD')):null
            this.queryhistorySync=true
            this.filter={name:row.key,startDate:start,endDate:end}
            this.getList()
        },
        getList(){
            openLoading()
            const { page, limit } = this.listhistoryQuery
            queryMobileHistory({pageIndex: page, pageSize: limit, filter:this.filter}).then(res=>{
                    if(res.isSuccess)
                    {
                        const { total, rows } = res.data
                        this.tabledetailData=rows
                        this.historytotal=total
                    }
            })
            closeLoading()
        },
        radioChange(val) {
			var start = new Date();
			var end = new Date();
			if(val === '3'){ //当月
				var Nowdate = new Date()
				start = new Date(Nowdate.getFullYear(),Nowdate.getMonth(), 1)
				var MonthNextFirstDay = new Date(Nowdate.getFullYear(),Nowdate.getMonth() + 1, 1)
  				end = new Date(MonthNextFirstDay - 86400000)
				start.setHours(0, 0, 0, 0);
				end.setHours(0, 0, 0, 0);
			}else if(val === '4'){ //当季
				var month = start.getMonth();
				if(month <3 ){
					start.setMonth(0);
					end.setMonth(2);
					end.setDate(31);
				}else if(2 < month && month < 6){
					start.setMonth(3);
					end.setMonth(5);
					end.setDate(30);
				}else if(5 < month && month < 9){
					start.setMonth(6);
					end.setMonth(8);
					end.setDate(30);
				}else if(8 < month && month < 11){
					start.setMonth(9);
					end.setMonth(11);
					end.setDate(31);
				}
				start.setDate(1);
				start.setHours(0, 0, 0, 0);
				end.setHours(0, 0, 0, 0);
			}else if(val === '5'){ //当年
				start.setDate(1);
    			start.setMonth(0);
				end.setMonth(11);
				end.setDate(31);
				start.setHours(0, 0, 0, 0);
				end.setHours(0, 0, 0, 0);
			}
            this.allDate=[start,end]
        },
        setShortcutDate() {
			this.shortcutDate = ''
		},
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
}
</style>