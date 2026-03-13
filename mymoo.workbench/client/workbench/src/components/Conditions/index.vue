<template>
    <div>
        <div class="conditionArrows"><div class="addCondition" @click="addConditionNode()">添加条件</div></div>
        <div class="flex">
            <div class="aside" v-for="(item,index) in conditions" :key="index">
                <div class="straight"></div>    
                <div class="start condition" :class="{'default':item.condition.conditionName=='默认条件'}" @click="openDrawer(index,-1)">
                    <div class="userTitle" v-if="index+1 == conditions.length">
                        {{item.condition.conditionName}}
                    </div>
                    <div class="userTitle" v-else-if="item.condition.conditionName != '条件'">
                        {{item.condition.conditionName}}
                        <div class="delNode" @click.stop="delNode(index,-1)">X</div>
                    </div>
                    <div class="userTitle" v-else>
                        {{item.condition.conditionName + ( index + 1 ) }}
                        <div class="delNode" @click.stop="delNode(index,-1)">X</div>
                    </div>
                    <div class="users">
                        <div v-show="typeof item.condition.conditionDetail == 'string'">{{item.condition.conditionDetail}}</div>
                        <div v-show="typeof item.condition.conditionDetail == 'object'" v-for="(item3,index3) in item.condition.conditionDetail" :key="index3">{{transform(item3)}}</div>
                    </div>
                </div>  
                <div class="arrows" @click="operate(index,-1)">
                    <div class="addicon">+</div>
                    <div class="operate" v-show="item.condition.isOperate" >
                        <div class="shenpi" @click="addApproval(index,-1)">审批人</div>
                        <div class="chaosong" @click="addDuplicate(index,-1)">抄送人</div>
                        <div class="tiaojian" @click="addCondition(index,-1)">条件分支</div>
                    </div>
                </div>

                <Conditions :conditions="item.conditions" :user.sync="user" :templateId.sync="templateId" v-if="item.conditions.length>0"></Conditions>

                <div v-for="(item2,index2) in item.users" :key="index2">
                    <div class="start" :class="{'approve':item2.userTitle=='审批人','duplicate':item2.userTitle=='抄送人'}" @click="openDrawer(index,index2)">
                        <div class="userTitle">
                            {{item2.userTitle}}
                            <div v-if=" item2.userTitle=='审批人' || item2.userTitle=='抄送人' " class="delNode" @click.stop="delNode(index,index2)">X</div>
                        </div>
                        <div class="users">
                            <span v-show="typeof item2.users == 'string'">{{item2.users}}</span>
                            <span v-show="typeof item2.users == 'object'" v-for="(u,i) in item2.users" :key="i">{{u.name}}<span v-if=" i != item2.users.length -1">、</span></span>
                        </div>
                    </div>
                    <div class="arrows" @click="operate(index,index2)">
                        <div class="addicon">+</div>
                        <div class="operate" v-show="item2.isOperate" >
                            <div class="shenpi" @click="addApproval(index,index2)">审批人</div>
                            <div class="chaosong" @click="addDuplicate(index,index2)">抄送人</div>
                            <div class="tiaojian" @click="addCondition(index,index2)">条件分支</div>
                        </div>
                    </div>
                    <Conditions :conditions="item2.conditions" :user.sync="user" :templateId.sync="templateId" v-if="item2.conditions.length>0"></Conditions>
                </div>
            </div>   
        </div>    
        <!-- <div class="conditionArrows"></div>               
        <div class="arrows" @click="operate()">
            <div class="addicon">+</div>
            <div class="operate" v-if="false">
                <div class="shenpi" @click="addApproval()">审批人</div>
                <div class="chaosong" @click="addDuplicate()">抄送人</div>
                <div class="tiaojian" @click="addCondition()">条件分支</div>
            </div>
        </div> -->
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
            <el-tree class="tree" ref="tree"
                v-if="approveway == 'specifiedmember'"
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
        <el-drawer
            :title="title+'设置'"
            :visible.sync="branch"
            :wrapperClosable="false"
            :direction="direction">
            <el-input class="common conname" v-model="conditionName" placeholder="请输入条件名称"></el-input>

            <el-select v-model="priority" class="common priority" @change="prioritySelect($event)">
                <el-option v-for="(item,index) in conditions" :key="index" v-show="index != conditions.length-1" :label="'优先级'+(index+1)" :value="index+1"></el-option>
            </el-select>

            <div class="common conon">同时满足以下条件</div>
            <div class="common">

                <div v-for="(item,index) in conditionField" :key="index" class="confield">       

                    <el-select v-model="item.name" class="select1" @change="select()">
                        <el-option v-for="(item2,index2) in field" :key="index2" :label="item2.fieldName" :value="item2.fieldNumber"></el-option>
                    </el-select>

                    <div class="texttype" v-if="item.name && filter(item.name,[0,7]) ">
                        为
    
                        <el-select  v-if="fieldObject[item.name] == '业务员'" filterable v-model="item.value" class="select1" :placeholder="'请选择'+fieldObject[item.name]" @change="saleNameSelect($event)">
                            <el-option v-for="(item2,index2) in saleName()" :key="index2" :label="item2.name" :value="item2.userId"></el-option>
                        </el-select>

                        <el-select  v-else-if="fieldObject[item.name].indexOf('结算方式') != -1" v-model="item.value" class="select1" :placeholder="'请选择'+fieldObject[item.name]">
                            <el-option v-for="(item2,index2) in paymentMethod" :key="index2" :label="item2.Name" :value="item2.Name"></el-option>
                        </el-select>
 
                        <el-input v-else v-model="item.value" class="input1" @input="input()" :placeholder="'请输入'+fieldObject[item.name]"></el-input>
                    </div>
                    <div class="numbertype">
                        <el-select v-model="item.symbol" class="select2" @change="select()">
                            <el-option label="等于" value="=="></el-option>
                            <el-option label="大于" value=">"></el-option>
                            <el-option label="大于等于" value=">="></el-option>
                            <el-option label="小于" value="<"></el-option>
                            <el-option label="小于等于" value="<="></el-option>
                        </el-select>
                        <el-input v-model="item.value" class="input2" @input="input()" :placeholder="'请输入'+fieldObject[item.name]"></el-input>
                    </div>
                    <div class="texttype" v-if="item.name && filter(item.name,[6]) ">
                        为
                        <el-select v-model="item.value" class="select3" @change="select()">
                            <el-option v-for="(item2,index2) in filter(item.name)" :key="index2" :label="item2.value[0].text" :value="item2.value[0].text"></el-option>
                        </el-select>
                    </div>
                    <div class="texttype" v-if="item.name && filter(item.name,[2]) ">
                        为
                        <el-date-picker
                            class="input1"
                            v-model="item.value"
                            type="date"
                            value-format="yyyy-MM-dd"
                            placeholder="选择日期">
                        </el-date-picker>
                    </div>
                    
                    <div class="condelete" @click="delField(index)">X</div>
                </div> 

                <div class="conadd" @click="addField" v-show="conditionField.length != field.length">+ 添加条件</div>       
            </div>
            <div class="but">
                <div class="cancel" @click="cancel">取消</div>
                <div class="confirm" @click="confirmCondition('请添加条件')">确定</div>
            </div>
        </el-drawer>                                  
    </div>
