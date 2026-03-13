<template>
    <div class="flow">
        <div class="header">
            <div class="title">{{templateName}}的审批流程设置</div>
            <div class="submit" @click="submit">提交</div>
        </div>
        <div class="content">             
            <div class="aside" >
                <div v-for="(item,index) in flowShow" :key="index">
                    <div v-if="item.userTitle">
                        <div class="start" :class="{'approve':item.userTitle=='审批人','duplicate':item.userTitle=='抄送人'}" @click="openDrawer(index)">
                            <div class="userTitle">
                                {{item.userTitle}}
                                <div v-if=" item.userTitle=='审批人' || item.userTitle=='抄送人' " class="delNode" @click.stop="delNode(index)">X</div>
                            </div>
                            <div class="users">
                                <span v-show="typeof item.users == 'string'">{{item.users}}</span>
                                <span v-show="typeof item.users == 'object'" v-for="(u,i) in item.users" :key="i">{{u.name}}<span v-if=" i != item.users.length -1">、</span></span>
                            </div>    
                        </div>
                        <div class="arrows" @click="operate(index)">
                            <div class="addicon">+</div>
                            <div class="operate" v-show="item.isOperate" >
                                <div class="shenpi" @click="addApproval(index)">审批人</div>
                                <div class="chaosong" @click="addDuplicate(index)">抄送人</div>
                                <div class="tiaojian" @click="addCondition(index)">条件分支</div>
                            </div>
                        </div>
                    </div>
                   
                    <!-- <div> -->
                        <Conditions :conditions.sync="item.conditions" :user.sync="user" :templateId.sync="templateId" v-if="item.conditions.length>0"></Conditions>
                    <!-- </div> -->

                    <div class="conditionArrows" v-if="item.conditions.length>0"></div>               
                    <div class="arrows" @click="operate(index)" v-if="item.conditions.length>0">
                        <div class="addicon">+</div>
                        <div class="operate" v-show="item.isOperate">
                            <div class="shenpi" @click="addApproval(index)">审批人</div>
                            <div class="chaosong" @click="addDuplicate(index)">抄送人</div>
                            <div class="tiaojian" @click="addCondition(index)">条件分支</div>
                        </div>
                    </div>

                </div>   
                <!-- 外层循环 -->
                
            </div> 
            <!-- aside -->

            <div class="end">
                流程结束
            </div>
        </div>
        <el-drawer
            :title="title+'设置'"
            :visible.sync="proposer"
            :wrapperClosable="false"
            :direction="direction">
            <div class="first">可提交申请的成员</div>
            <div class="second">模板可见范围内成员可提交申请，修改后，模板可见范围将被修改</div>
            <el-input v-model="searchName" style="marginLeft: 2%; width: 200px; marginRight: 12px" placeholder="请输入员工"></el-input>
            <el-button type="primary" @click="search">搜索</el-button>
            <el-tree ref="tree" class="tree"
                :data="data"
                @node-click="check"
                node-key="id"
                style="marginTop: 12px"
                :default-expanded-keys="searchId"
            >
            </el-tree>
            <div style="margin-top: 12px; margin-left: 2%; max-height: 78px; overflow: auto">
                <div style="position: relative; display: inline-block; margin-bottom: 8px; max-width: 200px; padding: 0 20px; height: 28px; line-height: 28px; text-align: center; margin-right: 12px; font-size: 14px; border: 1px solid #409eff; border-radius: 10px;" v-for="(item,index) in names" :key="index">{{item.name}}<span style=" font-size: 13px; position: absolute; right: 2px; top: 0px; color: red; display: inline-block; width: 16px; cursor: pointer; height: 16px; border: 1px solid red; line-height: 14px; text-align: center; border-radius: 50%" @click="delName(index)">x</span></div>
            </div>
            <div class="but">
                <div class="cancel" @click="cancel">取消</div>
                <div class="confirm" @click="confirm('请选择申请人（可不选）')">确定</div>
            </div>
        </el-drawer>
        <el-drawer
            :title="title+'设置'"
            :visible.sync="approver"
            :wrapperClosable="false"
            :direction="direction">
            <!-- <el-input class="nodename" v-model="nodeName" placeholder="请输入节点名称"></el-input> -->
            <el-radio-group class="approveway" @change="approvewaychange" v-model="approveway">
                <el-radio :label="'specifiedmember'">指定成员</el-radio>
                <el-radio :label="'departmenthead'">指定部门负责人</el-radio>
                <el-radio :label="'departmentheads'">连续多级部门负责人</el-radio>
            </el-radio-group>
            <el-input v-if="approveway == 'specifiedmember'" v-model="searchName" style="marginLeft: 2%; width: 200px; marginRight: 12px" placeholder="请输入员工"></el-input>
            <el-button v-if="approveway == 'specifiedmember'" type="primary" @click="search">搜索</el-button>
            <el-tree ref="tree"
                v-if="approveway == 'specifiedmember'"
                class="tree"
                :data="data"
                @node-click="check"
                node-key="id"
                :default-expanded-keys="searchId"
            >
            </el-tree>
            <div style="margin-top: 12px; margin-left: 2%; max-height: 78px; overflow: auto">
                <div style="position: relative; display: inline-block; margin-bottom: 8px; max-width: 200px; padding: 0 20px; height: 28px; line-height: 28px; text-align: center; margin-right: 12px; font-size: 14px; border: 1px solid #409eff; border-radius: 10px;" v-for="(item,index) in names" :key="index">{{item.name}}<span style=" font-size: 13px; position: absolute; right: 2px; top: 0px; color: red; display: inline-block; width: 16px; cursor: pointer; height: 16px; border: 1px solid red; line-height: 14px; text-align: center; border-radius: 50%" @click="delName(index)">x</span></div>
            </div>
            <br/>
            <div class="common">
                <div class="approvetype">多人审批方式</div>
                <el-radio v-model="radio" label="或签（一名成员同意即可）" @change="radioChange($event)" class="orapprove" ></el-radio>
                <br/>
                <el-radio v-model="radio" label="会签（须所有成员同意）" @change="radioChange($event)" ></el-radio>
            </div>
            <div class="but">
                <div class="cancel" @click="cancel">取消</div>
                <div class="confirm" @click="confirm('请选择审批人')">确定</div>
            </div>
        </el-drawer>
        <el-drawer
            :title="title+'设置'"
            :visible.sync="pwc"
            :wrapperClosable="false"
            :direction="direction">
            <!-- <el-input class="nodename" v-model="nodeName" placeholder="请输入节点名称"></el-input> -->
            <!-- 抄送人 -->
            <el-radio-group class="approveway" @change="approvewaychange" v-model="approveway">
                <el-radio :label="'specifiedmember'">指定成员</el-radio>
                <el-radio :label="'departmenthead'">指定部门负责人</el-radio>
                <el-radio :label="'departmentheads'">连续多级部门负责人</el-radio>
            </el-radio-group>
            <el-input v-model="searchName" style="marginLeft: 2%; width: 200px; marginRight: 12px" placeholder="请输入员工"></el-input>
            <el-button type="primary" @click="search">搜索</el-button>
            <el-tree class="tree" ref="tree"
                :data="data"
                @node-click="check"
                node-key="id"
                :default-expanded-keys="searchId"
            >
            </el-tree>
            <div style="margin-top: 12px; margin-left: 2%; max-height: 78px; overflow: auto">
                <div style="position: relative; display: inline-block; margin-bottom: 8px; max-width: 200px; padding: 0 20px; height: 28px; line-height: 28px; text-align: center; margin-right: 12px; font-size: 14px; border: 1px solid #409eff; border-radius: 10px;" v-for="(item,index) in names" :key="index">{{item.name}}<span style=" font-size: 13px; position: absolute; right: 2px; top: 0px; color: red; display: inline-block; width: 16px; cursor: pointer; height: 16px; border: 1px solid red; line-height: 14px; text-align: center; border-radius: 50%" @click="delName(index)">x</span></div>
            </div>
            <el-checkbox class="copy" v-model="copyCreateUser">抄送发起人</el-checkbox>
            <div class="but">
                <div class="cancel" @click="cancel">取消</div>
                <div class="confirm" @click="confirm('请选择抄送人')">确定</div>
            </div>
        </el-drawer>
    </div>
