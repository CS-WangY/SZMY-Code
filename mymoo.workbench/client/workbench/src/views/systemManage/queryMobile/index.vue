<template>
    <div class="matrixpricelistquery">
		<div class="queryPage">
			<div class="queryForm">
				<el-form ref="queryForm" :model="form" label-width="70px" size="mini" inline>
                    <el-form-item label="用户名" prop="userId">
					<UserSearch :user.sync="form.userId" :isMymo="true"  style="width:150px"  ref="userRef" placeholder="请输入用户名" />
                </el-form-item>
					<el-form-item>
						<el-button type="primary" style="margin-left: 1.5em" @click="getMobileById">查询</el-button>
					</el-form-item>
				</el-form>
			</div>
			<div class="tableBox" style="background-color: #f5f7fa;">
				<el-table :data="tableData" :stripe="true" style="width: 100%;" border size="mini">
					<el-table-column prop="name" label="姓名" align="center"></el-table-column>
					<el-table-column prop="mobile" label="手机号" align="center" ></el-table-column>
				</el-table>
			</div>
		</div>
		
	</div>
</template>

<script>
import { getMobileById} from '@/api/system'
import Pagination from '@/components/Pagination'
import UserSearch from '@/components/UserSearch'

export default {
    name: 'queryMobile',
	components: { Pagination, UserSearch },
	data(){
        return{
			status: [],
			astatus: [],
			form: {
                userId:null
			},
			tableData: []
		}
    },
    mounted(){      
	if(navigator.userAgent.match(/(phone|pad|pod|iPhone|iPod|ios|iPad|Android|Mobile|BlackBerry|IEMobile|MQQBrowser|JUC|Fennec|wOSBrowser|BrowserNG|WebOS|Symbian|Windows Phone)/i))
	{
		document.getElementById('tags-view-container').style.display='none'
		document.getElementsByClassName('navbar')[0].style.display='none'
	}
	else
	{
		document.getElementById('tags-view-container').style.display='block'
		document.getElementsByClassName('navbar')[0].style.display='block'
	}
    },	
    methods:{
		getMobileById(){
            getMobileById({id:this.form.userId}).then(res=>{
            if(!res.isSuccess)
            {
               this.$message.info(res.message) 
            }
              this.tableData=res.data
            })
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