</template>

<script>
import { getPaymentMethod, getTemplateField, GetWeiXinWorkDepartmentDetials} from '@/api/system'
import { mapGetters } from 'vuex'
export default{
    name: 'Conditions',
    props: ['conditions','templateId','user'],
    computed: {
		...mapGetters(['userName','userCode'])
	},
    data(){
        return{
           approver: false,
           pwc: false,
           branch: false,
           direction: 'rtl',
           title: '',
           conditionName: '',
           data: [],
           outIndex: -1,
           inIndex: -1,
           checkedNodes: [],
           field: [],//字段
           conditionField: [],//选中的条件字段
           priority: '',
           radio: '或签（一名成员同意即可）',
           sptype: 1,
           type: ["Text","Textarea","Date","Money","Number","File","Selector","Contact"],
           fieldObject: {},   
           department: [],
           names: [],
           count: 0,
           copyCreateUser: false,
           approveway: 'specifiedmember',
           mainDepartmentId: '',
           searchName: '',
           searchId: [],
           paymentMethod: []      
        }
    },
    created(){
        getTemplateField({templateId: this.templateId}).then(res =>{
            if(res.code == 'success'){
                this.field = res.data
                this.field.forEach( (element,index) =>{
                    this.fieldObject[element.fieldNumber] = element.fieldName
                    this.field[index].isOn = false
                    this.field[index].preOn = -1
                    if(this.field[index].selectOptionJson != ''){
                        this.field[index].selectOptionJson = JSON.parse(this.field[index].selectOptionJson)
                    }
                })
            }
        })
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
        this.getPaymentMethod()
    },
    watch:{
        branch(newVal){
            if(!newVal){
                for(var i=0;i<this.conditionField.length;){
                    if(!this.conditionField[i].name || !this.conditionField[i].value){
                        this.conditionField.splice(i,1)
                    }else{
                        i++
                    }
                }        
                if(this.conditionField.length <= 0){
                    this.conditions[this.outIndex].condition.conditionDetail = '请添加条件'
                }
            }
        }
    },
    methods:{
        getPaymentMethod(){
            getPaymentMethod().then(res=>{
                if(res.code == 'success'){
                    this.paymentMethod = JSON.parse(res.data)
                }
            })
        },
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
        saleNameSelect(event){

        },
        saleName(){
            var saleName = []
            this.user.forEach(element =>{
                if(element.position.indexOf("业务员") != -1){
                    saleName.push({name: element.name, userId: element.userId})
                }
            })
            return saleName
        },
        input(){   
            this.$forceUpdate()
        },
        filter(name,types){
            if(types){
                for(var i=0;i<this.field.length;i++){
                    if(this.field[i].fieldNumber == name){   
                        for(var j=0;j<types.length;j++){
                            if(this.field[i].fieldType == types[j]){
                                return true;
                            }
                        }                     
                    }
                }
                return false
            }else{
                for(i=0;i<this.field.length;i++){
                    if(this.field[i].fieldNumber == name){  
                        return this.field[i].selectOptionJson
                    }
                }
            }               
        },
        transform(conditionDetail){
            if(conditionDetail.name && conditionDetail.symbol && conditionDetail.value){
                return this.fieldObject[conditionDetail.name] + conditionDetail.symbol + conditionDetail.value
            }
        },
        prioritySelect(event){
            var index = event -1
            if(index == this.outIndex){
                return 
            }else{
                var conditions = this.conditions[this.outIndex]
                this.conditions.splice(this.outIndex,1)
                this.conditions.splice(index,0,conditions)
                this.outIndex = index
            }    
        },
        radioChange(event){
            if(event == '会签（须所有成员同意）') this.conditions[this.outIndex].users[this.inIndex].spType = 'and'
            else this.conditions[this.outIndex].users[this.inIndex].spType = 'or'
        },
        confirmCondition(conditionDetail){
            for(var i=0;i<this.conditionField.length;i++){
                if(!this.conditionField[i].name){
                    this.$message({
                        type: "error",
                        message: "请选择条件"
                    })
                    return
                }else{
                    if(this.filter(this.conditionField[i].name,[3,4])){
                        var reg = new RegExp("[\\u4E00-\\u9FFF]+","g")
                        if(reg.test(this.conditionField[i].value)){
                            this.$message({
                                type: "error",
                                message: this.fieldObject[this.conditionField[i].name]+"不能包含中文"
                            })
                            return
                        }
                    }
                }
                if(!this.conditionField[i].value){
                    this.$message({
                        type: "error",
                        message: "请输入"+this.fieldObject[this.conditionField[i].name]
                    })
                    return
                }
                if(!this.conditionField[i].symbol){
                    this.conditionField[i].symbol = "=="
                } 
                this.conditionField[i].value = this.conditionField[i].value.replaceAll(' ','')
            }                    

            if(this.conditionField.length > 0){    

                this.conditions[this.outIndex].condition.conditionDetail = this.conditionField

                this.conditions[this.outIndex].condition.conditionName = this.conditionName

                this.conditionName = ''

            }else{
                this.conditions[this.outIndex].condition.conditionDetail = conditionDetail
            }
            this.branch = false
        },
        select(){
            this.$forceUpdate();
        },
        delField(index){
            this.conditionField.splice(index,1);        
        },
        addField(){
            this.conditionField.push({symbol: '=='}) 
        },
        delNode(outIndex,inIndex){
            this.$confirm('确认删除该节点?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                // if(this.conditions[outIndex].users.length == 1){
                //     this.$message({
                //         type: 'error',
                //         message: '条件分支下面至少保留一个节点!'
                //     });
                //     return
                // }
                if(inIndex != -1){
                    this.conditions[outIndex].users.splice(inIndex,1)
                }else{
                    if(this.conditions.length == 2){
                        this.conditions.splice(outIndex,2)
                    }else{
                        this.conditions.splice(outIndex,1)
                    }
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
            if(this.copyCreateUser){
                this.names.unshift( { name: "发起人", iden: 'user' } )
            }
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
            if(this.approveway != 'specifiedmember'){
                this.names=[]
                this.names.push( {  name: this.approveway=='departmenthead'?'指定部门负责人':'连续多级部门负责人' , id: this.userCode, iden: this.approveway } )
            }
            if(this.names.length>0){
                this.conditions[this.outIndex].users[this.inIndex].users = this.names        
            }else{
                if( users == '请选择审批人' )
                    this.conditions[this.outIndex].users[this.inIndex].users = '请选择审批人'
                if( users == '请选择抄送人' )   
                    this.conditions[this.outIndex].users[this.inIndex].users = '请选择抄送人'
            }        

            this.checkedNodes = []
            this.copyCreateUser = false
            this.approveway = 'specifiedmember'      
            this.approver = false
            this.pwc = false
            this.names = []
        },
        check(data,object){   
            if(data.flag){
                if(!this.names.some(function(names){
                    return names.id == data.id
                }))
                {
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
        openDrawer(outIndex,inIndex){   
            this.copyCreateUser = false
            this.names = []
            if(inIndex != -1 && this.conditions[outIndex].users[inIndex].spType){
                if(this.conditions[outIndex].users[inIndex].spType == 'and') this.radio = '会签（须所有成员同意）'
                if(this.conditions[outIndex].users[inIndex].spType == 'or') this.radio = '或签（一名成员同意即可）'
            }else{
                this.radio = '或签（一名成员同意即可）'
            }
            if( inIndex != -1 && typeof this.conditions[outIndex].users[inIndex].users == 'object'){
                if(this.conditions[outIndex].users[inIndex].users[0].name == '发起人'){
                    this.copyCreateUser = true
                    this.names = this.conditions[outIndex].users[inIndex].users.slice(1)
                }else{
                    this.names = this.conditions[outIndex].users[inIndex].users
                } 
            }
            this.searchName = ''
            this.searchId = []
            this.data = []
            for(var i=0;i<this.department.length;i++){
                this.data.push( { label:  this.department[i].name, id: this.department[i].value, children: this.children(this.department[i].sonName, this.department[i].users) } )  
            } 
            this.outIndex = outIndex
            this.inIndex = inIndex
            if(inIndex == -1){

                //默认条件
                if(this.conditions.length-1 == outIndex){
                    return
                }

                this.priority = '优先级'+(outIndex+1)
               
                this.conditionField = []             

                if(typeof this.conditions[outIndex].condition.conditionDetail == 'object'){
                    this.conditionField = this.conditions[outIndex].condition.conditionDetail
                }

                this.title = '条件分支'
                if(this.conditions[outIndex].condition.conditionName == '条件'){
                    this.conditionName = '条件'+(outIndex + 1)
                }else{
                    this.conditionName = this.conditions[outIndex].condition.conditionName
                }
                this.branch = true
            }else{
                if(this.conditions[outIndex].users && typeof this.conditions[outIndex].users[inIndex].users == 'object'){
                    var keys= []
                    this.conditions[outIndex].users[inIndex].users.forEach(element => {
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
                this.title = this.conditions[outIndex].users[inIndex].userTitle
                if(this.title == '审批人'){
                    this.approver = true
                    this.conditionName = '审批人'
                }else if(this.title == '抄送人'){
                    this.pwc = true
                    this.conditionName = '抄送人'
                }
            }   
        },
        cancel(){
            this.approver = false
            this.pwc = false
            this.branch = false
        },
        operate(outIndex,inIndex){
            for(var i=0;i<this.conditions.length;i++){
                if( i == outIndex && inIndex == -1 ){
                    this.conditions[i].condition.isOperate = !this.conditions[i].condition.isOperate
                }else{
                    this.conditions[i].condition.isOperate = false
                }
                for(var j=0;j<this.conditions[i].users.length;j++){
                    if( i == outIndex && j == inIndex){
                        this.conditions[i].users[j].isOperate = !this.conditions[i].users[j].isOperate
                    }else{
                        this.conditions[i].users[j].isOperate = false
                    }
                }
            }
        },
        addApproval(outIndex,inIndex){
            if(inIndex == -1){
                var conditions = this.conditions[outIndex].conditions
                this.conditions[outIndex].conditions = []
            }else{
                var conditions = this.conditions[outIndex].users[inIndex].conditions
                this.conditions[outIndex].users[inIndex].conditions = []
            }
            
            this.conditions[outIndex].users.splice( inIndex + 1 , 0 , {userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: conditions})  
        },
        addDuplicate(outIndex,inIndex){    
            if(inIndex == -1){
                var conditions = this.conditions[outIndex].conditions
                this.conditions[outIndex].conditions = []
            }else{
                var conditions = this.conditions[outIndex].users[inIndex].conditions
                this.conditions[outIndex].users[inIndex].conditions = []
            }
            this.conditions[outIndex].users.splice( inIndex + 1 , 0 ,{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: conditions})
        },
        addCondition(outIndex,inIndex){
            if(inIndex == -1){
                if(this.conditions[outIndex].conditions.length>0){
                    var conditions = this.conditions[outIndex].conditions
                    this.conditions[outIndex].conditions = []
                    this.conditions[outIndex].conditions.push( { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions: [] } )
                    this.conditions[outIndex].conditions.push( { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users:[],conditions: conditions })
                }else{
                    var users = this.conditions[outIndex].users
                    this.conditions[outIndex].users = []
                    if(users.length==0){
                        users = [{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}]
                    }
                    this.conditions[outIndex].conditions.push( { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions: [] } )
                    this.conditions[outIndex].conditions.push( { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users: users,conditions: [] })
                }
            }else{
                if(this.conditions[outIndex].users[inIndex].conditions.length>0){
                    var conditions = this.conditions[outIndex].users[inIndex].conditions
                    this.conditions[outIndex].users[inIndex].conditions = [ { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] }, { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users:[],conditions: conditions }]                  
                }else{
                    var users = [];
                    for(var i = inIndex+1 ; i<this.conditions[outIndex].users.length;){        
                        if( i == this.conditions[outIndex].users.length-1 && this.conditions[outIndex].users[i].conditions.length>0){
                            users.push(this.conditions[outIndex].users[i])
                            users[ users.length-1 ].conditions = this.conditions[outIndex].users[i].conditions
                            this.conditions[outIndex].users.splice(i,1)
                        }      
                        else{
                            users.push(this.conditions[outIndex].users[i])
                            this.conditions[outIndex].users.splice(i,1)
                        }   
                    }
                    if(users.length <= 0){
                        users = [{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}]
                    }
                    this.conditions[outIndex].users[inIndex].conditions = [ { condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] }, { condition:{ conditionName: '默认条件',conditionDetail: '使用默认流程',isOperate: false },users:users,conditions:[] } ]
                }             
            }
        },
        addConditionNode(){
             this.conditions.unshift({ condition:{ conditionName: '条件',conditionDetail: '请添加条件',isOperate: false },users:[{userTitle: '审批人', users: '请选择审批人', isOperate: false,conditions: []},{userTitle: '抄送人', users: '请选择抄送人', isOperate: false,conditions: []}],conditions:[] })   
        }
    }
}
</script>

<style lang="scss" scoped>
    .nodename{
        width: 96%;
        margin-left: 2%; 
        margin-bottom: 20px;
    }
    .copy{
        margin-left: 2%;
        margin-top: 10px;
    }
    .approveway{
        margin-left: 2%;
        margin-bottom: 16px;
    }
    .confield{
        display: flex;
    }
    .numbertype{
        flex: 1;
    }
    .texttype{
        flex: 1;
        margin-left: 10px;
    }
    .select1{
        margin-bottom: 20px; 
        width: 160px;
    }
    .input1{
        width: 160px; 
        margin-left: 10px;
    }
    .select2{
        width: 120px; 
        margin-left: 10px;
    }
    .input2{
        width: 160px; 
        margin-left: 10px;
    }
    .select3{
        width: 180px; 
        margin-left: 10px;
    }
    .tree{
        max-height: 560px;
        overflow: auto;
        margin-left: 2%;
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
    .conname{
        width: 60%; 
        margin-bottom: 20px;
    }
    .priority{
        margin-bottom: 20px;
        width: 25%;
    }
    .conon{
        font-size: 15px;
        font-weight: bold;
        margin-bottom: 20px;
    }
    .condelete{
        cursor: pointer; 
        margin: 0 20px; 
        line-height: 40px; 
        font-size: 16px; 
        color: #287de1;
    }
    .conadd{
        cursor: pointer; 
        font-size: 15px; 
        color: #287de1;
    }
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
    .flex{
        display: flex;
        justify-content: center;
        width: 100%;
        .aside{
            margin: 0 40px;
            .straight{             
                border-left: 2px solid #dbdbdb;
                height: 50px;
                margin-left: 50%;
            }
        }
    }
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
    .default{
        cursor: default;
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
            position: relative;
            left: 50%;
            margin-left: -40px;
        }
    }
</style>