</template>
<script>
import Conditions from '@/components/Conditions'
import { addAuditFlowConfigList, GetWeiXinWorkDepartmentDetials} from '@/api/system'
import { mapGetters } from 'vuex'
export default {
    name: "ApprovalFlow",
    components: {Conditions},
    props: ['templateId','flowData','auditFlowConfigId','user','templateName'],
    computed: {
		...mapGetters(['userName','userCode'])
	},
    data(){
        return{
            flowShow: [{userTitle: '申请人', users: '请选择申请人（可不选）', isOperate: false, conditions: []}],
            proposer: false,
            approver: false,
            pwc: false,
            direction: 'rtl',
            title: '',
            nodeName: '',
            data: [],
            index: -1,
            checkedNodes: [],
            radio: '或签（一名成员同意即可）',
            spType: 'Or',
            auditFlowConfigDetail: [],
            count: 0,
            names: [],
            department: [],
            copyCreateUser: false,
            approveway: 'specifiedmember',
            mainDepartmentId: '',
            parentIds: [],
            searchName: '',
            searchId: [],
            lastNodeCount: 0,
            lastNodeConditionCount: 0
        }
    },
    created(){
        this.init()
        GetWeiXinWorkDepartmentDetials().then(res=>{
            if(res.code == 'success'){
                for(var h=0;h<res.data.length;h++){
                    // if(h==0) continue
                    for(var i=0;i<res.data[h].length;i++){
                        if(res.data[h][i].parentId == 0){
                            this.department.push( { name: res.data[h][i].name, value: res.data[h][i].departmentId, users: this.usersRecursion(res.data[h][i].departmentId), sonName: this.departmentRecursion(res.data[h],res.data[h][i].departmentId) } )            
                            break
                        }
                    }
                }
                for(var i=0;i<this.department.length;i++){
                    this.data.push( { label:  this.department[i].name, id: this.department[i].value, children: this.children(this.department[i].sonName, this.department[i].users) } )  
                }  
            }
        })
    },
    methods:{
        delName(index){
            this.$confirm('是否删除?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                this.names.splice(index,1)
                this.$message({
                    type: 'success',
                    message: '删除成功!'
                });
            }).catch(() => {       
            });
        },
        search(){
            this.data = []
            for(var i=0;i<this.department.length;i++){
                this.data.push( { label:  this.department[i].name, id: this.department[i].value, children: this.children(this.department[i].sonName, this.department[i].users) } )  
            }  
        },
        approvewaychange(){
            this.names = []
            this.checkedNodes = []
        },
        children(sonName,users){
            var children = []
            for(var i=0;i<users.length;i++){
                if(this.searchName == '' || users[i].name.indexOf(this.searchName) != -1){
                    children.push( { label: users[i].name, id: users[i].userId, departmentId: users[i].mainDepartmentId, flag: true } )
                }
                if(users[i].name.indexOf(this.searchName) != -1){
                    this.searchId.push(users[i].userId)
                }
            }
            for(var i=0;i<sonName.length;i++){
                children.push( { label: sonName[i].name, id: sonName[i].value, children: this.children(sonName[i].sonName,sonName[i].users) } )
            }
            return children
        },
        departmentRecursion(data,parentId){
            var sonName = []
            for(var i=0;i<data.length;i++){
                if(data[i].parentId == parentId){
                    sonName.push( { name: data[i].name, value: data[i].departmentId, users: this.usersRecursion(data[i].departmentId), sonName: this.departmentRecursion(data,data[i].departmentId) } )
                }
            }
            return sonName
        },
        usersRecursion(departmentId){
            var users = []
            for(var i=0;i<this.user.length;i++){
                if(this.user[i].mainDepartmentId == departmentId){
                    users.push(this.user[i])
                }
            }
            return users
        },
        init(){
            if(this.flowData.length>0){
                this.flowShow = []
            }
            var count = 0
            for(var i=0;i<this.flowData.length;i++){
                if( i > 0 && this.flowData[i].sonId == this.flowData[i-1].sonId ){
                    continue
                }
                if( this.flowData[i].type != 'Condition' ){
                    this.flowShow.push( { spType: this.flowData[i].spType, userTitle: this.flowData[i].type == 'Approve'?'审批人': this.flowData[i].type == 'Copy' ?'抄送人': '申请人', users: JSON.parse(this.flowData[i].userCode).length == 0?"请选择申请人（可不选）":JSON.parse(this.flowData[i].userCode), isOperate:false, conditions: [] } )     
                    count = 0
                }else{
                    if(count == 0){
                        this.flowShow.push( { conditions: [], isOperate: false } )
                        count = 1
                    }
                    this.flowShow[this.flowShow.length-1].conditions.push( { condition: { conditionName: this.flowData[i].conditionName, conditionDetail: JSON.parse(this.flowData[i].formal), isOperate: false} , users: this.recusionU(this.flowData[i].sonId,i+1), conditions: this.recusionC(this.flowData[i].sonId,i+1) } )                        
                    i = i + this.count    
                    this.count = 0
                }
            }
        },
        recusionU(parentId,index){
            var users = []
            for(var i=index;i<this.flowData.length;i++){
                if( i != this.flowData.length-1 && this.flowData[i].sonId == this.flowData[i+1].sonId ){
                    break;
                }
                if(this.flowData[i].parentId == parentId && this.flowData[i].type != 'Condition'){
                    users.push( { spType: this.flowData[i].spType, userTitle: this.flowData[i].type == 'Approve' ?'审批人':'抄送人', users: JSON.parse(this.flowData[i].userCode), isOperate:false, conditions: this.recusionC(this.flowData[i].sonId,i+1) } )                  
                    this.count = ++this.count
                    parentId = this.flowData[i].sonId
                }else{
                    break;
                }
            }
            return users
        },
        recusionC(parentId,index){
            var conditions = []
            for(var i=index;i<this.flowData.length;i++){
                if(this.flowData[i].parentId == parentId && this.flowData[i].type == 'Condition'){
                    this.count = ++this.count
                    conditions.push( { condition: { conditionName: this.flowData[i].conditionName, conditionDetail: JSON.parse(this.flowData[i].formal), isOperate: false} , users: this.recusionU(this.flowData[i].sonId,i+1), conditions: this.recusionC(this.flowData[i].sonId,i+1) } )
                }
            }
            return conditions
        },
        isSelect(){
            for(var i =1;i<this.flowShow.length;i++){
                if(this.flowShow[i].conditions.length<=0){
                    if(typeof this.flowShow[i].users == 'string'){
                        return true
                    }
                }else{
                    if(this.isSelectConditions(this.flowShow[i].conditions)){
                        return true
                    }
                }              
            }  
        },
        isSelectConditions(conditions){
            for(var i=0;i<conditions.length;i++){
                if(typeof conditions[i].condition.conditionDetail == 'string' && conditions[i].condition.conditionName != '默认条件'){
                    return true
                }
                if(conditions[i].conditions.length>0){
                    if(this.isSelectConditions(conditions[i].conditions)){
                        return true
                    }
                }
                if(conditions[i].users.length>0){
                    if(this.isSelectUsers(conditions[i].users)){
                        return true
                    }
                }
            }
        },
        isSelectUsers(users){
            for(var i=0;i<users.length;i++){
                if(typeof users[i].users == 'string'){
                    return true
                }
                if(users[i].conditions.length>0){
                    this.isSelectConditions(users[i].conditions)
                }
            }
        },
        submit(){
             this.$confirm('是否确定提交审批流程?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                // if(typeof this.flowShow[0].users == 'string'){
                //     this.$message({
                //         type: 'error',
                //         message: '请选择申请人!'
                //     });
                // }else 
                if(this.flowShow.length == 1){
                    this.$message({
                        type: 'error',
                        message: '请添加审核人或抄送人、条件分支!'
                    });
                }else if(this.isSelect()){
                    this.$message({
                        type: 'error',
                        message: '请选择审核人或抄送人、条件分支!'
                    });
                }
                else{
                    this.parentIds = []
                    this.count = 0
                    this.auditFlowConfigDetail = []
                    console.log(this.flowShow)
                    this.flowShow.forEach((element,index)=>{   
                        if(index == 0){ 
                            this.auditFlowConfigDetail.push({ seq: 0, auditFlowConfigId: this.auditFlowConfigId, parentId: this.count, sonId: this.count, type: 'CreateUser', spType: 'Null', userCode: JSON.stringify(typeof element.users == 'string'?[]:element.users) })
                        }
                        if(index != 0){ 
                            if(element.conditions.length<=0){
                                if(this.parentIds.length <= 0){
                                    this.auditFlowConfigDetail.push({ seq: 0, auditFlowConfigId: this.auditFlowConfigId, parentId: this.count, sonId: ++this.count, type: element.userTitle=='审批人'?'Approve':'Copy', spType: element.spType?element.spType:'Null', userCode: JSON.stringify(element.users) })                                                                                              
                                    this.lastNodeCount = this.count    
                                }else{
                                    this.count = ++this.count
                                    this.parentIds.forEach(element2 => {
                                        this.auditFlowConfigDetail.push({ seq: 0, auditFlowConfigId: this.auditFlowConfigId, parentId: element2, sonId: this.count, type: element.userTitle=='审批人'?'Approve':'Copy', spType: element.spType?element.spType:'Null', userCode: JSON.stringify(element.users) })
                                    });     
                                    this.parentIds = []
                                }
                            }else{
                                this.isInner = false
                                this.recursionConditions(element.conditions,true)
                            } 
                        }                 
                    })
                    addAuditFlowConfigList(this.auditFlowConfigDetail).then(res=>{
                        if(res.code == 'success'){
                            this.$message({
                                type: 'success',
                                message: '提交成功!'
                            });
                        }
                    })
                }
            }).catch(() => {
                         
            });
        },
        recursionUsers(users){
            // var id = this.count
            users.forEach((element3,index3)=>{       
                this.auditFlowConfigDetail.push({ seq: 0, auditFlowConfigId: this.auditFlowConfigId, parentId: this.count, sonId: ++this.count, type: element3.userTitle=='审批人'?'Approve':'Copy', spType: element3.spType?element3.spType:'Null', userCode: JSON.stringify(element3.users) })              
                if( index3 == users.length -1 && element3.conditions.length == 0 ){
                    this.parentIds.push(this.count)
                }
                //最后一个节点 
                if( index3 == users.length -1 && element3.conditions.length>0){                   
                    this.lastNodeConditionCount = this.count            
                }
                if(element3.conditions.length>0){                  
                    this.recursionConditions(element3.conditions)
                }
            })
        },
        recursionConditions(conditions,flag){
            var id =this.count
            var seq = 1
            conditions.forEach((element2,index2)=>{
                if(flag){
                    this.lastNodeConditionCount = 0
                }         
                // if(this.parentIds.length <= 0){
                    this.auditFlowConfigDetail.push({ seq: seq++, auditFlowConfigId: this.auditFlowConfigId, parentId: id, sonId: ++this.count, type: 3, spType: 'Null', ConditionName: element2.condition.conditionName, formal: JSON.stringify(element2.condition.conditionDetail) })
                // }else{
                //     this.parentIds.forEach(element3 =>{
                //         this.auditFlowConfigDetail.push({ seq: seq++, auditFlowConfigId: this.auditFlowConfigId, parentId: element3, sonId: ++this.count, type: 3, spType: 'Null', ConditionName: element2.condition.conditionName, formal: JSON.stringify(element2.condition.conditionDetail) })
                //     })
                //     this.parentIds = []
                // }         
                if(element2.conditions.length>0){
                    this.recursionConditions(element2.conditions)
                }
                if(element2.users.length>0){
                    this.recursionUsers(element2.users)
                }
                if(element2.conditions.length == 0 && element2.users.length == 0){
                    //如果条件下面没有节点
                    if(this.lastNodeConditionCount == 0){
                        //this.parentIds.push(this.lastNodeCount)
                        this.parentIds.push(this.count)
                    }else{
                        //console.log(this.lastNodeConditionCount)
                        this.parentIds.push(this.lastNodeConditionCount)
                    }                  
                }
           })
        },
        radioChange(event){
            if(event == '会签（须所有成员同意）') this.flowShow[this.index].spType = 'And'
            else this.flowShow[this.index].spType = 'Or'
        },
        delNode(index){
            this.$confirm('确认删除该节点?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                if( index != this.flowShow.length-1 && this.flowShow[index+1].conditions && this.flowShow[index+1].conditions.length>0){
                    this.flowShow.splice(index,2)
                }else{   
                    this.flowShow.splice(index,1)
                }
                this.$message({
                    type: 'success',
                    message: '删除成功!'
                });
            }).catch(() => {
            
            });
        },
        confirm(users){         
            this.radioChange(this.radio)
            // for(var i=0;i<this.checkedNodes.length;i++){
                // if(this.checkedNodes[i].id == 1){
                //     this.names.push( { name: this.checkedNodes[i].label, id: this.checkedNodes[i].id.toString(), iden: !this.checkedNodes[i].flag ? 'department': 'user' } )
                //     break
                // }
                // if( this.checkedNodes[i].flag ){
                //     this.names.push( { name: this.checkedNodes[i].label, id: this.checkedNodes[i].id, iden: !this.checkedNodes[i].flag ? 'department': 'user' } )
                // }
                // else{
                //     this.names.push( { name: this.checkedNodes[i].label, id: this.checkedNodes[i].id.toString(), iden: !this.checkedNodes[i].flag ? 'department': 'user' } )
                //     this.count = i
                //     this.test(this.department[0].sonName, this.checkedNodes[i].id)
                //     i = this.count
                // }
            // }
            console.log("this.approveway");
            console.log(this.approveway);

            if(this.approveway != 'specifiedmember'){
                this.names=[]
                this.names.push( {  name: this.approveway=='departmenthead'?'指定部门负责人':'连续多级部门负责人' , id: this.userCode, iden: this.approveway } )
            }
            if( users == '请选择抄送人' )
            {
                if(this.copyCreateUser){
                    this.names.unshift( { name: "发起人", iden: 'CreateUser' } )
                }               
            }

            if(this.names.length>0){
                this.flowShow[this.index].users = this.names         
            }else{
                if( users == '请选择审批人' )
                    this.flowShow[this.index].users = '请选择审批人'
                if( users == '请选择抄送人' )
                    this.flowShow[this.index].users = '请选择抄送人'
                if( users == '请选择申请人（可不选）' )
                    this.flowShow[this.index].users = '请选择申请人（可不选）'
            }       
            
            this.checkedNodes = []
            this.copyCreateUser = false
            this.approveway = 'specifiedmember'
            this.proposer = false
            this.approver = false
            this.pwc = false
            this.names = []
        },
        check(data,object,object2){   
            if(data.flag){
                if(!this.names.some(function(names){
                    return names.id == data.id
                }))
                {
                    if(this.index == 0){
                        if(this.names.length == 1){
                            this.$message.warning("只能选择一个申请人")
                            return
                        }
                    }
                    this.names.push({name: data.label, id: data.id, iden: 'user'})
                }
            }
            // this.checkedNodes = object.checkedNodes  
        },
        test(sonName,value){
            var flag = true
            for(var i=0;i<sonName.length;i++){
                if(sonName[i].value == value){
                    flag = false
                    this.test2(sonName[i].sonName)
                    this.test3(sonName[i].users)
                    break;
                }
            }
            if(flag){
                for(var i=0;i<sonName.length;i++){
                    this.test(sonName[i].sonName,value)
                }
            }
        },
        test2(sonName){
            for(var i=0;i<sonName.length;i++){
                ++this.count
                this.test2(sonName[i].sonName)
                this.test3(sonName[i].users)
            }
        },
        test3(users){
            for(var i=0;i<users.length;i++){
                ++this.count
            }
        },
        openDrawer(index){
            this.copyCreateUser = false
            this.names = []
            if(this.flowShow[index].spType){
                if(this.flowShow[index].spType == 'And') this.radio = '会签（须所有成员同意）'
                if(this.flowShow[index].spType == 'Or') this.radio = '或签（一名成员同意即可）'
            }else{
                this.radio = '或签（一名成员同意即可）'
            }
            if( typeof this.flowShow[index].users == 'object'){
                if(this.flowShow[index].users[0].name == '发起人'){
                    this.copyCreateUser = true
                    this.names = this.flowShow[index].users.slice(1)
                }else{
                    this.names = this.flowShow[index].users
                }      
            }
            this.searchName = ''
            this.searchId = []
            this.data = []
            for(var i=0;i<this.department.length;i++){
                this.data.push( { label:  this.department[i].name, id: this.department[i].value, children: this.children(this.department[i].sonName, this.department[i].users) } )  
            } 
            if(typeof this.flowShow[index].users == 'object'){
                var keys= [] 
                this.flowShow[index].users.forEach(element => {
                    keys.push(element.id)
                });
                this.$nextTick(() => {
                    this.$refs.tree.setCheckedKeys(keys);
                });
            }else{
                this.$nextTick(() => {
                    this.$refs.tree.setCheckedKeys([]);
                });
            }
            this.title = this.flowShow[index].userTitle
            this.index = index
            if(this.title == '申请人'){
                this.proposer = true
            }else if(this.title == '审批人'){
                this.approver = true
                this.nodeName = '审批人'
            }else if(this.title == '抄送人'){
                this.pwc = true
                this.nodeName = '抄送人'
            }
        },
        cancel(){
            this.proposer = false
            this.approver = false
            this.pwc = false
        },
        operate(index){   
            for(var i=0;i<this.flowShow.length;i++){
                if(i == index) this.flowShow[index].isOperate = !this.flowShow[index].isOperate
                else this.flowShow[i].isOperate = false;   
            }
        },
        addApproval(index){    
            this.flowShow.splice(index + 1, 0, {userTitle: '审批人', users: '请选择审批人', isOperate: false, conditions: []})  
        },
        addDuplicate(index){
            this.flowShow.splice(index + 1, 0, {userTitle: '抄送人', users: '请选择抄送人', isOperate: false, conditions: []})
        },
        addCondition(index){  
            if(this.flowShow[this.flowShow.length-1] && this.flowShow[this.flowShow.length-1].conditions.length==0 && !this.flowShow[this.flowShow.length-1].users){
                this.flowShow.splice(this.flowShow.length-1,1);
            }  
            // if(this.flowShow[index+1] && this.flowShow[index+1].conditions.length==0){
            //     this.flowShow.splice(index+1,1);
            // }        
            if(this.flowShow[index+1]){
                if(this.flowShow[index+1].conditions.length>0){
                    var conditions = this.flowShow[index+1].conditions
                    this.flowShow.splice(index+1,1)
                    this.flowShow.splice(index + 1, 0 ,{conditions: [ { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] }, { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users:[],conditions:conditions } ],isOperate: false})
               }else{
                    var users = [];
                    for(var i = index+1 ; i<this.flowShow.length;){
                        if(this.flowShow[i].conditions.length>0){                    
                            users[users.length-1].conditions = this.flowShow[i].conditions
                            this.flowShow.splice(i,1)
                            break;          
                        }
                        else{   
                            users.push(this.flowShow[i])
                            this.flowShow.splice(i,1)        
                        }
                    }
                    this.flowShow.splice(index + 1, 0 ,{conditions: [ { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] }, { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users: users,conditions:[] } ],isOperate: false})
                }    
            }else{
                this.flowShow.splice(index + 1, 0 ,{conditions: [ { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] }, { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] } ],isOperate: false})
            }
        }
    }
}
</script>
<style lang="scss" scoped>
    .tree{
        max-height: 560px;
        overflow: auto;
        margin-left: 2%;
    }
    .copy{
        margin-left: 2%;
        margin-top: 10px;
    }
    .approveway{
        margin-left: 2%;
        margin-bottom: 16px;
    }
    .nodename{
        width: 96%;
        margin-left: 2%; 
        margin-bottom: 20px;
    }
    .approvetype{
        font-size: 13px;
        color: #333333;
        font-weight: bold;
    }
    .orapprove{
        margin: 16px 0;
    }
    .common{
        margin-left: 2%;
    }
    .flow{
        background-color: #eef1f5;
        .el-drawer{
            .first{
                color: #000;
                font-size: 14px;
                margin-left: 20px;
                margin-bottom: 12px;
            }
            .second{
                color: #787878;
                font-size: 12px;
                margin-left: 20px;
                margin-bottom: 12px;
            }
            .but{
                display: flex;
                position: absolute;
                bottom: 20px;
                right: 20px;
                .confirm{
                    width: 80px;
                    height: 40px;
                    background-color: #287de1;
                    color: #fff;
                    font-size: 15px;
                    text-align: center;
                    line-height: 40px;
                    margin-left: 20px;
                    cursor: pointer;
                    border-radius: 4px;
                }
                .cancel{
                    width: 80px;
                    height: 40px;
                    border: 1px solid #87b8ff;
                    color: #87b8ff;
                    font-size: 15px;
                    text-align: center;
                    line-height: 40px;
                    margin-left: 20px;
                    cursor: pointer;
                    border-radius: 4px;
                }
            }
        }
        .header{
            height: 100px;
            margin: 0;
            background-color: #fff;
            border-bottom: 1px #dbdbdb solid;
            .title{
                text-align: center;
                line-height: 100px;
                font-size: 17px;
            }
            .submit{
                cursor: pointer; 
                width: 80px; 
                height: 38px; 
                background-color: #22a5f1; 
                position: absolute; 
                top: 38px; 
                right: 120px; 
                line-height: 38px; 
                text-align: center; 
                color: white;
                border-radius: 6px;
            }
        }
        .flex{
            display: flex;
            justify-content: center;
            .aside{
                margin: 0 40px;
                .straight{
                    border-left: 2px solid #dbdbdb;
                    height: 50px;
                    margin-left: 50%;
                }
            }
        }
       .content{
            margin-top: 40px;
            // transform: scale(0.8); 
            .start{
                width: 220px;
                background-color: #fff;
                margin: 0 auto;
                border-top: 4px solid #4a94ff;
                .userTitle{
                    height: 30px;
                    background-color: #e9f2ff; 
                    color: #4a94ff;
                    font-size: 14px;
                    line-height: 30px;
                    padding-left: 12px;
                    .delNode{
                        display: inline-block;
                        font-size: 14px;
                        float: right;
                        padding: 0 12px;
                    }
                }
                .users{
                    padding: 20px 0;
                    font-size: 14px;
                    display: table-cell;
                    vertical-align: middle;
                    padding-left: 12px;
                }
                cursor: pointer;
            }
            .approve{
                border-top-color: #fcad22;
                .userTitle{
                    color: #fcad22;
                    background-color: #fff9ee;
                }
            }
            .duplicate{
                border-top-color: #3cb4b2;
                .userTitle{
                    color: #3cb4b2;
                    background-color: #e6f8f8;
                }
            }
            .condition{
                border-top-color: #88939f;
                .userTitle{
                    color: #88939f;
                    background-color: #eef4fb;
                }
            }
            .arrows{
                height: 100px;
                border-left: 2px solid #dbdbdb;
                margin-left: 50%;
                .addicon{
                    cursor: pointer;
                    width: 32px;
                    height: 32px;
                    background-color: #a0b5c8;
                    border-radius: 50%;
                    margin-left: -16px;
                    position: relative;
                    top: 34px;
                    text-align: center;
                    line-height: 32px;
                    color: #fff;
                    font-size: 24px;
                }
                .operate{
                    display: flex;
                    font-size: 15px;
                    position: absolute;
                    .shenpi,.chaosong,.tiaojian{
                        margin-left: 32px;
                        margin-top: 8px;
                        cursor: pointer;
                    }
                    .shenpi{
                        color:#fcad22;
                    }
                    .chaosong{
                        color: #3cb4b2;
                    }
                    .tiaojian{
                        color: #88939f;
                    }
                }
            }
            .end{
                width: 220px;
                height: 56px;
                background-color: #fff;
                margin: 0 auto;
                text-align: center;
                line-height: 56px;
                font-size: 15px;
                margin-bottom: 40px;
            }
            .conditionArrows{
                width: 100%;
                margin: 0 auto;
                border-top: 2px solid #dbdbdb;             
                .addCondition{
                    width: 80px;
                    height: 32px;
                    background-color: #fff;
                    text-align: center;
                    line-height: 32px;
                    font-size: 14px;
                    margin: 0 auto;
                    cursor: pointer;
                    margin-top: -16px;
                    position: absolute;
                    left: 50%;
                    margin-left: -40px;
                }
            }
        }   
    }
</style>
